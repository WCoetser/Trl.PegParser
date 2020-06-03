using System;
using System.Text.RegularExpressions;

namespace Trl.PegParser.Tokenization
{
    public class TokenDefinition<TTokenNameType>
        where TTokenNameType: Enum
    {
        public TTokenNameType Name { get; set; }

        public Regex DefiningRegex { get; set; }

        public TokenDefinition(TTokenNameType name, Regex definition)
            => (Name, DefiningRegex) = (name, definition);
    }
}
