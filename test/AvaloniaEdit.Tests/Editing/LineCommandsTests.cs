using Avalonia.Headless.NUnit;
using AvaloniaEdit.Document;
using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace AvaloniaEdit.Editing
{
    [TestFixture]
    public class LineCommandsTests
    {
        private static TextArea CreateTextArea(string text, int caretOffset)
        {
            var textArea = new TextArea();
            textArea.Document = new TextDocument(text);
            textArea.Caret.Offset = caretOffset;
            return textArea;
        }

        private static void Select(TextArea textArea, int startOffset, int endOffset)
        {
            textArea.Selection = Selection.Create(textArea, startOffset, endOffset);
            textArea.Caret.Offset = endOffset;
        }

        private static void Execute(RoutedCommand command, TextArea textArea)
        {
            command.Execute(null, textArea);
        }

        [AvaloniaTest]
        public void Move_Lines_Down_Should_Swap_The_Caret_Line_With_The_Next_Line()
        {
            var textArea = CreateTextArea("one\ntwo\nthree", 1);

            Execute(AvaloniaEditCommands.MoveLinesDown, textArea);

            Assert.AreEqual("two\none\nthree", textArea.Document.Text);
            Assert.AreEqual(5, textArea.Caret.Offset);
            Assert.IsTrue(textArea.Selection.IsEmpty);
        }

        [AvaloniaTest]
        public void Move_Lines_Down_Should_Move_Onto_The_Last_Line_Without_A_Newline()
        {
            var textArea = CreateTextArea("one\ntwo", 0);

            Execute(AvaloniaEditCommands.MoveLinesDown, textArea);

            Assert.AreEqual("two\none", textArea.Document.Text);
            Assert.AreEqual(4, textArea.Caret.Offset);
        }

        [AvaloniaTest]
        public void Move_Lines_Down_Should_Do_Nothing_On_The_Last_Line()
        {
            var textArea = CreateTextArea("one\ntwo", 5);

            Execute(AvaloniaEditCommands.MoveLinesDown, textArea);

            Assert.AreEqual("one\ntwo", textArea.Document.Text);
            Assert.AreEqual(5, textArea.Caret.Offset);
            Assert.IsFalse(textArea.Document.UndoStack.CanUndo);
        }

        [AvaloniaTest]
        public void Move_Lines_Up_Should_Swap_The_Caret_Line_With_The_Previous_Line()
        {
            var textArea = CreateTextArea("one\ntwo\nthree", 6);

            Execute(AvaloniaEditCommands.MoveLinesUp, textArea);

            Assert.AreEqual("two\none\nthree", textArea.Document.Text);
            Assert.AreEqual(2, textArea.Caret.Offset);
        }

        [AvaloniaTest]
        public void Move_Lines_Up_Should_Move_The_Last_Line_Without_A_Newline()
        {
            var textArea = CreateTextArea("one\ntwo", 7);

            Execute(AvaloniaEditCommands.MoveLinesUp, textArea);

            Assert.AreEqual("two\none", textArea.Document.Text);
            Assert.AreEqual(3, textArea.Caret.Offset);
        }

        [AvaloniaTest]
        public void Move_Lines_Up_Should_Do_Nothing_On_The_First_Line()
        {
            var textArea = CreateTextArea("one\ntwo", 2);

            Execute(AvaloniaEditCommands.MoveLinesUp, textArea);

            Assert.AreEqual("one\ntwo", textArea.Document.Text);
            Assert.AreEqual(2, textArea.Caret.Offset);
        }

        [AvaloniaTest]
        public void Move_Lines_Should_Keep_The_Newlines_Of_The_Document()
        {
            var textArea = CreateTextArea("one\r\ntwo\r\nthree", 0);

            Execute(AvaloniaEditCommands.MoveLinesDown, textArea);

            Assert.AreEqual("two\r\none\r\nthree", textArea.Document.Text);
        }

        [AvaloniaTest]
        public void Move_Lines_Down_Should_Move_The_Selected_Lines_And_Keep_The_Selection()
        {
            var textArea = CreateTextArea("1\n2\n3\n4", 0);
            Select(textArea, 0, 3); // "1\n2"

            Execute(AvaloniaEditCommands.MoveLinesDown, textArea);

            Assert.AreEqual("3\n1\n2\n4", textArea.Document.Text);
            Assert.AreEqual(2, textArea.Selection.SurroundingSegment.Offset);
            Assert.AreEqual(3, textArea.Selection.SurroundingSegment.Length);
            Assert.AreEqual(5, textArea.Caret.Offset);
        }

        [AvaloniaTest]
        public void Move_Lines_Up_Should_Move_The_Selected_Lines_And_Keep_The_Selection()
        {
            var textArea = CreateTextArea("1\n2\n3\n4", 0);
            Select(textArea, 2, 5); // "2\n3"

            Execute(AvaloniaEditCommands.MoveLinesUp, textArea);

            Assert.AreEqual("2\n3\n1\n4", textArea.Document.Text);
            Assert.AreEqual(0, textArea.Selection.SurroundingSegment.Offset);
            Assert.AreEqual(3, textArea.Selection.SurroundingSegment.Length);
        }

        [AvaloniaTest]
        public void Move_Lines_Should_Not_Include_A_Last_Line_With_Nothing_Selected_On_It()
        {
            var textArea = CreateTextArea("1\n2\n3", 0);
            Select(textArea, 0, 2); // "1\n", ending at the start of line 2

            Execute(AvaloniaEditCommands.MoveLinesDown, textArea);

            Assert.AreEqual("2\n1\n3", textArea.Document.Text);
        }

        [AvaloniaTest]
        public void Move_Lines_Should_Be_Undone_In_One_Step()
        {
            var textArea = CreateTextArea("one\ntwo\nthree", 0);

            Execute(AvaloniaEditCommands.MoveLinesDown, textArea);
            Execute(AvaloniaEditCommands.MoveLinesDown, textArea);
            textArea.Document.UndoStack.Undo();

            Assert.AreEqual("two\none\nthree", textArea.Document.Text);
        }

        [AvaloniaTest]
        public void Move_Lines_Should_Do_Nothing_In_A_Read_Only_Document()
        {
            var textArea = CreateTextArea("one\ntwo", 0);
            textArea.ReadOnlySectionProvider = ReadOnlySectionDocument.Instance;

            Execute(AvaloniaEditCommands.MoveLinesDown, textArea);

            Assert.AreEqual("one\ntwo", textArea.Document.Text);
        }

        [AvaloniaTest]
        public void Duplicate_Lines_Should_Copy_The_Caret_Line_Below_And_Move_The_Caret_To_The_Copy()
        {
            var textArea = CreateTextArea("one\ntwo", 1);

            Execute(AvaloniaEditCommands.DuplicateLines, textArea);

            Assert.AreEqual("one\none\ntwo", textArea.Document.Text);
            Assert.AreEqual(5, textArea.Caret.Offset);
        }

        [AvaloniaTest]
        public void Duplicate_Lines_Should_Copy_The_Last_Line_Without_A_Newline()
        {
            var textArea = CreateTextArea("one\ntwo", 7);

            Execute(AvaloniaEditCommands.DuplicateLines, textArea);

            Assert.AreEqual("one\ntwo\ntwo", textArea.Document.Text);
            Assert.AreEqual(11, textArea.Caret.Offset);
        }

        [AvaloniaTest]
        public void Duplicate_Lines_Should_Use_The_Documents_Newline_On_The_Last_Line()
        {
            var textArea = CreateTextArea("one\r\ntwo", 8);

            Execute(AvaloniaEditCommands.DuplicateLines, textArea);

            Assert.AreEqual("one\r\ntwo\r\ntwo", textArea.Document.Text);
        }

        [AvaloniaTest]
        public void Duplicate_Lines_Should_Copy_The_Selected_Lines_And_Select_The_Copy()
        {
            var textArea = CreateTextArea("1\n2\n3", 0);
            Select(textArea, 0, 3); // "1\n2"

            Execute(AvaloniaEditCommands.DuplicateLines, textArea);

            Assert.AreEqual("1\n2\n1\n2\n3", textArea.Document.Text);
            Assert.AreEqual(4, textArea.Selection.SurroundingSegment.Offset);
            Assert.AreEqual(3, textArea.Selection.SurroundingSegment.Length);
        }

        [AvaloniaTest]
        public void Duplicate_Lines_Should_Copy_An_Empty_Line()
        {
            var textArea = CreateTextArea("one\n\ntwo", 4);

            Execute(AvaloniaEditCommands.DuplicateLines, textArea);

            Assert.AreEqual("one\n\n\ntwo", textArea.Document.Text);
            Assert.AreEqual(5, textArea.Caret.Offset);
        }

        [AvaloniaTest]
        public void Duplicate_Lines_Should_Be_Undone_In_One_Step()
        {
            var textArea = CreateTextArea("1\n2\n3", 0);
            Select(textArea, 0, 3);

            Execute(AvaloniaEditCommands.DuplicateLines, textArea);
            textArea.Document.UndoStack.Undo();

            Assert.AreEqual("1\n2\n3", textArea.Document.Text);
        }

        [AvaloniaTest]
        public void Duplicate_Lines_Should_Do_Nothing_In_A_Read_Only_Document()
        {
            var textArea = CreateTextArea("one\ntwo", 0);
            textArea.ReadOnlySectionProvider = ReadOnlySectionDocument.Instance;

            Execute(AvaloniaEditCommands.DuplicateLines, textArea);

            Assert.AreEqual("one\ntwo", textArea.Document.Text);
        }

        [Test]
        public void Line_Commands_Should_Have_Default_Shortcuts()
        {
            Assert.AreEqual(new Avalonia.Input.KeyGesture(Avalonia.Input.Key.Up, Avalonia.Input.KeyModifiers.Alt), AvaloniaEditCommands.MoveLinesUp.Gesture);
            Assert.AreEqual(new Avalonia.Input.KeyGesture(Avalonia.Input.Key.Down, Avalonia.Input.KeyModifiers.Alt), AvaloniaEditCommands.MoveLinesDown.Gesture);
            Assert.AreEqual(new Avalonia.Input.KeyGesture(Avalonia.Input.Key.D, Avalonia.Input.KeyModifiers.Control | Avalonia.Input.KeyModifiers.Shift), AvaloniaEditCommands.DuplicateLines.Gesture);
        }
    }
}
