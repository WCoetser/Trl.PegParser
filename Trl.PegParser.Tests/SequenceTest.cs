using System;
using System.Collections.Generic;
using System.Linq;
using Trl.PegParser.Grammer;
using Trl.PegParser.Tests.TestFixtures;
using Xunit;

namespace Trl.PegParser.Tests
{
    public class SequenceTest
    {
        private readonly PegFacade<TokenNames, ParsingRuleNames, string> peg = Peg.Facade();
        private TokensMatch<TokenNames> matchedTokenRangeAssert = null;
        private List<string> subActionResults = null;

        public SequenceTest()
        {
            var semanticActions = peg.DefaultSemanticActions;
            var extractValueTerminal = semanticActions.SemanticAction((matchedTokenRange, _, matchedPeg) => matchedTokenRange.GetMatchedString());
            semanticActions.SetTerminalAction(TokenNames.A, extractValueTerminal);
            semanticActions.SetTerminalAction(TokenNames.B, extractValueTerminal);
            semanticActions.SequenceAction = (matchedTokenRange, subResults, matchedPeg) =>
            {
                // Extract string result of matching the Terminal symbol
                matchedTokenRangeAssert = matchedTokenRange;
                subActionResults = subResults.ToList();
                return matchedTokenRange.GetMatchedString();
            };
        }

        [Fact]
        public void ShouldParse()
        {
            // Arrange
            var inputString = "aaabbb";
            var tokenizer = peg.Tokenizer(TokenDefinitions.AB);
            var rules = peg.ParserGenerator.GetParsingRules("ConcatenationTest => [A] [B]");
            var parser = peg.Parser(ParsingRuleNames.ConcatenationTest, rules);

            // Act
            var tokens = tokenizer.Tokenize(inputString);
            var parseResult = parser.Parse(tokens.MatchedRanges);

            // Assert
            Assert.Equal(new MatchRange(0, 2), matchedTokenRangeAssert.MatchedIndices);
            Assert.Equal(2, subActionResults.Count);
            Assert.Equal("aaa", subActionResults[0]);
            Assert.Equal("bbb", subActionResults[1]);
            Assert.True(parseResult.Succeed);
            Assert.Equal(inputString, parseResult.SemanticActionResult);
            Assert.Equal(2, parseResult.NextParseStartIndex);
        }

        [Fact]
        public void ShouldReturnNonTerminalNamesForValidationChecks()
        {
            // Arrange
            var op = peg.Operators;
            var seq = op.Sequence(op.NonTerminal(ParsingRuleNames.Head), op.NonTerminal(ParsingRuleNames.Tail));

            // Act
            var nonTerminals = seq.GetNonTerminalNames();

            // Assert
            var testSet = new HashSet<ParsingRuleNames>(new[] { ParsingRuleNames.Head, ParsingRuleNames.Tail });
            Assert.True(testSet.SetEquals(nonTerminals));
        }
    }
}
