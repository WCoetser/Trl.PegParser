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
            var extractValue = peg.SemanticAction((tokensMatch, subResults) =>
            {
                // Extract string result of matching the Terminal symbol
                subActionResults = subResults;
                return tokensMatch.GetMatchedString();
            });
            peg.SetDefaultTerminalAction(TokenNames.A, extractValue);
        }

        [Fact]
        public void ShouldParse()
        {
            // Arrange
            var inputString = "aaa";
            var tokenizer = peg.Tokenizer(TokenDefinitions.JustA);
            var parser = peg.Parser(ParsingRuleNames.TerminalTest, new[] { 
                peg.Rule(ParsingRuleNames.TerminalTest, peg.Terminal(TokenNames.A)),
            });

            // Act
            var tokensResult = tokenizer.Tokenize(inputString);            
            var parseResult = parser.Parse(tokensResult);

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
            var terminal = peg.Terminal(TokenNames.A);

            // Act
            var nonTerminalNames = terminal.GetNonTerminalNames();

            // Assert
            Assert.Empty(nonTerminalNames);
        }
    }
}
