using Xunit;
using Trs.PegParser.Tests.TestFixtures;
using Trs.PegParser.Tokenization;
using System;

namespace Trs.PegParser.Tests
{
    public class TokenizerTests
    {
        [Fact]
        public void ShouldFailOnEmptyTokenDefinitions()
        {            
            Assert.Throws<ArgumentException>(() => {
                var tokenizer = new Tokenizer<TokenNames>(TokenDefinitions.Empty);
            });
        }

        [Fact]
        public void ShouldMatchEmptyStringIfExpected()
        {
            // Arrange
            var tokenizer = new Tokenizer<TokenNames>(TokenDefinitions.MatchEmptyString);

            // Act
            var tokenizationResult = tokenizer.Tokenize(string.Empty);

            // Assert
            Assert.True(tokenizationResult.Succeed);
            Assert.Equal(1, tokenizationResult.MatchedRanges.Count);
            Assert.Equal(TokenNames.Empty, tokenizationResult.MatchedRanges[0].TokenName);
            Assert.Equal(new MatchRange(0, 0), tokenizationResult.MatchedRanges[0].MatchedCharacterRange);
        }

        [Fact]
        public void ShouldMatchString()
        {
            // Arrange
            var tokenizer = new Tokenizer<TokenNames>(TokenDefinitions.AB);

            // Act
            var tokenizationResult = tokenizer.Tokenize("ABBAAB");

            // Assert
            Assert.True(tokenizationResult.Succeed);
            Assert.Equal(4, tokenizationResult.MatchedRanges.Count);

            Assert.Equal(TokenNames.A, tokenizationResult.MatchedRanges[0].TokenName);
            Assert.Equal(new MatchRange(0, 1), tokenizationResult.MatchedRanges[0].MatchedCharacterRange);

            Assert.Equal(TokenNames.B, tokenizationResult.MatchedRanges[1].TokenName);
            Assert.Equal(new MatchRange(1, 2), tokenizationResult.MatchedRanges[1].MatchedCharacterRange);

            Assert.Equal(TokenNames.A, tokenizationResult.MatchedRanges[2].TokenName);
            Assert.Equal(new MatchRange(3, 2), tokenizationResult.MatchedRanges[2].MatchedCharacterRange);

            Assert.Equal(TokenNames.B, tokenizationResult.MatchedRanges[3].TokenName);
            Assert.Equal(new MatchRange(5, 1), tokenizationResult.MatchedRanges[3].MatchedCharacterRange);
        }

        [Fact]
        public void ShouldCreateUnmatchedRanges()
        {
            // Arrange
            var tokenizer = new Tokenizer<TokenNames>(TokenDefinitions.JustA);

            // Act
            var tokenizationResult = tokenizer.Tokenize("BABBAAB");

            // Assert
            Assert.False(tokenizationResult.Succeed);
            Assert.Equal(2, tokenizationResult.MatchedRanges.Count);

            Assert.Equal(TokenNames.A, tokenizationResult.MatchedRanges[0].TokenName);
            Assert.Equal(new MatchRange(1, 1), tokenizationResult.MatchedRanges[0].MatchedCharacterRange);

            Assert.Equal(TokenNames.A, tokenizationResult.MatchedRanges[1].TokenName);
            Assert.Equal(new MatchRange(4, 2), tokenizationResult.MatchedRanges[1].MatchedCharacterRange);

            Assert.Equal(3, tokenizationResult.UnmatchedRanges.Count);
            Assert.Equal(new MatchRange(0, 1), tokenizationResult.UnmatchedRanges[0]);
            Assert.Equal(new MatchRange(2, 2), tokenizationResult.UnmatchedRanges[1]);
            Assert.Equal(new MatchRange(6, 1), tokenizationResult.UnmatchedRanges[2]);
        }

        [Fact]
        public void ShouldCreateUnmatchedRangesIfNothingMatched()
        {
            // Arrange
            var tokenizer = new Tokenizer<TokenNames>(TokenDefinitions.JustA);

            // Act
            var tokenizationResult = tokenizer.Tokenize("CCCC");

            // Assert
            Assert.False(tokenizationResult.Succeed);
            Assert.Equal(0, tokenizationResult.MatchedRanges.Count);
            Assert.Equal(1, tokenizationResult.UnmatchedRanges.Count);
            Assert.Equal(new MatchRange(0, 4), tokenizationResult.UnmatchedRanges[0]);
        }
    }
}
