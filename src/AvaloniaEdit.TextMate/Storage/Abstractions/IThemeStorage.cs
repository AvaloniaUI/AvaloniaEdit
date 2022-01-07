using System;
using System.Collections.Generic;
using System.Text;
using TextMateSharp.Themes;

namespace AvaloniaEdit.TextMate.Storage.Abstractions
{
    public interface IThemeStorage
    {
        public Dictionary<string, IRawTheme> Themes { get; set; }

        public IRawTheme SelectedTheme { get; set; }
    }
}
