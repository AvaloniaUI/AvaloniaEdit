namespace AvaloniaEdit.Text
{
    public class TextCharacters : TextRun
    {
        public sealed override StringRange StringRange { get; }

        public sealed override int Length { get; }

        public sealed override TextRunProperties Properties { get; }

        public TextCharacters(string characterString, TextRunProperties textRunProperties)
            : this(characterString, 0, characterString?.Length ?? 0, textRunProperties)
        {
        }

        public TextCharacters(string characterString, int offsetToFirstChar, int length, TextRunProperties textRunProperties)
            : this(new StringRange(characterString, offsetToFirstChar, length), length, textRunProperties)
        {
        }

        private TextCharacters(StringRange stringRange, int length, TextRunProperties textRunProperties)
        {
            StringRange = stringRange;
            Length = length;
            Properties = textRunProperties;
        }
    }
}