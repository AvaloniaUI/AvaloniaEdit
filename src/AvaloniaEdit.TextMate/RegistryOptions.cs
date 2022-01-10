using AvaloniaEdit.TextMate.Models;
using AvaloniaEdit.TextMate.Storage.Abstractions;
using System;
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

        public Language GetLanguageByExtension(string extension)
        {
            foreach (GrammarDefinition definition in _resourceStorage.GrammarStorage.GrammarDefinitions)
            {
                foreach (var language in definition.Contributes.Languages)
                {
                    if (language.Extensions == null)
                        continue;

                    foreach (var languageExtension in language.Extensions)
                    {
                        if (extension.Equals(languageExtension,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            return language;
                        }
                    }
                }
            }

            return null;
        }

        public string GetScopeByExtension(string extension)
        {
            foreach (GrammarDefinition definition in _resourceStorage.GrammarStorage.GrammarDefinitions)
            {
                foreach (var language in definition.Contributes.Languages)
                {
                    foreach (var languageExtension in language.Extensions)
                    {
                        if (extension.Equals(languageExtension,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (var grammar in definition.Contributes.Grammars)
                            {
                                return grammar.ScopeName;
                            }
                        }
                    }
                }
            }

            return null;
        }

        public string GetScopeByLanguageId(string languageId)
        {
            if (string.IsNullOrEmpty(languageId))
                return null;

            foreach (GrammarDefinition definition in _resourceStorage.GrammarStorage.GrammarDefinitions)
            {
                foreach (var grammar in definition.Contributes.Grammars)
                {
                    if (languageId.Equals(grammar.Language))
                        return grammar.ScopeName;
                }
            }

            return null;
        }

        public IRawTheme GetTheme(string scopeName)
        {
            return _resourceStorage.ThemeStorage.Themes.First(x => x.Key == scopeName.Substring(2)).Value;
        }
    }
}
