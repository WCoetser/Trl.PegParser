using System;
using System.Collections.Generic;
using System.Linq;
using Trl.PegParser.Grammer;
using Trl.PegParser.Tests.TestFixtures;
using Xunit;

namespace Trl.PegParser.Tests
{
    public class ZeroOrMoreTests
    {
        private readonly PegFacade<TokenNames, ParsingRuleNames, string> peg = Peg.Facade();
        private IEnumerable<string> subActionResults = null;
        private TokensMatch<TokenNames> matchedTokens = null;

        public ZeroOrMoreTests()
        {
            var semanticActions = peg.DefaultSemanticActions;
            semanticActions.ZeroOrMoreAction = (tokensMatch, subResults, matchedPeg) =>
            {
                matchedTokens = tokensMatch;
                subActionResults = subResults;
                return tokensMatch.GetMatchedString();
            };
            semanticActions.SetTerminalAction(TokenNames.A, (match, _, matchedPeg) => match.GetMatchedString());
            semanticActions.SetTerminalAction(TokenNames.B, (match, _, matchedPeg) => match.GetMatchedString());
            semanticActions.OrderedChoiceAction = (_, subResults, matchedPeg) => subResults.Single();
        }

        [Fact]
        public void ShouldParseRepitition()
        {
            // Arrange
            var inputString = "aaabbbaaa";
            var tokenizer = peg.Tokenizer(TokenDefinitions.AB);
            var rules = peg.ParserGenerator.GetParsingRules("Start => ([A] | [B])*");
            var parser = peg.Parser(ParsingRuleNames.Start, rules);

            // Act
            var tokens = tokenizer.Tokenize(inputString);
            var parseResult = parser.Parse(tokens.MatchedRanges);

            // Assert
            Assert.True(parseResult.Succeed);
            Assert.Equal(3, parseResult.NextParseStartIndex);
            Assert.Equal(inputString, parseResult.SemanticActionResult);
            Assert.Equal(new MatchRange(0, 3), matchedTokens.MatchedIndices);
            var subResultList = subActionResults.ToList();
            Assert.Equal(3, subResultList.Count);
            Assert.Equal("aaa", subResultList[0]);
            Assert.Equal("bbb", subResultList[1]);
            Assert.Equal("aaa", subResultList[2]);
        }

        [Fact]
        public void ShouldParseEmptyCase() {
            // Arrange
            var inputString = string.Empty;
            var tokenizer = peg.Tokenizer(TokenDefinitions.AB);
            var rules = peg.ParserGenerator.GetParsingRules("Start => ([A] | [B])*");
            var parser = peg.Parser(ParsingRuleNames.Start, rules);

            // Act
            var tokens = tokenizer.Tokenize(inputString);
            var parseResult = parser.Parse(tokens.MatchedRanges);

            // Assert
            Assert.True(parseResult.Succeed);
            Assert.Equal(0, parseResult.NextParseStartIndex);
            Assert.Equal(inputString, parseResult.SemanticActionResult);
            Assert.Equal(new MatchRange(0, 0), matchedTokens.MatchedIndices);
            Assert.Empty(subActionResults);
        }

        [Fact]
        public void ShouldAvoidNonTermination()
        {
            // Repetition on the empty string operator could cause non-termination if not covered
            var inputString = string.Empty;
            var tokenizer = peg.Tokenizer(TokenDefinitions.AB);
            var rules = peg.ParserGenerator.GetParsingRules("Start => []*");
            var parser = peg.Parser(ParsingRuleNames.Start, rules);

            // Act
            var tokens = tokenizer.Tokenize(inputString);
            var parseResult = parser.Parse(tokens.MatchedRanges);

            // Assert
            Assert.True(parseResult.Succeed);
            Assert.Equal(0, parseResult.NextParseStartIndex);
            Assert.Equal(inputString, parseResult.SemanticActionResult);
            Assert.Equal(new MatchRange(0, 0), matchedTokens.MatchedIndices);
            var subResultList = subActionResults.ToList();
            Assert.Single(subResultList);
            Assert.Null(subResultList[0]);
        }
    }
}
