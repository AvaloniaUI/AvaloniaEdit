using System;
using System.Collections.Generic;

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
        private List<LineRange> _lineRanges = new List<LineRange>();
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
            _document.LineCountChanged += DocumentOnLineCountChanged;
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
                _lineRanges.Clear();
                foreach (var line in _document.Lines)
                {
                    _lineRanges.Add(new LineRange()
                    {
                        Offset = line.Offset,
                        Length = line.TotalLength
                    });
                }
            }
        }

        public override void Dispose()
        {
            _document.Changing -= DocumentOnChanging;
            _document.Changed -= DocumentOnChanged;
            _document.LineCountChanged -= DocumentOnLineCountChanged;
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

        private void DocumentOnLineCountChanged(object sender, EventArgs e)
        {
            try
            {
                lock (_lock)
                {
                    _lineCount = _document.LineCount;
                }
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
                if (e.RemovedText is { })
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

                if (e.InsertedText is { })
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
                InvalidateLine(startLine);
            }
            catch (Exception ex)
            {
                _exceptionHandler?.Invoke(ex);
            }
        }

        public override void UpdateLine(int lineIndex)
        {
            lock(_lock)
            {
                var line = _document.Lines[lineIndex];
                _lineRanges[lineIndex] = new LineRange()
                {
                    Offset = line.Offset,
                    Length = line.TotalLength
                };
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