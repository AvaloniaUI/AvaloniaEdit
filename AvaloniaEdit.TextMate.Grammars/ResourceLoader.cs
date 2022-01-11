using AvaloniaEdit.TextMate.Grammars.Enums;
using AvaloniaEdit.TextMate.Models;
using AvaloniaEdit.TextMate.Models.Abstractions;
using AvaloniaEdit.TextMate.Storage;
using AvaloniaEdit.TextMate.Storage.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TextMateSharp.Internal.Grammars.Reader;
using TextMateSharp.Internal.Themes.Reader;
using TextMateSharp.Internal.Types;
using TextMateSharp.Themes;

namespace AvaloniaEdit.TextMate.Grammars
{
    public static class ResourceLoader
    {
        private const string GrammarPrefix = "AvaloniaEdit.TextMate.Grammars.Resources.Grammars.";

        private const string ThemesPrefix = "AvaloniaEdit.TextMate.Grammars.Resources.Themes.";

        public static IResourceStorage SetupStorage(ThemeName selectedTheme, GrammarName selectedGrammar)
        {
            var themes = new Dictionary<string, IRawTheme>();
            var grammars = new Dictionary<string, IRawGrammar>();
            var grammarDefinitions = new List<IGrammarDefinition>();
            IRawTheme selectedThemeRaw = null;
            IRawGrammar selectedGrammarRaw = null;
            foreach (var item in Enum.GetValues(typeof(ThemeName)).Cast<ThemeName>())
            {
                var theme = LoadThemeByNameToStream(item);
                using StreamReader reader = new(theme.Item1);
                var rawTheme = ThemeReader.ReadThemeSync(reader);
                if (item == selectedTheme)
                {
                    selectedThemeRaw = rawTheme;
                }
                themes.Add(theme.Item2, rawTheme);
            }
            foreach (var item in Enum.GetValues(typeof(GrammarName)).Cast<GrammarName>())
            {
                var serializer = new JsonSerializer();
                GrammarDefinition definition = null;
                using var reader = new StreamReader(LoadGrammarByNameToStream(item));
                using var jsonTextReader = new JsonTextReader(reader);
                definition = serializer.Deserialize<GrammarDefinition>(jsonTextReader);
                grammarDefinitions.Add(definition);

                var grammarPackage = LoadGrammarPackageByNameToStream(item, GetFilePath(definition.Contributes.Grammars.First().ScopeName, definition));
                using var reader2 = new StreamReader(grammarPackage);
                var grammarRaw = GrammarReader.ReadGrammarSync(reader2);
                if (item == selectedGrammar)
                {
                    selectedGrammarRaw = grammarRaw;
                }
                grammars.Add(definition.Contributes.Grammars.First().ScopeName, grammarRaw);

            }
            return new ResourceStorage(new ThemeStorage(themes, selectedThemeRaw),
               new GrammarStorage(grammars, selectedGrammarRaw, grammarDefinitions));
        }
        private static string GetFilePath(string scopeName, IGrammarDefinition grammarDefinition)
        {
            foreach (Grammar grammar in grammarDefinition.Contributes.Grammars)
            {
                if (scopeName.Equals(grammar.ScopeName))
                {
                    string grammarPath = grammar.Path;

                    if (grammarPath.StartsWith("./"))
                        grammarPath = grammarPath.Substring(2);

                    grammarPath = grammarPath.Replace("/", ".");

                    return grammarPath;
                }
            }

            return null;
        }
        public static Stream LoadGrammarByNameToStream(GrammarName name)
        {
            return typeof(ResourceLoader).GetTypeInfo().Assembly.GetManifestResourceStream(
                GrammarPrefix + name.ToString().ToLower() + "." + "package.json");
        }

        public static Stream LoadGrammarPackageByNameToStream(GrammarName name, string fileName)
        {
            return typeof(ResourceLoader).GetTypeInfo().Assembly.GetManifestResourceStream(
                GrammarPrefix + name.ToString().ToLower() + "." + fileName);
        }

        public static Tuple<Stream, string> LoadThemeByNameToStream(ThemeName name)
        {
            var themeFileName = GetThemeFileName(name);
            var stream = typeof(ResourceLoader).GetTypeInfo().Assembly.GetManifestResourceStream(
                ThemesPrefix + themeFileName);
            return new Tuple<Stream, string>(stream, themeFileName);
        }

        private static string GetThemeFileName(ThemeName name)
        {
            return name switch
            {
                ThemeName.Abbys => "abyss-color-theme.json",
                ThemeName.Dark => "dark_vs.json",
                ThemeName.DarkPlus => "dark_plus.json",
                ThemeName.DimmedMonokai => "dimmed-monokai-color-theme.json",
                ThemeName.KimbieDark => "kimbie-dark-color-theme.json",
                ThemeName.Light => "light_vs.json",
                ThemeName.LightPlus => "light_plus.json",
                ThemeName.Monokai => "monokai-color-theme.json",
                ThemeName.QuietLight => "quietlight-color-theme.json",
                ThemeName.Red => "Red-color-theme.json",
                ThemeName.SolarizedDark => "solarized-dark-color-theme.json",
                ThemeName.SolarizedLight => "solarized-light-color-theme.json",
                ThemeName.TomorrowNightBlue => "tomorrow-night-blue-color-theme.json",
                _ => null,
            };
        }
    }
}
