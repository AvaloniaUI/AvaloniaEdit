using System;
using System.IO;
using System.Text;
using TextMateSharp.Internal.Grammars.Reader;
using TextMateSharp.Internal.Themes.Reader;
using TextMateSharp.Internal.Types;
using TextMateSharp.Themes;

namespace AvaloniaEdit.TextMate
{
    public class ResourceLoader
    {
        public static IRawTheme LoadThemeFromPath(string path)
        {
            using StreamReader reader = new(LoadThemeFromPathToStream(path));
            return ThemeReader.ReadThemeSync(reader);
        }
        public static Stream LoadThemeFromPathToStream(string path)
        {
            if (!File.Exists(path))
            {
                throw new Exception("Invalid path!");
            }
            else
            {
                return new FileStream(path, FileMode.Open, FileAccess.Read);
            }
        }
        public static IRawTheme LoadThemeFromStream(string stream)
        {
            using var temp= new MemoryStream(Encoding.UTF8.GetBytes(stream ?? ""));
            using StreamReader reader = new(temp);
            return ThemeReader.ReadThemeSync(reader);
        }
        public static IRawGrammar LoadGrammarFromStream(string stream)
        {
            using var temp = new MemoryStream(Encoding.UTF8.GetBytes(stream ?? ""));
            using StreamReader reader = new(temp);
            return GrammarReader.ReadGrammarSync(reader);
        }
        public static IRawTheme LoadThemeFromStream(Stream stream)
        {
            using StreamReader reader = new(stream);
            return ThemeReader.ReadThemeSync(reader);
        }
        public static IRawGrammar LoadGrammarFromStream(Stream stream)
        {
            using StreamReader reader = new(stream);
            return GrammarReader.ReadGrammarSync(reader);
        }
    }
}
