using Avalonia.Media;
using Avalonia.Platform;

using AvaloniaEdit.AvaloniaMocks;
using AvaloniaEdit.Rendering;

using Moq;

using NUnit.Framework;

namespace AvaloniaEdit.Text
{
    [TestFixture]
    internal class TextLineImplTests
    {
        [Test]
        public void Text_Line_Should_Generate_Text_Runs()
        {
            using var app = UnitTestApplication.Start(new TestServices().With(
                renderInterface: new MockPlatformRenderInterface(),
                fontManagerImpl: new MockFontManagerImpl(),
                formattedTextImpl: Mock.Of<IFormattedTextImpl>()));

            SimpleTextSource textSource = new SimpleTextSource("hello", CreateDefaultTextProperties());

            TextLineImpl textLine = TextLineImpl.Create(
                CreateDefaultParagraphProperties(), 0, 5, textSource);

            Assert.AreEqual(2, textLine.LineRuns.Length);
            Assert.AreEqual("hello", textLine.LineRuns[0].StringRange.ToString());
            Assert.IsTrue(textLine.LineRuns[1].IsEnd);
        }

        [Test]
        public void Tab_Block_Should_Split_Runs()
        {
            using var app = UnitTestApplication.Start(new TestServices().With(
                renderInterface: new MockPlatformRenderInterface(),
                fontManagerImpl: new MockFontManagerImpl(),
                formattedTextImpl: Mock.Of<IFormattedTextImpl>()));

            SimpleTextSource s = new SimpleTextSource(
                "\t\t",
                CreateDefaultTextProperties());

            TextLineImpl textLine = TextLineImpl.Create(
                CreateDefaultParagraphProperties(), 0, 2, s);

            var textRuns = textLine.GetTextRuns();

            Assert.AreEqual(3, textRuns.Count);
            Assert.IsTrue(textLine.LineRuns[0].IsTab);
            Assert.IsTrue(textLine.LineRuns[1].IsTab);
            Assert.IsTrue(textLine.LineRuns[2].IsEnd);
        }

        [Test]
        public void Tab_Block_With_Spaces_At_The_End_Should_Split_Runs()
        {
            using var app = UnitTestApplication.Start(new TestServices().With(
                renderInterface: new MockPlatformRenderInterface(),
                fontManagerImpl: new MockFontManagerImpl(),
                formattedTextImpl: Mock.Of<IFormattedTextImpl>()));

            SimpleTextSource s = new SimpleTextSource(
                "\t\t    ",
                CreateDefaultTextProperties());

            TextLineImpl textLine = TextLineImpl.Create(
                CreateDefaultParagraphProperties(), 0, 2, s);

            var textRuns = textLine.GetTextRuns();

            Assert.AreEqual(4, textRuns.Count);
            Assert.IsTrue(textLine.LineRuns[0].IsTab);
            Assert.IsTrue(textLine.LineRuns[1].IsTab);
            Assert.AreEqual("    ", textLine.LineRuns[2].StringRange.ToString());
            Assert.IsTrue(textLine.LineRuns[3].IsEnd);
        }

        [Test]
        public void Space_Block_Without_Tab_Should_Not_Split_Runs()
        {
            using var app = UnitTestApplication.Start(new TestServices().With(
                renderInterface: new MockPlatformRenderInterface(),
                fontManagerImpl: new MockFontManagerImpl(),
                formattedTextImpl: Mock.Of<IFormattedTextImpl>()));

            SimpleTextSource s = new SimpleTextSource(
                "    hello",
                CreateDefaultTextProperties());

            TextLineImpl textLine = TextLineImpl.Create(
                CreateDefaultParagraphProperties(), 0, 9, s);

            Assert.AreEqual(2, textLine.LineRuns.Length);

            Assert.AreEqual("    hello", textLine.LineRuns[0].StringRange.ToString());
            Assert.IsTrue(textLine.LineRuns[1].IsEnd);
        }

        [Test]
        public void Text_Line_Should_Not_Wrap_In_Two_Runs_When_Option_Disabled()
        {
            using var app = UnitTestApplication.Start(new TestServices().With(
                renderInterface: new MockPlatformRenderInterface(),
                fontManagerImpl: new MockFontManagerImpl(),
                formattedTextImpl: Mock.Of<IFormattedTextImpl>()));

            SimpleTextSource s = new SimpleTextSource(
                "hello world",
                CreateDefaultTextProperties());

            TextParagraphProperties paraProps = CreateDefaultParagraphProperties();
            paraProps.TextWrapping = TextWrapping.NoWrap;

            TextLineImpl textLine = TextLineImpl.Create(
                paraProps, 0, MockGlyphTypeface.GlyphAdvance * 7, s);

            Assert.AreEqual("hello world".Length, textLine.LineRuns[0].Length);
        }

        [Test]
        public void Text_Line_Should_Wrap_In_Two_Runs_Option_Enabled()
        {
            using var app = UnitTestApplication.Start(new TestServices().With(
                renderInterface: new MockPlatformRenderInterface(),
                fontManagerImpl: new MockFontManagerImpl(),
                formattedTextImpl: Mock.Of<IFormattedTextImpl>()));

            SimpleTextSource s = new SimpleTextSource(
                "hello world",
                CreateDefaultTextProperties());

            TextParagraphProperties paraProps = CreateDefaultParagraphProperties();
            paraProps.TextWrapping = TextWrapping.Wrap;

            TextLineImpl textLine = TextLineImpl.Create(
                paraProps, 0, MockGlyphTypeface.GlyphAdvance * 7, s);

            Assert.AreEqual("hello ".Length, textLine.LineRuns[0].Length);
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
