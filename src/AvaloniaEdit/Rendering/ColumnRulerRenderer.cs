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
using Avalonia.Media.Immutable;
using AvaloniaEdit.Utils;

namespace AvaloniaEdit.Rendering
{
	/// <summary>
	/// Renders a ruler at a certain column.
	/// </summary>
	internal sealed class ColumnRulerRenderer : IBackgroundRenderer
	{
	    private Pen _pen;
	    private int _column;
	    private readonly TextView _textView;
		
		public static readonly Color DefaultForeground = Colors.LightGray;
		
		public ColumnRulerRenderer(TextView textView)
		{
			_pen = new Pen(new ImmutableSolidColorBrush(DefaultForeground));
			_textView = textView ?? throw new ArgumentNullException(nameof(textView));
			_textView.BackgroundRenderers.Add(this);
		}
		
		public KnownLayer Layer => KnownLayer.Background;

	    public void SetRuler(int column, Pen pen)
		{
			if (_column != column) {
				_column = column;
				_textView.InvalidateLayer(Layer);
			}
			if (_pen != pen) {
				_pen = pen;
				_textView.InvalidateLayer(Layer);
			}
		}
		
		public void Draw(TextView textView, DrawingContext drawingContext)
		{
			if (_column < 1) return;
			double offset = textView.WideSpaceWidth * _column;
			Size pixelSize = PixelSnapHelpers.GetPixelSize(textView);
			double markerXPos = PixelSnapHelpers.PixelAlign(offset, pixelSize.Width);
			markerXPos -= textView.ScrollOffset.X;
			Point start = new Point(markerXPos, 0);
			Point end = new Point(markerXPos, Math.Max(textView.DocumentHeight, textView.Bounds.Height));
			
			drawingContext.DrawLine(_pen, start, end);
		}
	}
}
