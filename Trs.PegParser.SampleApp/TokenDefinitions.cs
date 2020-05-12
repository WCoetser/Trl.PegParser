using System.Collections.Generic;
using System.Text.RegularExpressions;
using Trs.PegParser.Tokenization;

namespace Trs.PegParser.SampleApp
{
    public enum TokensNames
    {
        Number,
        Comma,
        X,
        Plus,
        Minus,
        Multiply,
        Divide,
        Sin,
        Cos,
        Domain,
        Semicolon,
        Whitespace,
        OpenRoundBracket,
        CloseRoundBracket
    }

    public class TokenDefinitions
    {
        public static IEnumerable<TokenDefinition<TokensNames>> GetTokenDefinitions()
        {
            return new[]
            {
                new TokenDefinition<TokensNames>(TokensNames.Number, new Regex(@"[-+]?[0-9]+\.?[0-9]*")),
                new TokenDefinition<TokensNames>(TokensNames.Comma, new Regex(@"\,")),
                new TokenDefinition<TokensNames>(TokensNames.X, new Regex("x")),
                new TokenDefinition<TokensNames>(TokensNames.Plus, new Regex(@"\+")),
                new TokenDefinition<TokensNames>(TokensNames.Minus, new Regex(@"\-")),
                new TokenDefinition<TokensNames>(TokensNames.Multiply, new Regex(@"\*")),
                new TokenDefinition<TokensNames>(TokensNames.Divide, new Regex(@"\/")),
                new TokenDefinition<TokensNames>(TokensNames.Sin, new Regex(@"sin")),
                new TokenDefinition<TokensNames>(TokensNames.Cos, new Regex(@"cos")),
                new TokenDefinition<TokensNames>(TokensNames.Domain, new Regex(@"domain")),
                new TokenDefinition<TokensNames>(TokensNames.Semicolon, new Regex(";")),
                new TokenDefinition<TokensNames>(TokensNames.Whitespace, new Regex(@"\s+")),
                new TokenDefinition<TokensNames>(TokensNames.OpenRoundBracket, new Regex(@"\(")),
                new TokenDefinition<TokensNames>(TokensNames.CloseRoundBracket, new Regex(@"\)")),
            };
        }
    }
}
