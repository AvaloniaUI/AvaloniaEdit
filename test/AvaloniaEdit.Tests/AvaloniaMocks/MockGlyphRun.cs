using Avalonia;
using Avalonia.Media.TextFormatting;
using Avalonia.Platform;
using System.Collections.Generic;

namespace AvaloniaEdit.AvaloniaMocks
{
    internal class MockGlyphRun : IGlyphRunImpl
    {
        public MockGlyphRun(IReadOnlyList<GlyphInfo> glyphInfos)
        {
            var width = 0.0;

            for (var i = 0; i < glyphInfos.Count; ++i)
            {
                width += glyphInfos[i].GlyphAdvance;
            }

            Bounds = new Rect(new Size(width, 10));
        }

        public Rect Bounds { get; }

        public Point BaselineOrigin => new Point(0, 8);

        public void Dispose()
        {

        }

        public IReadOnlyList<float> GetIntersections(float lowerBound, float upperBound)
        {
            return null;
        }
    }
}
