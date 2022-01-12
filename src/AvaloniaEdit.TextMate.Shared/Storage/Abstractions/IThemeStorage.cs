using System.Collections.Generic;
using TextMateSharp.Themes;

namespace AvaloniaEdit.TextMate.Storage.Abstractions
{
    public interface IThemeStorage
    {
        public Dictionary<string, IRawTheme> Themes { get; }

        public IRawTheme SelectedTheme { get; set; }
    }
}
