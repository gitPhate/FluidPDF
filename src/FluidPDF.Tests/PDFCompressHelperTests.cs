using FluentAssertions;
using FluidPDF.Support.PDF;
using FluidPDF.Tests.Mothers;

namespace FluidPDF.Tests
{
    public class PDFCompressHelperTests
    {
        [Fact]
        public void CompressPDF_ShouldReturnValidPDF_WhenInputIsValidPDF()
        {
            // Arrange
            byte[] input = PDFDocumentMother.CreateSinglePagePDF();

            // Act
            byte[] result = PDFCompressHelper.CompressPDF(input);

            // Assert
            PDFDocumentMother.IsValidPDF(result).Should().BeTrue();
        }

        [Fact]
        public void CompressPDF_ShouldPreservePageCount_WhenInputHasMultiplePages()
        {
            // Arrange
            byte[] input = PDFDocumentMother.CreateMultiPagePDF(5);

            // Act
            byte[] result = PDFCompressHelper.CompressPDF(input);

            // Assert
            PDFDocumentMother.GetPageCount(result).Should().Be(5);
        }

        [Fact]
        public void CompressPDF_ShouldPreservePageText_WhenPagesContainText()
        {
            // Arrange
            byte[] input = PDFDocumentMother.CreateMultiPagePDFWithDuplicatedFonts(3, "FluidPDF");

            // Act
            byte[] result = PDFCompressHelper.CompressPDF(input);

            // Assert
            PDFDocumentMother.GetPagesText(result).Should().Equal("FluidPDF", "FluidPDF", "FluidPDF");
        }

        [Fact]
        public void CompressPDF_ShouldReduceSize_WhenPagesContainDuplicatedObjects()
        {
            // Arrange
            byte[] input = PDFDocumentMother.CreateMultiPagePDFWithDuplicatedFonts(50, "FluidPDF");

            // Act
            byte[] result = PDFCompressHelper.CompressPDF(input);

            // Assert
            result.Length.Should().BeLessThan(input.Length);
        }

        [Fact]
        public void CompressPDF_ShouldThrow_WhenInputIsNotPDF()
        {
            // Arrange
            byte[] input = [0x01, 0x02, 0x03];

            // Act
            Action act = () => PDFCompressHelper.CompressPDF(input);

            // Assert
            act.Should().Throw<Exception>();
        }
    }
}
