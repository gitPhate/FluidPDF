using FluentAssertions;
using FluidPDF.Exceptions;
using FluidPDF.Fluid;
using FluidPDF.Templating;
using FluidPDF.Tests.Mothers;
using System.Data;

namespace FluidPDF.Tests
{
    public class FluidTemplateEngineTests
    {
        // --- Instance method overloads (IFluidPDFTemplateEngine) ---

        [Fact]
        public async Task RenderTemplateAsync_ShouldRenderObjectValue_WhenObjectModelIsProvided()
        {
            // Arrange
            object model = TemplateModelMother.SimpleObject();
            string template = TemplateModelMother.SimpleTemplate;
            FluidTemplateEngine templateEngine = new();

            // Act
            string result = await templateEngine.RenderTemplateAsync(template, model, new());

            // Assert
            result.Should().Be(TemplateModelMother.SimpleObjectExpectedOutput);
        }

        [Fact]
        public async Task RenderTemplateAsync_ShouldRenderDictionaryValue_WhenDictionaryModelIsProvided()
        {
            // Arrange
            Dictionary<string, object> model = TemplateModelMother.SimpleDictionary();
            string template = TemplateModelMother.SimpleTemplate;
            FluidTemplateEngine templateEngine = new();

            // Act
            string result = await templateEngine.RenderTemplateAsync(template, model, new());

            // Assert
            result.Should().Be(TemplateModelMother.SimpleDictionaryExpectedOutput);
        }

        [Fact]
        public async Task RenderTemplateAsync_ShouldRenderDataTableValue_WhenDataTableModelIsProvided()
        {
            // Arrange
            DataTable model = TemplateModelMother.SimpleDataTable();
            string template = TemplateModelMother.SimpleDataTableTemplate;
            FluidTemplateEngine templateEngine = new();

            // Act
            string result = await templateEngine.RenderTemplateAsync(template, model, new());

            // Assert
            result.Should().Be(TemplateModelMother.SimpleDataTableExpectedOutput);
        }

        [Fact]
        public async Task RenderTemplateAsync_ShouldRenderBothModels_WhenFluidPDFTemplateModelArrayIsProvided()
        {
            // Arrange
            FluidPDFTemplateModel[] models = TemplateModelMother.TwoModelArray();
            string template = TemplateModelMother.TwoModelTemplate;
            FluidTemplateEngine templateEngine = new();

            // Act
            string result = await templateEngine.RenderTemplateAsync(template, models, new());

            // Assert
            result.Should().Be(TemplateModelMother.TwoModelExpectedOutput);
        }

        [Fact]
        public async Task RenderTemplateAsync_ShouldHtmlEncodeSpecialCharacters_WhenInstanceOverloadIsUsed()
        {
            // Arrange
            object model = TemplateModelMother.HtmlSpecialCharsObject();
            string template = TemplateModelMother.HtmlSpecialCharsTemplate;
            FluidTemplateEngine templateEngine = new();

            // Act
            string result = await templateEngine.RenderTemplateAsync(template, model, new());

            // Assert
            result.Should().Be(TemplateModelMother.HtmlEncodedExpectedOutput);
        }

        [Fact]
        public async Task RenderTemplateAsync_ShouldThrowFluidPDFTemplateRenderException_WhenTemplateIsInvalid()
        {
            // Arrange
            object model = TemplateModelMother.SimpleObject();
            string template = TemplateModelMother.InvalidTemplate;
            FluidTemplateEngine templateEngine = new();

            // Act
            Func<Task> act = async () =>
                await templateEngine.RenderTemplateAsync(template, model, new());

            // Assert
            await act.Should().ThrowAsync<FluidTemplateRenderException>();
        }

        // --- Static helper overloads ---

        [Fact]
        public async Task RenderWithObjectAsync_ShouldRenderObjectValue_WhenObjectModelIsProvided()
        {
            // Arrange
            object model = TemplateModelMother.SimpleObject();
            string template = TemplateModelMother.SimpleTemplate;

            // Act
            string result = await FluidTemplateEngine.RenderWithObjectAsync(template, model);

            // Assert
            result.Should().Be(TemplateModelMother.SimpleObjectExpectedOutput);
        }

        [Fact]
        public async Task RenderWithDictionaryAsync_ShouldRenderDictionaryValue_WhenDictionaryModelIsProvided()
        {
            // Arrange
            Dictionary<string, object> model = TemplateModelMother.SimpleDictionary();
            string template = TemplateModelMother.SimpleTemplate;

            // Act
            string result = await FluidTemplateEngine.RenderWithDictionaryAsync(template, model);

            // Assert
            result.Should().Be(TemplateModelMother.SimpleDictionaryExpectedOutput);
        }

        [Fact]
        public async Task RenderWithJsonStringAsync_ShouldRenderJsonStringValue_WhenJsonStringModelIsProvided()
        {
            // Arrange
            string model = TemplateModelMother.SimpleJsonString;
            string template = TemplateModelMother.SimpleTemplate;

            // Act
            string result = await FluidTemplateEngine.RenderWithJsonStringAsync(template, model);

            // Assert
            result.Should().Be(TemplateModelMother.SimpleJsonStringExpectedOutput);
        }

        [Fact]
        public async Task RenderWithDataRowAsync_ShouldRenderDataRowValue_WhenDataRowModelIsProvided()
        {
            // Arrange
            DataRow model = TemplateModelMother.SimpleDataRow();
            string template = TemplateModelMother.SimpleTemplate;

            // Act
            string result = await FluidTemplateEngine.RenderWithDataRowAsync(template, model);

            // Assert
            result.Should().Be(TemplateModelMother.SimpleDataRowExpectedOutput);
        }

        [Fact]
        public async Task RenderWithDataTableAsync_ShouldRenderDataTableValue_WhenDataTableModelIsProvided()
        {
            // Arrange
            DataTable model = TemplateModelMother.SimpleDataTable();
            string template = TemplateModelMother.SimpleDataTableTemplate;

            // Act
            string result = await FluidTemplateEngine.RenderWithDataTableAsync(template, model);

            // Assert
            result.Should().Be(TemplateModelMother.SimpleDataTableExpectedOutput);
        }

        [Fact]
        public async Task RenderWithMultipleModelsAsync_ShouldRenderBothModels_WhenMultipleModelsAreProvided()
        {
            // Arrange
            FluidPDFTemplateModel[] models = TemplateModelMother.TwoModelArray();
            string template = TemplateModelMother.TwoModelTemplate;

            // Act
            string result = await FluidTemplateEngine.RenderWithMultipleModelsAsync(template, models);

            // Assert
            result.Should().Be(TemplateModelMother.TwoModelExpectedOutput);
        }
    }
}
