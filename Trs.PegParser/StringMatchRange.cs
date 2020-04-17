using System;

namespace Trs.PegParser
{
    public readonly struct StringMatchRange
    {
        public int StartIndex { get; }

        public int Length { get; }

        public StringMatchRange(int startIndex, int length)
            => (StartIndex, Length) = (startIndex, length);

        public override bool Equals(object obj)
            => obj is StringMatchRange other 
                && StartIndex == other.StartIndex 
                && Length == other.Length;

        public override int GetHashCode()
            => HashCode.Combine(StartIndex, Length);
    }
}
