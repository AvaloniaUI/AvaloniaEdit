namespace AvaloniaEdit.Text
{
    public class TextEndOfParagraph : TextEndOfLine
    {
        public TextEndOfParagraph(int length) : base(length)
        {
        }

        public TextEndOfParagraph(int length, TextRunProperties textRunProperties)
            : base(length, textRunProperties)
        {
        }
    }
}