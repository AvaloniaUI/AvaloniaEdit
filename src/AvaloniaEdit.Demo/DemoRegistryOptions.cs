using System;
using System.Collections.Generic;
using System.IO;
using TextMateSharp.Internal.Themes.Reader;
using TextMateSharp.Registry;
using TextMateSharp.Themes;

namespace AvaloniaEdit.Demo
{
    class DemoRegistryOptions : IRegistryOptions
    {
        private string _grammarFile;
        private string _themeFile;

        internal DemoRegistryOptions(string grammarFile, string themeFile)
        {
            _grammarFile = grammarFile;
            _themeFile = themeFile;
        }

        public string GetFilePath(string scopeName)
        {
            return _grammarFile;
        }

        public ICollection<string> GetInjections(string scopeName)
        {
            return null;
        }

        public Stream GetInputStream(string scopeName)
        {
            return File.OpenRead(GetFilePath(scopeName));
        }

        public IRawTheme GetTheme()
        {
            int ini = Environment.TickCount;
                
            using (StreamReader reader = new StreamReader(_themeFile))
            {
                IRawTheme result = ThemeReader.ReadThemeSync(reader);
                Console.WriteLine("Loaded {0} in {1}ms.",
                    Path.GetFileName(_themeFile),
                    Environment.TickCount - ini);
                return result;
            }
        }
    }
}