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
using Avalonia;
using Avalonia.Media;
using AvaloniaEdit.Text;
using AvaloniaEdit.Utils;

namespace AvaloniaEdit.Rendering
{
    /// <summary>
    /// Formatted text (not normal document text).
    /// This is used as base class for various VisualLineElements that are displayed using a
    /// FormattedText, for example newline markers or collapsed folding sections.
    /// </summary>
    public class FormattedTextElement : VisualLineElement
    {
        internal FormattedText FormattedText { get; }
        internal string Text { get; set; }
        internal TextLine TextLine { get; set; }

        /// <summary>
        /// Creates a new FormattedTextElement that displays the specified text
        /// and occupies the specified length in the document.
        /// </summary>
        public FormattedTextElement(string text, int documentLength) : base(1, documentLength)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        /// <summary>
        /// Creates a new FormattedTextElement that displays the specified text
        /// and occupies the specified length in the document.
        /// </summary>
        internal FormattedTextElement(TextLine text, int documentLength) : base(1, documentLength)
        {
            TextLine = text ?? throw new ArgumentNullException(nameof(text));
        }

        /// <summary>
        /// Creates a new FormattedTextElement that displays the specified text
        /// and occupies the specified length in the document.
        /// </summary>
        public FormattedTextElement(FormattedText text, int documentLength) : base(1, documentLength)
        {
            FormattedText = text ?? throw new ArgumentNullException(nameof(text));
        }

        /// <inheritdoc/>
        public override TextRun CreateTextRun(int startVisualColumn, ITextRunConstructionContext context)
        {
            if (TextLine == null)
            {
                var formatter = TextFormatterFactory.Create();
                TextLine = PrepareText(formatter, Text, TextRunProperties);
                Text = null;
            }
            return new FormattedTextRun(this, TextRunProperties);
        }

        /// <summary>
        /// Constructs a TextLine from a simple text.
        /// </summary>
        internal static TextLine PrepareText(TextFormatter formatter, string text, TextRunProperties properties)
        {
            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));
            return formatter.FormatLine(
                new SimpleTextSource(text, properties),
                0,
                32000,
                new TextParagraphProperties
                {
                    DefaultTextRunProperties = properties,
                    TextWrapping = TextWrapping.NoWrap,
                    DefaultIncrementalTab = 40
                });
        }
    }

    /// <summary>
    /// This is the TextRun implementation used by the <see cref="FormattedTextElement"/> class.
    /// </summary>
    public class FormattedTextRun : TextEmbeddedObject
    {
        /// <summary>
        /// Creates a new FormattedTextRun.
        /// </summary>
        public FormattedTextRun(FormattedTextElement element, TextRunProperties properties)
        {
            Properties = properties ?? throw new ArgumentNullException(nameof(properties));
            Element = element ?? throw new ArgumentNullException(nameof(element));
        }

        /// <summary>
        /// Gets the element for which the FormattedTextRun was created.
        /// </summary>
        public FormattedTextElement Element { get; }

        /// <inheritdoc/>
        public override StringRange StringRange => default(StringRange);

        /// <inheritdoc/>
        public override int Length => Element.VisualLength;

        /// <inheritdoc/>
        public override bool HasFixedSize => true;

        /// <inheritdoc/>
        public override TextRunProperties Properties { get; }

        public override Size GetSize(double remainingParagraphWidth)
        {
            var formattedText = Element.FormattedText;
            if (formattedText != null)
            {
                return formattedText.Bounds.Size;
            }
            var text = Element.TextLine;
            return new Size(text.WidthIncludingTrailingWhitespace,
                text.Height);
        }

        /// <inheritdoc/>
        public override Rect ComputeBoundingBox()
        {
            return new Rect(GetSize(double.PositiveInfinity));
        }

        /// <inheritdoc/>
        public override void Draw(DrawingContext drawingContext, Point origin)
        {
            if (Element.FormattedText != null)
            {
                //origin = origin.WithY(origin.Y - Element.formattedText.Baseline);
                drawingContext.DrawText(null, origin, Element.FormattedText);
            }
            else
            {
                //origin.Y -= element.textLine.Baseline;
                Element.TextLine.Draw(drawingContext, origin);
            }
        }
    }
}
