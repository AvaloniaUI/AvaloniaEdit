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
	/// Element generator for tab characters.
	/// </summary>
	internal sealed class TabElementGenerator : VisualLineElementGenerator, IBuiltinElementGenerator
	{
		/// <summary>
		/// Gets/Sets whether to show » for tabs.
		/// </summary>
		public bool ShowTabs { get; set; }

		private ReadOnlyMemory<char> _padding;

		private TextLine _glyph;
		private string _glyphText;
		private TextRunProperties _glyphProperties;

		public TabElementGenerator()
		{
			ShowTabs = true;
		}

		void IBuiltinElementGenerator.FetchOptions(TextEditorOptions options)
		{
			ShowTabs = options.ShowTabs;
		}

		public override int GetFirstInterestedOffset(int startOffset)
		{
			if (!ShowTabs)
				return -1;

			var endLine = CurrentContext.VisualLine.LastDocumentLine;
			var relevantText = CurrentContext.GetText(startOffset, endLine.EndOffset - startOffset);

			for (var i = 0; i < relevantText.Count; i++) {
				if (relevantText.Text[relevantText.Offset + i] == '\t')
					return startOffset + i;
			}
			return -1;
		}

		public override VisualLineElement ConstructElement(int offset)
		{
			if (!ShowTabs || CurrentContext.Document.GetCharAt(offset) != '\t')
				return null;

			var columnsUntilNextTabStop = TabStop.ColumnsUntilNext(
				CurrentContext.VisualLine.CurrentDisplayColumn,
				CurrentContext.TextView.Options.IndentationSize);

			return new TabTextElement(GetGlyph(), GetPadding(columnsUntilNextTabStop));
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
				// the glyph is drawn in the first column, the remaining columns are padded up to the tab stop
				var column = startVisualColumn - VisualColumn;
				if (column < 0 || column >= VisualLength)
					throw new ArgumentOutOfRangeException(nameof(startVisualColumn));

				if (column == 0)
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
