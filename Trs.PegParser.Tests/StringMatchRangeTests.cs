using Xunit;

namespace Trs.PegParser.Tests
{
    public class StringMatchRangeTests
    {

        [Fact]
        public void ShouldImplementNotEqualsOperator()
        {
            // Arrange
            var lhs = new StringMatchRange(1, 2);
            var rhs = new StringMatchRange(2, 3);

            // Act
            var result = lhs != rhs;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ShouldImplementHashingAndEquality()
        {
            // Arrange
            var lhs = new StringMatchRange(1, 2);
            var rhs = new StringMatchRange(1, 2);

            // Act
            var lhsHash = lhs.GetHashCode();
            var rhsHash = rhs.GetHashCode();

            // Assert
            Assert.Equal(lhsHash, rhsHash);
        }
    }
}
