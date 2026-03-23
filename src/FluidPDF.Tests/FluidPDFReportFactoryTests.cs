using FluentAssertions;
using FluidPDF.Exceptions;
using FluidPDF.Fluid;
using FluidPDF.Support.PuppeteerSharp;
using FluidPDF.Templating;
using FluidPDF.Templating.Localization;
using FluidPDF.Tests.Mocks;
using FluidPDF.Tests.Mothers;
using NSubstitute;
using PuppeteerSharp;
using System.Globalization;

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
            FluidPDFReportFactory factory = new(templateEngine, retriever);
            string template = TemplateModelMother.SimpleTemplate;
            FluidPDFTemplateModel model = FluidPDFTemplateModel.FromObject(TemplateModelMother.SimpleObject());

            // Act
            byte[] result = await factory.CompileReportAsync(template, model, new FluidPDFReportOptions());

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
            FluidPDFReportFactory factory = new(templateEngine, retriever);
            string template = TemplateModelMother.SimpleTemplate;
            FluidPDFTemplateModel model = FluidPDFTemplateModel.FromObject(TemplateModelMother.SimpleObject());
            using MemoryStream stream = new();

            // Act
            await factory.CompileReportAsync(template, model, stream, new FluidPDFReportOptions());

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
            FluidPDFReportFactory factory = new(templateEngine, retriever);
            FluidPDFTemplateModel model = FluidPDFTemplateModel.FromObject(TemplateModelMother.SimpleObject());

            // Act
            await factory.CompileReportAsync(TemplateModelMother.SimpleTemplate, model, new FluidPDFReportOptions());

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
            FluidPDFReportFactory factory = new(templateEngine, retriever);
            FluidPDFTemplateModel model = FluidPDFTemplateModel.FromObject(TemplateModelMother.SimpleObject());

            // Act
            Func<Task> act = async () =>
                await factory.CompileReportAsync(TemplateModelMother.SimpleTemplate, model, new FluidPDFReportOptions());

            // Assert
            await act.Should().ThrowAsync<Exception>();
            await page.Received(1).CloseAsync();
            await browser.Received(1).CloseAsync();
        }

        [Fact]
        public async Task CompileReportAsync_ShouldThrowFluidPDFMissingLocalizationProviderException_WhenCultureIsProvidedWithoutProvider()
        {
            // Arrange
            IChromiumRetriever retriever = ChromiumRetrieverMock.CreateWithSinglePagePdf(out _, out _);
            FluidTemplateEngine templateEngine = new();
            FluidPDFReportFactory factory = new(templateEngine, retriever);
            FluidPDFTemplateModel model = FluidPDFTemplateModel.FromObject(TemplateModelMother.SimpleObject());

            // Act
            Func<Task> act = async () =>
                await factory.CompileReportAsync(
                    TemplateModelMother.SimpleTemplate,
                    model,
                    new FluidPDFReportOptions { CultureInfo = new CultureInfo("it-IT") });

            // Assert
            await act.Should().ThrowAsync<FluidPDFMissingLocalizationProviderException>();
        }

        [Fact]
        public async Task CompileReportAsync_ShouldFallbackToEnUsLocalization_WhenRequestedCultureIsMissing()
        {
            // Arrange
            IChromiumRetriever retriever = ChromiumRetrieverMock.CreateWithSinglePagePdf(out _, out _);
            FluidTemplateEngine templateEngine = new();
            DictionaryLocalizationProvider provider = new(
                new Dictionary<string, Dictionary<string, string>>
                {
                    ["en-US"] = new()
                    {
                        ["label_title"] = "Invoice"
                    }
                });

            FluidPDFReportFactory factory = new(templateEngine, retriever, provider);
            FluidPDFTemplateModel model = FluidPDFTemplateModel.FromObject(new { Title = "Invoice-001" });

            // Act
            byte[] result = await factory.CompileReportAsync(
                "<p>{{ Resx.label_title }}: {{ Model.Title }}</p>",
                model,
                new FluidPDFReportOptions { CultureInfo = new CultureInfo("it-IT") });

            // Assert
            result.Should().NotBeNullOrEmpty();
            PDFDocumentMother.IsValidPDF(result).Should().BeTrue();
        }

        [Fact]
        public async Task CompileReportAsync_ShouldThrowFluidPDFLocalizationException_WhenLocalizationContainsHtml()
        {
            // Arrange
            IChromiumRetriever retriever = ChromiumRetrieverMock.CreateWithSinglePagePdf(out _, out _);
            FluidTemplateEngine templateEngine = new();
            DictionaryLocalizationProvider provider = new(
                new Dictionary<string, Dictionary<string, string>>
                {
                    ["en-US"] = new()
                    {
                        ["label_title"] = "<b>Invoice</b>"
                    }
                });

            FluidPDFReportFactory factory = new(templateEngine, retriever, provider);
            FluidPDFTemplateModel model = FluidPDFTemplateModel.FromObject(TemplateModelMother.SimpleObject());

            // Act
            Func<Task> act = async () =>
                await factory.CompileReportAsync(TemplateModelMother.SimpleTemplate, model, new FluidPDFReportOptions());

            // Assert
            await act.Should().ThrowAsync<FluidPDFLocalizationException>();
        }
    }
}
