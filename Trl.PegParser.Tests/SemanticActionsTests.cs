using System;
using System.Linq;
using Trl.PegParser.Grammer;
using Trl.PegParser.Tests.TestFixtures;
using Xunit;

namespace Trl.PegParser.Tests
{
    public class SemanticActionsTests
    {
        private readonly PegFacade<TokenNames, ParsingRuleNames, IAstResult> _peg;

        public SemanticActionsTests()
        {
            _peg = new PegFacade<TokenNames, ParsingRuleNames, IAstResult>();
        }

        [Fact]
        public void ShouldReturnMarchingSubExpressionPEGOnSemanticAction()
        {
            // Arrange
            string input = "aaa";
            var tokenizer = _peg.Tokenizer(TokenDefinitions.AB);
            string matchedSpec = null;
            _peg.DefaultSemanticActions.SetDefaultGenericPassthroughAction<GenericPassthroughAst>();
            _peg.DefaultSemanticActions.OrderedChoiceAction = (_, subResults, matchedPeg) =>
            {
                matchedSpec = matchedPeg;
                return subResults.First();
            };
            var rules = _peg.ParserGenerator.GetParsingRules("Start => [A] | [B]");
            var parser = _peg.Parser(ParsingRuleNames.Start, rules);

            // Act
            var tokens = tokenizer.Tokenize(input);
            var parseResult = parser.Parse(tokens.MatchedRanges);

            // Assert
            Assert.Equal("([A] | [B])", matchedSpec);
            GenericPassthroughAst results = (GenericPassthroughAst)parseResult.SemanticActionResult;
            Assert.Equal("Start", results.MatchedPeg);
            results = (GenericPassthroughAst)results.SubResults.First();
            Assert.Equal("[A]", results.MatchedPeg);
        }
    }
}
