using PuppeteerSharp.Media;

namespace FluidPDF.PDF
{
    internal class PdfPrototypeFactoryOptions
    {
        internal PaperFormat Format { get; set; } = PaperFormat.A4;
        internal bool Landscape { get; set; } = false;
        internal MarginOptions MarginOptions { get; set; } = new MarginOptions { Bottom = "0.4 in", Left = "0.4 in", Right = "0.4 in", Top = "0.4 in" };
        internal decimal Scale { get; set; } = 1M;
    }
}
