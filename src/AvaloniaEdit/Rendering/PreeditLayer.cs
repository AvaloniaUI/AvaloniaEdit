using System;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Utils;

namespace AvaloniaEdit.Rendering
{
    /// <summary>
    /// Renders IME preedit (composition) text at the caret position.
    /// The text is rendered with an underline to indicate it is being composed.
    /// </summary>
    internal sealed class PreeditLayer : Layer
    {
        private readonly TextArea _textArea;
        private string _preeditText;
        private Rect _caretRect;
        private IBrush _foreground;
        private int? _cursorOffset;

        public PreeditLayer(TextArea textArea) : base(textArea.TextView, KnownLayer.Caret)
        {
            _textArea = textArea;
            IsHitTestVisible = false;
        }

        public void SetPreedit(string text, Rect caretRect, IBrush foreground, int? cursorOffset = null)
        {
            _preeditText = text;
            _caretRect = caretRect;
            _foreground = foreground;
            _cursorOffset = cursorOffset;
            InvalidateVisual();
        }

        public void Clear()
        {
            if (_preeditText == null)
                return;
            _preeditText = null;
            _cursorOffset = null;
            InvalidateVisual();
        }

        public override void Render(DrawingContext drawingContext)
        {
            base.Render(drawingContext);

            if (string.IsNullOrEmpty(_preeditText))
                return;

            var textView = TextView;
            if (textView?.Document == null)
                return;

            // Position preedit text at the right edge of the caret, adjusted for scroll offset
            var x = _caretRect.Right - textView.HorizontalOffset;
            var y = _caretRect.Y - textView.VerticalOffset;

            var typeface = textView.CreateTypeface();
            var fontRenderingEmSize = textView.GetValue(TemplatedControl.FontSizeProperty);
            var foreground = _foreground
                ?? textView.GetValue(TemplatedControl.ForegroundProperty) as IBrush
                ?? Brushes.White;

            var textLayout = new TextLayout(
                _preeditText,
                typeface,
                fontRenderingEmSize,
                foreground,
                textWrapping: TextWrapping.NoWrap);

            var origin = new Point(x, y);

            textLayout.Draw(drawingContext, origin);

            // Draw underline to indicate composition text
            var width = textLayout.WidthIncludingTrailingWhitespace;
            var height = textLayout.Height;
            var pen = new ImmutablePen(foreground.ToImmutable(), 1);
            drawingContext.DrawLine(pen,
                new Point(origin.X, origin.Y + height - 1),
                new Point(origin.X + width, origin.Y + height - 1));

            // Draw cursor within preedit text: use cursorOffset if specified, otherwise at end
            var effectiveCursorOffset = _cursorOffset ?? _preeditText.Length;
            if (effectiveCursorOffset >= 0 && effectiveCursorOffset <= _preeditText.Length)
            {
                var prefixText = _preeditText.Substring(0, effectiveCursorOffset);
                var prefixLayout = new TextLayout(
                    prefixText,
                    typeface,
                    fontRenderingEmSize,
                    foreground,
                    textWrapping: TextWrapping.NoWrap);
                var cursorX = origin.X + prefixLayout.WidthIncludingTrailingWhitespace;
                var cursorPen = new ImmutablePen(foreground.ToImmutable(), 2);
                drawingContext.DrawLine(cursorPen,
                    new Point(cursorX, origin.Y),
                    new Point(cursorX, origin.Y + height));
            }
        }
    }
}
