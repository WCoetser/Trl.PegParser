using System.Collections.Generic;
using System.Linq;
using Trs.PegParser.Grammer;
using Trs.PegParser.Tests.TestFixtures;
using Xunit;

namespace Trs.PegParser.Tests
{
    public class OptionalTests
    {
        [Fact]
        public void ShouldParseEmptyCase()
        {
            string testInput = string.Empty;

            TokensMatch<TokenNames> matchedTokenRangeAssert = null;
            IEnumerable<string> subActionResults = null;

            var peg = new PegFacade<TokenNames, ParsingRuleNames, string>();
            var tokenizer = peg.Tokenizer(TokenDefinitions.JustA);
            peg.SetDefaultTerminalAction(TokenNames.A, (tokens, _) => tokens.GetMatchedString());
            peg.SetDefaultOptionalAction((matchedTokenRange, subResults) =>
            {
                matchedTokenRangeAssert = matchedTokenRange;
                subActionResults = subResults;
                return matchedTokenRange.GetMatchedString();
            });
            var parser = peg.Parser(ParsingRuleNames.Start, new[]
            {
                peg.Rule(ParsingRuleNames.Start, peg.Optional(peg.Terminal(TokenNames.A)))
            });

            // Act
            var tokenizationResult = tokenizer.Tokenize(testInput);
            var parseResult = parser.Parse(tokenizationResult);

            // Assert
            Assert.True(parseResult.Succeed);
            Assert.Null(subActionResults);            
            Assert.Equal(new MatchRange(0, 0), matchedTokenRangeAssert.MatchedIndices);
            Assert.Equal(testInput, parseResult.SemanticActionResult);
        }

        [Fact]
        public void ShouldParseNonEmptyCase()
        {
            // Arrange
            string testInput = "aaaa";
            TokensMatch<TokenNames> matchedTokenRangeAssert = null;
            List<string> subActionResults = null;

            var peg = new PegFacade<TokenNames, ParsingRuleNames, string>();
            var tokenizer = peg.Tokenizer(TokenDefinitions.JustA);
            peg.SetDefaultTerminalAction(TokenNames.A, (tokens, _) => tokens.GetMatchedString());
            peg.SetDefaultOptionalAction((matchedTokenRange, subResults) =>
            {
                matchedTokenRangeAssert = matchedTokenRange;
                subActionResults = subResults.ToList();
                return matchedTokenRange.GetMatchedString();
            });
            var parser = peg.Parser(ParsingRuleNames.Start, new []
            {
                peg.Rule(ParsingRuleNames.Start, peg.Optional(peg.Terminal(TokenNames.A)))
            });

            // Act
            var tokenizationResult = tokenizer.Tokenize(testInput);
            var parseResult = parser.Parse(tokenizationResult);

            // Assert
            Assert.True(parseResult.Succeed);
            Assert.Single(subActionResults);
            Assert.Equal(testInput, subActionResults.Single());
            Assert.Equal(new MatchRange(0, 1), matchedTokenRangeAssert.MatchedIndices);
            Assert.Equal(testInput, parseResult.SemanticActionResult);
        }
    }
}
