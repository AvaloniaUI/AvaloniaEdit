using System;

namespace AvaloniaEdit.Text
{
    public struct StringRange : IEquatable<StringRange>
    {
        public string String { get; }

        public int Length { get; }

        public static StringRange Empty => default(StringRange);

        internal int OffsetToFirstChar { get; }

        internal char this[int index] => String?[OffsetToFirstChar + index] ?? '\0';

        public StringRange(string s, int offsetToFirstChar, int length)
        {
            String = s;
            OffsetToFirstChar = offsetToFirstChar;
            Length = length;
        }

        public override string ToString()
        {
            if (String == null) return string.Empty;

            if (OffsetToFirstChar == 0 && Length == String.Length) return String;

            return String.Substring(OffsetToFirstChar, Length);
        }

        public bool Equals(StringRange other)
        {
            return string.Equals(String, other.String, StringComparison.Ordinal) &&
                   Length == other.Length &&
                   OffsetToFirstChar == other.OffsetToFirstChar;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is StringRange && Equals((StringRange)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (String != null ? String.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Length;
                hashCode = (hashCode * 397) ^ OffsetToFirstChar;
                return hashCode;
            }
        }

        public static bool operator ==(StringRange left, StringRange right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StringRange left, StringRange right)
        {
            return !left.Equals(right);
        }
    }
}