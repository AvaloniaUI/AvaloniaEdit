using System.Globalization;
using Avalonia.Media;

namespace AvaloniaEdit.Text
{
    public class TextRunProperties
    {
        public TextRunProperties Clone() => new TextRunProperties
        {
            BackgroundBrush = BackgroundBrush,
            CultureInfo = CultureInfo,
            ForegroundBrush = ForegroundBrush,
            Typeface = Typeface,
            FontSize = FontSize
        };

        public IBrush BackgroundBrush { get; set; }

        public CultureInfo CultureInfo { get; set; }

        public IBrush ForegroundBrush { get; set; }

        public Typeface Typeface { get; set; }

        public double FontSize { get; set; }
    }
}
