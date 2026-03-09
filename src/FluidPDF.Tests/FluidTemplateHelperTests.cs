using FluentAssertions;
using FluidPDF.Fluid;
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
            string template = TemplateModelMother.SimpleObjectTemplate();

            // Act
            string result = await FluidTemplateHelper.RenderTemplateByTypeAsync(template, model);

            // Assert
            result.Should().Be(TemplateModelMother.SimpleObjectExpectedOutput());
        }

        [Fact]
        public async Task RenderTemplateByTypeAsync_ShouldRenderJsonProperty_WhenJsonStringModelIsProvided()
        {
            // Arrange
            string model = TemplateModelMother.SimpleJsonString();
            string template = TemplateModelMother.SimpleJsonTemplate();

            // Act
            string result = await FluidTemplateHelper.RenderTemplateByTypeAsync(template, model);

            // Assert
            result.Should().Be(TemplateModelMother.SimpleJsonExpectedOutput());
        }

        [Fact]
        public async Task RenderTemplateByTypeAsync_ShouldRenderDictionaryValue_WhenDictionaryModelIsProvided()
        {
            // Arrange
            System.Collections.Generic.Dictionary<string, object> model = TemplateModelMother.SimpleDictionary();
            string template = TemplateModelMother.SimpleDictionaryTemplate();

            // Act
            string result = await FluidTemplateHelper.RenderTemplateByTypeAsync(template, model);

            // Assert
            result.Should().Be(TemplateModelMother.SimpleDictionaryExpectedOutput());
        }

        [Fact]
        public async Task RenderTemplateByTypeAsync_ShouldRenderBothModels_WhenFluidModelArrayIsProvided()
        {
            // Arrange
            FluidModel[] models = TemplateModelMother.TwoModelArray();
            string template = TemplateModelMother.TwoModelTemplate();

            // Act
            string result = await FluidTemplateHelper.RenderTemplateByTypeAsync(template, models);

            // Assert
            result.Should().Be(TemplateModelMother.TwoModelExpectedOutput());
        }

        [Fact]
        public async Task RenderTemplateByTypeAsync_ShouldHtmlEncodeSpecialCharacters_WhenEncodeHtmlIsTrue()
        {
            // Arrange
            object model = TemplateModelMother.HtmlSpecialCharsObject();
            string template = TemplateModelMother.HtmlSpecialCharsTemplate();

            // Act
            string result = await FluidTemplateHelper.RenderTemplateByTypeAsync(template, model, encodeHtml: true);

            // Assert
            result.Should().Be(TemplateModelMother.HtmlEncodedExpectedOutput());
        }

        [Fact]
        public async Task RenderTemplateByTypeAsync_ShouldThrowFluidRenderException_WhenTemplateIsInvalid()
        {
            // Arrange
            object model = TemplateModelMother.SimpleObject();
            string template = TemplateModelMother.InvalidTemplate();

            // Act
            System.Func<Task> act = async () =>
                await FluidTemplateHelper.RenderTemplateByTypeAsync(template, model);

            // Assert
            await act.Should().ThrowAsync<FluidRenderException>();
        }
    }
}
