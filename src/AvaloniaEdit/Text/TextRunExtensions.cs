namespace AvaloniaEdit.Text
{
    internal static class TextRunExtensions
    {
        internal static StringRange GetStringRange(this TextRun textRun)
        {
            switch (textRun)
            {
                case TextCharacters _:
                    return textRun.StringRange;
                default:
                    return StringRange.Empty;
            }
        }
    }
}