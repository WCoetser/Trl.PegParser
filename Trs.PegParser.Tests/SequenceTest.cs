using System;
using System.Collections.Generic;
using System.Linq;
using Trs.PegParser.Grammer;
using Trs.PegParser.Tests.TestFixtures;
using Xunit;

namespace Trs.PegParser.Tests
{
    public class SequenceTest
    {
        private readonly PegFacade<TokenNames, ParsingRuleNames, string> peg;
        private TokensMatch<TokenNames> matchedTokenRangeAssert = null;
        private List<string> subActionResults = null;

        public SequenceTest()
        {
            peg = new PegFacade<TokenNames, ParsingRuleNames, string>();
            var extractValueSequence = peg.SemanticAction((matchedTokenRange, subResults) =>
            {
                // Extract string result of matching the Terminal symbol
                matchedTokenRangeAssert = matchedTokenRange;
                subActionResults = subResults.ToList();
                return matchedTokenRange.GetMatchedString();
            });
            var extractValueTerminal = peg.SemanticAction((matchedTokenRange, _) => matchedTokenRange.GetMatchedString());
            peg.SetDefaultTerminalAction(TokenNames.A, extractValueTerminal);
            peg.SetDefaultTerminalAction(TokenNames.B, extractValueTerminal);
            peg.SetDefaultSequenceAction(extractValueSequence);
        }

        [Fact]
        public void ShouldParse()
        {
            // Arrange
            var inputString = "aaabbb";
            var tokenizer = peg.Tokenizer(TokenDefinitions.AB);
            var parser = peg.Parser(ParsingRuleNames.ConcatenationTest, new[] {
                peg.Rule(ParsingRuleNames.ConcatenationTest, peg.Sequence(peg.Terminal(TokenNames.A), peg.Terminal(TokenNames.B)))
            });

            // Act
            var tokens = tokenizer.Tokenize(inputString);
            var parseResult = parser.Parse(tokens);

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
            var seq = peg.Sequence(peg.NonTerminal(ParsingRuleNames.Head), peg.NonTerminal(ParsingRuleNames.Tail));

            // Act
            var nonTerminals = seq.GetNonTerminalNames();

            // Assert
            var testSet = new HashSet<ParsingRuleNames>(new[] { ParsingRuleNames.Head, ParsingRuleNames.Tail });
            Assert.True(testSet.SetEquals(nonTerminals));
        }
    }
}
