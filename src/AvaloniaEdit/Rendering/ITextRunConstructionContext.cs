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

using System.Globalization;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;

#nullable enable

namespace AvaloniaEdit.Rendering
{
	/// <summary>
	/// Contains information relevant for text run creation.
	/// </summary>
	public interface ITextRunConstructionContext
	{
		/// <summary>
		/// Gets the text document.
		/// </summary>
		TextDocument Document { get; }
		
		/// <summary>
		/// Gets the text view for which the construction runs.
		/// </summary>
		TextView TextView { get; }
		
		/// <summary>
		/// Gets the visual line that is currently being constructed.
		/// </summary>
		VisualLine VisualLine { get; }
		
		/// <summary>
		/// Gets the global text run properties.
		/// </summary>
		CustomTextRunProperties GlobalTextRunProperties { get; }
		
		/// <summary>
		/// Gets a piece of text from the document.
		/// </summary>
		/// <remarks>
		/// This method is allowed to return a larger string than requested.
		/// It does this by returning a <see cref="StringSegment"/> that describes the requested segment within the returned string.
		/// This method should be the preferred text access method in the text transformation pipeline, as it can avoid repeatedly allocating string instances
		/// for text within the same line.
		/// </remarks>
		string GetText(int offset, int length);
	}

	public sealed class CustomTextRunProperties : TextRunProperties
	{
		public const double DefaultFontRenderingEmSize = 12;
		
		private Typeface _typeface;
		private double _fontRenderingEmSize;
		private TextDecorationCollection? _textDecorations;
		private IBrush? _foregroundBrush;
		private IBrush? _backgroundBrush;
		private CultureInfo? _cultureInfo;
		private BaselineAlignment _baselineAlignment;

		internal CustomTextRunProperties(Typeface typeface, 
			double fontRenderingEmSize = 12,
			TextDecorationCollection? textDecorations = null, 
			IBrush? foregroundBrush = null,
			IBrush? backgroundBrush = null,
			CultureInfo? cultureInfo = null, 
			BaselineAlignment baselineAlignment = BaselineAlignment.Baseline)
		{
			_typeface = typeface;
			_fontRenderingEmSize = fontRenderingEmSize;
			_textDecorations = textDecorations;
			_foregroundBrush = foregroundBrush;
			_backgroundBrush = backgroundBrush;
			_cultureInfo = cultureInfo;
			_baselineAlignment = baselineAlignment;
		}

		public override Typeface Typeface => _typeface;

		public override double FontRenderingEmSize => _fontRenderingEmSize;

		public override TextDecorationCollection? TextDecorations => _textDecorations;

		public override IBrush? ForegroundBrush => _foregroundBrush;

		public override IBrush? BackgroundBrush => _backgroundBrush;

		public override CultureInfo? CultureInfo => _cultureInfo;

		public override BaselineAlignment BaselineAlignment => _baselineAlignment;

		public CustomTextRunProperties Clone()
		{
			return new CustomTextRunProperties(Typeface, FontRenderingEmSize, TextDecorations, ForegroundBrush,
				BackgroundBrush, CultureInfo, BaselineAlignment);
		}

		public void SetForegroundBrush(IBrush foregroundBrush)
		{
			_foregroundBrush = foregroundBrush;
		}

		public void SetBackgroundBrush(IBrush backgroundBrush)
		{
			_backgroundBrush = backgroundBrush;
		}

		public void SetTypeface(Typeface typeface)
		{
			_typeface = typeface;
		}

		public void SetFontSize(int colorFontSize)
		{
			_fontRenderingEmSize = colorFontSize;
		}

		public void SetTextDecorations(TextDecorationCollection textDecorations)
		{
			_textDecorations = textDecorations;
		}
	}

	public sealed class CustomTextParagraphProperties : TextParagraphProperties
	{
		public const double DefaultIncrementalTabWidth = 4 * CustomTextRunProperties.DefaultFontRenderingEmSize;
		
		private TextWrapping _textWrapping;
		private double _lineHeight;
		private double _indent;
		private double _defaultIncrementalTab;
		private readonly bool _firstLineInParagraph;

		public CustomTextParagraphProperties(TextRunProperties defaultTextRunProperties,
			bool firstLineInParagraph = true,
			TextWrapping textWrapping = TextWrapping.NoWrap,
			double lineHeight = 0, 
			double indent = 0, 
			double defaultIncrementalTab = DefaultIncrementalTabWidth)
		{
			DefaultTextRunProperties = defaultTextRunProperties;
			_firstLineInParagraph = firstLineInParagraph;
			_textWrapping = textWrapping;
			_lineHeight = lineHeight;
			_indent = indent;
			_defaultIncrementalTab = defaultIncrementalTab;
		}

		public override FlowDirection FlowDirection => FlowDirection.LeftToRight;
		public override TextAlignment TextAlignment => TextAlignment.Left;
		public override double LineHeight => _lineHeight;
		public override bool FirstLineInParagraph => _firstLineInParagraph;
		public override TextRunProperties DefaultTextRunProperties { get; }
		public override TextWrapping TextWrapping => _textWrapping;
		public override double Indent => _indent;
	}
}
