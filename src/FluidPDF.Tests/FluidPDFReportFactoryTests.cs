using FluentAssertions;
using FluidPDF.Fluid;
using FluidPDF.Support.PuppeteerSharp;
using FluidPDF.Tests.Mocks;
using FluidPDF.Tests.Mothers;
using NSubstitute;
using PuppeteerSharp;

namespace FluidPDF.Tests
{
    public class FluidPDFReportFactoryTests
    {
        [Fact]
        public async Task CompileReportAsync_ShouldReturnValidPdfBytes_WhenValidTemplateAndObjectModelAreProvided()
        {
            // Arrange
            IChromiumRetriever retriever = ChromiumRetrieverMock.CreateWithSinglePagePdf(out _, out _);
            FluidTemplateEngine templateEngine = new();
            FluidPDFReportFactory factory = new(templateEngine, retriever, new FluidPDFReportOptions());
            string template = TemplateModelMother.SimpleTemplate();
            object model = TemplateModelMother.SimpleObject();

            // Act
            byte[] result = await factory.CompileReportAsync(template, model);

            // Assert
            result.Should().NotBeNullOrEmpty();
            PDFDocumentMother.IsValidPDF(result).Should().BeTrue();
        }

        [Fact]
        public async Task CompileReportAsync_ShouldWriteValidPdfBytesToStream_WhenStreamOverloadIsUsed()
        {
            // Arrange
            IChromiumRetriever retriever = ChromiumRetrieverMock.CreateWithSinglePagePdf(out _, out _);
            FluidTemplateEngine templateEngine = new();
            FluidPDFReportFactory factory = new(templateEngine, retriever, new FluidPDFReportOptions());
            string template = TemplateModelMother.SimpleTemplate();
            object model = TemplateModelMother.SimpleObject();
            using MemoryStream stream = new();

            // Act
            await factory.CompileReportAsync(template, model, stream);

            // Assert
            stream.Length.Should().BeGreaterThan(0);
            PDFDocumentMother.IsValidPDF(stream.ToArray()).Should().BeTrue();
        }

        [Fact]
        public async Task CompileReportAsync_ShouldCloseBrowserAndPage_WhenRenderCompletes()
        {
            // Arrange
            IChromiumRetriever retriever = ChromiumRetrieverMock.CreateWithSinglePagePdf(out IBrowser browser, out IPage page);
            FluidTemplateEngine templateEngine = new();
            FluidPDFReportFactory factory = new(templateEngine, retriever, new FluidPDFReportOptions());

            // Act
            await factory.CompileReportAsync(TemplateModelMother.SimpleTemplate(), TemplateModelMother.SimpleObject());

            // Assert
            await page.Received(1).CloseAsync();
            await browser.Received(1).CloseAsync();
        }

        [Fact]
        public async Task CompileReportAsync_ShouldCloseBrowserAndPage_WhenPageThrowsDuringRender()
        {
            // Arrange
            IChromiumRetriever retriever = ChromiumRetrieverMock.CreateWithPageThatThrowsOnSetContent(out IBrowser browser, out IPage page);
            FluidTemplateEngine templateEngine = new();
            FluidPDFReportFactory factory = new(templateEngine, retriever, new FluidPDFReportOptions());

            // Act
            Func<Task> act = async () =>
                await factory.CompileReportAsync(TemplateModelMother.SimpleTemplate(), TemplateModelMother.SimpleObject());

            // Assert
            await act.Should().ThrowAsync<Exception>();
            await page.Received(1).CloseAsync();
            await browser.Received(1).CloseAsync();
        }
    }
}
