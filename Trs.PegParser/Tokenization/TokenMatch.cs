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
        
        public StringMatchRange MatchedCharacterRange { get;  }

        internal TokenMatch(TTokenName name, StringMatchRange matchedCharacterRange)
            => (TokenName, MatchedCharacterRange) = (name, matchedCharacterRange);        
    }
}
