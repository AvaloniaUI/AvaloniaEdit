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
using Avalonia.Data.Converters;

namespace AvaloniaEdit.Converters
{
    /// <summary>
    /// Converts a text length to a <see cref="bool"/> indicating whether the text is empty.
    /// </summary>
    /// <remarks>
    /// Bind consumers to <c>Document.TextLength</c> instead of <c>Document.Text</c>.
    /// <c>TextLength</c> is an <see cref="Int32"/> produced without any allocation, while reading
    /// <c>Text</c> materializes the entire document as a <see cref="String"/> on each change
    /// when the value is consumed.
    /// </remarks>
    public sealed class TextLengthToBoolConverter : IValueConverter
    {
        /// <summary>
        /// Default instance.
        /// </summary>
        public static readonly TextLengthToBoolConverter Instance = new();

        private TextLengthToBoolConverter()
        {
        }

        /// <summary>
        /// Converts a text length to a <see cref="bool"/>.
        /// </summary>
        /// <param name="value">The text length, or null when the document source is unavailable.</param>
        /// <param name="targetType">The type of the conversion target: <see cref="bool"/>.</param>
        /// <param name="parameter">No parameter is supported.</param>
        /// <param name="culture">Culture information is not used.</param>
        /// <returns>true when the text is empty (null or zero length), false otherwise.</returns>
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
            => value switch
            {
                null => true,
                Int32 textLength => textLength == 0,
                _ => false
            };

        /// <summary>
        /// Not supported. This converter can only convert from a text length to a
        /// <see cref="Boolean"/>; the reverse conversion is not defined.
        /// </summary>
        /// <param name="value">The value to convert back.</param>
        /// <param name="targetType">The type of the conversion target.</param>
        /// <param name="parameter">Conversion parameter.</param>
        /// <param name="culture">Culture information.</param>
        /// <returns>Nothing; this method always throws.</returns>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
