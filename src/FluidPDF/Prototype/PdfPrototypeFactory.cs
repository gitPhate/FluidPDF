using FluidPDF.Fluid;
using FluidPDF.PuppeteerSharp;
using FluidPDF.Support;
using PuppeteerSharp;
using System.Globalization;
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

        internal async Task<IPdfPrototype> NewPdfPrototypeAsync<T>(string template, T model, bool toBeCompressed, CultureInfo? cultureInfo = null)
            where T : notnull
        {
            string reportContent = await FluidTemplateHelper.RenderTemplateByTypeAsync(template, model, cultureInfo: cultureInfo, encodeHtml: true).ConfigureAwait(false);

            IBrowser browser = await ChromiumRetriever.RetrieveBrowserInstanceAsync(_chromiumRetrieverOptions).ConfigureAwait(false);
            IPage page = await browser.NewPageAsync().ConfigureAwait(false);

            await page.SetContentAsync(reportContent).ConfigureAwait(false);

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

            PdfPrototype prototype = new(reportContent, browser, page, pdfOptions, toBeCompressed);
            return prototype;
        }
    }
}
