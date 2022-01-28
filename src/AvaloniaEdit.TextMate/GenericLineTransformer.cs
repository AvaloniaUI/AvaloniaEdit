using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;

namespace AvaloniaEdit.TextMate
{
    public abstract class GenericLineTransformer : DocumentColorizingTransformer
    {
        protected override void ColorizeLine(DocumentLine line)
        {
            TransformLine(line, CurrentContext);
        }

        protected abstract void TransformLine(DocumentLine line, ITextRunConstructionContext context);

        public void SetTextStyle(
            DocumentLine line,
            int startIndex,
            int length,
            IBrush foreground,
            IBrush background,
            FontStyle fontStyle, 
            FontWeight fontWeigth, 
            bool isUnderline)
        {
            int startOffset = 0;
            int endOffset = 0;

            if (startIndex >= 0 && length > 0)
            {
                if ((line.Offset + startIndex + length) > line.EndOffset)
                {
                    length = (line.EndOffset - startIndex) - line.Offset - startIndex;
                }

                startOffset = line.Offset + startIndex;
                endOffset = line.Offset + startIndex + length;
            }
            else
            {
                startOffset = line.Offset;
                endOffset = line.EndOffset;
            }

            if (startOffset > CurrentContext.Document.TextLength ||
                endOffset > CurrentContext.Document.TextLength)
                return;

            ChangeLinePart(
                startOffset,
                endOffset,
                visualLine => ChangeVisualLine(visualLine, foreground, background, fontStyle, fontWeigth, isUnderline));
        }

        void ChangeVisualLine(
            VisualLineElement visualLine,
            IBrush foreground,
            IBrush background,
            FontStyle fontStyle,
            FontWeight fontWeigth,
            bool isUnderline)
        {
            if (foreground != null)
                visualLine.TextRunProperties.ForegroundBrush = foreground;

            if (background != null)
                visualLine.TextRunProperties.BackgroundBrush = background;

            visualLine.TextRunProperties.Underline = isUnderline;

            if (visualLine.TextRunProperties.Typeface.Style != fontStyle ||
                visualLine.TextRunProperties.Typeface.Weight != fontWeigth)
            {
                visualLine.TextRunProperties.Typeface = new Typeface(
                    visualLine.TextRunProperties.Typeface.FontFamily, fontStyle, fontWeigth);
            }

        }
    }
}