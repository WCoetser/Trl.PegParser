using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Trs.PegParser.Tokenization;

namespace Trs.PegParser.Tests.TestFixtures
{
    public static class TokenDefinitions
    {       
        public static IEnumerable<TokenDefinition<TokenNames>> Empty => Enumerable.Empty<TokenDefinition<TokenNames>>();

        public static IEnumerable<TokenDefinition<TokenNames>> MatchEmptyString => new[] {
                Peg.Facade().Token(TokenNames.Empty, new Regex("^$"))
            };

        public static IEnumerable<TokenDefinition<TokenNames>> AB => new[]
        {
            Peg.Facade().Token(TokenNames.A, new Regex("a+", RegexOptions.IgnoreCase)),
            Peg.Facade().Token(TokenNames.B, new Regex("b+", RegexOptions.IgnoreCase))
        };

        public static IEnumerable<TokenDefinition<TokenNames>> JustA => new[]
        {
            Peg.Facade().Token(TokenNames.A, new Regex("a+", RegexOptions.IgnoreCase))
        };
    }
}
