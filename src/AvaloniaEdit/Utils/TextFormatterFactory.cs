// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Globalization;
using AvaloniaEdit.Text;
using Avalonia.Controls;
using Avalonia.Media;

namespace AvaloniaEdit.Utils
{
    /// <summary>
    /// Creates TextFormatter instances that with the correct TextFormattingMode, if running on .NET 4.0.
    /// </summary>
    public static class TextFormatterFactory
    {
        public static TextFormatter Create()
        {
            return new TextFormatter();
        }

        public static Avalonia.Media.TextFormatting.TextLine CreateTextLine(
            string text,
            Typeface typeface,
            double fontSize,
            IBrush foreground)
        {
            return new Avalonia.Media.TextFormatting.TextLayout(text, typeface, fontSize, foreground).TextLines[0];
        }

        /// <summary>
        /// Creates text line.
        /// </summary>
        /// <param name="element">The owner element. The text formatter setting are read from this element.</param>
        /// <param name="text">The text.</param>
        /// <param name="typeface">The typeface to use. If this parameter is null, the typeface of the <paramref name="element"/> will be used.</param>
        /// <param name="emSize">The font size. If this parameter is null, the font size of the <paramref name="element"/> will be used.</param>
        /// <param name="foreground">The foreground color. If this parameter is null, the foreground of the <paramref name="element"/> will be used.</param>
        /// <returns>A FormattedText object using the specified settings.</returns>
        public static Avalonia.Media.TextFormatting.TextLine CreateTextLine(
            Control element, 
            string text, 
            FontFamily typeface, double? emSize, 
            IBrush foreground)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (typeface == null)
                typeface = TextBlock.GetFontFamily(element);
            if (emSize == null)
                emSize = TextBlock.GetFontSize(element);
            if (foreground == null)
                foreground = TextBlock.GetForeground(element);

            return CreateTextLine(text, new Typeface(typeface.Name), emSize.Value, foreground!);
        }
    }
}
