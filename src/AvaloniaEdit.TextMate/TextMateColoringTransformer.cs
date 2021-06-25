using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using TextMateSharp.Grammars;
using TextMateSharp.Model;
using TextMateSharp.Themes;

namespace AvaloniaEdit.TextMate
{
    class TextEditorModel : AbstractLineList, IModelTokensChangedListener
    {
        private object _lock = new object();
        private TextEditor _editor;
        private TextDocument _document;
        private int _lineCount;

        public TextEditorModel(TextEditor editor)
        {
            _editor = editor;
            _editor.DocumentChanged += EditorOnDocumentChanged;
            
            EditorOnDocumentChanged(editor, EventArgs.Empty);
        }

        private void EditorOnDocumentChanged(object? sender, EventArgs e)
        {
            if (_document is { })
            {
                _document.Changing -= DocumentOnChanging;
                _document.Changed -= DocumentOnChanged;
                _document.LineCountChanged -= DocumentOnLineCountChanged;
            }
            
            _document = _editor.Document;
            _lineCount = _document.LineCount;
            
            _document.Changing +=  DocumentOnChanging;
            _document.Changed += DocumentOnChanged;
            _document.LineCountChanged += DocumentOnLineCountChanged;
            
            for (int i = 0; i < _document.LineCount; i++)
            {
                AddLine(i);
            }
        }

        private void DocumentOnLineCountChanged(object? sender, EventArgs e)
        {
            lock (_lock)
            {
                _lineCount = _document.LineCount;
            }
        }

        private void DocumentOnChanging(object? sender, DocumentChangeEventArgs e)
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

        private void DocumentOnChanged(object? sender, DocumentChangeEventArgs e)
        {
            int startLine = _document.GetLineByOffset(e.Offset).LineNumber - 1;

            if (e.InsertedText is { })
            {
                int endLine = _document.GetLineByOffset(e.Offset + e.InsertionLength).LineNumber - 1;

                for (int i = startLine; i < endLine; i++)
                {
                    AddLine(i + 1);
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

        public override void UpdateLine(int lineIndex)
        {
            // No op
        }

        public override int GetNumberOfLines() => _lineCount;

        public override string GetLineText(int lineIndex)
        {
            return Dispatcher.UIThread.InvokeAsync(() =>
            {
                return _document.GetText(_document.Lines[lineIndex]);
            }).GetAwaiter().GetResult();
        }

        public override int GetLineLength(int lineIndex)
        {
            return Dispatcher.UIThread.InvokeAsync(() =>
            {
                return _document.Lines[lineIndex].Length;
            }).GetAwaiter().GetResult();
        }

        public override void Dispose()
        {
            // todo implement dispose.
        }

        public void ModelTokensChanged(ModelTokensChangedEvent e)
        {
            Dispatcher.UIThread.Post(() =>
            {
                foreach (var range in e.ranges)
                {
                    var startLine = _editor.Document.GetLineByNumber(range.fromLineNumber);
                    var endLine = _editor.Document.GetLineByNumber(range.toLineNumber);

                    _editor.TextArea.TextView.Redraw(startLine.Offset, endLine.EndOffset - startLine.Offset);
                }
            });
        }
    }
    
    public static class TextMate
    {
        public static void InstallTextMate(this TextEditor editor, Theme theme, IGrammar grammar)
        {
            editor.InstallTheme(theme);
            editor.InstallGrammar(grammar);
        }
        
        public static void InstallGrammar(this TextEditor editor, IGrammar grammar)
        {
            var transformer = editor.GetOrCreateTransformer();
            
            transformer.SetGrammar(grammar);
        }

        public static void InstallTheme(this TextEditor editor, Theme theme)
        {
            var transformer = editor.GetOrCreateTransformer();
            
            transformer.SetTheme(theme);
            
            editor.InvalidateVisual();
        }

        private static TextMateColoringTransformer GetOrCreateTransformer(this TextEditor editor)
        {
            var transformer = editor.TextArea.TextView.LineTransformers.OfType<TextMateColoringTransformer>().FirstOrDefault();

            if (transformer is null)
            {
                var editorModel = new TextEditorModel(editor);
                var model = new TMModel(editorModel);
                
                transformer = new TextMateColoringTransformer(model);
                model.AddModelTokensChangedListener(editorModel);
                
                editor.TextArea.TextView.LineTransformers.Add(transformer);
            }

            return transformer;
        }
    }
    
    public class TextMateColoringTransformer : GenericLineTransformer
    {
        private Theme _theme;
        private readonly TMModel _model;

        public TextMateColoringTransformer(TMModel model)
        {
            _model = model;
        }

        public void SetTheme(Theme theme)
        {
            _theme = theme;
        }

        public void SetGrammar(IGrammar grammar)
        {
            _model.SetGrammar(grammar);
        }

        protected override void TransformLine(DocumentLine line, ITextRunConstructionContext context)
        {
            var tokens = _model.GetLineTokens(line.LineNumber - 1);

            if (tokens is { })
            {
                for (int i = 0; i < tokens.Count; i++)
                {
                    var token = tokens[i];
                    var nextToken = (i + 1) < tokens.Count ? tokens[i + 1] : null;

                    var startIndex = token.StartIndex;
                    var endIndex = nextToken?.StartIndex ?? line.Length;

                    if (startIndex == endIndex)
                    {
                        continue;
                    }

                    var themeRules = _theme.Match(token.type);

                    foreach (var themeRule in themeRules)
                    {
                        var foreground = _theme.GetColor(themeRule.foreground);

                        if (foreground != null)
                        {
                            SetTextStyle(line, startIndex, endIndex - startIndex, SolidColorBrush.Parse(foreground));
                        }
                    }
                }
            }
        }
    }
}