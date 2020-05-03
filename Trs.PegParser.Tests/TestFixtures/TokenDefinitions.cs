using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Trs.PegParser.Tokenization;

namespace Trs.PegParser.Tests.TestFixtures
{
    public static class TokenDefinitions
    {
        // TODO: Create facade method for token definitions
        
        public static IEnumerable<TokenDefinition<TokenNames>> Empty => Enumerable.Empty<TokenDefinition<TokenNames>>();

        public static IEnumerable<TokenDefinition<TokenNames>> MatchEmptyString => new[] {
                new TokenDefinition<TokenNames>(TokenNames.Empty, new Regex("^$"))
            };

        public static IEnumerable<TokenDefinition<TokenNames>> AB => new[]
        {
            new TokenDefinition<TokenNames>(TokenNames.A, new Regex("a+", RegexOptions.IgnoreCase)),
            new TokenDefinition<TokenNames>(TokenNames.B, new Regex("b+", RegexOptions.IgnoreCase))
        };

        public static IEnumerable<TokenDefinition<TokenNames>> JustA => new[]
        {
            new TokenDefinition<TokenNames>(TokenNames.A, new Regex("a+", RegexOptions.IgnoreCase))
        };
    }
}
