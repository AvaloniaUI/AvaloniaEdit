using System;
using Avalonia.Controls.Shapes;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.TextMate;

using NUnit.Framework;
using TextMateSharp.Grammars;

namespace AvaloniaEdit.Tests.TextMate
{
    [TestFixture]
    internal class TextEditorModelTests
    {
        [Test]
        public void Lines_Should_Have_Valid_Count()
        {
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\npussy\nbirdie";

            Assert.AreEqual(3, textEditorModel.GetNumberOfLines());
        }

        [Test]
        public void Lines_Should_Have_Valid_Content()
        {
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\npussy\nbirdie";

            AssertLinesAreEqual("puppy\n", textEditorModel.GetLineTextIncludingTerminators(0));
            AssertLinesAreEqual("pussy\n", textEditorModel.GetLineTextIncludingTerminators(1));
            AssertLinesAreEqual("birdie", textEditorModel.GetLineTextIncludingTerminators(2));
        }

        [Test]
        public void Editing_Line_Should_Update_The_Line_Content()
        {
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\npussy\nbirdie";

            document.Insert(0, "cutty ");

            AssertLinesAreEqual("cutty puppy\n", textEditorModel.GetLineTextIncludingTerminators(0));
            AssertLinesAreEqual("pussy\n", textEditorModel.GetLineTextIncludingTerminators(1));
            AssertLinesAreEqual("birdie", textEditorModel.GetLineTextIncludingTerminators(2));
        }

        [Test]
        public void Editing_Line_Should_Update_The_Line_Length()
        {
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\npussy\nbirdie";

            document.Insert(0, "cutty ");

            Assert.AreEqual("cutty puppy".Length, textEditorModel.GetLineLength(0));
        }

        [Test]
        public void Inserting_Line_Should_Update_The_Line_Ranges()
        {
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\npussy\nbirdie";

            document.Insert(0, "lion\n");

            AssertLinesAreEqual("lion\n", textEditorModel.GetLineTextIncludingTerminators(0));
            AssertLinesAreEqual("puppy\n", textEditorModel.GetLineTextIncludingTerminators(1));
            AssertLinesAreEqual("pussy\n", textEditorModel.GetLineTextIncludingTerminators(2));
            AssertLinesAreEqual("birdie", textEditorModel.GetLineTextIncludingTerminators(3));
        }

        [Test]
        public void Removing_Line_Should_Update_The_Line_Ranges()
        {
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\npussy\nbirdie";

            document.Remove(
                document.Lines[0].Offset,
                document.Lines[0].TotalLength);

            AssertLinesAreEqual("pussy\n", textEditorModel.GetLineTextIncludingTerminators(0));
            AssertLinesAreEqual("birdie", textEditorModel.GetLineTextIncludingTerminators(1));
        }

        [Test]
        public void Removing_Line_With_LFCR_Should_Update_The_Line_Ranges()
        {
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\r\npussy\r\nbirdie";

            document.Remove(
                document.Lines[0].Offset,
                document.Lines[0].TotalLength);

            AssertLinesAreEqual("pussy\r\n", textEditorModel.GetLineTextIncludingTerminators(0));
            AssertLinesAreEqual("birdie", textEditorModel.GetLineTextIncludingTerminators(1));
        }

        [Test]
        public void Document_Lines_Count_Should_Match_Model_Lines_Count()
        {
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\npussy\nbirdie";

            int count = 0;
            textEditorModel.ForEach((m) => count++);

            Assert.AreEqual(document.LineCount, count);
        }

        [Test]
        public void Edit_Document_Line_Should_Not_Add_Or_Remove_Model_Lines()
        {
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\npussy\nbirdie";

            document.Insert(0, "cutty ");

            int count = 0;
            textEditorModel.ForEach((m) => count++);

            Assert.AreEqual(document.LineCount, count);
        }

        [Test]
        public void Insert_Document_Line_Should_Insert_Model_Line()
        {
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\npussy\nbirdie";

            document.Insert(0, "lion\n");

            int count = 0;
            textEditorModel.ForEach((m) => count++);

            Assert.AreEqual(document.LineCount, count);
        }

        [Test]
        public void Remove_Document_Line_Should_Remove_Model_Line()
        {
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\npussy\nbirdie";

            document.Remove(
                document.Lines[0].Offset,
                document.Lines[0].TotalLength);

            int count = 0;
            textEditorModel.ForEach((m) => count++);

            Assert.AreEqual(document.LineCount, count);
        }

        [Test]
        public void Replace_Text_Of_Same_Length_Should_Update_Line_Content()
        {
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\npussy\nbirdie";

            document.Replace(0, 1, "P");

            AssertLinesAreEqual("Puppy\n", textEditorModel.GetLineTextIncludingTerminators(0));
        }

        [Test]
        public void Replace_Text_Of_Same_Length_With_CR_Should_Update_Line_Content()
        {
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\npussy\nbirdie";

            document.Replace(0, 1, "\n");

            AssertLinesAreEqual("\n", textEditorModel.GetLineTextIncludingTerminators(0));
        }

        [Test]
        public void Remove_Document_Text_Should_Update_Line_Contents()
        {
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\npussy\nbirdie";
            Assert.AreEqual(3, textEditorModel.GetNumberOfLines());

            document.Text = string.Empty;
            Assert.AreEqual(1, textEditorModel.GetNumberOfLines());
            AssertLinesAreEqual(string.Empty, textEditorModel.GetLineTextIncludingTerminators(0));
        }

        [Test]
        public void Replace_Document_Text_Should_Update_Line_Contents()
        {
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\npussy\nbirdie";
            Assert.AreEqual(3, textEditorModel.GetNumberOfLines());

            document.Text = "one\ntwo\nthree\nfour";
            Assert.AreEqual(4, textEditorModel.GetNumberOfLines());

            AssertLinesAreEqual("one\n", textEditorModel.GetLineTextIncludingTerminators(0));
            AssertLinesAreEqual("two\n", textEditorModel.GetLineTextIncludingTerminators(1));
            AssertLinesAreEqual("three\n", textEditorModel.GetLineTextIncludingTerminators(2));
            AssertLinesAreEqual("four", textEditorModel.GetLineTextIncludingTerminators(3));
        }

        [Test]
        public void Batch_Document_Changes_Should_Invalidate_Lines()
        {
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\npuppy\npuppy";

            document.BeginUpdate();

            document.Insert(0, "*");
            Assert.AreEqual(0, textEditorModel.InvalidRange.StartLine,
                "Wrong InvalidRange.StartLine 1");
            Assert.AreEqual(0, textEditorModel.InvalidRange.EndLine,
                "Wrong InvalidRange.EndLine 1");

            document.Insert(7, "*");
            Assert.AreEqual(0, textEditorModel.InvalidRange.StartLine,
                "Wrong InvalidRange.StartLine 2");
            Assert.AreEqual(1, textEditorModel.InvalidRange.EndLine,
                "Wrong InvalidRange.EndLine 2");

            document.Insert(14, "*");
            Assert.AreEqual(0, textEditorModel.InvalidRange.StartLine,
                "Wrong InvalidRange.StartLine 3");
            Assert.AreEqual(2, textEditorModel.InvalidRange.EndLine,
                "Wrong InvalidRange.EndLine 3");

            document.EndUpdate();
            Assert.IsNull(textEditorModel.InvalidRange,
                "InvalidRange should be null");

            AssertLinesAreEqual("*puppy\n", textEditorModel.GetLineTextIncludingTerminators(0));
            AssertLinesAreEqual("*puppy\n", textEditorModel.GetLineTextIncludingTerminators(1));
            AssertLinesAreEqual("*puppy", textEditorModel.GetLineTextIncludingTerminators(2));
        }

        [Test]
        public void Nested_Batch_Document_Changes_Should_Invalidate_Lines()
        {
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\npuppy\npuppy";

            document.BeginUpdate();

            document.Insert(0, "*");
            Assert.AreEqual(0, textEditorModel.InvalidRange.StartLine,
                "Wrong InvalidRange.StartLine 1");
            Assert.AreEqual(0, textEditorModel.InvalidRange.EndLine,
                "Wrong InvalidRange.EndLine 1");

            document.BeginUpdate();
            document.Insert(7, "*");
            Assert.AreEqual(0, textEditorModel.InvalidRange.StartLine,
                "Wrong InvalidRange.StartLine 2");
            Assert.AreEqual(1, textEditorModel.InvalidRange.EndLine,
                "Wrong InvalidRange.EndLine 2");

            document.Insert(14, "*");
            Assert.AreEqual(0, textEditorModel.InvalidRange.StartLine,
                "Wrong InvalidRange.StartLine 3");
            Assert.AreEqual(2, textEditorModel.InvalidRange.EndLine,
                "Wrong InvalidRange.EndLine 3");

            document.EndUpdate();
            Assert.IsNotNull(textEditorModel.InvalidRange,
                "InvalidRange should not be null");
            document.EndUpdate();
            Assert.IsNull(textEditorModel.InvalidRange,
                "InvalidRange should be null");

            AssertLinesAreEqual("*puppy\n", textEditorModel.GetLineTextIncludingTerminators(0));
            AssertLinesAreEqual("*puppy\n", textEditorModel.GetLineTextIncludingTerminators(1));
            AssertLinesAreEqual("*puppy", textEditorModel.GetLineTextIncludingTerminators(2));
        }

        private static void AssertLinesAreEqual(LineText expected, LineText actual)
        {
            Assert.AreEqual(expected, actual);
        }
    }
}
