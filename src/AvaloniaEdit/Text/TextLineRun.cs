using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

using AvaloniaEdit.Rendering;
using AvaloniaEdit.Utils;

namespace AvaloniaEdit.Text
{
    internal sealed class TextLineRun
    {
        private const string NewlineString = "\r\n";
        private const string TabString = "\t";

        private Avalonia.Media.TextFormatting.TextLine _formattedText;
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
                       CreateRunForText(stringRange, textRun, widthLeft, false, true, paragraphProperties);
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

        internal static TextLineRun CreateRunForText(StringRange stringRange, TextRun textRun, double widthLeft, bool emergencyWrap, bool breakOnTabs, TextParagraphProperties paragraphProperties)
        {
            var ngwdIdx = stringRange.IndexOf('\t');

            StringRange text;
            TextRun run;

            if (ngwdIdx == -1)
            {
                text = stringRange;
                run = textRun;
            }
            else
            {

                text = stringRange.SubRange(0, ngwdIdx);
                run = new TextRunImpl(text, textRun.Properties);

            }

            var linerun = new TextLineRun
            {
                StringRange = text,
                TextRun = run,
                Length = run.Length
            };

            var tf = linerun.Typeface;

            var useCheapGlyphMeasurement =
                    run.Length >= VisualLine.LENGTH_LIMIT &&
                    tf.GlyphTypeface.IsFixedPitch;


            var line = TextFormatterFactory.CreateTextLine(
                               text.ToString(),
                               new Typeface(tf.FontFamily, tf.Style, tf.Weight),
                               linerun.FontSize,
                               run.Properties.ForegroundBrush);

            linerun._formattedText = line;

            var size = new Size(line.WidthIncludingTrailingWhitespace, line.Height);

            linerun._formattedTextSize = size;

            linerun.Width = size.Width;

            linerun._glyphWidths = new CharacterWidths(line, text.Length, useCheapGlyphMeasurement);

            return linerun;
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

                _formattedText.Draw(drawingContext, new Point(x, y));

                var glyphTypeface = TextRun.Properties.Typeface.GlyphTypeface;

                var scale = TextRun.Properties.FontSize / glyphTypeface.DesignEmHeight;

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
                    var pen = new Pen(TextRun.Properties.ForegroundBrush, glyphTypeface.StrikethroughThickness * scale);

                    var posY = baseline + glyphTypeface.StrikethroughPosition * scale;

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

        class CharacterWidths : IReadOnlyList<double>
        {
            private Avalonia.Media.TextFormatting.TextLine _measure;
            private double[] _width;
            private bool _isMeasured;

            public CharacterWidths(Avalonia.Media.TextFormatting.TextLine measure, int length, bool useCheapGlyphMeasurement)
            {
                _measure = measure;
                _width = new double[length];
                Count = length;
                _isMeasured = false;

                if (useCheapGlyphMeasurement)
                {
                    _isMeasured = true;

                    var hit = _measure.GetNextCaretCharacterHit(new CharacterHit(0));
                    var firstCharWid = _measure.GetDistanceFromCharacterHit(hit);

                    for (var i = 0; i < Count; ++i)
                        _width[i] = firstCharWid;
                }
            }


            public double this[int index]
            {
                get
                {
                    if (!_isMeasured)
                    {
                        var hit = new CharacterHit(0);
                        double prevPos = 0;
                        for (var i = 0; i < Count; ++i)
                        {
                            hit = _measure.GetNextCaretCharacterHit(hit);
                            var dist = _measure.GetDistanceFromCharacterHit(hit);
                            _width[i] = dist - prevPos;

                            i = Math.Max(i, hit.FirstCharacterIndex - 1);
                            prevPos = dist;
                        }

                        _isMeasured = true;
                    }

                    return _width[index];
                }
            }

            public int Count { get; }

            public IEnumerator<double> GetEnumerator()
            {
                for (int i = 0; i < Count; ++i)
                    yield return this[i];
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        class TextRunImpl : TextRun
        {
            public sealed override StringRange StringRange { get; }

            public sealed override int Length { get; }

            public sealed override TextRunProperties Properties { get; }


            public TextRunImpl(StringRange stringRange, TextRunProperties textRunProperties)
            {
                StringRange = stringRange;
                Length = stringRange.Length;
                Properties = textRunProperties;
            }
        }
    }
}