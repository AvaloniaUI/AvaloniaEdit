namespace AvaloniaEdit.Text
{
    public class TextEndOfLine : TextRun
    {
        public sealed override StringRange StringRange => default(StringRange);

        public sealed override int Length { get; }

        public sealed override TextRunProperties Properties { get; }

        public TextEndOfLine(int length) : this(length, null)
        {
        }

        public TextEndOfLine(int length, TextRunProperties textRunProperties)
        {
            Length = length;
            Properties = textRunProperties;
        }
    }
}