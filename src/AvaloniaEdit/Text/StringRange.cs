using System;

namespace AvaloniaEdit.Text
{
    public struct StringRange : IEquatable<StringRange>
    {
        public string String { get; }

        public int Length { get; set; }

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
            return ToString(String, OffsetToFirstChar, Length);
        }

        public string ToString(int maxLength)
        {
            return ToString(String, OffsetToFirstChar, maxLength);
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

        public StringRange WithLength(int length)
        {
            return new StringRange(String, OffsetToFirstChar, length);
        }

        static string ToString(string value, int offsetToFirstChar, int length)
        {
            if (value == null) return string.Empty;

            if (offsetToFirstChar == 0 && length == value.Length) return value;

            return value.Substring(offsetToFirstChar, length);
        }
    }
}