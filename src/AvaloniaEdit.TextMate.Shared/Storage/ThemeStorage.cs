using AvaloniaEdit.TextMate.Storage.Abstractions;
using System.Collections.Generic;
using TextMateSharp.Themes;

namespace AvaloniaEdit.TextMate.Storage
{
    public class ThemeStorage : IThemeStorage
    {
        public ThemeStorage(Dictionary<string, IRawTheme> themes, IRawTheme selectedTheme)
        {
            Themes = themes;
            SelectedTheme = selectedTheme;
        }
        public Dictionary<string, IRawTheme> Themes { get; set; }
        public IRawTheme SelectedTheme { get; set; }
    }
}
