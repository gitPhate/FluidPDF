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
        protected override string LocalizationTemplate => TemplateModelMother.LocalizationTemplate;
        protected override string ArrayTemplate => TemplateModelMother.ScribanArrayTemplate;

        // --- Engine-specific: invalid template throws InvalidOperationException ---

        [Fact]
        public async Task RenderTemplateAsync_ShouldThrowFluidPDFTemplateRenderException_WhenTemplateIsInvalid()
        {
            // Arrange
            object model = TemplateModelMother.SimpleObject();
            using ScribanTemplateEngine templateEngine = new();

            // Act
            Func<Task> act = async () =>
                await templateEngine.RenderTemplateAsync("{{ if }}", model, new());

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }
    }
}
