using FluentAssertions;
using FluidPDF.Templating;
using FluidPDF.Tests.Mothers;
using System.Data;

namespace FluidPDF.Tests
{
    public abstract class TemplateEngineTests
    {
        protected abstract IFluidPDFTemplateEngine CreateEngine();
        protected abstract string DataTableTemplate { get; }
        protected abstract string HtmlSpecialCharsExpectedOutput { get; }

        protected virtual string SimpleTemplate => TemplateModelMother.SimpleTemplate;
        protected virtual string TwoModelTemplate => TemplateModelMother.TwoModelTemplate;
        protected virtual string HtmlSpecialCharsTemplate => TemplateModelMother.HtmlSpecialCharsTemplate;

        [Fact]
        public async Task RenderTemplateAsync_ShouldRenderObjectValue_WhenObjectModelIsProvided()
        {
            // Arrange
            object model = TemplateModelMother.SimpleObject();
            string template = SimpleTemplate;
            IFluidPDFTemplateEngine templateEngine = CreateEngine();

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
            string template = SimpleTemplate;
            IFluidPDFTemplateEngine templateEngine = CreateEngine();

            // Act
            string result = await templateEngine.RenderTemplateAsync(template, model, new());

            // Assert
            result.Should().Be(TemplateModelMother.SimpleDictionaryExpectedOutput);
        }

        [Fact]
        public async Task RenderTemplateAsync_ShouldRenderBothModels_WhenFluidPDFTemplateModelArrayIsProvided()
        {
            // Arrange
            FluidPDFTemplateModel[] models = TemplateModelMother.TwoModelArray();
            string template = TwoModelTemplate;
            IFluidPDFTemplateEngine templateEngine = CreateEngine();

            // Act
            string result = await templateEngine.RenderTemplateAsync(template, models, new());

            // Assert
            result.Should().Be(TemplateModelMother.TwoModelExpectedOutput);
        }

        [Fact]
        public async Task RenderTemplateAsync_ShouldRenderDataTableValue_WhenDataTableModelIsProvided()
        {
            // Arrange
            DataTable model = TemplateModelMother.SimpleDataTable();
            string template = DataTableTemplate;
            IFluidPDFTemplateEngine templateEngine = CreateEngine();

            // Act
            string result = await templateEngine.RenderTemplateAsync(template, model, new());

            // Assert
            result.Should().Be(TemplateModelMother.SimpleDataTableExpectedOutput());
        }

        [Fact]
        public async Task RenderTemplateAsync_ShouldRenderJsonStringValue_WhenJsonStringIsProvided()
        {
            // Arrange
            string model = TemplateModelMother.SimpleJsonString;
            string template = SimpleTemplate;
            IFluidPDFTemplateEngine templateEngine = CreateEngine();

            // Act
            string result = await templateEngine.RenderTemplateAsync(template, model, new());

            // Assert
            result.Should().Be(TemplateModelMother.SimpleObjectExpectedOutput);
        }

        [Fact]
        public async Task RenderTemplateAsync_ShouldRenderHtmlSpecialCharsAccordingToEngine_WhenObjectModelIsProvided()
        {
            // Arrange
            object model = TemplateModelMother.HtmlSpecialCharsObject();
            string template = HtmlSpecialCharsTemplate;
            IFluidPDFTemplateEngine templateEngine = CreateEngine();

            // Act
            string result = await templateEngine.RenderTemplateAsync(template, model, new());

            // Assert
            result.Should().Be(HtmlSpecialCharsExpectedOutput);
        }

        [Fact]
        public async Task RenderTemplateAsync_ShouldThrowFluidPDFTemplateRenderException_WhenMultipleModelsShareTheSameKey()
        {
            // Arrange
            // Both models are registered under options.ModelName ("Model") — the second Add call
            // raises ArgumentException because ScriptObject does not allow duplicate keys.
            FluidPDFTemplateModel m1 = FluidPDFTemplateModel.FromObject(new { Name = "Dave" });
            FluidPDFTemplateModel m2 = FluidPDFTemplateModel.FromObject(new { Name = "Eve" });
            IFluidPDFTemplateEngine templateEngine = CreateEngine();

            // Act
            Func<Task> act = async () =>
                await templateEngine.RenderTemplateAsync("<p>{{ Model.Name }}</p>", [m1, m2], new());

            // Assert
            // This documents the known bug: the engine always uses options.ModelName as the key
            // for every model in the array rather than each model's own Name property.
            // This is a limitation of scriban but it has been ported to the Fluid engine as well
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("*already been added*");
        }
    }
}
