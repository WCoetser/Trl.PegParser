using System;
using System.Collections.Generic;

namespace Trl.PegParser.Tokenization
{
    public class TokenizationResult<TTokenName>
        where TTokenName : Enum
    {
        public bool Succeed { get => UnmatchedRanges.Count == 0; }

        public IReadOnlyList<MatchRange> UnmatchedRanges { get; }

        public IReadOnlyList<TokenMatch<TTokenName>> MatchedRanges { get; }

        internal TokenizationResult(IReadOnlyList<TokenMatch<TTokenName>> matchedRanges, IReadOnlyList<MatchRange> unmatchedRanges)
            => (UnmatchedRanges, MatchedRanges) = (unmatchedRanges, matchedRanges);
    }
}
