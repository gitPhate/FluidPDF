using System;
using System.Globalization;
using System.Threading.Tasks;

namespace FluidPDF.Templating
{
    public interface IFluidPDFTemplateEngine
    {
        ValueTask<string> RenderTemplateAsync<T>(string template, T model, FluidPDFTemplateRenderOptions options) where T : notnull;
    }

    public record FluidPDFTemplateRenderOptions
    {
        public string ModelName { get; init; } = "Model";
        public CultureInfo? CultureInfo { get; init; }
    }
}
