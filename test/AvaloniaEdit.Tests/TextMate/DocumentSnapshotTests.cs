using System;
using AvaloniaEdit.Document;
using AvaloniaEdit.TextMate;

using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace AvaloniaEdit.Tests.TextMate
{
    [TestFixture]
    public class DocumentSnapshotTests
    {
        #region Constructor tests

        [Test]
        public void Constructor_Should_Throw_When_Document_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() => new DocumentSnapshot(null));
        }

        [Test]
        public void Constructor_Should_Initialize_LineCount_From_Document()
        {
            TextDocument document = new TextDocument();
            document.Text = "alpha\nbravo\ncharlie";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.AreEqual(3, snapshot.LineCount);
        }

        [Test]
        public void Constructor_Should_Initialize_With_Empty_Document()
        {
            TextDocument document = new TextDocument();

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.AreEqual(1, snapshot.LineCount);
            Assert.AreEqual("", snapshot.GetLineText(0));
        }

        [Test]
        public void Constructor_Should_Initialize_With_Single_Line_Document()
        {
            TextDocument document = new TextDocument();
            document.Text = "hello";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.AreEqual(1, snapshot.LineCount);
            Assert.AreEqual("hello", snapshot.GetLineText(0));
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

        #endregion Constructor tests

        #region GetLineText tests

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
        public void GetLineText_Should_Throw_When_LineIndex_Is_Negative()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\npussy\nbirdie";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.Throws<ArgumentOutOfRangeException>(() => snapshot.GetLineText(-1));
        }

        [Test]
        public void GetLineText_Should_Throw_When_LineIndex_Equals_LineCount()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\npussy\nbirdie";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.Throws<ArgumentOutOfRangeException>(() => snapshot.GetLineText(3));
        }

        [Test]
        public void GetLineText_Should_Throw_When_LineIndex_Exceeds_LineCount()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\npussy\nbirdie";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.Throws<ArgumentOutOfRangeException>(() => snapshot.GetLineText(100));
        }

        #endregion GetLineText tests

        #region GetLineTextIncludingTerminator tests

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
        public void GetLineTextIncludingTerminator_Should_Throw_When_LineIndex_Is_Negative()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\npussy";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.Throws<ArgumentOutOfRangeException>(() => snapshot.GetLineTextIncludingTerminator(-1));
        }

        [Test]
        public void GetLineTextIncludingTerminator_Should_Throw_When_LineIndex_Equals_LineCount()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\npussy";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.Throws<ArgumentOutOfRangeException>(() => snapshot.GetLineTextIncludingTerminator(2));
        }

        #endregion GetLineTextIncludingTerminator tests

        #region GetLineTextIncludingTerminatorAsMemory tests

        [Test]
        public void GetLineTextIncludingTerminatorAsMemory_Should_Return_Correct_Content()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\npussy\nbirdie";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.AreEqual("puppy\n", snapshot.GetLineTextIncludingTerminatorAsMemory(0).ToString());
            Assert.AreEqual("pussy\n", snapshot.GetLineTextIncludingTerminatorAsMemory(1).ToString());
            Assert.AreEqual("birdie", snapshot.GetLineTextIncludingTerminatorAsMemory(2).ToString());
        }

        [Test]
        public void GetLineTextIncludingTerminatorAsMemory_Should_Throw_When_LineIndex_Is_Negative()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\npussy";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.Throws<ArgumentOutOfRangeException>(() => snapshot.GetLineTextIncludingTerminatorAsMemory(-1));
        }

        [Test]
        public void GetLineTextIncludingTerminatorAsMemory_Should_Throw_When_LineIndex_Equals_LineCount()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\npussy";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.Throws<ArgumentOutOfRangeException>(() => snapshot.GetLineTextIncludingTerminatorAsMemory(2));
        }

        #endregion GetLineTextIncludingTerminatorAsMemory tests

        #region GetLineTerminator tests

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
        public void GetLineTerminator_Should_Return_LF_For_LF_Document()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\npussy\nbirdie";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.AreEqual("\n", snapshot.GetLineTerminator(0));
            Assert.AreEqual("\n", snapshot.GetLineTerminator(1));
            Assert.AreEqual("", snapshot.GetLineTerminator(2));
        }

        [Test]
        public void GetLineTerminator_Should_Throw_When_LineIndex_Is_Negative()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\npussy";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.Throws<ArgumentOutOfRangeException>(() => snapshot.GetLineTerminator(-1));
        }

        [Test]
        public void GetLineTerminator_Should_Throw_When_LineIndex_Equals_LineCount()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\npussy";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.Throws<ArgumentOutOfRangeException>(() => snapshot.GetLineTerminator(2));
        }

        #endregion GetLineTerminator tests

        #region GetLineLength tests

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
        public void GetLineLength_Should_Throw_When_LineIndex_Is_Negative()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\npussy";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.Throws<ArgumentOutOfRangeException>(() => snapshot.GetLineLength(-1));
        }

        [Test]
        public void GetLineLength_Should_Throw_When_LineIndex_Equals_LineCount()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\npussy";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.Throws<ArgumentOutOfRangeException>(() => snapshot.GetLineLength(2));
        }

        #endregion GetLineLength tests

        #region GetTotalLineLength tests

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
        public void GetTotalLineLength_Should_Throw_When_LineIndex_Is_Negative()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\npussy";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.Throws<ArgumentOutOfRangeException>(() => snapshot.GetTotalLineLength(-1));
        }

        [Test]
        public void GetTotalLineLength_Should_Throw_When_LineIndex_Equals_LineCount()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\npussy";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.Throws<ArgumentOutOfRangeException>(() => snapshot.GetTotalLineLength(2));
        }

        #endregion GetTotalLineLength tests

        #region GetText tests

        [Test]
        public void GetText_Should_Return_Full_Document_Text()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\npussy\nbirdie";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.AreEqual("puppy\npussy\nbirdie", snapshot.GetText());
        }

        [Test]
        public void GetText_Should_Return_Empty_String_For_Empty_Document()
        {
            TextDocument document = new TextDocument();

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.AreEqual("", snapshot.GetText());
        }

        #endregion GetText tests

        #region RemoveLines validation tests

        [Test]
        public void RemoveLines_Should_Throw_When_StartLine_Is_Negative()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\r\npussy\r\nbirdie\r\ndoggie";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.Throws<ArgumentOutOfRangeException>(() => snapshot.RemoveLines(-1, 0));
        }

        [Test]
        public void RemoveLines_Should_Throw_When_EndLine_Is_Less_Than_StartLine()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\r\npussy\r\nbirdie\r\ndoggie";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.Throws<ArgumentOutOfRangeException>(() => snapshot.RemoveLines(2, 1));
        }

        [Test]
        public void RemoveLines_Should_Throw_When_EndLine_Equals_LineCount()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\r\npussy\r\nbirdie\r\ndoggie";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.Throws<ArgumentOutOfRangeException>(() => snapshot.RemoveLines(0, 4));
        }

        [Test]
        public void RemoveLines_Should_Throw_When_EndLine_Exceeds_LineCount()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\r\npussy\r\nbirdie\r\ndoggie";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.Throws<ArgumentOutOfRangeException>(() => snapshot.RemoveLines(0, 100));
        }

        [Test]
        public void RemoveLines_Should_Throw_When_Both_Parameters_Are_Negative()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\r\npussy\r\nbirdie";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.Throws<ArgumentOutOfRangeException>(() => snapshot.RemoveLines(-2, -1));
        }

        #endregion RemoveLines validation tests

        #region RemoveLines functional tests

        [Test]
        public void DocumentSnapshot_Should_Have_Correct_Line_Count_After_Remove_All_Lines()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\r\npussy\r\nbirdie\r\ndoggie";

            DocumentSnapshot documentSnaphot = new DocumentSnapshot(document);

            documentSnaphot.RemoveLines(0, 3);

            Assert.AreEqual(0, documentSnaphot.LineCount);
        }

        [Test]
        public void DocumentSnapshot_Should_Have_Correct_Line_Count_After_Remove_First_Line()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\r\npussy\r\nbirdie\r\ndoggie";

            DocumentSnapshot documentSnaphot = new DocumentSnapshot(document);

            documentSnaphot.RemoveLines(0, 0);

            Assert.AreEqual(3, documentSnaphot.LineCount);
        }

        [Test]
        public void DocumentSnapshot_Should_Have_Correct_Line_Count_After_Remove_Two_First_Lines()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\r\npussy\r\nbirdie\r\ndoggie";

            DocumentSnapshot documentSnaphot = new DocumentSnapshot(document);

            documentSnaphot.RemoveLines(0, 1);

            Assert.AreEqual(2, documentSnaphot.LineCount);
        }

        [Test]
        public void DocumentSnapshot_Should_Have_Correct_Line_Count_After_Remove_Last_Line()
        {
            TextDocument document = new TextDocument();
            document.Text = "puppy\r\npussy\r\nbirdie\r\ndoggie";

            DocumentSnapshot documentSnaphot = new DocumentSnapshot(document);

            documentSnaphot.RemoveLines(3, 3);

            Assert.AreEqual(3, documentSnaphot.LineCount);
        }

        [Test]
        public void RemoveLines_Should_Remove_Middle_Lines_And_Preserve_Surrounding()
        {
            TextDocument document = new TextDocument();
            document.Text = "alpha\nbravo\ncharlie\ndelta\necho";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            // Remove bravo and charlie (lines 1-2)
            snapshot.RemoveLines(1, 2);

            Assert.AreEqual(3, snapshot.LineCount);
        }

        [Test]
        public void RemoveLines_Should_Remove_Single_Middle_Line()
        {
            TextDocument document = new TextDocument();
            document.Text = "alpha\nbravo\ncharlie\ndelta\necho";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            // Remove charlie (line 2)
            snapshot.RemoveLines(2, 2);

            Assert.AreEqual(4, snapshot.LineCount);
        }

        [Test]
        public void RemoveLines_Should_Handle_Sequential_Removals()
        {
            TextDocument document = new TextDocument();
            document.Text = "alpha\nbravo\ncharlie\ndelta";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            // Remove last line first
            snapshot.RemoveLines(3, 3);
            Assert.AreEqual(3, snapshot.LineCount);

            // Remove first line
            snapshot.RemoveLines(0, 0);
            Assert.AreEqual(2, snapshot.LineCount);
        }

        [Test]
        public void RemoveLines_Should_Throw_After_All_Lines_Removed()
        {
            TextDocument document = new TextDocument();
            document.Text = "alpha\nbravo";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            snapshot.RemoveLines(0, 1);
            Assert.AreEqual(0, snapshot.LineCount);

            // Any further removal should throw since there are no lines left
            Assert.Throws<ArgumentOutOfRangeException>(() => snapshot.RemoveLines(0, 0));
        }

        [Test]
        public void RemoveLines_Should_Throw_When_StartLine_Equals_LineCount_After_Prior_Removal()
        {
            TextDocument document = new TextDocument();
            document.Text = "alpha\nbravo\ncharlie";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            // Remove last line, leaving 2 lines (indices 0-1)
            snapshot.RemoveLines(2, 2);
            Assert.AreEqual(2, snapshot.LineCount);

            // startLine=2 is now out of bounds
            Assert.Throws<ArgumentOutOfRangeException>(() => snapshot.RemoveLines(2, 2));
        }

        #endregion RemoveLines functional tests

        #region Accessor validation after RemoveLines tests

        [Test]
        public void GetLineText_Should_Throw_After_All_Lines_Removed()
        {
            TextDocument document = new TextDocument();
            document.Text = "alpha\nbravo";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            snapshot.RemoveLines(0, 1);

            Assert.Throws<ArgumentOutOfRangeException>(() => snapshot.GetLineText(0));
        }

        [Test]
        public void GetLineLength_Should_Throw_After_All_Lines_Removed()
        {
            TextDocument document = new TextDocument();
            document.Text = "alpha\nbravo";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            snapshot.RemoveLines(0, 1);

            Assert.Throws<ArgumentOutOfRangeException>(() => snapshot.GetLineLength(0));
        }

        [Test]
        public void GetTotalLineLength_Should_Throw_After_All_Lines_Removed()
        {
            TextDocument document = new TextDocument();
            document.Text = "alpha\nbravo";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            snapshot.RemoveLines(0, 1);

            Assert.Throws<ArgumentOutOfRangeException>(() => snapshot.GetTotalLineLength(0));
        }

        [Test]
        public void GetLineTerminator_Should_Throw_After_All_Lines_Removed()
        {
            TextDocument document = new TextDocument();
            document.Text = "alpha\nbravo";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            snapshot.RemoveLines(0, 1);

            Assert.Throws<ArgumentOutOfRangeException>(() => snapshot.GetLineTerminator(0));
        }

        [Test]
        public void GetLineTextIncludingTerminator_Should_Throw_After_All_Lines_Removed()
        {
            TextDocument document = new TextDocument();
            document.Text = "alpha\nbravo";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            snapshot.RemoveLines(0, 1);

            Assert.Throws<ArgumentOutOfRangeException>(() => snapshot.GetLineTextIncludingTerminator(0));
        }

        [Test]
        public void GetLineTextIncludingTerminatorAsMemory_Should_Throw_After_All_Lines_Removed()
        {
            TextDocument document = new TextDocument();
            document.Text = "alpha\nbravo";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            snapshot.RemoveLines(0, 1);

            Assert.Throws<ArgumentOutOfRangeException>(() => snapshot.GetLineTextIncludingTerminatorAsMemory(0));
        }

        #endregion Accessor validation after RemoveLines tests

        #region Edge case tests

        [Test]
        public void GetLineText_Should_Return_Empty_String_For_Empty_Line()
        {
            TextDocument document = new TextDocument();
            document.Text = "alpha\n\ncharlie";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.AreEqual("", snapshot.GetLineText(1));
        }

        [Test]
        public void GetLineTerminator_Should_Return_Empty_For_Last_Line_Without_Terminator()
        {
            TextDocument document = new TextDocument();
            document.Text = "alpha\nbravo";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.AreEqual("", snapshot.GetLineTerminator(1));
        }

        [Test]
        public void GetLineLength_Should_Return_Zero_For_Empty_Line()
        {
            TextDocument document = new TextDocument();
            document.Text = "alpha\n\ncharlie";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.AreEqual(0, snapshot.GetLineLength(1));
        }

        [Test]
        public void GetTotalLineLength_Should_Include_Terminator_For_Empty_Line()
        {
            TextDocument document = new TextDocument();
            document.Text = "alpha\n\ncharlie";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            // Empty line still has the \n terminator, so TotalLength = 1
            Assert.AreEqual(1, snapshot.GetTotalLineLength(1));
        }

        [Test]
        public void Single_Line_Document_Should_Have_Correct_Line_Properties()
        {
            TextDocument document = new TextDocument();
            document.Text = "hello";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.AreEqual(1, snapshot.LineCount);
            Assert.AreEqual("hello", snapshot.GetLineText(0));
            Assert.AreEqual("hello", snapshot.GetLineTextIncludingTerminator(0));
            Assert.AreEqual("", snapshot.GetLineTerminator(0));
            Assert.AreEqual(5, snapshot.GetLineLength(0));
            Assert.AreEqual(5, snapshot.GetTotalLineLength(0));
        }

        [Test]
        public void Document_With_Trailing_Newline_Should_Have_Empty_Last_Line()
        {
            TextDocument document = new TextDocument();
            document.Text = "alpha\n";

            DocumentSnapshot snapshot = new DocumentSnapshot(document);

            Assert.AreEqual(2, snapshot.LineCount);
            Assert.AreEqual("alpha", snapshot.GetLineText(0));
            Assert.AreEqual("", snapshot.GetLineText(1));
        }

        #endregion Edge case tests

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
