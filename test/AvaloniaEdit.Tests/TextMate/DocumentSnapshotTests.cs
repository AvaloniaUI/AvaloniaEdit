using System;
using AvaloniaEdit.Document;
using AvaloniaEdit.TextMate;

using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace AvaloniaEdit.Tests.TextMate
{
    [TestFixture]
    internal class DocumentSnapshotTests
    {
        [Test]
        public void DocumentSnaphot_Should_Have_CorrectLineText()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\npussy\nbirdie";

            DocumentSnapshot documentSnaphot = new DocumentSnapshot(document);

            Assert.AreEqual("puppy", documentSnaphot.GetLineText(0));
            Assert.AreEqual("pussy", documentSnaphot.GetLineText(1));
            Assert.AreEqual("birdie", documentSnaphot.GetLineText(2));
        }

        [Test]
        public void DocumentSnaphot_Should_Have_CorrectLineText_Including_Terminator()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\npussy\nbirdie";

            DocumentSnapshot documentSnaphot = new DocumentSnapshot(document);

            Assert.AreEqual("puppy\n", documentSnaphot.GetLineTextIncludingTerminator(0));
            Assert.AreEqual("pussy\n", documentSnaphot.GetLineTextIncludingTerminator(1));
            Assert.AreEqual("birdie", documentSnaphot.GetLineTextIncludingTerminator(2));
        }

        [Test]
        public void DocumentSnaphot_Should_Have_Correct_Line_Text_Including_Terminator_CR_LF()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\r\npussy\r\nbirdie";

            DocumentSnapshot documentSnaphot = new DocumentSnapshot(document);

            Assert.AreEqual("puppy\r\n", documentSnaphot.GetLineTextIncludingTerminator(0));
            Assert.AreEqual("pussy\r\n", documentSnaphot.GetLineTextIncludingTerminator(1));
            Assert.AreEqual("birdie", documentSnaphot.GetLineTextIncludingTerminator(2));
        }

        [Test]
        public void DocumentSnaphot_Should_Have_Correct_Line_Terminators()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\r\npussy\r\nbirdie";

            DocumentSnapshot documentSnaphot = new DocumentSnapshot(document);

            Assert.AreEqual("\r\n", documentSnaphot.GetLineTerminator(0));
            Assert.AreEqual("\r\n", documentSnaphot.GetLineTerminator(1));
            Assert.AreEqual("", documentSnaphot.GetLineTerminator(2));
        }

        [Test]
        public void DocumentSnaphot_Should_Have_Correct_Line_Len()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\r\npussy\r\nbirdie";

            DocumentSnapshot documentSnaphot = new DocumentSnapshot(document);

            Assert.AreEqual(5, documentSnaphot.GetLineLength(0));
            Assert.AreEqual(5, documentSnaphot.GetLineLength(1));
            Assert.AreEqual(6, documentSnaphot.GetLineLength(2));
        }

        [Test]
        public void DocumentSnaphot_Should_Have_Correct_Line_Total_Len()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\r\npussy\r\nbirdie";

            DocumentSnapshot documentSnaphot = new DocumentSnapshot(document);

            Assert.AreEqual(7, documentSnaphot.GetTotalLineLength(0));
            Assert.AreEqual(7, documentSnaphot.GetTotalLineLength(1));
            Assert.AreEqual(6, documentSnaphot.GetTotalLineLength(2));
        }

        [Test]
        public void DocumentSnapshot_Should_Have_Correct_Line_Count_Before_Remove_All_Lines()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\r\npussy\r\nbirdie\r\ndoggie";

            DocumentSnapshot documentSnaphot = new DocumentSnapshot(document);

            documentSnaphot.RemoveLines(0, 3);

            Assert.AreEqual(0, documentSnaphot.LineCount);
        }

        [Test]
        public void DocumentSnapshot_Should_Have_Correct_Line_Count_Before_Remove_First_Line()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\r\npussy\r\nbirdie\r\ndoggie";

            DocumentSnapshot documentSnaphot = new DocumentSnapshot(document);

            documentSnaphot.RemoveLines(0, 0);

            Assert.AreEqual(3, documentSnaphot.LineCount);
        }

        [Test]
        public void DocumentSnapshot_Should_Have_Correct_Line_Count_Before_Remove_Two_First_Lines()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\r\npussy\r\nbirdie\r\ndoggie";

            DocumentSnapshot documentSnaphot = new DocumentSnapshot(document);

            documentSnaphot.RemoveLines(0, 1);

            Assert.AreEqual(2, documentSnaphot.LineCount);
        }

        [Test]
        public void DocumentSnapshot_Should_Have_Correct_Line_Count_Before_Remove_Last_Line()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\r\npussy\r\nbirdie\r\ndoggie";

            DocumentSnapshot documentSnaphot = new DocumentSnapshot(document);

            documentSnaphot.RemoveLines(3, 3);

            Assert.AreEqual(3, documentSnaphot.LineCount);
        }

        [Test]
        public void Constructor_ShouldInitializeSnapshotFromDocument()
        {
            // Arrange
            TextDocument document = new TextDocument("alpha\r\nbeta\r\ngamma");
            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            // Act
            int lineCount = snapshot.LineCount;
            string text = snapshot.GetText();
            string firstLine = snapshot.GetLineText(0);
            string secondLine = snapshot.GetLineText(1);
            string thirdLine = snapshot.GetLineText(2);

            // Assert
            Assert.AreEqual(3, lineCount);
            Assert.AreEqual("alpha\r\nbeta\r\ngamma", text);
            Assert.AreEqual("alpha", firstLine);
            Assert.AreEqual("beta", secondLine);
            Assert.AreEqual("gamma", thirdLine);
        }

        [Test]
        public void GetLineMembers_ShouldReturnExpectedValues_ForLineWithTerminator()
        {
            // Arrange
            TextDocument document = new TextDocument("alpha\r\nbeta\r\ngamma");
            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            // Act
            string lineText = snapshot.GetLineText(0);
            string lineTextIncludingTerminator = snapshot.GetLineTextIncludingTerminator(0);
            ReadOnlyMemory<char> lineMemoryIncludingTerminator = snapshot.GetLineTextIncludingTerminatorAsMemory(0);
            string terminator = snapshot.GetLineTerminator(0);
            int lineLength = snapshot.GetLineLength(0);
            int totalLineLength = snapshot.GetTotalLineLength(0);

            // Assert
            Assert.AreEqual("alpha", lineText);
            Assert.AreEqual("alpha\r\n", lineTextIncludingTerminator);
            Assert.AreEqual("alpha\r\n", lineMemoryIncludingTerminator.ToString());
            Assert.AreEqual("\r\n", terminator);
            Assert.AreEqual(5, lineLength);
            Assert.AreEqual(7, totalLineLength);
        }

        [Test]
        public void GetLineMembers_ShouldReturnExpectedValues_ForLastLineWithoutTerminator()
        {
            // Arrange
            TextDocument document = new TextDocument("alpha\r\nbeta\r\ngamma");
            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            // Act
            string lineText = snapshot.GetLineText(2);
            string lineTextIncludingTerminator = snapshot.GetLineTextIncludingTerminator(2);
            ReadOnlyMemory<char> lineMemoryIncludingTerminator = snapshot.GetLineTextIncludingTerminatorAsMemory(2);
            string terminator = snapshot.GetLineTerminator(2);
            int lineLength = snapshot.GetLineLength(2);
            int totalLineLength = snapshot.GetTotalLineLength(2);

            // Assert
            Assert.AreEqual("gamma", lineText);
            Assert.AreEqual("gamma", lineTextIncludingTerminator);
            Assert.AreEqual("gamma", lineMemoryIncludingTerminator.ToString());
            Assert.AreEqual(string.Empty, terminator);
            Assert.AreEqual(5, lineLength);
            Assert.AreEqual(5, totalLineLength);
        }

        [Test]
        public void RemoveLines_ShouldRemoveInclusiveRange_AndUpdateLineCount()
        {
            // Arrange
            TextDocument document = new TextDocument("line1\r\nline2\r\nline3\r\nline4");
            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            // Act
            snapshot.RemoveLines(1, 2);

            int lineCount = snapshot.LineCount;
            string firstRemainingLine = snapshot.GetLineText(0);
            string secondRemainingLine = snapshot.GetLineText(1);

            // Assert
            Assert.AreEqual(2, lineCount);
            Assert.AreEqual("line1", firstRemainingLine);
            Assert.AreEqual("line4", secondRemainingLine);
        }

        [Test]
        public void Update_WithNullChangeArgs_ShouldRecomputeAllLineRanges()
        {
            // Arrange
            TextDocument document = new TextDocument("one\r\ntwo");
            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            document.Text = "first\r\nsecond\r\nthird";

            // Act
            snapshot.Update(null);

            int lineCount = snapshot.LineCount;
            string text = snapshot.GetText();
            string firstLine = snapshot.GetLineText(0);
            string secondLine = snapshot.GetLineText(1);
            string thirdLine = snapshot.GetLineText(2);

            // Assert
            Assert.AreEqual(3, lineCount);
            Assert.AreEqual("first\r\nsecond\r\nthird", text);
            Assert.AreEqual("first", firstLine);
            Assert.AreEqual("second", secondLine);
            Assert.AreEqual("third", thirdLine);
        }

        [Test]
        public void Update_WithSingleLineChange_ShouldRecalculateOffsetsUsingOffsetChangeMap()
        {
            // Arrange
            TextDocument document = new TextDocument("ab\r\ncd\r\nef");
            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            DocumentChangeEventArgs change = CaptureInsertChange(document, 1, "XYZ");

            // Act
            snapshot.Update(change);

            int lineCount = snapshot.LineCount;
            string fullText = snapshot.GetText();
            string firstLine = snapshot.GetLineText(0);
            string secondLine = snapshot.GetLineText(1);
            string thirdLine = snapshot.GetLineText(2);
            string secondLineWithTerminator = snapshot.GetLineTextIncludingTerminator(1);
            int firstLineLength = snapshot.GetLineLength(0);
            int secondLineTotalLength = snapshot.GetTotalLineLength(1);

            // Assert
            Assert.AreEqual(3, lineCount);
            Assert.AreEqual("aXYZb\r\ncd\r\nef", fullText);
            Assert.AreEqual("aXYZb", firstLine);
            Assert.AreEqual("cd", secondLine);
            Assert.AreEqual("ef", thirdLine);
            Assert.AreEqual("cd\r\n", secondLineWithTerminator);
            Assert.AreEqual(5, firstLineLength);
            Assert.AreEqual(4, secondLineTotalLength);
        }

        [Test]
        public void Update_WithLineCountChange_ShouldRecomputeAllLineRanges()
        {
            // Arrange
            TextDocument document = new TextDocument("ab\r\ncd");
            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            DocumentChangeEventArgs change = CaptureInsertChange(document, 2, "\r\nxx");

            // Act
            snapshot.Update(change);

            int lineCount = snapshot.LineCount;
            string fullText = snapshot.GetText();
            string firstLine = snapshot.GetLineText(0);
            string secondLine = snapshot.GetLineText(1);
            string thirdLine = snapshot.GetLineText(2);

            // Assert
            Assert.AreEqual(3, lineCount);
            Assert.AreEqual("ab\r\nxx\r\ncd", fullText);
            Assert.AreEqual("ab", firstLine);
            Assert.AreEqual("xx", secondLine);
            Assert.AreEqual("cd", thirdLine);
        }

        [Test]
        public void Update_AfterRemovingNewline_ShouldRecomputeAllLineRanges_WhenLineCountShrinks()
        {
            // Arrange
            TextDocument document = new TextDocument("ab\r\ncd\r\nef");
            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            DocumentChangeEventArgs change = CaptureRemoveChange(document, 2, 2);

            // Act
            snapshot.Update(change);

            int lineCount = snapshot.LineCount;
            string fullText = snapshot.GetText();
            string firstLine = snapshot.GetLineText(0);
            string secondLine = snapshot.GetLineText(1);

            // Assert
            Assert.AreEqual(2, lineCount);
            Assert.AreEqual("abcd\r\nef", fullText);
            Assert.AreEqual("abcd", firstLine);
            Assert.AreEqual("ef", secondLine);
        }

        private static DocumentChangeEventArgs CaptureInsertChange(TextDocument document, int offset, string text)
        {
            ChangeRecorder recorder = new ChangeRecorder();

            document.Changed += recorder.OnDocumentChanged;
            try
            {
                document.Insert(offset, text);
            }
            finally
            {
                document.Changed -= recorder.OnDocumentChanged;
            }

            Assert.IsNotNull(recorder.LastChange);
            return recorder.LastChange;
        }

        private static DocumentChangeEventArgs CaptureRemoveChange(TextDocument document, int offset, int length)
        {
            ChangeRecorder recorder = new ChangeRecorder();

            document.Changed += recorder.OnDocumentChanged;
            try
            {
                document.Remove(offset, length);
            }
            finally
            {
                document.Changed -= recorder.OnDocumentChanged;
            }

            Assert.IsNotNull(recorder.LastChange);
            return recorder.LastChange;
        }

        private sealed class ChangeRecorder
        {
            public DocumentChangeEventArgs LastChange { get; private set; }

            public void OnDocumentChanged(object sender, DocumentChangeEventArgs e)
            {
                LastChange = e;
            }
        }
    }
}
