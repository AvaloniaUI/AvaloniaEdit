using TextMateSharp.Grammars;

namespace AvaloniaEdit.Demo.ViewModels;

public class ThemeViewModel
{
    private ThemeName _themeName;

    public ThemeName ThemeName => _themeName;

    public string DisplayName => _themeName.ToString();
    public ThemeViewModel(ThemeName themeName)
    {
        _themeName = themeName;
    }
}