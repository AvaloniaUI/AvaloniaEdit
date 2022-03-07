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
using System.Diagnostics;
using Avalonia.Media.TextFormatting;
using Avalonia.Utilities;
using AvaloniaEdit.Document;
using AvaloniaEdit.Text;
using AvaloniaEdit.Utils;
using JetBrains.Annotations;
using ITextSource = Avalonia.Media.TextFormatting.ITextSource;

namespace AvaloniaEdit.Rendering
{
    /// <summary>
    /// TextSource implementation that creates TextRuns for a VisualLine.
    /// </summary>
    internal sealed class VisualLineTextSource : ITextSource, ITextRunConstructionContext
    {
        public VisualLineTextSource(VisualLine visualLine)
        {
            VisualLine = visualLine;
        }

        public VisualLine VisualLine { get; }
        public TextView TextView { get; set; }
        public TextDocument Document { get; set; }
        public CustomTextRunProperties GlobalTextRunProperties { get; set; }

        [CanBeNull]
        public TextRun GetTextRun(int characterIndex)
        {
            try
            {
                foreach (var element in VisualLine.Elements)
                {
                    if (characterIndex >= element.VisualColumn
                        && characterIndex < element.VisualColumn + element.VisualLength)
                    {
                        var relativeOffset = characterIndex - element.VisualColumn;
                        var run = element.CreateTextRun(characterIndex, this);
                        if (run == null)
                            throw new ArgumentNullException(element.GetType().Name + ".CreateTextRun");
                        if (run.TextSourceLength == 0)
                            throw new ArgumentException("The returned TextRun must not have length 0.", element.GetType().Name + ".Length");
                        if (relativeOffset + run.TextSourceLength > element.VisualLength)
                            throw new ArgumentException("The returned TextRun is too long.", element.GetType().Name + ".CreateTextRun");
                        if (run is InlineObjectRun inlineRun)
                        {
                            inlineRun.VisualLine = VisualLine;
                            VisualLine.HasInlineObjects = true;
                            TextView.AddInlineObject(inlineRun);
                        }
                        return run;
                    }
                }
                if (TextView.Options.ShowEndOfLine && characterIndex == VisualLine.VisualLength)
                {
                    return CreateTextRunForNewLine();
                }
                return new TextEndOfParagraph(1);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        private TextRun CreateTextRunForNewLine()
        {
            var newlineText = "";
            var lastDocumentLine = VisualLine.LastDocumentLine;
            if (lastDocumentLine.DelimiterLength == 2)
            {
                newlineText = "¶";
            }
            else if (lastDocumentLine.DelimiterLength == 1)
            {
                var newlineChar = Document.GetCharAt(lastDocumentLine.Offset + lastDocumentLine.Length);
                switch (newlineChar)
                {
                    case '\r':
                        newlineText = "\\r";
                        break;
                    case '\n':
                        newlineText = "\\n";
                        break;
                    default:
                        newlineText = "?";
                        break;
                }
            }
            return new FormattedTextRun(new FormattedTextElement(TextView.CachedElements.GetTextForNonPrintableCharacter(newlineText, this), 0), GlobalTextRunProperties);
        }

        private ReadOnlySlice<char> _cachedString;
        private int _cachedStringOffset;

        public ReadOnlySlice<char> GetText(int offset, int length)
        {
            if (!_cachedString.IsEmpty)
            {
                if (offset >= _cachedStringOffset && offset + length <= _cachedStringOffset + _cachedString.Length)
                {
                    return new ReadOnlySlice<char>(_cachedString.Buffer, offset, length, offset - _cachedStringOffset);
                }
            }
            
            _cachedStringOffset = offset;

            _cachedString = new ReadOnlySlice<char>(Document.GetText(offset, length).AsMemory(), offset, length);

            return _cachedString;
        }
    }
}
