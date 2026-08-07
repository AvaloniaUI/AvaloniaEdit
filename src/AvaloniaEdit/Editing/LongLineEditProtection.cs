using System;
using System.Collections.Generic;

using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;

namespace AvaloniaEdit.Editing
{
    /// <summary>
    /// Disallow edits in the collapsed tail of long lines (see <see cref="VisualLine.LENGTH_LIMIT"/>):
    /// </summary>
    internal static class LongLineEditProtection
    {
        internal static bool CanInsert(TextDocument document, int offset)
        {
            if (document == null)
                return true;

            DocumentLine line = document.GetLineByOffset(offset);
            return offset - line.Offset < VisualLine.LENGTH_LIMIT;
        }

        internal static IEnumerable<ISegment> GetDeletableSegments(
            TextDocument document, ISegment segment)
        {
            if (segment == null)
                throw new ArgumentNullException(nameof(segment));

            if (document == null)
            {
                yield return segment;
                yield break;
            }

            if (segment.Length == 0)
            {
                if (CanInsert(document, segment.Offset))
                    yield return segment;
                yield break;
            }

            int deletableStart = segment.Offset;
            int segmentEnd = segment.EndOffset;

            DocumentLine line = document.GetLineByOffset(segment.Offset);
            while (line != null && line.Offset < segmentEnd)
            {
                if (line.Length > VisualLine.LENGTH_LIMIT)
                {
                    // protect the hidden text plus the line delimiter; deleting the
                    // delimiter would merge the next line into the hidden region
                    int protectedStart = line.Offset + VisualLine.LENGTH_LIMIT;
                    int protectedEnd = line.Offset + line.TotalLength;

                    if (protectedStart < segmentEnd && protectedEnd > deletableStart)
                    {
                        if (protectedStart > deletableStart)
                        {
                            yield return new SimpleSegment(
                                deletableStart, protectedStart - deletableStart);
                        }

                        deletableStart = Math.Min(protectedEnd, segmentEnd);
                    }
                }

                line = line.NextLine;
            }

            if (deletableStart < segmentEnd)
                yield return new SimpleSegment(deletableStart, segmentEnd - deletableStart);
        }
    }
}
