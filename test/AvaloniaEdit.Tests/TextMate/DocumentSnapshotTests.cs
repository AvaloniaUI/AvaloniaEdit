using AvaloniaEdit.Document;
using AvaloniaEdit.TextMate;

using NUnit.Framework;

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
    }
}
