using Avalonia.Media;
using Avalonia.Platform;

using Moq;

using System.Collections.Generic;
using System.Globalization;
using System.IO;

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

        public string[] GetInstalledFontFamilyNames(bool checkForUpdates = false)
        {
            return new[] { _defaultFamilyName };
        }

        public bool TryMatchCharacter(int codepoint, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch,
            FontFamily? fontFamily, CultureInfo? culture, out Typeface typeface)
        {
            typeface = new Typeface(_defaultFamilyName);

            return true;
        }

        public bool TryCreateGlyphTypeface(string familyName, FontStyle style, FontWeight weight,
                                           FontStretch stretch, out IGlyphTypeface glyphTypeface)
        {
            glyphTypeface = new MockGlyphTypeface();
            return true;
        }

        public bool TryCreateGlyphTypeface(Stream stream, out IGlyphTypeface glyphTypeface)
        {
            glyphTypeface = new MockGlyphTypeface();
            return true;
        }
    }
}
