using System;
using System.IO;
using TextMateSharp.Internal.Themes.Reader;
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
        public static IRawTheme LoadThemeFromStream(Stream stream)
        {
            StreamReader reader = new(stream);
            return ThemeReader.ReadThemeSync(reader);
        }
    }
}
