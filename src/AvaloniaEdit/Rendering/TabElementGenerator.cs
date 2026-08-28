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
using Avalonia.Media.TextFormatting;
using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;
using LogicalDirection = AvaloniaEdit.Document.LogicalDirection;

namespace AvaloniaEdit.Rendering
{
	// This class is internal because it does not need to be accessed by the user - it can be configured using TextEditorOptions.

	/// <summary>
	/// Element generator for tab characters. A tab spans the columns up to the next multiple of
	/// <see cref="TextEditorOptions.IndentationSize"/>, so that text after it lines up.
	/// </summary>
	/// <remarks>
	/// A column is <see cref="TextView.WideSpaceWidth"/> wide, so the alignment holds for the
	/// monospaced typefaces a code editor is used with, but not for proportional ones. Columns are
	/// counted from the start of the visual line, so on a wrapped row they continue the line they
	/// belong to rather than restarting, and <see cref="TextEditorOptions.WordWrapIndentation"/>
	/// shifts that whole row.
	/// </remarks>
	internal sealed class TabElementGenerator : VisualLineElementGenerator
	{
		/// <summary>
		/// Gets/Sets whether to show » for tabs.
		/// </summary>
		public bool ShowTabs { get; set; }

		private ReadOnlyMemory<char> _padding;

		private TextLine _glyph;
		private string _glyphText;
		private TextRunProperties _glyphProperties;

		public void FetchOptions(TextEditorOptions options)
		{
			ShowTabs = options.ShowTabs;
		}

		public override bool RunsOnLongLines => true;

		public override int GetFirstInterestedOffset(int startOffset)
		{
			var endLine = CurrentContext.VisualLine.LastDocumentLine;
			var relevantText = CurrentContext.GetText(startOffset, endLine.EndOffset - startOffset);
			var tab = relevantText.Text.AsSpan(relevantText.Offset, relevantText.Count).IndexOf('\t');

			return tab < 0 ? -1 : startOffset + tab;
		}

		public override VisualLineElement ConstructElement(int offset)
		{
			if (CurrentContext.Document.GetCharAt(offset) != '\t')
				return null;

			var columnsUntilNextTabStop = TabStop.ColumnsUntilNext(
				CurrentContext.VisualLine.CurrentDisplayColumn,
				CurrentContext.TextView.Options.IndentationSize);

			return new TabTextElement(ShowTabs ? GetGlyph() : null, GetPadding(columnsUntilNextTabStop));
		}

		/// <summary>
		/// Formats the glyph, and keeps it for as long as its inputs stay the same. The global text
		/// run properties are a new instance for every redraw, which is what expires it.
		/// </summary>
		private TextLine GetGlyph()
		{
			var glyphText = CurrentContext.TextView.Options.ShowTabsGlyph;
			var globalProperties = CurrentContext.GlobalTextRunProperties;

			if (_glyph == null || _glyphText != glyphText || _glyphProperties != globalProperties)
			{
				var properties = new VisualLineElementTextRunProperties(globalProperties);
				properties.SetForegroundBrush(CurrentContext.TextView.NonPrintableCharacterBrush);
				var textSource = new SimpleTextSource(glyphText, properties);

				_glyph = TextFormatter.Current.FormatLine(textSource, 0, double.MaxValue, new GenericTextParagraphProperties(properties));
				_glyphText = glyphText;
				_glyphProperties = globalProperties;
			}

			return _glyph;
		}

		private ReadOnlyMemory<char> GetPadding(int columnCount)
		{
			if (_padding.Length < columnCount)
				_padding = new string(' ', columnCount).AsMemory();
			return _padding.Slice(0, columnCount);
		}

		private sealed class TabTextElement : VisualLineElement
		{
			private readonly TextLine _glyph;
			private readonly ReadOnlyMemory<char> _padding;

			public TabTextElement(TextLine glyph, ReadOnlyMemory<char> padding) : base(padding.Length, 1)
			{
				_glyph = glyph;
				_padding = padding;
			}

			public override TextRun CreateTextRun(int startVisualColumn, ITextRunConstructionContext context)
			{
				var column = startVisualColumn - VisualColumn;
				if (column < 0 || column >= VisualLength)
					throw new ArgumentOutOfRangeException(nameof(startVisualColumn));

				// one column wide rather than as wide as the glyph, so that a wide ShowTabsGlyph
				// cannot stretch a tab (#206, #207)
				if (column == 0 && _glyph != null)
					return new TabGlyphRun(_glyph, TextRunProperties, context.TextView.WideSpaceWidth);

				return new TextCharacters(_padding.Slice(column), TextRunProperties);
			}

			public override int GetNextCaretPosition(int visualColumn, LogicalDirection direction, CaretPositioningMode mode)
			{
				if (mode == CaretPositioningMode.Normal || mode == CaretPositioningMode.EveryCodepoint)
					return base.GetNextCaretPosition(visualColumn, direction, mode);
				else
					return -1;
			}

			public override bool IsWhitespace(int visualColumn)
			{
				return true;
			}
		}

		private sealed class TabGlyphRun : DrawableTextRun
		{
			private readonly TextLine _glyph;

			public TabGlyphRun(TextLine glyph, TextRunProperties properties, double width)
			{
				if (properties == null)
					throw new ArgumentNullException(nameof(properties));
				Properties = properties;
				_glyph = glyph;
				Size = new Size(width, 0);
			}

			public override TextRunProperties Properties { get; }

			public override double Baseline => _glyph.Baseline;

			public override Size Size { get; }

			public override void Draw(DrawingContext drawingContext, Point origin)
			{
				_glyph.Draw(drawingContext, origin);
			}
		}
	}
}
