using System;

using Avalonia.Threading;

using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;

using TextMateSharp.Model;

#if NET6_0_OR_GREATER
using Math = System.Math;
#else
using Math = AvaloniaEdit.TextMate.Compatibility.Math;
#endif

namespace AvaloniaEdit.TextMate
{
    public class TextEditorModel : AbstractLineList, IDisposable
    {
        private readonly TextDocument _document;
        private readonly TextView _textView;
        private DocumentSnapshot _documentSnapshot;
        private Action<Exception> _exceptionHandler;
        private InvalidLineRange _invalidRange;

        public DocumentSnapshot DocumentSnapshot { get { return _documentSnapshot; } }
        internal InvalidLineRange InvalidRange { get { return _invalidRange; } }

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
        }

        public override void Dispose()
        {
            _document.Changing -= DocumentOnChanging;
            _document.Changed -= DocumentOnChanged;
            _document.UpdateFinished -= DocumentOnUpdateFinished;
            _textView.ScrollOffsetChanged -= TextView_ScrollOffsetChanged;
        }

        public override void UpdateLine(int lineIndex) { }

        public void InvalidateViewPortLines()
        {
            if (!_textView.VisualLinesValid ||
                _textView.VisualLines.Count == 0)
                return;

            InvalidateLineRange(
                _textView.VisualLines[0].FirstDocumentLine.LineNumber - 1,
                _textView.VisualLines[_textView.VisualLines.Count - 1].LastDocumentLine.LineNumber - 1);
        }

        public override int GetNumberOfLines()
        {
            return _documentSnapshot.LineCount;
        }

        public override string GetLineText(int lineIndex)
        {
            return _documentSnapshot.GetLineText(lineIndex);
        }

        public override int GetLineLength(int lineIndex)
        {
            return _documentSnapshot.GetLineLength(lineIndex);
        }

        private void TextView_ScrollOffsetChanged(object sender, EventArgs e)
        {
            try
            {
                TokenizeViewPort();
            }
            catch (Exception ex)
            {
                _exceptionHandler?.Invoke(ex);
            }
        }

        private void DocumentOnChanging(object sender, DocumentChangeEventArgs e)
        {
            try
            {
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
            try
            {
                int startLine = _document.GetLineByOffset(e.Offset).LineNumber - 1;
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
        }

        private void SetInvalidRange(int startLine, int endLine)
        {
            if (!_document.IsInUpdate)
            {
                InvalidateLineRange(startLine, endLine);
                return;
            }

            // we're in a document change, store the max invalid range
            if (_invalidRange == null)
            {
                _invalidRange = new InvalidLineRange(startLine, endLine);
                return;
            }

            _invalidRange.SetInvalidRange(startLine, endLine);
        }

        void DocumentOnUpdateFinished(object sender, EventArgs e)
        {
            if (_invalidRange == null)
                return;

            try
            {
                int startLine = Math.Clamp(_invalidRange.StartLine, 0, _documentSnapshot.LineCount - 1);
                int endLine = Math.Clamp(_invalidRange.EndLine, 0, _documentSnapshot.LineCount - 1);
                InvalidateLineRange(startLine, endLine);
            }
            finally
            {
                _invalidRange = null;
            }
        }

        private void TokenizeViewPort()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!_textView.VisualLinesValid ||
                    _textView.VisualLines.Count == 0)
                    return;

                ForceTokenization(
                    _textView.VisualLines[0].FirstDocumentLine.LineNumber - 1,
                    _textView.VisualLines[_textView.VisualLines.Count - 1].LastDocumentLine.LineNumber - 1);
            }, DispatcherPriority.Default);
        }

        internal class InvalidLineRange
        {
            internal int StartLine { get; private set; }
            internal int EndLine { get; private set; }

            internal InvalidLineRange(int startLine, int endLine)
            {
                StartLine = startLine;
                EndLine = endLine;
            }

            internal void SetInvalidRange(int startLine, int endLine)
            {
                if (startLine < StartLine)
                    StartLine = startLine;

                if (endLine > EndLine)
                    EndLine = endLine;
            }
        }
    }
}