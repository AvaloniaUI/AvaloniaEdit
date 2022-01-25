using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;

using System.Collections.Generic;

namespace AvaloniaEdit.Tests.AvaloniaMocks
{
    internal class MockFormattedTextImpl : IFormattedTextImpl
    {
        Size IFormattedTextImpl.Constraint => new Size(0, 0);

        Rect IFormattedTextImpl.Bounds => new Rect(0, 0, 0, 0);

        string IFormattedTextImpl.Text => throw new System.NotImplementedException();

        IEnumerable<FormattedTextLine> IFormattedTextImpl.GetLines()
        {
            return null;
        }

        TextHitTestResult IFormattedTextImpl.HitTestPoint(Point point)
        {
            return null;
        }

        Rect IFormattedTextImpl.HitTestTextPosition(int index)
        {
            return Rect.Empty;
        }

        IEnumerable<Rect> IFormattedTextImpl.HitTestTextRange(int index, int length)
        {
            return new Rect[] { };
        }
    }
}
