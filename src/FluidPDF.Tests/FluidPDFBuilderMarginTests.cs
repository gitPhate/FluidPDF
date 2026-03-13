using FluentAssertions;
using FluidPDF.Builder;
using FluidPDF.Support.PuppeteerSharp;
using FluidPDF.Tests.Mocks;
using FluidPDF.Tests.Mothers;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace FluidPDF.Tests
{
    public class FluidPDFBuilderMarginTests
    {
        [Fact]
        public async Task BuildAsync_ShouldPassUniformInchMarginToPdfDataAsync_WhenWithInchMarginUniformIsCalled()
        {
            // Arrange
            IChromiumRetriever retriever = ChromiumRetrieverMock.CreateWithSinglePagePdfAndOptionCapture(
                out _,
                out _,
                out PdfOptions?[] box);

            FluidPDFBuilder<object> builder = new(TemplateModelMother.SimpleObject(), retriever);
            builder.WithInchMargin(0.75m);
            builder.WithTemplate(TemplateModelMother.SimpleTemplate);

            // Act
            await builder.BuildAsync();

            // Assert
            PdfOptions? options = box[0];
            options.Should().NotBeNull();
            options!.MarginOptions.Bottom.Should().Be("0.75 in");
            options.MarginOptions.Left.Should().Be("0.75 in");
            options.MarginOptions.Right.Should().Be("0.75 in");
            options.MarginOptions.Top.Should().Be("0.75 in");
        }

        [Fact]
        public async Task BuildAsync_ShouldPassPerSideInchMarginsToPdfDataAsync_WhenWithInchMarginPerSideIsCalled()
        {
            // Arrange
            IChromiumRetriever retriever = ChromiumRetrieverMock.CreateWithSinglePagePdfAndOptionCapture(
                out _,
                out _,
                out PdfOptions?[] box);

            FluidPDFBuilder<object> builder = new(TemplateModelMother.SimpleObject(), retriever);
            builder.WithInchMargin(bottom: 0.1m, left: 0.2m, right: 0.3m, top: 0.4m);
            builder.WithTemplate(TemplateModelMother.SimpleTemplate);

            // Act
            await builder.BuildAsync();

            // Assert
            PdfOptions? options = box[0];
            options.Should().NotBeNull();
            options!.MarginOptions.Bottom.Should().Be("0.1 in");
            options.MarginOptions.Left.Should().Be("0.2 in");
            options.MarginOptions.Right.Should().Be("0.3 in");
            options.MarginOptions.Top.Should().Be("0.4 in");
        }

        [Fact]
        public async Task BuildAsync_ShouldPassUniformPixelMarginToPdfDataAsync_WhenWithPixelMarginUniformIsCalled()
        {
            // Arrange
            IChromiumRetriever retriever = ChromiumRetrieverMock.CreateWithSinglePagePdfAndOptionCapture(
                out _,
                out _,
                out PdfOptions?[] box);

            FluidPDFBuilder<object> builder = new(TemplateModelMother.SimpleObject(), retriever);
            builder.WithPixelMargin(20m);
            builder.WithTemplate(TemplateModelMother.SimpleTemplate);

            // Act
            await builder.BuildAsync();

            // Assert
            PdfOptions? options = box[0];
            options.Should().NotBeNull();
            options!.MarginOptions.Bottom.Should().Be("20 px");
            options.MarginOptions.Left.Should().Be("20 px");
            options.MarginOptions.Right.Should().Be("20 px");
            options.MarginOptions.Top.Should().Be("20 px");
        }

        [Fact]
        public async Task BuildAsync_ShouldPassPerSidePixelMarginsToPdfDataAsync_WhenWithPixelMarginPerSideIsCalled()
        {
            // Arrange
            IChromiumRetriever retriever = ChromiumRetrieverMock.CreateWithSinglePagePdfAndOptionCapture(
                out _,
                out _,
                out PdfOptions?[] box);

            FluidPDFBuilder<object> builder = new(TemplateModelMother.SimpleObject(), retriever);
            builder.WithPixelMargin(bottom: 10m, left: 20m, right: 30m, top: 40m);
            builder.WithTemplate(TemplateModelMother.SimpleTemplate);

            // Act
            await builder.BuildAsync();

            // Assert
            PdfOptions? options = box[0];
            options.Should().NotBeNull();
            options!.MarginOptions.Bottom.Should().Be("10 px");
            options.MarginOptions.Left.Should().Be("20 px");
            options.MarginOptions.Right.Should().Be("30 px");
            options.MarginOptions.Top.Should().Be("40 px");
        }
    }
}
