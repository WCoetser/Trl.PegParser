using System;
using System.Collections.Generic;
using System.Linq;
using Trs.PegParser.Grammer;
using Trs.PegParser.Tests.TestFixtures;
using Xunit;

namespace Trs.PegParser.Tests
{
    public class NonTerminalTests
    {
        private readonly PegFacade<TokenNames, ParsingRuleNames, string> peg = Peg.Facade();
        TokensMatch<TokenNames> tokens;
        List<string> _subResults = null;

        public NonTerminalTests()
        {
            var semanticActions = peg.DefaultSemanticActions;
            semanticActions.SetTerminalAction(TokenNames.A, (matchedTokens, _) => matchedTokens.GetMatchedString());
            semanticActions.SetNonTerminalAction(ParsingRuleNames.NonTerminalA, (matchedTokens, subResults) =>
            {
                this.tokens = matchedTokens;
                _subResults = subResults.ToList();
                return subResults.First();
            });
        }

        [Fact]
        public void ShouldParse()
        {
            // Arrange
            string inputString = "aaaa";
            var op = peg.Operators;
            var tokenizer = peg.Tokenizer(TokenDefinitions.JustA);
            var parser = peg.Parser(ParsingRuleNames.Start, new[] {
                peg.Rule(ParsingRuleNames.Start, op.NonTerminal(ParsingRuleNames.NonTerminalA)),
                peg.Rule(ParsingRuleNames.NonTerminalA, op.Terminal(TokenNames.A))
            });

            // Act
            var inputTokens = tokenizer.Tokenize(inputString);
            var parseResult = parser.Parse(inputTokens);

            // Assert
            Assert.True(parseResult.Succeed);
            Assert.Equal(new MatchRange(0, 1), parseResult.MatchedTokens.MatchedIndices);
            Assert.Equal(inputString, parseResult.SemanticActionResult);
            Assert.Equal(inputString, _subResults.Single());
            Assert.Equal(1, parseResult.NextParseStartIndex);
        }

        [Fact]
        public void ShouldSubstituteRuleBodyOnParserAssign()
        {
            // Arrange
            var op = peg.Operators;
            var testNonTerminal = op.NonTerminal(ParsingRuleNames.NonTerminalA);

            // Act
            bool before = ((IParsingOperatorExecution<TokenNames, ParsingRuleNames, string>)testNonTerminal).HasNonTerminalParsingRuleBodies;
            peg.Parser(ParsingRuleNames.Start, new[] {
                peg.Rule(ParsingRuleNames.Start, testNonTerminal),
                peg.Rule(ParsingRuleNames.NonTerminalA, op.Terminal(TokenNames.A))
            });
            bool after = ((IParsingOperatorExecution<TokenNames, ParsingRuleNames, string>)testNonTerminal).HasNonTerminalParsingRuleBodies;

            // Assert
            Assert.False(before);
            Assert.True(after);
        }

        [Fact]
        public void PredicateMustNotConsumeTokens()
        {
            throw new NotImplementedException();
        }
    }
}
