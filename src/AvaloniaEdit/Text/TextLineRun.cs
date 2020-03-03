using System;
using Avalonia;
using Avalonia.Media;

namespace AvaloniaEdit.Text
{
    internal sealed class TextLineRun
    {
        private const string NewlineString = "\r\n";
        internal const double BaselineFactor = 0.1;
        internal const double HeightFactor = 1.2;

        private FormattedText _formattedText;
        private Size _formattedTextSize;
        private double[] _glyphWidths;

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
                if (IsEmbedded && TextRun is TextEmbeddedObject embeddedObject)
                {
                    var box = embeddedObject.ComputeBoundingBox();
                    return box.Y;
                }
                return FontSize * BaselineFactor;
            }
        }

        public double Height
        {
            get
            {
                if(IsEnd)
                {
                    return 0.0;
                }
                if(IsEmbedded && TextRun is TextEmbeddedObject embeddedObject)
                {
                    var box = embeddedObject.ComputeBoundingBox();
                    return box.Height;
                }
                return FontSize * HeightFactor;
            }
        }

        public Typeface Typeface => TextRun.Properties.Typeface;

        public double FontSize => TextRun.Properties.FontSize;

        private TextLineRun()
        {
        }

        public static TextLineRun Create(TextSource textSource, int index, int firstIndex, double lengthLeft)
        {
            var textRun = textSource.GetTextRun(index);
            var stringRange = textRun.GetStringRange();
            return Create(textSource, stringRange, textRun, index, lengthLeft);
        }

        private static TextLineRun Create(TextSource textSource, StringRange stringRange, TextRun textRun, int index, double widthLeft)
        {
            if (textRun is TextCharacters)
            {
                return CreateRunForEol(textSource, stringRange, textRun, index) ??
                       CreateRunForText(stringRange, textRun, widthLeft, false, true);
            }

            if (textRun is TextEndOfLine)
            {
                return new TextLineRun(textRun.Length, textRun) { IsEnd = true };
            }

            if (textRun is TextEmbeddedObject embeddedObject)
            {
                double width = embeddedObject.GetSize(double.PositiveInfinity).Width;
                return new TextLineRun(textRun.Length, textRun) {
                    IsEmbedded = true,
                    _glyphWidths = new double[] { width },
                    // Embedded objects must propagate their width to the container.
                    // Otherwise text runs after the embedded object are drawn at the same x position.
                    Width = width
                };
            }

            throw new NotSupportedException("Unsupported run type");
        }

        private static TextLineRun CreateRunForEol(TextSource textSource, StringRange stringRange, TextRun textRun, int index)
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
                    return CreateRunForTab(textRun);
                default:
                    return null;
            }
        }

        private static TextLineRun CreateRunForTab(TextRun textRun)
        {
            var spaceRun = new TextCharacters(" ", textRun.Properties);
            var stringRange = spaceRun.StringRange;
            var run = new TextLineRun(1, spaceRun)
            {
                IsTab = true,
                StringRange = stringRange,
                // TODO: get from para props
                Width = 40
            };

            run.SetGlyphWidths();

            return run;
        }

        internal static TextLineRun CreateRunForText(StringRange stringRange, TextRun textRun, double widthLeft, bool emergencyWrap, bool breakOnTabs)
        {
            var run = new TextLineRun
            {
                StringRange = stringRange,
                TextRun = textRun,
                Length = textRun.Length
            };

            var tf = run.Typeface;
            var formattedText = new FormattedText
            {
                Text = stringRange.ToString(),
                Typeface = new Typeface(tf.FontFamily, tf.Weight, tf.Style),
                FontSize = run.FontSize
            };
       

            run._formattedText = formattedText;

            var size = formattedText.Bounds.Size;
            run._formattedTextSize = size;

            run.Width = size.Width;

            run.SetGlyphWidths();

            return run;
        }

        private TextLineRun(int length, TextRun textRun)
        {
            Length = length;
            TextRun = textRun;
        }

        private void SetGlyphWidths()
        {
            var result = new double[StringRange.Length];

            for (var i = 0; i < StringRange.Length; i++)
            {
                // TODO: is there a better way of getting glyph metrics?
                var tf = Typeface;
                var size = new FormattedText
                {
                    Text = StringRange[i].ToString(),
                    Typeface = new Typeface(tf.FontFamily, tf.Weight, tf.Style),
                    FontSize = FontSize
                }.Bounds.Size;
                
                result[i] = size.Width;
            }

            _glyphWidths = result;
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
    }
}