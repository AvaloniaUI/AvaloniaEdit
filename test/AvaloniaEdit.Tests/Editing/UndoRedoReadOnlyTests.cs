using Avalonia.Headless.NUnit;
using AvaloniaEdit.Document;
using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace AvaloniaEdit.Editing
{
    [TestFixture]
    public class UndoRedoReadOnlyTests
    {
        private static TextArea CreateReadOnlyTextArea(out TextDocument document)
        {
            document = new TextDocument("a");
            var textArea = new TextArea { Document = document };
            document.Insert(document.TextLength, "b");
            document.Insert(document.TextLength, "c");
            textArea.ReadOnlySectionProvider = ReadOnlySectionDocument.Instance;
            return textArea;
        }

        [AvaloniaTest]
        public void UndoDoesNotModifyReadOnlyDocument()
        {
            var textArea = CreateReadOnlyTextArea(out var document);
            var text = document.Text;

            Assert.IsTrue(textArea.IsReadOnly);
            Assert.IsFalse(ApplicationCommands.Undo.CanExecute(null, textArea));
            ApplicationCommands.Undo.Execute(null, textArea);

            Assert.AreEqual(text, document.Text);
        }

        [AvaloniaTest]
        public void RedoDoesNotModifyReadOnlyDocument()
        {
            var textArea = CreateReadOnlyTextArea(out var document);
            textArea.ReadOnlySectionProvider = NoReadOnlySections.Instance;
            document.UndoStack.Undo();
            var text = document.Text;
            textArea.ReadOnlySectionProvider = ReadOnlySectionDocument.Instance;

            Assert.IsFalse(ApplicationCommands.Redo.CanExecute(null, textArea));
            ApplicationCommands.Redo.Execute(null, textArea);

            Assert.AreEqual(text, document.Text);
        }

        [AvaloniaTest]
        public void UndoStillWorksWhenNotReadOnly()
        {
            var textArea = CreateReadOnlyTextArea(out var document);
            textArea.ReadOnlySectionProvider = NoReadOnlySections.Instance;

            Assert.IsTrue(ApplicationCommands.Undo.CanExecute(null, textArea));
            ApplicationCommands.Undo.Execute(null, textArea);

            Assert.AreEqual("ab", document.Text);
        }
    }
}
