using AvaloniaEdit.AvaloniaMocks;
using NUnit.Framework;
using System;
using TextMateSharp.Grammars;
using TextMateSharp.Registry;
using TextMateSharp.Themes;


namespace AvaloniaEdit.Tests.TextMate
{
    [TestFixture]
    internal class TextMateInstallationTests
    {
        private const string MockScopeName = "source.mock";
        private const ThemeName MockThemeName = ThemeName.Abbys;

        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            UnitTestApplication.BuildAvaloniaApp().SetupWithoutStarting();
        }

        #region Constructor tests
        [Test]
        public void Constructor_ShouldThrow_When_TextEditor_IsNull()
        {
            // arrange
            RegistryOptions registryOptions = new RegistryOptions(MockThemeName);

            // act & assert
            Assert.Throws<ArgumentNullException>(() =>
                new AvaloniaEdit.TextMate.TextMate.Installation(null, registryOptions));
        }

        [Test]
        public void Constructor_ShouldThrow_When_RegistryOptions_IsNull()
        {
            // arrange
            TextEditor textEditor = new TextEditor();

            // act & assert
            Assert.Throws<ArgumentNullException>(() =>
                new AvaloniaEdit.TextMate.TextMate.Installation(textEditor, null));
        }
        #endregion Constructor tests

        #region SetGrammar tests
        [Test]
        public void SetGrammar_ShouldThrow_When_Disposed()
        {
            // arrange
            TextEditor textEditor = new TextEditor();
            RegistryOptions registryOptions = new RegistryOptions(MockThemeName);
            AvaloniaEdit.TextMate.TextMate.Installation installation = AvaloniaEdit.TextMate.TextMate.InstallTextMate(
                textEditor, registryOptions);
            installation.Dispose();

            // act & assert
            Assert.Throws<ObjectDisposedException>(() => installation.SetGrammar(MockScopeName));
        }
        #endregion SetGrammar tests

        #region SetGrammarFile tests
        [Test]
        public void SetGrammarFile_ShouldThrow_When_Disposed()
        {
            // arrange
            TextEditor textEditor = new TextEditor();
            RegistryOptions registryOptions = new RegistryOptions(MockThemeName);
            AvaloniaEdit.TextMate.TextMate.Installation installation = AvaloniaEdit.TextMate.TextMate.InstallTextMate(
                textEditor, registryOptions);
            installation.Dispose();

            // act & assert
            Assert.Throws<ObjectDisposedException>(() => installation.SetGrammarFile("mock.tmLanguage"));
        }
        #endregion SetGrammarFile tests

        #region SetTheme tests
        [Test]
        public void SetTheme_ShouldThrow_When_Disposed()
        {
            // arrange
            TextEditor textEditor = new TextEditor();
            RegistryOptions registryOptions = new RegistryOptions(MockThemeName);
            AvaloniaEdit.TextMate.TextMate.Installation installation = AvaloniaEdit.TextMate.TextMate.InstallTextMate(
                textEditor, registryOptions);
            IRawTheme theme = registryOptions.GetDefaultTheme();
            installation.Dispose();

            // act & assert
            Assert.Throws<ObjectDisposedException>(() => installation.SetTheme(theme));
        }
        #endregion SetTheme tests

        #region TryGetThemeColor tests
        [Test]
        public void TryGetThemeColor_ShouldThrow_When_Disposed()
        {
            // arrange
            TextEditor textEditor = new TextEditor();
            RegistryOptions registryOptions = new RegistryOptions(MockThemeName);
            AvaloniaEdit.TextMate.TextMate.Installation installation = AvaloniaEdit.TextMate.TextMate.InstallTextMate(
                textEditor, registryOptions);
            installation.Dispose();

            // act & assert
            Assert.Throws<ObjectDisposedException>(() =>
                installation.TryGetThemeColor("foreground", out string _));
        }
        #endregion TryGetThemeColor tests

        #region Dispose tests
        [Test]
        public void Dispose_Should_Be_Idempotent()
        {
            // arrange
            TextEditor textEditor = new TextEditor();
            RegistryOptions registryOptions = new RegistryOptions(MockThemeName);
            AvaloniaEdit.TextMate.TextMate.Installation installation = AvaloniaEdit.TextMate.TextMate.InstallTextMate(
                textEditor, registryOptions);

            // act & assert
            Assert.DoesNotThrow(() =>
            {
                installation.Dispose();
                installation.Dispose();
            });
        }
        #endregion Dispose tests

        [Test]
        public void RegistryOptions_ShouldReturn_The_RegistryOptions_Passed_In_Constructor()
        {
            // arrange
            TextEditor textEditor = new TextEditor();
            RegistryOptions registryOptions = new RegistryOptions(MockThemeName);
            AvaloniaEdit.TextMate.TextMate.Installation installation = AvaloniaEdit.TextMate.TextMate.InstallTextMate(
                textEditor, registryOptions);

            // act
            IRegistryOptions result = installation.RegistryOptions;

            // assert
            Assert.AreSame(registryOptions, result);
        }
    }
}
