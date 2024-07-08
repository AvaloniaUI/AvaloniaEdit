using System.Collections.ObjectModel;
using ReactiveUI;
using TextMateSharp.Grammars;

namespace AvaloniaEdit.Demo.ViewModels;

public class MainWindowViewModel(TextMate.TextMate.Installation _textMateInstallation, RegistryOptions _registryOptions) : ReactiveObject
{
    public ObservableCollection<ThemeViewModel> AllThemes { get; set; } = [];
    private ThemeViewModel _selectedTheme;

    public ThemeViewModel SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedTheme, value);
            _textMateInstallation.SetTheme(_registryOptions.LoadTheme(value.ThemeName));
        }
    }
}