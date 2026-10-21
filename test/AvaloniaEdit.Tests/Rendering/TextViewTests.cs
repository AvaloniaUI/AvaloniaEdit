using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.NUnit;
using AvaloniaEdit.AvaloniaMocks;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace AvaloniaEdit.Tests.Rendering
{
    internal class TextViewTests
    {
        // https://github.com/AvaloniaUI/Avalonia/blob/master/src/Headless/Avalonia.Headless/HeadlessPlatformStubs.cs#L126
        private const int HeadlessGlyphAdvance = 8;
        
        [AvaloniaTest]
        public void Visual_Line_Should_Create_Two_Text_Lines_When_Wrapping()
        {
            TextView textView = new TextView();

            TextDocument document = new TextDocument("hello world".ToCharArray());   

            textView.Document = document;

            ((ILogicalScrollable)textView).CanHorizontallyScroll = false;
            textView.Width = HeadlessGlyphAdvance * 8;

            textView.Measure(Size.Infinity);

            VisualLine visualLine = textView.GetOrConstructVisualLine(document.Lines[0]);

            Assert.AreEqual(2, visualLine.TextLines.Count);
            Assert.AreEqual("hello ", new string(visualLine.TextLines[0].TextRuns[0].Text.Span));
            Assert.AreEqual("world", new string(visualLine.TextLines[1].TextRuns[0].Text.Span));
        }

        [AvaloniaTest]
        public void Visual_Line_Should_Create_One_Text_Lines_When_Not_Wrapping()
        {
            TextView textView = new TextView();

            TextDocument document = new TextDocument("hello world".ToCharArray());

            textView.Document = document;
            textView.EnsureVisualLines();
            ((ILogicalScrollable)textView).CanHorizontallyScroll = false;
            textView.Width = HeadlessGlyphAdvance * 500;

            textView.Measure(Size.Infinity);

            VisualLine visualLine = textView.GetOrConstructVisualLine(document.Lines[0]);

            Assert.AreEqual(1, visualLine.TextLines.Count);
            Assert.AreEqual("hello world", new string(visualLine.TextLines[0].TextRuns[0].Text.Span));
        }
    

        [AvaloniaTest]
        public void Setting_Offset_Directly_Should_Raise_ScrollInvalidated()
        {
            TextView textView = new TextView();
            textView.Document = new TextDocument(string.Join("\n", Enumerable.Repeat("line", 100)));
            textView.Measure(new Size(200, 100));
            textView.Arrange(new Rect(0, 0, 200, 100));

            int scrollInvalidatedCount = 0;
            ((ILogicalScrollable)textView).ScrollInvalidated += (_, _) => scrollInvalidatedCount++;

            ((IScrollable)textView).Offset = new Vector(0, 50);

            Assert.AreEqual(1, scrollInvalidatedCount);
            Assert.AreEqual(new Vector(0, 50), textView.ScrollOffset);
        }

        [AvaloniaTest]
        public void Setting_Offset_Directly_Should_Not_Be_Reverted_By_ScrollViewer_On_Extent_Change()
        {
            UnitTestApplication.InitializeStyles();

            TextEditor textEditor = CreateEditor(string.Join("\n", Enumerable.Repeat("line", 200)));
            TextView textView = textEditor.TextArea.TextView;
            ScrollViewer scrollViewer = textEditor.ScrollViewer;

            // Scroll the view directly, bypassing the owning ScrollViewer.
            ((IScrollable)textView).Offset = new Vector(0, 500);
            textEditor.UpdateLayout();

            // Changing the extent makes the ScrollViewer re-sync with the TextView.
            textEditor.Document.Insert(textEditor.Document.TextLength, "\nanother line");
            textEditor.UpdateLayout();

            Assert.AreEqual(new Vector(0, 500), textView.ScrollOffset);
            Assert.AreEqual(new Vector(0, 500), scrollViewer.Offset);
        }

        [AvaloniaTest]
        public void Horizontal_Extent_Should_Be_Content_Width_By_Default()
        {
            TextView textView = CreateTextView("hello");

            Assert.AreEqual(0, textView.MinimumHorizontalExtent);
            Assert.Greater(textView.ContentWidth, 0);
            Assert.AreEqual(textView.ContentWidth, ((IScrollable)textView).Extent.Width);

            // The content is narrower than the viewport, so there is nothing to scroll.
            ((IScrollable)textView).Offset = new Vector(50, 0);
            LayoutTextView(textView);

            Assert.AreEqual(0, textView.HorizontalOffset);
        }

        [AvaloniaTest]
        public void MinimumHorizontalExtent_Should_Raise_Extent_Width()
        {
            TextView textView = CreateTextView("hello");
            double contentWidth = textView.ContentWidth;

            textView.MinimumHorizontalExtent = 500;
            LayoutTextView(textView);

            Assert.AreEqual(500, ((IScrollable)textView).Extent.Width);
            Assert.AreEqual(contentWidth, textView.ContentWidth);
        }

        [AvaloniaTest]
        public void MinimumHorizontalExtent_Should_Not_Raise_Extent_Width_Below_Content_Width()
        {
            TextView textView = CreateTextView("hello");
            double contentWidth = textView.ContentWidth;

            textView.MinimumHorizontalExtent = contentWidth / 2;
            LayoutTextView(textView);

            Assert.AreEqual(contentWidth, ((IScrollable)textView).Extent.Width);
            Assert.AreEqual(contentWidth, textView.ContentWidth);
        }

        [AvaloniaTest]
        public void MinimumHorizontalExtent_Should_Allow_Scrolling_Up_To_Extent_Minus_Viewport()
        {
            TextView textView = CreateTextView("hello");
            textView.MinimumHorizontalExtent = 500;
            LayoutTextView(textView);

            double maxOffset = 500 - ((IScrollable)textView).Viewport.Width;

            ((IScrollable)textView).Offset = new Vector(maxOffset, 0);
            LayoutTextView(textView);
            Assert.AreEqual(maxOffset, textView.HorizontalOffset);

            ((IScrollable)textView).Offset = new Vector(maxOffset + 50, 0);
            LayoutTextView(textView);
            Assert.AreEqual(maxOffset, textView.HorizontalOffset);
        }

        [AvaloniaTest]
        public void MinimumHorizontalExtent_Should_Be_Honored_By_ScrollViewer()
        {
            UnitTestApplication.InitializeStyles();

            TextEditor textEditor = CreateEditor("hello");
            TextView textView = textEditor.TextArea.TextView;
            ScrollViewer scrollViewer = textEditor.ScrollViewer;

            textView.MinimumHorizontalExtent = 1000;
            textEditor.UpdateLayout();

            Assert.AreEqual(1000, scrollViewer.Extent.Width);

            double maxOffset = scrollViewer.Extent.Width - scrollViewer.Viewport.Width;
            scrollViewer.Offset = new Vector(maxOffset, 0);
            textEditor.UpdateLayout();

            Assert.AreEqual(maxOffset, scrollViewer.Offset.X);
            Assert.AreEqual(maxOffset, textView.HorizontalOffset);
        }

        private static TextView CreateTextView(string text)
        {
            TextView textView = new TextView();
            textView.Document = new TextDocument(text);
            LayoutTextView(textView);
            return textView;
        }

        private static void LayoutTextView(TextView textView)
        {
            textView.Measure(new Size(200, 100));
            textView.Arrange(new Rect(0, 0, 200, 100));
        }

        private static TextEditor CreateEditor(string text)
        {
            TextEditor textEditor = new TextEditor
            {
                Document = new TextDocument(text)
            };

            Window window = new Window
            {
                Width = 200,
                Height = 200,
                Content = textEditor
            };

            window.Show();
            textEditor.ApplyTemplate();
            textEditor.UpdateLayout();

            return textEditor;
        }
    }
}
