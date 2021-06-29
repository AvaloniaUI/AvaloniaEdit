using System;
using System.Collections.Generic;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using TextMateSharp.Grammars;
using TextMateSharp.Model;
using TextMateSharp.Themes;

namespace AvaloniaEdit.TextMate
{
    public class TextMateColoringTransformer : GenericLineTransformer, IModelTokensChangedListener
    {
        private Theme _theme;
        private IGrammar _grammar;
        private TMModel _model;

        private Dictionary<int, IBrush> _brushes;
        private TextSegmentCollection<TextTransformation> _transformations;
        private TextDocument _document;

        public TextMateColoringTransformer()
        {
            _brushes = new Dictionary<int, IBrush>();
        }

        public void SetModel(TMModel model)
        {
            _model = model;

            if (_grammar != null)
            {
                _model.SetGrammar(_grammar);
            }
        }

        public void SetTheme(Theme theme)
        {
            _theme = theme;

            _brushes.Clear();

            var map = _theme.GetColorMap();

            foreach (var color in map)
            {
                var id = _theme.GetColorId(color);

                _brushes[id] = SolidColorBrush.Parse(color);
            }
        }

        public void SetGrammar(IGrammar grammar)
        {
            _grammar = grammar;

            if (_model != null)
            {
                _model.SetGrammar(grammar);
            }
        }

        protected override void TransformLine(DocumentLine line, ITextRunConstructionContext context)
        {
            if (_model is { })
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

                        if (startIndex == endIndex || token.type == string.Empty)
                        {
                            continue;
                        }

                        var themeRules = _theme.Match(token.type);

                        foreach (var themeRule in themeRules)
                        {
                            if (themeRule.foreground > 0 && _brushes.ContainsKey(themeRule.foreground))
                            {
                                SetTextStyle(line, startIndex, endIndex - startIndex, _brushes[themeRule.foreground]);
                            }
                        }
                    }
                }
            }
        }

        public void ModelTokensChanged(ModelTokensChangedEvent e)
        {
            var ranges = e.ranges.ToArray();

            Dispatcher.UIThread.Post(() =>
            {
                foreach (var range in ranges)
                {
                    ProcessRange(range);
                }
            });
        }
    }
}
