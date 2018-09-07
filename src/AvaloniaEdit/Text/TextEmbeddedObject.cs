using Avalonia;
using Avalonia.Media;

namespace AvaloniaEdit.Text
{
    public abstract class TextEmbeddedObject : TextRun
    {
        public abstract bool HasFixedSize { get; }

        public abstract Size GetSize(double remainingParagraphWidth);

        public abstract Rect ComputeBoundingBox();

        public abstract void Draw(DrawingContext drawingContext, Point origin);
    }
}