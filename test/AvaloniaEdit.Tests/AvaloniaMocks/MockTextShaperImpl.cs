using System.Globalization;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Platform;
using Avalonia.Utilities;

namespace AvaloniaEdit.AvaloniaMocks;

#nullable enable

public class MockTextShaperImpl : ITextShaperImpl
{
    public ShapedBuffer ShapeText(ReadOnlySlice<char> text, GlyphTypeface typeface, double fontRenderingEmSize, CultureInfo? culture,
        sbyte bidiLevel)
    {
        throw new System.NotImplementedException();
    }
}