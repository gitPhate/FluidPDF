using FluidPDF.Prototype;
using System.Threading.Tasks;

namespace FluidPDF.Builder
{
    public interface IFluidPDFBuilder
    {
        IFluidPDFBuilder WithExternalChromeProcess(string chromeExePath);
        IFluidPDFBuilder WithStandaloneChromium();
        IFluidPDFBuilder WithLanscapeOrientation();
        IFluidPDFBuilder WithA2Format();
        IFluidPDFBuilder WithA3Format();
        IFluidPDFBuilder WithA5Format();
        IFluidPDFBuilder WithA6Format();
        //IFluidPDFBuilder WithPixelMargin(double bottom, double left, double right, double top);
        //IFluidPDFBuilder WithPixelMargin(double margin);
        //IFluidPDFBuilder WithInchMargin(double bottom, double left, double right, double top);
        //IFluidPDFBuilder WithInchMargin(double margin);
        IFluidPDFBuilder WithCustomMargin(FluidPDFMargins margins);
        IFluidPDFBuilder WithCustomScalePercentage(int scale);
        IFluidPDFBuilder WithCulture(string cultureCode);
        IFluidPDFBuilder WithTemplate(string template);
        IFluidPDFBuilder WithTemplateFile(string filePath);
        IFluidPDFBuilder WithCompression();
        Task<IPdfPrototype> BuildAsync();
    }
}