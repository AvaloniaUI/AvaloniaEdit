using Avalonia.Media;
using Avalonia.Platform;

using AvaloniaEdit.AvaloniaMocks;
using AvaloniaEdit.Rendering;

using Moq;

using NUnit.Framework;

using static AvaloniaEdit.Rendering.SingleCharacterElementGenerator;

namespace AvaloniaEdit.Text
{
    [TestFixture]
    internal class TextLineRunTests
    {
        [Test]
        public void Text_Line_Run_Should_Have_Valid_Glyph_Widths()
        {
            using var app = UnitTestApplication.Start(new TestServices().With(
                renderInterface: new MockPlatformRenderInterface(),
                fontManagerImpl: new MockFontManagerImpl(),
                formattedTextImpl: Mock.Of<IFormattedTextImpl>()));

            SimpleTextSource s = new SimpleTextSource(
                "0123",
                CreateDefaultTextProperties());

            TextLineRun run = TextLineRun.Create(s, 0, 0, 4);

            Assert.AreEqual(MockGlyphTypeface.GlyphAdvance * 0, run.GetDistanceFromCharacter(0));
            Assert.AreEqual(MockGlyphTypeface.GlyphAdvance * 1, run.GetDistanceFromCharacter(1));
            Assert.AreEqual(MockGlyphTypeface.GlyphAdvance * 2, run.GetDistanceFromCharacter(2));
            Assert.AreEqual(MockGlyphTypeface.GlyphAdvance * 3, run.GetDistanceFromCharacter(3));
        }

        [Test]
        public void Tab_Line_Run_Should_Have_Fixed_Glyph_Width()
        {
            using var app = UnitTestApplication.Start(new TestServices().With(
                renderInterface: new MockPlatformRenderInterface(),
                fontManagerImpl: new MockFontManagerImpl(),
                formattedTextImpl: Mock.Of<IFormattedTextImpl>()));

            SimpleTextSource s = new SimpleTextSource(
                "\t",
                CreateDefaultTextProperties());

            TextLineRun run = TextLineRun.Create(s, 0, 0, 1);

            Assert.AreEqual(40, run.GetDistanceFromCharacter(1));
        }

        [Test]
        public void TextEmbeddedObject_Line_Run_Should_Have_Fixed_Glyph_Width()
        {
            using var app = UnitTestApplication.Start(new TestServices().With(
                renderInterface: new MockPlatformRenderInterface(),
                fontManagerImpl: new MockFontManagerImpl(),
                formattedTextImpl: Mock.Of<IFormattedTextImpl>()));

            int runWidth = 50;

            TextLine textLine = Mock.Of<TextLine>(
                t => t.WidthIncludingTrailingWhitespace == runWidth);

            SpecialCharacterTextRun f = new SpecialCharacterTextRun(
                new FormattedTextElement("BEL", 1) { TextLine = textLine },
                CreateDefaultTextProperties());

            Mock<TextSource> ts = new Mock<TextSource>();
            ts.Setup(s=> s.GetTextRun(It.IsAny<int>())).Returns(f);

            TextLineRun run = TextLineRun.Create(ts.Object, 0, 0, 1);

            Assert.AreEqual(
                runWidth + SpecialCharacterTextRun.BoxMargin,
                run.GetDistanceFromCharacter(1));
        }

        TextRunProperties CreateDefaultTextProperties()
        {
            return new TextRunProperties()
            {
                Typeface = new Typeface("Default"),
                FontSize = MockGlyphTypeface.DefaultFontSize,
            };
        }
    }
}
