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

        public static IResourceStorage SetupStorage()
        {
            var themes = new Dictionary<string, IRawTheme>();
            var grammars = new Dictionary<string, IRawGrammar>();
            var grammarDefinitions = new List<IGrammarDefinition>();
            foreach (var item in Enum.GetValues(typeof(ThemeName)).Cast<ThemeName>())
            {
                var theme = LoadThemeByNameToStream(item);
                using StreamReader reader = new(theme.Item1);
                themes.Add(theme.Item2, ThemeReader.ReadThemeSync(reader));
            }
            foreach (var item in Enum.GetValues(typeof(GrammarName)).Cast<GrammarName>())
            {
                var stream = LoadGrammarByNameToStream(item);

                var serializer = new JsonSerializer();
                GrammarDefinition definition = null;
                using (var reader = new StreamReader(stream))
                using (var jsonTextReader = new JsonTextReader(reader))
                {
                    definition = serializer.Deserialize<GrammarDefinition>(jsonTextReader);
                    grammarDefinitions.Add(definition);
                }

                var gr2 = LoadGrammarByNameToStream2(item, GetFilePath(definition.Contributes.Grammars.First().ScopeName, definition));
                using StreamReader reader2 = new(gr2);
                grammars.Add(definition.Contributes.Grammars.First().ScopeName, GrammarReader.ReadGrammarSync(reader2));

            }
            return new ResourceStorage(new ThemeStorage(themes, themes.First(x => x.Value.GetName() == "Dark+ (default dark)").Value),
               new GrammarStorage(grammars, grammars.First().Value, grammarDefinitions));
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

        public static Stream LoadGrammarByNameToStream2(GrammarName name, string fileName)
        {
            return typeof(ResourceLoader).GetTypeInfo().Assembly.GetManifestResourceStream(
                GrammarPrefix + name.ToString().ToLower() + "." + fileName);
        }

        public static Tuple<Stream, string> LoadThemeByNameToStream(ThemeName name)
        {
            var stream = typeof(ResourceLoader).GetTypeInfo().Assembly.GetManifestResourceStream(
                ThemesPrefix + GetThemeFileName(name));
            return new Tuple<Stream, string>(stream, GetThemeFileName(name));
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
