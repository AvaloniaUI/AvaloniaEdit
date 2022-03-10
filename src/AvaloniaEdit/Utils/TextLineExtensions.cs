using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Utilities;

namespace AvaloniaEdit.Utils;

#nullable enable

public static class TextLineExtensions
{
    public static IReadOnlyList<TextBounds> GetTextBounds(this TextLine textLine, int start, int length)
    {
        if (start + length <= 0)
        {
            return Array.Empty<TextBounds>();
        }

        var result = new List<TextBounds>(textLine.TextRuns.Count);
        
        var currentPosition = 0;
        var currentRect = Rect.Empty;
        
        //Current line isn't covered.
        if (textLine.TextRange.Length <= start)
        {
            return result;
        }

        //The whole line is covered.
        if (currentPosition >= start && start + length > textLine.TextRange.Length)
        {
            currentRect = new Rect(textLine.Start, 0, textLine.WidthIncludingTrailingWhitespace,
                textLine.Height);

            result.Add(new TextBounds{ Rectangle = currentRect});

            return result;
        }

        var startX = textLine.Start;

        //A portion of the line is covered.
        for (var index = 0; index < textLine.TextRuns.Count; index++)
        {
            var currentRun = textLine.TextRuns[index] as DrawableTextRun;
            var currentShaped = currentRun as ShapedTextCharacters;

            if (currentRun is null)
            {
                continue;
            }
            
            TextRun? nextRun = null;

            if (index + 1 < textLine.TextRuns.Count)
            {
                nextRun = textLine.TextRuns[index + 1];
            }

            if (nextRun != null)
            {
                if (nextRun.Text.Start < currentRun.Text.Start && start + length < currentRun.Text.End)
                {
                    goto skip;
                }

                if (currentRun.Text.Start >= start + length)
                {
                    goto skip;
                }

                if (currentRun.Text.Start > nextRun.Text.Start && currentRun.Text.Start < start)
                {
                    goto skip;
                }

                if (currentRun.Text.End < start)
                {
                    goto skip;
                }

                goto noop;

                skip:
                {
                    startX += currentRun.Size.Width;
                }

                continue;

                noop:
                {
                }
            }

            var endX = startX;
            var endOffset = 0d;

            if (currentShaped != null)
            {
                endOffset = currentShaped.GlyphRun.GetDistanceFromCharacterHit(
                    currentShaped.ShapedBuffer.IsLeftToRight ? new CharacterHit(start + length) : new CharacterHit(start));

                endX += endOffset;

                var startOffset = currentShaped.GlyphRun.GetDistanceFromCharacterHit(
                    currentShaped.ShapedBuffer.IsLeftToRight ? new CharacterHit(start) : new CharacterHit(start + length));

                startX += startOffset;

                var characterHit = currentShaped.GlyphRun.IsLeftToRight
                    ? currentShaped.GlyphRun.GetCharacterHitFromDistance(endOffset, out _)
                    : currentShaped.GlyphRun.GetCharacterHitFromDistance(startOffset, out _);

                currentPosition = characterHit.FirstCharacterIndex + characterHit.TrailingLength;
                
                if (nextRun is ShapedTextCharacters nextShaped)
                {
                    if (currentShaped.ShapedBuffer.IsLeftToRight == nextShaped.ShapedBuffer.IsLeftToRight)
                    {
                        endOffset = nextShaped.GlyphRun.GetDistanceFromCharacterHit(
                            nextShaped.ShapedBuffer.IsLeftToRight
                                ? new CharacterHit(start + length)
                                : new CharacterHit(start));

                        index++;

                        endX += endOffset;

                        currentRun = currentShaped = nextShaped;

                        if (nextShaped.ShapedBuffer.IsLeftToRight)
                        {
                            characterHit = nextShaped.GlyphRun.GetCharacterHitFromDistance(endOffset, out _);

                            currentPosition = characterHit.FirstCharacterIndex + characterHit.TrailingLength;
                        }
                    }
                }
            }
            
            if (endX < startX)
            {
                (endX, startX) = (startX, endX);
            }

            var width = endX - startX;

            if (result.Count > 0 && MathUtilities.AreClose(currentRect.Top, 0) &&
                MathUtilities.AreClose(currentRect.Right, startX))
            {
                var textBounds = new TextBounds {Rectangle = currentRect.WithWidth(currentRect.Width + width)};
                
                result[result.Count - 1] = textBounds;
            }
            else
            {
                currentRect = new Rect(startX, 0, width, textLine.Height);

                result.Add(new TextBounds{ Rectangle = currentRect});
            }

            if (currentShaped != null && currentShaped.ShapedBuffer.IsLeftToRight)
            {
                if (nextRun != null)
                {
                    if (nextRun.Text.Start > currentRun.Text.Start && nextRun.Text.Start >= start + length)
                    {
                        break;
                    }

                    currentPosition = nextRun.Text.End;
                }
                else
                {
                    if (currentPosition >= start + length)
                    {
                        break;
                    }
                }
            }
            else
            {
                if (currentPosition <= start)
                {
                    break;
                }
            }

            if (currentShaped != null && !currentShaped.ShapedBuffer.IsLeftToRight && currentPosition != currentRun.Text.Start)
            {
                endX += currentShaped.GlyphRun.Size.Width - endOffset;
            }

            startX = endX;
        }

        return result;
    }
}

public struct TextBounds
{
    public Rect Rectangle { get; set; }
}