using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.TextMate;
using NUnit.Framework;
using TextMateSharp.Grammars;
using TextMateSharp.Model;
using Assert = NUnit.Framework.Legacy.ClassicAssert;
using CollectionAssert = NUnit.Framework.Legacy.CollectionAssert;

namespace AvaloniaEdit.Tests.TextMate
{
    // Pins TokenizeViewPortCore's time-sliced viewport tokenization on the UI thread.
    // TMModel's own tokenizer thread is held at line 0 where a test needs the viewport to be
    // tokenized by the viewport pass alone. Timeouts only turn a hang into a failure.
    [TestFixture]
    internal class TextEditorModelViewPortTokenizationTests
    {
        private static readonly TimeSpan Generous = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan SlowLine = TimeSpan.FromMilliseconds(2);
        // Over the model's 4 ms slice budget: one line ends a slice.
        private static readonly TimeSpan OverBudgetLine = TimeSpan.FromMilliseconds(5);
        private const int LineCount = 500;
        private static readonly Size ViewSize = new Size(400, 600);

        [AvaloniaTest]
        public void ViewPort_Is_Tokenized_In_Slices_On_The_Calling_Thread()
        {
            using var h = new Harness(holdTokenizerThread: true, lineCost: SlowLine);
            h.ScrollTo(100);
            var viewport = h.VisibleLineIndexes();

            int caller = Environment.CurrentManagedThreadId;

            // A thread descheduled past the budget before the first run leaves a correct, empty slice; a new
            // request gets a new budget. One run of 8 lines at 2 ms each overruns it, so a slice stops there.
            int firstSlice = 0;
            for (int attempt = 0; attempt < 5 && firstSlice == 0; attempt++)
            {
                h.EditorModel.TokenizeViewPortCore();
                firstSlice = h.Grammar.CallsFor(h.TextsOf(viewport)).Count();
            }

            Assert.AreEqual(8, firstSlice, "the first slice did not stop after one run once its budget was spent");
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending,
                "the whole viewport was tokenized in one go; a slow grammar would block the UI thread for all of it");

            Assert.IsTrue(PumpUntil(() => !h.EditorModel.IsViewPortTokenizationPending), "the remaining slices never ran");
            Assert.IsEmpty(viewport.Where(i => h.TmModel.GetLineTokens(i) == null).ToList(), "visible lines were left without tokens");
            Assert.IsNull(h.TmModel.GetLineTokens(viewport[^1] + 1), "a run went past the viewport, into TMModel's thread's lines");
            var threads = h.Grammar.CallsFor(h.TextsOf(viewport)).Select(c => c.Thread).Distinct().ToList();
            CollectionAssert.AreEqual(new[] { caller }, threads,
                "viewport lines were tokenized off the UI thread, where a pass can straddle an edit");
        }

        [AvaloniaTest]
        public void Newest_ViewPort_Replaces_The_Remaining_Range()
        {
            using var h = new Harness(holdTokenizerThread: true, lineCost: SlowLine);
            h.ScrollTo(100);
            var rangeA = h.VisibleLineIndexes();
            h.EditorModel.TokenizeViewPortCore();
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "range A finished in its first slice");

            h.ScrollTo(300);
            var rangeC = h.VisibleLineIndexes();
            h.EditorModel.TokenizeViewPortCore();
            Assert.IsTrue(PumpUntil(() => !h.EditorModel.IsViewPortTokenizationPending), "the pass never finished");

            Assert.IsEmpty(rangeC.Where(i => h.TmModel.GetLineTokens(i) == null).ToList(), "the newest viewport was not tokenized");
            Assert.IsTrue(rangeA.Any(i => h.TmModel.GetLineTokens(i) == null),
                "the rest of the replaced range was still tokenized");
        }

        // Without the pending check a continuation at Default or above would pass: it drains in the first
        // round, before the input job exists.
        [AvaloniaTest]
        public void Remaining_Slices_Run_Below_Input()
        {
            using var h = new Harness(holdTokenizerThread: true, lineCost: SlowLine);
            h.ScrollTo(100);
            var texts = h.TextsOf(h.VisibleLineIndexes());

            RunJobs();
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the pass finished above input priority");

            int callsBefore = h.Grammar.CallsFor(texts).Count();
            int callsAtInput = -1;
            Dispatcher.UIThread.Post(() => callsAtInput = h.Grammar.CallsFor(texts).Count(), DispatcherPriority.Input);
            RunJobs();

            Assert.AreEqual(callsBefore, callsAtInput, "a slice ran ahead of the queued input job");
        }

        [AvaloniaTest]
        public void Lines_With_Valid_Tokens_Are_Not_Tokenized_Again()
        {
            using var h = new Harness(holdTokenizerThread: false);
            h.WaitForTokenizerThread();
            h.ScrollTo(100);
            h.Settle();
            var texts = h.TextsOf(h.VisibleLineIndexes());
            int before = h.Grammar.CallsFor(texts).Count();

            h.EditorModel.TokenizeViewPortCore();
            Assert.IsTrue(PumpUntil(() => !h.EditorModel.IsViewPortTokenizationPending), "the pass never finished");

            Assert.AreEqual(before, h.Grammar.CallsFor(texts).Count(),
                "visible lines that already had valid tokens were tokenized again (and redrawn)");
        }

        [AvaloniaTest]
        public void A_Run_Takes_In_Only_The_Lines_That_Need_Tokens()
        {
            using var h = new Harness(holdTokenizerThread: true);
            h.ScrollTo(100);
            h.Settle();
            var viewport = h.VisibleLineIndexes();
            int invalid = viewport[0] + 2;
            var invalidText = h.TextsOf(new[] { invalid });
            var others = h.TextsOf(viewport.Where(i => i != invalid));
            int invalidBefore = h.Grammar.CallsFor(invalidText).Count();
            int othersBefore = h.Grammar.CallsFor(others).Count();
            h.TmModel.InvalidateLine(invalid);

            h.EditorModel.TokenizeViewPortCore();
            Assert.IsTrue(PumpUntil(() => !h.EditorModel.IsViewPortTokenizationPending), "the pass never finished");

            Assert.AreEqual(invalidBefore + 1, h.Grammar.CallsFor(invalidText).Count(), "the invalid line was not tokenized");
            Assert.AreEqual(othersBefore, h.Grammar.CallsFor(others).Count(), "valid lines around the invalid one were tokenized again");
        }

        // SetGrammar drops every line's tokens without marking the lines invalid.
        [AvaloniaTest]
        public void Lines_A_Grammar_Change_Left_Without_Tokens_Are_Tokenized()
        {
            using var h = new Harness(holdTokenizerThread: true);
            h.ScrollTo(100);
            h.Settle();
            var viewport = h.VisibleLineIndexes();
            using var release = new ManualResetEventSlim();
            var replacement = new ScriptedGrammar(default, stateful: false, text =>
            {
                if (text == "HEAD")
                    release.Wait(Generous);
            });

            try
            {
                h.TmModel.SetGrammar(replacement);
                Assert.IsTrue(viewport.All(i => h.TmModel.GetLineTokens(i) == null && !h.TmModel.IsLineInvalid(i)),
                    "the grammar change no longer leaves lines valid without tokens");

                h.EditorModel.TokenizeViewPortCore();
                Assert.IsTrue(PumpUntil(() => !h.EditorModel.IsViewPortTokenizationPending), "the pass never finished");

                Assert.IsEmpty(viewport.Where(i => h.TmModel.GetLineTokens(i) == null).ToList(),
                    "visible lines without tokens were skipped because they were not marked invalid");
            }
            finally
            {
                release.Set();
            }
        }

        // A model listener may scroll and pump the dispatcher while ForceTokenization raises its event; the
        // interrupted slice must not carry its old position into the newer range.
        [AvaloniaTest]
        public void A_Nested_Request_During_A_Run_Owns_The_Range()
        {
            using var h = new Harness(holdTokenizerThread: true, lineCost: SlowLine);
            List<int> newest = null;
            h.TmModel.AddModelTokensChangedListener(new CallbackListener(() =>
            {
                if (newest != null)
                    return;
                h.ScrollTo(300);
                newest = h.VisibleLineIndexes();
                var frame = new DispatcherFrame();
                Dispatcher.UIThread.Post(() => frame.Continue = false, DispatcherPriority.Default);
                Dispatcher.UIThread.PushFrame(frame);
            }));

            h.ScrollTo(100);
            var abandoned = h.VisibleLineIndexes();
            Assert.IsTrue(PumpUntil(() => newest != null && !h.EditorModel.IsViewPortTokenizationPending), "the passes never finished");

            Assert.IsEmpty(newest.Where(i => h.TmModel.GetLineTokens(i) == null).ToList(), "the newest viewport was not tokenized");
            var gap = Enumerable.Range(abandoned[^1] + 1, newest[0] - abandoned[^1] - 1)
                .Where(i => h.TmModel.GetLineTokens(i) != null).ToList();
            Assert.IsEmpty(gap, "lines in neither viewport were tokenized: the old slice resumed against the new range");
        }

        // A grammar that pumps the dispatcher lets a newer request redraw a run's lines, and a layout build them,
        // before the run stores their tokens; the newer pass then skips them, so the run's event must redraw them.
        [AvaloniaTest]
        public void A_Run_Replaced_Before_It_Stores_Its_Tokens_Redraws_Its_Lines()
        {
            using var h = new Harness(holdTokenizerThread: true, lineCost: SlowLine, withTransformer: true);
            h.ScrollTo(100);
            h.Settle();
            var viewport = h.VisibleLineIndexes();
            int first = viewport[0];
            for (int i = first; i < first + 8; i++)
                h.TmModel.InvalidateLine(i);
            List<VisualLine> built = null;
            h.Grammar.BeforeTokenize = text =>
            {
                if (built != null || !Dispatcher.UIThread.CheckAccess() || text != "v" + first)
                    return;
                built = new List<VisualLine>();
                h.ScrollTo(first + 1 - 10);
                var frame = new DispatcherFrame();
                Dispatcher.UIThread.Post(() => frame.Continue = false, DispatcherPriority.Default);
                Dispatcher.UIThread.PushFrame(frame);
                h.Layout();
                built.AddRange(h.View.VisualLines.Where(l => l.FirstDocumentLine.LineNumber - 1 >= first &&
                                                             l.FirstDocumentLine.LineNumber - 1 < first + 8));
            };

            h.EditorModel.TokenizeViewPortCore();
            Assert.IsTrue(PumpUntil(() => !h.EditorModel.IsViewPortTokenizationPending), "the passes never finished");

            Assert.AreEqual(8, built.Count, "the run's lines were not built during the newer request");
            Assert.IsTrue(Enumerable.Range(first, 8).All(i => h.TmModel.GetLineTokens(i) != null), "the run stored no tokens");
            Assert.IsTrue(built.All(l => l.IsDisposed), "lines built before the replaced run stored their tokens kept that build");
        }

        // An edit below the viewport leaves the pass running but redraws the held lines, the run's among them, before
        // the run stores their tokens; a layout then builds them, so the run's event must redraw them.
        [AvaloniaTest]
        [TestCase(1)]
        [TestCase(10)]
        public void An_Edit_Below_The_ViewPort_Before_A_Run_Stores_Its_Tokens_Redraws_Its_Lines(int linesBelow)
        {
            using var h = new Harness(holdTokenizerThread: true, withTransformer: true);
            h.ScrollTo(100);
            h.Settle();
            var viewport = h.VisibleLineIndexes();
            int first = viewport[0];
            for (int i = first; i < first + 8; i++)
                h.TmModel.InvalidateLine(i);
            List<VisualLine> built = null;
            bool pendingAfterEdit = false;
            h.Grammar.BeforeTokenize = text =>
            {
                if (built != null || !Dispatcher.UIThread.CheckAccess() || text != "v" + first)
                    return;
                built = new List<VisualLine>();
                h.Document.Insert(h.Document.Lines[viewport[^1] + linesBelow].Offset, "below");
                pendingAfterEdit = h.EditorModel.IsViewPortTokenizationPending;
                h.Layout();
                built.AddRange(h.View.VisualLines.Where(l => l.FirstDocumentLine.LineNumber - 1 >= first &&
                                                             l.FirstDocumentLine.LineNumber - 1 < first + 8));
            };

            h.EditorModel.TokenizeViewPortCore();
            Assert.IsTrue(PumpUntil(() => !h.EditorModel.IsViewPortTokenizationPending), "the pass never finished");

            Assert.IsTrue(pendingAfterEdit, "the edit below the viewport ended the pass");
            Assert.AreEqual(8, built.Count, "the run's lines were not built after the edit");
            Assert.IsTrue(Enumerable.Range(first, 8).All(i => h.TmModel.GetLineTokens(i) != null), "the run stored no tokens");
            Assert.IsTrue(built.All(l => l.IsDisposed), "lines built after an edit below the viewport kept that build");
        }

        // A continuation queued before a newer request runs that request's pass when a run pumps the dispatcher; the
        // pass ending there redraws the held lines, the run's among them, before the run stores their tokens.
        [AvaloniaTest]
        public void A_Pass_Ended_During_A_Run_Of_The_Same_Request_Redraws_The_Runs_Lines()
        {
            ThrowingModel model = null;
            using var h = new Harness(holdTokenizerThread: true, lineCost: SlowLine, withTransformer: true,
                createModel: (view, document, handler) => model = new ThrowingModel(view, document, handler));
            h.ScrollTo(100);
            h.Settle();
            var viewport = h.VisibleLineIndexes();
            int target = viewport[0] + 8;
            foreach (int i in viewport)
                h.TmModel.InvalidateLine(i);
            h.EditorModel.TokenizeViewPortCore();
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the first slice tokenized the whole viewport");

            List<VisualLine> built = null;
            bool endedInside = false;
            h.Grammar.BeforeTokenize = text =>
            {
                if (built != null || !Dispatcher.UIThread.CheckAccess() || text != "v" + target)
                    return;
                built = new List<VisualLine>();
                model.OnNextLineCount = () => throw new InvalidOperationException("from the nested pass");
                endedInside = PumpUntil(() => !h.EditorModel.IsViewPortTokenizationPending);
                h.Layout();
                built.AddRange(h.View.VisualLines.Where(l => l.FirstDocumentLine.LineNumber - 1 >= target &&
                                                             l.FirstDocumentLine.LineNumber - 1 < target + 8));
            };
            h.Layout();
            h.EditorModel.TokenizeViewPortCore();
            Assert.IsTrue(PumpUntil(() => !h.EditorModel.IsViewPortTokenizationPending), "the pass never finished");

            Assert.IsTrue(endedInside, "the queued continuation did not end the pass inside the run");
            Assert.IsTrue(h.Exceptions.TryDequeue(out var exception), "the exception did not reach the handler");
            Assert.AreEqual("from the nested pass", exception.Message);
            Assert.AreEqual(8, built.Count, "the run's lines were not built inside it");
            Assert.IsTrue(Enumerable.Range(target, 8).All(i => h.TmModel.GetLineTokens(i) != null), "the run stored no tokens");
            Assert.IsTrue(built.All(l => l.IsDisposed), "lines built after a pass ended inside the run kept that build");
        }

        // The same queued continuation can run one slice inside a run and tokenize a line above the run's last; the
        // held lines must still reach the run's last line, which the run's own event leaves to them.
        [AvaloniaTest]
        public void A_Slice_Inside_A_Run_Of_The_Same_Request_Keeps_The_Runs_Lines_Held()
        {
            using var h = new Harness(holdTokenizerThread: true, lineCost: OverBudgetLine, withTransformer: true);
            h.ScrollTo(100);
            h.Settle();
            int first = h.VisibleLineIndexes()[0];
            int last = first + 7;
            for (int i = first; i <= last; i++)
                h.TmModel.InvalidateLine(i);
            h.EditorModel.TokenizeViewPortCore();
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the first slice ended the pass");

            for (int i = first; i <= last; i++)
                h.TmModel.InvalidateLine(i);
            VisualLine built = null;
            bool entered = false;
            bool pendingAfterSlice = false;
            DispatcherFrame slice = null;
            h.Grammar.BeforeTokenize = text =>
            {
                if (!Dispatcher.UIThread.CheckAccess())
                    return;
                // A frame drains every queued Background job, so the slice inside the run stops the frame itself.
                if (slice != null && text == "v" + (first + 1))
                {
                    Dispatcher.UIThread.Post(() => throw new OperationCanceledException("one slice"), DispatcherPriority.Send);
                    return;
                }
                if (entered || text != "v" + last)
                    return;
                entered = true;
                h.TmModel.InvalidateLine(first + 1);
                slice = new DispatcherFrame();
                // Below Background: ends the frame once the queue is idle, if no slice reaches that line.
                Dispatcher.UIThread.Post(() => slice.Continue = false, DispatcherPriority.ContextIdle);
                try
                {
                    Dispatcher.UIThread.PushFrame(slice);
                }
                catch (OperationCanceledException stop) when (stop.Message == "one slice")
                {
                }
                pendingAfterSlice = h.EditorModel.IsViewPortTokenizationPending;
                h.Layout();
                built = h.View.VisualLines.Single(l => l.FirstDocumentLine.LineNumber - 1 == last);
            };
            h.Layout();
            h.EditorModel.TokenizeViewPortCore();
            Assert.IsTrue(PumpUntil(() => !h.EditorModel.IsViewPortTokenizationPending), "the pass never finished");

            Assert.IsNotNull(built, "the run's last line was not built inside it");
            Assert.IsTrue(pendingAfterSlice, "the slice inside the run ended the pass");
            Assert.IsFalse(h.TmModel.IsLineInvalid(first + 1), "the slice inside the run did not tokenize the line above");
            Assert.IsNotNull(h.TmModel.GetLineTokens(last), "the run stored no tokens");
            Assert.IsTrue(built.IsDisposed, "the run's last line, built before its tokens, kept that build");
        }

        // An edit during a run redraws the held lines and a slice inside the run holds new ones below it; the run's
        // return moves the pass back above those, and a run the pass then starts there must be held too.
        [AvaloniaTest]
        public void A_Run_Started_Above_Lines_Held_During_An_Earlier_Run_Redraws_Its_Lines()
        {
            using var h = new Harness(holdTokenizerThread: true, lineCost: OverBudgetLine, withTransformer: true);
            h.ScrollTo(100);
            h.Settle();
            var viewport = h.VisibleLineIndexes();
            int first = viewport[0];
            int last = first + 7;
            int target = first + 9;
            foreach (int i in viewport)
                h.TmModel.InvalidateLine(i);
            h.EditorModel.TokenizeViewPortCore();
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the first slice ended the pass");

            for (int i = first; i <= last; i++)
                h.TmModel.InvalidateLine(i);
            VisualLine built = null;
            bool entered = false;
            int slices = 0;
            bool revokedAfterEdit = false;
            bool pendingAfterSlices = false;
            DispatcherFrame slice = null;
            int sliceLast = -1;
            void RunOneSlice(int lastLine)
            {
                slice = new DispatcherFrame();
                sliceLast = lastLine;
                DispatcherFrame frame = slice;
                Dispatcher.UIThread.Post(() => frame.Continue = false, DispatcherPriority.ContextIdle);
                try
                {
                    Dispatcher.UIThread.PushFrame(frame);
                }
                catch (OperationCanceledException stop) when (stop.Message == "one slice")
                {
                    slices++;
                }
                slice = null;
            }
            h.Grammar.BeforeTokenize = text =>
            {
                if (!Dispatcher.UIThread.CheckAccess())
                    return;
                if (slice != null && text == "v" + sliceLast)
                {
                    Dispatcher.UIThread.Post(() => throw new OperationCanceledException("one slice"), DispatcherPriority.Send);
                    return;
                }
                if (entered || text != "v" + last)
                    return;
                entered = true;
                RunOneSlice(first + 14);
                h.Document.Insert(h.Document.Lines[viewport[^1] + 20].Offset, "below");
                revokedAfterEdit = !h.EditorModel.IsInViewPortRun(h.View, first, last);
                h.Layout();
                RunOneSlice(first + 22);
                pendingAfterSlices = h.EditorModel.IsViewPortTokenizationPending;
                h.TmModel.InvalidateLine(target);
                h.Layout();
                built = h.View.VisualLines.Single(l => l.FirstDocumentLine.LineNumber - 1 == target);
            };
            h.Layout();
            h.EditorModel.TokenizeViewPortCore();
            Assert.IsTrue(PumpUntil(() => !h.EditorModel.IsViewPortTokenizationPending), "the pass never finished");

            Assert.AreEqual(2, slices, "the slices inside the run did not reach their last lines");
            Assert.IsTrue(revokedAfterEdit, "the edit during the run left it holding its lines");
            Assert.IsTrue(pendingAfterSlices, "the slices inside the run ended the pass");
            Assert.IsNotNull(built, "the line after the run's successor was not built inside it");
            Assert.IsFalse(h.TmModel.IsLineInvalid(target), "the pass did not tokenize the line again");
            Assert.IsTrue(built.IsDisposed, "a line a run tokenized above the lines held again kept its build");
        }

        // The redraw a pass makes before it starts invalidates the view's measure, where a view can scroll and pump
        // the dispatcher; the newer pass that starts there keeps its range.
        [AvaloniaTest]
        public void A_ViewPort_Started_During_The_Redraw_Before_A_Pass_Keeps_Its_Range()
        {
            var view = new InvalidationView();
            using var h = new Harness(holdTokenizerThread: true, lineCost: OverBudgetLine, view: view);
            h.ScrollTo(100);
            h.Settle();
            var viewport = h.VisibleLineIndexes();
            for (int i = viewport[0]; i < viewport[0] + 8; i++)
                h.TmModel.InvalidateLine(i);
            h.EditorModel.TokenizeViewPortCore();
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the first slice ended the pass");

            List<int> scrolled = null;
            bool sliced = false;
            view.OnNextMeasureInvalidated = () =>
            {
                h.ScrollTo(300);
                scrolled = h.VisibleLineIndexes();
                var slice = new DispatcherFrame();
                Dispatcher.UIThread.Post(() => slice.Continue = false, DispatcherPriority.ContextIdle);
                h.Grammar.BeforeTokenize = text =>
                {
                    if (Dispatcher.UIThread.CheckAccess() && text == "v" + (scrolled[0] + 7))
                        Dispatcher.UIThread.Post(() => throw new OperationCanceledException("one slice"), DispatcherPriority.Send);
                };
                try
                {
                    Dispatcher.UIThread.PushFrame(slice);
                }
                catch (OperationCanceledException stop) when (stop.Message == "one slice")
                {
                    sliced = true;
                }
                h.Grammar.BeforeTokenize = null;
            };
            h.EditorModel.TokenizeViewPortCore();
            Assert.IsTrue(PumpUntil(() => !h.EditorModel.IsViewPortTokenizationPending), "the pass never finished");

            Assert.IsNotNull(scrolled, "the redraw before the pass raised no measure invalidation");
            Assert.IsTrue(sliced, "the newer pass did not run one slice inside the redraw");
            Assert.Greater(scrolled[0], viewport[^1], "the newer viewport overlaps the older one");
            Assert.IsTrue(scrolled.All(i => !h.TmModel.IsLineInvalid(i) && h.TmModel.GetLineTokens(i) != null),
                "the older pass took the newer viewport's range");
        }

        // The redraw before an edit invalidates the view's measure, where a view can pump the dispatcher; a slice that
        // runs there holds its lines by their numbers before the edit, which moves them.
        [AvaloniaTest]
        [TestCase(true)]
        [TestCase(false)]
        public void Lines_Held_During_The_Redraw_Before_An_Edit_Above_Are_Redrawn(bool removal)
        {
            var view = new InvalidationView();
            using var h = new Harness(holdTokenizerThread: true, lineCost: OverBudgetLine, withTransformer: true, view: view);
            h.ScrollTo(100);
            h.Settle();
            var viewport = h.VisibleLineIndexes();
            int first = viewport[0];
            int target = first + 20;
            h.TmModel.InvalidateLine(first);
            h.EditorModel.TokenizeViewPortCore();
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the first slice ended the pass");

            h.TmModel.InvalidateLine(target);
            DocumentLine targetLine = h.Document.Lines[target];
            VisualLine built = null;
            bool sliced = false;
            bool pendingAfterSlice = false;
            // The view's own redraw of the edit comes first; the model's, of the held line, second.
            view.OnNextMeasureInvalidated = () =>
            {
                h.Layout();
                view.OnNextMeasureInvalidated = () =>
                {
                    h.Layout();
                    built = h.View.VisualLines.Single(l => l.FirstDocumentLine == targetLine);
                    var slice = new DispatcherFrame();
                    Dispatcher.UIThread.Post(() => slice.Continue = false, DispatcherPriority.ContextIdle);
                    h.Grammar.BeforeTokenize = text =>
                    {
                        if (Dispatcher.UIThread.CheckAccess() && text == "v" + target)
                            Dispatcher.UIThread.Post(() => throw new OperationCanceledException("one slice"), DispatcherPriority.Send);
                    };
                    try
                    {
                        Dispatcher.UIThread.PushFrame(slice);
                    }
                    catch (OperationCanceledException stop) when (stop.Message == "one slice")
                    {
                        sliced = true;
                    }
                    h.Grammar.BeforeTokenize = null;
                    pendingAfterSlice = h.EditorModel.IsViewPortTokenizationPending;
                };
            };
            if (removal)
                h.Document.Remove(h.Document.Lines[50].Offset, h.Document.Lines[50].TotalLength + h.Document.Lines[51].TotalLength);
            else
                h.Document.Insert(h.Document.Lines[50].Offset, "x\ny\n");

            // The view keeps its offset, so the edit scrolls in lines without tokens on the side away from the target;
            // only a pass the restart starts tokenizes them, and a layout starts it.
            int untokenized = viewport.Count(i => h.TmModel.GetLineTokens(i) == null);
            Assert.IsTrue(PumpUntil(() =>
            {
                h.Layout();
                return !h.EditorModel.IsViewPortTokenizationPending &&
                       h.VisibleLineIndexes().All(i => h.TmModel.GetLineTokens(i) != null);
            }), "no pass ran after the edit");
            h.Layout();

            Assert.Greater(untokenized, 0, "the edit scrolled in no line without tokens");
            Assert.IsTrue(sliced, "no slice ran inside the redraw before the edit");
            Assert.IsTrue(pendingAfterSlice, "the slice inside the redraw ended the pass");
            Assert.AreEqual(removal ? target - 2 : target + 2, targetLine.LineNumber - 1, "the edit did not move the line");
            Assert.IsTrue(h.View.VisualLines.Any(l => l.FirstDocumentLine == targetLine), "the line left the viewport");
            Assert.IsNotNull(h.TmModel.GetLineTokens(targetLine.LineNumber - 1), "the slice stored no tokens");
            Assert.IsTrue(built.IsDisposed, "a line held before an edit moved it kept its build from before its tokens");
        }

        // An application's Changing handler runs after the model's, which has already decided whether the edit ends
        // the pass; a pass that handler starts names lines by their numbers before the edit.
        [AvaloniaTest]
        public void A_Pass_Started_By_A_Later_Changing_Handler_Starts_Again_After_The_Edit()
        {
            using var h = new Harness(holdTokenizerThread: true, lineCost: OverBudgetLine);
            h.ScrollTo(100);
            h.Settle();
            int first = h.VisibleLineIndexes()[0];
            int target = first + 20;
            var above = Enumerable.Range(first - 4, 4).Where(i => h.TmModel.GetLineTokens(i) == null)
                .Select(i => h.Document.Lines[i]).ToList();
            DocumentLine targetLine = h.Document.Lines[target];
            h.TmModel.InvalidateLine(target);
            Assert.IsFalse(h.EditorModel.IsViewPortTokenizationPending, "a pass was running before the edit");

            bool sliced = false;
            bool pendingAfterSlice = false;
            h.Document.Changing += (_, _) =>
            {
                h.ScrollTo(101);
                var slice = new DispatcherFrame();
                Dispatcher.UIThread.Post(() => slice.Continue = false, DispatcherPriority.ContextIdle);
                h.Grammar.BeforeTokenize = text =>
                {
                    if (Dispatcher.UIThread.CheckAccess() && text == "v" + target)
                        Dispatcher.UIThread.Post(() => throw new OperationCanceledException("one slice"), DispatcherPriority.Send);
                };
                try
                {
                    Dispatcher.UIThread.PushFrame(slice);
                }
                catch (OperationCanceledException stop) when (stop.Message == "one slice")
                {
                    sliced = true;
                }
                h.Grammar.BeforeTokenize = null;
                pendingAfterSlice = h.EditorModel.IsViewPortTokenizationPending;
            };
            h.Document.Insert(h.Document.Lines[50].Offset, "a\nb\nc\nd\n");

            // A restart posts its pass from the layout that follows, so each round runs what that layout posted.
            Assert.IsTrue(PumpUntil(() =>
            {
                h.Layout();
                RunJobs();
                h.Layout();
                return !h.EditorModel.IsViewPortTokenizationPending;
            }), "the pass never finished");

            var visible = h.VisibleLineIndexes();
            Assert.IsTrue(sliced, "no slice ran inside the later handler");
            Assert.IsTrue(pendingAfterSlice, "the slice inside the later handler ended the pass");
            Assert.AreEqual(target + 4, targetLine.LineNumber - 1, "the edit did not move the line");
            Assert.IsTrue(above.Any(l => visible.Contains(l.LineNumber - 1)), "the edit scrolled in no line without tokens");
            var untokenized = visible.Where(i => h.TmModel.GetLineTokens(i) == null).ToList();
            Assert.IsEmpty(untokenized, "lines the edit scrolled in were left without tokens: " + string.Join(", ", untokenized));
        }

        // A run that changes the start state of the line after it leaves that line, and the lines its new state
        // reaches, invalid; the pass must not skip them for the tokens they had.
        [AvaloniaTest]
        public void A_Changed_State_At_A_Run_End_Retokenizes_The_Following_Lines()
        {
            using var h = new Harness(holdTokenizerThread: true, stateful: true);
            h.ScrollTo(100);
            var viewport = h.VisibleLineIndexes();
            h.EditorModel.TokenizeViewPortCore();
            Assert.IsTrue(PumpUntil(() => !h.EditorModel.IsViewPortTokenizationPending), "the first pass never finished");
            Assert.IsTrue(viewport.All(i => h.ScopeOf(i) == "state.green"));
            int below = viewport[^1] + 1;
            h.TmModel.ForceTokenization(below, below);
            Assert.IsFalse(h.TmModel.IsLineInvalid(below));

            // The edit invalidates only the edited line and the one above it; TMModel's thread is held,
            // so whatever the viewport pass leaves is what the view shows.
            int switchLine = viewport[0] + 3;
            var line = h.Document.Lines[switchLine];
            h.Document.Replace(line.Offset, line.Length, "RED");
            h.Layout();

            h.EditorModel.TokenizeViewPortCore();
            Assert.IsTrue(PumpUntil(() => !h.EditorModel.IsViewPortTokenizationPending), "the second pass never finished");

            var wrong = viewport.Where(i => i >= switchLine && h.ScopeOf(i) != "state.red").ToList();
            Assert.IsEmpty(wrong, "lines after the state change kept tokens from the old state: " + string.Join(", ", wrong));
            Assert.IsTrue(h.TmModel.IsLineInvalid(below),
                "the line below the viewport kept its old-state tokens marked valid instead of going to TMModel's thread");
        }

        // A redraw per run rebuilds the viewport's lines between slices; the pass draws its lines once instead.
        [AvaloniaTest]
        public void Lines_A_Pass_Tokenizes_Are_Redrawn_When_It_Ends()
        {
            using var h = new Harness(holdTokenizerThread: true, lineCost: SlowLine, withTransformer: true);
            // An edit before the pass, far below the viewport, must not cost the pass its own redraw.
            h.Document.Replace(h.Document.Lines[450].Offset, 1, "w");
            h.ScrollTo(100);
            var shown = h.View.VisualLines.ToList();

            RunJobs();
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the viewport finished in its first slice");
            var deadline = Stopwatch.StartNew();
            while (h.EditorModel.IsViewPortTokenizationPending)
            {
                Assert.IsEmpty(shown.Where(l => l.IsDisposed).ToList(), "a run redrew lines while the pass went on");
                Assert.Less(deadline.Elapsed, Generous, "the pass never finished");
                RunJobs();
            }

            Assert.IsTrue(shown.All(l => l.IsDisposed), "lines the pass tokenized were not redrawn when it ended");

            // Outside a run, the coloring transformer draws a forced line itself, also one the last run covered.
            h.Layout();
            var rebuilt = h.View.VisualLines.ToList();
            int forced = h.VisibleLineIndexes()[^1];
            h.TmModel.InvalidateLine(forced);
            h.TmModel.ForceTokenization(forced, forced);
            RunJobs();
            Assert.IsTrue(rebuilt[^1].IsDisposed, "a line forced outside a run was left to a pass that never came");

            h.Layout();
            rebuilt = h.View.VisualLines.ToList();
            h.EditorModel.TokenizeViewPortCore();
            Assert.IsFalse(h.EditorModel.IsViewPortTokenizationPending, "a pass over tokenized lines did not finish at once");
            Assert.IsFalse(rebuilt.Any(l => l.IsDisposed), "a pass that tokenized nothing redrew the last pass's lines");
        }

        // After earlier passes have redrawn their lines, a pass still leaves its runs to its own redraw; between its
        // slices, a line forced inside its last run is drawn at once rather than at the pass's end.
        [AvaloniaTest]
        public void A_Later_Pass_Holds_Its_Redraw_And_Draws_A_Line_Forced_Between_Slices()
        {
            using var h = new Harness(holdTokenizerThread: true, lineCost: SlowLine, withTransformer: true);
            h.ScrollTo(100);
            h.Settle();
            var viewport = h.VisibleLineIndexes();
            foreach (int i in viewport)
                h.TmModel.InvalidateLine(i);
            var shown = h.View.VisualLines.ToList();

            h.EditorModel.TokenizeViewPortCore();
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the pass finished in its first slice");
            RunDefaultJobs();
            Assert.IsEmpty(shown.Where(l => l.IsDisposed).ToList(), "a later pass's run redrew its lines at once");

            h.TmModel.InvalidateLine(viewport[0]);
            h.TmModel.ForceTokenization(viewport[0], viewport[0]);
            RunDefaultJobs();
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the pass ended before the forced line was checked");
            Assert.IsTrue(shown[0].IsDisposed, "a line forced between slices was left to the pass's end");

            Assert.IsTrue(PumpUntil(() => !h.EditorModel.IsViewPortTokenizationPending), "the pass never finished");
            Assert.IsTrue(shown.All(l => l.IsDisposed), "lines the pass tokenized were not redrawn when it ended");
        }

        // The pass redraws only its model's view; another view whose transformer shares the model draws the run's
        // tokens as they come.
        [AvaloniaTest]
        public void Another_View_On_The_Same_Model_Redraws_The_Lines_A_Pass_Tokenizes()
        {
            using var h = new Harness(holdTokenizerThread: true, lineCost: SlowLine, withTransformer: true);
            var other = new TextView { Document = h.Document };
            using var otherTransformer = new TextMateColoringTransformer(other, h.Exceptions.Enqueue);
            otherTransformer.SetModel(h.Document, h.TmModel);
            h.TmModel.AddModelTokensChangedListener(otherTransformer);
            other.Measure(ViewSize);
            other.Arrange(new Rect(ViewSize));
            ((IScrollable)other).Offset = new Vector(0, other.GetVisualTopByDocumentLine(100));
            other.Measure(ViewSize);
            other.Arrange(new Rect(ViewSize));
            var otherShown = other.VisualLines.ToList();

            h.ScrollTo(100);
            Assert.IsTrue(otherShown.Select(l => l.FirstDocumentLine.LineNumber - 1).Intersect(h.VisibleLineIndexes()).Any(),
                "the two views show different lines");
            Assert.IsTrue(PumpUntil(() => !h.EditorModel.IsViewPortTokenizationPending), "the pass never finished");

            Assert.IsTrue(otherShown.All(l => l.IsDisposed),
                "another view on the model kept the lines it drew before the pass tokenized them");
        }

        // TMModel's own thread stores the tokens of an edit, say; those are drawn as they come, even mid-pass
        // and for a line the run covers.
        [AvaloniaTest]
        public void Tokens_From_Another_Thread_During_A_Run_Are_Redrawn_At_Once()
        {
            using var h = new Harness(holdTokenizerThread: true, lineCost: SlowLine, withTransformer: true);
            h.ScrollTo(100);
            var first = h.View.VisualLines[0];
            int caller = Environment.CurrentManagedThreadId;
            bool sent = false;
            h.TmModel.AddModelTokensChangedListener(new CallbackListener(() =>
            {
                if (sent || Environment.CurrentManagedThreadId != caller)
                    return;
                sent = true;
                var e = new ModelTokensChangedEvent(new TextMateSharp.Model.Range(first.FirstDocumentLine.LineNumber), h.TmModel);
                Assert.IsTrue(Task.Run(() => h.Transformer.ModelTokensChanged(e)).Wait(Generous), "the other thread never returned");
            }));

            RunJobs();

            Assert.IsTrue(sent, "no run raised an event");
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the viewport finished in its first slice");
            Assert.IsTrue(first.IsDisposed, "tokens from TMModel's thread waited for the pass to end");
        }

        // The pass redraws only the lines its runs cover, so a line a listener forces during a run is not its own.
        [AvaloniaTheory]
        [TestCase(0)]
        [TestCase(-1)]
        public void A_Line_A_Listener_Forces_During_A_Run_Is_Redrawn(int position)
        {
            using var h = new Harness(holdTokenizerThread: true, withTransformer: true);
            h.ScrollTo(100);
            h.Settle();
            var viewport = h.VisibleLineIndexes();
            var targetLine = h.View.VisualLines[position < 0 ? h.View.VisualLines.Count + position : position];
            int target = targetLine.FirstDocumentLine.LineNumber - 1;
            h.TmModel.InvalidateLine(viewport[3]);
            int caller = Environment.CurrentManagedThreadId;
            bool forced = false;
            h.TmModel.AddModelTokensChangedListener(new CallbackListener(() =>
            {
                if (forced || Environment.CurrentManagedThreadId != caller)
                    return;
                forced = true;
                h.TmModel.ForceTokenization(target, target);
            }));

            h.EditorModel.TokenizeViewPortCore();
            Assert.IsTrue(PumpUntil(() => !h.EditorModel.IsViewPortTokenizationPending), "the pass never finished");

            Assert.IsTrue(forced, "no run raised an event");
            Assert.IsTrue(targetLine.IsDisposed,
                "a line forced by a listener during a run kept its build without its tokens");
        }

        // An edit renumbers the lines, so the run's range no longer names the lines the run tokenized.
        [AvaloniaTest]
        public void A_Line_Forced_After_An_Edit_During_A_Run_Is_Redrawn()
        {
            using var h = new Harness(holdTokenizerThread: true, withTransformer: true);
            h.ScrollTo(100);
            h.Settle();
            var viewport = h.VisibleLineIndexes();
            var targetLine = h.View.VisualLines[9];
            Assert.AreEqual(viewport[9] + 1, targetLine.FirstDocumentLine.LineNumber);
            for (int i = 0; i < 8; i++)
                h.TmModel.InvalidateLine(viewport[i]);
            int caller = Environment.CurrentManagedThreadId;
            bool forced = false;
            h.TmModel.AddModelTokensChangedListener(new CallbackListener(() =>
            {
                if (forced || Environment.CurrentManagedThreadId != caller)
                    return;
                forced = true;
                var above = h.Document.Lines[viewport[0] - 4];
                h.Document.Remove(above.Offset, h.Document.Lines[viewport[0] - 2].Offset - above.Offset);
                int target = targetLine.FirstDocumentLine.LineNumber - 1;
                Assert.AreEqual(viewport[7], target, "the edit was meant to move the line into the run's old range");
                h.TmModel.ForceTokenization(target, target);
            }));

            h.EditorModel.TokenizeViewPortCore();
            Assert.IsTrue(PumpUntil(() => !h.EditorModel.IsViewPortTokenizationPending), "the pass never finished");

            Assert.IsTrue(forced, "no run raised an event");
            Assert.IsTrue(targetLine.IsDisposed, "a line forced after an edit during a run kept its build without its tokens");
        }

        [AvaloniaTest]
        public void Dispose_During_A_Run_Redraws_The_Lines_The_Pass_Tokenized() => DisposeRedrawsTheLinesThePassTokenized(duringRun: true);

        [AvaloniaTest]
        public void Dispose_Between_Slices_Redraws_The_Lines_The_Pass_Tokenized() => DisposeRedrawsTheLinesThePassTokenized(duringRun: false);

        // A line transformer runs inside layout, where a redraw of a line being built throws.
        [AvaloniaTest]
        public void Dispose_During_Layout_Redraws_The_Lines_The_Pass_Tokenized_After_It()
        {
            using var h = new Harness(holdTokenizerThread: true, lineCost: SlowLine, withTransformer: true);
            DocumentLine rebuilt = null;
            bool disposed = false;
            // Added first: adding a line transformer redraws the whole view.
            h.View.LineTransformers.Add(new CallbackTransformer(line =>
            {
                if (disposed || rebuilt == null || line.FirstDocumentLine != rebuilt)
                    return;
                disposed = true;
                h.EditorModel.Dispose();
            }));
            h.ScrollTo(100);
            var shown = h.View.VisualLines.ToList();
            h.EditorModel.TokenizeViewPortCore();
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the viewport finished in its first slice");
            rebuilt = shown[12].FirstDocumentLine;
            h.View.Redraw(rebuilt);

            h.Layout();
            Assert.IsTrue(disposed, "the layout did not rebuild the line");
            Assert.IsFalse(shown[0].IsDisposed, "the layout was meant to reuse the line the pass tokenized");
            RunJobs();

            Assert.IsTrue(shown[0].IsDisposed, "a line the pass tokenized kept its build after the model was disposed");
        }

        // A line transformer, or code it calls, can pump the dispatcher while layout builds the view, and a posted
        // job then runs inside that layout.
        [AvaloniaTest]
        public void Dispose_In_A_Dispatcher_Frame_During_Layout_Redraws_After_It() =>
            PumpDuringLayoutRedrawsAfterIt(h =>
            {
                h.EditorModel.Dispose();
                RunJobs();
            });

        [AvaloniaTest]
        public void A_Pass_Ending_In_A_Dispatcher_Frame_During_Layout_Redraws_After_It() =>
            PumpDuringLayoutRedrawsAfterIt(h =>
                Assert.IsTrue(PumpUntil(() => !h.EditorModel.IsViewPortTokenizationPending), "the pass never finished"));

        private static void PumpDuringLayoutRedrawsAfterIt(Action<Harness> duringLayout)
        {
            using var h = new Harness(holdTokenizerThread: true, lineCost: SlowLine, withTransformer: true);
            DocumentLine rebuilt = null;
            bool called = false;
            h.View.LineTransformers.Add(new CallbackTransformer(line =>
            {
                if (called || rebuilt == null || line.FirstDocumentLine != rebuilt)
                    return;
                called = true;
                duringLayout(h);
            }));
            h.ScrollTo(100);
            var shown = h.View.VisualLines.ToList();
            h.EditorModel.TokenizeViewPortCore();
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the viewport finished in its first slice");
            rebuilt = shown[12].FirstDocumentLine;
            h.View.Redraw(rebuilt);

            h.Layout();
            Assert.IsTrue(called, "the layout did not rebuild the line");
            Assert.IsEmpty(h.Exceptions, "a redraw ran while the layout built the view");
            RunJobs();
            h.Layout();

            Assert.IsTrue(shown[0].IsDisposed, "a line the pass tokenized kept its build from before its tokens");
        }

        // A model created inside a build has not seen the view announce it.
        [AvaloniaTest]
        public void A_Model_Created_And_Disposed_During_Layout_Redraws_After_It()
        {
            var document = new TextDocument(string.Join("\n", Enumerable.Range(0, 100).Select(i => "line" + i)));
            var view = new TextView { Document = document };
            void Layout()
            {
                view.Measure(ViewSize);
                view.Arrange(new Rect(ViewSize));
            }

            var errors = new List<Exception>();
            DocumentLine rebuilt = null;
            view.LineTransformers.Add(new CallbackTransformer(line =>
            {
                if (rebuilt == null || line.FirstDocumentLine != rebuilt)
                    return;
                rebuilt = null;
                new TextEditorModel(view, document, ex => { errors.Add(ex); throw ex; }).Dispose();
                RunJobs();
            }));
            Layout();
            var first = view.VisualLines[0];
            rebuilt = view.VisualLines[12].FirstDocumentLine;
            view.Redraw(rebuilt);

            Layout();
            Assert.IsEmpty(errors, "the disposed model redrew the view while the layout built it");
            Layout();

            Assert.IsTrue(first.IsDisposed, "the disposed model's redraw never ran");
        }

        // A model disposed before it has seen a build end defers its redraw, and a view that then shows no document
        // and is not measured again never builds. The exception handler's owner holds the model and its document.
        [AvaloniaTest]
        public void A_Disposed_Model_Is_Not_Kept_By_A_View_That_Shows_No_Document()
        {
            var view = new TextView();
            var (model, document) = CreateAndDisposeModel(view);
            view.Document = null;
            RunJobs();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Assert.IsFalse(model.IsAlive, "the view kept the disposed model");
            Assert.IsFalse(document.IsAlive, "the view kept the disposed model's document");

            view.Measure(ViewSize);
            view.Arrange(new Rect(ViewSize));
            int builds = 0;
            view.VisualLineConstructionStarting += (_, _) => builds++;
            view.Document = new TextDocument("d\ne");
            view.Measure(ViewSize);
            view.Arrange(new Rect(ViewSize));
            Assert.AreEqual(1, builds, "a redraw waited past a measure with no document and rebuilt the next one");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static (WeakReference Model, WeakReference Document) CreateAndDisposeModel(TextView view)
        {
            var document = new TextDocument("a\nb\nc");
            view.Document = document;
            view.Measure(ViewSize);
            view.Arrange(new Rect(ViewSize));
            var owner = new ErrorOwner { Document = document };
            owner.Model = new TextEditorModel(view, document, owner.Handle);
            owner.Model.Dispose();
            return (new WeakReference(owner.Model), new WeakReference(document));
        }

        // A redraw that throws, through a handler that rethrows, still ends the disposed model's subscriptions.
        [AvaloniaTest]
        public void A_Disposed_Model_Is_Not_Kept_When_Its_Redraw_Throws()
        {
            var view = new TextView();
            var (model, document) = CreateAndDisposeModelWhoseRedrawThrows(view);
            view.Document = null;
            RunJobs();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Assert.IsFalse(model.IsAlive, "a redraw that threw left the disposed model subscribed to the view");
            Assert.IsFalse(document.IsAlive, "a redraw that threw left the view holding the disposed model's document");
            GC.KeepAlive(view);
        }

        // A redraw the model hands to the view's next build reports what its invalidation throws, as a direct one does.
        [AvaloniaTest]
        public void A_Deferred_Redraw_Reports_What_Its_Invalidation_Throws()
        {
            var view = new ThrowingInvalidationView { Document = new TextDocument("a\nb\nc") };
            view.Measure(ViewSize);
            view.Arrange(new Rect(ViewSize));
            var errors = new List<Exception>();
            var model = new TextEditorModel(view, view.Document, errors.Add);
            view.ThrowOnInvalidation = true;
            model.Dispose();
            RunJobs();

            Assert.AreEqual(1, errors.Count, "the deferred redraw's invalidation did not reach the exception handler");
        }

        // A redraw a newer viewport starts reports what it throws once, also to a handler that rethrows it.
        [AvaloniaTest]
        public void A_Redraw_Before_A_Newer_Viewport_Reports_Once_To_A_Rethrowing_Handler()
        {
            using var release = new ManualResetEventSlim(false);
            var document = new TextDocument("HEAD\n" + string.Join("\n", Enumerable.Range(1, 499).Select(i => "v" + i)));
            var view = new ThrowingInvalidationView { Document = document };
            view.Measure(ViewSize);
            view.Arrange(new Rect(ViewSize));
            ((IScrollable)view).Offset = new Vector(0, view.GetVisualTopByDocumentLine(100));
            view.Measure(ViewSize);
            view.Arrange(new Rect(ViewSize));
            var errors = new List<Exception>();
            // Created after the layout, so its redraw waits for the next build and invalidates the view.
            var model = new TextEditorModel(view, document, e => { errors.Add(e); throw e; });
            var tm = new TMModel(model);
            try
            {
                tm.SetGrammar(new ScriptedGrammar(SlowLine, false, text => { if (text == "HEAD") release.Wait(); }));
                model.TokenizeViewPortCore();
                Assert.IsTrue(model.IsViewPortTokenizationPending, "the first slice tokenized the whole viewport");
                view.ThrowOnInvalidation = true;

                Assert.Catch<InvalidOperationException>(model.TokenizeViewPortCore, "the handler's rethrow was swallowed");
                Assert.AreEqual(1, errors.Count, "the redraw's exception reached the handler more than once");
            }
            finally
            {
                release.Set();
                tm.Dispose();
                RunJobs();
            }
        }

        private sealed class ThrowingInvalidationView : TextView
        {
            public bool ThrowOnInvalidation;

            protected override void OnMeasureInvalidated()
            {
                base.OnMeasureInvalidated();
                if (ThrowOnInvalidation)
                {
                    ThrowOnInvalidation = false;
                    throw new InvalidOperationException("invalidating the view threw");
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static (WeakReference Model, WeakReference Document) CreateAndDisposeModelWhoseRedrawThrows(TextView view)
        {
            var document = new TextDocument("a\nb\nc");
            view.Document = document;
            var owner = new ErrorOwner { Document = document };
            owner.Model = new TextEditorModel(view, document, owner.Handle);
            view.Measure(ViewSize);
            view.Arrange(new Rect(ViewSize));
            var drawn = view.GetVisualDescendants().OfType<VisualLineDrawingVisual>().First();
            bool armed = true;
            drawn.PropertyChanged += (_, e) =>
            {
                if (armed && e.Property == StyledElement.ParentProperty && drawn.Parent == null)
                {
                    armed = false;
                    throw new InvalidOperationException("detaching a line's drawing threw");
                }
            };
            owner.Model.Dispose();
            Assert.Catch<InvalidOperationException>(RunJobs, "the redraw after dispose did not throw");
            return (new WeakReference(owner.Model), new WeakReference(document));
        }

        private sealed class ErrorOwner
        {
            public TextDocument Document;
            public TextEditorModel Model;

            public void Handle(Exception exception) => throw exception;
        }

        // A grammar may pump the dispatcher; tokens a run stores after the model is disposed are no longer the pass's.
        [AvaloniaTest]
        public void Tokens_A_Run_Stores_After_Dispose_Are_Redrawn()
        {
            using var h = new Harness(holdTokenizerThread: true, withTransformer: true);
            h.ScrollTo(100);
            h.Settle();
            int first = h.VisibleLineIndexes()[0];
            string text = h.Document.GetText(h.Document.Lines[first]);
            h.TmModel.InvalidateLine(first);
            int caller = Environment.CurrentManagedThreadId;
            VisualLine builtBeforeTokens = null;
            h.Grammar.BeforeTokenize = t =>
            {
                if (builtBeforeTokens != null || t != text || Environment.CurrentManagedThreadId != caller)
                    return;
                h.EditorModel.Dispose();
                RunJobs();
                h.Layout();
                builtBeforeTokens = h.View.VisualLines[0];
            };

            h.EditorModel.TokenizeViewPortCore();
            h.Grammar.BeforeTokenize = null;
            Assert.IsNotNull(builtBeforeTokens, "the run did not tokenize the line");
            RunJobs();

            Assert.IsTrue(builtBeforeTokens.IsDisposed, "a line built before a disposed run stored its tokens kept that build");
        }

        // The lines a pass has tokenized but not yet redrawn would keep their builds from before their tokens.
        private static void DisposeRedrawsTheLinesThePassTokenized(bool duringRun)
        {
            using var h = new Harness(holdTokenizerThread: true, lineCost: SlowLine, withTransformer: true);
            h.ScrollTo(100);
            var shown = h.View.VisualLines.ToList();
            int caller = Environment.CurrentManagedThreadId;
            if (duringRun)
            {
                h.TmModel.AddModelTokensChangedListener(new CallbackListener(() =>
                {
                    if (Environment.CurrentManagedThreadId == caller)
                        h.EditorModel.Dispose();
                }));
            }

            h.EditorModel.TokenizeViewPortCore();
            if (!duringRun)
            {
                Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the viewport finished in its first slice");
                Assert.IsFalse(shown[0].IsDisposed, "the first run's line was redrawn before the dispose");
                // From another thread, as TMModel's own disposal may run; the view is redrawn on the UI thread.
                Assert.IsTrue(Task.Run(() => h.EditorModel.Dispose()).Wait(Generous), "the dispose never returned");
            }
            RunJobs();

            Assert.IsTrue(shown[0].IsDisposed, "a line the pass tokenized kept its build after the model was disposed");
        }

        // A scrollbar drag replaces pass after pass, so a line still on screen cannot wait for a pass that ends.
        [AvaloniaTest]
        public void A_Replaced_Pass_Redraws_The_Lines_It_Tokenized()
        {
            using var h = new Harness(holdTokenizerThread: true, lineCost: SlowLine, withTransformer: true);
            h.ScrollTo(100);
            var builtBeforeTokens = h.View.VisualLines.ToList();
            h.EditorModel.TokenizeViewPortCore();
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the viewport finished in its first slice");
            var tokenized = h.VisibleLineIndexes().Where(i => h.TmModel.GetLineTokens(i) != null).ToList();
            Assert.IsNotEmpty(tokenized, "the first slice tokenized nothing");

            h.ScrollTo(tokenized[^1]);
            var kept = h.View.VisualLines
                .Where(l => builtBeforeTokens.Contains(l) && tokenized.Contains(l.FirstDocumentLine.LineNumber - 1)).ToList();
            Assert.IsNotEmpty(kept, "no line the first slice tokenized stayed on screen as it was built before its tokens");

            h.EditorModel.TokenizeViewPortCore();

            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the newer viewport finished in its first slice");
            Assert.IsTrue(kept.All(l => l.IsDisposed), "lines the replaced pass tokenized waited for the newer pass to end");
        }

        // TextView redraws only the edited text, and an edit moves the lines the pass holds to other indexes.
        [AvaloniaTest]
        public void An_Edit_During_A_Pass_Redraws_The_Lines_It_Tokenized()
        {
            using var h = new Harness(holdTokenizerThread: true, lineCost: OverBudgetLine, withTransformer: true);
            h.ScrollTo(100);
            var shown = h.View.VisualLines.ToList();
            RunJobs();
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the viewport finished in its first slice");
            var tokenized = shown.Where(l => h.TmModel.GetLineTokens(l.FirstDocumentLine.LineNumber - 1) != null).ToList();
            Assert.Greater(tokenized.Count, 2, "the first slice tokenized too few lines");
            Assert.IsFalse(tokenized.Any(l => l.IsDisposed));
            // A redraw also disposes the line after its range, so the line kept is further away.
            VisualLine notReached = shown.Last();
            Assert.Greater(notReached.FirstDocumentLine.LineNumber, tokenized.Max(l => l.FirstDocumentLine.LineNumber) + 1,
                "the first slice reached the viewport's end");

            h.Document.Insert(tokenized[0].FirstDocumentLine.Offset, "x\n");

            Assert.IsTrue(tokenized.All(l => l.IsDisposed), "lines the pass tokenized kept a build without their tokens");
            Assert.IsFalse(notReached.IsDisposed, "the edit redrew lines the pass had not reached");
        }

        // A deletion moves lines the pass has not reached to indexes it has passed; the view keeps its offset, so a
        // deletion above the viewport does the same.
        [AvaloniaTest]
        [TestCase(false)]
        [TestCase(true)]
        public void A_Deletion_During_A_Pass_Leaves_No_Visible_Line_Without_Tokens(bool aboveViewPort)
        {
            using var h = new Harness(holdTokenizerThread: true, lineCost: SlowLine);
            h.ScrollTo(100);
            var viewport = h.VisibleLineIndexes();
            RunJobs();
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the viewport finished in its first slice");
            int next = viewport.First(i => h.TmModel.GetLineTokens(i) == null);
            Assert.GreaterOrEqual(next, 105, "the first slice tokenized too few lines");

            int from = aboveViewPort ? 50 : next - 4;
            h.Document.Remove(h.Document.Lines[from].Offset, h.Document.Lines[from + 4].Offset - h.Document.Lines[from].Offset);
            h.Layout();
            Assert.IsTrue(PumpUntil(() => !h.EditorModel.IsViewPortTokenizationPending), "the pass never finished");

            h.Layout();
            var untokenized = h.VisibleLineIndexes().Where(i => h.TmModel.GetLineTokens(i) == null).ToList();
            Assert.IsEmpty(untokenized, "the pass skipped lines the deletion moved: " + string.Join(", ", untokenized));

            // Only the build after the edit starts a pass.
            int line = h.VisibleLineIndexes()[0];
            h.TmModel.InvalidateLine(line);
            h.View.Redraw();
            h.Layout();
            RunJobs();
            Assert.IsTrue(h.TmModel.IsLineInvalid(line), "a later build started a pass");
        }

        // A listener can edit during a run; the run must not go on to the next of the old indexes.
        [AvaloniaTest]
        public void A_Deletion_During_A_Run_Ends_The_Pass_Until_The_Next_Build()
        {
            using var h = new Harness(holdTokenizerThread: true, lineCost: SlowLine);
            h.ScrollTo(100);
            var viewport = h.VisibleLineIndexes();
            int caller = Environment.CurrentManagedThreadId;
            bool edited = false;
            h.TmModel.AddModelTokensChangedListener(new CallbackListener(() =>
            {
                if (edited || Environment.CurrentManagedThreadId != caller)
                    return;
                edited = true;
                var from = h.Document.Lines[viewport[20]];
                h.Document.Remove(from.Offset, h.Document.Lines[viewport[24]].Offset - from.Offset);
            }));

            h.EditorModel.TokenizeViewPortCore();

            Assert.IsTrue(edited, "no run raised an event");
            Assert.IsFalse(h.EditorModel.IsViewPortTokenizationPending, "the run went on over the old indexes");
            h.Layout();
            Assert.IsTrue(PumpUntil(() => !h.EditorModel.IsViewPortTokenizationPending), "the pass never finished");
            h.Layout();
            Assert.IsEmpty(h.VisibleLineIndexes().Where(i => h.TmModel.GetLineTokens(i) == null).ToList(),
                "the pass did not start again from the next build's viewport");
        }

        // A build during the change still shows the lines before it, so the pass the edit ended waits for one after
        // it, also when the callback a build posts runs before the layout that follows the edit.
        [AvaloniaTest]
        public void A_Build_During_An_Edit_Leaves_The_Restart_To_The_Build_After_It()
        {
            using var h = new Harness(holdTokenizerThread: true, lineCost: SlowLine);
            h.ScrollTo(100);
            RunJobs();
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the viewport finished in its first slice");
            int first = h.VisibleLineIndexes()[0];
            // The view keeps its offset, so the 60 lines inserted at the top bring these into view.
            var shiftedIn = Enumerable.Range(first - 60, 30).Where(i => h.TmModel.GetLineTokens(i) == null)
                .Select(i => h.Document.Lines[i]).ToList();

            bool built = false;
            h.Document.Changing += (_, _) =>
            {
                h.View.Redraw();
                h.Layout();
                built = h.View.VisualLinesValid;
            };
            h.Document.Insert(0, string.Concat(Enumerable.Repeat("x\n", 60)));
            RunDefaultJobs();

            Assert.IsTrue(PumpUntil(() =>
            {
                h.Layout();
                RunJobs();
                h.Layout();
                return !h.EditorModel.IsViewPortTokenizationPending;
            }), "the pass never finished");

            var visible = h.VisibleLineIndexes();
            Assert.IsTrue(built, "the later Changing handler built no lines");
            Assert.IsTrue(shiftedIn.Count(l => visible.Contains(l.LineNumber - 1)) >= 20, "the edit scrolled in too few lines without tokens");
            var untokenized = visible.Where(i => h.TmModel.GetLineTokens(i) == null).ToList();
            Assert.IsEmpty(untokenized, "lines the edit scrolled in were left without tokens: " + string.Join(", ", untokenized));
        }

        // A layout later in the update follows the change but uses up the build the change asked for; the update's end
        // asks for another.
        [AvaloniaTest]
        public void A_Build_Later_In_The_Update_Leaves_The_Restart_To_A_Build_After_It()
        {
            using var h = new Harness(holdTokenizerThread: true, lineCost: SlowLine);
            h.ScrollTo(100);
            RunJobs();
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the viewport finished in its first slice");

            bool built;
            using (h.Document.RunUpdate())
            {
                h.Document.Insert(0, string.Concat(Enumerable.Repeat("x\n", 60)));
                h.Layout();
                built = h.View.VisualLinesValid;
            }

            Assert.IsTrue(PumpUntil(() =>
            {
                h.Layout();
                RunJobs();
                h.Layout();
                return !h.EditorModel.IsViewPortTokenizationPending;
            }), "the pass never finished");
            Assert.IsTrue(built, "the layout in the update built no lines");
            var untokenized = h.VisibleLineIndexes().Where(i => h.TmModel.GetLineTokens(i) == null).ToList();
            Assert.IsEmpty(untokenized, "lines the edit scrolled in were left without tokens: " + string.Join(", ", untokenized));
        }

        // The update's end hands the edit's lines to TMModel before it asks the view for a build, which can throw.
        [AvaloniaTest]
        public void A_Build_Request_That_Throws_At_The_Update_End_Keeps_The_Edits_Lines_Invalid()
        {
            var view = new ThrowingInvalidationView();
            using var h = new Harness(holdTokenizerThread: true, lineCost: SlowLine, view: view);
            h.ScrollTo(100);
            RunJobs();
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the viewport finished in its first slice");
            int target = h.VisibleLineIndexes()[0];
            Assert.AreEqual("text.v", h.ScopeOf(target), "the first slice did not tokenize the target line");

            h.Document.BeginUpdate();
            h.Document.Insert(h.Document.Lines[target].Offset, "x");
            h.Layout();
            Assert.IsTrue(h.View.VisualLinesValid, "the layout in the update built no lines");
            view.ThrowOnInvalidation = true;
            Assert.Throws<InvalidOperationException>(h.Document.EndUpdate, "the build request did not throw");

            Assert.IsTrue(h.TmModel.IsLineInvalid(target), "the edited line kept tokens of its old text");
        }

        // A Changing handler that lays out and throws cancels the edit and the model's Changed; the pass the model
        // ended there starts again at a build the update's end asks for.
        [AvaloniaTest]
        public void A_Pass_Ended_By_A_Cancelled_Edit_Starts_Again_At_The_Next_Build()
        {
            using var h = new Harness(holdTokenizerThread: true, lineCost: SlowLine);
            h.ScrollTo(100);
            RunJobs();
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the viewport finished in its first slice");

            bool built = false;
            EventHandler<DocumentChangeEventArgs> cancel = (_, _) =>
            {
                h.Layout();
                built = h.View.VisualLinesValid;
                throw new InvalidOperationException("cancelled");
            };
            h.Document.Changing += cancel;
            Assert.Throws<InvalidOperationException>(() => h.Document.Insert(h.Document.Lines[110].Offset, "x\n"));
            h.Document.Changing -= cancel;
            Assert.IsTrue(built, "the cancelling handler built no lines");
            Assert.IsFalse(h.EditorModel.IsViewPortTokenizationPending, "the model's Changing did not end the pass");

            Assert.IsTrue(PumpUntil(() =>
            {
                h.Layout();
                RunJobs();
                h.Layout();
                return !h.EditorModel.IsViewPortTokenizationPending;
            }), "the pass never finished");
            Assert.IsEmpty(h.VisibleLineIndexes().Where(i => h.TmModel.GetLineTokens(i) == null).ToList(),
                "the pass the cancelled edit ended did not start again");
        }

        // Lines up to the viewport keep their indexes, and no layout may follow an edit the view does not show.
        [AvaloniaTest]
        public void An_Edit_Below_The_ViewPort_Leaves_The_Pass_Running()
        {
            using var h = new Harness(holdTokenizerThread: true, lineCost: SlowLine);
            h.ScrollTo(100);
            var viewport = h.VisibleLineIndexes();
            RunJobs();
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the viewport finished in its first slice");

            h.Document.Remove(h.Document.Lines[450].Offset, h.Document.Lines[454].Offset - h.Document.Lines[450].Offset);
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "an edit below the viewport ended the pass");
            Assert.IsTrue(PumpUntil(() => !h.EditorModel.IsViewPortTokenizationPending), "the pass never finished");

            Assert.IsEmpty(viewport.Where(i => h.TmModel.GetLineTokens(i) == null).ToList(),
                "visible lines were left without tokens");
        }

        [AvaloniaTest]
        public void Dispose_Stops_The_Remaining_Slices()
        {
            using var h = new Harness(holdTokenizerThread: true, lineCost: SlowLine);
            h.ScrollTo(100);
            var texts = h.TextsOf(h.VisibleLineIndexes());
            h.EditorModel.TokenizeViewPortCore();
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the viewport finished in its first slice");
            int before = h.Grammar.CallsFor(texts).Count();

            h.EditorModel.Dispose();
            RunJobs();

            Assert.AreEqual(before, h.Grammar.CallsFor(texts).Count(), "a slice ran after Dispose");
        }

        [AvaloniaTest]
        public void Dispose_During_A_Run_Stops_The_Rest_Of_The_Slice()
        {
            // JIT time would spend the budget, and this slice has to have time left for a second run.
            using (var warm = new Harness(holdTokenizerThread: true))
            {
                warm.ScrollTo(100);
                warm.Settle();
            }

            using var h = new Harness(holdTokenizerThread: true);
            h.ScrollTo(100);
            var texts = h.TextsOf(h.VisibleLineIndexes());
            int atDispose = -1;
            int caller = Environment.CurrentManagedThreadId;
            h.TmModel.AddModelTokensChangedListener(new CallbackListener(() =>
            {
                if (atDispose >= 0 || Environment.CurrentManagedThreadId != caller)
                    return;
                atDispose = h.Grammar.CallsFor(texts).Count();
                h.EditorModel.Dispose();
            }));

            h.EditorModel.TokenizeViewPortCore();

            Assert.Greater(atDispose, 0, "the listener never ran");
            Assert.AreEqual(atDispose, h.Grammar.CallsFor(texts).Count(), "a run started after Dispose returned");
        }

        // The redraw before a pass invalidates the view's measure, where a view can dispose the model.
        [AvaloniaTest]
        public void Dispose_During_The_Redraw_Before_A_Pass_Starts_No_Run()
        {
            var view = new InvalidationView();
            using var h = new Harness(holdTokenizerThread: true, lineCost: OverBudgetLine, view: view);
            h.ScrollTo(100);
            h.Settle();
            var viewport = h.VisibleLineIndexes();
            for (int i = viewport[0]; i < viewport[0] + 16; i++)
                h.TmModel.InvalidateLine(i);
            h.EditorModel.TokenizeViewPortCore();
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the first slice ended the pass");

            var texts = h.TextsOf(viewport);
            int atDispose = -1;
            view.OnNextMeasureInvalidated = () =>
            {
                atDispose = h.Grammar.CallsFor(texts).Count();
                h.EditorModel.Dispose();
            };
            h.EditorModel.TokenizeViewPortCore();

            Assert.GreaterOrEqual(atDispose, 0, "the redraw before the pass raised no measure invalidation");
            Assert.AreEqual(atDispose, h.Grammar.CallsFor(texts).Count(), "a run started after Dispose");
        }

        // TMModel's thread is tokenizing the line from its old start state when a run changes that state. Its
        // thread is then held on the next line, so the result under test is what the pass leaves.
        [AvaloniaTest]
        public void A_Line_Tokenized_From_Its_Old_State_During_A_Run_Ends_With_Tokens_Of_The_New_State()
        {
            using var h = new Harness(holdTokenizerThread: true, lineCost: SlowLine, stateful: true);
            h.ScrollTo(100);
            var viewport = h.VisibleLineIndexes();
            int boundary = viewport[0] + 8;
            string boundaryText = "v" + boundary;
            string afterText = "v" + (boundary + 1);
            using var boundaryEntered = new ManualResetEventSlim();
            using var boundaryRelease = new ManualResetEventSlim();
            using var afterEntered = new ManualResetEventSlim();
            using var afterRelease = new ManualResetEventSlim();
            int caller = Environment.CurrentManagedThreadId;
            h.Grammar.BeforeTokenize = text =>
            {
                if (Environment.CurrentManagedThreadId == caller)
                    return;
                if (text == boundaryText)
                {
                    boundaryEntered.Set();
                    boundaryRelease.Wait(Generous);
                }
                else if (text == afterText)
                {
                    afterEntered.Set();
                    afterRelease.Wait(Generous);
                }
            };

            try
            {
                h.ReleaseTokenizerThread();
                Assert.IsTrue(boundaryEntered.Wait(Generous), "TMModel's thread never reached the line");

                int firstOffset = h.Document.Lines[viewport[0]].Offset;
                int endOffset = h.Document.Lines[boundary - 1].EndOffset;
                string edit = "RED\n" + string.Join("\n", Enumerable.Range(viewport[0] + 1, 7).Select(i => "v" + i));
                h.Document.Replace(firstOffset, endOffset - firstOffset, edit);
                h.Layout();

                // The first run ends just above the line: this writes its new start state.
                h.EditorModel.TokenizeViewPortCore();
                Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the viewport finished in its first slice");
                Assert.IsNull(h.TmModel.GetLineTokens(boundary));

                boundaryRelease.Set();
                Assert.IsTrue(afterEntered.Wait(Generous), "TMModel's thread never moved past the line");

                Assert.IsTrue(PumpUntil(() => !h.EditorModel.IsViewPortTokenizationPending), "the pass never finished");

                Assert.AreEqual("state.red", h.ScopeOf(boundary), "the line kept tokens computed from its old state");
            }
            finally
            {
                boundaryRelease.Set();
                afterRelease.Set();
                h.Grammar.BeforeTokenize = null;
            }
        }

        [AvaloniaTest]
        public void Only_One_Continuation_Is_Queued()
        {
            using var h = new Harness(holdTokenizerThread: true, lineCost: SlowLine);
            h.ScrollTo(100);
            var texts = h.TextsOf(h.VisibleLineIndexes());

            // Runs the pass the scroll posted, which queues the continuation, but not the continuation.
            var frame = new DispatcherFrame();
            Dispatcher.UIThread.Post(() => frame.Continue = false, DispatcherPriority.Default);
            Dispatcher.UIThread.PushFrame(frame);
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the viewport finished in its first slice");

            h.EditorModel.TokenizeViewPortCore();
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the viewport finished in its second slice");
            int before = h.Grammar.CallsFor(texts).Count();

            // Queued behind the continuation and ahead of the one that continuation posts.
            int atNextJob = -1;
            Dispatcher.UIThread.Post(() => atNextJob = h.Grammar.CallsFor(texts).Count(), DispatcherPriority.Background);
            RunJobs();

            Assert.LessOrEqual(atNextJob - before, 8, "two continuations ran one after the other");
        }

        // An exception out of an older request's run must not end the range of the request that replaced it.
        [AvaloniaTest]
        public void An_Exception_From_A_Replaced_Request_Leaves_The_Newer_Range_Pending()
        {
            ThrowingModel model = null;
            using var h = new Harness(holdTokenizerThread: true, lineCost: SlowLine,
                createModel: (view, document, handler) => model = new ThrowingModel(view, document, handler));
            List<int> newest = null;
            bool pendingAtThrow = false;
            model.OnNextLineCount = () =>
            {
                h.ScrollTo(300);
                newest = h.VisibleLineIndexes();
                var frame = new DispatcherFrame();
                Dispatcher.UIThread.Post(() => frame.Continue = false, DispatcherPriority.Default);
                Dispatcher.UIThread.PushFrame(frame);
                pendingAtThrow = h.EditorModel.IsViewPortTokenizationPending;
                throw new InvalidOperationException("from the older run");
            };

            h.ScrollTo(100);
            Assert.IsTrue(PumpUntil(() => newest != null && !h.EditorModel.IsViewPortTokenizationPending), "the passes never finished");

            Assert.IsTrue(pendingAtThrow, "the newer request had finished before the older run threw");
            Assert.IsTrue(h.Exceptions.TryDequeue(out var exception), "the exception did not reach the handler");
            Assert.AreEqual("from the older run", exception.Message);
            Assert.IsEmpty(newest.Where(i => h.TmModel.GetLineTokens(i) == null).ToList(), "the older run's exception ended the newer range");
        }

        // A source that keeps failing must not fail once per slice.
        [AvaloniaTest]
        public void An_Exception_Ends_The_Pass()
        {
            ThrowingModel model = null;
            using var h = new Harness(holdTokenizerThread: true, lineCost: SlowLine,
                createModel: (view, document, handler) => model = new ThrowingModel(view, document, handler));
            int thrown = 0;
            void Throw()
            {
                thrown++;
                model.OnNextLineCount = Throw;
                throw new InvalidOperationException("from the run");
            }

            try
            {
                model.OnNextLineCount = Throw;
                h.ScrollTo(100);
                Assert.IsTrue(PumpUntil(() => !h.EditorModel.IsViewPortTokenizationPending), "the pass went on after the exception");
                RunJobs();
                Assert.AreEqual(1, thrown, "a run started after the pass failed");
            }
            finally
            {
                model.OnNextLineCount = null;
            }

            Assert.IsTrue(h.Exceptions.TryDequeue(out var exception), "the exception did not reach the handler");
            Assert.AreEqual("from the run", exception.Message);
        }

        // A queued continuation that a run's grammar runs by pumping the dispatcher can fail there; the pass it ends
        // stays ended when the run around it returns.
        [AvaloniaTest]
        public void An_Exception_In_A_Slice_Inside_A_Run_Ends_The_Pass()
        {
            ThrowingModel model = null;
            using var h = new Harness(holdTokenizerThread: true, lineCost: SlowLine,
                createModel: (view, document, handler) => model = new ThrowingModel(view, document, handler));
            h.ScrollTo(100);
            h.Settle();
            var viewport = h.VisibleLineIndexes();
            int target = viewport[0] + 8;
            foreach (int i in viewport)
                h.TmModel.InvalidateLine(i);
            h.EditorModel.TokenizeViewPortCore();
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the first slice tokenized the whole viewport");

            // The lines after the run that pumps.
            var after = h.TextsOf(Enumerable.Range(target + 8, viewport[^1] - target - 7));
            Assert.IsNotEmpty(after, "the viewport ends inside the run");
            int afterCalls = h.Grammar.CallsFor(after).Count();
            bool entered = false;
            bool endedInside = false;
            h.Grammar.BeforeTokenize = text =>
            {
                if (entered || !Dispatcher.UIThread.CheckAccess() || text != "v" + target)
                    return;
                entered = true;
                model.OnNextLineCount = () => throw new InvalidOperationException("from the nested slice");
                endedInside = PumpUntil(() => !h.EditorModel.IsViewPortTokenizationPending);
            };
            h.EditorModel.TokenizeViewPortCore();
            h.Grammar.BeforeTokenize = null;
            Assert.IsTrue(PumpUntil(() => !h.EditorModel.IsViewPortTokenizationPending), "the pass never finished");

            Assert.IsTrue(entered, "the run never reached the line");
            Assert.IsTrue(endedInside, "the queued continuation did not end the pass inside the run");
            Assert.IsTrue(h.Exceptions.TryDequeue(out var exception), "the exception did not reach the handler");
            Assert.AreEqual("from the nested slice", exception.Message);
            Assert.AreEqual(afterCalls, h.Grammar.CallsFor(after).Count(), "the run around the failed slice went on with the pass");
        }

        // The same continuation can fail inside the redraw a newer pass makes before it starts, which still runs.
        [AvaloniaTest]
        public void An_Exception_In_A_Slice_During_The_Redraw_Before_A_Pass_Leaves_That_Pass_Running()
        {
            var view = new InvalidationView();
            ThrowingModel model = null;
            using var h = new Harness(holdTokenizerThread: true, lineCost: OverBudgetLine, view: view,
                createModel: (v, document, handler) => model = new ThrowingModel(v, document, handler));
            h.ScrollTo(100);
            h.Settle();
            var viewport = h.VisibleLineIndexes();
            for (int i = viewport[0]; i < viewport[0] + 24; i++)
                h.TmModel.InvalidateLine(i);
            h.EditorModel.TokenizeViewPortCore();
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the first slice ended the pass");

            bool failedInside = false;
            view.OnNextMeasureInvalidated = () =>
            {
                model.OnNextLineCount = () => throw new InvalidOperationException("from the older pass");
                RunJobs();
                failedInside = h.Exceptions.Count == 1;
            };
            h.EditorModel.TokenizeViewPortCore();
            Assert.IsTrue(PumpUntil(() => !h.EditorModel.IsViewPortTokenizationPending), "the pass never finished");

            Assert.IsTrue(failedInside, "the queued continuation did not fail inside the redraw");
            Assert.IsTrue(h.Exceptions.TryDequeue(out var exception), "the exception did not reach the handler");
            Assert.AreEqual("from the older pass", exception.Message);
            Assert.IsEmpty(viewport.Where(i => h.TmModel.IsLineInvalid(i) || h.TmModel.GetLineTokens(i) == null).ToList(),
                "the failure inside the redraw ended the newer pass");
        }

        // The coloring transformer left the earlier runs' lines to the pass's redraw, which a rethrow must not skip.
        [AvaloniaTest]
        public void A_Pass_Ended_By_A_Rethrowing_Handler_Redraws_The_Lines_It_Tokenized()
        {
            ThrowingModel model = null;
            using var h = new Harness(holdTokenizerThread: true, lineCost: SlowLine, withTransformer: true,
                createModel: (view, document, handler) =>
                    model = new ThrowingModel(view, document, e => { handler(e); throw e; }));
            h.ScrollTo(100);
            var shown = h.View.VisualLines.ToList();
            RunJobs();
            Assert.IsTrue(h.EditorModel.IsViewPortTokenizationPending, "the viewport finished in its first slice");
            var tokenized = shown.Where(l => h.TmModel.GetLineTokens(l.FirstDocumentLine.LineNumber - 1) != null).ToList();
            Assert.IsNotEmpty(tokenized, "the first slice tokenized nothing");
            Assert.IsFalse(tokenized.Any(l => l.IsDisposed), "a run redrew lines while the pass went on");

            model.OnNextLineCount = () => throw new InvalidOperationException("from the run");
            Assert.Catch<InvalidOperationException>(RunJobs, "the handler's rethrow was swallowed");

            Assert.IsFalse(h.EditorModel.IsViewPortTokenizationPending, "the exception did not end the pass");
            Assert.IsTrue(tokenized.All(l => l.IsDisposed), "lines the pass tokenized kept a build without their tokens");
            Assert.IsTrue(h.Exceptions.TryDequeue(out var exception), "the exception did not reach the handler");
            Assert.AreEqual("from the run", exception.Message);
        }

        private sealed class InvalidationView : TextView
        {
            public Action OnNextMeasureInvalidated { get; set; }

            protected override void OnMeasureInvalidated()
            {
                base.OnMeasureInvalidated();
                var action = OnNextMeasureInvalidated;
                OnNextMeasureInvalidated = null;
                action?.Invoke();
            }
        }

        private sealed class ThrowingModel : TextEditorModel
        {
            public ThrowingModel(TextView view, TextDocument document, Action<Exception> exceptionHandler)
                : base(view, document, exceptionHandler)
            {
            }

            public Action OnNextLineCount { get; set; }

            public override int GetNumberOfLines()
            {
                var action = OnNextLineCount;
                if (action != null && Dispatcher.UIThread.CheckAccess())
                {
                    OnNextLineCount = null;
                    action();
                }

                return base.GetNumberOfLines();
            }
        }

        private sealed class CallbackListener : IModelTokensChangedListener
        {
            private readonly Action _callback;
            public CallbackListener(Action callback) => _callback = callback;
            public void ModelTokensChanged(ModelTokensChangedEvent e) => _callback();
        }

        private sealed class CallbackTransformer : IVisualLineTransformer
        {
            private readonly Action<VisualLine> _callback;
            public CallbackTransformer(Action<VisualLine> callback) => _callback = callback;
            public void Transform(ITextRunConstructionContext context, IList<VisualLineElement> elements) =>
                _callback(context.VisualLine);
        }

        // Dispatcher.RunJobs returns only when the queue is empty, so a slice that keeps posting itself would
        // never return it and PumpUntil's timeout would never be checked. This round ends behind what is queued.
        private static void RunJobs()
        {
            var frame = new DispatcherFrame();
            Dispatcher.UIThread.Post(() => frame.Continue = false, DispatcherPriority.Background);
            Dispatcher.UIThread.PushFrame(frame);
        }

        // Runs what is queued at Default and above, not the pass's Background continuation.
        private static void RunDefaultJobs()
        {
            var frame = new DispatcherFrame();
            Dispatcher.UIThread.Post(() => frame.Continue = false, DispatcherPriority.Default);
            Dispatcher.UIThread.PushFrame(frame);
        }

        private static bool PumpUntil(Func<bool> condition) =>
            SpinWait.SpinUntil(() =>
            {
                RunJobs();
                return condition();
            }, Generous);

        private sealed class Harness : IDisposable
        {
            public TextDocument Document { get; }
            public TextView View { get; }
            public TextEditorModel EditorModel { get; }
            public TMModel TmModel { get; }
            public ScriptedGrammar Grammar { get; }
            public TextMateColoringTransformer Transformer { get; }
            public ConcurrentQueue<Exception> Exceptions { get; } = new ConcurrentQueue<Exception>();
            private readonly ManualResetEventSlim _releaseTokenizerThread = new ManualResetEventSlim(false);

            public Harness(bool holdTokenizerThread, TimeSpan lineCost = default, bool stateful = false,
                Func<TextView, TextDocument, Action<Exception>, TextEditorModel> createModel = null,
                bool withTransformer = false, TextView view = null)
            {
                var lines = new List<string> { "HEAD" };
                for (int i = 1; i < LineCount; i++)
                    lines.Add("v" + i);
                Document = new TextDocument(string.Join("\n", lines));

                View = view ?? new TextView();
                View.Document = Document;
                Layout();

                // Line 0 is never in the viewport, so only TMModel's own thread tokenizes it.
                Grammar = new ScriptedGrammar(lineCost, stateful,
                    holdTokenizerThread ? text => { if (text == "HEAD") _releaseTokenizerThread.Wait(); } : null);

                EditorModel = createModel?.Invoke(View, Document, Exceptions.Enqueue)
                    ?? new TextEditorModel(View, Document, Exceptions.Enqueue);
                TmModel = new TMModel(EditorModel);
                TmModel.SetGrammar(Grammar);

                if (withTransformer)
                {
                    Transformer = new TextMateColoringTransformer(View, Exceptions.Enqueue);
                    Transformer.SetModel(Document, TmModel);
                    TmModel.AddModelTokensChangedListener(Transformer);
                }
            }

            // The scroll also posts its own pass, which runs at the next pump as a second request.
            public void ScrollTo(int firstLineNumber)
            {
                ((IScrollable)View).Offset = new Vector(0, View.GetVisualTopByDocumentLine(firstLineNumber));
                Layout();
                Assert.IsTrue(View.VisualLinesValid);
                Assert.That(Math.Abs(View.VisualLines[0].FirstDocumentLine.LineNumber - firstLineNumber), Is.LessThanOrEqualTo(1),
                    "the viewport should start at the requested line and never include line 0");
            }

            // Runs what the scroll posted, so a test starts from a quiet dispatcher, and lays out the lines the
            // pass redrew.
            public void Settle()
            {
                Assert.IsTrue(PumpUntil(() => !EditorModel.IsViewPortTokenizationPending), "the scroll's pass never finished");
                Layout();
            }

            public void ReleaseTokenizerThread() => _releaseTokenizerThread.Set();

            public void WaitForTokenizerThread() =>
                Assert.IsTrue(SpinWait.SpinUntil(() => Enumerable.Range(0, LineCount)
                    .All(i => !TmModel.IsLineInvalid(i) && TmModel.GetLineTokens(i) != null), Generous),
                    "TMModel's tokenizer thread never finished the document");

            public List<int> VisibleLineIndexes()
            {
                int first = View.VisualLines[0].FirstDocumentLine.LineNumber - 1;
                int last = View.VisualLines[^1].LastDocumentLine.LineNumber - 1;
                return Enumerable.Range(first, last - first + 1).ToList();
            }

            public HashSet<string> TextsOf(IEnumerable<int> lineIndexes) =>
                lineIndexes.Select(i => Document.GetText(Document.Lines[i])).ToHashSet();

            public string ScopeOf(int lineIndex)
            {
                var tokens = TmModel.GetLineTokens(lineIndex);
                return tokens == null || tokens.Count == 0 ? null : tokens[0].Scopes.LastOrDefault();
            }

            public void Layout()
            {
                View.Measure(ViewSize);
                View.Arrange(new Rect(ViewSize));
            }

            public void Dispose()
            {
                _releaseTokenizerThread.Set();
                RunJobs();
                TmModel.Dispose(); // also disposes EditorModel (TMModel disposes its lines)
                Transformer?.Dispose();
                Assert.IsEmpty(Exceptions, "the exception handler was called: " + string.Join("; ", Exceptions));
            }
        }

        // Scope "text.<first char>", or with stateful: true "state.red"/"state.green", where a line
        // "RED" or "GREEN" sets the state and every other line keeps the one it starts with.
        internal sealed class ScriptedGrammar : IGrammar
        {
            private readonly TimeSpan _lineCost;
            private readonly bool _stateful;
            private readonly Action<string> _beforeTokenize;

            public Action<string> BeforeTokenize { get; set; }

            public ScriptedGrammar(TimeSpan lineCost, bool stateful, Action<string> beforeTokenize)
            {
                _lineCost = lineCost;
                _stateful = stateful;
                _beforeTokenize = beforeTokenize;
            }

            public ConcurrentQueue<(string Text, int Thread)> Calls { get; } = new ConcurrentQueue<(string, int)>();
            public IEnumerable<(string Text, int Thread)> CallsFor(HashSet<string> texts) => Calls.Where(c => texts.Contains(c.Text));
            public bool IsCompiling => false;

            public string GetName() => "scripted";
            public string GetScopeName() => "source.test";
            public ICollection<string> GetFileTypes() => new List<string>();
            public ITokenizeLineResult TokenizeLine(LineText lineText) => TokenizeLine(lineText, null, TimeSpan.Zero);
            public ITokenizeLineResult2 TokenizeLine2(LineText lineText) => throw new NotSupportedException();
            public ITokenizeLineResult2 TokenizeLine2(LineText lineText, IStateStack prevState, TimeSpan timeLimit) =>
                throw new NotSupportedException();

            public ITokenizeLineResult TokenizeLine(LineText lineText, IStateStack prevState, TimeSpan timeLimit)
            {
                string text = lineText.ToString().TrimEnd('\r', '\n');
                _beforeTokenize?.Invoke(text);
                BeforeTokenize?.Invoke(text);
                if (_lineCost > TimeSpan.Zero && text != "HEAD")
                {
                    long until = Stopwatch.GetTimestamp() + (long)(_lineCost.TotalSeconds * Stopwatch.Frequency);
                    while (Stopwatch.GetTimestamp() < until) { }
                }

                Calls.Enqueue((text, Environment.CurrentManagedThreadId));

                IStateStack state = prevState;
                string scope = text.Length == 0 ? "text.empty" : "text." + text[0];
                if (_stateful)
                {
                    var current = text == "RED" ? State.Red : text == "GREEN" ? State.Green : prevState as State ?? State.Green;
                    state = current;
                    scope = current == State.Red ? "state.red" : "state.green";
                }

                return new Result(new IToken[] { new Token(0, text.Length, new List<string> { "source.test", scope }) }, state);
            }

            // Two instances, compared by reference by TMModel's state check.
            private sealed class State : IStateStack
            {
                public static readonly State Red = new State("red");
                public static readonly State Green = new State("green");
                private State(string name) => EndRule = name;
                public int Depth => 1;
                public TextMateSharp.Internal.Rules.RuleId RuleId => default;
                public string EndRule { get; }
            }

            private sealed class Result : ITokenizeLineResult
            {
                public Result(IToken[] tokens, IStateStack ruleStack)
                {
                    Tokens = tokens;
                    RuleStack = ruleStack;
                }

                public IToken[] Tokens { get; }
                public IStateStack RuleStack { get; }
            }

            private sealed class Token : IToken
            {
                public Token(int start, int end, List<string> scopes)
                {
                    StartIndex = start;
                    EndIndex = end;
                    Scopes = scopes;
                }

                public int StartIndex { get; set; }
                public int EndIndex { get; }
                public int Length => EndIndex - StartIndex;
                public List<string> Scopes { get; }
            }
        }
    }
}
