using FluentAssertions;
using FluidPDF.Razor;
using FluidPDF.Templating;
using FluidPDF.Tests.Mothers;
using RazorEngineCore;

namespace FluidPDF.Tests
{
    public class RazorTemplateEngineTests : TemplateEngineTests
    {
        protected override IFluidPDFTemplateEngine CreateEngine() => new RazorTemplateEngine();

        protected override string SimpleTemplate => TemplateModelMother.RazorSimpleTemplate;
        protected override string TwoModelTemplate => TemplateModelMother.RazorTwoModelTemplate;
        protected override string HtmlSpecialCharsTemplate => TemplateModelMother.RazorHtmlSpecialCharsTemplate;
        protected override string DataTableTemplate => TemplateModelMother.RazorDataTableTemplate();
        protected override string HtmlSpecialCharsExpectedOutput => TemplateModelMother.RazorHtmlSpecialCharsExpectedOutput;

        // --- Engine-specific: invalid template throws RazorEngineCompilationException ---

        [Fact]
        public async Task RenderTemplateAsync_ShouldThrowFluidTemplateRenderException_WhenTemplateIsInvalid()
        {
            // Arrange
            object model = TemplateModelMother.SimpleObject();
            RazorTemplateEngine templateEngine = new();

            // Act
            Func<Task> act = async () =>
                await templateEngine.RenderTemplateAsync("@{ var x = }", model, new());

            // Assert
            await act.Should().ThrowAsync<RazorEngineCompilationException>();
        }
    }
}
