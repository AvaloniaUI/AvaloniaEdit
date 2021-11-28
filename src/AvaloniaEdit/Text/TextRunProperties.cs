using System.Globalization;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace AvaloniaEdit.Text
{
    public class TextRunProperties
    {
        private IBrush _backgroundBrush;
        private CultureInfo _cultureInfo;
        private IBrush _foregroundBrush;
        private Typeface _typeface;
        private double _fontSize;
        private FontMetrics _fontMetrics;
        private bool _underline;
        private bool _strikethrough;

        public TextRunProperties Clone()
        {
            TextRunProperties clone = new TextRunProperties();

            clone._backgroundBrush = BackgroundBrush;
            clone._cultureInfo = CultureInfo;
            clone._foregroundBrush = ForegroundBrush;
            clone._typeface = Typeface;
            clone._fontSize = FontSize;
            clone._fontMetrics = FontMetrics;
            clone._underline = Underline;
            clone._strikethrough = Strikethrough;

            return clone;
        }

        public IBrush BackgroundBrush
        {
            get { return _backgroundBrush; }
            set { _backgroundBrush = value; }
        }

        public CultureInfo CultureInfo
        {
            get { return _cultureInfo; }
            set { _cultureInfo = value; }
        }

        public IBrush ForegroundBrush
        {
            get { return _foregroundBrush; }
            set { _foregroundBrush = value; }
        }

        public Typeface Typeface
        {
            get { return _typeface; }
            set
            {
                _typeface = value;
                InvalidateFontMetrics();
            }
        }

        public double FontSize
        {
            get { return _fontSize; }
            set
            {
                _fontSize = value;
                InvalidateFontMetrics();
            }
        }

        public bool Underline
        {
            get{ return _underline; }
            set { _underline = value; }
        }

        public bool Strikethrough
        {
            get { return _strikethrough; }
            set { _strikethrough = value; }
        }

        public FontMetrics FontMetrics
        {
            get { return _fontMetrics; }
        }

        void InvalidateFontMetrics()
        {
            if (_typeface.FontFamily == null || _fontSize == 0)
                return;

            _fontMetrics = new FontMetrics(_typeface, _fontSize);
        }
    }
}
