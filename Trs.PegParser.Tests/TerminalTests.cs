using System;
using Trs.PegParser.Tests.TestFixtures;
using Xunit;

namespace Trs.PegParser.Tests
{
    public class TerminalTests
    {
        [Fact]
        public void ShouldParse()
        {
            var inputString = "aaa";
            
            // Arrange ... Store values for assert
            MatchRange? matchedTokenRangeAssert = null;
            string[] subActionResults = Array.Empty<string>();

            // Arrange ... Set up parser
            var peg = new PegFacade<TokenNames, ParsingRuleNames, string>();
            var tokenizer = peg.Tokenizer(TokenDefinitions.JustA);
            var extractValue = peg.SemanticAction((matchedTokenRange, inputTokens, subResults) =>
            {
                // Extract string result of matching the Terminal symbol
                matchedTokenRangeAssert = matchedTokenRange;
                subActionResults = subResults;
                return peg.GetStringValue(matchedTokenRange, inputTokens, inputString);
            });
            var parser = peg.Parser(ParsingRuleNames.MatchA, new[] { 
                peg.Rule(ParsingRuleNames.MatchA, peg.Terminal(TokenNames.A, extractValue)),
            });

            // Act
            var tokens = tokenizer.Tokenize(inputString);
            if (!tokens.Succeed) throw new Exception();
            var parseResult = parser.Parse(tokens.MatchedRanges);

            // Assert
            Assert.Equal(new MatchRange(0, 1), matchedTokenRangeAssert);
            Assert.Null(subActionResults);
            Assert.True(parseResult.Succeed);
            Assert.Equal(inputString, parseResult.SemanticActionResult);
            Assert.Equal(1, parseResult.NextParsePosition);
        }
    }
}
