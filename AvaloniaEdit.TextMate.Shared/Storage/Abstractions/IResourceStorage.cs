namespace AvaloniaEdit.TextMate.Storage.Abstractions
{
    public interface IResourceStorage
    {
        public IThemeStorage ThemeStorage { get; set; }

        public IGrammarStorage GrammarStorage { get; set; }
    }
}
