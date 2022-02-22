using System;

using Avalonia.Media;

using AvaloniaEdit.Document;

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
            IBrush GetBrush(int color);
        }

        private IColorMap _colorMap;
        private Action<Exception> _exceptionHandler;
        private int _foreground;
        private int _background;
        private int _fontStyle;

        public ForegroundTextTransformation(
            IColorMap colorMap,
            Action<Exception> exceptionHandler,
            int startOffset,
            int endOffset,
            int foreground,
            int background,
            int fontStyle) : base(startOffset, endOffset)
        {
            _colorMap = colorMap;
            _exceptionHandler = exceptionHandler;
            _foreground = foreground;
            _background = background;
            _fontStyle = fontStyle;
        }

        public override void Transform(GenericLineTransformer transformer, DocumentLine line)
        {
            try
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
                _colorMap.GetBrush(_foreground),
                _colorMap.GetBrush(_background),
                GetFontStyle(),
                GetFontWeight(),
                IsUnderline());
            }
            catch(Exception ex)
            {
                _exceptionHandler?.Invoke(ex);
            }
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
    }
}