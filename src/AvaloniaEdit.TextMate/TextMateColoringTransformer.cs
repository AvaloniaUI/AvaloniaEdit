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
    public class TextMateColoringTransformer :
        GenericLineTransformer,
        IModelTokensChangedListener,
        ForegroundTextTransformation.IColorMap
    {
        private Theme _theme;
        private IGrammar _grammar;
        private TMModel _model;
        private TextDocument _document;
        private TextView _textView;

        private readonly Dictionary<int, IBrush> _brushes;
        private TextSegmentCollection<TextTransformation> _transformations;

        public TextMateColoringTransformer()
        {
            _brushes = new Dictionary<int, IBrush>();
        }

        public void SetModel(TextDocument document, TextView textView, TMModel model)
        {
            _document = document;
            _model = model;
            _textView = textView;

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

            _transformations?.Clear();
        }

        public void SetGrammar(IGrammar grammar)
        {
            _grammar = grammar;
            _transformations?.Clear();

            if (_model != null)
            {
                _model.SetGrammar(grammar);
            }
        }

        bool ForegroundTextTransformation.IColorMap.Contains(int foregroundColor)
        {
            if (_brushes == null)
                return false;

            return _brushes.ContainsKey(foregroundColor);
        }

        IBrush ForegroundTextTransformation.IColorMap.GetForegroundBrush(int foregroundColor)
        {
            return _brushes[foregroundColor];
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

                if (startIndex >= endIndex || token.Scopes == null || token.Scopes.Count == 0)
                {
                    continue;
                }

                var lineOffset = _document.GetLineByNumber(lineNumber).Offset;

                foreach (var themeRule in _theme.Match(token.Scopes))
                {
                    if (themeRule.foreground > 0 && _brushes.ContainsKey(themeRule.foreground))
                    {
                        _transformations.Add(new ForegroundTextTransformation(this, lineOffset + startIndex,
                            lineOffset + endIndex, themeRule.foreground));

                        break;
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

                if (tokens == null)
                    continue;

                RemoveLineTransformations(i);
                ProcessTokens(i, tokens);
            }
        }

        public void ModelTokensChanged(ModelTokensChangedEvent e)
        {
            var ranges = e.ranges.ToArray();

            Dispatcher.UIThread.Post(() =>
            {
                if (_model.IsStopped)
                    return;

                foreach (var range in ranges)
                {
                    if (!IsValidRange(range, _document.LineCount))
                        continue;

                    ProcessRange(range);

                    var startLine = _document.GetLineByNumber(range.fromLineNumber);
                    var endLine = _document.GetLineByNumber(range.toLineNumber);

                    _textView.Redraw(startLine.Offset, endLine.EndOffset - startLine.Offset);
                }
            });
        }

        static bool IsValidRange(Range range, int lineCount)
        {
            if (range.fromLineNumber < 0 || range.fromLineNumber > lineCount)
                return false;

            if (range.toLineNumber < 0 || range.toLineNumber > lineCount)
                return false;

            return true;
        }
    }
}