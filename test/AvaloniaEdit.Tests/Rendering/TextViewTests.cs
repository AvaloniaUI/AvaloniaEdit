using System.Text;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.NUnit;
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
        public void Tab_Should_Reach_The_Next_Tab_Stop()
        {
            TextView textView = new TextView();

            TextDocument document = new TextDocument("a\tb\naaa\tb\nabcd\tb".ToCharArray());

            textView.Document = document;
            textView.Options.IndentationSize = 4;

            ((ILogicalScrollable)textView).CanHorizontallyScroll = false;
            textView.Width = HeadlessGlyphAdvance * 500;

            textView.Measure(Size.Infinity);

            Assert.AreEqual("a   b", GetVisualLineText(textView, document, 0),
                "a tab starting at column 1 should reach column 4");
            Assert.AreEqual("aaa b", GetVisualLineText(textView, document, 1),
                "a tab starting at column 3 should reach column 4");
            Assert.AreEqual("abcd    b", GetVisualLineText(textView, document, 2),
                "a tab starting on a tab stop should reach the next one");
        }

        private static string GetVisualLineText(TextView textView, TextDocument document, int lineIndex)
        {
            VisualLine visualLine = textView.GetOrConstructVisualLine(document.Lines[lineIndex]);

            StringBuilder text = new StringBuilder();

            foreach (TextRun textRun in visualLine.TextLines[0].TextRuns)
                text.Append(textRun.Text.Span);

            return text.ToString();
        }
    }
}
