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

        private volatile int _firstVisibleLine = -1;
        private volatile int _lastVisibleLine = -1;

        private readonly Dictionary<int, IBrush> _brushes;
        private TextSegmentCollection<TextTransformation> _transformations;

        public TextMateColoringTransformer(TextView textView)
        {
            _textView = textView;
            _brushes = new Dictionary<int, IBrush>();
            _textView.VisualLinesChanged += TextView_VisualLinesChanged;
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

        private void TextView_VisualLinesChanged(object sender, EventArgs e)
        {
            if (!_textView.VisualLinesValid || _textView.VisualLines.Count == 0)
                return;

            _firstVisibleLine = _textView.VisualLines[0].FirstDocumentLine.LineNumber - 1;
            _lastVisibleLine = _textView.VisualLines[_textView.VisualLines.Count - 1].LastDocumentLine.LineNumber - 1;
        }

        public void Dispose()
        {
            if (_textView != null)
                _textView.VisualLinesChanged -= TextView_VisualLinesChanged;
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
            int i = line.LineNumber;

            var tokens = _model.GetLineTokens(i - 1);

            if (tokens == null)
                return;

            RemoveLineTransformations(i);
            ProcessTokens(i, tokens);

            var transformsInLine = _transformations.FindOverlappingSegments(line);

            foreach (var transform in transformsInLine)
            {
                transform.Transform(this, line);
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

        public void ModelTokensChanged(ModelTokensChangedEvent e)
        {
            if (e.ranges == null)
                return;

            if (_model.IsStopped)
                return;

            bool redraw = false;

            foreach (var range in e.ranges)
            {
                if (range.fromLineNumber >= _firstVisibleLine || range.toLineNumber >= _firstVisibleLine ||
                   range.fromLineNumber <= _lastVisibleLine || range.toLineNumber <= _lastVisibleLine)
                {
                    redraw = true;
                    break;
                }
            }

            if (!redraw)
                return;

            Dispatcher.UIThread.Post(() =>
            {
                _textView.Redraw();
            }, DispatcherPriority.Render - 1);
        }
    }
}