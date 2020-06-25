using System;
using System.Linq;
using Trl.PegParser.Grammer;
using Trl.PegParser.Tests.TestFixtures;
using Xunit;

namespace Trl.PegParser.Tests
{
    public class SemanticActionsTests
    {
        private readonly PegFacade<TokenNames, ParsingRuleNames, string> peg = Peg.Facade();

        [Fact]
        public void ShouldReturnMarchingSubExpressionPEGOnSemanticAction()
        {
            // Arrange
            string input = "aaa";
            var tokenizer = peg.Tokenizer(TokenDefinitions.AB);
            string matchedSpec = null;
            peg.DefaultSemanticActions.OrderedChoiceAction = (_, subResults, matchedPeg) =>
            {
                matchedSpec = matchedPeg;
                return null;
            };
            var rules = peg.ParserGenerator.GetParsingRules("Start => [A] | [B]");
            var parser = peg.Parser(ParsingRuleNames.Start, rules);

            // Act
            var tokens = tokenizer.Tokenize(input);
            var parseResult = parser.Parse(tokens.MatchedRanges);

            // Assert
            Assert.Equal("([A] | [B])", matchedSpec);
        }
    }
}
