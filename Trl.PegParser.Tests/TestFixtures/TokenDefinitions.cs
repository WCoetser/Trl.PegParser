using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Trl.PegParser.Tokenization;

namespace Trl.PegParser.Tests.TestFixtures
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

        public static IEnumerable<TokenDefinition<TokenNames>> PrioritizationTest => new[]
        {
            Peg.Facade().Token(TokenNames.String, new Regex("\"([^\"]|(\\\"))*\"", RegexOptions.Compiled)), // \" is used to escape quote characters
            Peg.Facade().Token(TokenNames.Identifier, new Regex(@"[_a-zA-Z\d]\w*(\.[_a-zA-Z\d]\w*)*", RegexOptions.Compiled)),
            Peg.Facade().Token(TokenNames.Whitespace, new Regex(@"\s+", RegexOptions.Compiled)),
            Peg.Facade().Token(TokenNames.SemiColon, new Regex(@";", RegexOptions.Compiled))
        };
    }
}
