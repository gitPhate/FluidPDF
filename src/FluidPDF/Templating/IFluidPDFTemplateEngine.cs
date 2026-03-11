using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;

namespace FluidPDF.Templating
{
    public interface IFluidPDFTemplateEngine
    {
        ValueTask<string> RenderTemplateAsync(string template, DataTable model, FluidPDFTemplateRenderOptions options);
        ValueTask<string> RenderTemplateAsync(string template, IDictionary<string, object> model, FluidPDFTemplateRenderOptions options);
        ValueTask<string> RenderTemplateAsync(string template, object model, FluidPDFTemplateRenderOptions options);
        ValueTask<string> RenderTemplateAsync(string template, FluidPDFTemplateModel[] models, FluidPDFTemplateRenderOptions options);
    }

    public record FluidPDFTemplateRenderOptions
    {
        public string ModelName { get; init; } = "Model";
        public CultureInfo? CultureInfo { get; init; }
    }
}
