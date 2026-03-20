using System.Collections.ObjectModel;
using AvaloniaEdit.Editing;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TextMateSharp.Grammars;

namespace AvaloniaEdit.Demo.ViewModels;

public partial class MainWindowViewModel(TextMate.TextMate.Installation _textMateInstallation, RegistryOptions _registryOptions) : ObservableObject
{
    public ObservableCollection<ThemeViewModel> AllThemes { get; set; } = [];

    public ThemeViewModel SelectedTheme
    {
        get;
        set
        {
            SetProperty(ref field, value);
            _textMateInstallation.SetTheme(_registryOptions.LoadTheme(value.ThemeName));
        }
    }

    [RelayCommand]
    private void CopyMouse(TextArea textArea)
    {
        ApplicationCommands.Copy.Execute(null, textArea);
    }

    [RelayCommand]
    private void CutMouse(TextArea textArea)
    {
        ApplicationCommands.Cut.Execute(null, textArea);
    }

    [RelayCommand]
    private void PasteMouse(TextArea textArea)
    {
        ApplicationCommands.Paste.Execute(null, textArea);
    }

    [RelayCommand]
    private void SelectAllMouse(TextArea textArea)
    {
        ApplicationCommands.SelectAll.Execute(null, textArea);
    }

    [RelayCommand]
    private void UndoMouse(TextArea textArea)
    {
        ApplicationCommands.Undo.Execute(null, textArea);
    }
}