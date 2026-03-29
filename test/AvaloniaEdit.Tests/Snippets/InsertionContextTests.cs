// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Linq;
using Avalonia.Headless.NUnit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Snippets;
using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace AvaloniaEdit.Tests.Snippets
{
    [TestFixture]
    public class InsertionContextTests
    {
        #region Constructor tests

        [AvaloniaTest]
        public void Constructor_ShouldThrow_When_TextAreaIsNull()
        {
            // act & assert
            Assert.Throws<ArgumentNullException>(() => new InsertionContext(null, 0));
        }

        [AvaloniaTest]
        public void Constructor_ShouldSetTextAreaProperty()
        {
            // arrange
            var textArea = CreateTextArea("hello");

            // act
            var context = new InsertionContext(textArea, 0);

            // assert
            Assert.AreSame(textArea, context.TextArea);
        }

        [AvaloniaTest]
        public void Constructor_ShouldSetDocumentProperty()
        {
            // arrange
            var textArea = CreateTextArea("hello");

            // act
            var context = new InsertionContext(textArea, 0);

            // assert
            Assert.AreSame(textArea.Document, context.Document);
        }

        [AvaloniaTest]
        public void Constructor_ShouldSetInsertionPosition()
        {
            // arrange
            var textArea = CreateTextArea("hello");

            // act
            var context = new InsertionContext(textArea, 3);

            // assert
            Assert.AreEqual(3, context.InsertionPosition);
        }

        [AvaloniaTest]
        public void Constructor_ShouldSetStartPosition()
        {
            // arrange
            var textArea = CreateTextArea("hello");

            // act
            var context = new InsertionContext(textArea, 3);

            // assert
            Assert.AreEqual(3, context.StartPosition);
        }

        [AvaloniaTest]
        public void Constructor_ShouldCaptureSelectedText()
        {
            // arrange
            var textArea = CreateTextArea("hello world");

            // act - no selection, should be empty
            var context = new InsertionContext(textArea, 0);

            // assert
            Assert.AreEqual("", context.SelectedText);
        }

        [AvaloniaTest]
        public void Constructor_ShouldSetTabFromOptions()
        {
            // arrange - default: ConvertTabsToSpaces = false, so Tab = "\t"
            var textArea = CreateTextArea("hello");
            var context = new InsertionContext(textArea, 0);

            // assert
            Assert.AreEqual("\t", context.Tab);
        }

        [AvaloniaTest]
        public void Constructor_ShouldSetTabToSpaces_When_ConvertTabsToSpaces()
        {
            // arrange
            var textArea = CreateTextAreaWithSpaces("hello", 4);
            var context = new InsertionContext(textArea, 0);

            // assert
            Assert.AreEqual("    ", context.Tab);
        }

        [AvaloniaTest]
        public void Constructor_ShouldCaptureIndentation_When_InsertedInIndentedLine()
        {
            // arrange - "    hello\n" with insertion at offset 4 (after the whitespace)
            var textArea = CreateTextArea("    hello\n");
            var context = new InsertionContext(textArea, 4);

            // assert - captures the whitespace before the insertion position on the same line
            Assert.AreEqual("    ", context.Indentation);
        }

        [AvaloniaTest]
        public void Constructor_ShouldCaptureEmptyIndentation_When_NoLeadingWhitespace()
        {
            // arrange
            var textArea = CreateTextArea("hello\n");
            var context = new InsertionContext(textArea, 0);

            // assert
            Assert.AreEqual("", context.Indentation);
        }

        [AvaloniaTest]
        public void Constructor_ShouldSetLineTerminator_CrLf()
        {
            // arrange - document with \r\n line endings
            var textArea = CreateTextArea("hello\r\nworld");
            var context = new InsertionContext(textArea, 0);

            // assert
            Assert.AreEqual("\r\n", context.LineTerminator);
        }

        [AvaloniaTest]
        public void Constructor_ShouldSetLineTerminator_Lf()
        {
            // arrange - document with \n line endings
            var textArea = CreateTextArea("hello\nworld");
            var context = new InsertionContext(textArea, 0);

            // assert
            Assert.AreEqual("\n", context.LineTerminator);
        }

        #endregion Constructor tests

        #region InsertText - exception ordering tests

        [AvaloniaTest]
        public void InsertText_ShouldThrowInvalidOperationException_Before_ArgumentNullException()
        {
            // This test verifies the original exception precedence:
            // _currentStatus is checked BEFORE text nullability.
            // If both conditions would throw, InvalidOperationException must win.

            // arrange
            var textArea = CreateTextArea("hello");
            var context = CreateContext(textArea, 5);
            // Transition away from Insertion status
            context.RaiseInsertionCompleted(EventArgs.Empty);

            // act & assert - passing null text when not in Insertion status
            // must throw InvalidOperationException, NOT ArgumentNullException
            Assert.Throws<InvalidOperationException>(() => context.InsertText(null));
        }

        [AvaloniaTest]
        public void InsertText_ShouldThrow_When_TextIsNull_And_StatusIsInsertion()
        {
            // arrange
            var textArea = CreateTextArea("hello");
            var context = CreateContext(textArea, 5);

            // act & assert - status IS Insertion, so null check fires
            Assert.Throws<ArgumentNullException>(() => context.InsertText(null));
        }

        [AvaloniaTest]
        public void InsertText_ShouldThrow_When_StatusIsNotInsertion()
        {
            // arrange
            var textArea = CreateTextArea("hello");
            var context = CreateContext(textArea, 5);

            // Transition to Deactivated status (no elements -> auto-deactivates)
            context.RaiseInsertionCompleted(EventArgs.Empty);

            // act & assert - context is now in Deactivated state
            Assert.Throws<InvalidOperationException>(() => context.InsertText("world"));
        }

        #endregion InsertText - exception ordering tests

        #region InsertText - fast path tests (no special characters)

        [AvaloniaTest]
        public void InsertText_PlainText_ShouldInsertDirectly()
        {
            // arrange
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);

            // act
            context.InsertText("hello world");

            // assert
            Assert.AreEqual("hello world", textArea.Document.Text);
            Assert.AreEqual(11, context.InsertionPosition);
        }

        [AvaloniaTest]
        public void InsertText_PlainText_ShouldInsertAtMiddleOfDocument()
        {
            // arrange
            var textArea = CreateTextArea("helloworld");
            var context = CreateContext(textArea, 5);

            // act
            context.InsertText(" ");

            // assert
            Assert.AreEqual("hello world", textArea.Document.Text);
            Assert.AreEqual(6, context.InsertionPosition);
        }

        [AvaloniaTest]
        public void InsertText_EmptyString_ShouldNotModifyDocument()
        {
            // arrange
            var textArea = CreateTextArea("hello");
            var context = CreateContext(textArea, 5);

            // act
            context.InsertText("");

            // assert
            Assert.AreEqual("hello", textArea.Document.Text);
            Assert.AreEqual(5, context.InsertionPosition);
        }

        [AvaloniaTest]
        public void InsertText_PlainText_ShouldInsertAtEndOfDocument()
        {
            // arrange
            var textArea = CreateTextArea("hello");
            var context = CreateContext(textArea, 5);

            // act
            context.InsertText(" world");

            // assert
            Assert.AreEqual("hello world", textArea.Document.Text);
            Assert.AreEqual(11, context.InsertionPosition);
        }

        #endregion InsertText - fast path tests (no special characters)

        #region InsertText - tab replacement tests

        [AvaloniaTest]
        public void InsertText_SingleTab_ShouldReplaceWithTabString()
        {
            // arrange - default options: ConvertTabsToSpaces = false, so Tab = "\t"
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);

            // act
            context.InsertText("\t");

            // assert - Tab property is "\t" when ConvertTabsToSpaces is false
            Assert.AreEqual("\t", textArea.Document.Text);
            Assert.AreEqual(1, context.InsertionPosition);
        }

        [AvaloniaTest]
        public void InsertText_Tab_ShouldReplaceWithSpaces_When_ConvertTabsToSpaces()
        {
            // arrange
            var textArea = CreateTextAreaWithSpaces("", 4);
            var context = CreateContext(textArea, 0);

            // act
            context.InsertText("\t");

            // assert - Tab property should be "    " (4 spaces)
            Assert.AreEqual("    ", textArea.Document.Text);
            Assert.AreEqual(4, context.InsertionPosition);
        }

        [AvaloniaTest]
        public void InsertText_MultipleTabs_ShouldReplaceEachWithTabString()
        {
            // arrange
            var textArea = CreateTextAreaWithSpaces("", 2);
            var context = CreateContext(textArea, 0);

            // act
            context.InsertText("\t\t");

            // assert - each tab becomes "  " (2 spaces)
            Assert.AreEqual("    ", textArea.Document.Text);
            Assert.AreEqual(4, context.InsertionPosition);
        }

        [AvaloniaTest]
        public void InsertText_TabSurroundedByText_ShouldReplaceTabOnly()
        {
            // arrange
            var textArea = CreateTextAreaWithSpaces("", 4);
            var context = CreateContext(textArea, 0);

            // act
            context.InsertText("if\t(true)");

            // assert
            Assert.AreEqual("if    (true)", textArea.Document.Text);
            Assert.AreEqual(12, context.InsertionPosition);
        }

        #endregion InsertText - tab replacement tests

        #region InsertText - tab with newline characters tests

        [AvaloniaTest]
        public void InsertText_Tab_ShouldNormalizeNewlines_When_TabContainsLineFeed()
        {
            // arrange - create a TextArea with custom options that returns Tab with newline
            var textArea = CreateTextAreaWithCustomIndentation("    code\n", "  \n  ");
            var context = CreateContext(textArea, 4);
            string term = context.LineTerminator;
            string indent = context.Indentation;

            // Verify Tab contains newline
            Assert.IsTrue(context.Tab.Contains('\n'));
            Assert.AreEqual("  \n  ", context.Tab);

            // act - input text contains a tab character
            context.InsertText("x\ty");

            // assert - tab should be replaced with Tab string, and its embedded newline normalized
            // Expected: "    " + "x" + "  " + term + indent + "  " + "y" + "code\n"
            string expected = "    x  " + term + indent + "  y" + "code\n";
            Assert.AreEqual(expected, textArea.Document.Text);
        }

        [AvaloniaTest]
        public void InsertText_Tab_ShouldNormalizeNewlines_When_TabContainsCarriageReturn()
        {
            // arrange
            var textArea = CreateTextAreaWithCustomIndentation("start\n", "\t\r\t");
            var context = CreateContext(textArea, 0);
            string term = context.LineTerminator;
            string indent = context.Indentation;

            // Verify Tab contains newline
            Assert.IsTrue(context.Tab.Contains('\r'));
            Assert.AreEqual("\t\r\t", context.Tab);

            // act
            context.InsertText("a\tb");

            // assert - the \r in Tab should be normalized to term + indent
            string expected = "a\t" + term + indent + "\tb" + "start\n";
            Assert.AreEqual(expected, textArea.Document.Text);
        }

        [AvaloniaTest]
        public void InsertText_Tab_ShouldNormalizeNewlines_When_TabContainsCrLf()
        {
            // arrange
            var textArea = CreateTextAreaWithCustomIndentation("    \n", "--\r\n++");
            var context = CreateContext(textArea, 4);
            string term = context.LineTerminator;
            string indent = context.Indentation;

            // Verify Tab contains newline
            Assert.IsTrue(context.Tab.Contains("\r\n"));
            Assert.AreEqual("--\r\n++", context.Tab);

            // act
            context.InsertText("\t");

            // assert - the \r\n in Tab should be normalized to term + indent
            string expected = "    --" + term + indent + "++" + "\n";
            Assert.AreEqual(expected, textArea.Document.Text);
        }

        [AvaloniaTest]
        public void InsertText_MultipleTabs_ShouldNormalizeNewlines_When_TabContainsNewline()
        {
            // arrange
            var textArea = CreateTextAreaWithCustomIndentation("", ">\n<");
            var context = CreateContext(textArea, 0);
            string term = context.LineTerminator;
            string indent = context.Indentation;

            Assert.AreEqual(">\n<", context.Tab);

            // act - input contains multiple tabs
            context.InsertText("\t\t");

            // assert - each tab's embedded newline should be normalized
            string expected = ">" + term + indent + "<"
                            + ">" + term + indent + "<";
            Assert.AreEqual(expected, textArea.Document.Text);
        }

        [AvaloniaTest]
        public void InsertText_TabWithSurroundingText_ShouldNormalizeEmbeddedNewlines()
        {
            // arrange
            var textArea = CreateTextAreaWithCustomIndentation("    \n", " [\n] ");
            var context = CreateContext(textArea, 4);
            string term = context.LineTerminator;
            string indent = context.Indentation;

            Assert.AreEqual(" [\n] ", context.Tab);

            // act
            context.InsertText("before\tmiddle\tafter");

            // assert
            string expected = "    before ["
                            + term + indent + "] middle ["
                            + term + indent + "] after"
                            + "\n";
            Assert.AreEqual(expected, textArea.Document.Text);
        }

        [AvaloniaTest]
        public void InsertText_Tab_ShouldNormalizeMixedNewlines_When_TabContainsMultipleNewlineTypes()
        {
            // arrange
            var textArea = CreateTextAreaWithCustomIndentation("", "a\rb\nc\r\nd");
            var context = CreateContext(textArea, 0);
            string term = context.LineTerminator;
            string indent = context.Indentation;

            Assert.IsTrue(context.Tab.Contains('\r') || context.Tab.Contains('\n'));

            // act
            context.InsertText("\t");

            // assert - all newlines in Tab should be normalized
            string expected = "a" + term + indent
                            + "b" + term + indent
                            + "c" + term + indent
                            + "d";
            Assert.AreEqual(expected, textArea.Document.Text);
        }

        [AvaloniaTest]
        public void InsertText_TabAndInputNewline_ShouldNormalizeBoth_When_TabContainsNewline()
        {
            // arrange
            var textArea = CreateTextAreaWithCustomIndentation("    \n", ">>>\n<<<");
            var context = CreateContext(textArea, 4);
            string term = context.LineTerminator;
            string indent = context.Indentation;

            // act - input contains both tab and explicit newline
            context.InsertText("x\t\ny");

            // assert - both the tab's embedded newline and the explicit \n should be normalized
            string expected = "    x>>>"
                            + term + indent + "<<<"
                            + term + indent + "y"
                            + "\n";
            Assert.AreEqual(expected, textArea.Document.Text);
        }

        #endregion InsertText - tab with newline characters tests

        #region InsertText - newline normalization tests (\n)

        [AvaloniaTest]
        public void InsertText_LineFeed_ShouldReplaceWithLineTerminatorAndIndentation()
        {
            // arrange - document with \n line endings, insert at position after indentation
            var textArea = CreateTextArea("    code\n");
            // Insert at offset 4 (after the leading whitespace "    ")
            var context = CreateContext(textArea, 4);
            string indent = context.Indentation;
            string term = context.LineTerminator;

            // act
            context.InsertText("line1\nline2");

            // assert - the existing "code\n" remains after the insertion point
            string expected = "    " + "line1" + term + indent + "line2" + "code\n";
            Assert.AreEqual(expected, textArea.Document.Text);
        }

        [AvaloniaTest]
        public void InsertText_MultipleLineFeeds_ShouldReplaceEach()
        {
            // arrange - insert at offset 0 of "start\n"
            var textArea = CreateTextArea("start\n");
            var context = CreateContext(textArea, 0);
            string term = context.LineTerminator;
            string indent = context.Indentation;

            // act
            context.InsertText("a\nb\nc");

            // assert - "start\n" remains after the insertion
            string inserted = "a" + term + indent
                            + "b" + term + indent
                            + "c";
            Assert.AreEqual(inserted + "start\n", textArea.Document.Text);
            Assert.AreEqual(inserted.Length, context.InsertionPosition);
        }

        #endregion InsertText - newline normalization tests (\n)

        #region InsertText - newline normalization tests (\r)

        [AvaloniaTest]
        public void InsertText_CarriageReturn_ShouldReplaceWithLineTerminatorAndIndentation()
        {
            // arrange
            var textArea = CreateTextArea("start\n");
            var context = CreateContext(textArea, 0);
            string term = context.LineTerminator;
            string indent = context.Indentation;

            // act
            context.InsertText("line1\rline2");

            // assert - "start\n" remains after the insertion
            string inserted = "line1" + term + indent + "line2";
            Assert.AreEqual(inserted + "start\n", textArea.Document.Text);
        }

        #endregion InsertText - newline normalization tests (\r)

        #region InsertText - newline normalization tests (\r\n)

        [AvaloniaTest]
        public void InsertText_CrLf_ShouldReplaceWithLineTerminatorAndIndentation()
        {
            // arrange
            var textArea = CreateTextArea("start\n");
            var context = CreateContext(textArea, 0);
            string term = context.LineTerminator;
            string indent = context.Indentation;

            // act
            context.InsertText("line1\r\nline2");

            // assert - "start\n" remains after the insertion
            string inserted = "line1" + term + indent + "line2";
            Assert.AreEqual(inserted + "start\n", textArea.Document.Text);
        }

        [AvaloniaTest]
        public void InsertText_MultipleCrLf_ShouldReplaceEach()
        {
            // arrange
            var textArea = CreateTextArea("start\n");
            var context = CreateContext(textArea, 0);
            string term = context.LineTerminator;
            string indent = context.Indentation;

            // act
            context.InsertText("a\r\nb\r\nc");

            // assert - "start\n" remains after the insertion
            string inserted = "a" + term + indent
                            + "b" + term + indent
                            + "c";
            Assert.AreEqual(inserted + "start\n", textArea.Document.Text);
        }

        #endregion InsertText - newline normalization tests (\r\n)

        #region InsertText - mixed newline tests

        [AvaloniaTest]
        public void InsertText_MixedNewlines_ShouldNormalizeAll()
        {
            // arrange
            var textArea = CreateTextArea("start\n");
            var context = CreateContext(textArea, 0);
            string term = context.LineTerminator;
            string indent = context.Indentation;

            // act - mix of \n, \r, and \r\n
            context.InsertText("a\nb\rc\r\nd");

            // assert - all newlines normalized, "start\n" remains after insertion
            string inserted = "a" + term + indent
                            + "b" + term + indent
                            + "c" + term + indent
                            + "d";
            Assert.AreEqual(inserted + "start\n", textArea.Document.Text);
        }

        #endregion InsertText - mixed newline tests

        #region InsertText - mixed tabs and newlines tests

        [AvaloniaTest]
        public void InsertText_TabsAndNewlines_ShouldReplaceAll()
        {
            // arrange - use spaces for tabs so we can verify expansion
            var textArea = CreateTextAreaWithSpaces("start\n", 4);
            var context = CreateContext(textArea, 0);
            string term = context.LineTerminator;
            string indent = context.Indentation;
            string tab = context.Tab;

            // act
            context.InsertText("\tif (true)\n\t\treturn;");

            // assert - "start\n" remains after insertion
            string inserted = tab + "if (true)"
                            + term + indent
                            + tab + tab + "return;";
            Assert.AreEqual(inserted + "start\n", textArea.Document.Text);
        }

        [AvaloniaTest]
        public void InsertText_TabImmediatelyBeforeNewline_ShouldReplaceBoth()
        {
            // arrange
            var textArea = CreateTextAreaWithSpaces("", 2);
            var context = CreateContext(textArea, 0);
            string term = context.LineTerminator;
            string indent = context.Indentation;
            string tab = context.Tab;

            // act
            context.InsertText("x\t\ny");

            // assert
            string expected = "x" + tab + term + indent + "y";
            Assert.AreEqual(expected, textArea.Document.Text);
        }

        [AvaloniaTest]
        public void InsertText_NewlineImmediatelyBeforeTab_ShouldReplaceBoth()
        {
            // arrange
            var textArea = CreateTextAreaWithSpaces("", 2);
            var context = CreateContext(textArea, 0);
            string term = context.LineTerminator;
            string indent = context.Indentation;
            string tab = context.Tab;

            // act
            context.InsertText("x\n\ty");

            // assert
            string expected = "x" + term + indent + tab + "y";
            Assert.AreEqual(expected, textArea.Document.Text);
        }

        #endregion InsertText - mixed tabs and newlines tests

        #region InsertText - indentation preservation tests

        [AvaloniaTest]
        public void InsertText_ShouldPreserveIndentation_When_InsertedInIndentedLine()
        {
            // arrange - insert at end of "    hello" (offset 9), before the \n
            var textArea = CreateTextArea("    hello\n");
            var context = CreateContext(textArea, 9);

            // The indentation should be "    " (the leading whitespace of the line)
            Assert.AreEqual("    ", context.Indentation);

            string term = context.LineTerminator;

            // act
            context.InsertText("\nworld");

            // assert - newline should include the indentation, then rest of document follows
            Assert.AreEqual("    hello" + term + "    world\n", textArea.Document.Text);
        }

        [AvaloniaTest]
        public void InsertText_ShouldUseEmptyIndentation_When_InsertedAtLineStart()
        {
            // arrange - insertion at the very start of a non-indented line
            var textArea = CreateTextArea("hello\n");
            var context = CreateContext(textArea, 0);

            // No leading whitespace before position 0
            Assert.AreEqual("", context.Indentation);

            string term = context.LineTerminator;

            // act
            context.InsertText("a\nb");

            // assert - no indentation added after the newline, "hello\n" remains
            Assert.AreEqual("a" + term + "b" + "hello\n", textArea.Document.Text);
        }

        #endregion InsertText - indentation preservation tests

        #region InsertText - consecutive calls tests

        [AvaloniaTest]
        public void InsertText_MultipleConsecutiveCalls_ShouldAppendCorrectly()
        {
            // arrange
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);

            // act
            context.InsertText("hello");
            context.InsertText(" ");
            context.InsertText("world");

            // assert
            Assert.AreEqual("hello world", textArea.Document.Text);
            Assert.AreEqual(11, context.InsertionPosition);
        }

        [AvaloniaTest]
        public void InsertText_MultipleCallsWithNewlines_ShouldMaintainInsertionPosition()
        {
            // arrange
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);
            string term = context.LineTerminator;
            string indent = context.Indentation;

            // act
            context.InsertText("line1\n");
            int posAfterFirst = context.InsertionPosition;
            context.InsertText("line2");

            // assert
            string expectedFirstPart = "line1" + term + indent;
            Assert.AreEqual(expectedFirstPart.Length, posAfterFirst);
            Assert.AreEqual(expectedFirstPart + "line2", textArea.Document.Text);
        }

        #endregion InsertText - consecutive calls tests

        #region InsertText - InsertionPosition tracking tests

        [AvaloniaTest]
        public void InsertText_InsertionPosition_ShouldAdvanceByInsertedLength_PlainText()
        {
            // arrange
            var textArea = CreateTextArea("existing");
            var context = CreateContext(textArea, 8);
            int startPos = context.InsertionPosition;

            // act
            context.InsertText("added");

            // assert
            Assert.AreEqual(startPos + 5, context.InsertionPosition);
        }

        [AvaloniaTest]
        public void InsertText_InsertionPosition_ShouldAccountForExpandedTabs()
        {
            // arrange
            var textArea = CreateTextAreaWithSpaces("", 4);
            var context = CreateContext(textArea, 0);

            // act
            context.InsertText("\t");

            // assert - tab expanded to 4 spaces
            Assert.AreEqual(4, context.InsertionPosition);
        }

        [AvaloniaTest]
        public void InsertText_InsertionPosition_ShouldAccountForExpandedNewlines()
        {
            // arrange
            var textArea = CreateTextArea("    code\n");
            var context = CreateContext(textArea, 4);
            string term = context.LineTerminator;
            string indent = context.Indentation;

            // act
            context.InsertText("a\nb");

            // assert
            int expectedLength = ("a" + term + indent + "b").Length;
            Assert.AreEqual(4 + expectedLength, context.InsertionPosition);
        }

        #endregion InsertText - InsertionPosition tracking tests

        #region InsertText - edge case tests

        [AvaloniaTest]
        public void InsertText_OnlyNewline_ShouldInsertTerminatorAndIndentation()
        {
            // arrange
            var textArea = CreateTextArea("    code\n");
            var context = CreateContext(textArea, 4);
            string term = context.LineTerminator;
            string indent = context.Indentation;

            // act
            context.InsertText("\n");

            // assert - "code\n" remains after the insertion
            string expected = "    " + term + indent + "code\n";
            Assert.AreEqual(expected, textArea.Document.Text);
        }

        [AvaloniaTest]
        public void InsertText_OnlyTab_ShouldInsertTabString()
        {
            // arrange
            var textArea = CreateTextAreaWithSpaces("", 3);
            var context = CreateContext(textArea, 0);

            // act
            context.InsertText("\t");

            // assert
            Assert.AreEqual("   ", textArea.Document.Text);
            Assert.AreEqual(3, context.InsertionPosition);
        }

        [AvaloniaTest]
        public void InsertText_TrailingNewline_ShouldAppendTerminatorAndIndentation()
        {
            // arrange
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);
            string term = context.LineTerminator;
            string indent = context.Indentation;

            // act
            context.InsertText("hello\n");

            // assert
            string expected = "hello" + term + indent;
            Assert.AreEqual(expected, textArea.Document.Text);
            Assert.AreEqual(expected.Length, context.InsertionPosition);
        }

        [AvaloniaTest]
        public void InsertText_LeadingNewline_ShouldPrependTerminatorAndIndentation()
        {
            // arrange
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);
            string term = context.LineTerminator;
            string indent = context.Indentation;

            // act
            context.InsertText("\nhello");

            // assert
            string expected = term + indent + "hello";
            Assert.AreEqual(expected, textArea.Document.Text);
        }

        [AvaloniaTest]
        public void InsertText_ConsecutiveNewlines_ShouldInsertMultipleTerminators()
        {
            // arrange - insert into empty document
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);
            string term = context.LineTerminator;
            string indent = context.Indentation;

            // act
            context.InsertText("a\n\nb");

            // assert - two newlines should produce two terminator+indentation pairs
            string expected = "a" + term + indent
                                  + term + indent
                            + "b";
            Assert.AreEqual(expected, textArea.Document.Text);
        }

        [AvaloniaTest]
        public void InsertText_ConsecutiveTabs_ShouldReplaceEachIndividually()
        {
            // arrange
            var textArea = CreateTextAreaWithSpaces("", 2);
            var context = CreateContext(textArea, 0);

            // act
            context.InsertText("\t\t\t");

            // assert - three tabs, each expanded to 2 spaces
            Assert.AreEqual("      ", textArea.Document.Text);
            Assert.AreEqual(6, context.InsertionPosition);
        }

        [AvaloniaTest]
        public void InsertText_CrWithoutLf_ShouldTreatAsSingleNewline()
        {
            // arrange - insert into empty document to avoid stale text confusion
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);
            string term = context.LineTerminator;
            string indent = context.Indentation;

            // act - lone \r should be treated as a newline, not ignored
            context.InsertText("a\rb");

            // assert
            string expected = "a" + term + indent + "b";
            Assert.AreEqual(expected, textArea.Document.Text);
        }

        [AvaloniaTest]
        public void InsertText_CrLfAsUnitNotTwoNewlines()
        {
            // arrange - verify that \r\n produces exactly ONE newline, not two
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);
            string term = context.LineTerminator;
            string indent = context.Indentation;

            // act
            context.InsertText("a\r\nb");

            // assert - should be exactly one line break
            string expected = "a" + term + indent + "b";
            Assert.AreEqual(expected, textArea.Document.Text);
        }

        #endregion InsertText - edge case tests

        #region InsertText - document line terminator preservation tests

        [AvaloniaTest]
        public void InsertText_ShouldUseDocumentLineTerminator_CrLf()
        {
            // arrange - document with \r\n line endings
            var textArea = CreateTextArea("hello\r\nworld");
            var context = CreateContext(textArea, 0);

            // The LineTerminator should be detected from the document
            Assert.AreEqual("\r\n", context.LineTerminator);

            // act
            context.InsertText("a\nb");

            // assert - the \n in the input should become \r\n in the output,
            // and the existing "hello\r\nworld" remains after the insertion
            Assert.AreEqual("a\r\nb" + "hello\r\nworld", textArea.Document.Text);
        }

        [AvaloniaTest]
        public void InsertText_ShouldUseDocumentLineTerminator_Lf()
        {
            // arrange - document with \n line endings
            var textArea = CreateTextArea("hello\nworld");
            var context = CreateContext(textArea, 0);

            Assert.AreEqual("\n", context.LineTerminator);

            // act
            context.InsertText("a\r\nb");

            // assert - the \r\n in the input should become \n in the output,
            // and the existing "hello\nworld" remains after the insertion
            Assert.AreEqual("a\nb" + "hello\nworld", textArea.Document.Text);
        }

        #endregion InsertText - document line terminator preservation tests

        #region InsertText - large input tests

        [AvaloniaTest]
        public void InsertText_LargeInput_ShouldHandleCorrectly()
        {
            // arrange
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);
            string term = context.LineTerminator;

            // Build a large input with many lines
            var inputBuilder = new System.Text.StringBuilder();
            for (int i = 0; i < 100; i++)
            {
                if (i > 0)
                {
                    inputBuilder.Append('\n');
                }

                inputBuilder.Append($"line{i}");
            }
            string input = inputBuilder.ToString();

            // act
            context.InsertText(input);

            // assert - verify the document contains the expected text
            string documentText = textArea.Document.Text;

            // Verify first and last lines are present
            Assert.IsTrue(documentText.StartsWith("line0"));
            Assert.IsTrue(documentText.EndsWith("line99"));

            // Verify line count: 100 lines of text means 99 newlines
            int terminatorCount = 0;
            int searchPos = 0;
            while ((searchPos = documentText.IndexOf(term, searchPos, StringComparison.Ordinal)) >= 0)
            {
                terminatorCount++;
                searchPos += term.Length;
            }
            Assert.AreEqual(99, terminatorCount);
        }

        #endregion InsertText - large input tests

        #region InsertText - tab identity tests

        [AvaloniaTest]
        public void InsertText_TabEqualsLiteralTab_ShouldProduceCorrectResult()
        {
            // arrange - when ConvertTabsToSpaces is false, Tab == "\t"
            // So replacing \t with \t is a no-op. The optimized version skips tab
            // processing when Tab == "\t" and only scans for newlines.
            var textArea = CreateTextArea("");
            textArea.Options.ConvertTabsToSpaces = false;
            var context = CreateContext(textArea, 0);

            // act
            context.InsertText("hello\tworld");

            // assert - tab is preserved as-is (identity replacement)
            Assert.AreEqual("hello\tworld", textArea.Document.Text);
            Assert.AreEqual(11, context.InsertionPosition);
        }

        #endregion InsertText - tab identity tests

        #region InsertText - RunUpdate event observation tests

        [AvaloniaTest]
        public void InsertText_ShouldFireUpdateStartedAndFinished_ForPlainText()
        {
            // This test verifies that InsertText wraps its document operation
            // in RunUpdate(), which fires UpdateStarted and UpdateFinished events.
            // The original code always uses RunUpdate(); the optimized version must too.

            // arrange
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);
            int updateStartedCount = 0;
            int updateFinishedCount = 0;
            textArea.Document.UpdateStarted += (_, _) => updateStartedCount++;
            textArea.Document.UpdateFinished += (_, _) => updateFinishedCount++;

            // act
            context.InsertText("hello");

            // assert - should have fired exactly one UpdateStarted/UpdateFinished pair
            Assert.AreEqual(1, updateStartedCount);
            Assert.AreEqual(1, updateFinishedCount);
        }

        [AvaloniaTest]
        public void InsertText_ShouldFireUpdateStartedAndFinished_ForTextWithNewlines()
        {
            // arrange
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);
            int updateStartedCount = 0;
            int updateFinishedCount = 0;
            textArea.Document.UpdateStarted += (_, _) => updateStartedCount++;
            textArea.Document.UpdateFinished += (_, _) => updateFinishedCount++;

            // act
            context.InsertText("hello\nworld");

            // assert
            Assert.AreEqual(1, updateStartedCount);
            Assert.AreEqual(1, updateFinishedCount);
        }

        [AvaloniaTest]
        public void InsertText_ShouldGroupAsOneUndoAction()
        {
            // Verifies the RunUpdate scope groups all changes into a single undo action.

            // arrange
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);

            // act
            context.InsertText("line1\nline2\nline3");

            // assert - one undo should revert the entire insertion
            textArea.Document.UndoStack.Undo();
            Assert.AreEqual("", textArea.Document.Text);
        }

        #endregion InsertText - RunUpdate event observation tests

        #region InsertText - regression parity tests

        [AvaloniaTest]
        public void InsertText_ShouldProduceSameResult_AsOriginalForSimpleSnippet()
        {
            // This test verifies a realistic snippet insertion scenario:
            // inserting "if (condition)\n{\n\tstatement;\n}" into an indented context.

            // arrange
            var textArea = CreateTextAreaWithSpaces("    \n", 4);
            // Insert at offset 4, which is after "    " indentation on line 1
            var context = CreateContext(textArea, 4);

            Assert.AreEqual("    ", context.Indentation);
            string term = context.LineTerminator;
            string indent = context.Indentation;
            string tab = context.Tab;

            // act
            context.InsertText("if (condition)\n{\n\tstatement;\n}");

            // assert - "\n" at end of original document remains after insertion
            string expected = "    "
                            + "if (condition)" + term + indent
                            + "{" + term + indent
                            + tab + "statement;" + term + indent
                            + "}"
                            + "\n";
            Assert.AreEqual(expected, textArea.Document.Text);
        }

        [AvaloniaTest]
        public void InsertText_ShouldProduceSameResult_ForMultipleTabsInLine()
        {
            // Simulates: "\t\tint x = 0;\n\t\tint y = 0;"

            // arrange
            var textArea = CreateTextAreaWithSpaces("", 4);
            var context = CreateContext(textArea, 0);
            string term = context.LineTerminator;
            string indent = context.Indentation;
            string tab = context.Tab;

            // act
            context.InsertText("\t\tint x = 0;\n\t\tint y = 0;");

            // assert
            string expected = tab + tab + "int x = 0;"
                            + term + indent
                            + tab + tab + "int y = 0;";
            Assert.AreEqual(expected, textArea.Document.Text);
        }

        #endregion InsertText - regression parity tests

        #region InsertText - StartPosition and anchor tests

        [AvaloniaTest]
        public void InsertText_StartPosition_ShouldRemainAtOriginalOffset()
        {
            // arrange
            var textArea = CreateTextArea("prefix");
            var context = CreateContext(textArea, 6);

            // act
            context.InsertText("inserted text");

            // assert - StartPosition should still point to the beginning of the snippet
            Assert.AreEqual(6, context.StartPosition);
        }

        #endregion InsertText - StartPosition and anchor tests

        #region RegisterActiveElement tests

        [AvaloniaTest]
        public void RegisterActiveElement_ShouldThrow_When_OwnerIsNull()
        {
            // arrange
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);
            var element = new TestActiveElement();

            // act & assert
            Assert.Throws<ArgumentNullException>(() => context.RegisterActiveElement(null, element));
        }

        [AvaloniaTest]
        public void RegisterActiveElement_ShouldThrow_When_ElementIsNull()
        {
            // arrange
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);
            var owner = new TestSnippetElement();

            // act & assert
            Assert.Throws<ArgumentNullException>(() => context.RegisterActiveElement(owner, null));
        }

        [AvaloniaTest]
        public void RegisterActiveElement_ShouldThrow_When_NotInInsertionStatus()
        {
            // arrange
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);
            // Transition away from Insertion status
            context.RaiseInsertionCompleted(EventArgs.Empty);

            var owner = new TestSnippetElement();
            var element = new TestActiveElement();

            // act & assert
            Assert.Throws<InvalidOperationException>(() => context.RegisterActiveElement(owner, element));
        }

        [AvaloniaTest]
        public void RegisterActiveElement_ShouldSucceed_DuringInsertion()
        {
            // arrange
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);
            var owner = new TestSnippetElement();
            var element = new TestActiveElement();

            // act - should not throw
            context.RegisterActiveElement(owner, element);

            // assert - element should be retrievable
            Assert.AreSame(element, context.GetActiveElement(owner));
        }

        [AvaloniaTest]
        public void RegisterActiveElement_ShouldThrow_When_DuplicateOwner()
        {
            // arrange
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);
            var owner = new TestSnippetElement();
            var element1 = new TestActiveElement();
            var element2 = new TestActiveElement();

            context.RegisterActiveElement(owner, element1);

            // act & assert - same owner registered twice should throw (Dictionary.Add behavior)
            Assert.Throws<ArgumentException>(() => context.RegisterActiveElement(owner, element2));
        }

        #endregion RegisterActiveElement tests

        #region GetActiveElement tests

        [AvaloniaTest]
        public void GetActiveElement_ShouldThrow_When_OwnerIsNull()
        {
            // arrange
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);

            // act & assert
            Assert.Throws<ArgumentNullException>(() => context.GetActiveElement(null));
        }

        [AvaloniaTest]
        public void GetActiveElement_ShouldReturnNull_When_OwnerNotRegistered()
        {
            // arrange
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);
            var owner = new TestSnippetElement();

            // act
            var result = context.GetActiveElement(owner);

            // assert
            Assert.IsNull(result);
        }

        [AvaloniaTest]
        public void GetActiveElement_ShouldReturnRegisteredElement()
        {
            // arrange
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);
            var owner = new TestSnippetElement();
            var element = new TestActiveElement();
            context.RegisterActiveElement(owner, element);

            // act
            var result = context.GetActiveElement(owner);

            // assert
            Assert.AreSame(element, result);
        }

        [AvaloniaTest]
        public void GetActiveElement_ShouldReturnCorrectElement_When_MultipleRegistered()
        {
            // arrange
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);
            var owner1 = new TestSnippetElement();
            var owner2 = new TestSnippetElement();
            var element1 = new TestActiveElement();
            var element2 = new TestActiveElement();
            context.RegisterActiveElement(owner1, element1);
            context.RegisterActiveElement(owner2, element2);

            // act & assert
            Assert.AreSame(element1, context.GetActiveElement(owner1));
            Assert.AreSame(element2, context.GetActiveElement(owner2));
        }

        #endregion GetActiveElement tests

        #region ActiveElements property tests

        [AvaloniaTest]
        public void ActiveElements_ShouldBeEmpty_Initially()
        {
            // arrange
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);

            // act & assert
            Assert.IsFalse(context.ActiveElements.Any());
        }

        [AvaloniaTest]
        public void ActiveElements_ShouldReturnRegisteredElements_InOrder()
        {
            // arrange
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);
            var owner1 = new TestSnippetElement();
            var owner2 = new TestSnippetElement();
            var element1 = new TestActiveElement();
            var element2 = new TestActiveElement();
            context.RegisterActiveElement(owner1, element1);
            context.RegisterActiveElement(owner2, element2);

            // act
            var elements = context.ActiveElements.ToList();

            // assert - should preserve insertion order
            Assert.AreEqual(2, elements.Count);
            Assert.AreSame(element1, elements[0]);
            Assert.AreSame(element2, elements[1]);
        }

        #endregion ActiveElements property tests

        #region RaiseInsertionCompleted tests

        [AvaloniaTest]
        public void RaiseInsertionCompleted_ShouldThrow_When_NotInInsertionStatus()
        {
            // arrange
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);
            // First call transitions away from Insertion
            context.RaiseInsertionCompleted(EventArgs.Empty);

            // act & assert - second call should throw
            Assert.Throws<InvalidOperationException>(() => context.RaiseInsertionCompleted(EventArgs.Empty));
        }

        [AvaloniaTest]
        public void RaiseInsertionCompleted_ShouldCallOnInsertionCompleted_OnAllElements()
        {
            // arrange
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);
            var owner1 = new TestSnippetElement();
            var owner2 = new TestSnippetElement();
            var element1 = new TestActiveElement { IsEditable = true, Segment = new SimpleSegment(0, 0) };
            var element2 = new TestActiveElement { IsEditable = true, Segment = new SimpleSegment(0, 0) };
            context.RegisterActiveElement(owner1, element1);
            context.RegisterActiveElement(owner2, element2);

            // act
            context.RaiseInsertionCompleted(EventArgs.Empty);

            // assert
            Assert.IsTrue(element1.InsertionCompletedCalled);
            Assert.IsTrue(element2.InsertionCompletedCalled);
        }

        [AvaloniaTest]
        public void RaiseInsertionCompleted_ShouldRaiseInsertionCompletedEvent()
        {
            // arrange
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);
            bool eventRaised = false;
            context.InsertionCompleted += (_, _) => eventRaised = true;

            // act
            context.RaiseInsertionCompleted(EventArgs.Empty);

            // assert
            Assert.IsTrue(eventRaised);
        }

        [AvaloniaTest]
        public void RaiseInsertionCompleted_ShouldAutoDeactivate_When_NoActiveElements()
        {
            // arrange - no elements registered
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);
            bool deactivatedRaised = false;
            DeactivateReason? reason = null;
            context.Deactivated += (_, e) =>
            {
                deactivatedRaised = true;
                reason = e.Reason;
            };

            // act
            context.RaiseInsertionCompleted(EventArgs.Empty);

            // assert - should auto-deactivate with NoActiveElements reason
            Assert.IsTrue(deactivatedRaised);
            Assert.AreEqual(DeactivateReason.NoActiveElements, reason);
        }

        [AvaloniaTest]
        public void RaiseInsertionCompleted_ShouldSetStartPosition_ViaWholeSnippetAnchor()
        {
            // arrange
            var textArea = CreateTextArea("prefix");
            var context = CreateContext(textArea, 6);
            context.InsertText("body");

            // act
            context.RaiseInsertionCompleted(EventArgs.Empty);

            // assert - StartPosition should now be backed by the AnchorSegment
            Assert.AreEqual(6, context.StartPosition);
        }

        [AvaloniaTest]
        public void RaiseInsertionCompleted_ShouldAcceptNullEventArgs()
        {
            // arrange
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);
            bool eventRaised = false;
            EventArgs receivedArgs = null;
            context.InsertionCompleted += (_, e) =>
            {
                eventRaised = true;
                receivedArgs = e;
            };

            // act - null should be replaced with EventArgs.Empty
            context.RaiseInsertionCompleted(null);

            // assert
            Assert.IsTrue(eventRaised);
            Assert.AreSame(EventArgs.Empty, receivedArgs);
        }

        #endregion RaiseInsertionCompleted tests

        #region Deactivate tests

        [AvaloniaTest]
        public void Deactivate_ShouldThrow_When_NotInInteractiveStatus()
        {
            // arrange - context is still in Insertion status
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);

            // act & assert
            Assert.Throws<InvalidOperationException>(() =>
                context.Deactivate(new SnippetEventArgs(DeactivateReason.Unknown)));
        }

        [AvaloniaTest]
        public void Deactivate_ShouldCallDeactivate_OnAllElements()
        {
            // arrange
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);
            var owner1 = new TestSnippetElement();
            var owner2 = new TestSnippetElement();
            var element1 = new TestActiveElement { IsEditable = true, Segment = new SimpleSegment(0, 0) };
            var element2 = new TestActiveElement { IsEditable = true, Segment = new SimpleSegment(0, 0) };
            context.RegisterActiveElement(owner1, element1);
            context.RegisterActiveElement(owner2, element2);
            context.RaiseInsertionCompleted(EventArgs.Empty);

            // act
            context.Deactivate(new SnippetEventArgs(DeactivateReason.EscapePressed));

            // assert
            Assert.IsTrue(element1.DeactivateCalled);
            Assert.IsTrue(element2.DeactivateCalled);
            Assert.AreEqual(DeactivateReason.EscapePressed, element1.DeactivateArgs.Reason);
            Assert.AreEqual(DeactivateReason.EscapePressed, element2.DeactivateArgs.Reason);
        }

        [AvaloniaTest]
        public void Deactivate_ShouldRaiseDeactivatedEvent()
        {
            // arrange
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);
            var owner = new TestSnippetElement();
            var element = new TestActiveElement { IsEditable = true, Segment = new SimpleSegment(0, 0) };
            context.RegisterActiveElement(owner, element);
            context.RaiseInsertionCompleted(EventArgs.Empty);

            bool deactivatedRaised = false;
            DeactivateReason? reason = null;
            context.Deactivated += (_, e) =>
            {
                deactivatedRaised = true;
                reason = e.Reason;
            };

            // act
            context.Deactivate(new SnippetEventArgs(DeactivateReason.ReturnPressed));

            // assert
            Assert.IsTrue(deactivatedRaised);
            Assert.AreEqual(DeactivateReason.ReturnPressed, reason);
        }

        [AvaloniaTest]
        public void Deactivate_ShouldBeIdempotent_WhenCalledMultipleTimes()
        {
            // arrange
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);
            // No elements -> auto-deactivates on RaiseInsertionCompleted
            context.RaiseInsertionCompleted(EventArgs.Empty);

            // act & assert - second deactivate should be a no-op (not throw)
            Assert.DoesNotThrow(() => context.Deactivate(new SnippetEventArgs(DeactivateReason.Unknown)));
        }

        [AvaloniaTest]
        public void Deactivate_ShouldUseUnknownReason_When_EventArgsIsNull()
        {
            // arrange
            var textArea = CreateTextArea("");
            var context = CreateContext(textArea, 0);
            var owner = new TestSnippetElement();
            var element = new TestActiveElement { IsEditable = true, Segment = new SimpleSegment(0, 0) };
            context.RegisterActiveElement(owner, element);
            context.RaiseInsertionCompleted(EventArgs.Empty);

            // act - null should be replaced with Unknown reason
            context.Deactivate(null);

            // assert
            Assert.IsTrue(element.DeactivateCalled);
            Assert.AreEqual(DeactivateReason.Unknown, element.DeactivateArgs.Reason);
        }

        #endregion Deactivate tests

        #region Link tests

        [AvaloniaTest]
        public void Link_ShouldRegisterActiveElements_ForMainAndBound()
        {
            // arrange - create a document with text that has identifiable segments
            var textArea = CreateTextArea("var name = name;");
            var context = CreateContext(textArea, 0);

            // Main element: "name" at offset 4, length 4
            var mainSegment = new SimpleSegment(4, 4);
            // Bound element: "name" at offset 11, length 4
            var boundSegments = new ISegment[] { new SimpleSegment(11, 4) };

            // act
            context.Link(mainSegment, boundSegments);

            // assert - should have registered 2 elements (1 main + 1 bound)
            var elements = context.ActiveElements.ToList();
            Assert.AreEqual(2, elements.Count);
        }

        [AvaloniaTest]
        public void Link_ShouldRegisterMultipleBoundElements()
        {
            // arrange
            var textArea = CreateTextArea("var x = x + x;");
            var context = CreateContext(textArea, 0);

            var mainSegment = new SimpleSegment(4, 1);
            var boundSegments = new ISegment[]
            {
                new SimpleSegment(8, 1),
                new SimpleSegment(12, 1)
            };

            // act
            context.Link(mainSegment, boundSegments);

            // assert - should have 3 elements (1 main + 2 bound)
            var elements = context.ActiveElements.ToList();
            Assert.AreEqual(3, elements.Count);
        }

        #endregion Link tests

        #region Test helpers

        /// <summary>
        /// Creates a <see cref="TextArea"/> with the specified document text and
        /// default options (tabs = "\t", indentation size = 4, ConvertTabsToSpaces = false).
        /// </summary>
        private static TextArea CreateTextArea(string documentText)
        {
            var textArea = new TextArea
            {
                Document = new TextDocument(documentText),
                Options =
                {
                    // Ensure default tab behavior: tabs stay as literal "\t" replacement
                    ConvertTabsToSpaces = false,
                    IndentationSize = 4
                }
            };
            return textArea;
        }

        /// <summary>
        /// Creates a <see cref="TextArea"/> configured to convert tabs to spaces.
        /// </summary>
        private static TextArea CreateTextAreaWithSpaces(string documentText, int indentationSize = 4)
        {
            var textArea = new TextArea
            {
                Document = new TextDocument(documentText),
                Options =
                {
                    ConvertTabsToSpaces = true,
                    IndentationSize = indentationSize
                }
            };
            return textArea;
        }

        /// <summary>
        /// Creates an <see cref="InsertionContext"/> at the specified offset.
        /// </summary>
        private static InsertionContext CreateContext(TextArea textArea, int insertionPosition)
        {
            return new InsertionContext(textArea, insertionPosition);
        }

        /// <summary>
        /// Creates a <see cref="TextArea"/> with custom IndentationString for testing
        /// scenarios where Tab property contains newline characters.
        /// </summary>
        private static TextArea CreateTextAreaWithCustomIndentation(string documentText, string customIndentation)
        {
            var textArea = new TextArea
            {
                Document = new TextDocument(documentText),
                Options = new TestEditorOptionsWithCustomIndentation(customIndentation)
            };

            return textArea;
        }

        /// <summary>
        /// Custom <see cref="TextEditorOptions"/> that overrides GetIndentationString
        /// to return a custom indentation string, used for testing edge cases where
        /// the indentation string contains newline characters.
        /// </summary>
        private sealed class TestEditorOptionsWithCustomIndentation : TextEditorOptions
        {
            private readonly string _customIndentation;

            public TestEditorOptionsWithCustomIndentation(string customIndentation)
            {
                _customIndentation = customIndentation ?? throw new ArgumentNullException(nameof(customIndentation));
            }

            public override string GetIndentationString(int column) => _customIndentation;
        }

        /// <summary>
        /// Simple stub implementation of <see cref="IActiveElement"/> for testing.
        /// Tracks whether OnInsertionCompleted and Deactivate were called.
        /// </summary>
        private sealed class TestActiveElement : IActiveElement
        {
            public bool InsertionCompletedCalled { get; private set; }
            public bool DeactivateCalled { get; private set; }
            public SnippetEventArgs DeactivateArgs { get; private set; }
            public bool IsEditable { get; init; }
            public ISegment Segment { get; init; } = new TextSegment();

            public void OnInsertionCompleted()
            {
                InsertionCompletedCalled = true;
            }

            public void Deactivate(SnippetEventArgs e)
            {
                DeactivateCalled = true;
                DeactivateArgs = e;
            }
        }

        /// <summary>
        /// Simple concrete <see cref="SnippetElement"/> subclass for use as a dictionary key
        /// in RegisterActiveElement/GetActiveElement tests.
        /// </summary>
        private sealed class TestSnippetElement : SnippetElement
        {
            public override void Insert(InsertionContext context)
            {
                // no-op for testing
            }
        }

        #endregion Test helpers
    }
}
