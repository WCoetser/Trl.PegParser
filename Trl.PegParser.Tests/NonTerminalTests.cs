using System.Collections.Generic;
using System.Linq;
using Trl.PegParser.Grammer;
using Trl.PegParser.Tests.TestFixtures;
using Xunit;

namespace Trl.PegParser.Tests
{
    public class NonTerminalTests
    {
        private readonly PegFacade<TokenNames, ParsingRuleNames, string> peg = Peg.Facade();
        List<string> _subResults = null;

        public NonTerminalTests()
        {
            var semanticActions = peg.DefaultSemanticActions;
            semanticActions.SetTerminalAction(TokenNames.A, (matchedTokens, _) => matchedTokens.GetMatchedString());
            semanticActions.SetNonTerminalAction(ParsingRuleNames.NonTerminalA, (matchedTokens, subResults) =>
            {
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
            var rules = peg.ParserGenerator.GetParsingRules("Start => NonTerminalA; NonTerminalA => [A];");
            var parser = peg.Parser(ParsingRuleNames.Start, rules);

            // Act
            var inputTokens = tokenizer.Tokenize(inputString);
            var parseResult = parser.Parse(inputTokens.MatchedRanges);

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
            bool before = testNonTerminal.HasNonTerminalParsingRuleBodies;
            peg.Parser(ParsingRuleNames.Start, new[] {
                peg.Rule(ParsingRuleNames.Start, testNonTerminal),
                peg.Rule(ParsingRuleNames.NonTerminalA, op.Terminal(TokenNames.A))
            });
            bool after = testNonTerminal.HasNonTerminalParsingRuleBodies;

            // Assert
            Assert.False(before);
            Assert.True(after);
        }

        [Fact]
        public void ShouldAvoidNonTerminationOnLeftRecursion()
        {
            // Arrange
            string inputString = string.Empty;
            var op = peg.Operators;
            var tokenizer = peg.Tokenizer(TokenDefinitions.JustA);
            // Create a parser with a cycle due to recursive nonterminal definitions
            var rules = peg.ParserGenerator.GetParsingRules("Start => NonTerminalA; NonTerminalA => NonTerminalA; NonTerminalB => Start;");
            var parser = peg.Parser(ParsingRuleNames.Start, rules);

            // Act
            var inputTokens = tokenizer.Tokenize(inputString);
            var parseResult = parser.Parse(inputTokens.MatchedRanges);

            // Assert
            Assert.False(parseResult.Succeed); // if it got this far it terminated.
        }
    }
}
