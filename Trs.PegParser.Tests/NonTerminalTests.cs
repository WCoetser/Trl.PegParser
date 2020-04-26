using System.Collections.Generic;
using System.Linq;
using Trs.PegParser.Grammer;
using Trs.PegParser.Tests.TestFixtures;
using Xunit;

namespace Trs.PegParser.Tests
{
    public class NonTerminalTests
    {
        [Fact]
        public void ShouldParse()
        {
            // Arrange
            string inputString = "aaaa";
            var peg = new PegFacade<TokenNames, ParsingRuleNames, string>();
            TokensMatch<TokenNames> tokens;
            List<string> _subResults = null;
            peg.SetDefaultTerminalAction(TokenNames.A, (matchedTokens, _) => matchedTokens.GetMatchedString());
            peg.SetDefaultNonTerminalAction(ParsingRuleNames.NonTerminalA, (matchedTokens, subResults) =>
            {
                tokens = matchedTokens;
                _subResults = subResults.ToList();
                return subResults.First();
            });
            var tokenizer = peg.Tokenizer(TokenDefinitions.JustA);
            var parser = peg.Parser(ParsingRuleNames.Start, new[] {
                peg.Rule(ParsingRuleNames.Start, peg.NonTerminal(ParsingRuleNames.NonTerminalA)),
                peg.Rule(ParsingRuleNames.NonTerminalA, peg.Terminal(TokenNames.A))
            });

            // Act
            var inputTokens = tokenizer.Tokenize(inputString);
            var parseResult = parser.Parse(inputTokens);

            // Assert
            Assert.True(parseResult.Succeed);
            Assert.Equal(new MatchRange(0, 1), parseResult.MatchedTokens.MatchedIndices);
            Assert.Equal(inputString, parseResult.SemanticActionResult);
            Assert.Equal(inputString, _subResults.Single());
            Assert.Equal(1, parseResult.NextParsePosition);
        }

        [Fact]
        public void ShouldSubstituteRuleBodyOnParserAssign()
        {
            // Arrange
            var peg = new PegFacade<TokenNames, ParsingRuleNames, string>();
            var testNonTerminal = peg.NonTerminal(ParsingRuleNames.NonTerminalA);

            // Act
            bool before = testNonTerminal.HasNonTerminalParsingRuleBodies;
            peg.Parser(ParsingRuleNames.Start, new[] {
                peg.Rule(ParsingRuleNames.Start, testNonTerminal),
                peg.Rule(ParsingRuleNames.NonTerminalA, peg.Terminal(TokenNames.A))
            });
            bool after = testNonTerminal.HasNonTerminalParsingRuleBodies;

            // Assert
            Assert.False(before);
            Assert.True(after);
        }
    }
}
