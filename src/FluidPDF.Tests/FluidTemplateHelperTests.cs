using FluentAssertions;
using FluidPDF.Fluid;
using FluidPDF.Templating;
using FluidPDF.Tests.Mothers;

namespace FluidPDF.Tests
{
    public class FluidTemplateHelperTests
    {
        [Fact]
        public async Task RenderTemplateByTypeAsync_ShouldRenderObjectProperty_WhenObjectModelIsProvided()
        {
            // Arrange
            object model = TemplateModelMother.SimpleObject();
            string template = TemplateModelMother.SimpleTemplate();
            FluidTemplateEngine templateEngine = new();

            // Act
            string result = await templateEngine.RenderTemplateAsync(template, model, new());

            // Assert
            result.Should().Be(TemplateModelMother.SimpleObjectExpectedOutput());
        }

        [Fact]
        public async Task RenderTemplateByTypeAsync_ShouldRenderDictionaryValue_WhenDictionaryModelIsProvided()
        {
            // Arrange
            Dictionary<string, object> model = TemplateModelMother.SimpleDictionary();
            string template = TemplateModelMother.SimpleTemplate();
            FluidTemplateEngine templateEngine = new();

            // Act
            string result = await templateEngine.RenderTemplateAsync(template, model, new());

            // Assert
            result.Should().Be(TemplateModelMother.SimpleDictionaryExpectedOutput());
        }

        [Fact]
        public async Task RenderTemplateByTypeAsync_ShouldRenderBothModels_WhenFluidPDFTemplateModelArrayIsProvided()
        {
            // Arrange
            FluidPDFTemplateModel[] models = TemplateModelMother.TwoModelArray();
            string template = TemplateModelMother.TwoModelTemplate();
            FluidTemplateEngine templateEngine = new();

            // Act
            string result = await templateEngine.RenderTemplateAsync(template, models, new());

            // Assert
            result.Should().Be(TemplateModelMother.TwoModelExpectedOutput());
        }

        [Fact]
        public async Task RenderTemplateByTypeAsync_ShouldHtmlEncodeSpecialCharacters_WhenEncodeHtmlIsTrue()
        {
            // Arrange
            object model = TemplateModelMother.HtmlSpecialCharsObject();
            string template = TemplateModelMother.HtmlSpecialCharsTemplate();
            FluidTemplateEngine templateEngine = new();

            // Act
            string result = await templateEngine.RenderTemplateAsync(template, model, new());

            // Assert
            result.Should().Be(TemplateModelMother.HtmlEncodedExpectedOutput());
        }

        [Fact]
        public async Task RenderTemplateByTypeAsync_ShouldThrowFluidRenderException_WhenTemplateIsInvalid()
        {
            // Arrange
            object model = TemplateModelMother.SimpleObject();
            string template = TemplateModelMother.InvalidTemplate();
            FluidTemplateEngine templateEngine = new();

            // Act
            Func<Task> act = async () =>
                await templateEngine.RenderTemplateAsync(template, model, new());

            // Assert
            await act.Should().ThrowAsync<FluidPDFTemplateRenderException>();
        }
    }
}
