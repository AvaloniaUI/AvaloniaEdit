using System;
using System.Diagnostics;

using AvaloniaEdit.Document;

namespace AvaloniaEdit.TextMate
{
    internal class LineRanges
    {
        private LineRange[] _lineRanges;
        private TextDocument _document;
        internal LineRanges(TextDocument document)
        {
            _document = document;
            _lineRanges = new LineRange[document.LineCount];

            Update(null);
        }

        internal LineRange Get(int lineIndex)
        {
            return _lineRanges[lineIndex];
        }

        Stopwatch sw = new Stopwatch();

        internal void Update(DocumentChangeEventArgs e)
        {
            int lineCount = _document.Lines.Count;

            if (e != null && e.OffsetChangeMap != null && _lineRanges != null && lineCount == _lineRanges.Length)
            {
                // it's a single-line change
                // update the offsets usign the OffsetChangeMap

                var changedLine = _document.GetLineByOffset(e.Offset);
                int lineIndex = changedLine.LineNumber - 1;

                _lineRanges[lineIndex].Offset = changedLine.Offset;
                _lineRanges[lineIndex].Length = changedLine.Length;

                for (int i = lineIndex + 1; i < lineCount; i++)
                {
                    _lineRanges[i].Offset = e.OffsetChangeMap.GetNewOffset(_lineRanges[i].Offset);
                }
            }
            else
            {
                // recompute all the line ranges
                // based in the document lines

                Array.Resize(ref _lineRanges, lineCount);

                int currentLineIndex = (e != null) ?
                    _document.GetLineByOffset(e.Offset).LineNumber - 1 : 0;
                var currentLine = _document.GetLineByNumber(currentLineIndex + 1);

                while (currentLine != null)
                {
                    _lineRanges[currentLineIndex].Offset = currentLine.Offset;
                    _lineRanges[currentLineIndex].Length = currentLine.Length;
                    currentLine = currentLine.NextLine;
                    currentLineIndex++;
                }
            }
        }
    }

    internal struct LineRange
    {
        public int Offset;
        public int Length;
    }
}
