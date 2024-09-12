using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit.Utils;

namespace AvaloniaEdit.Document;

public class DataObjectCopyingEventArgs(IDataObject dataObject, bool isDragDrop) :
    RoutedEventArgs(DataObjectEx.DataObjectCopyingEvent)
{
    public bool CommandCancelled { get; private set; }
    public IDataObject DataObject { get; } = dataObject;
    public bool IsDragDrop { get; } = isDragDrop;
    public void CancelCommand() => CommandCancelled = true;
}
