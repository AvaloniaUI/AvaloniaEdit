using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using AvaloniaEdit.Utils;
using Avalonia.Media;

namespace AvaloniaEdit.Text
{
    internal sealed class TextLineImpl : TextLine
    {
        private readonly TextLineRun[] _runs;

        public override int FirstIndex { get; }

        public override int Length { get; }

        public override int TrailingWhitespaceLength { get; }

        public override double Width { get; }

        public override double WidthIncludingTrailingWhitespace { get; }

        public override double Height { get; }

        public override double Baseline { get; }

        internal static TextLineImpl Create(TextParagraphProperties paragraphProperties, int firstIndex, int paragraphLength, TextSource textSource)
        {
            var index = firstIndex;
            var visibleLength = 0;
            var widthLeft = paragraphProperties.TextWrapping == TextWrapping.Wrap && paragraphLength > 0 ? paragraphLength : double.MaxValue;
            TextLineRun prevRun = null;
            var run = TextLineRun.Create(textSource, index, firstIndex, widthLeft);
            if (!run.IsEnd && run.Width <= widthLeft)
            {
                index += run.Length;
                widthLeft -= run.Width;
                prevRun = run;
                run = TextLineRun.Create(textSource, index, firstIndex, widthLeft);
            }

            var trailing = new TrailingInfo();
            var runs = new List<TextLineRun>(2);
            if (prevRun != null)
            {
                visibleLength += AddRunReturnVisibleLength(runs, prevRun);
            }

            while (true)
            {
                visibleLength += AddRunReturnVisibleLength(runs, run);
                index += run.Length;
                widthLeft -= run.Width;
                if (run.IsEnd || widthLeft <= 0)
                {
                    trailing.SpaceWidth = 0;
                    UpdateTrailingInfo(runs, trailing);
                    return new TextLineImpl(paragraphProperties, firstIndex, runs, trailing);
                }

                run = TextLineRun.Create(textSource, index, firstIndex, widthLeft);
            }
        }

        private TextLineImpl(TextParagraphProperties paragraphProperties, int firstIndex, List<TextLineRun> runs, TrailingInfo trailing)
        {
            var top = 0.0;
            var height = 0.0;

            var index = 0;
            _runs = new TextLineRun[runs.Count];

            foreach (var run in runs)
            {
                _runs[index++] = run;

                if (run.Length <= 0) continue;

                if (run.IsEnd)
                {
                    trailing.Count += run.Length;
                }
                else
                {
                    top = Math.Max(top, run.Height - run.Baseline);
                    height = Math.Max(height, run.Height);
                    Baseline = Math.Max(Baseline, run.Baseline);
                }

                Length += run.Length;
                WidthIncludingTrailingWhitespace += run.Width;
            }

            Height = Math.Max(height, Baseline + top);

            if (Height <= 0)
            {
                Height = TextLineRun.GetDefaultLineHeight(paragraphProperties.DefaultTextRunProperties.FontMetrics);
                Baseline = TextLineRun.GetDefaultBaseline(paragraphProperties.DefaultTextRunProperties.FontMetrics);
            }

            FirstIndex = firstIndex;
            TrailingWhitespaceLength = trailing.Count;
            Width = WidthIncludingTrailingWhitespace - trailing.SpaceWidth;
        }

        private static void UpdateTrailingInfo(List<TextLineRun> runs, TrailingInfo trailing)
        {
            for (var index = (runs?.Count ?? 0) - 1; index >= 0; index--)
            {
                // ReSharper disable once PossibleNullReferenceException
                if (!runs[index].UpdateTrailingInfo(trailing))
                {
                    return;
                }
            }
        }

        private static int AddRunReturnVisibleLength(List<TextLineRun> runs, TextLineRun run)
        {
            if (run.Length > 0)
            {
                runs.Add(run);
                if (!run.IsEnd)
                {
                    return run.Length;
                }
            }

            return 0;
        }

        public override void Draw(DrawingContext drawingContext, Point origin)
        {
            if (drawingContext == null) throw new ArgumentNullException(nameof(drawingContext));

            if (_runs.Length == 0)
            {
                return;
            }

            double width = 0;
            var y = origin.Y;

            foreach (var run in _runs)
            {
                run.Draw(drawingContext, width + origin.X, y);
                width += run.Width;
            }
        }

        public override double GetDistanceFromCharacter(int firstIndex, int trailingLength)
        {
            double distance = 0;
            var index = firstIndex + (trailingLength != 0 ? 1 : 0) - FirstIndex;
            var runs = _runs;
            foreach (var run in runs)
            {
                distance += run.GetDistanceFromCharacter(index);
                if (index <= run.Length)
                {
                    break;
                }

                index -= run.Length;
            }

            return distance;
        }

        public override (int firstIndex, int trailingLength) GetCharacterFromDistance(double distance)
        {
            var firstIndex = FirstIndex;
            if (distance < 0)
            {
                return (FirstIndex, 0);
            }

            (int firstIndex, int trailingLength) result = (FirstIndex, 0);

            foreach (var run in _runs)
            {
                if (!run.IsEnd)
                {
                    firstIndex += result.trailingLength;
                    result = run.GetCharacterFromDistance(distance);
                    firstIndex += result.firstIndex;
                }

                if (distance <= run.Width)
                {
                    break;
                }

                distance -= run.Width;
            }

            return (firstIndex, result.trailingLength);
        }

        public override Rect GetTextBounds(int firstIndex, int textLength)
        {
            if (textLength == 0) throw new ArgumentOutOfRangeException(nameof(textLength));

            if (textLength < 0)
            {
                firstIndex += textLength;
                textLength = -textLength;
            }

            if (firstIndex < FirstIndex)
            {
                textLength += firstIndex - FirstIndex;
                firstIndex = FirstIndex;
            }

            if (firstIndex + textLength > FirstIndex + Length)
            {
                textLength = FirstIndex + Length - firstIndex;
            }

            var distance = GetDistanceFromCharacter(firstIndex, 0);
            var distanceToLast = GetDistanceFromCharacter(firstIndex + textLength, 0);

            return new Rect(distance, 0.0, distanceToLast - distance, Height);
        }

        public override IList<TextRun> GetTextRuns()
        {
            return _runs.Select(x => x.TextRun).ToArray();
        }
    }
}