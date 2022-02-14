using Avalonia.Controls;
using Avalonia.Controls.Primitives;

using AvaloniaEdit.AvaloniaMocks;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.Text;

using NUnit.Framework;

namespace AvaloniaEdit.Tests.Rendering
{
    [TestFixture]
    internal class TextViewTests
    {
        [Test]
        public void Visual_Line_Should_Create_Two_Text_Lines_When_Wrapping()
        {
            using var app = UnitTestApplication.Start(TestServices.StyledWindow);

            TextView textView = new TextView();

            TextDocument document = new TextDocument("hello world".ToCharArray());

            textView.Document = document;
            textView.EnsureVisualLines();
            ((ILogicalScrollable)textView).CanHorizontallyScroll = false;
            textView.Width = MockGlyphTypeface.GlyphAdvance * 8;

            Window window = new Window();
            window.Content = textView;
            window.Show();

            VisualLine visualLine = textView.GetOrConstructVisualLine(document.Lines[0]);

            Assert.AreEqual(2, visualLine.TextLines.Count);
            Assert.AreEqual("hello ", ((TextLineImpl)visualLine.TextLines[0]).LineRuns[0].StringRange.ToString());
            Assert.AreEqual("world", ((TextLineImpl)visualLine.TextLines[1]).LineRuns[0].StringRange.ToString());

            window.Close();
        }

        [Test]
        public void Visual_Line_Should_Create_One_Text_Lines_When_Not_Wrapping()
        {
            using var app = UnitTestApplication.Start(TestServices.StyledWindow);

            TextView textView = new TextView();

            TextDocument document = new TextDocument("hello world".ToCharArray());

            textView.Document = document;
            textView.EnsureVisualLines();
            ((ILogicalScrollable)textView).CanHorizontallyScroll = false;
            textView.Width = MockGlyphTypeface.GlyphAdvance * 500;

            Window window = new Window();
            window.Content = textView;
            window.Show();

            VisualLine visualLine = textView.GetOrConstructVisualLine(document.Lines[0]);

            Assert.AreEqual(1, visualLine.TextLines.Count);
            Assert.AreEqual("hello world", ((TextLineImpl)visualLine.TextLines[0]).LineRuns[0].StringRange.ToString());

            window.Close();
        }
    }
}
