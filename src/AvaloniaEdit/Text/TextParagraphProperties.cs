using Avalonia.Media;

namespace AvaloniaEdit.Text
{
    public sealed class TextParagraphProperties
    {
        public double DefaultIncrementalTab { get; set; }

        public bool FirstLineInParagraph { get; set; }

        public TextRunProperties DefaultTextRunProperties { get; set; }

        public TextWrapping TextWrapping { get; set; }

        public double Indent { get; set; }
    }
}
