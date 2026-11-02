using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using AvaloniaEdit.AvaloniaMocks;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.TextMate;
using NUnit.Framework;
using TextMateSharp.Grammars;
using TextMateSharp.Model;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace AvaloniaEdit.Tests.TextMate
{
    // Pins which scrolls run the viewport pass. The harness holds TMModel's own tokenizer thread at line 0 unless
    // a test releases it, so every token of a visible line comes from the pass.
    [TestFixture]
    internal class TextEditorModelScrollTests
    {
        private static readonly TimeSpan Generous = TimeSpan.FromSeconds(10);
        private const int LineCount = 500;
        private static readonly Size ViewSize = new Size(400, 600);

        [AvaloniaTest]
        public void A_Sideways_Scroll_Does_Not_Run_The_ViewPort_Pass()
        {
            using var h = new Harness();
            h.ScrollTo(100);
            Dispatcher.UIThread.RunJobs();
            h.EditLine(h.VisibleLineIndexes()[3]);
            int calls = h.Grammar.Calls.Count;

            h.ScrollSidewaysTo(50);
            Dispatcher.UIThread.RunJobs();

            Assert.AreEqual(calls, h.Grammar.Calls.Count, "a scroll that shows the same lines tokenized on the UI thread");
        }

        [AvaloniaTest]
        public void Typing_Past_The_Right_Edge_Does_Not_Tokenize_On_The_UI_Thread()
        {
            UnitTestApplication.InitializeStyles();
            string wide = new string('.', 400);
            var text = string.Join("\n", Enumerable.Range(0, LineCount).Select(i => "v" + i + wide));
            var editor = new TextEditor { Document = new TextDocument(text) };
            var window = new Window { Width = 400, Height = 400, Content = editor };
            window.Show();
            try
            {
                editor.ApplyTemplate();
                editor.UpdateLayout();
                Dispatcher.UIThread.RunJobs();

                int uiThread = Environment.CurrentManagedThreadId;
                int uiCalls = 0;
                var model = new TextEditorModel(editor.TextArea.TextView, editor.Document, null);
                using var tmModel = new TMModel(model);
                tmModel.SetGrammar(new ScriptedGrammar(_ => { if (Environment.CurrentManagedThreadId == uiThread) uiCalls++; }));
                Assert.IsTrue(SpinWait.SpinUntil(() => Enumerable.Range(0, LineCount).All(i => !tmModel.IsLineInvalid(i)), Generous),
                    "TMModel's tokenizer thread never finished the document");

                var view = editor.TextArea.TextView;
                editor.ScrollToLine(200);
                Dispatcher.UIThread.RunJobs();
                var line = view.VisualLines[3].FirstDocumentLine;
                editor.CaretOffset = line.EndOffset;
                editor.TextArea.Caret.BringCaretToView();
                Dispatcher.UIThread.RunJobs();
                double x = view.HorizontalOffset;
                uiCalls = 0;

                for (int i = 0; i < 10; i++)
                {
                    editor.TextArea.PerformTextInput("x");
                    Dispatcher.UIThread.RunJobs();
                }

                Assert.That(view.HorizontalOffset, Is.GreaterThan(x), "the keys were meant to scroll sideways");
                Assert.AreEqual(0, uiCalls, "a key that scrolled sideways tokenized on the UI thread");
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaTest]
        public void The_Edited_Line_Gets_Its_Tokens_From_TMModels_Thread_After_A_Sideways_Scroll()
        {
            using var h = new Harness();
            h.ScrollTo(100);
            Dispatcher.UIThread.RunJobs();
            int edited = h.VisibleLineIndexes()[3];
            h.EditLine(edited);
            h.ScrollSidewaysTo(50);
            Dispatcher.UIThread.RunJobs();

            h.ReleaseTokenizerThread();

            Assert.IsTrue(SpinWait.SpinUntil(() => !h.TmModel.IsLineInvalid(edited), Generous), "the edited line stayed invalid");
            Assert.AreEqual("text.w", h.ScopeOf(edited));
        }

        [AvaloniaTest]
        public void A_Vertical_Scroll_While_Scrolled_Sideways_Runs_The_ViewPort_Pass()
        {
            using var h = new Harness();
            h.ScrollTo(100);
            Dispatcher.UIThread.RunJobs();
            h.ScrollSidewaysTo(50);
            Dispatcher.UIThread.RunJobs();

            h.ScrollTo(300, x: 50);
            var viewport = h.VisibleLineIndexes();
            Dispatcher.UIThread.RunJobs();

            Assert.IsEmpty(h.WithoutTokens(viewport), "visible lines were left without tokens");
        }

        [AvaloniaTest]
        public void A_Scroll_On_Both_Axes_Runs_The_ViewPort_Pass()
        {
            using var h = new Harness();
            h.ScrollTo(100);
            Dispatcher.UIThread.RunJobs();

            h.ScrollTo(300, x: 50);
            var viewport = h.VisibleLineIndexes();
            Dispatcher.UIThread.RunJobs();

            Assert.IsEmpty(h.WithoutTokens(viewport), "visible lines were left without tokens");
        }

        [AvaloniaTest]
        public void Scrolling_Back_To_An_Earlier_Offset_Runs_The_ViewPort_Pass()
        {
            using var h = new Harness();
            h.ScrollTo(100);
            Dispatcher.UIThread.RunJobs();
            string first = h.TextOf(h.VisibleLineIndexes()[0]);
            h.ScrollTo(300);
            Dispatcher.UIThread.RunJobs();
            int calls = h.Grammar.Calls.Count(c => c == first);

            h.ScrollTo(100);
            Dispatcher.UIThread.RunJobs();

            Assert.AreEqual(calls + 1, h.Grammar.Calls.Count(c => c == first), "the first visible line was not tokenized again");
        }

        // TMModel's thread can store tokens of a line's old text and clear its invalid flag when an edit lands
        // while it tokenizes; the state is set directly here instead of reproducing that race.
        [AvaloniaTest]
        public void A_Vertical_Scroll_Of_One_Pixel_Repairs_A_Stale_First_Line()
        {
            using var h = new Harness();
            h.ScrollTo(100, x: 50);
            h.NudgeDown();
            Dispatcher.UIThread.RunJobs();
            int first = h.VisibleLineIndexes()[0];
            Assert.AreEqual("text.v", h.ScopeOf(first));
            h.EditLine(first);
            h.EditorModel.Get(first).IsInvalid = false;

            h.NudgeDown();
            Dispatcher.UIThread.RunJobs();

            Assert.AreEqual(first, h.VisibleLineIndexes()[0], "the scroll was meant to stay on the same first line");
            Assert.AreEqual("text.w", h.ScopeOf(first), "the first visible line kept tokens of its old text");
        }

        // A resize or a fold brings in lines without raising a vertical scroll. TMModel walks past a line whose
        // invalid flag is clear, so the flag is set as it is on a line that was never tokenized.
        [AvaloniaTheory]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(5)]
        [TestCase(-1)]
        public void A_Sideways_Scroll_Runs_The_ViewPort_Pass_For_One_Line_Without_Tokens(int position)
        {
            using var h = new Harness();
            h.ScrollTo(100);
            Dispatcher.UIThread.RunJobs();
            var viewport = h.VisibleLineIndexes();
            var line = h.EditorModel.Get(viewport[position < 0 ? viewport.Count + position : position]);
            line.Tokens = null;
            line.IsInvalid = true;

            h.ScrollSidewaysTo(50);
            Dispatcher.UIThread.RunJobs();

            Assert.IsEmpty(h.WithoutTokens(viewport), "a visible line was left without tokens");
        }

        // A grammar change clears every line's tokens and raises no scroll.
        [AvaloniaTest]
        public void A_Sideways_Scroll_After_A_Grammar_Change_Runs_The_ViewPort_Pass()
        {
            using var h = new Harness();
            h.ScrollTo(100);
            Dispatcher.UIThread.RunJobs();
            var viewport = h.VisibleLineIndexes();
            using var hold = new ManualResetEventSlim(false);
            h.TmModel.SetGrammar(new ScriptedGrammar(text => { if (text == "HEAD") hold.Wait(); }));
            Assert.IsNotEmpty(h.WithoutTokens(viewport), "the new grammar's thread was meant to be held");

            h.ScrollSidewaysTo(50);
            Dispatcher.UIThread.RunJobs();
            var left = h.WithoutTokens(viewport);
            hold.Set();

            Assert.IsEmpty(left, "visible lines were left without tokens");
        }

        // A hidden view gets no layout, so its pass sees the old lines at the new offset.
        [AvaloniaTest]
        public void A_Sideways_Scroll_After_A_Pass_On_Old_Lines_Runs_The_ViewPort_Pass()
        {
            using var h = new Harness();
            h.ScrollTo(100);
            Dispatcher.UIThread.RunJobs();
            ((IScrollable)h.View).Offset = new Vector(0, h.View.GetVisualTopByDocumentLine(300));
            Assert.IsTrue(h.View.VisualLinesValid);
            Dispatcher.UIThread.RunJobs();
            h.Layout();
            var viewport = h.VisibleLineIndexes();
            Assert.IsNotEmpty(h.WithoutTokens(viewport), "the pass was meant to see the old lines");

            h.ScrollSidewaysTo(50);
            Dispatcher.UIThread.RunJobs();

            Assert.IsEmpty(h.WithoutTokens(viewport), "visible lines were left without tokens");
        }

        private sealed class Harness : IDisposable
        {
            public TextDocument Document { get; }
            public TextView View { get; }
            public TextEditorModel EditorModel { get; }
            public TMModel TmModel { get; }
            public ScriptedGrammar Grammar { get; }
            public ConcurrentQueue<Exception> Exceptions { get; } = new ConcurrentQueue<Exception>();
            private readonly ManualResetEventSlim _releaseTokenizerThread = new ManualResetEventSlim(false);

            public Harness()
            {
                var lines = new List<string> { "HEAD" };
                string wide = new string('.', 400);
                for (int i = 1; i < LineCount; i++)
                    lines.Add("v" + i + wide);
                Document = new TextDocument(string.Join("\n", lines));

                View = new TextView { Document = Document };
                Layout();

                // Line 0 is never in the viewport, so only TMModel's own thread tokenizes it.
                Grammar = new ScriptedGrammar(text => { if (text == "HEAD") _releaseTokenizerThread.Wait(); });

                EditorModel = new TextEditorModel(View, Document, Exceptions.Enqueue);
                TmModel = new TMModel(EditorModel);
                TmModel.SetGrammar(Grammar);
            }

            public void ScrollTo(int firstLineNumber, double x = 0)
            {
                ((IScrollable)View).Offset = new Vector(x, View.GetVisualTopByDocumentLine(firstLineNumber));
                Layout();
                Assert.AreEqual(x, View.HorizontalOffset);
                Assert.IsTrue(View.VisualLinesValid);
                Assert.That(Math.Abs(View.VisualLines[0].FirstDocumentLine.LineNumber - firstLineNumber), Is.LessThanOrEqualTo(1),
                    "the viewport should start at the requested line and never include line 0");
            }

            public void ScrollSidewaysTo(double x)
            {
                double y = View.VerticalOffset;
                ((IScrollable)View).Offset = new Vector(x, y);
                Layout();
                Assert.AreEqual(x, View.HorizontalOffset);
                Assert.AreEqual(y, View.VerticalOffset);
            }

            public void NudgeDown()
            {
                ((IScrollable)View).Offset = View.ScrollOffset + new Vector(0, 1);
                Layout();
            }

            public void EditLine(int lineIndex)
            {
                Document.Replace(Document.Lines[lineIndex].Offset, 1, "w");
                Layout();
            }

            public void ReleaseTokenizerThread() => _releaseTokenizerThread.Set();

            public List<int> VisibleLineIndexes()
            {
                int first = View.VisualLines[0].FirstDocumentLine.LineNumber - 1;
                int last = View.VisualLines[^1].LastDocumentLine.LineNumber - 1;
                return Enumerable.Range(first, last - first + 1).ToList();
            }

            public List<int> WithoutTokens(IEnumerable<int> lineIndexes) =>
                lineIndexes.Where(i => TmModel.GetLineTokens(i) == null).ToList();

            public string TextOf(int lineIndex) => Document.GetText(Document.Lines[lineIndex]);

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
                Dispatcher.UIThread.RunJobs();
                TmModel.Dispose(); // also disposes EditorModel (TMModel disposes its lines)
                Assert.IsEmpty(Exceptions, "the exception handler was called: " + string.Join("; ", Exceptions));
            }
        }

        // Scope "text.<first char>".
        private sealed class ScriptedGrammar : IGrammar
        {
            private readonly Action<string> _beforeTokenize;

            public ScriptedGrammar(Action<string> beforeTokenize) => _beforeTokenize = beforeTokenize;

            public ConcurrentQueue<string> Calls { get; } = new ConcurrentQueue<string>();
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
                Calls.Enqueue(text);
                string scope = text.Length == 0 ? "text.empty" : "text." + text[0];
                return new Result(new IToken[] { new Token(0, text.Length, new List<string> { "source.test", scope }) }, prevState);
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
