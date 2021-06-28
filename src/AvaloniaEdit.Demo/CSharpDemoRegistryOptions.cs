using System;
using System.Collections.Generic;
using System.IO;
using TextMateSharp.Internal.Themes.Reader;
using TextMateSharp.Registry;
using TextMateSharp.Themes;

namespace AvaloniaEdit.Demo
{
    class CSharpDemoRegistryOptions : IRegistryOptions
    {
        private string _csharpGrammarFile;
        private string _darkPlusThemeFile;
        private string _darkVsThemeFile;
        internal CSharpDemoRegistryOptions(
            string csharpGrammarFile,
            string darkPlusThemeFile,
            string darkVsThemeFile)
        {
            _csharpGrammarFile = csharpGrammarFile;
            _darkPlusThemeFile = darkPlusThemeFile;
            _darkVsThemeFile = darkVsThemeFile;
        }
        public string GetFilePath(string scopeName)
        {
            if (scopeName.Contains("dark_vs.json"))
                return _darkVsThemeFile;
            return _csharpGrammarFile;
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
            using (StreamReader reader = new StreamReader(_darkPlusThemeFile))
            {
                IRawTheme result = ThemeReader.ReadThemeSync(reader);
                Console.WriteLine("Loaded {0} in {1}ms.",
                    Path.GetFileName(_darkPlusThemeFile),
                    Environment.TickCount - ini);
                return result;
            }
        }
    }
}