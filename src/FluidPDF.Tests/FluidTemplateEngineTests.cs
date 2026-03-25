using FluentAssertions;
using FluidPDF.Exceptions;
using FluidPDF.Fluid;
using FluidPDF.Templating;
using FluidPDF.Tests.Mothers;

namespace FluidPDF.Tests
{
    public class FluidTemplateEngineTests : TemplateEngineTests
    {
        protected override IFluidPDFTemplateEngine CreateEngine() => new FluidTemplateEngine();
        protected override string DataTableTemplate => TemplateModelMother.SimpleDataTableTemplate();
        protected override string LocalizationTemplate => TemplateModelMother.LocalizationTemplate;

        // --- Engine-specific: invalid template throws domain exception ---

        [Fact]
        public async Task RenderTemplateAsync_ShouldThrowFluidPDFTemplateRenderException_WhenTemplateIsInvalid()
        {
            // Arrange
            object model = TemplateModelMother.SimpleObject();
            string template = TemplateModelMother.InvalidTemplate;
            using FluidTemplateEngine templateEngine = new();

            // Act
            Func<Task> act = async () =>
                await templateEngine.RenderTemplateAsync(template, model, new());

            // Assert
            await act.Should().ThrowAsync<FluidTemplateRenderException>();
        }
    }
}
