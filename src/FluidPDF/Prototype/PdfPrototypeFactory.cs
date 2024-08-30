using FluidPDF.Fluid;
using FluidPDF.PuppeteerSharp;
using Kyklos.Kernel.Core.Asserts;
using PuppeteerSharp;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace FluidPDF.Prototype
{
    internal class PdfPrototypeFactory
    {
        private readonly ChromiumRetrieverOptions _chromiumRetrieverOptions;
        private readonly PdfPrototypeFactoryOptions _fluidPdfOptions;

        internal PdfPrototypeFactory(ChromiumRetrieverOptions chromiumRetrieverOptions, PdfPrototypeFactoryOptions fluidPDFOptions)
        {
            _chromiumRetrieverOptions = chromiumRetrieverOptions.GetNonNullOrThrow(nameof(chromiumRetrieverOptions));
            _fluidPdfOptions = fluidPDFOptions.GetNonNullOrThrow(nameof(fluidPDFOptions));
        }

        internal async Task<IPdfPrototype> NewPdfPrototypeAsync<T>(string template, T model, bool toBeCompressed)
            where T : notnull
        {
            string renderedTemplate = await RenderTemplateByTypeAsync(template, model).ConfigureAwait(false);

            IBrowser browser = await ChromiumRetriever.RetrieveBrowserInstanceAsync(_chromiumRetrieverOptions).ConfigureAwait(false);
            IPage page = await browser.NewPageAsync().ConfigureAwait(false);

            await page.SetContentAsync(renderedTemplate).ConfigureAwait(false);

            PdfOptions pdfOptions =
                new()
                {
                    PreferCSSPageSize = true,
                    PrintBackground = true,
                    Format = _fluidPdfOptions.Format,
                    Landscape = _fluidPdfOptions.Landscape,
                    MarginOptions = _fluidPdfOptions.MarginOptions,
                    Scale = _fluidPdfOptions.Scale
                };

            PdfPrototype prototype = new(browser, page, pdfOptions, toBeCompressed);
            return prototype;
        }

        private ValueTask<string> RenderTemplateByTypeAsync<T>(string template, T model)
            where T : notnull =>
            model switch
            {
                DataRow => FluidHelper.RenderTemplateWithDataRowAsync(template, (model as DataRow)!),
                DataTable => FluidHelper.RenderTemplateWithDataTableAsync(template, (model as DataTable)!),
                Dictionary<string, object> => FluidHelper.RenderTemplateWithDictionaryAsync(template, (model as Dictionary<string, object>)!),
                string => FluidHelper.RenderTemplateWithJsonStringAsync(template, (model as string)!),
                FluidModel[] => FluidHelper.RenderTemplateWithMultipleModelsAsync(template, model as FluidModel[] ?? []),
                _ => FluidHelper.RenderTemplateWithObjectAsync(template, model)
            };
    }
}
