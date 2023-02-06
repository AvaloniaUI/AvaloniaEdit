using Avalonia.Media;
using Avalonia.Platform;

using Moq;

using System.Collections.Generic;
using System.Globalization;

#nullable enable

namespace AvaloniaEdit.AvaloniaMocks
{
    public class MockFontManagerImpl : IFontManagerImpl
    {
        private readonly string _defaultFamilyName;

        public MockFontManagerImpl(string defaultFamilyName = "Default")
        {
            _defaultFamilyName = defaultFamilyName;
        }

        public string GetDefaultFontFamilyName()
        {
            return _defaultFamilyName;
        }

        public IEnumerable<string> GetInstalledFontFamilyNames(bool checkForUpdates = false)
        {
            return new[] { _defaultFamilyName };
        }

        public bool TryMatchCharacter(int codepoint, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch,
            FontFamily? fontFamily, CultureInfo? culture, out Typeface typeface)
        {
            typeface = new Typeface(_defaultFamilyName);

            return true;
        }

        public IGlyphTypeface CreateGlyphTypeface(Typeface typeface)
        {
            return new MockGlyphTypeface();
        }
    }
}
