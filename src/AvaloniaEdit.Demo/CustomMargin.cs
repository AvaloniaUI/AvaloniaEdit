using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;

#nullable enable

namespace AvaloniaEdit.Demo
{
    /// <summary>
    /// A custom, clickable margin.
    /// </summary>
    internal sealed class CustomMargin : AbstractMargin
    {
        private readonly IBrush _backgroundBrush = new ImmutableSolidColorBrush(Colors.DarkGray, 0.5f);
        private readonly IBrush _pointerOverBrush = new ImmutableSolidColorBrush(new Color(192, 80, 80, 80));
        private readonly IPen _pointerOverPen = new ImmutablePen(new ImmutableSolidColorBrush(new Color(192, 37, 37, 37)), 1);
        private readonly IBrush _markerBrush = new ImmutableSolidColorBrush(new Color(192, 196, 65, 74));
        private readonly IPen _markerPen = new ImmutablePen(new ImmutableSolidColorBrush(new Color(192, 186, 17, 28)), 1);

        private readonly List<int> _markedDocumentLines = [];
        private int _pointerOverLine = -1;

        public CustomMargin()
        {
            Cursor = new Cursor(StandardCursorType.Arrow);
        }

        protected override void OnTextViewChanged(TextView? oldTextView, TextView? newTextView)
        {
            if (oldTextView != null)
                oldTextView.VisualLinesChanged -= OnVisualLinesChanged;
            if (newTextView != null)
                newTextView.VisualLinesChanged += OnVisualLinesChanged;

            base.OnTextViewChanged(oldTextView, newTextView);
        }

        private void OnVisualLinesChanged(object? sender, EventArgs eventArgs)
        {
            InvalidateVisual();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(20, 0);
        }

        private int GetLineNumber(PointerEventArgs e)
        {
            double visualY = e.GetPosition(TextView).Y + TextView.VerticalOffset;
            VisualLine visualLine = TextView.GetVisualLineFromVisualTop(visualY);
            return visualLine.FirstDocumentLine.LineNumber;
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            _pointerOverLine = GetLineNumber(e);
            InvalidateVisual();

            base.OnPointerMoved(e);
        }

        protected override void OnPointerExited(PointerEventArgs e)
        {
            _pointerOverLine = -1;

            base.OnPointerExited(e);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            int line = _pointerOverLine = GetLineNumber(e);

            if (!_markedDocumentLines.Remove(line))
                _markedDocumentLines.Add(line);

            _markedDocumentLines.Sort();
            InvalidateVisual();
            e.Handled = true;

            base.OnPointerPressed(e);
        }

        public override void Render(DrawingContext context)
        {
            context.DrawRectangle(_backgroundBrush, null, Bounds);

            if (TextView?.VisualLinesValid == true)
            {
                foreach (var visualLine in TextView.VisualLines)
                {
                    double y = TextView.VerticalOffset + visualLine.VisualTop + visualLine.Height / 2;

                    if (_pointerOverLine == visualLine.FirstDocumentLine.LineNumber)
                        context.DrawEllipse(_pointerOverBrush, _pointerOverPen, new Point(10, y), 8, 8);
                    else if (_markedDocumentLines.Contains(visualLine.FirstDocumentLine.LineNumber))
                        context.DrawEllipse(_markerBrush, _markerPen, new Point(10, y), 8, 8);
                }
            }

            base.Render(context);
        }
    }
}
