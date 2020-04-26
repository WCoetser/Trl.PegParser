using System;
using System.Collections.Generic;
using System.Linq;
using Trs.PegParser.Tokenization;

namespace Trs.PegParser.Grammer
{
    /// <summary>
    /// Represents a matched sequence of tokens.
    /// </summary>
    public class TokensMatch<TTokenTypeName>
        where TTokenTypeName : Enum 
    {
        private readonly IReadOnlyList<TokenMatch<TTokenTypeName>> _inputTokens;

        public TokensMatch(IReadOnlyList<TokenMatch<TTokenTypeName>> inputTokens, MatchRange matchedIndices)
            => (_inputTokens, MatchedIndices) = (inputTokens, matchedIndices);

        public IEnumerable<TokenMatch<TTokenTypeName>> GetMatchedTokens()
            => Enumerable.Range(MatchedIndices.StartIndex, MatchedIndices.Length)
                    .Select(i => _inputTokens[i]);

        public string GetMatchedString() 
            => string.Concat(GetMatchedTokens().Select(t => t.GetMatchedString()));

        public MatchRange MatchedIndices { get; }
    }
}
