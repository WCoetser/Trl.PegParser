using System;

namespace Trl.PegParser
{
    /// <summary>
    /// Specifies a sequential range on a list or array using start index and length.
    /// </summary>
    public readonly struct MatchRange : IEquatable<MatchRange>
    {
        public int StartIndex { get; }

        public int Length { get; }

        public MatchRange(int startIndex, int length)
            => (StartIndex, Length) = (startIndex, length);

        public override bool Equals(object obj)
            => obj is MatchRange other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(StartIndex, Length);

        public bool Equals(MatchRange other)
            => StartIndex == other.StartIndex && Length == other.Length;

        public static bool operator ==(MatchRange left, MatchRange right)
            => left.Equals(right);

        public static bool operator !=(MatchRange left, MatchRange right)
            => !(left == right);
    }
}
