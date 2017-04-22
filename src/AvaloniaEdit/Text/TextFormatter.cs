namespace AvaloniaEdit.Text
{
    public class TextFormatter
    {
        public TextLine FormatLine(TextSource textSource, int firstCharIndex, double paragraphWidth, TextParagraphProperties paragraphProperties)
        {
            return TextLineImpl.Create(paragraphProperties, firstCharIndex, (int)paragraphWidth, textSource);
        }
    }
}