using System;

namespace Trs.PegParser.Grammer
{
    public class ParseResult<TTokenTypeName, TActionResult>
        where TTokenTypeName: Enum
    {
        /// <summary>
        /// Specifies whether parsing succeeded.
        /// </summary>
        public bool Succeed { get; }

        /// <summary>
        /// Where to parse from next.
        /// </summary>
        public int NextParseStartIndex { get; }

        /// <summary>
        /// Range of tokens matched.
        /// </summary>
        public TokensMatch<TTokenTypeName> MatchedTokens { get; }

        /// <summary>
        /// The result of the semantic actions carried out when this match took place.
        /// </summary>
        public TActionResult SemanticActionResult { get; }

        private ParseResult(bool succeed, int nextParseStartIndex, TokensMatch<TTokenTypeName> matchedTokens = null, TActionResult semanticActionResult = default)
            => (NextParseStartIndex, Succeed, MatchedTokens, SemanticActionResult) = (nextParseStartIndex, succeed, matchedTokens, semanticActionResult);

        public static ParseResult<TTokenTypeName, TActionResult>
            Succeeded(int nextParseStartIndex, TokensMatch<TTokenTypeName> matchedTokens, TActionResult semanticActionResult)
                => new ParseResult<TTokenTypeName, TActionResult>(true, nextParseStartIndex, matchedTokens, semanticActionResult);

        public static ParseResult<TTokenTypeName, TActionResult>
            Failed(int nextParseStartIndex)
            
                => new ParseResult<TTokenTypeName, TActionResult>(false, nextParseStartIndex);

    }
}
