using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using TextMateSharp.Grammars;
using TextMateSharp.Themes;

namespace AvaloniaEdit.TextMate
{
    public static class TextMate
    {
        public static void InstallTextMate(this TextEditor editor, Theme theme, IGrammar grammar)
        {
            editor.InstallTheme(theme);
            editor.InstallGrammar(grammar);
        }

        public static void ConnectTextMate(this TextEditor editor)
        {
            editor.DocumentChanged += EditorOnDocumentChanged;
            
            EditorOnDocumentChanged(editor, EventArgs.Empty);
        }
        
        public static void InstallGrammar(this TextEditor editor, IGrammar grammar)
        {
            var transformer = editor.GetOrCreateTransformer();

            transformer.SetGrammer(grammar);
        }

        private static void EditorOnDocumentChanged(object sender, EventArgs e)
        {
            var editor = sender as TextEditor;

            if (editor is { })
            {
                var transformer = editor.GetOrCreateTransformer();
                
                if(editor.Document is { })
                {
                    transformer.ConnectDocument(editor.Document);
                }
                else
                {
                    transformer.DisconnectDocument();
                }
            }
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
                transformer = new TextMateColoringTransformer();
                editor.TextArea.TextView.LineTransformers.Add(transformer);
            }

            return transformer;
        }
    }
    
    public class TextMateColoringTransformer : GenericLineTransformer
    {
        private TextDocument _document;
        private Theme _theme;
        private IGrammar _grammar;

        public void ConnectDocument(TextDocument document)
        {
            DisconnectDocument();

            _document = document;

            TextTransformations = new TextSegmentCollection<TextTransformation>(document);
        }

        public void SetGrammer(IGrammar grammar)
        {
            _grammar = grammar;
        }

        public void SetTheme(Theme theme)
        {
            _theme = theme;
        }

        public TextSegmentCollection<TextTransformation> TextTransformations { get; private set; }

        public void DisconnectDocument()
        {
            if (_document is { })
            {
                TextTransformations.Disconnect(_document);
                TextTransformations.Clear();
                TextTransformations = null;
            }
        }

        protected override void TransformLine(DocumentLine line, ITextRunConstructionContext context)
        {
            if (_document is { })
            {
                var tokens = _grammar.TokenizeLine(_document.GetText(line)).GetTokens();
                
                foreach(var token in tokens)
                {
                    int startIndex = (token.StartIndex > line.Length) ? line.Length : token.StartIndex;
                    int endIndex = (token.EndIndex > line.Length) ? line.Length : token.EndIndex;

                    if (startIndex == endIndex)
                        continue;
                    
                    foreach (string scopeName in token.Scopes)
                    {
                        List<ThemeTrieElementRule> themeRules = _theme.Match(scopeName);

                        foreach (var themeRule in themeRules)
                        {
                            var background = _theme.GetColor(themeRule.background);
                            var foreground = _theme.GetColor(themeRule.foreground);
                            var fontStyle = themeRule.fontStyle;

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
}