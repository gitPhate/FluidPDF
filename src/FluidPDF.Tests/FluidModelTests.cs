using FluentAssertions;
using FluidPDF.Templating;
using System.Text.Json.Nodes;

namespace FluidPDF.Tests
{
    public class FluidPDFTemplateModelTests
    {
        [Fact]
        public void FromObject_ShouldSetTypeToObject_WhenCreatedWithAnObject()
        {
            // Arrange
            object subject = new { Name = "Alice" };

            // Act
            FluidPDFTemplateModel model = FluidPDFTemplateModel.FromObject("Model", subject);

            // Assert
            model.Type.Should().Be(FluidPDFTemplateModelType.Object);
            model.IsObject.Should().BeTrue();
            model.Name.Should().Be("Model");
        }

        [Fact]
        public void FromJsonString_ShouldSetTypeToJsonString_WhenCreatedWithAJsonString()
        {
            // Arrange
            string subject = """{"Name":"Bob"}""";

            // Act
            FluidPDFTemplateModel model = FluidPDFTemplateModel.FromJsonString("Model", subject);

            // Assert
            model.Type.Should().Be(FluidPDFTemplateModelType.JsonString);
            model.IsJsonString.Should().BeTrue();
            model.JsonString.Should().Be(subject);
        }

        [Fact]
        public void FromDictionary_ShouldSetTypeToDictionary_WhenCreatedWithADictionary()
        {
            // Arrange
            Dictionary<string, object> subject = new() { { "Name", "Carol" } };

            // Act
            FluidPDFTemplateModel model = FluidPDFTemplateModel.FromDictionary("Model", subject);

            // Assert
            model.Type.Should().Be(FluidPDFTemplateModelType.Dictionary);
            model.IsDictionary.Should().BeTrue();
            model.Dictionary.Should().BeSameAs(subject);
        }

        [Fact]
        public void FromObject_ShouldExposeNonNullValue_WhenObjectModelIsResolved()
        {
            // Arrange
            FluidPDFTemplateModel model = FluidPDFTemplateModel.FromObject("Model", new { Name = "Dave" });

            // Act
            object? value = model.Value;

            // Assert
            value.Should().NotBeNull();
            value.Should().BeAssignableTo<JsonNode>();
        }
    }
}
