using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;

namespace FluidPDF.Templating
{
    public interface IFluidPDFTemplateEngine
    {
        ValueTask<string> RenderTemplateAsync(string template, DataTable model, FluidPDFTemplateRenderOptions options, string modelName = ModelNames.DefaultModelName);
        ValueTask<string> RenderTemplateAsync(string template, IDictionary<string, object> model, FluidPDFTemplateRenderOptions options, string modelName = ModelNames.DefaultModelName);
        ValueTask<string> RenderTemplateAsync(string template, string jsonModel, FluidPDFTemplateRenderOptions options, string modelName = ModelNames.DefaultModelName);
        ValueTask<string> RenderTemplateAsync(string template, object model, FluidPDFTemplateRenderOptions options, string modelName = ModelNames.DefaultModelName);
        ValueTask<string> RenderTemplateAsync(string template, FluidPDFTemplateModel[] models, FluidPDFTemplateRenderOptions options, string modelName = ModelNames.DefaultModelName);
    }

    public record FluidPDFTemplateRenderOptions
    {
        public CultureInfo? CultureInfo { get; init; }
        public bool EncodeHtml { get; init; }
    }
}
