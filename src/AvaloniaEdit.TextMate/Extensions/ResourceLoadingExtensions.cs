using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TextMateSharp.Internal.Grammars.Reader;
using TextMateSharp.Internal.Themes.Reader;
using TextMateSharp.Internal.Types;
using TextMateSharp.Themes;

namespace AvaloniaEdit.TextMate.Extensions
{
    public static class ResourceLoadingExtensions
    {
        public static IRawTheme LoadThemeFromPath(this string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(fs);
            return ThemeReader.ReadThemeSync(reader);
        }
        public static IRawGrammar LoadGrammarFromPath(this string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(fs);
            return GrammarReader.ReadGrammarSync(reader);
        }
        public static IRawTheme LoadThemeFromString(this string json)
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            using var reader = new StreamReader(ms);
            return ThemeReader.ReadThemeSync(reader);
        }
        public static IRawGrammar LoadGrammarFromString(this string json)
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            using var reader = new StreamReader(ms);
            return GrammarReader.ReadGrammarSync(reader);
        }
        public static IRawTheme LoadThemeFromStream(this Stream stream)
        {
            using StreamReader reader = new(stream);
            return ThemeReader.ReadThemeSync(reader);
        }
        public static IRawGrammar LoadGrammarFromStream(this Stream stream)
        {
            using var reader = new StreamReader(stream);
            return GrammarReader.ReadGrammarSync(reader);
        }
    }
}
