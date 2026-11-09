using System;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using AvaloniaEdit.AvaloniaMocks;
using AvaloniaEdit.Document;
using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace AvaloniaEdit.Editing
{
    [TestFixture]
    public class CaretNavigationTests
    {
        private static TextEditor CreateEditor(string text)
        {
            UnitTestApplication.InitializeStyles();

            var textEditor = new TextEditor { Text = text };
            var window = new Window { Width = 400, Height = 300, Content = textEditor };
            window.Show();
            textEditor.ApplyTemplate();
            textEditor.TextArea.TextView.EnsureVisualLines();
            return textEditor;
        }

        private static TextLocation Move(TextEditor textEditor, TextLocation from, CaretMovementType direction)
        {
            double desiredXPos = double.NaN;
            return Move(textEditor, from, direction, ref desiredXPos);
        }

        private static TextLocation Move(TextEditor textEditor, TextLocation from, CaretMovementType direction, ref double desiredXPos)
        {
            var position = CaretNavigation.GetNewCaretPosition(textEditor.TextArea.TextView, new TextViewPosition(from),
                direction, false, ref desiredXPos);
            return position.Location;
        }

        [AvaloniaTest]
        public void Char_Right_Should_Move_One_Character()
        {
            var textEditor = CreateEditor("hello world");

            Assert.AreEqual(new TextLocation(1, 2), Move(textEditor, new TextLocation(1, 1), CaretMovementType.CharRight));
        }

        [AvaloniaTest]
        public void Char_Right_At_The_End_Of_A_Line_Should_Move_To_The_Next_Line()
        {
            var textEditor = CreateEditor("ab\ncd");

            Assert.AreEqual(new TextLocation(2, 1), Move(textEditor, new TextLocation(1, 3), CaretMovementType.CharRight));
        }

        [AvaloniaTest]
        public void Word_Right_Should_Move_To_The_Start_Of_The_Next_Word()
        {
            var textEditor = CreateEditor("hello world");

            Assert.AreEqual(new TextLocation(1, 7), Move(textEditor, new TextLocation(1, 1), CaretMovementType.WordRight));
        }

        [AvaloniaTest]
        public void Word_Left_Should_Move_To_The_Start_Of_The_Previous_Word()
        {
            var textEditor = CreateEditor("hello world");

            Assert.AreEqual(new TextLocation(1, 7), Move(textEditor, new TextLocation(1, 10), CaretMovementType.WordLeft));
        }

        [AvaloniaTest]
        public void Line_Start_Should_Go_To_The_First_Non_Whitespace_Character_Then_To_Column_One()
        {
            var textEditor = CreateEditor("    indented");

            var first = Move(textEditor, new TextLocation(1, 9), CaretMovementType.LineStart);
            Assert.AreEqual(new TextLocation(1, 5), first);
            Assert.AreEqual(new TextLocation(1, 1), Move(textEditor, first, CaretMovementType.LineStart));
        }

        [AvaloniaTest]
        public void Line_End_Should_Go_To_The_End_Of_The_Line()
        {
            var textEditor = CreateEditor("hello world\nnext");

            Assert.AreEqual(new TextLocation(1, 12), Move(textEditor, new TextLocation(1, 3), CaretMovementType.LineEnd));
        }

        [AvaloniaTest]
        public void Line_Down_Should_Keep_The_Desired_X_Position_Through_A_Shorter_Line()
        {
            var textEditor = CreateEditor("abcdef\nab\nabcdef");
            double desiredXPos = double.NaN;

            var middle = Move(textEditor, new TextLocation(1, 6), CaretMovementType.LineDown, ref desiredXPos);
            Assert.AreEqual(new TextLocation(2, 3), middle);
            Assert.IsFalse(double.IsNaN(desiredXPos));

            Assert.AreEqual(new TextLocation(3, 6), Move(textEditor, middle, CaretMovementType.LineDown, ref desiredXPos));
        }

        [AvaloniaTest]
        public void Line_Up_On_The_First_Line_Should_Stay_On_It()
        {
            var textEditor = CreateEditor("one\ntwo");

            Assert.AreEqual(1, Move(textEditor, new TextLocation(1, 3), CaretMovementType.LineUp).Line);
        }

        [AvaloniaTest]
        public void Document_Start_And_End_Should_Go_To_The_Ends_Of_The_Document()
        {
            var textEditor = CreateEditor("one\ntwo\nthree");

            Assert.AreEqual(new TextLocation(1, 1), Move(textEditor, new TextLocation(2, 2), CaretMovementType.DocumentStart));
            Assert.AreEqual(new TextLocation(3, 6), Move(textEditor, new TextLocation(2, 2), CaretMovementType.DocumentEnd));
        }

        [AvaloniaTest]
        public void None_Should_Not_Move()
        {
            var textEditor = CreateEditor("hello");

            Assert.AreEqual(new TextLocation(1, 3), Move(textEditor, new TextLocation(1, 3), CaretMovementType.None));
        }

        [AvaloniaTest]
        public void Movements_Should_Match_The_Text_Areas_Own_Commands()
        {
            var commands = new (RoutedCommand Command, CaretMovementType Direction)[]
            {
                (EditingCommands.MoveRightByCharacter, CaretMovementType.CharRight),
                (EditingCommands.MoveLeftByCharacter, CaretMovementType.CharLeft),
                (EditingCommands.MoveRightByWord, CaretMovementType.WordRight),
                (EditingCommands.MoveLeftByWord, CaretMovementType.WordLeft),
                (EditingCommands.MoveDownByLine, CaretMovementType.LineDown),
                (EditingCommands.MoveUpByLine, CaretMovementType.LineUp),
                (EditingCommands.MoveToLineStart, CaretMovementType.LineStart),
                (EditingCommands.MoveToLineEnd, CaretMovementType.LineEnd),
            };
            foreach (var (command, direction) in commands)
            {
                var textEditor = CreateEditor("SELECT o.OrderId, o.Total\n    FROM dbo.Orders o\nWHERE o.Total > 10");
                var from = new TextLocation(2, 9);
                textEditor.TextArea.Caret.Location = from;

                var expected = Move(textEditor, from, direction);
                command.Execute(null, textEditor.TextArea);

                Assert.AreEqual(expected, textEditor.TextArea.Caret.Location, direction.ToString());
            }
        }

        [Test]
        public void A_Null_Text_View_Should_Throw()
        {
            double desiredXPos = double.NaN;
            Assert.Throws<ArgumentNullException>(() =>
                CaretNavigation.GetNewCaretPosition(null, new TextViewPosition(1, 1), CaretMovementType.CharRight, false, ref desiredXPos));
        }
    }
}
