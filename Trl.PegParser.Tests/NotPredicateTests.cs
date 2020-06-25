using System.Linq;
using Trl.PegParser.Tests.TestFixtures;
using Xunit;

namespace Trl.PegParser.Tests
{
    public class NotPredicateTests
    {
        private readonly PegFacade<TokenNames, ParsingRuleNames, string> peg = Peg.Facade();

        public NotPredicateTests()
        {
            var semanticActions = peg.DefaultSemanticActions;
            semanticActions.SetTerminalAction(TokenNames.A, (match, _, matchedPeg) => match.GetMatchedString());
        }

        [Fact]
        public void PredicateMustSucceed()
        {
            // Arrange
            string testInput = "aaaabbbb";
            var tokenizer = peg.Tokenizer(TokenDefinitions.AB);
            var rules = peg.ParserGenerator.GetParsingRules("Start => !([B]) [A] [B]");
            var parser = peg.Parser(ParsingRuleNames.Start, rules);

            // Act
            var tokenizationResult = tokenizer.Tokenize(testInput);
            var parseResult = parser.Parse(tokenizationResult.MatchedRanges);

            // Assert
            Assert.True(parseResult.Succeed);
            Assert.Equal(2, parseResult.NextParseStartIndex);
        }

        [Fact]
        public void PredicateMustFail()
        {
            // Arrange
            string testInput = "aaaabbbb";
            var tokenizer = peg.Tokenizer(TokenDefinitions.AB);
            var rules = peg.ParserGenerator.GetParsingRules("Start => !([A]) [A] [B]");
            var parser = peg.Parser(ParsingRuleNames.Start, rules);

            // Act
            var tokenizationResult = tokenizer.Tokenize(testInput);
            var parseResult = parser.Parse(tokenizationResult.MatchedRanges);

            // Assert
            Assert.False(parseResult.Succeed);
        }

        [Fact]
        public void ShouldSupportGenericPassthroughResult()
        {
            // Arrange
            var peg = Peg.GenericPassthroughTest();
            peg.DefaultSemanticActions.SetDefaultGenericPassthroughAction<GenericPassthroughAst>();
            string testInput = "aaaabbbb";            
            var tokenizer = peg.Tokenizer(TokenDefinitions.AB);
            var rules = peg.ParserGenerator.GetParsingRules("Start => !([B]) [A] [B]");
            var parser = peg.Parser(ParsingRuleNames.Start, rules);

            // Act
            var tokenizationResult = tokenizer.Tokenize(testInput);
            var parseResult = parser.Parse(tokenizationResult.MatchedRanges);

            // Assert
            var result = (GenericPassthroughAst)parseResult.SemanticActionResult;
            var subResults = result.SubResults.Cast<GenericPassthroughAst>().First().SubResults.Cast<GenericPassthroughAst>().ToList();
            Assert.Null(subResults[0]); // predicate does not consume tokens
            var seqResults = subResults[1].SubResults.Cast<GenericPassthroughAst>().ToList();
            Assert.Equal("aaaa", seqResults[0].MatchedTokens.GetMatchedString());
            Assert.Equal("bbbb", seqResults[1].MatchedTokens.GetMatchedString());
        }
    }
}
