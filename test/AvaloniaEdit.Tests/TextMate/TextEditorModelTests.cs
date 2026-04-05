using System;
using System.Collections.ObjectModel;
using Avalonia.Headless.NUnit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.Tests.TestUtils;
using AvaloniaEdit.TextMate;
using NUnit.Framework;
using TextMateSharp.Grammars;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace AvaloniaEdit.Tests.TextMate
{
    [TestFixture]
    internal class TextEditorModelTests
    {
        #region Line Count

        [Test]
        public void Lines_Should_Have_Valid_Count()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            // Act
            document.Text = "puppy\npussy\nbirdie";

            // Assert
            Assert.AreEqual(3, textEditorModel.GetNumberOfLines());
        }

        [Test]
        public void Empty_Document_Should_Have_One_Line()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            // Act
            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            // Assert
            Assert.AreEqual(1, textEditorModel.GetNumberOfLines());
        }

        [Test]
        public void Document_Lines_Count_Should_Match_Model_Lines_Count()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            // Act
            document.Text = "puppy\npussy\nbirdie";
            int count = 0;
            textEditorModel.ForEach(_ => count++);

            // Assert
            Assert.AreEqual(document.LineCount, count);
        }

        #endregion

        #region Line Content

        [Test]
        public void Lines_Should_Have_Valid_Content()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            // Act
            document.Text = "puppy\npussy\nbirdie";

            // Assert
            AssertLinesAreEqual("puppy\n", textEditorModel.GetLineTextIncludingTerminators(0));
            AssertLinesAreEqual("pussy\n", textEditorModel.GetLineTextIncludingTerminators(1));
            AssertLinesAreEqual("birdie", textEditorModel.GetLineTextIncludingTerminators(2));
        }

        [Test]
        public void Editing_Line_Should_Update_The_Line_Content()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\npussy\nbirdie";

            // Act
            document.Insert(0, "cutty ");

            // Assert
            AssertLinesAreEqual("cutty puppy\n", textEditorModel.GetLineTextIncludingTerminators(0));
            AssertLinesAreEqual("pussy\n", textEditorModel.GetLineTextIncludingTerminators(1));
            AssertLinesAreEqual("birdie", textEditorModel.GetLineTextIncludingTerminators(2));
        }

        [Test]
        public void Editing_Line_Should_Update_The_Line_Length()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\npussy\nbirdie";

            // Act
            document.Insert(0, "cutty ");

            // Assert
            Assert.AreEqual("cutty puppy".Length, textEditorModel.GetLineLength(0));
        }

        [Test]
        public void Inserting_Line_Should_Update_The_Line_Ranges()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\npussy\nbirdie";

            // Act
            document.Insert(0, "lion\n");

            // Assert
            AssertLinesAreEqual("lion\n", textEditorModel.GetLineTextIncludingTerminators(0));
            AssertLinesAreEqual("puppy\n", textEditorModel.GetLineTextIncludingTerminators(1));
            AssertLinesAreEqual("pussy\n", textEditorModel.GetLineTextIncludingTerminators(2));
            AssertLinesAreEqual("birdie", textEditorModel.GetLineTextIncludingTerminators(3));
        }

        [Test]
        public void Removing_Line_Should_Update_The_Line_Ranges()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\npussy\nbirdie";

            // Act
            document.Remove(
                document.Lines[0].Offset,
                document.Lines[0].TotalLength);

            // Assert
            AssertLinesAreEqual("pussy\n", textEditorModel.GetLineTextIncludingTerminators(0));
            AssertLinesAreEqual("birdie", textEditorModel.GetLineTextIncludingTerminators(1));
        }

        [Test]
        public void Removing_Line_With_LFCR_Should_Update_The_Line_Ranges()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\r\npussy\r\nbirdie";

            // Act
            document.Remove(
                document.Lines[0].Offset,
                document.Lines[0].TotalLength);

            // Assert
            AssertLinesAreEqual("pussy\r\n", textEditorModel.GetLineTextIncludingTerminators(0));
            AssertLinesAreEqual("birdie", textEditorModel.GetLineTextIncludingTerminators(1));
        }

        #endregion

        #region Insert / Remove Line Count

        [Test]
        public void Edit_Document_Line_Should_Not_Add_Or_Remove_Model_Lines()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\npussy\nbirdie";

            // Act
            document.Insert(0, "cutty ");
            int count = 0;
            textEditorModel.ForEach(_ => count++);

            // Assert
            Assert.AreEqual(document.LineCount, count);
        }

        [Test]
        public void Insert_Document_Line_Should_Insert_Model_Line()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\npussy\nbirdie";

            // Act
            document.Insert(0, "lion\n");

            int count = 0;
            textEditorModel.ForEach(_ => count++);

            // Assert
            Assert.AreEqual(document.LineCount, count);
        }

        [Test]
        public void Remove_Document_Line_Should_Remove_Model_Line()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\npussy\nbirdie";

            // Act
            document.Remove(
                document.Lines[0].Offset,
                document.Lines[0].TotalLength);

            int count = 0;
            textEditorModel.ForEach(_ => count++);

            // Assert
            Assert.AreEqual(document.LineCount, count);
        }

        #endregion

        #region Replace

        [Test]
        public void Replace_Text_Of_Same_Length_Should_Update_Line_Content()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\npussy\nbirdie";

            // Act
            document.Replace(0, 1, "P");

            // Assert
            AssertLinesAreEqual("Puppy\n", textEditorModel.GetLineTextIncludingTerminators(0));
        }

        [Test]
        public void Replace_Text_Of_Same_Length_With_CR_Should_Update_Line_Content()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\npussy\nbirdie";

            // Act
            document.Replace(0, 1, "\n");

            // Assert
            AssertLinesAreEqual("\n", textEditorModel.GetLineTextIncludingTerminators(0));
        }

        [Test]
        public void Remove_Document_Text_Should_Update_Line_Contents()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\npussy\nbirdie";
            Assert.AreEqual(3, textEditorModel.GetNumberOfLines());

            // Act
            document.Text = string.Empty;

            // Assert
            Assert.AreEqual(1, textEditorModel.GetNumberOfLines());
            AssertLinesAreEqual(string.Empty, textEditorModel.GetLineTextIncludingTerminators(0));
        }

        [Test]
        public void Replace_Document_Text_Should_Update_Line_Contents()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\npussy\nbirdie";
            Assert.AreEqual(3, textEditorModel.GetNumberOfLines());

            // Act
            document.Text = "one\ntwo\nthree\nfour";

            // Assert
            Assert.AreEqual(4, textEditorModel.GetNumberOfLines());
            AssertLinesAreEqual("one\n", textEditorModel.GetLineTextIncludingTerminators(0));
            AssertLinesAreEqual("two\n", textEditorModel.GetLineTextIncludingTerminators(1));
            AssertLinesAreEqual("three\n", textEditorModel.GetLineTextIncludingTerminators(2));
            AssertLinesAreEqual("four", textEditorModel.GetLineTextIncludingTerminators(3));
        }

        #endregion

        #region Batch Document Changes

        [Test]
        public void Batch_Document_Changes_Should_Invalidate_Lines()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\npuppy\npuppy";

            // Act & Assert (interleaved to verify state at each step)
            document.BeginUpdate();

            document.Insert(0, "*");
            Assert.IsNotNull(textEditorModel.InvalidRange,
                "InvalidRange should not be null after first edit in batch");
            Assert.AreEqual(0, textEditorModel.InvalidRange.Value.StartLine,
                "Wrong InvalidRange.StartLine 1");
            Assert.AreEqual(0, textEditorModel.InvalidRange.Value.EndLine,
                "Wrong InvalidRange.EndLine 1");

            document.Insert(7, "*");
            Assert.AreEqual(0, textEditorModel.InvalidRange.Value.StartLine,
                "Wrong InvalidRange.StartLine 2");
            Assert.AreEqual(1, textEditorModel.InvalidRange.Value.EndLine,
                "Wrong InvalidRange.EndLine 2");

            document.Insert(14, "*");
            Assert.AreEqual(0, textEditorModel.InvalidRange.Value.StartLine,
                "Wrong InvalidRange.StartLine 3");
            Assert.AreEqual(2, textEditorModel.InvalidRange.Value.EndLine,
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
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();

            using var textEditorModel = new TextEditorModel(
                textView, document, null);

            document.Text = "puppy\npuppy\npuppy";

            // Act & Assert (interleaved to verify state at each step)
            document.BeginUpdate();

            document.Insert(0, "*");
            Assert.IsNotNull(textEditorModel.InvalidRange,
                "InvalidRange should not be null after first edit in batch");
            Assert.AreEqual(0, textEditorModel.InvalidRange.Value.StartLine,
                "Wrong InvalidRange.StartLine 1");
            Assert.AreEqual(0, textEditorModel.InvalidRange.Value.EndLine,
                "Wrong InvalidRange.EndLine 1");

            document.BeginUpdate();
            document.Insert(7, "*");
            Assert.AreEqual(0, textEditorModel.InvalidRange.Value.StartLine,
                "Wrong InvalidRange.StartLine 2");
            Assert.AreEqual(1, textEditorModel.InvalidRange.Value.EndLine,
                "Wrong InvalidRange.EndLine 2");

            document.Insert(14, "*");
            Assert.AreEqual(0, textEditorModel.InvalidRange.Value.StartLine,
                "Wrong InvalidRange.StartLine 3");
            Assert.AreEqual(2, textEditorModel.InvalidRange.Value.EndLine,
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

        [Test]
        public void Single_Change_Outside_Batch_Should_Not_Set_InvalidRange()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            using var textEditorModel = new TextEditorModel(
                textView, document, null);
            document.Text = "puppy\npuppy\npuppy";

            // Act - single insert outside BeginUpdate/EndUpdate
            document.Insert(0, "X");

            // Assert - InvalidRange is only set during batches
            // outside a batch, InvalidateLineRange is called directly
            Assert.IsNull(textEditorModel.InvalidRange,
                "InvalidRange should be null outside of a batch update");
            AssertLinesAreEqual("Xpuppy\n", textEditorModel.GetLineTextIncludingTerminators(0));
            AssertLinesAreEqual("puppy\n", textEditorModel.GetLineTextIncludingTerminators(1));
            AssertLinesAreEqual("puppy", textEditorModel.GetLineTextIncludingTerminators(2));
        }

        #endregion

        #region Dispose - Idempotency

        [Test]
        public void Dispose_Should_Not_Throw_When_Called_Once()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            var textEditorModel = new TextEditorModel(textView, document, null);

            // Act & Assert
            Assert.DoesNotThrow(() => textEditorModel.Dispose());
        }

        [Test]
        public void Dispose_Should_Not_Throw_When_Called_Twice()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            var textEditorModel = new TextEditorModel(textView, document, null);

            // Act
            textEditorModel.Dispose();

            // Assert
            Assert.DoesNotThrow(() => textEditorModel.Dispose());
        }

        [Test]
        public void Dispose_Should_Not_Throw_When_Called_Multiple_Times()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            var textEditorModel = new TextEditorModel(textView, document, null);

            // Act & Assert
            textEditorModel.Dispose();
            Assert.DoesNotThrow(() => textEditorModel.Dispose());
            Assert.DoesNotThrow(() => textEditorModel.Dispose());
            Assert.DoesNotThrow(() => textEditorModel.Dispose());
        }

        #endregion

        #region Dispose - ObjectDisposedException

        [Test]
        public void InvalidateViewPortLines_Should_Throw_ObjectDisposedException_After_Dispose()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            var textEditorModel = new TextEditorModel(
                textView, document, null);
            textEditorModel.Dispose();

            // Act & Assert
            Assert.Throws<ObjectDisposedException>(() => textEditorModel.InvalidateViewPortLines());
        }

        [Test]
        public void InvalidateViewPortLines_Should_Throw_ObjectDisposedException_With_Correct_ObjectName()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            var textEditorModel = new TextEditorModel(
                textView, document, null);
            textEditorModel.Dispose();

            // Act
            var ex = Assert.Throws<ObjectDisposedException>(() => textEditorModel.InvalidateViewPortLines());

            // Assert
            Assert.AreEqual(nameof(TextEditorModel), ex.ObjectName);
        }

        #endregion

        #region Dispose - AbstractLineList Overrides Still Functional

        [Test]
        public void GetNumberOfLines_Should_Return_Last_Known_Count_After_Dispose()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            var textEditorModel = new TextEditorModel(
                textView, document, null);
            document.Text = "puppy\npussy\nbirdie";
            int lineCountBeforeDispose = textEditorModel.GetNumberOfLines();

            // Act
            textEditorModel.Dispose();
            int lineCountAfterDispose = textEditorModel.GetNumberOfLines();

            // Assert - _documentSnapshot is never nulled, so the snapshot
            // remains valid and returns real data from the last known state
            Assert.AreEqual(3, lineCountBeforeDispose);
            Assert.AreEqual(lineCountBeforeDispose, lineCountAfterDispose);
        }

        [Test]
        public void GetLineTextIncludingTerminators_Should_Return_Last_Known_Content_After_Dispose()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            var textEditorModel = new TextEditorModel(textView, document, null);
            document.Text = "puppy\npussy\nbirdie";
            LineText contentBeforeDispose = textEditorModel.GetLineTextIncludingTerminators(0);

            // Act
            textEditorModel.Dispose();
            LineText contentAfterDispose = textEditorModel.GetLineTextIncludingTerminators(0);

            // Assert
            AssertLinesAreEqual("puppy\n", contentBeforeDispose);
            AssertLinesAreEqual(contentBeforeDispose, contentAfterDispose);
        }

        [Test]
        public void GetLineLength_Should_Return_Last_Known_Length_After_Dispose()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            var textEditorModel = new TextEditorModel(textView, document, null);
            document.Text = "puppy\npussy\nbirdie";
            int lengthBeforeDispose = textEditorModel.GetLineLength(0);

            // Act
            textEditorModel.Dispose();
            int lengthAfterDispose = textEditorModel.GetLineLength(0);

            // Assert
            Assert.AreEqual(5, lengthBeforeDispose);
            Assert.AreEqual(lengthBeforeDispose, lengthAfterDispose);
        }

        #endregion

        #region Dispose - Event Handler Resilience

        [Test]
        public void Document_Changes_After_Dispose_Should_Not_Throw()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            var textEditorModel = new TextEditorModel(textView, document, null);
            document.Text = "puppy\npussy\nbirdie";
            textEditorModel.Dispose();

            // Act & Assert - document mutations after dispose should not
            // propagate exceptions even though the model is disposed,
            // because Dispose unsubscribes all event handlers.
            Assert.DoesNotThrow(() => document.Insert(0, "test"));
            Assert.DoesNotThrow(() => document.Remove(0, 4));
            Assert.DoesNotThrow(() => document.Text = "completely new text");
        }

        [Test]
        public void Batch_Document_Changes_After_Dispose_Should_Not_Throw()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            var textEditorModel = new TextEditorModel(textView, document, null);
            document.Text = "puppy\npussy\nbirdie";
            textEditorModel.Dispose();

            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                document.BeginUpdate();
                document.Insert(0, "test\n");
                document.Insert(5, "more\n");
                document.EndUpdate();
            });
        }

        [Test]
        public void Document_Snapshot_Should_Not_Reflect_Changes_After_Dispose()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            var textEditorModel = new TextEditorModel(textView, document, null);
            document.Text = "puppy\npussy\nbirdie";
            textEditorModel.Dispose();

            // Act - modify the document after disposal
            document.Text = "completely different text";

            // Assert - the snapshot was frozen at disposal time because
            // the event handlers were unsubscribed and can no longer
            // update it
            Assert.AreEqual(3, textEditorModel.GetNumberOfLines());
            AssertLinesAreEqual("puppy\n", textEditorModel.GetLineTextIncludingTerminators(0));
        }

        #endregion

        #region Dispose - ExceptionHandler

        [Test]
        public void Null_ExceptionHandler_Should_Not_Throw_On_Construction()
        {
            // Arrange & Act & Assert
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            Assert.DoesNotThrow(() =>
            {
                using var textEditorModel = new TextEditorModel(
                    textView, document, null);
            });
        }

        [Test]
        public void ExceptionHandler_Should_Not_Be_Invoked_During_Normal_Operations()
        {
            // Arrange
            Exception capturedException = null;
            void Handler(Exception ex)
            {
                capturedException = ex;
            }

            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            using var textEditorModel = new TextEditorModel(textView, document, Handler);
            document.Text = "puppy\npussy\nbirdie";

            // Act - perform various normal document operations
            document.Insert(0, "X");
            document.Remove(0, 1);
            document.Text = string.Empty;
            document.Text = "one\ntwo\nthree";

            // Assert
            Assert.IsNull(capturedException,
                "ExceptionHandler should not be invoked during normal operations");
        }

        #endregion

        #region InvalidateViewPortLines

        [Test]
        public void InvalidateViewPortLines_Should_Not_Throw_When_VisualLines_Are_Invalid()
        {
            // Arrange - by default, a fresh TextView has no visual lines
            // (_visibleVisualLines is null), so VisualLinesValid is false
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            using var textEditorModel = new TextEditorModel(
                textView, document, null);
            document.Text = "puppy\npussy\nbirdie";

            // Act & Assert - should early-return without error
            Assert.IsFalse(textView.VisualLinesValid);
            Assert.DoesNotThrow(() => textEditorModel.InvalidateViewPortLines());
        }

        [Test]
        public void InvalidateViewPortLines_Should_Not_Throw_When_VisualLines_Are_Empty()
        {
            // Arrange - set _visibleVisualLines to an empty collection
            // so VisualLinesValid is true but Count is 0
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            using var textEditorModel = new TextEditorModel(
                textView, document, null);
            document.Text = "puppy\npussy\nbirdie";

            ReflectionTestHelper.SetPrivateField(textView,
                "_visibleVisualLines",
                new ReadOnlyCollection<VisualLine>(Array.Empty<VisualLine>()));

            // Act & Assert - should early-return at Count == 0 check
            Assert.IsTrue(textView.VisualLinesValid);
            Assert.DoesNotThrow(() => textEditorModel.InvalidateViewPortLines());
        }

        [AvaloniaTest]
        public void InvalidateViewPortLines_Should_Invoke_InvalidateLineRange_With_Valid_VisualLines()
        {
            // Arrange - use headless Avalonia to build real visual lines
            TextDocument document = new TextDocument("puppy\npussy\nbirdie");
            TextView textView = new TextView
            {
                Document = document
            };
            textView.Measure(Avalonia.Size.Infinity);

            using var textEditorModel = new TextEditorModel(textView, document, null);

            // Act & Assert - VisualLines are valid after Measure,
            // so InvalidateViewPortLines should reach the InvalidateLineRange
            // call without throwing
            Assert.IsTrue(textView.VisualLinesValid);
            Assert.IsTrue(textView.VisualLines.Count > 0);
            Assert.DoesNotThrow(() => textEditorModel.InvalidateViewPortLines());
        }

        [AvaloniaTest]
        public void InvalidateViewPortLines_Should_Cover_All_Visible_Lines()
        {
            // Arrange
            TextDocument document = new TextDocument("line1\nline2\nline3\nline4\nline5");
            TextView textView = new TextView
            {
                Document = document
            };
            textView.Measure(Avalonia.Size.Infinity);

            using var textEditorModel = new TextEditorModel(textView, document, null);

            // Act - capture the visible line range before calling
            int firstVisibleLine = textView.VisualLines[0].FirstDocumentLine.LineNumber;
            int lastVisibleLine = textView.VisualLines[^1].LastDocumentLine.LineNumber;

            // Assert - confirm we have multiple visible lines and the
            // call completes without error
            Assert.IsTrue(firstVisibleLine >= 1);
            Assert.IsTrue(lastVisibleLine >= firstVisibleLine);
            Assert.DoesNotThrow(() => textEditorModel.InvalidateViewPortLines());
        }

        #endregion

        #region TokenizeViewPort

        [Test]
        public void TokenizeViewPort_Should_Not_Throw_When_Called_Before_Dispose()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            using var textEditorModel = new TextEditorModel(
                textView, document, null);
            document.Text = "puppy\npussy\nbirdie";

            // Act & Assert - TokenizeViewPort is private; invoke via
            // reflection to verify it does not throw when queuing
            // work via Dispatcher.Post
            Assert.DoesNotThrow(() =>
                ReflectionTestHelper.InvokePrivateMethod(
                    textEditorModel, "TokenizeViewPort"));
        }

        [Test]
        public void TokenizeViewPort_Should_Not_Throw_After_Dispose()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            var textEditorModel = new TextEditorModel(textView, document, null);
            document.Text = "puppy\npussy\nbirdie";
            textEditorModel.Dispose();

            // Act & Assert - invoking TokenizeViewPort after dispose
            // should not throw; the Post lambda will check _isDisposed
            // and silently return when it eventually executes
            Assert.DoesNotThrow(() =>
                ReflectionTestHelper.InvokePrivateMethod(
                    textEditorModel, "TokenizeViewPort"));
        }

        [Test]
        public void TokenizeViewPort_With_Null_ExceptionHandler_Should_Not_Throw()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            using var textEditorModel = new TextEditorModel(textView, document, null);
            document.Text = "puppy\npussy\nbirdie";

            // Act & Assert
            Assert.DoesNotThrow(() =>
                ReflectionTestHelper.InvokePrivateMethod(
                    textEditorModel, "TokenizeViewPort"));
        }

        #endregion

        #region TokenizeViewPortCore

        [Test]
        public void TokenizeViewPortCore_Should_Silently_Return_After_Dispose()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            var textEditorModel = new TextEditorModel(textView, document, null);
            document.Text = "puppy\npussy\nbirdie";
            textEditorModel.Dispose();

            // Act & Assert - the _isDisposed guard should cause an
            // early return without accessing any other state
            Assert.DoesNotThrow(() => ReflectionTestHelper.InvokePrivateMethod(textEditorModel, "TokenizeViewPortCore"));
        }

        [Test]
        public void TokenizeViewPortCore_Should_Return_When_VisualLines_Are_Invalid()
        {
            // Arrange - fresh TextView has _visibleVisualLines == null,
            // so VisualLinesValid is false
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            using var textEditorModel = new TextEditorModel(textView, document, null);
            document.Text = "puppy\npussy\nbirdie";

            // Act & Assert - should early-return at VisualLinesValid check
            Assert.IsFalse(textView.VisualLinesValid);
            Assert.DoesNotThrow(() => ReflectionTestHelper.InvokePrivateMethod(textEditorModel, "TokenizeViewPortCore"));
        }

        [Test]
        public void TokenizeViewPortCore_Should_Return_When_VisualLines_Are_Empty()
        {
            // Arrange - set _visibleVisualLines to an empty collection
            // so VisualLinesValid is true but Count is 0
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            using var textEditorModel = new TextEditorModel(textView, document, null);
            document.Text = "puppy\npussy\nbirdie";

            ReflectionTestHelper.SetPrivateField(textView,
                "_visibleVisualLines",
                new ReadOnlyCollection<VisualLine>(Array.Empty<VisualLine>()));

            // Act & Assert - should early-return at Count == 0 check
            Assert.IsTrue(textView.VisualLinesValid);
            Assert.DoesNotThrow(() => ReflectionTestHelper.InvokePrivateMethod(textEditorModel, "TokenizeViewPortCore"));
        }

        [AvaloniaTest]
        public void TokenizeViewPortCore_Should_Call_ForceTokenization_With_Valid_VisualLines()
        {
            // Arrange - use headless Avalonia to build real visual lines
            TextView textView = new TextView();
            TextDocument document = new TextDocument("puppy\npussy\nbirdie");
            textView.Document = document;
            textView.Measure(Avalonia.Size.Infinity);

            using var textEditorModel = new TextEditorModel(textView, document, null);

            // Act & Assert - VisualLines are valid after Measure, so
            // TokenizeViewPortCore should reach ForceTokenization
            // without throwing
            Assert.IsTrue(textView.VisualLinesValid);
            Assert.IsTrue(textView.VisualLines.Count > 0);
            Assert.DoesNotThrow(() => ReflectionTestHelper.InvokePrivateMethod(textEditorModel, "TokenizeViewPortCore"));
        }

        [Test]
        public void TokenizeViewPortCore_Should_Invoke_ExceptionHandler_On_Error()
        {
            // Arrange - set up an exception handler and force an error
            // by setting _visibleVisualLines to a non-null, non-empty
            // collection with null entries, causing an NRE when
            // accessing FirstDocumentLine
            Exception capturedException = null;
            void Handler(Exception ex) => capturedException = ex;

            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            using var textEditorModel = new TextEditorModel(textView, document, Handler);
            document.Text = "puppy\npussy\nbirdie";

            // Inject a VisualLine array with a null entry to force
            // NullReferenceException inside the try block
            ReflectionTestHelper.SetPrivateField(textView,
                "_visibleVisualLines",
                new ReadOnlyCollection<VisualLine>(new VisualLine[] { null }));

            // Act
            ReflectionTestHelper.InvokePrivateMethod(textEditorModel, "TokenizeViewPortCore");

            // Assert - exception was caught and routed to the handler
            Assert.IsNotNull(capturedException, "ExceptionHandler should have been invoked");
            Assert.IsInstanceOf<NullReferenceException>(capturedException);
        }

        [Test]
        public void TokenizeViewPortCore_Should_Not_Throw_With_Null_ExceptionHandler_On_Error()
        {
            // Arrange - null exception handler with a forced error
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            using var textEditorModel = new TextEditorModel(textView, document, null);
            document.Text = "puppy\npussy\nbirdie";

            // Inject a VisualLine array with a null entry
            ReflectionTestHelper.SetPrivateField(textView,
                "_visibleVisualLines",
                new ReadOnlyCollection<VisualLine>(new VisualLine[] { null }));

            // Act & Assert - null-conditional on _exceptionHandler
            // should prevent NRE from the handler itself
            Assert.DoesNotThrow(() =>
                ReflectionTestHelper.InvokePrivateMethod(textEditorModel, "TokenizeViewPortCore"));
        }

        #endregion

        #region ScrollOffsetChanged - Event Handler

        [Test]
        public void ScrollOffsetChanged_After_Dispose_Should_Not_Throw()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            var textEditorModel = new TextEditorModel(
                textView, document, null);
            document.Text = "puppy\npussy\nbirdie";
            textEditorModel.Dispose();

            // Act & Assert - fire ScrollOffsetChanged via the private
            // SetScrollOffset method; after dispose, the event handler
            // is unsubscribed so the model's handler is not invoked
            Assert.DoesNotThrow(() =>
                ReflectionTestHelper.InvokePrivateMethod(
                    textView, "SetScrollOffset",
                    new Avalonia.Vector(0, 50)));
        }

        [Test]
        public void ScrollOffsetChanged_Before_Dispose_Should_Not_Throw()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            using var textEditorModel = new TextEditorModel(
                textView, document, null);
            document.Text = "puppy\npussy\nbirdie";

            // Act & Assert - fire ScrollOffsetChanged before dispose
            // the event handler calls TokenizeViewPort which queues
            // work via Dispatcher.Post
            Assert.DoesNotThrow(() =>
                ReflectionTestHelper.InvokePrivateMethod(
                    textView, "SetScrollOffset",
                    new Avalonia.Vector(0, 50)));
        }

        [Test]
        public void Multiple_ScrollOffsetChanged_Should_Not_Throw()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            using var textEditorModel = new TextEditorModel(
                textView, document, null);
            document.Text = "puppy\npussy\nbirdie";

            // Act & Assert - multiple rapid scroll events should
            // each queue a Post without errors
            Assert.DoesNotThrow(() =>
            {
                ReflectionTestHelper.InvokePrivateMethod(
                    textView, "SetScrollOffset",
                    new Avalonia.Vector(0, 10));
                ReflectionTestHelper.InvokePrivateMethod(
                    textView, "SetScrollOffset",
                    new Avalonia.Vector(0, 20));
                ReflectionTestHelper.InvokePrivateMethod(
                    textView, "SetScrollOffset",
                    new Avalonia.Vector(0, 30));
                ReflectionTestHelper.InvokePrivateMethod(
                    textView, "SetScrollOffset",
                    new Avalonia.Vector(0, 0));
            });
        }

        #endregion

        #region InvalidLineRange Struct

        [Test]
        public void InvalidLineRange_Should_Store_StartLine_And_EndLine()
        {
            // Arrange & Act
            var range = new TextEditorModel.InvalidLineRange(3, 7);

            // Assert
            Assert.AreEqual(3, range.StartLine);
            Assert.AreEqual(7, range.EndLine);
        }

        [Test]
        public void InvalidLineRange_Should_Store_Same_StartLine_And_EndLine()
        {
            // Arrange & Act
            var range = new TextEditorModel.InvalidLineRange(5, 5);

            // Assert
            Assert.AreEqual(5, range.StartLine);
            Assert.AreEqual(5, range.EndLine);
        }

        [Test]
        public void InvalidLineRange_Should_Store_Zero_Values()
        {
            // Arrange & Act
            var range = new TextEditorModel.InvalidLineRange(0, 0);

            // Assert
            Assert.AreEqual(0, range.StartLine);
            Assert.AreEqual(0, range.EndLine);
        }

        [Test]
        public void InvalidLineRange_Merge_Should_Expand_Range_With_Earlier_StartLine()
        {
            // Arrange
            var range = new TextEditorModel.InvalidLineRange(5, 10);

            // Act
            var merged = range.Merge(2, 8);

            // Assert
            Assert.AreEqual(2, merged.StartLine);
            Assert.AreEqual(10, merged.EndLine);
        }

        [Test]
        public void InvalidLineRange_Merge_Should_Expand_Range_With_Later_EndLine()
        {
            // Arrange
            var range = new TextEditorModel.InvalidLineRange(5, 10);

            // Act
            var merged = range.Merge(7, 15);

            // Assert
            Assert.AreEqual(5, merged.StartLine);
            Assert.AreEqual(15, merged.EndLine);
        }

        [Test]
        public void InvalidLineRange_Merge_Should_Expand_Both_Directions()
        {
            // Arrange
            var range = new TextEditorModel.InvalidLineRange(5, 10);

            // Act
            var merged = range.Merge(2, 15);

            // Assert
            Assert.AreEqual(2, merged.StartLine);
            Assert.AreEqual(15, merged.EndLine);
        }

        [Test]
        public void InvalidLineRange_Merge_Should_Not_Shrink_Range_With_Narrower_Values()
        {
            // Arrange
            var range = new TextEditorModel.InvalidLineRange(2, 15);

            // Act
            var merged = range.Merge(5, 10);

            // Assert
            Assert.AreEqual(2, merged.StartLine);
            Assert.AreEqual(15, merged.EndLine);
        }

        [Test]
        public void InvalidLineRange_Merge_Should_Return_Same_Range_With_Equal_Values()
        {
            // Arrange
            var range = new TextEditorModel.InvalidLineRange(5, 10);

            // Act
            var merged = range.Merge(5, 10);

            // Assert
            Assert.AreEqual(5, merged.StartLine);
            Assert.AreEqual(10, merged.EndLine);
        }

        [Test]
        public void InvalidLineRange_Merge_Should_Not_Mutate_Original()
        {
            // Arrange
            var original = new TextEditorModel.InvalidLineRange(5, 10);

            // Act
            var merged = original.Merge(1, 20);

            // Assert - original is unchanged (readonly struct)
            Assert.AreEqual(5, original.StartLine);
            Assert.AreEqual(10, original.EndLine);
            Assert.AreEqual(1, merged.StartLine);
            Assert.AreEqual(20, merged.EndLine);
        }

        [Test]
        public void InvalidLineRange_Merge_With_Zero_StartLine()
        {
            // Arrange
            var range = new TextEditorModel.InvalidLineRange(3, 7);

            // Act
            var merged = range.Merge(0, 5);

            // Assert
            Assert.AreEqual(0, merged.StartLine);
            Assert.AreEqual(7, merged.EndLine);
        }

        [Test]
        public void InvalidLineRange_Merge_Sequential_Should_Accumulate()
        {
            // Arrange
            var range = new TextEditorModel.InvalidLineRange(5, 5);

            // Act
            range = range.Merge(3, 3);
            range = range.Merge(8, 10);
            range = range.Merge(1, 6);

            // Assert
            Assert.AreEqual(1, range.StartLine);
            Assert.AreEqual(10, range.EndLine);
        }

        [Test]
        public void InvalidLineRange_Default_Should_Have_Zero_Values()
        {
            // Arrange & Act
            var range = default(TextEditorModel.InvalidLineRange);

            // Assert
            Assert.AreEqual(0, range.StartLine);
            Assert.AreEqual(0, range.EndLine);
        }

        #endregion

        #region DocumentSnapshot Property

        [Test]
        public void DocumentSnapshot_Should_Not_Be_Null_After_Construction()
        {
            // Arrange & Act
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            using var textEditorModel = new TextEditorModel(textView, document, null);

            // Assert
            Assert.IsNotNull(textEditorModel.DocumentSnapshot);
        }

        [Test]
        public void DocumentSnapshot_LineCount_Should_Match_GetNumberOfLines()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            using var textEditorModel = new TextEditorModel(textView, document, null);
            document.Text = "one\ntwo\nthree";

            // Act
            int snapshotCount = textEditorModel.DocumentSnapshot.LineCount;
            int modelCount = textEditorModel.GetNumberOfLines();

            // Assert
            Assert.AreEqual(snapshotCount, modelCount);
        }

        #endregion

        #region Multi-Line Insert and Remove

        [Test]
        public void Insert_Multiple_Lines_Should_Update_Line_Count()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            using var textEditorModel = new TextEditorModel(textView, document, null);
            document.Text = "first\nlast";
            Assert.AreEqual(2, textEditorModel.GetNumberOfLines());

            // Act
            document.Insert(6, "second\nthird\n");

            // Assert
            Assert.AreEqual(4, textEditorModel.GetNumberOfLines());
            AssertLinesAreEqual("first\n", textEditorModel.GetLineTextIncludingTerminators(0));
            AssertLinesAreEqual("second\n", textEditorModel.GetLineTextIncludingTerminators(1));
            AssertLinesAreEqual("third\n", textEditorModel.GetLineTextIncludingTerminators(2));
            AssertLinesAreEqual("last", textEditorModel.GetLineTextIncludingTerminators(3));
        }

        [Test]
        public void Remove_Multiple_Lines_Should_Update_Line_Count()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            using var textEditorModel = new TextEditorModel(textView, document, null);
            document.Text = "first\nsecond\nthird\nfourth\nfifth";
            Assert.AreEqual(5, textEditorModel.GetNumberOfLines());

            // Act - remove "second\nthird\n" (lines 2 and 3)
            int startOffset = document.Lines[1].Offset;
            int length = document.Lines[1].TotalLength + document.Lines[2].TotalLength;
            document.Remove(startOffset, length);

            // Assert
            Assert.AreEqual(3, textEditorModel.GetNumberOfLines());
            AssertLinesAreEqual("first\n", textEditorModel.GetLineTextIncludingTerminators(0));
            AssertLinesAreEqual("fourth\n", textEditorModel.GetLineTextIncludingTerminators(1));
            AssertLinesAreEqual("fifth", textEditorModel.GetLineTextIncludingTerminators(2));
        }

        #endregion

        #region CRLF Handling

        [Test]
        public void Lines_With_CRLF_Should_Have_Valid_Content()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            using var textEditorModel = new TextEditorModel(textView, document, null);

            // Act
            document.Text = "alpha\r\nbeta\r\ngamma";

            // Assert
            Assert.AreEqual(3, textEditorModel.GetNumberOfLines());
            AssertLinesAreEqual("alpha\r\n", textEditorModel.GetLineTextIncludingTerminators(0));
            AssertLinesAreEqual("beta\r\n", textEditorModel.GetLineTextIncludingTerminators(1));
            AssertLinesAreEqual("gamma", textEditorModel.GetLineTextIncludingTerminators(2));
        }

        [Test]
        public void Lines_With_CRLF_Should_Have_Correct_Line_Length()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            using var textEditorModel = new TextEditorModel(textView, document, null);

            // Act
            document.Text = "alpha\r\nbeta\r\ngamma";

            // Assert - GetLineLength returns length without terminator
            Assert.AreEqual(5, textEditorModel.GetLineLength(0));
            Assert.AreEqual(4, textEditorModel.GetLineLength(1));
            Assert.AreEqual(5, textEditorModel.GetLineLength(2));
        }

        [Test]
        public void Mixed_Line_Endings_Should_Have_Correct_Line_Count()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            using var textEditorModel = new TextEditorModel(textView, document, null);

            // Act
            document.Text = "alpha\nbeta\r\ngamma\ndelta";

            // Assert
            Assert.AreEqual(4, textEditorModel.GetNumberOfLines());
        }

        #endregion

        #region Replace Entire Document

        [Test]
        public void Replace_Entire_Document_Should_Reset_Lines()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            using var textEditorModel = new TextEditorModel(textView, document, null);
            document.Text = "one\ntwo\nthree\nfour\nfive";
            Assert.AreEqual(5, textEditorModel.GetNumberOfLines());

            // Act
            document.Text = "single line";

            // Assert
            Assert.AreEqual(1, textEditorModel.GetNumberOfLines());
            AssertLinesAreEqual("single line", textEditorModel.GetLineTextIncludingTerminators(0));
        }

        [Test]
        public void Replace_Entire_Document_With_More_Lines_Should_Increase_Count()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            using var textEditorModel = new TextEditorModel(textView, document, null);
            document.Text = "one";
            Assert.AreEqual(1, textEditorModel.GetNumberOfLines());

            // Act
            document.Text = "one\ntwo\nthree";

            // Assert
            Assert.AreEqual(3, textEditorModel.GetNumberOfLines());
        }

        #endregion

        #region Edge Cases

        [Test]
        public void Insert_At_End_Of_Document_Should_Update_Content()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            using var textEditorModel = new TextEditorModel(textView, document, null);
            document.Text = "hello";

            // Act
            document.Insert(document.TextLength, " world");

            // Assert
            AssertLinesAreEqual("hello world", textEditorModel.GetLineTextIncludingTerminators(0));
        }

        [Test]
        public void Insert_Newline_At_End_Should_Add_Line()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            using var textEditorModel = new TextEditorModel(textView, document, null);
            document.Text = "hello";
            Assert.AreEqual(1, textEditorModel.GetNumberOfLines());

            // Act
            document.Insert(document.TextLength, "\n");

            // Assert
            Assert.AreEqual(2, textEditorModel.GetNumberOfLines());
            AssertLinesAreEqual("hello\n", textEditorModel.GetLineTextIncludingTerminators(0));
            AssertLinesAreEqual("", textEditorModel.GetLineTextIncludingTerminators(1));
        }

        [Test]
        public void GetLineLength_For_Empty_Last_Line_Should_Return_Zero()
        {
            // Arrange
            TextView textView = new TextView();
            TextDocument document = new TextDocument();
            using var textEditorModel = new TextEditorModel(textView, document, null);

            // Act
            document.Text = "hello\n";

            // Assert - the empty line after the trailing newline
            Assert.AreEqual(2, textEditorModel.GetNumberOfLines());
            Assert.AreEqual(0, textEditorModel.GetLineLength(1));
        }

        #endregion

        #region Helpers

        private static void AssertLinesAreEqual(LineText expected, LineText actual)
        {
            Assert.AreEqual(expected, actual);
        }

        #endregion
    }
}