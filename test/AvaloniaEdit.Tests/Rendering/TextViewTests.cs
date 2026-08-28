using System.Text;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
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

            Assert.AreEqual("a\tb", RenderedText(visualLine));
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

        private static TextView CreateTextView(string text)
        {
            TextView textView = new TextView
            {
                Document = new TextDocument(text.ToCharArray()),
                Width = HeadlessGlyphAdvance * 500
            };
            textView.Options.IndentationSize = 4;
            textView.Options.ShowTabs = true;

            ((ILogicalScrollable)textView).CanHorizontallyScroll = false;
            textView.Measure(Size.Infinity);

            return textView;
        }

        /// <summary>
        /// Gets the visual column at which the text after the last tab of the line starts.
        /// </summary>
        private static int GetColumnAfterLastTab(TextView textView, int lineIndex)
        {
            DocumentLine line = textView.Document.Lines[lineIndex];
            VisualLine visualLine = textView.GetOrConstructVisualLine(line);
            int lastTab = textView.Document.GetText(line).LastIndexOf('\t');

            return visualLine.GetVisualColumn(lastTab + 1);
        }

        private static string RenderedText(VisualLine visualLine)
        {
            StringBuilder text = new StringBuilder();

            foreach (TextRun textRun in visualLine.TextLines[0].TextRuns)
                text.Append(textRun.Text.Span);

            return text.ToString();
        }
    }
}
