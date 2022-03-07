using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace AvaloniaEdit.Utils;

public static class TextLineExtensions
{
    public static Rect GetTextBounds(this TextLine textLine, int start, int length)
    {
        var startX = textLine.GetDistanceFromCharacterHit(new CharacterHit(start));

        var endX = textLine.GetDistanceFromCharacterHit(new CharacterHit(start + length));

        return new Rect(startX, 0, endX - startX, textLine.Height);
    }
}