using System;
using System.Diagnostics;
using System.Threading;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using TextMateSharp.Grammars;
using TextMateSharp.Model;

namespace AvaloniaEdit.TextMate
{
    public class TextEditorModel : AbstractLineList, IDisposable
    {
        private readonly TextDocument _document;
        private readonly TextView _textView;
        private readonly DocumentSnapshot _documentSnapshot;
        private readonly Action<Exception> _exceptionHandler;
        private InvalidLineRange? _invalidRange;
        private bool _isDisposed;

        // UI thread only. A slice stops at the first run boundary past the budget, so one run, not the budget, bounds
        // what a slow grammar takes from a dispatcher turn. Best tried on one slow and one fast grammar: longer runs
        // made frames longer, shorter ones delayed colour, a 16 ms budget only made frames longer.
        private const double ViewPortSliceBudgetMs = 4;
        private const int ViewPortRunLines = 8;
        private int _viewPortNext = -1;
        private int _viewPortEnd = -1;
        private int _viewPortRequest;
        private bool _viewPortSlicePosted;
        private int _runFirst = -1;
        private int _runLast = -1;
        private int _runRedraws;
        private int _redrawFirst = -1;
        private int _redrawLast = -1;
        private int _redraws;
        private bool _viewPortRestart;
        // From the model's Changing to the end of the update, which TextDocument raises also when a Changing handler
        // throws.
        private bool _inDocumentChange;

        // TextView throws when a line it is building is redrawn, and a line transformer can pump the dispatcher during
        // layout, so a redraw asked for during a build waits for the next build's start. A new model assumes a build
        // until it sees one end. UI thread only.
        private bool _buildingVisualLines = true;
        private RedrawAtNextBuild _redrawAtNextBuild;

        public DocumentSnapshot DocumentSnapshot => _documentSnapshot;
        internal InvalidLineRange? InvalidRange => _invalidRange;
        internal bool IsViewPortTokenizationPending => _viewPortNext >= 0 && _viewPortNext <= _viewPortEnd;

        // True while a run's ForceTokenization raises events for lines inside that run, which the coloring transformer
        // of this model's view leaves to the pass's redraw: not for another view, which the pass does not redraw, nor
        // for lines outside the run, nor once the held lines, which include the run's, are redrawn during it (an edit,
        // a newer request or a pass ended inside it) or the model is disposed. UI thread only.
        internal bool IsInViewPortRun(TextView textView, int firstLineIndex, int lastLineIndex) =>
            textView == _textView && _runFirst >= 0 && _runRedraws == _redraws && !Volatile.Read(ref _isDisposed) &&
            firstLineIndex >= _runFirst && lastLineIndex <= _runLast;

        public TextEditorModel(TextView textView, TextDocument document, Action<Exception> exceptionHandler)
        {
            _textView = textView;
            _document = document;
            _exceptionHandler = exceptionHandler;

            _documentSnapshot = new DocumentSnapshot(_document);

            for (int i = 0; i < _document.LineCount; i++)
                AddLine(i);

            _document.Changing += DocumentOnChanging;
            _document.Changed += DocumentOnChanged;
            _document.UpdateFinished += DocumentOnUpdateFinished;
            _textView.ScrollOffsetChanged += TextView_ScrollOffsetChanged;
            _textView.VisualLineConstructionStarting += TextView_VisualLineConstructionStarting;
            _textView.VisualLinesChanged += TextView_VisualLinesChanged;
        }

        public override void Dispose()
        {
            // Fast path: Volatile.Read avoids redundant unsubscription when
            // already disposed.
            if (Volatile.Read(ref _isDisposed))
                return;

            // Volatile.Write ensures that Volatile.Read callers (event handlers,
            // public method guards) see this write with proper memory ordering.
            Volatile.Write(ref _isDisposed, true);

            _document.Changing -= DocumentOnChanging;
            _document.Changed -= DocumentOnChanged;
            _document.UpdateFinished -= DocumentOnUpdateFinished;
            _textView.ScrollOffsetChanged -= TextView_ScrollOffsetChanged;

            // Rebuilds the lines the pass tokenized; the view may show another document by now, so it is redrawn whole.
            // Posted: a line transformer can dispose the model during layout. The view's build events stay subscribed
            // until that job runs.
            Dispatcher.UIThread.Post(RedrawView);
        }

        private void TextView_VisualLineConstructionStarting(object sender, VisualLineConstructionStartEventArgs e) =>
            _buildingVisualLines = true;

        // Raised when a measure that built lines ends; a build that throws leaves the flag set until the next ends. A
        // build during a document change shows the lines before it, so the restart waits for one after it.
        private void TextView_VisualLinesChanged(object sender, EventArgs e)
        {
            _buildingVisualLines = false;
            if (_viewPortRestart && !_inDocumentChange)
            {
                _viewPortRestart = false;
                TokenizeViewPort();
            }
        }

        private void RedrawView()
        {
            // Unsubscribed before the redraw, whose exception the handler may rethrow.
            if (Volatile.Read(ref _isDisposed))
            {
                _textView.VisualLineConstructionStarting -= TextView_VisualLineConstructionStarting;
                _textView.VisualLinesChanged -= TextView_VisualLinesChanged;
            }

            try
            {
                if (!_buildingVisualLines)
                    _textView.Redraw();
                else if (_redrawAtNextBuild == null || _redrawAtNextBuild.IsDone)
                    _redrawAtNextBuild = new RedrawAtNextBuild(_textView);
            }
            catch (Exception ex)
            {
                _exceptionHandler?.Invoke(ex);
            }
        }

        // Redraws the whole view when its next build starts, before any line is reused. Holds only the view, not the
        // exception handler, whose target can keep a disposed model alive; what the redraw throws goes to that layout.
        // Ends at a measure with no document, which builds no lines.
        private sealed class RedrawAtNextBuild
        {
            private readonly TextView _textView;

            public RedrawAtNextBuild(TextView textView)
            {
                _textView = textView;
                _textView.VisualLineConstructionStarting += OnBuildStarting;
                _textView.VisualLinesChanged += OnLinesChanged;
                // Measure marks the view valid before building, so this re-measures after a running build.
                _textView.InvalidateMeasure();
            }

            public bool IsDone { get; private set; }

            private void OnBuildStarting(object sender, VisualLineConstructionStartEventArgs e)
            {
                End();
                _textView.Redraw();
            }

            private void OnLinesChanged(object sender, EventArgs e)
            {
                if (_textView.Document == null)
                    End();
            }

            private void End()
            {
                IsDone = true;
                _textView.VisualLineConstructionStarting -= OnBuildStarting;
                _textView.VisualLinesChanged -= OnLinesChanged;
            }
        }

        public override void UpdateLine(int lineIndex) { }

        /// <summary>
        /// Invalidates the visual lines currently displayed in the viewport, causing them to be refreshed or redrawn.
        /// </summary>
        /// <remarks>Call this method when changes to the underlying document or view state require the
        /// visible lines to be updated. This method has no effect if the visual lines are already invalid or if there
        /// are no visual lines present.</remarks>
        /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
        public void InvalidateViewPortLines()
        {
            ThrowIfDisposed();

            if (!_textView.VisualLinesValid ||
                _textView.VisualLines.Count == 0)
                return;

            InvalidateLineRange(
                _textView.VisualLines[0].FirstDocumentLine.LineNumber - 1,
                _textView.VisualLines[^1].LastDocumentLine.LineNumber - 1);
        }

        public override int GetNumberOfLines()
        {
            return _documentSnapshot.LineCount;
        }

        public override LineText GetLineTextIncludingTerminators(int lineIndex)
        {
            return _documentSnapshot.GetLineTextIncludingTerminatorAsMemory(lineIndex);
        }

        public override int GetLineLength(int lineIndex)
        {
            return _documentSnapshot.GetLineLength(lineIndex);
        }

        private void TextView_ScrollOffsetChanged(object sender, EventArgs e)
        {
            if (Volatile.Read(ref _isDisposed))
                return;

            TokenizeViewPort();
        }

        private void DocumentOnChanging(object sender, DocumentChangeEventArgs e)
        {
            if (Volatile.Read(ref _isDisposed))
                return;

            _inDocumentChange = true;
            // Before the change, while the held line indexes still name the lines the pass tokenized.
            RedrawTokenizedLines();

            try
            {
                RestartViewPortIfEditReachesIt(_document.GetLineByOffset(e.Offset).LineNumber - 1);

                if (e.RemovalLength > 0)
                {
                    var startLine = _document.GetLineByOffset(e.Offset).LineNumber - 1;
                    var endLine = _document.GetLineByOffset(e.Offset + e.RemovalLength).LineNumber - 1;
                    for (int i = endLine; i > startLine; i--)
                    {
                        RemoveLine(i);
                    }

                    _documentSnapshot.RemoveLines(startLine, endLine);
                }
            }
            catch (Exception ex)
            {
                _exceptionHandler?.Invoke(ex);
            }
        }

        private void DocumentOnChanged(object sender, DocumentChangeEventArgs e)
        {
            if (Volatile.Read(ref _isDisposed))
                return;

            try
            {
                int startLine = _document.GetLineByOffset(e.Offset).LineNumber - 1;
                // A Changing handler after the model's can start a pass, which names lines as they were before.
                RestartViewPortIfEditReachesIt(startLine);
                int endLine = startLine;
                if (e.InsertionLength > 0)
                {
                    endLine = _document.GetLineByOffset(e.Offset + e.InsertionLength).LineNumber - 1;

                    for (int i = startLine; i < endLine; i++)
                    {
                        AddLine(i);
                    }
                }

                _documentSnapshot.Update(e);

                if (startLine == 0)
                {
                    SetInvalidRange(startLine, endLine);
                    return;
                }

                // some grammars (JSON, csharp, ...)
                // need to invalidate the previous line too

                SetInvalidRange(startLine - 1, endLine);
            }
            catch (Exception ex)
            {
                _exceptionHandler?.Invoke(ex);
            }
            finally
            {
                // Lines held now were held after the redraw before the change, by a slice a callback ran there: they
                // name lines by their numbers before the change, so the whole view is redrawn instead.
                if (_redrawFirst >= 0)
                {
                    _redrawFirst = -1;
                    _redrawLast = -1;
                    _redraws++;
                    RedrawView();
                }
            }
        }

        // An edit up to the viewport's end moves lines across the pass's indexes; the view keeps its offset, so the
        // pass starts again from the viewport of the build that follows. The edit's first line keeps its index.
        private void RestartViewPortIfEditReachesIt(int editLine)
        {
            if (IsViewPortTokenizationPending && editLine <= _viewPortEnd)
            {
                _viewPortNext = -1;
                _viewPortRequest++;
                _viewPortRestart = true;
            }
        }

        private void SetInvalidRange(int startLine, int endLine)
        {
            if (!_document.IsInUpdate)
            {
                InvalidateLineRange(startLine, endLine);
                return;
            }

            // we're in a document change, store the max invalid range
            _invalidRange = _invalidRange?.Merge(startLine, endLine) ?? new InvalidLineRange(startLine, endLine);
        }

        private void DocumentOnUpdateFinished(object sender, EventArgs e)
        {
            _inDocumentChange = false;
            if (Volatile.Read(ref _isDisposed))
                return;

            if (_invalidRange != null)
            {
                try
                {
                    var range = _invalidRange.Value;
                    int startLine = Math.Clamp(range.StartLine, 0, _documentSnapshot.LineCount - 1);
                    int endLine = Math.Clamp(range.EndLine, 0, _documentSnapshot.LineCount - 1);
                    InvalidateLineRange(startLine, endLine);
                }
                finally
                {
                    _invalidRange = null;
                }
            }

            // A build during the update left the restart and may have used up the view's invalidation from the change,
            // so the restart asks for a build after the update. Last, as the view can throw.
            if (_viewPortRestart)
                _textView.InvalidateMeasure();
        }

        internal void TokenizeViewPort()
        {
            // Post is fire-and-forget - avoids the Task allocation that
            // InvokeAsync incurs. DispatcherPriority.Default is the default
            // for Post, matching the original InvokeAsync behavior
            Dispatcher.UIThread.Post(TokenizeViewPortCore, DispatcherPriority.Default);
        }

        /// <summary>
        /// Tokenizes the visible lines that still need tokens, in time-bounded slices on the UI thread.
        /// </summary>
        /// <remarks>
        /// UI thread only. The first slice runs before this returns, the rest at
        /// <see cref="DispatcherPriority.Background"/>; a newer viewport replaces the remaining range. Exceptions go
        /// to the exception handler and end the pass.</remarks>
        internal void TokenizeViewPortCore()
        {
            if (Volatile.Read(ref _isDisposed))
                return;

            int first;
            int last;
            try
            {
                if (!_textView.VisualLinesValid ||
                    _textView.VisualLines.Count == 0)
                    return;

                first = _textView.VisualLines[0].FirstDocumentLine.LineNumber - 1;
                last = _textView.VisualLines[^1].LastDocumentLine.LineNumber - 1;
            }
            catch (Exception ex)
            {
                _exceptionHandler?.Invoke(ex);
                return;
            }

            // After the read, as it invalidates VisualLines; outside the catch, as it reports its own exceptions. The
            // view can scroll, pump the dispatcher or dispose the model while it redraws; a newer request started there
            // keeps its range.
            int request = _viewPortRequest;
            RedrawTokenizedLines();
            if (request != _viewPortRequest || Volatile.Read(ref _isDisposed))
                return;

            _viewPortNext = first;
            _viewPortEnd = last;
            _viewPortRequest++;
            TokenizeViewPortSlice();
        }

        private void TokenizeViewPortSlice()
        {
            // A listener can scroll and pump the dispatcher during ForceTokenization; a newer request then takes over.
            int request = _viewPortRequest;
            try
            {
                long start = Stopwatch.GetTimestamp();
                while (IsViewPortTokenizationPending &&
                       Stopwatch.GetElapsedTime(start).TotalMilliseconds < ViewPortSliceBudgetMs)
                {
                    // Needs TextMateSharp 2.0.5: before it, a line could look valid with stale tokens. A line whose
                    // start state TMModel's thread just moved still looks valid until that thread reaches it.
                    if (IsTokenized(_viewPortNext))
                    {
                        _viewPortNext++;
                        continue;
                    }

                    int runEnd = _viewPortNext;
                    while (runEnd < _viewPortEnd && runEnd - _viewPortNext + 1 < ViewPortRunLines && !IsTokenized(runEnd + 1))
                        runEnd++;

                    // A slice inside a run of the same request can leave the pass above or below lines already held, so
                    // they only grow until redrawn: a run that leaves its events to them always lies inside them.
                    if (_redrawFirst < 0 || _viewPortNext < _redrawFirst)
                        _redrawFirst = _viewPortNext;
                    _redrawLast = Math.Max(_redrawLast, runEnd);

                    int outerFirst = _runFirst;
                    int outerLast = _runLast;
                    int outerRedraws = _runRedraws;
                    _runFirst = _viewPortNext;
                    _runLast = runEnd;
                    _runRedraws = _redraws;
                    try
                    {
                        ForceTokenization(_viewPortNext, runEnd);
                    }
                    finally
                    {
                        _runFirst = outerFirst;
                        _runLast = outerLast;
                        _runRedraws = outerRedraws;
                    }

                    // The same request with no next line: a slice that failed inside this run ended the pass.
                    if (request != _viewPortRequest || _viewPortNext < 0 || Volatile.Read(ref _isDisposed))
                        return;

                    _viewPortNext = runEnd + 1;
                }
            }
            catch (Exception ex)
            {
                if (request == _viewPortRequest)
                    _viewPortNext = -1;
                try
                {
                    _exceptionHandler?.Invoke(ex);
                }
                finally
                {
                    // Also when the handler rethrows: the coloring transformer left the runs' lines to this redraw.
                    EndViewPortSlice();
                }
                return;
            }

            EndViewPortSlice();
        }

        private void EndViewPortSlice()
        {
            if (!IsViewPortTokenizationPending)
            {
                RedrawTokenizedLines();
            }
            else if (!_viewPortSlicePosted)
            {
                _viewPortSlicePosted = true;
                // Background, below input and rendering: at Default an input job waits behind the whole pass.
                Dispatcher.UIThread.Post(RunPostedViewPortSlice, DispatcherPriority.Background);
            }
        }

        private void RunPostedViewPortSlice()
        {
            _viewPortSlicePosted = false;
            if (!Volatile.Read(ref _isDisposed))
                TokenizeViewPortSlice();
        }

        // Not clipped to VisualLines: a redraw leaves them invalid while the next layout still reuses every line it
        // did not dispose. Redraw rebuilds only built lines in the range, so lines off screen cost nothing.
        private void RedrawTokenizedLines()
        {
            if (_redrawFirst < 0)
                return;

            int first = _redrawFirst;
            int last = _redrawLast;
            _redrawFirst = -1;
            _redrawLast = -1;
            _redraws++;

            if (Volatile.Read(ref _isDisposed))
                return;

            if (_buildingVisualLines)
            {
                RedrawView();
                return;
            }

            try
            {
                int lineCount = _document.LineCount;
                if (first >= lineCount)
                    return;

                var firstLine = _document.GetLineByNumber(first + 1);
                var lastLine = _document.GetLineByNumber(Math.Min(last, lineCount - 1) + 1);
                _textView.Redraw(firstLine.Offset, lastLine.Offset + lastLine.TotalLength - firstLine.Offset);
            }
            catch (Exception ex)
            {
                _exceptionHandler?.Invoke(ex);
            }
        }

        // Reads outside TMModel's lock, which it does not expose; a line TMModel's thread stores just after this
        // read is tokenized twice or left to the next pass.
        private bool IsTokenized(int lineIndex)
        {
            var line = Get(lineIndex);
            return line != null && line.Tokens != null && !line.IsInvalid;
        }

        /// <summary>
        /// Throws <see cref="ObjectDisposedException"/> if this instance has been disposed.
        /// Uses <see cref="Volatile.Read(ref readonly bool)"/> for a lock-free memory-barrier-safe
        /// read paired with <see cref="Volatile.Write(ref bool, bool)"/> in
        /// <see cref="Dispose"/>.
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (Volatile.Read(ref _isDisposed))
                throw new ObjectDisposedException(nameof(TextEditorModel));
        }

        /// <summary>
        /// Represents a range of line numbers that is considered invalid within a given context.
        /// NOTE: this is a struct rather than a class to avoid heap allocations when tracking
        /// invalid ranges during document updates, which can occur frequently and may involve multiple ranges.
        /// </summary>
        /// <remarks>This struct is intended for internal use to track and manipulate invalid line ranges,
        /// such as those encountered during parsing or validation operations. The range is inclusive of both the start
        /// and end lines.</remarks>
        internal readonly struct InvalidLineRange
        {
            internal int StartLine { get; }
            internal int EndLine { get; }

            internal InvalidLineRange(int startLine, int endLine)
            {
                StartLine = startLine;
                EndLine = endLine;
            }

            internal InvalidLineRange Merge(int startLine, int endLine)
            {
                return new InvalidLineRange(
                    Math.Min(startLine, StartLine),
                    Math.Max(endLine, EndLine));
            }
        }
    }
}
