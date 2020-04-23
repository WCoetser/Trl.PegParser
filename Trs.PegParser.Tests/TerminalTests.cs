using System;
using System.Collections.Generic;
using Trs.PegParser.Tests.TestFixtures;
using Xunit;

namespace Trs.PegParser.Tests
{
    public class TerminalTests
    {
        [Fact]
        public void ShouldParse()
        {
            var inputString = "aaa";
            
            // Arrange ... Store values for assert
            IEnumerable<string> subActionResults = Array.Empty<string>();

            // Arrange ... Set up parser
            var peg = new PegFacade<TokenNames, ParsingRuleNames, string>();
            var tokenizer = peg.Tokenizer(TokenDefinitions.JustA);
            var extractValue = peg.SemanticAction((tokensMatch, subResults) =>
            {
                // Extract string result of matching the Terminal symbol
                subActionResults = subResults;
                return tokensMatch.GetMatchedString();
            });
            var parser = peg.Parser(ParsingRuleNames.TerminalTest, new[] { 
                peg.Rule(ParsingRuleNames.TerminalTest, peg.Terminal(TokenNames.A, extractValue)),
            });

            // Act
            var tokensResult = tokenizer.Tokenize(inputString);
            if (!tokensResult.Succeed) throw new Exception();
            var parseResult = parser.Parse(tokensResult);

            // Assert
            Assert.Equal(new MatchRange(0, 1), parseResult.MatchedRange);
            Assert.Null(subActionResults);
            Assert.True(parseResult.Succeed);
            Assert.Equal(inputString, parseResult.SemanticActionResult);
            Assert.Equal(1, parseResult.NextParsePosition);
        }

        [Fact]
        public void ShouldNotReturnNonTerminalNamesForValidationChecks()
        {
            // Arrange
            var peg = new PegFacade<TokenNames, ParsingRuleNames, string>();
            var terminal = peg.Terminal(TokenNames.A);

            // Act
            var nonTerminalNames = terminal.GetNonTerminalNames();

            // Assert
            Assert.Empty(nonTerminalNames);
        }
    }
}
