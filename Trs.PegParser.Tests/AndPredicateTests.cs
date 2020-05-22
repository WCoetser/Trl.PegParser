using System.Linq;
using Trs.PegParser.Tests.TestFixtures;
using Xunit;

namespace Trs.PegParser.Tests
{
    public class AndPredicateTests
    {
        private readonly PegFacade<TokenNames, ParsingRuleNames, string> peg = Peg.Facade();

        public AndPredicateTests()
        {
            var semanticActions = peg.DefaultSemanticActions;
            semanticActions.SetTerminalAction(TokenNames.A, (match, _) => match.GetMatchedString());
        }

        [Fact]
        public void PredicateMustFail()
        {
            // Arrange
            string testInput = "aaaabbbb";
            var op = peg.Operators;
            var tokenizer = peg.Tokenizer(TokenDefinitions.AB);
            var term_A = op.Terminal(TokenNames.A);
            var term_B = op.Terminal(TokenNames.B);
            var parser = peg.Parser(ParsingRuleNames.Start, new[]
            {
                peg.Rule(ParsingRuleNames.Start, op.Sequence(op.AndPredicate(term_B), term_A, term_B))
            });

            // Act
            var tokenizationResult = tokenizer.Tokenize(testInput);
            var parseResult = parser.Parse(tokenizationResult.MatchedRanges);

            // Assert
            Assert.False(parseResult.Succeed);            
        }


        [Fact]
        public void PredicateMustSucceed()
        {
            // Arrange
            string testInput = "aaaabbbb";
            var op = peg.Operators;
            var tokenizer = peg.Tokenizer(TokenDefinitions.AB);
            var term_A = op.Terminal(TokenNames.A);
            var term_B = op.Terminal(TokenNames.B);
            var parser = peg.Parser(ParsingRuleNames.Start, new[]
            {
                peg.Rule(ParsingRuleNames.Start, op.Sequence(op.AndPredicate(term_A), term_A, term_B))
            });

            // Act
            var tokenizationResult = tokenizer.Tokenize(testInput);
            var parseResult = parser.Parse(tokenizationResult.MatchedRanges);

            // Assert
            Assert.True(parseResult.Succeed);
            Assert.Equal(2, parseResult.NextParseStartIndex);
        }

        [Fact]
        public void ShouldSupportGenericPassthroughResult()
        {
            // Arrange
            var peg = Peg.GenericPassthroughTest();
            peg.DefaultSemanticActions.SetDefaultGenericPassthroughAction<GenericPassthroughAst>();

            string testInput = "aaaabbbb";
            var op = peg.Operators;
            var tokenizer = peg.Tokenizer(TokenDefinitions.AB);
            var term_A = op.Terminal(TokenNames.A);
            var term_B = op.Terminal(TokenNames.B);
            var parser = peg.Parser(ParsingRuleNames.Start, new[]
            {
                peg.Rule(ParsingRuleNames.Start, op.Sequence(op.AndPredicate(term_A), term_A, term_B))
            });

            // Act
            var tokenizationResult = tokenizer.Tokenize(testInput);
            var parseResult = parser.Parse(tokenizationResult.MatchedRanges);

            // Assert
            var result = (GenericPassthroughAst)parseResult.SemanticActionResult;
            var subResults = result.SubResults.Cast<GenericPassthroughAst>().ToList();
            Assert.Null(subResults[0]); // predicate does not consume tokens
            Assert.Equal("aaaa", subResults[1].MatchedTokens.GetMatchedString());
            Assert.Equal("bbbb", subResults[2].MatchedTokens.GetMatchedString());
        }
    }
}
