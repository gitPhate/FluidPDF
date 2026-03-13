using System.IO;
using System.Threading.Tasks;

namespace FluidPDF.Builder
{
    public interface IFluidPDFBuilder
    {
        IFluidPDFBuilder WithExternalChromeProcess(string chromeExePath);
        IFluidPDFBuilder WithStandaloneChromium();
        IFluidPDFBuilder WithLandscapeOrientation();
        IFluidPDFBuilder WithA2Format();
        IFluidPDFBuilder WithA3Format();
        IFluidPDFBuilder WithA5Format();
        IFluidPDFBuilder WithA6Format();
        IFluidPDFBuilder WithPixelMargin(decimal margin);
        IFluidPDFBuilder WithPixelMargin(decimal bottom, decimal left, decimal right, decimal top);
        IFluidPDFBuilder WithInchMargin(decimal margin);
        IFluidPDFBuilder WithInchMargin(decimal bottom, decimal left, decimal right, decimal top);
        IFluidPDFBuilder WithCustomScalePercentage(int scale);
        IFluidPDFBuilder WithCulture(string cultureCode);
        IFluidPDFBuilder WithTemplate(string template);
        IFluidPDFBuilder WithTemplateFile(string filePath);
        IFluidPDFBuilder WithCompression();
        Task<byte[]> BuildAsync();
        Task BuildAsync(Stream stream);
    }
}