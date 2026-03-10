using FluentAssertions;
using FluidPDF.Templating;
using System.Data;
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
        public void FromDataRow_ShouldSetTypeToDataRow_WhenCreatedWithADataRow()
        {
            // Arrange
            DataTable table = new();
            table.Columns.Add("Name", typeof(string));
            DataRow subject = table.NewRow();
            subject["Name"] = "Eve";
            table.Rows.Add(subject);

            // Act
            FluidPDFTemplateModel model = FluidPDFTemplateModel.FromDataRow("Row", subject);

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
            FluidPDFTemplateModel model = FluidPDFTemplateModel.FromDataTable("Table", subject);

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
            FluidPDFTemplateModel model = FluidPDFTemplateModel.FromPlainValue("Greeting", "Hello");

            // Assert
            model.Type.Should().Be(FluidPDFTemplateModelType.PlainValue);
            model.IsPlainValue.Should().BeTrue();
            model.Name.Should().Be("Greeting");
            model.PlainValue.Should().Be("Hello");
        }

        [Fact]
        public void FromPlainValue_ShouldAllowNull_WhenNullIsPassedAsValue()
        {
            // Act
            FluidPDFTemplateModel model = FluidPDFTemplateModel.FromPlainValue("Empty", null!);

            // Assert
            model.Type.Should().Be(FluidPDFTemplateModelType.PlainValue);
            model.PlainValue.Should().BeNull();
            model.Value.Should().BeNull();
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

        [Fact]
        public void FromJsonString_ShouldExposeNonNullValue_WhenJsonStringModelIsResolved()
        {
            // Arrange
            FluidPDFTemplateModel model = FluidPDFTemplateModel.FromJsonString("Model", """{"Name":"Frank"}""");

            // Act
            object? value = model.Value;

            // Assert
            value.Should().NotBeNull();
            value.Should().BeAssignableTo<JsonNode>();
        }

        [Fact]
        public void FromDataRow_ShouldThrowArgumentNullException_WhenNullIsPassedAsDataRow()
        {
            // Act
            Action act = () => FluidPDFTemplateModel.FromDataRow("Row", null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void FromDataTable_ShouldThrowArgumentNullException_WhenNullIsPassedAsDataTable()
        {
            // Act
            Action act = () => FluidPDFTemplateModel.FromDataTable("Table", null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void FromObject_ShouldThrowArgumentNullException_WhenNullIsPassedAsObject()
        {
            // Act
            Action act = () => FluidPDFTemplateModel.FromObject("Model", null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void FromJsonString_ShouldThrowArgumentNullException_WhenNullIsPassedAsJsonString()
        {
            // Act
            Action act = () => FluidPDFTemplateModel.FromJsonString("Model", null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void FromDictionary_ShouldThrowArgumentNullException_WhenNullIsPassedAsDictionary()
        {
            // Act
            Action act = () => FluidPDFTemplateModel.FromDictionary("Model", null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
