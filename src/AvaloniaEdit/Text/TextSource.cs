namespace AvaloniaEdit.Text
{
    public abstract class TextSource
    {
        public abstract TextRun GetTextRun(int characterIndex);
    }
}