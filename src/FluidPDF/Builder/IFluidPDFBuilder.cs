using FluidPDF.Templating;
using FluidPDF.Templating.Localization;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace FluidPDF.Builder
{
    public interface IFluidPDFBuilder
    {
        IFluidPDFBuilder WithDataRowModel(DataRow dataRow, string modelName = ModelNames.DefaultModelName);
        IFluidPDFBuilder WithDataTableModel(DataTable dataTable, string modelName = ModelNames.DefaultModelName);
        IFluidPDFBuilder WithDictionaryModel(IDictionary<string, object> dictionary, string modelName = ModelNames.DefaultModelName);
        IFluidPDFBuilder WithJsonStringModel(string jsonString, string modelName = ModelNames.DefaultModelName);
        IFluidPDFBuilder WithObjectModel(object obj, string modelName = ModelNames.DefaultModelName);
        IFluidPDFBuilder WithTemplateEngine(IFluidPDFTemplateEngine templateEngine);
        IFluidPDFBuilder WithExternalChromeProcess(string chromeExePath);
        IFluidPDFBuilder WithLandscapeOrientation();
        IFluidPDFBuilder WithHtmlEncode();
        IFluidPDFBuilder WithA2Format();
        IFluidPDFBuilder WithA3Format();
        IFluidPDFBuilder WithA5Format();
        IFluidPDFBuilder WithA6Format();
        IFluidPDFBuilder WithPixelMargin(decimal margin);
        IFluidPDFBuilder WithPixelMargin(decimal bottom, decimal left, decimal right, decimal top);
        IFluidPDFBuilder WithInchMargin(decimal margin);
        IFluidPDFBuilder WithInchMargin(decimal bottom, decimal left, decimal right, decimal top);
        IFluidPDFBuilder WithScalePercentage(int scale);
        IFluidPDFBuilder WithLocalization(ILocalizationProvider provider);
        IFluidPDFBuilder WithCulture(CultureInfo culture);
        IFluidPDFBuilder WithCulture(string cultureCode);
        IFluidPDFBuilder WithTemplate(string template);
        IFluidPDFBuilder WithTemplateFile(string filePath);
        IFluidPDFBuilder WithPDFCompression();
        Task<byte[]> BuildAsync();
        Task BuildAsync(Stream stream);
    }
}