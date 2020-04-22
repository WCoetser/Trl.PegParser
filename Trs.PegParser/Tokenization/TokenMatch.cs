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

        public string InputString { get; }

        internal TokenMatch(TTokenName name, MatchRange matchedCharacterRange, string inputString)
            => (TokenName, MatchedCharacterRange, InputString) = (name, matchedCharacterRange, inputString);

        public string GetMatchedString()
            => MatchedCharacterRange.Length switch
            {
                0 => string.Empty,
                _ => InputString.Substring(MatchedCharacterRange.StartIndex, MatchedCharacterRange.Length)
            };
    }
}
