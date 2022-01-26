using Avalonia;
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

            TextLineRun run = TextLineRun.Create(s, 0, 0, 4, CreateDefaultParagraphProperties());

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

            var paragraphProperties = CreateDefaultParagraphProperties();

            TextLineRun run = TextLineRun.Create(s, 0, 0, 1, paragraphProperties);

            Assert.AreEqual(paragraphProperties.DefaultIncrementalTab, run.GetDistanceFromCharacter(1));
        }

        [Test]
        public void Spaces_Plus_Tab_Line_Run_Should_Have_Correct_Glyph_Widths()
        {
            using var app = UnitTestApplication.Start(new TestServices().With(
                renderInterface: new MockPlatformRenderInterface(),
                fontManagerImpl: new MockFontManagerImpl(),
                formattedTextImpl: Mock.Of<IFormattedTextImpl>()));

            SimpleTextSource s = new SimpleTextSource(
                " \t ",
                CreateDefaultTextProperties());

            var paragraphProperties = CreateDefaultParagraphProperties();

            TextLineRun run = TextLineRun.Create(s, 0, 0, 1, paragraphProperties);

            double[] expectedLengths = new double[]
            {
                0,
                MockGlyphTypeface.GlyphAdvance * 1,
                MockGlyphTypeface.GlyphAdvance * 1 + paragraphProperties.DefaultIncrementalTab,
                MockGlyphTypeface.GlyphAdvance * 2 + paragraphProperties.DefaultIncrementalTab
            };

            for (int i = 0; i < 4; i++)
                Assert.AreEqual(expectedLengths[i], run.GetDistanceFromCharacter(i));
        }

        [Test]
        public void Chars_Plus_Tab_Line_Run_Should_Have_Correct_Glyph_Widths()
        {
            using var app = UnitTestApplication.Start(new TestServices().With(
                renderInterface: new MockPlatformRenderInterface(),
                fontManagerImpl: new MockFontManagerImpl(),
                formattedTextImpl: Mock.Of<IFormattedTextImpl>()));

            SimpleTextSource s = new SimpleTextSource(
                "a\ta",
                CreateDefaultTextProperties());

            var paragraphProperties = CreateDefaultParagraphProperties();

            TextLineRun run = TextLineRun.Create(s, 0, 0, 1, paragraphProperties);

            double[] expectedLengths = new double[]
            {
                0,
                MockGlyphTypeface.GlyphAdvance * 1,
                MockGlyphTypeface.GlyphAdvance * 1 + paragraphProperties.DefaultIncrementalTab,
                MockGlyphTypeface.GlyphAdvance * 2 + paragraphProperties.DefaultIncrementalTab
            };

            for (int i = 0; i < 4; i++)
                Assert.AreEqual(expectedLengths[i], run.GetDistanceFromCharacter(i));
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

            TextLineRun run = TextLineRun.Create(ts.Object, 0, 0, 1, CreateDefaultParagraphProperties());

            Assert.AreEqual(
                runWidth + SpecialCharacterTextRun.BoxMargin,
                run.GetDistanceFromCharacter(1));
        }

        [Test]
        public void TextEmbeddedObject_Should_Have_Valid_Baseline()
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
            ts.Setup(s => s.GetTextRun(It.IsAny<int>())).Returns(f);

            TextLineRun run = TextLineRun.Create(ts.Object, 0, 0, 1, CreateDefaultParagraphProperties());

            Assert.AreEqual(MockGlyphTypeface.GlyphAscent, run.Baseline);
        }

        [Test]
        public void Simple_Run_Should_Have_Valid_Baseline()
        {
            using var app = UnitTestApplication.Start(new TestServices().With(
                renderInterface: new MockPlatformRenderInterface(),
                fontManagerImpl: new MockFontManagerImpl(),
                formattedTextImpl: Mock.Of<IFormattedTextImpl>()));

            SimpleTextSource s = new SimpleTextSource(
                "a\ta",
                CreateDefaultTextProperties());

            var paragraphProperties = CreateDefaultParagraphProperties();

            TextLineRun run = TextLineRun.Create(s, 0, 0, 1, paragraphProperties);

            Assert.AreEqual(MockGlyphTypeface.GlyphAscent, run.Baseline);
        }

        [Test]
        public void Tab_Glyph_Run_Shuld_Have_Valid_Bounds()
        {
            using var app = UnitTestApplication.Start(new TestServices().With(
                renderInterface: new MockPlatformRenderInterface(),
                fontManagerImpl: new MockFontManagerImpl(),
                formattedTextImpl: Mock.Of<IFormattedTextImpl>()));

            double runWidth = 50;
            double runHeight = 20;

            Mock<TextLine> textLineMock = new Mock<TextLine>();
            textLineMock.Setup(
                t => t.WidthIncludingTrailingWhitespace).Returns(runWidth);
            textLineMock.Setup(
                t => t.Height).Returns(runHeight);

            TabTextElement tabTextElement = new TabTextElement(textLineMock.Object);

            TabGlyphRun tabRun = new TabGlyphRun(
                tabTextElement,
                CreateDefaultTextProperties());

            Size runSize = tabRun.GetSize(double.PositiveInfinity);

            Assert.AreEqual(runWidth, runSize.Width, "Wrong run width");
            Assert.AreEqual(runHeight, runSize.Height, "Wrong run height");
        }

        TextRunProperties CreateDefaultTextProperties()
        {
            return new TextRunProperties()
            {
                Typeface = new Typeface("Default"),
                FontSize = MockGlyphTypeface.DefaultFontSize,
            };
        }

        TextParagraphProperties CreateDefaultParagraphProperties()
        {
            return new TextParagraphProperties()
            {
                DefaultTextRunProperties = CreateDefaultTextProperties(),
                DefaultIncrementalTab = 70,
                Indent = 4,
            };
        }
    }
}
