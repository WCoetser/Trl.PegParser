using System;

namespace Trs.PegParser.Tokenization
{
    public class TokenMatch<TTokenName>
        where TTokenName: Enum
    {
        /// <summary>
        /// The name of the token definition.
        /// </summary>
        public TTokenName TokenName { get; }
        
        public MatchRange MatchedCharacterRange { get;  }

        internal TokenMatch(TTokenName name, MatchRange matchedCharacterRange)
            => (TokenName, MatchedCharacterRange) = (name, matchedCharacterRange);
    }
}
