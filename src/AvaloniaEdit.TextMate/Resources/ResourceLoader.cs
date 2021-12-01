using System;
using System.IO;
using System.Reflection;

namespace AvaloniaEdit.TextMate.Resources
{
    internal class ResourceLoader
    {
        const string GrammarPrefix = "AvaloniaEdit.TextMate.Resources.Grammars.";
        const string ThemesPrefix = "AvaloniaEdit.TextMate.Resources.Themes.";

        internal static Stream OpenGrammarPackage(string grammarName)
        {
            string grammarPackage = GrammarPrefix + grammarName.ToLower() + "." + "package.json";

            var result = typeof(ResourceLoader).GetTypeInfo().Assembly.GetManifestResourceStream(
                grammarPackage);

            if (result == null)
                throw new FileNotFoundException("The grammar package '" + grammarPackage + "' was not found.");

            return result;
        }

        internal static Stream TryOpenGrammarStream(string path)
        {
            return typeof(ResourceLoader).GetTypeInfo().Assembly.GetManifestResourceStream(
                GrammarPrefix + path);
        }

        internal static Stream TryOpenThemeStream(string path)
        {
            return typeof(ResourceLoader).GetTypeInfo().Assembly.GetManifestResourceStream(
                ThemesPrefix + path);
        }
    }
}
