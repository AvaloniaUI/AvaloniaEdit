namespace AvaloniaEdit.TextMate.Storage.Abstractions
{
    public interface IResourceStorage
    {
        public IThemeStorage ThemeStorage { get; }

        public IGrammarStorage GrammarStorage { get; }
    }
}
