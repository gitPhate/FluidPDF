using FluentAssertions;
using FluidPDF.Builder;
using FluidPDF.Tests.Mothers;
using PuppeteerSharp.Media;

namespace FluidPDF.Tests
{
    public class FluidPDFBuilderOptionsTests
    {
        [Fact]
        public void NewFluidPDFReportOptions_ShouldDefaultToA4Portrait_WhenNoFormatOrOrientationIsSet()
        {
            // Arrange
            FluidPDFBuilder<object> builder = new(TemplateModelMother.SimpleObject());

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
            FluidPDFBuilder<object> builder = new(TemplateModelMother.SimpleObject());
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
            FluidPDFBuilder<object> builder = new(TemplateModelMother.SimpleObject());
            builder.WithCustomScalePercentage(1);

            // Act
            FluidPDFReportOptions options = builder.NewFluidPDFReportOptions();

            // Assert
            options.Scale.Should().Be(0.1M);
        }

        [Fact]
        public void NewFluidPDFReportOptions_ShouldClampScaleToMaximum_WhenScalePercentageIsAboveTwoHundred()
        {
            // Arrange
            FluidPDFBuilder<object> builder = new(TemplateModelMother.SimpleObject());
            builder.WithCustomScalePercentage(300);

            // Act
            FluidPDFReportOptions options = builder.NewFluidPDFReportOptions();

            // Assert
            options.Scale.Should().Be(2.0M);
        }

        [Fact]
        public void NewFluidPDFReportOptions_ShouldSetEmptyMargins_WhenWithCustomMarginNoneIsCalled()
        {
            // Arrange
            FluidPDFBuilder<object> builder = new(TemplateModelMother.SimpleObject());
            builder.WithCustomMargin(FluidPDFMargins.None);

            // Act
            FluidPDFReportOptions options = builder.NewFluidPDFReportOptions();

            // Assert
            ((string?)options.MarginOptions.Bottom).Should().BeNullOrEmpty();
            ((string?)options.MarginOptions.Left).Should().BeNullOrEmpty();
            ((string?)options.MarginOptions.Right).Should().BeNullOrEmpty();
            ((string?)options.MarginOptions.Top).Should().BeNullOrEmpty();
        }
    }
}
