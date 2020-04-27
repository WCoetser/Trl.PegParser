using System.Collections.Generic;
using Trs.PegParser.Grammer;
using Trs.PegParser.Tests.TestFixtures;
using Xunit;

namespace Trs.PegParser.Tests
{
    public class EmptyStringTests
    {
        [Fact]
        public void ShouldParseEmptyInputTokens()
        {
            // Arrange ... Store values for assert
            TokensMatch<TokenNames> matchedTokenRangeAssert = null;
            IEnumerable<string> subActionResults = null;
            string inputString = string.Empty;

            // Arrange ... Set up parser
            var peg = new PegFacade<TokenNames, ParsingRuleNames, string>();
            var tokenizer = peg.Tokenizer(TokenDefinitions.AB);
            var extractValue = peg.SemanticAction((tokensMatch, subResults) =>
            {
                // Extract string result of matching the Terminal symbol
                matchedTokenRangeAssert = tokensMatch;
                subActionResults = subResults;
                return tokensMatch.GetMatchedString();
            });
            peg.SetDefaultEmptyStringAction(extractValue);
            var parser = peg.Parser(ParsingRuleNames.Start, new[] {
                peg.Rule(ParsingRuleNames.Start, peg.EmptyString()),
            });

            // Act
            var tokensResult = tokenizer.Tokenize(inputString);
            var parseResult = parser.Parse(tokensResult);

            // Assert
            Assert.Equal(new MatchRange(0, 0), parseResult.MatchedTokens.MatchedIndices);
            Assert.Null(subActionResults);
            Assert.True(parseResult.Succeed);
            Assert.Equal(inputString, parseResult.SemanticActionResult);
            Assert.Equal(0, parseResult.NextParsePosition);
        }
    }
}
