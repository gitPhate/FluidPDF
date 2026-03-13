using FluentAssertions;
using FluidPDF.Builder;
using FluidPDF.Scriban;
using FluidPDF.Support.PuppeteerSharp;
using FluidPDF.Tests.Mocks;
using FluidPDF.Tests.Mothers;
using NSubstitute;
using PuppeteerSharp;

namespace FluidPDF.Tests
{
    public class FluidPDFBuilderScribanExtensionsTests
    {
        [Fact]
        public async Task BuildAsync_ShouldRenderTemplateWithScribanEngine_WhenWithScribanTemplateEngineIsCalled()
        {
            // Arrange
            IChromiumRetriever retriever = ChromiumRetrieverMock.CreateWithSinglePagePdf(out _, out IPage page);

            FluidPDFBuilder<object> builder = new(TemplateModelMother.SimpleObject(), retriever);
            builder
                .WithScribanTemplateEngine()
                .WithTemplate(TemplateModelMother.SimpleTemplate);

            // Act
            await builder.BuildAsync();

            // Assert — the Scriban engine rendered the template and set valid HTML content on the page
            await page
                .Received(1)
                .SetContentAsync(TemplateModelMother.SimpleObjectExpectedOutput);
        }

        [Fact]
        public void WithScribanTemplateEngine_ShouldReturnSameBuilderInstance_ForFluentChaining()
        {
            // Arrange
            IFluidPDFBuilder builder = FluidPDFBuilder.NewWithModel(TemplateModelMother.SimpleObject());

            // Act
            IFluidPDFBuilder result = builder.WithScribanTemplateEngine();

            // Assert
            result.Should().BeSameAs(builder);
        }
    }
}
