using FluentAssertions;
using FluidPDF.Builder;
using FluidPDF.Exceptions;
using FluidPDF.Tests.Mothers;

namespace FluidPDF.Tests
{
    public class FluidPDFBuilderConfigTests
    {
        [Fact]
        public async Task BuildAsync_ShouldThrowFluidPDFBuilderConfigException_WhenNoTemplateIsSet()
        {
            // Arrange
            IFluidPDFBuilder builder = FluidPDFBuilder.NewWithModel(TemplateModelMother.SimpleObject())
                .WithStandaloneChromium();

            // Act
            Func<Task> act = builder.BuildAsync;

            // Assert
            await act.Should().ThrowAsync<FluidPDFBuilderConfigException>();
        }

        [Fact]
        public async Task BuildAsync_ShouldThrowFluidPDFBuilderConfigException_WhenNoChromiumIsSet()
        {
            // Arrange
            IFluidPDFBuilder builder = FluidPDFBuilder.NewWithModel(TemplateModelMother.SimpleObject())
                .WithTemplate(TemplateModelMother.SimpleObjectTemplate());

            // Act
            Func<Task> act = builder.BuildAsync;

            // Assert
            await act.Should().ThrowAsync<FluidPDFBuilderConfigException>();
        }

        [Fact]
        public async Task BuildAsync_ShouldThrowFluidPDFBuilderConfigException_WhenNeitherTemplateNorChromiumIsSet()
        {
            // Arrange
            IFluidPDFBuilder builder = FluidPDFBuilder.NewWithModel(TemplateModelMother.SimpleObject());

            // Act
            Func<Task> act = builder.BuildAsync;

            // Assert
            await act.Should().ThrowAsync<FluidPDFBuilderConfigException>();
        }

        [Fact]
        public void WithTemplateFile_ShouldThrowFileNotFoundException_WhenFilePathDoesNotExist()
        {
            // Arrange
            IFluidPDFBuilder builder = FluidPDFBuilder.NewWithModel(TemplateModelMother.SimpleObject());

            // Act
            Action act = () => builder.WithTemplateFile("C:\\nonexistent\\path\\template.html");

            // Assert
            act.Should().Throw<FileNotFoundException>();
        }

        [Fact]
        public void WithExternalChromeProcess_ShouldThrowArgumentNullException_WhenNullIsPassedAsPath()
        {
            // Arrange
            IFluidPDFBuilder builder = FluidPDFBuilder.NewWithModel(TemplateModelMother.SimpleObject());

            // Act
            Action act = () => builder.WithExternalChromeProcess(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
