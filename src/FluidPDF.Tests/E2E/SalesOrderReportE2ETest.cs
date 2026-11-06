using FluentAssertions;
using FluidPDF.Razor;
using FluidPDF.Support.IO;

namespace FluidPDF.Tests.E2E
{
    [Trait("Category", "E2E")]
    public class SalesOrderReportE2ETest : E2ETestBase
    {
        [Fact(Skip = "Manual E2E: requires real Chrome. Remove Skip to run.")]
        public async Task SalesOrder_ShouldProducePDF_WithRealChromium()
        {
            //Arrange
            string json = await FileHelper.ReadAllTextAsync("E2E/Data/SalesOrder.json");
            string template = await FileHelper.ReadAllTextAsync("E2E/Templates/SalesOrder_Razor.cshtml");

            //Act
            byte[] pdfBytes =
                await Builder.FluidPDF
                    .NewReport()
                    .WithExternalChromeProcess("C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe")
                    .WithRazorTemplateEngine()
                    .WithTemplate(template)
                    .WithJsonStringModel(json)
                    .WithInchMargin(0.5M, 0.5M, 0.5M, 0.5M)
                    .BuildAsync();

            string outPath = Path.Combine(OutputPath, "SalesOrder.pdf");
            await FileHelper.WriteAllBytesAsync(outPath, pdfBytes);

            //Assert
            pdfBytes.Should().NotBeEmpty();
            pdfBytes.Length.Should().BeGreaterThan(1_000);
        }
    }
}
