using System.Collections.Generic;
using TextMateSharp.Themes;

namespace AvaloniaEdit.TextMate.Storage.Abstractions
{
    public interface IThemeStorage
    {
        /// <summary>
        /// Dictionary where the key represents scope name and value is IRawTheme.
        /// </summary>
        public Dictionary<string, IRawTheme> Themes { get; }

        public IRawTheme SelectedTheme { get; set; }
    }
}
