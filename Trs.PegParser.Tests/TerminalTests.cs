using System;
using System.Collections.Generic;
using Trs.PegParser.Tests.TestFixtures;
using Xunit;

namespace Trs.PegParser.Tests
{
    public class TerminalTests
    {
        private readonly PegFacade<TokenNames, ParsingRuleNames, string> peg = Peg.Facade();
        private IEnumerable<string> subActionResults = Array.Empty<string>();

        public TerminalTests()
        {
            var semanticActions = peg.DefaultSemanticActions;
            var extractValue = semanticActions.SemanticAction((tokensMatch, subResults) =>
            {
                // Extract string result of matching the Terminal symbol
                subActionResults = subResults;
                return tokensMatch.GetMatchedString();
            });
            semanticActions.SetTerminalAction(TokenNames.A, extractValue);
        }

        [Fact]
        public void ShouldParse()
        {
            // Arrange
            var inputString = "aaa";
            var tokenizer = peg.Tokenizer(TokenDefinitions.JustA);
            var rules = peg.ParserGenerator.GetParsingRules("TerminalTest => [A];");
            var parser = peg.Parser(ParsingRuleNames.TerminalTest, rules);

            // Act
            var tokensResult = tokenizer.Tokenize(inputString);
            var parseResult = parser.Parse(tokensResult.MatchedRanges);

            // Assert
            Assert.Equal(new MatchRange(0, 1), parseResult.MatchedTokens.MatchedIndices);
            Assert.Empty(subActionResults);
            Assert.True(parseResult.Succeed);
            Assert.Equal(inputString, parseResult.SemanticActionResult);
            Assert.Equal(1, parseResult.NextParseStartIndex);
        }

        [Fact]
        public void ShouldNotReturnNonTerminalNamesForValidationChecks()
        {
            // Arrange
            var op = peg.Operators;
            var terminal = op.Terminal(TokenNames.A);

            // Act
            var nonTerminalNames = terminal.GetNonTerminalNames();

            // Assert
            Assert.Empty(nonTerminalNames);
        }
    }
}
