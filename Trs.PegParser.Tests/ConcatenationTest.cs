﻿using System;
using System.Collections.Generic;
using System.Linq;
using Trs.PegParser.Grammer;
using Trs.PegParser.Tests.TestFixtures;
using Xunit;

namespace Trs.PegParser.Tests
{
    public class ConcatenationTest
    {
        [Fact]
        public void ShouldParse()
        {
            var inputString = "aaabbb";

            // Arrange ... Store values for assert
            TokensMatch<TokenNames> matchedTokenRangeAssert = null;
            List<string> subActionResults = null;

            // Arrange ... Set up parser
            var peg = new PegFacade<TokenNames, ParsingRuleNames, string>();
            var tokenizer = peg.Tokenizer(TokenDefinitions.AB);
            var extractValueSequence = peg.SemanticAction((matchedTokenRange, subResults) =>
            {
                // Extract string result of matching the Terminal symbol
                matchedTokenRangeAssert = matchedTokenRange;
                subActionResults = subResults.ToList();
                return matchedTokenRange.GetMatchedString();
            });
            var extractValueTerminal = peg.SemanticAction((matchedTokenRange, _) => matchedTokenRange.GetMatchedString());
            var parser = peg.Parser(ParsingRuleNames.ConcatenationTest, new[] {
                peg.Rule(ParsingRuleNames.ConcatenationTest, peg.Sequence(new [] { peg.Terminal(TokenNames.A, extractValueTerminal), peg.Terminal(TokenNames.B, extractValueTerminal) }, extractValueSequence))
            });

            // Act
            var tokens = tokenizer.Tokenize(inputString);
            if (!tokens.Succeed) throw new Exception();
            var parseResult = parser.Parse(tokens);

            // Assert
            Assert.Equal(new MatchRange(0, 2), matchedTokenRangeAssert.MatchedTokenIndices);
            Assert.Equal(2, subActionResults.Count);
            Assert.Equal("aaa", subActionResults[0]);
            Assert.Equal("bbb", subActionResults[1]);
            Assert.True(parseResult.Succeed);
            Assert.Equal(inputString, parseResult.SemanticActionResult);
            Assert.Equal(2, parseResult.NextParsePosition);
        }

        [Fact]
        public void ShouldReturnNonTerminalNamesForValidationChecks()
        {
            throw new NotImplementedException();
        }
    }
}
