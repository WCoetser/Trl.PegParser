using System;
using System.Collections.Generic;

namespace Trs.PegParser.Tokenization
{
    public class TokenizationResult<TTokenName>
        where TTokenName : Enum
    {
        public bool Succeeded { get => UnmatchedRanges.Count == 0; }

        public IReadOnlyList<StringMatchRange> UnmatchedRanges { get; }

        public IReadOnlyList<TokenMatch<TTokenName>> MatchedRanges { get; }

        internal TokenizationResult(IReadOnlyList<TokenMatch<TTokenName>> matchedRanges, IReadOnlyList<StringMatchRange> unmatchedRanges)
            => (UnmatchedRanges, MatchedRanges) = (unmatchedRanges, matchedRanges);
    }
}
