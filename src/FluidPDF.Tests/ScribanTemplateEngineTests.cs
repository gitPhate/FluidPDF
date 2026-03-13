using FluentAssertions;
using FluidPDF.Scriban;
using FluidPDF.Templating;
using FluidPDF.Tests.Mothers;

namespace FluidPDF.Tests
{
    public class ScribanTemplateEngineTests : TemplateEngineTests
    {
        protected override IFluidPDFTemplateEngine CreateEngine() => new ScribanTemplateEngine();
        protected override string DataTableTemplate => TemplateModelMother.ScribanDataTableTemplate;
        protected override string HtmlSpecialCharsExpectedOutput => TemplateModelMother.ScribanHtmlSpecialCharsExpectedOutput;

        // --- Engine-specific: invalid template throws InvalidOperationException ---

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
            await act.Should().ThrowAsync<InvalidOperationException>();
        }
    }
}
