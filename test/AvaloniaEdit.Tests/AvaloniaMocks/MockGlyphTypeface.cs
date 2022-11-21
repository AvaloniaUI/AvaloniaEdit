using Avalonia.Media;

using System;

namespace AvaloniaEdit.AvaloniaMocks
{
    public class MockGlyphTypeface : IGlyphTypeface
    {
        public const int GlyphAdvance = 8;
        public const short DefaultFontSize = 10;
        public const int GlyphAscent = 2;
        public const int GlyphDescent = 10;

        public short DesignEmHeight => DefaultFontSize;
        public int Ascent => GlyphAscent;
        public int Descent => GlyphDescent;
        public FontSimulations FontSimulations { get; }
        public int GlyphCount { get; }
        public int LineGap { get; }
        public FontMetrics Metrics { get; }
        public int UnderlinePosition { get; }
        public int UnderlineThickness { get; }
        public int StrikethroughPosition { get; }
        public int StrikethroughThickness { get; }
        public bool IsFixedPitch { get; }

        public ushort GetGlyph(uint codepoint)
        {
            return 0;
        }

        public ushort[] GetGlyphs(ReadOnlySpan<uint> codepoints)
        {
            return new ushort[codepoints.Length];
        }

        public int GetGlyphAdvance(ushort glyph)
        {
            return GlyphAdvance;
        }

        public int[] GetGlyphAdvances(ReadOnlySpan<ushort> glyphs)
        {
            var advances = new int[glyphs.Length];

            for (var i = 0; i < advances.Length; i++)
            {
                advances[i] = GlyphAdvance;
            }

            return advances;
        }

        public void Dispose() { }

        public bool TryGetGlyph(uint codepoint, out ushort glyph)
        {
            glyph = default;
            return false;
        }

        public bool TryGetGlyphMetrics(ushort glyph, out GlyphMetrics metrics)
        {
            metrics = default;
            return false;
        }

        public bool TryGetTable(uint tag, out byte[] table)
        {
            table = default;
            return false;
        }
    }
}
