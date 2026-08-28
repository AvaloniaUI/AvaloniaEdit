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

			var properties = new VisualLineElementTextRunProperties(CurrentContext.GlobalTextRunProperties);
			properties.SetForegroundBrush(CurrentContext.TextView.NonPrintableCharacterBrush);
			var textSource = new SimpleTextSource(CurrentContext.TextView.Options.ShowTabsGlyph, properties);
			var textLine = TextFormatter.Current.FormatLine(textSource, 0, double.MaxValue, new GenericTextParagraphProperties(properties));
			return new TabTextElement(textLine);
		}

		private sealed class TabTextElement : VisualLineElement
		{
			internal readonly TextLine Text;

			public TabTextElement(TextLine text) : base(2, 1)
			{
				Text = text;
			}

			public override TextRun CreateTextRun(int startVisualColumn, ITextRunConstructionContext context)
			{
				// the TabTextElement consists of two TextRuns:
				// first a TabGlyphRun, then TextCharacters '\t' to let WPF handle the tab indentation
				if (startVisualColumn == VisualColumn)
					return new TabGlyphRun(this, TextRunProperties);
				else if (startVisualColumn == VisualColumn + 1)
					return new TextCharacters("\t".AsMemory(), TextRunProperties);
				else
					throw new ArgumentOutOfRangeException(nameof(startVisualColumn));
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
			private readonly TabTextElement _element;

			public TabGlyphRun(TabTextElement element, TextRunProperties properties)
			{
				if (properties == null)
					throw new ArgumentNullException(nameof(properties));
				Properties = properties;
				_element = element;
			}

			public override TextRunProperties Properties { get; }

			public override double Baseline => _element.Text.Baseline;

			public override Size Size => default;

			public override void Draw(DrawingContext drawingContext, Point origin)
			{
				_element.Text.Draw(drawingContext, origin);
			}
		}
	}
}
