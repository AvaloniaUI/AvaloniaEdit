using AvaloniaEdit.TextMate.Storage.Abstractions;

namespace AvaloniaEdit.TextMate.Storage
{
    public class ResourceStorage : IResourceStorage
    {
        public ResourceStorage(IThemeStorage storage, IGrammarStorage grammarStorage)
        {
            ThemeStorage = storage;
            GrammarStorage = grammarStorage;
        }

        public IThemeStorage ThemeStorage { get; set; }
        public IGrammarStorage GrammarStorage { get; set; }
    }
}
