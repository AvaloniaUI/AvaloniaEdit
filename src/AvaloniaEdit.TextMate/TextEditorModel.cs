using System;

using Avalonia.Threading;

using AvaloniaEdit.Document;

using TextMateSharp.Model;

namespace AvaloniaEdit.TextMate
{
    class TextEditorModel : AbstractLineList
    {
        private object _lock = new object();
        private readonly TextDocument _document;
        private readonly TextEditor _editor;
        private int _lineCount;

        public TextEditorModel(TextEditor editor, TextDocument document, Action<Exception> exceptionHandler)
        {
            _editor = editor;
            _document = document;
            _exceptionHandler = exceptionHandler;

            _lineCount = _document.LineCount;

            for (int i = 0; i < _document.LineCount; i++)
                AddLine(i);

            _document.Changing += DocumentOnChanging;
            _document.Changed += DocumentOnChanged;
            _document.LineCountChanged += DocumentOnLineCountChanged;
            _editor.TextArea.TextView.ScrollOffsetChanged += TextView_ScrollOffsetChanged;
        }

        public override void Dispose()
        {
            _document.Changing -= DocumentOnChanging;
            _document.Changed -= DocumentOnChanged;
            _document.LineCountChanged -= DocumentOnLineCountChanged;
            _editor.TextArea.TextView.ScrollOffsetChanged -= TextView_ScrollOffsetChanged;
        }

        public void TokenizeViewPort()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!_editor.TextArea.TextView.VisualLinesValid ||
                    _editor.TextArea.TextView.VisualLines.Count == 0)
                    return;

                ForceTokenization(
                    _editor.TextArea.TextView.VisualLines[0].FirstDocumentLine.LineNumber - 1,
                    _editor.TextArea.TextView.VisualLines[_editor.TextArea.TextView.VisualLines.Count - 1].LastDocumentLine.LineNumber - 1);
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

                InvalidateLine(startLine);
            }
            catch (Exception ex)
            {
                _exceptionHandler?.Invoke(ex);
            }
        }

        public override void UpdateLine(int lineIndex)
        {
            // No op
        }

        public override int GetNumberOfLines() => _lineCount;

        public override string GetLineText(int lineIndex)
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                return _document.GetText(_document.Lines[lineIndex]);
            }

            return Dispatcher.UIThread.InvokeAsync(() => { return _document.GetText(_document.Lines[lineIndex]); })
                .GetAwaiter().GetResult();
        }

        public override int GetLineLength(int lineIndex)
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                return _document.Lines[lineIndex].Length;
            }

            return Dispatcher.UIThread.InvokeAsync(() =>
            {
                return _document.Lines[lineIndex].Length;
            }).GetAwaiter().GetResult();
        }

        Action<Exception> _exceptionHandler;
    }
}