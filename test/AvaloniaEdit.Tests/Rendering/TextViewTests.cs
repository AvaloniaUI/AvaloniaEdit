using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using AvaloniaEdit.AvaloniaMocks;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Folding;
using AvaloniaEdit.Rendering;
using Assert = NUnit.Framework.Legacy.ClassicAssert;
using LogicalDirection = AvaloniaEdit.Document.LogicalDirection;

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
        public void Tab_Should_Not_Be_Rendered_As_A_Control_Character_Box()
        {
            TextView textView = new TextView
            {
                Document = new TextDocument("a\tb".ToCharArray()),
                Width = HeadlessGlyphAdvance * 500
            };

            ((ILogicalScrollable)textView).CanHorizontallyScroll = false;
            textView.Measure(Size.Infinity);

            VisualLine visualLine = textView.GetOrConstructVisualLine(textView.Document.Lines[0]);

            Assert.AreEqual("a   b", RenderedText(visualLine));
        }

        [AvaloniaTest]
        public void Tab_Should_Span_The_Same_Columns_Whether_Or_Not_The_Glyph_Is_Shown()
        {
            Assert.AreEqual(3, GetTabColumnCount(CreateTextView("a\tb", showTabs: true), lineIndex: 0));
            Assert.AreEqual(3, GetTabColumnCount(CreateTextView("a\tb", showTabs: false), lineIndex: 0));
        }

        [AvaloniaTest]
        public void Tab_Should_Reach_The_Next_Tab_Stop()
        {
            TextView textView = CreateTextView("a\tb\naaa\tb\nabcd\tb");

            Assert.AreEqual(4, GetColumnAfterLastTab(textView, lineIndex: 0),
                "a tab starting at column 1 should reach column 4");
            Assert.AreEqual(4, GetColumnAfterLastTab(textView, lineIndex: 1),
                "a tab starting at column 3 should reach column 4");
            Assert.AreEqual(8, GetColumnAfterLastTab(textView, lineIndex: 2),
                "a tab starting on a tab stop should reach the next one");
        }

        [AvaloniaTest]
        public void Consecutive_Tabs_Should_Reach_Consecutive_Tab_Stops()
        {
            TextView textView = CreateTextView("a\t\tb");

            Assert.AreEqual(8, GetColumnAfterLastTab(textView, lineIndex: 0));
        }

        [AvaloniaTest]
        public void Tab_Should_Be_As_Wide_On_Screen_As_The_Columns_It_Spans()
        {
            TextView textView = CreateTextView("a\tb");

            VisualLine visualLine = textView.GetOrConstructVisualLine(textView.Document.Lines[0]);

            Assert.AreEqual(textView.WideSpaceWidth * 4,
                visualLine.TextLines[0].GetDistanceFromCharacterHit(new CharacterHit(4)));
        }

        [AvaloniaTest]
        public void Tab_Should_Reach_The_Next_Tab_Stop_On_A_Line_Too_Long_For_Element_Generation()
        {
            TextView textView = CreateTextView("a\t" + new string('b', VisualLine.LENGTH_LIMIT));

            Assert.AreEqual(4, GetColumnAfterLastTab(textView, lineIndex: 0));
        }

        [AvaloniaTest]
        public void Tab_After_A_Control_Character_Box_Should_Reach_The_Next_Tab_Stop()
        {
            // the box shows the character name, NUL, so the tab starts at column 3
            TextView textView = CreateTextView("\0\tb");

            Assert.AreEqual(1, GetTabColumnCount(textView, lineIndex: 0));
        }

        [AvaloniaTest]
        public void Tab_After_A_Collapsed_Folding_Should_Reach_The_Next_Tab_Stop()
        {
            TextArea textArea = new TextArea();
            TextView textView = textArea.TextView;

            textArea.Document = new TextDocument("a(\nx\n)\tb".ToCharArray());
            textView.Options.IndentationSize = 4;
            textView.Options.ShowTabs = true;
            textView.Width = HeadlessGlyphAdvance * 500;

            FoldingManager foldingManager = FoldingManager.Install(textArea);
            // folds "(\nx\n" away, leaving "a" + "..." + ")" before the tab, so it starts at column 5
            foldingManager.CreateFolding(1, 5).IsFolded = true;

            ((ILogicalScrollable)textView).CanHorizontallyScroll = false;
            textView.Measure(Size.Infinity);

            Assert.AreEqual(3, GetTabColumnCount(textView, lineIndex: 0));
        }

        [AvaloniaTest]
        public void Caret_Should_Step_Over_A_Tab_Rather_Than_Into_It()
        {
            TextView textView = CreateTextView("a\tb");
            VisualLine visualLine = GetVisualLine(textView, lineIndex: 0);

            // the tab starts at column 1 and reaches column 4
            Assert.AreEqual(4, visualLine.GetNextCaretPosition(1, LogicalDirection.Forward,
                CaretPositioningMode.Normal, allowVirtualSpace: false));
            Assert.AreEqual(1, visualLine.GetNextCaretPosition(4, LogicalDirection.Backward,
                CaretPositioningMode.Normal, allowVirtualSpace: false));
        }

        [AvaloniaTest]
        public void Columns_Inside_A_Tab_Should_Map_To_Its_Offset()
        {
            TextView textView = CreateTextView("a\tb");
            VisualLine visualLine = GetVisualLine(textView, lineIndex: 0);

            Assert.AreEqual(1, visualLine.GetRelativeOffset(2));
            Assert.AreEqual(1, visualLine.GetRelativeOffset(3));
        }

        private static TextView CreateTextView(string text, bool showTabs = true)
        {
            TextView textView = new TextView
            {
                Document = new TextDocument(text.ToCharArray()),
                Width = HeadlessGlyphAdvance * 500
            };
            textView.Options.IndentationSize = 4;
            textView.Options.ShowTabs = showTabs;

            ((ILogicalScrollable)textView).CanHorizontallyScroll = false;
            textView.Measure(Size.Infinity);

            return textView;
        }

        /// <summary>
        /// Gets the visual column at which the text after the last tab of the line starts.
        /// </summary>
        private static int GetColumnAfterLastTab(TextView textView, int lineIndex)
        {
            VisualLine visualLine = GetVisualLine(textView, lineIndex);
            int lastTab = GetCoveredText(textView, visualLine).LastIndexOf('\t');

            return visualLine.GetVisualColumn(lastTab + 1);
        }

        /// <summary>
        /// Gets the number of columns spanned by the first tab of the line.
        /// </summary>
        private static int GetTabColumnCount(TextView textView, int lineIndex)
        {
            VisualLine visualLine = GetVisualLine(textView, lineIndex);
            int tab = GetCoveredText(textView, visualLine).IndexOf('\t');

            return visualLine.GetVisualColumn(tab + 1) - visualLine.GetVisualColumn(tab);
        }

        private static VisualLine GetVisualLine(TextView textView, int lineIndex)
        {
            return textView.GetOrConstructVisualLine(textView.Document.Lines[lineIndex]);
        }

        /// <summary>
        /// Gets the document text the visual line covers, which spans several document lines when a
        /// folding between them is collapsed. Offsets into it are relative to the visual line start.
        /// </summary>
        private static string GetCoveredText(TextView textView, VisualLine visualLine)
        {
            return textView.Document.GetText(visualLine.StartOffset,
                visualLine.LastDocumentLine.EndOffset - visualLine.StartOffset);
        }

        private static string RenderedText(VisualLine visualLine)
        {
            StringBuilder text = new StringBuilder();

            foreach (TextRun textRun in visualLine.TextLines[0].TextRuns)
                text.Append(textRun.Text.Span);

            return text.ToString();
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
