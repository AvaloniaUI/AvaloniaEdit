using System;
using System.Buffers;

using Avalonia.Threading;

using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;

using TextMateSharp.Model;

namespace AvaloniaEdit.TextMate
{
    public class TextEditorModel : AbstractLineList
    {
        private object _lock = new object();
        private readonly TextDocument _document;
        private readonly TextView _textView;
        private int _lineCount;
        private ITextSource _textSource;
        private LineRange[] _lineRanges;
        private Action<Exception> _exceptionHandler;

        public TextEditorModel(TextView textView, TextDocument document, Action<Exception> exceptionHandler)
        {
            _textView = textView;
            _document = document;
            _exceptionHandler = exceptionHandler;

            _lineCount = _document.LineCount;

            for (int i = 0; i < _lineCount; i++)
                AddLine(i);

            UpdateSnapshot();
            UpdateLineRanges();

            _document.Changing += DocumentOnChanging;
            _document.Changed += DocumentOnChanged;
            _textView.ScrollOffsetChanged += TextView_ScrollOffsetChanged;
        }

        private void UpdateSnapshot()
        {
            lock (_lock)
            {
                _textSource = _document.CreateSnapshot();
            }
        }

        private void UpdateLineRanges()
        {
            lock (_lock)
            {
                _lineCount = _document.Lines.Count;

                if (_lineRanges == null)
                    _lineRanges = new LineRange[_lineCount];

                Array.Resize(ref _lineRanges, _lineCount);

                for (int i = 0; i < _lineCount; i++)
                {
                    var line = _document.Lines[i];
                    _lineRanges[i].Offset = line.Offset;
                    _lineRanges[i].Length = line.Length;
                }
            }
        }

        public override void Dispose()
        {
            _document.Changing -= DocumentOnChanging;
            _document.Changed -= DocumentOnChanged;
            _textView.ScrollOffsetChanged -= TextView_ScrollOffsetChanged;
        }

        public void TokenizeViewPort()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!_textView.VisualLinesValid ||
                    _textView.VisualLines.Count == 0)
                    return;

                ForceTokenization(
                    _textView.VisualLines[0].FirstDocumentLine.LineNumber - 1,
                    _textView.VisualLines[_textView.VisualLines.Count - 1].LastDocumentLine.LineNumber - 1);
            }, DispatcherPriority.MinValue);
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

                if (e.InsertionLength > 0)
                {
                    int endLine = _document.GetLineByOffset(e.Offset + e.InsertionLength).LineNumber - 1;

                    for (int i = startLine; i < endLine; i++)
                    {
                        AddLine(i);
                    }

                    if (startLine == endLine)
                    {
                        UpdateLine(startLine);
                    }
                }
                else
                {
                    UpdateLine(startLine);
                }

                if (e.RemovalLength > 0 || e.InsertionLength > 0)
                {
                    UpdateLineRanges();
                }

                UpdateSnapshot();

                // invalidate current and previous line
                // some grammars (JSON, csharp, ...)
                // need to invalidate previous lines
                if (startLine - 1 >= 0)
                    InvalidateLine(startLine - 1);
                InvalidateLine(startLine);
            }
            catch (Exception ex)
            {
                _exceptionHandler?.Invoke(ex);
            }
        }

        public override void UpdateLine(int lineIndex)
        {
            lock (_lock)
            {
                var line = _document.Lines[lineIndex];
                _lineRanges[lineIndex].Offset = line.Offset;
                _lineRanges[lineIndex].Length = line.Length;
            }
        }

        public override int GetNumberOfLines()
        {
            lock (_lock)
            {
                return _lineCount;
            }
        }

        public override string GetLineText(int lineIndex)
        {
            lock (_lock)
            {
                var lineRange = _lineRanges[lineIndex];
                return _textSource.GetText(lineRange.Offset, lineRange.Length);
            }
        }

        public override int GetLineLength(int lineIndex)
        {
            lock (_lock)
            {
                return _lineRanges[lineIndex].Length;
            }
        }

        struct LineRange
        {
            public int Offset;
            public int Length;
        }
    }
}