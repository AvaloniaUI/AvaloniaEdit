using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;

namespace AvaloniaEdit.Text
{
    public abstract class TextLine
    {
        public abstract int FirstIndex { get; }
        public abstract int Length { get; }
        public abstract int TrailingWhitespaceLength { get; }
        public abstract double Width { get; }
        public abstract double WidthIncludingTrailingWhitespace { get; }
        public abstract double Height { get; }
        public abstract double Baseline { get; }
        public abstract void Draw(DrawingContext drawingContext, Point origin);
        public abstract double GetDistanceFromCharacter(int firstIndex, int trailingLength);
        public abstract (int firstIndex, int trailingLength) GetCharacterFromDistance(double distance);
        public abstract Rect GetTextBounds(int firstIndex, int textLength);
        public abstract IList<TextRun> GetTextRuns();
    }
}