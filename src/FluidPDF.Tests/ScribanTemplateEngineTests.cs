using FluentAssertions;
using FluentAssertions.Specialized;
using FluidPDF.Scriban;
using FluidPDF.Templating;
using FluidPDF.Tests.Mothers;
using System.Collections.Generic;
using System.Data;

namespace FluidPDF.Tests
{
    public class ScribanTemplateEngineTests
    {
        // --- Instance method overloads (IFluidPDFTemplateEngine) ---

        [Fact]
        public async Task RenderTemplateAsync_ShouldRenderObjectValue_WhenObjectModelIsProvided()
        {
            // Arrange
            object model = TemplateModelMother.SimpleObject();
            string template = TemplateModelMother.SimpleTemplate;
            ScribanTemplateEngine templateEngine = new();

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
            ScribanTemplateEngine templateEngine = new();

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
            string template = TemplateModelMother.ScribanDataTableTemplate;
            ScribanTemplateEngine templateEngine = new();

            // Act
            string result = await templateEngine.RenderTemplateAsync(template, model, new());

            // Assert
            result.Should().Be(TemplateModelMother.ScribanDataTableExpectedOutput);
        }

        [Fact]
        public async Task RenderTemplateAsync_ShouldNotHtmlEncodeSpecialCharacters_WhenObjectModelIsProvided()
        {
            // Arrange
            object model = TemplateModelMother.HtmlSpecialCharsObject();
            string template = TemplateModelMother.HtmlSpecialCharsTemplate;
            ScribanTemplateEngine templateEngine = new();

            // Act
            string result = await templateEngine.RenderTemplateAsync(template, model, new());

            // Assert
            // Scriban does not HTML-encode output by default — raw characters are emitted.
            result.Should().Be(TemplateModelMother.ScribanHtmlSpecialCharsExpectedOutput);
        }

        [Fact]
        public async Task RenderTemplateAsync_ShouldThrowFluidPDFTemplateRenderException_WhenPlainValueModelTypeIsProvided()
        {
            // Arrange
            FluidPDFTemplateModel model = FluidPDFTemplateModel.FromPlainValue("Greeting", "Hello");
            ScribanTemplateEngine templateEngine = new();

            // Act
            Func<Task> act = async () =>
                await templateEngine.RenderTemplateAsync("<p>{{ Greeting }}</p>", [model], new());

            // Assert
            ExceptionAssertions<FluidPDFTemplateRenderException> assertion =
                await act.Should().ThrowAsync<FluidPDFTemplateRenderException>();
            assertion.WithInnerException<InvalidOperationException>()
                .WithMessage("*PlainValue*");
        }

        [Fact]
        public async Task RenderTemplateAsync_ShouldThrowFluidPDFTemplateRenderException_WhenJsonStringModelTypeIsProvided()
        {
            // Arrange
            FluidPDFTemplateModel model = FluidPDFTemplateModel.FromJsonString("Model", "{\"Name\":\"Alice\"}");
            ScribanTemplateEngine templateEngine = new();

            // Act
            Func<Task> act = async () =>
                await templateEngine.RenderTemplateAsync("<p>{{ Model.Name }}</p>", [model], new());

            // Assert
            ExceptionAssertions<FluidPDFTemplateRenderException> assertion =
                await act.Should().ThrowAsync<FluidPDFTemplateRenderException>();
            assertion.WithInnerException<InvalidOperationException>()
                .WithMessage("*JsonString*");
        }

        [Fact]
        public async Task RenderTemplateAsync_ShouldThrowFluidPDFTemplateRenderException_WhenTemplateIsInvalid()
        {
            // Arrange
            object model = TemplateModelMother.SimpleObject();
            ScribanTemplateEngine templateEngine = new();

            // Act
            Func<Task> act = async () =>
                await templateEngine.RenderTemplateAsync("{{ if }}", model, new());

            // Assert
            await act.Should().ThrowAsync<FluidPDFTemplateRenderException>();
        }

        [Fact]
        public async Task RenderTemplateAsync_ShouldThrowFluidPDFTemplateRenderException_WhenMultipleModelsShareTheSameKey()
        {
            // Arrange
            // Both models are registered under options.ModelName ("Model") — the second Add call
            // raises ArgumentException because ScriptObject does not allow duplicate keys.
            FluidPDFTemplateModel m1 = FluidPDFTemplateModel.FromObject("Model", new { Name = "Dave" });
            FluidPDFTemplateModel m2 = FluidPDFTemplateModel.FromObject("Model", new { Name = "Eve" });
            ScribanTemplateEngine templateEngine = new();

            // Act
            Func<Task> act = async () =>
                await templateEngine.RenderTemplateAsync("<p>{{ Model.Name }}</p>", [m1, m2], new());

            // Assert
            // This documents the known bug: the engine always uses options.ModelName as the key
            // for every model in the array rather than each model's own Name property.
            ExceptionAssertions<FluidPDFTemplateRenderException> assertion =
                await act.Should().ThrowAsync<FluidPDFTemplateRenderException>();
            assertion.WithInnerException<ArgumentException>()
                .WithMessage("*already been added*");
        }
    }
}
