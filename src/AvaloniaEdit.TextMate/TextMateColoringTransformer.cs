using System.Collections.Generic;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using TextMateSharp.Grammars;
using TextMateSharp.Model;
using TextMateSharp.Themes;

namespace AvaloniaEdit.TextMate
{
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