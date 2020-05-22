using System;
using System.Collections.Generic;
using System.Linq;
using Trs.PegParser.Tests.TestFixtures;
using Xunit;

namespace Trs.PegParser.Tests
{
    public class EmptyStringTests
    {
        private readonly PegFacade<TokenNames, ParsingRuleNames, string> peg = Peg.Facade();
        private IEnumerable<string> subActionResults = null;

        public EmptyStringTests()
        {
            var semanticActions = peg.DefaultSemanticActions;
            semanticActions.EmptyStringAction = (tokensMatch, subResults) =>
            {
                // Extract string result of matching the Terminal symbol
                subActionResults = subResults;
                return tokensMatch.GetMatchedString();
            };
        }

        [Fact]
        public void ShouldParseEmptyInputTokens()
        {
            // Arrange
            string inputString = string.Empty;
            var op = peg.Operators;
            var tokenizer = peg.Tokenizer(TokenDefinitions.AB);
            var parser = peg.Parser(ParsingRuleNames.Start, new[] {
                peg.Rule(ParsingRuleNames.Start, op.EmptyString()),
            });

            // Act
            var tokensResult = tokenizer.Tokenize(inputString);
            var parseResult = parser.Parse(tokensResult.MatchedRanges);

            // Assert
            Assert.Equal(new MatchRange(0, 0), parseResult.MatchedTokens.MatchedIndices);
            Assert.Empty(subActionResults);
            Assert.True(parseResult.Succeed);
            Assert.Equal(inputString, parseResult.SemanticActionResult);
            Assert.Equal(0, parseResult.NextParseStartIndex);
        }

        [Fact]
        public void ShouldSupportGenericPassthroughResult()
        {
            // Arrange
            var peg = Peg.GenericPassthroughTest();
            peg.DefaultSemanticActions.SetDefaultGenericPassthroughAction<GenericPassthroughAst>();
            string inputString = string.Empty;
            var op = peg.Operators;
            var tokenizer = peg.Tokenizer(TokenDefinitions.AB);
            var parser = peg.Parser(ParsingRuleNames.Start, new[] {
                peg.Rule(ParsingRuleNames.Start, op.EmptyString()),
            });

            // Act
            var tokensResult = tokenizer.Tokenize(inputString);
            var parseResult = parser.Parse(tokensResult.MatchedRanges);

            // Assert
            var result = (GenericPassthroughAst)parseResult.SemanticActionResult;
            var subResults = result.SubResults.Cast<GenericPassthroughAst>().ToList();
            Assert.Equal(string.Empty, result.MatchedTokens.GetMatchedString());
            Assert.Empty(subResults);
        }
    }
}
