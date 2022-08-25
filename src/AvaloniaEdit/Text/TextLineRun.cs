using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Media.TextFormatting.Unicode;
using Avalonia.Utilities;

using AvaloniaEdit.Rendering;

namespace AvaloniaEdit.Text
{
    internal sealed class TextLineRun
    {
        private const string NewlineString = "\r\n";
        private const string TabString = "\t";

        private FormattedText _formattedText;
        private Size _formattedTextSize;
        private IReadOnlyList<double> _glyphWidths;
        public StringRange StringRange { get; private set; }

        public int Length { get; set; }

        public double Width { get; private set; }

        public TextRun TextRun { get; private set; }

        public bool IsEnd { get; private set; }

        public bool IsTab { get; private set; }

        public bool IsEmbedded { get; private set; }

        public double Baseline
        {
            get
            {
                if (IsEnd)
                {
                    return 0.0;
                }

                double defaultBaseLine = GetDefaultBaseline(TextRun.Properties.FontMetrics);

                if (IsEmbedded && TextRun is TextEmbeddedObject embeddedObject)
                {
                    var box = embeddedObject.ComputeBoundingBox();
                    return defaultBaseLine - box.Y;
                }

                return defaultBaseLine;
            }
        }

        public double Height
        {
            get
            {
                if (IsEnd)
                {
                    return 0.0;
                }
                if (IsEmbedded && TextRun is TextEmbeddedObject embeddedObject)
                {
                    var box = embeddedObject.ComputeBoundingBox();
                    return box.Height;
                }
                
                return GetDefaultLineHeight(TextRun.Properties.FontMetrics);
            }
        }

        public static double GetDefaultLineHeight(FontMetrics fontMetrics)
        {
            // adding an extra 15% of the line height look good across different font sizes
            double extraLineHeight = fontMetrics.LineHeight * 0.15;
            return fontMetrics.LineHeight + extraLineHeight;
        }

        public static double GetDefaultBaseline(FontMetrics fontMetrics)
        {
            return Math.Abs(fontMetrics.Ascent);
        }

        public Typeface Typeface => TextRun.Properties.Typeface;

        public double FontSize => TextRun.Properties.FontSize;

        private TextLineRun()
        {
        }

        public static TextLineRun Create(TextSource textSource, int index, int firstIndex, double lengthLeft, TextParagraphProperties paragraphProperties)
        {
            var textRun = textSource.GetTextRun(index);
            var stringRange = textRun.GetStringRange();
            return Create(textSource, stringRange, textRun, index, lengthLeft, paragraphProperties);
        }

        private static TextLineRun Create(TextSource textSource, StringRange stringRange, TextRun textRun, int index, double widthLeft, TextParagraphProperties paragraphProperties)
        {
            if (textRun is TextCharacters)
            {
                return CreateRunForSpecialChars(textSource, stringRange, textRun, index, paragraphProperties) ??
                       CreateRunForText(stringRange, textRun, widthLeft, paragraphProperties);
            }

            if (textRun is TextEndOfLine)
            {
                return new TextLineRun(textRun.Length, textRun) { IsEnd = true };
            }

            if (textRun is TextEmbeddedObject embeddedObject)
            {
                double width = embeddedObject.GetSize(double.PositiveInfinity).Width;
                return new TextLineRun(textRun.Length, textRun)
                {
                    IsEmbedded = true,
                    _glyphWidths = new double[] { width },
                    // Embedded objects must propagate their width to the container.
                    // Otherwise text runs after the embedded object are drawn at the same x position.
                    Width = width
                };
            }

            throw new NotSupportedException("Unsupported run type");
        }

        private static TextLineRun CreateRunForSpecialChars(TextSource textSource, StringRange stringRange, TextRun textRun, int index, TextParagraphProperties paragraphProperties)
        {
            switch (stringRange[0])
            {
                case '\r':
                    var runLength = 1;
                    if (stringRange.Length > 1 && stringRange[1] == '\n')
                    {
                        runLength = 2;
                    }
                    else if (stringRange.Length == 1)
                    {
                        var nextRun = textSource.GetTextRun(index + 1);
                        var range = nextRun.GetStringRange();
                        if (range.Length > 0 && range[0] == '\n')
                        {
                            var eolRun = new TextCharacters(NewlineString, textRun.Properties);
                            return new TextLineRun(eolRun.Length, eolRun) { IsEnd = true };
                        }
                    }

                    return new TextLineRun(runLength, textRun) { IsEnd = true };
                case '\n':
                    return new TextLineRun(1, textRun) { IsEnd = true };
                case '\t':
                    return CreateRunForTab(textRun, paragraphProperties);
                default:
                    return null;
            }
        }

        private static TextLineRun CreateRunForTab(TextRun textRun, TextParagraphProperties paragraphProperties)
        {
            var tabRun = new TextCharacters(TabString, textRun.Properties);
            var stringRange = tabRun.StringRange;
            var run = new TextLineRun(1, tabRun)
            {
                IsTab = true,
                StringRange = stringRange,
                Width = paragraphProperties.DefaultIncrementalTab
            };

            run._glyphWidths = new double[] { run.Width };

            return run;
        }

        internal static TextLineRun CreateRunForText(
            StringRange stringRange,
            TextRun textRun,
            double widthLeft,
            TextParagraphProperties paragraphProperties)
        {
            TextLineRun run = CreateTextLineRun(stringRange, textRun, textRun.Length, paragraphProperties);

            if (run.Width <= widthLeft)
                return run;

            TextLineRun wrapped = PerformTextWrapping(run, widthLeft, paragraphProperties);
            wrapped.Width = run.Width;
            return wrapped;
        }

        private static TextLineRun CreateTextLineRun(StringRange stringRange, TextRun textRun, int length, TextParagraphProperties paragraphProperties)
        {
            var run = new TextLineRun
            {
                StringRange = stringRange,
                TextRun = textRun,
                Length = length
            };

            var tf = run.Typeface;
            var formattedText = new FormattedText
            {
                Text = stringRange.ToString(run.Length),
                Typeface = new Typeface(tf.FontFamily, tf.Style, tf.Weight),
                FontSize = run.FontSize
            };

            run._formattedText = formattedText;

            var size = formattedText.Bounds.Size;

            run._formattedTextSize = size;

            run.Width = size.Width;

            run._glyphWidths = new GlyphWidths(
                run.StringRange,
                run.Typeface.GlyphTypeface,
                run.FontSize,
                paragraphProperties.DefaultIncrementalTab);

            return run;
        }

        private static TextLineRun PerformTextWrapping(TextLineRun run, double widthLeft, TextParagraphProperties paragraphProperties)
        {
            (int firstIndex, int trailingLength) characterHit = run.GetCharacterFromDistance(widthLeft);

            int lenForTextWrapping = FindPositionForTextWrapping(run.StringRange, characterHit.firstIndex);

            return CreateTextLineRun(
                run.StringRange.WithLength(lenForTextWrapping),
                run.TextRun,
                lenForTextWrapping,
                paragraphProperties);
        }

        private static int FindPositionForTextWrapping(StringRange range, int maxIndex)
        {
            if (maxIndex > range.Length - 1)
                maxIndex = range.Length - 1;

            LineBreakEnumerator lineBreakEnumerator = new LineBreakEnumerator(
                new ReadOnlySlice<char>(range.String.AsMemory().Slice(range.OffsetToFirstChar, range.Length)));

            LineBreak? lineBreak = null;

            while (lineBreakEnumerator.MoveNext())
            {
                if (lineBreakEnumerator.Current.PositionWrap > maxIndex)
                    break;

                lineBreak = lineBreakEnumerator.Current;
            }

            return lineBreak.HasValue ? lineBreak.Value.PositionWrap : maxIndex;
        }

        private TextLineRun(int length, TextRun textRun)
        {
            Length = length;
            TextRun = textRun;
        }

        public void Draw(DrawingContext drawingContext, double x, double y)
        {
            if (IsEmbedded)
            {
                var embeddedObject = (TextEmbeddedObject)TextRun;
                embeddedObject.Draw(drawingContext, new Point(x, y));
                return;
            }

            if (Length <= 0 || IsEnd)
            {
                return;
            }

            if (_formattedText != null && drawingContext != null)
            {
                if (TextRun.Properties.BackgroundBrush != null)
                {
                    var bounds = new Rect(x, y, _formattedTextSize.Width, _formattedTextSize.Height);
                    drawingContext.FillRectangle(TextRun.Properties.BackgroundBrush, bounds);
                }

                drawingContext.DrawText(TextRun.Properties.ForegroundBrush,
                    new Point(x, y), _formattedText);

                var glyphTypeface = TextRun.Properties.Typeface.GlyphTypeface;
                
                var scale =  TextRun.Properties.FontSize / glyphTypeface.DesignEmHeight;

                var baseline = y + -glyphTypeface.Ascent * scale;
                
                if (TextRun.Properties.Underline)
                {
                    var pen = new Pen(TextRun.Properties.ForegroundBrush, glyphTypeface.UnderlineThickness * scale);

                    var posY = baseline + glyphTypeface.UnderlinePosition * scale;

                    drawingContext.DrawLine(pen,
                        new Point(x, posY),
                        new Point(x + _formattedTextSize.Width, posY));
                }

                if (TextRun.Properties.Strikethrough)
                {
                    double penThickness = glyphTypeface.StrikethroughThickness == 0 ?
                        0.5 :
                        glyphTypeface.StrikethroughThickness * scale;

                    var pen = new Pen(TextRun.Properties.ForegroundBrush, penThickness);

                    var posY = glyphTypeface.StrikethroughPosition == 0
                        ? glyphTypeface.LineHeight * scale / 2
                        : baseline + glyphTypeface.StrikethroughPosition * scale;

                    drawingContext.DrawLine(pen,
                        new Point(x, posY),
                        new Point(x + _formattedTextSize.Width, posY));
                }
            }
        }

        public bool UpdateTrailingInfo(TrailingInfo trailing)
        {
            if (IsEnd) return true;

            if (IsTab) return false;

            var index = Length;
            if (index > 0 && IsSpace(StringRange[index - 1]))
            {
                while (index > 0 && IsSpace(StringRange[index - 1]))
                {
                    trailing.SpaceWidth += _glyphWidths[index - 1];
                    index--;
                    trailing.Count++;
                }

                return index == 0;
            }

            return false;
        }

        public double GetDistanceFromCharacter(int index)
        {
            if (!IsEnd && !IsTab)
            {
                if (index > Length)
                {
                    index = Length;
                }

                double distance = 0;
                for (var i = 0; i < index; i++)
                {
                    distance += _glyphWidths[i];
                }

                return distance;
            }

            return index > 0 ? Width : 0;
        }

        public (int firstIndex, int trailingLength) GetCharacterFromDistance(double distance)
        {
            if (IsEnd) return (0, 0);

            if (Length <= 0) return (0, 0);

            var index = 0;
            double width = 0;
            for (; index < Length; index++)
            {
                width = IsTab ? Width / Length : _glyphWidths[index];
                if (distance < width)
                {
                    break;
                }

                distance -= width;
            }

            return index < Length
                ? (index, distance > width / 2 ? 1 : 0)
                : (Length - 1, 1);
        }

        private static bool IsSpace(char ch)
        {
            return ch == ' ' || ch == '\u00a0';
        }

        class GlyphWidths : IReadOnlyList<double>
        {
            private const double NOT_CALCULATED_YET = -1;
            private double[] _glyphWidths;
            private GlyphTypeface _typeFace;
            private StringRange _range;
            private double _scale;
            private double _tabSize;

            public int Count => _glyphWidths.Length;
            public double this[int index] => GetAt(index);

            internal GlyphWidths(StringRange range, GlyphTypeface typeFace, double fontSize, double tabSize)
            {
                _range = range;
                _typeFace = typeFace;
                _scale = fontSize / _typeFace.DesignEmHeight;
                _tabSize = tabSize;

                InitGlyphWidths();
            }

            double GetAt(int index)
            {
                if (_glyphWidths.Length == 0)
                    return 0;

                if (_range[index] == '\t')
                    return _tabSize;

                if (_glyphWidths[index] == NOT_CALCULATED_YET)
                    _glyphWidths[index] = MeasureGlyphAt(index);

                return _glyphWidths[index];
            }

            double MeasureGlyphAt(int index)
            {
                return _typeFace.GetGlyphAdvance(
                    _typeFace.GetGlyph(_range[index])) * _scale;
            }

            void InitGlyphWidths()
            {
                int capacity = _range.Length;

                bool useCheapGlyphMeasurement = 
                    capacity >= VisualLine.LENGTH_LIMIT && 
                    _typeFace.IsFixedPitch;

                if (useCheapGlyphMeasurement)
                {
                    double size = MeasureGlyphAt(0);
                    _glyphWidths = Enumerable.Repeat<double>(size, capacity).ToArray();
                    return;
                }

                _glyphWidths = Enumerable.Repeat<double>(NOT_CALCULATED_YET, capacity).ToArray();
            }

            IEnumerator<double> IEnumerable<double>.GetEnumerator()
            {
                foreach (double value in _glyphWidths)
                    yield return value;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _glyphWidths.GetEnumerator();
            }
        }
    }
}