using FluentAssertions;
using FluidPDF.Fluid;
using System.Text.Json.Nodes;

namespace FluidPDF.Tests
{
    public class FluidModelTests
    {
        [Fact]
        public void FromObject_ShouldSetTypeToObject_WhenCreatedWithAnObject()
        {
            // Arrange
            object subject = new { Name = "Alice" };

            // Act
            FluidModel model = FluidModel.FromObject("Model", subject);

            // Assert
            model.Type.Should().Be(FluidModelType.Object);
            model.IsObject.Should().BeTrue();
            model.Name.Should().Be("Model");
        }

        [Fact]
        public void FromJsonString_ShouldSetTypeToJsonString_WhenCreatedWithAJsonString()
        {
            // Arrange
            string subject = """{"Name":"Bob"}""";

            // Act
            FluidModel model = FluidModel.FromJsonString("Model", subject);

            // Assert
            model.Type.Should().Be(FluidModelType.JsonString);
            model.IsJsonString.Should().BeTrue();
            model.JsonString.Should().Be(subject);
        }

        [Fact]
        public void FromDictionary_ShouldSetTypeToDictionary_WhenCreatedWithADictionary()
        {
            // Arrange
            Dictionary<string, object> subject = new() { { "Name", "Carol" } };

            // Act
            FluidModel model = FluidModel.FromDictionary("Model", subject);

            // Assert
            model.Type.Should().Be(FluidModelType.Dictionary);
            model.IsDictionary.Should().BeTrue();
            model.Dictionary.Should().BeSameAs(subject);
        }

        [Fact]
        public void FromObject_ShouldExposeNonNullValue_WhenObjectModelIsResolved()
        {
            // Arrange
            FluidModel model = FluidModel.FromObject("Model", new { Name = "Dave" });

            // Act
            object? value = model.Value;

            // Assert
            value.Should().NotBeNull();
            value.Should().BeAssignableTo<JsonNode>();
        }
    }
}
