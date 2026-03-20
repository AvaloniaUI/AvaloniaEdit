using System.Collections.ObjectModel;
using AvaloniaEdit.Editing;
using ReactiveUI;
using TextMateSharp.Grammars;

namespace AvaloniaEdit.Demo.ViewModels;

public class MainWindowViewModel(TextMate.TextMate.Installation _textMateInstallation, RegistryOptions _registryOptions) : ReactiveObject
{
    public ObservableCollection<ThemeViewModel> AllThemes { get; set; } = [];

    public ThemeViewModel SelectedTheme
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            _textMateInstallation.SetTheme(_registryOptions.LoadTheme(value.ThemeName));
        }
    }

    public void CopyMouseCommand(TextArea textArea)
    {
        ApplicationCommands.Copy.Execute(null, textArea);
    }

    public void CutMouseCommand(TextArea textArea)
    {
        ApplicationCommands.Cut.Execute(null, textArea);
    }
    
    public void PasteMouseCommand(TextArea textArea)
    {
        ApplicationCommands.Paste.Execute(null, textArea);
    }

    public void SelectAllMouseCommand(TextArea textArea)
    {
        ApplicationCommands.SelectAll.Execute(null, textArea);
    }

    // Undo Status is not given back to disable it's item in ContextFlyout; therefore it's not being used yet.
    public void UndoMouseCommand(TextArea textArea)
    {
        ApplicationCommands.Undo.Execute(null, textArea);
    }
}