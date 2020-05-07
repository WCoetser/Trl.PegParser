using Trs.PegParser.Tests.TestFixtures;
using Xunit;

namespace Trs.PegParser.Tests
{
    public class NotPredicateTests
    {
        private readonly PegFacade<TokenNames, ParsingRuleNames, string> peg = Peg.Facade();

        public NotPredicateTests()
        {
            var semanticActions = peg.DefaultSemanticActions;
            semanticActions.SetTerminalAction(TokenNames.A, (match, _) => match.GetMatchedString());
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
                peg.Rule(ParsingRuleNames.Start, op.Sequence(op.NotPredicate(term_B), term_A, term_B))
            });

            // Act
            var tokenizationResult = tokenizer.Tokenize(testInput);
            var parseResult = parser.Parse(tokenizationResult);

            // Assert
            Assert.True(parseResult.Succeed);
            Assert.Equal(2, parseResult.NextParseStartIndex);
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
                peg.Rule(ParsingRuleNames.Start, op.Sequence(op.NotPredicate(term_A), term_A, term_B))
            });

            // Act
            var tokenizationResult = tokenizer.Tokenize(testInput);
            var parseResult = parser.Parse(tokenizationResult);

            // Assert
            Assert.False(parseResult.Succeed);            
        }
    }
}
