using FluentAssertions;
using FluidPDF.Templating;
using System.Data;
using System.Text.Json;
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
            FluidPDFTemplateModel model = FluidPDFTemplateModel.FromObject(subject);

            // Assert
            model.Type.Should().Be(FluidPDFTemplateModelType.Object);
            model.IsObject.Should().BeTrue();
            model.Name.Should().Be(ModelNames.DefaultModelName);
        }

        [Fact]
        public void FromJsonString_ShouldSetTypeToJsonNode_WhenCreatedWithAJsonString()
        {
            // Arrange
            string subject = """{"Name":"Bob"}""";

            // Act
            FluidPDFTemplateModel model = FluidPDFTemplateModel.FromJsonString(subject);

            // Assert
            model.Type.Should().Be(FluidPDFTemplateModelType.JsonNode);
            model.IsJsonNode.Should().BeTrue();
            model.JsonNode.Should().NotBeNull();
        }

        [Fact]
        public void FromJsonString_ShouldParseJsonArray_WhenCreatedWithAJsonArray()
        {
            // Arrange
            string subject = """[{"Name":"Alice"},{"Name":"Bob"}]""";

            // Act
            FluidPDFTemplateModel model = FluidPDFTemplateModel.FromJsonString(subject);

            // Assert
            model.Type.Should().Be(FluidPDFTemplateModelType.JsonNode);
            model.IsJsonNode.Should().BeTrue();
            model.JsonNode.Should().BeOfType<JsonArray>();
            model.JsonNode!.AsArray().Should().HaveCount(2);
        }

        [Fact]
        public void FromDictionary_ShouldSetTypeToDictionary_WhenCreatedWithADictionary()
        {
            // Arrange
            Dictionary<string, object?> subject = new() { { "Name", "Carol" } };

            // Act
            FluidPDFTemplateModel model = FluidPDFTemplateModel.FromDictionary(subject);

            // Assert
            model.Type.Should().Be(FluidPDFTemplateModelType.Dictionary);
            model.IsDictionary.Should().BeTrue();
            model.Dictionary.Should().BeSameAs(subject);
        }

        [Fact]
        public void FromDataRow_ShouldSetTypeToDataRow_WhenCreatedWithADataRow()
        {
            // Arrange
            DataTable table = new();
            table.Columns.Add("Name", typeof(string));
            DataRow subject = table.NewRow();
            subject["Name"] = "Eve";
            table.Rows.Add(subject);

            // Act
            FluidPDFTemplateModel model = FluidPDFTemplateModel.FromDataRow(subject, "Row");

            // Assert
            model.Type.Should().Be(FluidPDFTemplateModelType.DataRow);
            model.IsDataRow.Should().BeTrue();
            model.Name.Should().Be("Row");
            model.DataRow.Should().BeSameAs(subject);
        }

        [Fact]
        public void FromDataTable_ShouldSetTypeToDataTable_WhenCreatedWithADataTable()
        {
            // Arrange
            DataTable subject = new();
            subject.Columns.Add("Name", typeof(string));

            // Act
            FluidPDFTemplateModel model = FluidPDFTemplateModel.FromDataTable(subject, "Table");

            // Assert
            model.Type.Should().Be(FluidPDFTemplateModelType.DataTable);
            model.IsDataTable.Should().BeTrue();
            model.Name.Should().Be("Table");
            model.DataTable.Should().BeSameAs(subject);
        }

        [Fact]
        public void FromPlainValue_ShouldSetTypeToPlainValue_WhenCreatedWithAString()
        {
            // Act
            FluidPDFTemplateModel model = FluidPDFTemplateModel.FromPlainValue("Hello", "Greeting");

            // Assert
            model.Type.Should().Be(FluidPDFTemplateModelType.PlainValue);
            model.IsPlainValue.Should().BeTrue();
            model.Name.Should().Be("Greeting");
            model.PlainValue.Should().Be("Hello");
        }

        [Fact]
        public void FromPlainValue_ShouldThrowArgumentNullException_WhenNullIsPassedAsValue()
        {
            // Act
            Action act = () => FluidPDFTemplateModel.FromPlainValue(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("value");
        }

        [Fact]
        public void FromObject_ShouldExposeNonNullObjectValue_WhenObjectModelIsCreated()
        {
            // Arrange
            object subject = new { Name = "Dave" };
            FluidPDFTemplateModel model = FluidPDFTemplateModel.FromObject(subject);

            // Assert
            model.ObjectValue.Should().NotBeNull();
            model.ObjectValue.Should().BeSameAs(subject);
        }

        [Fact]
        public void FromArray_ShouldSerializeToJsonArray_WhenCreatedWithAnArray()
        {
            // Arrange
            List<object?> subject = [1, 2, 3];

            // Act
            FluidPDFTemplateModel model = FluidPDFTemplateModel.FromArray(subject);
            string expectedJson = JsonSerializer.Serialize(subject);

            // Assert
            model.Type.Should().Be(FluidPDFTemplateModelType.JsonNode);
            model.JsonNode.Should().NotBeNull();
            model.JsonNode.Should().BeOfType<JsonArray>();
            model.JsonNode!.AsArray().Should().HaveCount(3);
            model.JsonNode!.ToJsonString().Should().Be(expectedJson);
        }

        [Fact]
        public void FromJsonNode_ShouldSetTypeToJsonNode_WhenCreatedWithAJsonNode()
        {
            // Arrange
            JsonNode? subject = JsonNode.Parse("""{"Name":"Bob"}""");

            // Act
            FluidPDFTemplateModel model = FluidPDFTemplateModel.FromJsonNode(subject!);

            // Assert
            model.Type.Should().Be(FluidPDFTemplateModelType.JsonNode);
            model.IsJsonNode.Should().BeTrue();
            model.JsonNode.Should().NotBeNull();
        }

        [Fact]
        public void FromArray_ShouldThrowArgumentNullException_WhenNullIsPassedAsArray()
        {
            // Act
            Action act = () => FluidPDFTemplateModel.FromArray(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void FromDataRow_ShouldThrowArgumentNullException_WhenNullIsPassedAsDataRow()
        {
            // Act
            Action act = () => FluidPDFTemplateModel.FromDataRow(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void FromDataTable_ShouldThrowArgumentNullException_WhenNullIsPassedAsDataTable()
        {
            // Act
            Action act = () => FluidPDFTemplateModel.FromDataTable(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void FromObject_ShouldThrowArgumentNullException_WhenNullIsPassedAsObject()
        {
            // Act
            Action act = () => FluidPDFTemplateModel.FromObject(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void FromJsonString_ShouldThrowArgumentNullException_WhenNullIsPassedAsJsonString()
        {
            // Act
            Action act = () => FluidPDFTemplateModel.FromJsonString(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void FromDictionary_ShouldThrowArgumentNullException_WhenNullIsPassedAsDictionary()
        {
            // Act
            Action act = () => FluidPDFTemplateModel.FromDictionary(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
