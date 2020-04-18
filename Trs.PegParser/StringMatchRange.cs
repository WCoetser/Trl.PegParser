using System;

namespace Trs.PegParser
{
    public readonly struct StringMatchRange : IEquatable<StringMatchRange>
    {
        public int StartIndex { get; }

        public int Length { get; }

        public StringMatchRange(int startIndex, int length)
            => (StartIndex, Length) = (startIndex, length);

        public override bool Equals(object obj)
            => obj is StringMatchRange other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(StartIndex, Length);

        public bool Equals(StringMatchRange other)
            => StartIndex == other.StartIndex && Length == other.Length;

        public static bool operator ==(StringMatchRange left, StringMatchRange right)
            => left.Equals(right);

        public static bool operator !=(StringMatchRange left, StringMatchRange right)
            => !(left == right);
    }
}
