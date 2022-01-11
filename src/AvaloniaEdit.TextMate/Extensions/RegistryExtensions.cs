using AvaloniaEdit.TextMate.Models;
using AvaloniaEdit.TextMate.Storage.Abstractions;
using System;

namespace AvaloniaEdit.TextMate.Extensions
{
    public static class RegistryExtensions
    {
        public static string GetScopeByExtension(this IResourceStorage storage, string extension)
        {
            foreach (GrammarDefinition definition in storage.GrammarStorage.GrammarDefinitions)
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

        public static string GetScopeByLanguageId(this IResourceStorage storage, string languageId)
        {
            if (string.IsNullOrEmpty(languageId))
                return null;

            foreach (GrammarDefinition definition in storage.GrammarStorage.GrammarDefinitions)
            {
                foreach (var grammar in definition.Contributes.Grammars)
                {
                    if (languageId.Equals(grammar.Language))
                        return grammar.ScopeName;
                }
            }

            return null;
        }

        public static Language GetLanguageByExtension(this IResourceStorage storage, string extension)
        {
            foreach (var definition in storage.GrammarStorage.GrammarDefinitions)
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
    }
}
