using System;
using Trl.PegParser.Tests.TestFixtures;
using Xunit;

namespace Trl.PegParser.Tests
{
    public class ParserGeneratorTests
    {
        private readonly PegFacade<TokenNames, ParsingRuleNames, string> _peg;

        public ParserGeneratorTests()
        {
            _peg = new PegFacade<TokenNames, ParsingRuleNames, string>();
        }

        [InlineData("Start => [A] ([B] | [B])")]
        [InlineData("Start => ([A] | [B]) | [B]")]
        [InlineData("Start => &([A] [A]) [A]+")]
        [InlineData("Start => !([B] [B]) [B]*")]
        [InlineData("Start => [A]? | [B];")]
        [InlineData("Start => [A] | [B]?;")]
        [InlineData("Start => [A] | []")]
        [InlineData("Start => [A]? [B];")]
        [InlineData("Start => [A] [B]?;")]
        [InlineData("Start => [A]?")]
        [InlineData("Start => ([A] | [B])+")]
        [InlineData("Start => ([A] [B])*")]
        [InlineData("Start => [A]+")]
        [InlineData("Start => [A]*")]
        [InlineData("Start => &([A])")]
        [InlineData("Start => !([A])")]
        [InlineData("Start => [A] | NonTerminalB; NonTerminalB => [B]")]
        [InlineData("Start => [A]")]
        [InlineData("Start => []")]
        [Theory]
        public void ShouldParse(string inputGrammer)
        {
            // If no exception is thrown it suceeded
            _ = _peg.ParserGenerator.GetParsingRules(inputGrammer);
        }

        [Fact]
        public void ShouldNotHaveExponentialExecution()
        {
            // This test case takes a very long time to parse if the parser generator did not have memoization
            // on non-terminals

            string end = "[B] (([A] | [B]) | [B]) | (([A] | [B]) | [B])";
            string testCase = "Start => ";
            for (int i = 0; i < 10; i++)
            {
                testCase += end;
            }
            _ = _peg.ParserGenerator.GetParsingRules(testCase);
        }

        [Fact]
        public void ShouldThrowExceptionIfParsingFails()
        {
            Assert.Throws<Exception>(() =>
            {
                _ = _peg.ParserGenerator.GetParsingRules("[A]");
            });
        }
    }
}
