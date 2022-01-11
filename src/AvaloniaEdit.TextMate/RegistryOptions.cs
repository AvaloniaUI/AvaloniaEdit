using AvaloniaEdit.TextMate.Models;
using AvaloniaEdit.TextMate.Storage.Abstractions;
using System.Collections.Generic;
using System.Linq;
using TextMateSharp.Internal.Types;
using TextMateSharp.Registry;
using TextMateSharp.Themes;

namespace AvaloniaEdit.TextMate
{
    public class RegistryOptions : IRegistryOptions
    {
        private readonly IResourceStorage _resourceStorage;

        public RegistryOptions(IResourceStorage storage)
        {
            _resourceStorage = storage;
        }

        public IResourceStorage Storage { get => _resourceStorage; }
        public List<Language> GetAvailableLanguages()
        {
            List<Language> result = new List<Language>();

            foreach (GrammarDefinition definition in _resourceStorage.GrammarStorage.GrammarDefinitions)
            {
                foreach (Language language in definition.Contributes.Languages)
                {
                    if (language.Aliases == null || language.Aliases.Count == 0)
                        continue;

                    result.Add(language);
                }
            }

            return result;
        }

        public IRawTheme GetDefaultTheme()
        {
            return _resourceStorage.ThemeStorage.SelectedTheme;
        }

        public IRawGrammar GetGrammar(string scopeName)
        {
            return _resourceStorage.GrammarStorage.Grammars.First(x => x.Key == scopeName).Value;
        }

        ICollection<string> IRegistryOptions.GetInjections(string scopeName)
        {
            return null;
        }

        public IRawTheme GetTheme(string scopeName)
        {
            return _resourceStorage.ThemeStorage.Themes.First(x => x.Key == scopeName.Substring(2)).Value;
        }
    }
}
