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
            // Arrange
            var tokenizer = new Tokenizer<TokenNames>();

            // Assert && Act
            Assert.Throws<ArgumentException>(() => tokenizer.Tokenize(string.Empty, TokenDefinitions.Empty));
        }

        [Fact]
        public void ShouldMatchEmptyStringIfExpected()
        {
            // Arrange
            var tokenizer = new Tokenizer<TokenNames>();

            // Act
            var tokenizationResult = tokenizer.Tokenize(string.Empty, TokenDefinitions.MatchEmptyString);

            // Assert
            Assert.True(tokenizationResult.Succeeded);
            Assert.Equal(1, tokenizationResult.MatchedRanges.Count);
            Assert.Equal(TokenNames.Empty, tokenizationResult.MatchedRanges[0].TokenName);
            Assert.Equal(new StringMatchRange(0, 0), tokenizationResult.MatchedRanges[0].MatchedCharacterRange);
        }

        [Fact]
        public void ShouldMatchString()
        {
            // Arrange
            var tokenizer = new Tokenizer<TokenNames>();

            // Act
            var tokenizationResult = tokenizer.Tokenize("ABBAAB", TokenDefinitions.AB);

            // Assert
            Assert.True(tokenizationResult.Succeeded);
            Assert.Equal(4, tokenizationResult.MatchedRanges.Count);

            Assert.Equal(TokenNames.A, tokenizationResult.MatchedRanges[0].TokenName);
            Assert.Equal(new StringMatchRange(0, 1), tokenizationResult.MatchedRanges[0].MatchedCharacterRange);

            Assert.Equal(TokenNames.B, tokenizationResult.MatchedRanges[1].TokenName);
            Assert.Equal(new StringMatchRange(1, 2), tokenizationResult.MatchedRanges[1].MatchedCharacterRange);

            Assert.Equal(TokenNames.A, tokenizationResult.MatchedRanges[2].TokenName);
            Assert.Equal(new StringMatchRange(3, 2), tokenizationResult.MatchedRanges[2].MatchedCharacterRange);

            Assert.Equal(TokenNames.B, tokenizationResult.MatchedRanges[3].TokenName);
            Assert.Equal(new StringMatchRange(5, 1), tokenizationResult.MatchedRanges[3].MatchedCharacterRange);
        }

        [Fact]
        public void ShouldCreateUnmatchedRanges()
        {
            // Arrange
            var tokenizer = new Tokenizer<TokenNames>();

            // Act
            var tokenizationResult = tokenizer.Tokenize("BABBAAB", TokenDefinitions.JustA);

            // Assert
            Assert.False(tokenizationResult.Succeeded);
            Assert.Equal(2, tokenizationResult.MatchedRanges.Count);

            Assert.Equal(TokenNames.A, tokenizationResult.MatchedRanges[0].TokenName);
            Assert.Equal(new StringMatchRange(1, 1), tokenizationResult.MatchedRanges[0].MatchedCharacterRange);

            Assert.Equal(TokenNames.A, tokenizationResult.MatchedRanges[1].TokenName);
            Assert.Equal(new StringMatchRange(4, 2), tokenizationResult.MatchedRanges[1].MatchedCharacterRange);

            Assert.Equal(3, tokenizationResult.UnmatchedRanges.Count);
            Assert.Equal(new StringMatchRange(0, 1), tokenizationResult.UnmatchedRanges[0]);
            Assert.Equal(new StringMatchRange(2, 2), tokenizationResult.UnmatchedRanges[1]);
            Assert.Equal(new StringMatchRange(6, 1), tokenizationResult.UnmatchedRanges[2]);
        }

        [Fact]
        public void ShouldCreateUnmatchedRangesIfNothingMatched()
        {
            // Arrange
            var tokenizer = new Tokenizer<TokenNames>();

            // Act
            var tokenizationResult = tokenizer.Tokenize("CCCC", TokenDefinitions.JustA);

            // Assert
            Assert.False(tokenizationResult.Succeeded);
            Assert.Equal(0, tokenizationResult.MatchedRanges.Count);
            Assert.Equal(1, tokenizationResult.UnmatchedRanges.Count);
            Assert.Equal(new StringMatchRange(0, 4), tokenizationResult.UnmatchedRanges[0]);
        }
    }
}
