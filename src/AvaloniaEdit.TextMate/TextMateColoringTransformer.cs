using System;
using System.Buffers;
using System.Collections.Generic;

using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Threading;

using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using TextMateSharp.Grammars;
using TextMateSharp.Model;
using TextMateSharp.Themes;
using FontStyle = TextMateSharp.Themes.FontStyle;

namespace AvaloniaEdit.TextMate
{
    public class TextMateColoringTransformer :
        GenericLineTransformer,
        IModelTokensChangedListener
    {
        private Theme _theme;
        private IGrammar _grammar;
        private TMModel _model;
        private TextDocument _document;
        private TextView _textView;
        private Action<Exception> _exceptionHandler;

        private volatile bool _areVisualLinesValid = false;
        private volatile int _firstVisibleLineIndex = -1;
        private volatile int _lastVisibleLineIndex = -1;

        private readonly Dictionary<int, IBrush> _brushes;

        public TextMateColoringTransformer(
            TextView textView,
            Action<Exception> exceptionHandler)
            : base(exceptionHandler)
        {
            _textView = textView;
            _exceptionHandler = exceptionHandler;

            _brushes = new Dictionary<int, IBrush>();
            _textView.VisualLinesChanged += TextView_VisualLinesChanged;
        }

        public void SetModel(TextDocument document, TMModel model)
        {
            _areVisualLinesValid = false;
            _document = document;
            _model = model;

            if (_grammar != null)
            {
                _model.SetGrammar(_grammar);
            }
        }

        private void TextView_VisualLinesChanged(object sender, EventArgs e)
        {
            try
            {
                if (!_textView.VisualLinesValid || _textView.VisualLines.Count == 0)
                    return;

                _areVisualLinesValid = true;
                _firstVisibleLineIndex = _textView.VisualLines[0].FirstDocumentLine.LineNumber - 1;
                _lastVisibleLineIndex = _textView.VisualLines[_textView.VisualLines.Count - 1].LastDocumentLine.LineNumber - 1;
            }
            catch (Exception ex)
            {
                _exceptionHandler?.Invoke(ex);
            }
        }

        public void Dispose()
        {
            _textView.VisualLinesChanged -= TextView_VisualLinesChanged;
            _brushes.Clear();
        }

        public void SetTheme(Theme theme)
        {
            _theme = theme;

            _brushes.Clear();

            var map = _theme.GetColorMap();

            foreach (var color in map)
            {
                var id = _theme.GetColorId(color);

                _brushes[id] = new ImmutableSolidColorBrush(Color.Parse(NormalizeColor(color)));
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
            try
            {
                if (_model == null)
                    return;

                int lineNumber = line.LineNumber;

                var tokens = _model.GetLineTokens(lineNumber - 1);

                if (tokens == null)
                    return;

                var transformsInLine = ArrayPool<ForegroundTextTransformation>.Shared.Rent(tokens.Count);

                try
                {
                    GetLineTransformations(lineNumber, tokens, transformsInLine);

                    for (int i = 0; i < tokens.Count; i++)
                    {
                        if (transformsInLine[i] == null)
                            continue;

                        transformsInLine[i].Transform(this, line);
                    }
                }
                finally
                {
                    ArrayPool<ForegroundTextTransformation>.Shared.Return(transformsInLine);
                }
            }
            catch (Exception ex)
            {
                _exceptionHandler?.Invoke(ex);
            }
        }

        private void GetLineTransformations(int lineNumber, List<TMToken> tokens, ForegroundTextTransformation[] transformations)
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                var nextToken = (i + 1) < tokens.Count ? tokens[i + 1] : null;

                var startIndex = token.StartIndex;
                var endIndex = nextToken?.StartIndex ?? _model.GetLines().GetLineLength(lineNumber - 1);

                if (startIndex >= endIndex || token.Scopes == null || token.Scopes.Count == 0)
                {
                    transformations[i] = null;
                    continue;
                }

                var lineOffset = _document.GetLineByNumber(lineNumber).Offset;

                int foreground = 0;
                int background = 0;
                FontStyle fontStyle = 0;

                foreach (var themeRule in _theme.Match(token.Scopes))
                {
                    if (foreground == 0 && themeRule.foreground > 0)
                        foreground = themeRule.foreground;

                    if (background == 0 && themeRule.background > 0)
                        background = themeRule.background;

                    if (fontStyle == 0 && themeRule.fontStyle > 0)
                        fontStyle = themeRule.fontStyle;
                }

                if (transformations[i] == null)
                    transformations[i] = new ForegroundTextTransformation();

                transformations[i].ColorMap = _brushes;
                transformations[i].ExceptionHandler = _exceptionHandler;
                transformations[i].StartOffset = lineOffset + startIndex;
                transformations[i].EndOffset = lineOffset + endIndex;
                transformations[i].ForegroundColor = foreground;
                transformations[i].BackgroundColor = background;
                transformations[i].FontStyle = fontStyle;
            }
        }


        public void ModelTokensChanged(ModelTokensChangedEvent e)
        {
            if (e.Ranges == null)
                return;

            if (_model == null || _model.IsStopped)
                return;

            int firstChangedLineIndex = int.MaxValue;
            int lastChangedLineIndex = -1;

            foreach (var range in e.Ranges)
            {
                firstChangedLineIndex = Math.Min(range.FromLineNumber - 1, firstChangedLineIndex);
                lastChangedLineIndex = Math.Max(range.ToLineNumber - 1, lastChangedLineIndex);
            }

            if (_areVisualLinesValid)
            {
                bool changedLinesAreNotVisible =
                    ((firstChangedLineIndex < _firstVisibleLineIndex && lastChangedLineIndex < _firstVisibleLineIndex) ||
                    (firstChangedLineIndex > _lastVisibleLineIndex && lastChangedLineIndex > _lastVisibleLineIndex));

                if (changedLinesAreNotVisible)
                    return;
            }

            Dispatcher.UIThread.Post(() =>
            {
                int firstLineIndexToRedraw = Math.Max(firstChangedLineIndex, _firstVisibleLineIndex);
                int lastLineIndexToRedrawLine = Math.Min(lastChangedLineIndex, _lastVisibleLineIndex);

                int totalLines = _document.Lines.Count - 1;

                firstLineIndexToRedraw = Clamp(firstLineIndexToRedraw, 0, totalLines);
                lastLineIndexToRedrawLine = Clamp(lastLineIndexToRedrawLine, 0, totalLines);

                DocumentLine firstLineToRedraw = _document.Lines[firstLineIndexToRedraw];
                DocumentLine lastLineToRedraw = _document.Lines[lastLineIndexToRedrawLine];

                _textView.Redraw(
                    firstLineToRedraw.Offset,
                    (lastLineToRedraw.Offset + lastLineToRedraw.TotalLength) - firstLineToRedraw.Offset);
            });
        }

        static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        static string NormalizeColor(string color)
        {
            if (color.Length == 9)
            {
                Span<char> normalizedColor = stackalloc char[] { '#', color[7], color[8], color[1], color[2], color[3], color[4], color[5], color[6] };

                return normalizedColor.ToString();
            }

            return color;
        }
    }
}