using System;
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

        internal void Update(DocumentChangeEventArgs e)
        {
            int _lineCount = _document.Lines.Count;

            if (e != null && e.OffsetChangeMap != null && _lineRanges != null && _lineCount == _lineRanges.Length)
            {
                // it's a single-line change
                // update the offsets usign the OffsetChangeMap

                var changedLine = _document.GetLineByOffset(e.Offset);
                int lineIndex = changedLine.LineNumber - 1;

                _lineRanges[lineIndex].Offset = changedLine.Offset;
                _lineRanges[lineIndex].Length = changedLine.Length;

                for (int i = lineIndex + 1; i < _lineCount; i++)
                {
                    _lineRanges[i].Offset = e.OffsetChangeMap.GetNewOffset(_lineRanges[i].Offset);
                }
            }
            else
            {
                // recompute all the line ranges
                // based in the document lines

                Array.Resize(ref _lineRanges, _lineCount);

                int startLineIndex = (e != null) ?
                    _document.GetLineByOffset(e.Offset).LineNumber - 1 : 0;

                for (int i = startLineIndex; i < _lineCount; i++)
                {
                    var line = _document.Lines[i];
                    _lineRanges[i].Offset = line.Offset;
                    _lineRanges[i].Length = line.Length;
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
