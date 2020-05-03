using System.Collections.Generic;
using Trs.PegParser.Tests.TestFixtures;
using Xunit;

namespace Trs.PegParser.Tests
{
    public class EmptyStringTests
    {
        private readonly PegFacade<TokenNames, ParsingRuleNames, string> peg;
        private IEnumerable<string> subActionResults = null;

        public EmptyStringTests()
        {
            peg = new PegFacade<TokenNames, ParsingRuleNames, string>();
            var extractValue = peg.SemanticAction((tokensMatch, subResults) =>
            {
                // Extract string result of matching the Terminal symbol
                subActionResults = subResults;
                return tokensMatch.GetMatchedString();
            });
            peg.SetDefaultEmptyStringAction(extractValue);
        }

        [Fact]
        public void ShouldParseEmptyInputTokens()
        {
            // Arrange
            string inputString = string.Empty;
            var tokenizer = peg.Tokenizer(TokenDefinitions.AB);
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
            Assert.Equal(0, parseResult.NextParseStartIndex);
        }
    }
}
