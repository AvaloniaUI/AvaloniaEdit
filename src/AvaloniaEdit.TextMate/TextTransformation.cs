using System;
using System.Collections.Generic;

using Avalonia.Media;

using AvaloniaEdit.Document;

using TextMateSharp.Model;

namespace AvaloniaEdit.TextMate
{
    public abstract class TextTransformation : TextSegment
    {
        public TextTransformation(int startOffset, int endOffset)
        {
            StartOffset = startOffset;
            EndOffset = endOffset;
        }

        public abstract void Transform(GenericLineTransformer transformer, DocumentLine line);
    }

    public class ForegroundTextTransformation : TextTransformation
    {
        public interface IColorMap
        {
            bool Contains(int color);
            IBrush GetBrush(int color);
        }

        int _foregroundBrushId;
        int _backgroundBrushId;
        int _fontStyle;

        public ForegroundTextTransformation(
            IColorMap colorMap,
            int startOffset,
            int endOffset,
            int foregroundBrushId,
            int backgroundBrushId,
            int fontStyle) : base(startOffset, endOffset)
        {
            _colorMap = colorMap;
            _foregroundBrushId = foregroundBrushId;
            _backgroundBrushId = backgroundBrushId;
            _fontStyle = fontStyle;
        }

        public override void Transform(GenericLineTransformer transformer, DocumentLine line)
        {
            if (Length == 0)
            {
                return;
            }

            var formattedOffset = 0;
            var endOffset = line.EndOffset;

            if (StartOffset > line.Offset)
            {
                formattedOffset = StartOffset - line.Offset;
            }

            if (EndOffset < line.EndOffset)
            {
                endOffset = EndOffset;
            }

                transformer.SetTextStyle(line, formattedOffset, endOffset - line.Offset - formattedOffset,
                _colorMap.GetBrush(_foregroundBrushId),
                _colorMap.GetBrush(_backgroundBrushId),
                GetFontStyle(),
                GetFontWeight(),
                IsUnderline());
        }

        FontStyle GetFontStyle()
        {
            if (_fontStyle != TextMateSharp.Themes.FontStyle.NotSet &&
                (_fontStyle & TextMateSharp.Themes.FontStyle.Italic) != 0)
                return FontStyle.Italic;

            return FontStyle.Normal;
        }

        FontWeight GetFontWeight()
        {
            if (_fontStyle != TextMateSharp.Themes.FontStyle.NotSet &&
                (_fontStyle & TextMateSharp.Themes.FontStyle.Bold) != 0)
                return FontWeight.Bold;

            return FontWeight.Regular;
        }

        bool IsUnderline()
        {
            if (_fontStyle != TextMateSharp.Themes.FontStyle.NotSet &&
                (_fontStyle & TextMateSharp.Themes.FontStyle.Underline) != 0)
                return true;

            return false;
        }

        IColorMap _colorMap;
    }
}