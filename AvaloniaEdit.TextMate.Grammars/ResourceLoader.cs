using AvaloniaEdit.TextMate.Grammars.Enums;
using System;
using System.IO;
using System.Reflection;

namespace AvaloniaEdit.TextMate.Grammars
{
    public static class ResourceLoader
    {
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
        const string GrammarPrefix = "AvaloniaEdit.TextMate.Grammars.Resources.Grammars.";
        const string ThemesPrefix = "AvaloniaEdit.TextMate.Grammars.Resources.Themes.";

        public static Tuple<Stream,string> LoadGrammarByNameToStream(GrammarName name)
        {
            return new Tuple<Stream, string>(typeof(ResourceLoader).GetTypeInfo().Assembly.GetManifestResourceStream(
                GrammarPrefix + name.ToString().ToLower() + "." + "package.json"), name.ToString().ToLower() + "." + "package.json");
        }

        public static Tuple<Stream,string> LoadThemeByNameToStream(ThemeName name)
        {
            return new Tuple<Stream, string>(typeof(ResourceLoader).GetTypeInfo().Assembly.GetManifestResourceStream(
                ThemesPrefix + GetThemeFileName(name)), GetThemeFileName(name));
        }
    }
}
