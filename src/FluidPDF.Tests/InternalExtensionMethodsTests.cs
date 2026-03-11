using FluentAssertions;
using FluidPDF.Support;

namespace FluidPDF.Tests
{
    public class InternalExtensionMethodsTests
    {
        [Fact]
        public void GetNonNullOrThrow_ShouldReturnValue_WhenReferenceTypeIsNotNull()
        {
            // Arrange
            string? value = "hello";

            // Act
            string result = value.GetNonNullOrThrow();

            // Assert
            result.Should().Be("hello");
        }

        [Fact]
        public void GetNonNullOrThrow_ShouldReturnUnwrappedValue_WhenStructIsNotNull()
        {
            // Arrange
            int? value = 42;

            // Act
            int result = value.GetNonNullOrThrow();

            // Assert
            result.Should().Be(42);
        }

        [Fact]
        public void GetNonNullOrThrow_ShouldThrowArgumentNullException_WithCallerArgumentName_WhenReferenceTypeIsNull()
        {
            // Arrange
            string? value = null;

            // Act
            Action act = () => value.GetNonNullOrThrow();

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("value");
        }

        [Fact]
        public void GetNonNullOrThrow_ShouldThrowArgumentNullException_WithCallerArgumentName_WhenStructIsNull()
        {
            // Arrange
            int? value = null;

            // Act
            Action act = () => value.GetNonNullOrThrow();

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("value");
        }
    }
}
