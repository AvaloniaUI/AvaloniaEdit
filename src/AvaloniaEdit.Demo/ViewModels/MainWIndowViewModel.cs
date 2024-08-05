using System.Collections.ObjectModel;
using AvaloniaEdit.Editing;
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

    public void CopyMouseCommmand(TextArea textArea)
    {
        ApplicationCommands.Copy.Execute(null, textArea);
    }

    public void CutMouseCommand(TextArea textArea)
    {
        ApplicationCommands.Cut.Execute(null, textArea);
    }
    
    public void PasteMouseCommmand(TextArea textArea)
    {
        ApplicationCommands.Paste.Execute(null, textArea);
    }

    public void SelectAllMouseCommmand(TextArea textArea)
    {
        ApplicationCommands.SelectAll.Execute(null, textArea);
    }

    // Undo Status is not given back to disable it's item in ContextFlyout; therefore it's not being used yet.
    public void UndoMouseCommmand(TextArea textArea)
    {
        ApplicationCommands.Undo.Execute(null, textArea);
    }
}