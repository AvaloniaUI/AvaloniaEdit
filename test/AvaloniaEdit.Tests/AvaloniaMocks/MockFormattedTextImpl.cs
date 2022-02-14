using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;

using System.Collections.Generic;

namespace AvaloniaEdit.Tests.AvaloniaMocks
{
    internal class MockFormattedTextImpl : IFormattedTextImpl
    {
        private string _text;
        private Typeface _typeface;

        internal MockFormattedTextImpl(string text, Typeface typeface)
        {
            _text = text;
            _typeface = typeface;
        }
        Size IFormattedTextImpl.Constraint => new Size(0, 0);

        Rect IFormattedTextImpl.Bounds => new Rect(0, 0, _text.Length * _typeface.GlyphTypeface.GetGlyphAdvance(0), 18);

        string IFormattedTextImpl.Text => _text;

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
