using FluidPDF.Templating;
using System.IO;
using System.Threading.Tasks;
namespace FluidPDF.Builder
{
    public interface IFluidPDFBuilder
    {
        IFluidPDFBuilder WithTemplateEngine(IFluidPDFTemplateEngine templateEngine);
        IFluidPDFBuilder WithExternalChromeProcess(string chromeExePath);
        IFluidPDFBuilder WithLandscapeOrientation();
        IFluidPDFBuilder WithA2Format();
        IFluidPDFBuilder WithA3Format();
        IFluidPDFBuilder WithA5Format();
        IFluidPDFBuilder WithA6Format();
        IFluidPDFBuilder WithPixelMargin(decimal margin);
        IFluidPDFBuilder WithPixelMargin(decimal bottom, decimal left, decimal right, decimal top);
        IFluidPDFBuilder WithInchMargin(decimal margin);
        IFluidPDFBuilder WithInchMargin(decimal bottom, decimal left, decimal right, decimal top);
        IFluidPDFBuilder WithScalePercentage(int scale);
        IFluidPDFBuilder WithCulture(string cultureCode);
        IFluidPDFBuilder WithTemplate(string template);
        IFluidPDFBuilder WithTemplateFile(string filePath);
        IFluidPDFBuilder WithPDFCompression();
        Task<byte[]> BuildAsync();
        Task BuildAsync(Stream stream);
    }
}