using FluentAssertions;
using FluidPDF.Builder;
using FluidPDF.Exceptions;
using FluidPDF.Support.PuppeteerSharp;
using FluidPDF.Templating;
using FluidPDF.Templating.Localization;
using FluidPDF.Tests.Mocks;
using FluidPDF.Tests.Mothers;
using NSubstitute;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.Globalization;

namespace FluidPDF.Tests
{
    public class FluidPDFBuilderTests
    {
        [Fact]
        public async Task BuildAsync_ShouldPassUniformInchMarginToPdfDataAsync_WhenWithInchMarginUniformIsCalled()
        {
            // Arrange
            IChromiumRetriever retriever = ChromiumRetrieverMock.CreateWithSinglePagePdfAndOptionCapture(
                out _,
                out _,
                out PdfOptions?[] box);

            using FluidPDFBuilder builder = new(retriever);
            builder.WithObjectModel(TemplateModelMother.SimpleObject());
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
        public void Dispose_ShouldDisposePreviouslyAssignedTemplateEngine_WhenWithTemplateEngineWasCalled()
        {
            // Arrange
            IFluidPDFTemplateEngine engine = Substitute.For<IFluidPDFTemplateEngine>();
            using FluidPDFBuilder builder = new();
            builder.WithTemplateEngine(engine);

            // Act
            builder.Dispose();

            // Assert
            engine.Received(1).Dispose();
        }

        [Fact]
        public void WithTemplateEngine_ShouldDisposePreviousEngine_WhenReplaced()
        {
            // Arrange
            IFluidPDFTemplateEngine firstEngine = Substitute.For<IFluidPDFTemplateEngine>();
            IFluidPDFTemplateEngine secondEngine = Substitute.For<IFluidPDFTemplateEngine>();
            using FluidPDFBuilder builder = new();

            // Act
            builder.WithTemplateEngine(firstEngine);
            builder.WithTemplateEngine(secondEngine);

            // Assert
            firstEngine.Received(1).Dispose();
            secondEngine.DidNotReceive().Dispose();
        }

        [Fact]
        public async Task BuildAsync_ShouldPassPerSideInchMarginsToPdfDataAsync_WhenWithInchMarginPerSideIsCalled()
        {
            // Arrange
            IChromiumRetriever retriever = ChromiumRetrieverMock.CreateWithSinglePagePdfAndOptionCapture(
                out _,
                out _,
                out PdfOptions?[] box);

            using FluidPDFBuilder builder = new(retriever);
            builder.WithObjectModel(TemplateModelMother.SimpleObject());
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

            using FluidPDFBuilder builder = new(retriever);
            builder.WithObjectModel(TemplateModelMother.SimpleObject());
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

            using FluidPDFBuilder builder = new(retriever);
            builder.WithObjectModel(TemplateModelMother.SimpleObject());
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

        [Fact]
        public async Task BuildAsync_ShouldThrowFluidPDFBuilderConfigException_WhenNoTemplateIsSet()
        {
            // Arrange
            IFluidPDFBuilder builder = Builder.FluidPDF.NewReport().WithObjectModel(TemplateModelMother.SimpleObject());

            // Act
            Func<Task> act = builder.BuildAsync;

            // Assert
            await act.Should().ThrowAsync<FluidPDFBuilderConfigException>();
        }

        [Fact]
        public void NewFluidPDFReportOptions_ShouldDefaultToA4Portrait_WhenNoFormatOrOrientationIsSet()
        {
            // Arrange
            using FluidPDFBuilder builder = new();
            builder.WithObjectModel(TemplateModelMother.SimpleObject());

            // Act
            FluidPDFReportOptions options = builder.NewFluidPDFReportOptions();

            // Assert
            options.Format.Should().Be(PaperFormat.A4);
            options.Landscape.Should().BeFalse();
        }

        [Fact]
        public void NewFluidPDFReportOptions_ShouldSetLandscapeTrue_WhenWithLandscapeOrientationIsCalled()
        {
            // Arrange
            using FluidPDFBuilder builder = new();
            builder.WithObjectModel(TemplateModelMother.SimpleObject());
            builder.WithLandscapeOrientation();

            // Act
            FluidPDFReportOptions options = builder.NewFluidPDFReportOptions();

            // Assert
            options.Landscape.Should().BeTrue();
        }

        [Fact]
        public void NewFluidPDFReportOptions_ShouldClampScaleToMinimum_WhenScalePercentageIsBelowTen()
        {
            // Arrange
            using FluidPDFBuilder builder = new();
            builder.WithObjectModel(TemplateModelMother.SimpleObject());
            builder.WithScalePercentage(1);

            // Act
            FluidPDFReportOptions options = builder.NewFluidPDFReportOptions();

            // Assert
            options.Scale.Should().Be(0.1M);
        }

        [Fact]
        public void NewFluidPDFReportOptions_ShouldClampScaleToMaximum_WhenScalePercentageIsAboveTwoHundred()
        {
            // Arrange
            using FluidPDFBuilder builder = new();
            builder.WithObjectModel(TemplateModelMother.SimpleObject());
            builder.WithScalePercentage(300);

            // Act
            FluidPDFReportOptions options = builder.NewFluidPDFReportOptions();

            // Assert
            options.Scale.Should().Be(2.0M);
        }

        [Fact]
        public void WithTemplateFile_ShouldThrowFileNotFoundException_WhenFilePathDoesNotExist()
        {
            // Arrange
            IFluidPDFBuilder builder = Builder.FluidPDF.NewReport().WithObjectModel(TemplateModelMother.SimpleObject());

            // Act
            Action act = () => builder.WithTemplateFile("C:\\nonexistent\\path\\template.html");

            // Assert
            act.Should().Throw<FileNotFoundException>();
        }

        [Fact]
        public void WithExternalChromeProcess_ShouldThrowArgumentNullException_WhenNullIsPassedAsPath()
        {
            // Arrange
            IFluidPDFBuilder builder = Builder.FluidPDF.NewReport().WithObjectModel(TemplateModelMother.SimpleObject());

            // Act
            Action act = () => builder.WithExternalChromeProcess(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task BuildAsync_ShouldPassEncodeHtmlFalseToTemplateEngine_ByDefault()
        {
            // Arrange
            IChromiumRetriever retriever = ChromiumRetrieverMock.CreateWithSinglePagePdf(out _, out _);

            FluidPDFTemplateRenderOptions? capturedOptions = null;
            IFluidPDFTemplateEngine engine = Substitute.For<IFluidPDFTemplateEngine>();
            engine
                .RenderTemplateAsync(Arg.Any<string>(), Arg.Any<FluidPDFTemplateModel[]>(), Arg.Any<FluidPDFTemplateRenderOptions>(), Arg.Any<string>())
                .Returns(callInfo =>
                {
                    capturedOptions = callInfo.Arg<FluidPDFTemplateRenderOptions>();
                    return new ValueTask<string>("<p>Alice is 30</p>");
                });

            using FluidPDFBuilder builder = new(retriever);
            builder.WithObjectModel(TemplateModelMother.SimpleObject());
            builder.WithTemplate(TemplateModelMother.SimpleTemplate);
            builder.WithTemplateEngine(engine);

            // Act
            await builder.BuildAsync();

            // Assert
            capturedOptions.Should().NotBeNull();
            capturedOptions!.EncodeHtml.Should().BeFalse();
        }

        [Fact]
        public async Task BuildAsync_ShouldPassEncodeHtmlTrueToTemplateEngine_WhenWithHtmlEncodeIsCalled()
        {
            // Arrange
            IChromiumRetriever retriever = ChromiumRetrieverMock.CreateWithSinglePagePdf(out _, out _);

            FluidPDFTemplateRenderOptions? capturedOptions = null;
            IFluidPDFTemplateEngine engine = Substitute.For<IFluidPDFTemplateEngine>();
            engine
                .RenderTemplateAsync(Arg.Any<string>(), Arg.Any<FluidPDFTemplateModel[]>(), Arg.Any<FluidPDFTemplateRenderOptions>(), Arg.Any<string>())
                .Returns(callInfo =>
                {
                    capturedOptions = callInfo.Arg<FluidPDFTemplateRenderOptions>();
                    return new ValueTask<string>("<p>Alice is 30</p>");
                });

            using FluidPDFBuilder builder = new(retriever);
            builder.WithObjectModel(TemplateModelMother.SimpleObject());
            builder.WithTemplate(TemplateModelMother.SimpleTemplate);
            builder.WithTemplateEngine(engine);
            builder.WithHtmlEncode();

            // Act
            await builder.BuildAsync();

            // Assert
            capturedOptions.Should().NotBeNull();
            capturedOptions!.EncodeHtml.Should().BeTrue();
        }

        [Fact]
        public async Task BuildAsync_StreamOverload_ShouldPassEncodeHtmlFalseToTemplateEngine_ByDefault()
        {
            // Arrange
            IChromiumRetriever retriever = ChromiumRetrieverMock.CreateWithSinglePagePdf(out _, out _);

            FluidPDFTemplateRenderOptions? capturedOptions = null;
            IFluidPDFTemplateEngine engine = Substitute.For<IFluidPDFTemplateEngine>();
            engine
                .RenderTemplateAsync(Arg.Any<string>(), Arg.Any<FluidPDFTemplateModel[]>(), Arg.Any<FluidPDFTemplateRenderOptions>(), Arg.Any<string>())
                .Returns(callInfo =>
                {
                    capturedOptions = callInfo.Arg<FluidPDFTemplateRenderOptions>();
                    return new ValueTask<string>("<p>Alice is 30</p>");
                });

            using FluidPDFBuilder builder = new(retriever);
            builder.WithObjectModel(TemplateModelMother.SimpleObject());
            builder.WithTemplate(TemplateModelMother.SimpleTemplate);
            builder.WithTemplateEngine(engine);

            // Act
            using MemoryStream stream = new();
            await builder.BuildAsync(stream);

            // Assert
            capturedOptions.Should().NotBeNull();
            capturedOptions!.EncodeHtml.Should().BeFalse();
        }

        [Fact]
        public async Task BuildAsync_StreamOverload_ShouldPassEncodeHtmlTrueToTemplateEngine_WhenWithHtmlEncodeIsCalled()
        {
            // Arrange
            IChromiumRetriever retriever = ChromiumRetrieverMock.CreateWithSinglePagePdf(out _, out _);

            FluidPDFTemplateRenderOptions? capturedOptions = null;
            IFluidPDFTemplateEngine engine = Substitute.For<IFluidPDFTemplateEngine>();
            engine
                .RenderTemplateAsync(Arg.Any<string>(), Arg.Any<FluidPDFTemplateModel[]>(), Arg.Any<FluidPDFTemplateRenderOptions>(), Arg.Any<string>())
                .Returns(callInfo =>
                {
                    capturedOptions = callInfo.Arg<FluidPDFTemplateRenderOptions>();
                    return new ValueTask<string>("<p>Alice is 30</p>");
                });

            using FluidPDFBuilder builder = new(retriever);
            builder.WithObjectModel(TemplateModelMother.SimpleObject());
            builder.WithTemplate(TemplateModelMother.SimpleTemplate);
            builder.WithTemplateEngine(engine);
            builder.WithHtmlEncode();

            // Act
            using MemoryStream stream = new();
            await builder.BuildAsync(stream);

            // Assert
            capturedOptions.Should().NotBeNull();
            capturedOptions!.EncodeHtml.Should().BeTrue();
        }

        [Fact]
        public async Task BuildAsync_ShouldPassCultureToTemplateEngine_WhenWithCultureCultureInfoIsCalled()
        {
            // Arrange
            IChromiumRetriever retriever = ChromiumRetrieverMock.CreateWithSinglePagePdf(out _, out _);

            FluidPDFTemplateRenderOptions? capturedOptions = null;
            IFluidPDFTemplateEngine engine = Substitute.For<IFluidPDFTemplateEngine>();
            engine
                .RenderTemplateAsync(Arg.Any<string>(), Arg.Any<FluidPDFTemplateModel[]>(), Arg.Any<FluidPDFTemplateRenderOptions>(), Arg.Any<string>())
                .Returns(callInfo =>
                {
                    capturedOptions = callInfo.Arg<FluidPDFTemplateRenderOptions>();
                    return new ValueTask<string>("<p>Alice is 30</p>");
                });

            DictionaryLocalizationProvider provider = new(
                new Dictionary<string, Dictionary<string, string>>
                {
                    ["en-US"] = new()
                    {
                        ["label_title"] = "Invoice"
                    }
                });

            using FluidPDFBuilder builder = new(retriever);
            builder.WithObjectModel(TemplateModelMother.SimpleObject());
            builder.WithTemplate(TemplateModelMother.SimpleTemplate);
            builder.WithTemplateEngine(engine);
            builder.WithLocalization(provider);
            builder.WithCulture(new CultureInfo("it-IT"));

            // Act
            await builder.BuildAsync();

            // Assert
            capturedOptions.Should().NotBeNull();
            capturedOptions!.CultureInfo.Should().NotBeNull();
            capturedOptions.CultureInfo!.Name.Should().Be("it-IT");
        }

        [Fact]
        public void WithLocalization_ShouldThrowArgumentNullException_WhenProviderIsNull()
        {
            // Arrange
            IFluidPDFBuilder builder = Builder.FluidPDF.NewReport().WithObjectModel(TemplateModelMother.SimpleObject());

            // Act
            Action act = () => builder.WithLocalization(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task BuildAsync_ShouldThrowFluidPDFBuilderConfigException_WhenNoModelIsSet()
        {
            // Arrange
            IFluidPDFBuilder builder = Builder.FluidPDF.NewReport().WithTemplate(TemplateModelMother.SimpleTemplate);

            // Act
            Func<Task> act = builder.BuildAsync;

            // Assert
            await act.Should().ThrowAsync<FluidPDFBuilderConfigException>();
        }

        [Fact]
        public async Task BuildAsync_ShouldThrowFluidPDFBuilderConfigException_WhenWithModelsIsCalledWithEmptyArray()
        {
            // Arrange
            IFluidPDFBuilder builder = Builder.FluidPDF.NewReport().WithTemplate(TemplateModelMother.SimpleTemplate);
            builder.WithModels([]);

            // Act
            Func<Task> act = builder.BuildAsync;

            // Assert
            await act.Should().ThrowAsync<FluidPDFBuilderConfigException>();
        }

        [Fact]
        public void WithModel_ShouldThrowArgumentNullException_WhenNullIsPassed()
        {
            // Arrange
            IFluidPDFBuilder builder = Builder.FluidPDF.NewReport();

            // Act
            Action act = () => builder.WithModel(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WithModels_ShouldThrowArgumentNullException_WhenNullIsPassed()
        {
            // Arrange
            IFluidPDFBuilder builder = Builder.FluidPDF.NewReport();

            // Act
            Action act = () => builder.WithModels(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task BuildAsync_ShouldPassModelsToTemplateEngine_WhenWithModelsIsCalled()
        {
            // Arrange
            IChromiumRetriever retriever = ChromiumRetrieverMock.CreateWithSinglePagePdf(out _, out _);

            FluidPDFTemplateModel[]? capturedModels = null;
            IFluidPDFTemplateEngine engine = Substitute.For<IFluidPDFTemplateEngine>();
            engine
                .RenderTemplateAsync(Arg.Any<string>(), Arg.Any<FluidPDFTemplateModel[]>(), Arg.Any<FluidPDFTemplateRenderOptions>(), Arg.Any<string>())
                .Returns(callInfo =>
                {
                    capturedModels = callInfo.Arg<FluidPDFTemplateModel[]>();
                    return new ValueTask<string>(TemplateModelMother.TwoModelExpectedOutput);
                });

            using FluidPDFBuilder builder = new(retriever);
            builder.WithModels(TemplateModelMother.TwoModelArray());
            builder.WithTemplate(TemplateModelMother.TwoModelTemplate);
            builder.WithTemplateEngine(engine);

            // Act
            await builder.BuildAsync();

            // Assert
            capturedModels.Should().NotBeNull();
            capturedModels!.Length.Should().Be(2);
        }

        [Fact]
        public async Task BuildAsync_ShouldPassSingleModelToTemplateEngine_WhenWithModelIsCalled()
        {
            // Arrange
            IChromiumRetriever retriever = ChromiumRetrieverMock.CreateWithSinglePagePdf(out _, out _);

            FluidPDFTemplateModel[]? capturedModels = null;
            IFluidPDFTemplateEngine engine = Substitute.For<IFluidPDFTemplateEngine>();
            engine
                .RenderTemplateAsync(Arg.Any<string>(), Arg.Any<FluidPDFTemplateModel[]>(), Arg.Any<FluidPDFTemplateRenderOptions>(), Arg.Any<string>())
                .Returns(callInfo =>
                {
                    capturedModels = callInfo.Arg<FluidPDFTemplateModel[]>();
                    return new ValueTask<string>(TemplateModelMother.SimpleObjectExpectedOutput);
                });

            using FluidPDFBuilder builder = new(retriever);
            builder.WithModel(FluidPDFTemplateModel.FromObject(TemplateModelMother.SimpleObject()));
            builder.WithTemplate(TemplateModelMother.SimpleTemplate);
            builder.WithTemplateEngine(engine);

            // Act
            await builder.BuildAsync();

            // Assert
            capturedModels.Should().NotBeNull();
            capturedModels!.Length.Should().Be(1);
            capturedModels[0].IsObject.Should().BeTrue();
        }
    }
}
