using System;
using System.Collections.Generic;
using System.IO;

using AvaloniaEdit.TextMate.Resources;

using Newtonsoft.Json;

using TextMateSharp.Internal.Themes.Reader;
using TextMateSharp.Registry;
using TextMateSharp.Themes;

namespace AvaloniaEdit.TextMate
{
    public class RegistryOptions : IRegistryOptions
    {
        ThemeName _defaultTheme;
        Dictionary<string, GrammarDefinition> _availableGrammars = new Dictionary<string, GrammarDefinition>();

        public RegistryOptions(ThemeName defaultTheme)
        {
            _defaultTheme = defaultTheme;
            InitializeGrammars();
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

        public IRawTheme LoadTheme(ThemeName name)
        {
            string themeFile = GetThemeFile(name);

            if (themeFile == null)
                return null;

            using (Stream s = ResourceLoader.TryOpenThemeStream(GetThemeFile(name)))
            using (StreamReader reader = new StreamReader(s))
            {
                return ThemeReader.ReadThemeSync(reader);
            }
        }

        string GetThemeFile(ThemeName name)
        {
            switch (name)
            {
                case ThemeName.Abbys:
                    return "abyss-color-theme.json";
                case ThemeName.Dark:
                    return "dark_vs.json";
                case ThemeName.DarkPlus:
                    return "dark_plus.json";
                case ThemeName.DimmedMonokai:
                    return "dimmed-monokai-color-theme.json";
                case ThemeName.KimbieDark:
                    return "kimbie-dark-color-theme.json";
                case ThemeName.Light:
                    return "light_vs.json";
                case ThemeName.LightPlus:
                    return "light_plus.json";
                case ThemeName.Monokai:
                    return "monokai-color-theme.json";
                case ThemeName.QuietLight:
                    return "quietlight-color-theme.json";
                case ThemeName.Red:
                    return "Red-color-theme.json";
                case ThemeName.SolarizedDark:
                    return "solarized-dark-color-theme.json";
                case ThemeName.SolarizedLight:
                    return "solarized-light-color-theme.json";
                case ThemeName.TomorrowNightBlue:
                    return "tomorrow-night-blue-color-theme.json";
            }

            return null;
        }

        void InitializeGrammars()
        {
            var serializer = new JsonSerializer();

            foreach (string grammar in GrammarNames.SupportedGrammars)
            {
                using (Stream stream = ResourceLoader.OpenGrammarPackage(grammar))
                using (StreamReader reader = new StreamReader(stream))
                using (JsonTextReader jsonTextReader = new JsonTextReader(reader))
                {
                    GrammarDefinition definition = serializer.Deserialize<GrammarDefinition>(jsonTextReader);
                    _availableGrammars.Add(grammar, definition);
                }
            }
        }

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
            Stream themeStream = ResourceLoader.TryOpenThemeStream(scopeName.Replace("./", string.Empty));

            if (themeStream != null)
                return themeStream;

            return ResourceLoader.TryOpenGrammarStream(((IRegistryOptions)this).GetFilePath(scopeName));
        }

        IRawTheme IRegistryOptions.GetTheme()
        {
            return LoadTheme(_defaultTheme);
        }
    }
}
