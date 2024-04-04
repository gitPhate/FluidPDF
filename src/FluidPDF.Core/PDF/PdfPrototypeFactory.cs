using FluidPDF.Core.KTemplating;
using FluidPDF.Core.PuppeteerSharp;
using KTemplating.Core.Support;
using Kyklos.Kernel.Core.Asserts;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace FluidPDF.Core.PDF
{
    internal class PdfPrototypeFactory
    {
        private readonly ChromiumRetrieverOptions _chromiumRetrieverOptions;
        private readonly PdfPrototypeFactoryOptions _fluidPdfOptions;
        private readonly KTemplateHelperWrapper _kTemplateHelperWrapper;

        internal PdfPrototypeFactory(ChromiumRetrieverOptions chromiumRetrieverOptions, PdfPrototypeFactoryOptions fluidPDFOptions, KTemplateHelperWrapper KTemplateHelperWrapper)
        {
            _chromiumRetrieverOptions = chromiumRetrieverOptions.GetNonNullOrThrow(nameof(chromiumRetrieverOptions));
            _fluidPdfOptions = fluidPDFOptions.GetNonNullOrThrow(nameof(fluidPDFOptions));
            _kTemplateHelperWrapper = KTemplateHelperWrapper.GetNonNullOrThrow(nameof(KTemplateHelperWrapper));
        }

        internal async Task<IPdfPrototype> NewPdfPrototypeAsync<T>(string template, T model, bool toBeCompressed)
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

        private ValueTask<string> RenderTemplateByTypeAsync<T>(string template, T model) =>
            model switch
            {
                DataRow => _kTemplateHelperWrapper.RenderTemplateWithDataRowAsync(template, (model as DataRow)!),
                DataTable => _kTemplateHelperWrapper.RenderTemplateWithDataTableAsync(template, (model as DataTable)!),
                IDictionary<string, object> => _kTemplateHelperWrapper.RenderTemplateWithDictionaryAsync(template, (model as IDictionary<string, object>)!),
                JObject => _kTemplateHelperWrapper.RenderTemplateWithJsonObjectAsync(template, (model as JObject)!),
                string => _kTemplateHelperWrapper.RenderTemplateWithJsonStringAsync(template, (model as string)!),
                IEnumerable<KTemplatingModel> => _kTemplateHelperWrapper.RenderTemplateWithMultipleModelsAsync(template, (model as IEnumerable<KTemplatingModel>)!),
                _ => throw new InvalidCastException("Invalid model type")
            };
    }
}
