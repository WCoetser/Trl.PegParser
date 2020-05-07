using System;
using System.Collections.Generic;
using System.Linq;
using Trs.PegParser.Grammer;
using Trs.PegParser.Tests.TestFixtures;
using Xunit;

namespace Trs.PegParser.Tests
{
    public class ZeroOrMoreTests
    {
        private readonly PegFacade<TokenNames, ParsingRuleNames, string> peg = Peg.Facade();
        private IEnumerable<string> subActionResults = null;
        private TokensMatch<TokenNames> matchedTokens = null;

        public ZeroOrMoreTests()
        {
            var semanticActions = peg.DefaultSemanticActions;
            peg = new PegFacade<TokenNames, ParsingRuleNames, string>();
            semanticActions.ZeroOrMoreAction = (tokensMatch, subResults) =>
            {
                matchedTokens = tokensMatch;
                subActionResults = subResults;
                return tokensMatch.GetMatchedString();
            };
            semanticActions.SetTerminalAction(TokenNames.A, (match, _) => match.GetMatchedString());
            semanticActions.SetTerminalAction(TokenNames.B, (match, _) => match.GetMatchedString());
            semanticActions.OrderedChoiceAction = (_, subResults) => subResults.Single();
        }

        [Fact]
        public void ShouldParseRepitition()
        {
            // Arrange
            var inputString = "aaabbbaaa";
            var tokenizer = peg.Tokenizer(TokenDefinitions.AB);
            var op = peg.Operators;
            var parser = peg.Parser(ParsingRuleNames.Start, new[] {
                peg.Rule(ParsingRuleNames.Start, op.ZeroOrMore(op.OrderedChoice(op.Terminal(TokenNames.A), op.Terminal(TokenNames.B))))
            });

            // Act
            var tokens = tokenizer.Tokenize(inputString);
            var parseResult = parser.Parse(tokens);

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
            var op = peg.Operators;
            var parser = peg.Parser(ParsingRuleNames.Start, new[] {
                peg.Rule(ParsingRuleNames.Start, op.ZeroOrMore(op.OrderedChoice(op.Terminal(TokenNames.A), op.Terminal(TokenNames.B))))
            });

            // Act
            var tokens = tokenizer.Tokenize(inputString);
            var parseResult = parser.Parse(tokens);

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
            var op = peg.Operators;
            var parser = peg.Parser(ParsingRuleNames.Start, new[] {
                peg.Rule(ParsingRuleNames.Start, op.ZeroOrMore(op.EmptyString()))
            });

            // Act
            var tokens = tokenizer.Tokenize(inputString);
            var parseResult = parser.Parse(tokens);

            // Assert
            Assert.True(parseResult.Succeed);
            Assert.Equal(0, parseResult.NextParseStartIndex);
            Assert.Equal(inputString, parseResult.SemanticActionResult);
            Assert.Equal(new MatchRange(0, 0), matchedTokens.MatchedIndices);
            var subResultList = subActionResults.ToList();
            Assert.Single(subResultList);
            Assert.Null(subResultList[0]);
        }

        [Fact]
        public void PredicateMustNotConsumeTokens()
        {
            throw new NotImplementedException();
        }
    }
}
