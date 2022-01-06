using AvaloniaEdit.TextMate.Models;
using System;
using System.Collections.Generic;
using System.IO;
using TextMateSharp.Registry;
using TextMateSharp.Themes;

namespace AvaloniaEdit.TextMate
{
    public class RegistryOptions : IRegistryOptions
    {
        public Tuple<IRawTheme, Stream> _defaultTheme;
        Dictionary<string, GrammarDefinition> _availableGrammars = new Dictionary<string, GrammarDefinition>();

        public RegistryOptions(Stream defaultThemeStream)
        {
            _defaultTheme = new Tuple<IRawTheme, Stream>(ResourceLoader.LoadThemeFromStream(defaultThemeStream), defaultThemeStream);
            //InitializeGrammars();
        }

        public List<Language> GetAvailableLanguages()
        {
            List<Language> result = new List<Language>();

            foreach (GrammarDefinition definition in _availableGrammars.Values)
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

        public Language GetLanguageByExtension(string extension)
        {
            foreach (GrammarDefinition definition in _availableGrammars.Values)
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
            foreach (GrammarDefinition definition in _availableGrammars.Values)
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

            foreach (GrammarDefinition definition in _availableGrammars.Values)
            {
                foreach (var grammar in definition.Contributes.Grammars)
                {
                    if (languageId.Equals(grammar.Language))
                        return grammar.ScopeName;
                }
            }

            return null;
        }

        public IRawTheme LoadTheme(string themePath)
        {
            return ResourceLoader.LoadThemeFromPath(themePath);
        }

        //void InitializeGrammars()
        //{
        //    var serializer = new JsonSerializer();

        //    foreach (string grammar in GrammarNames.SupportedGrammars)
        //    {
        //        using (Stream stream = ResourceLoader.OpenGrammarPackage(grammar))
        //        using (StreamReader reader = new StreamReader(stream))
        //        using (JsonTextReader jsonTextReader = new JsonTextReader(reader))
        //        {
        //            GrammarDefinition definition = serializer.Deserialize<GrammarDefinition>(jsonTextReader);
        //            _availableGrammars.Add(grammar, definition);
        //        }
        //    }
        //}

        string IRegistryOptions.GetFilePath(string scopeName)
        {
            foreach (string grammarName in _availableGrammars.Keys)
            {
                GrammarDefinition definition = _availableGrammars[grammarName];

                foreach (Grammar grammar in definition.Contributes.Grammars)
                {
                    if (scopeName.Equals(grammar.ScopeName))
                    {
                        string grammarPath = grammar.Path;

                        if (grammarPath.StartsWith("./"))
                            grammarPath = grammarPath.Substring(2);

                        grammarPath = grammarPath.Replace("/", ".");

                        return grammarName.ToLower() + "." + grammarPath;
                    }
                }
            }

            return null;
        }

        ICollection<string> IRegistryOptions.GetInjections(string scopeName)
        {
            return null;
        }

        Stream IRegistryOptions.GetInputStream(string scopeName)
        {
            return _defaultTheme.Item2;
        }

        IRawTheme IRegistryOptions.GetTheme()
        {
            return _defaultTheme.Item1;
        }
    }
}
