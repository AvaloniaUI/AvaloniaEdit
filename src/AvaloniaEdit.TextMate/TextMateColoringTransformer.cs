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
        private TextDocument _document;

        private readonly Dictionary<int, IBrush> _brushes;
        private TextSegmentCollection<TextTransformation> _transformations;

        public TextMateColoringTransformer()
        {
            _brushes = new Dictionary<int, IBrush>();
        }

        public void SetModel(TextDocument document, TMModel model)
        {
            _document = document;
            _model = model;

            _transformations = new TextSegmentCollection<TextTransformation>(_document);

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
            if (_transformations is { })
            {
                var transformsInLine = _transformations.FindOverlappingSegments(line);

                foreach (var transform in transformsInLine)
                {
                    transform.Transform(this, line);
                }
            }
        }

        private void ProcessTokens(int lineNumber, List<TMToken> tokens)
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                var nextToken = (i + 1) < tokens.Count ? tokens[i + 1] : null;

                var startIndex = token.StartIndex;
                var endIndex = nextToken?.StartIndex ?? _model.GetLines().GetLineLength(lineNumber - 1);

                if (startIndex == endIndex || token.type == string.Empty)
                {
                    continue;
                }

                var themeRules = _theme.Match(token.type);

                var lineOffset = _document.GetLineByNumber(lineNumber).Offset;

                foreach (var themeRule in themeRules)
                {
                    if (themeRule.foreground > 0 && _brushes.ContainsKey(themeRule.foreground))
                    {
                        _transformations.Add(new ForegroundTextTransformation(_brushes, lineOffset + startIndex,
                            lineOffset + endIndex, themeRule.foreground));
                    }
                }
            }
        }

        private void RemoveLineTransformations(int lineNumber)
        {
            var line = _document.GetLineByNumber(lineNumber);
            var transformsInLine = _transformations.FindOverlappingSegments(line);

            foreach (var transform in transformsInLine)
            {
                _transformations.Remove(transform);
            }
        }

        private void ProcessRange(Range range)
        {
            
            
            for (int i = range.fromLineNumber; i <= range.toLineNumber; i++)
            {
                var tokens = _model.GetLineTokens(i - 1);

                if (tokens is { })
                {
                    RemoveLineTransformations(i);
                    ProcessTokens(i, tokens);
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