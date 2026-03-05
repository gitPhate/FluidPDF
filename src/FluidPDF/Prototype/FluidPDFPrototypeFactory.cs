using FluidPDF.Fluid;
using FluidPDF.PuppeteerSharp;
using FluidPDF.Support;
using PuppeteerSharp;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace FluidPDF.Prototype
{
    internal class FluidPDFPrototypeFactory
    {
        private readonly ChromiumRetrieverOptions _chromiumRetrieverOptions;
        private readonly FluidPDFPrototypeFactoryOptions _fluidPdfOptions;

        internal FluidPDFPrototypeFactory(ChromiumRetrieverOptions chromiumRetrieverOptions, FluidPDFPrototypeFactoryOptions fluidPDFOptions)
        {
            _chromiumRetrieverOptions = chromiumRetrieverOptions.GetNonNullOrThrow(nameof(chromiumRetrieverOptions));
            _fluidPdfOptions = fluidPDFOptions.GetNonNullOrThrow(nameof(fluidPDFOptions));
        }

        internal async Task<IFluidPDFPrototype> NewLazyAsync<T>(string template, T model, bool toBeCompressed, CultureInfo? cultureInfo = null)
            where T : notnull
        {
            string reportContent = await FluidTemplateHelper.RenderTemplateByTypeAsync(template, model, cultureInfo: cultureInfo, encodeHtml: true).ConfigureAwait(false);

            IBrowser browser = await ChromiumRetriever.RetrieveBrowserInstanceAsync(_chromiumRetrieverOptions).ConfigureAwait(false);
            IPage page = await browser.NewPageAsync().ConfigureAwait(false);

            await page.SetContentAsync(reportContent).ConfigureAwait(false);

            PdfOptions pdfOptions = NewPdfOptions();

            FluidPDFLazyPrototype prototype = new(reportContent, browser, page, pdfOptions, toBeCompressed);
            return prototype;
        }

        internal Task<IFluidPDFPrototype> NewEagerStreamAsync<T>(string template, T model, bool toBeCompressed, CultureInfo? cultureInfo = null)
            where T : notnull =>
            NewPrototypeAsync
            (
                async (page, options, content, compress) =>
                {
                    Stream stream = await page.PdfStreamAsync(options).ConfigureAwait(false);
                    return new FluidPDFEagerStreamPrototype(stream, content, compress);
                },
                template,
                model,
                toBeCompressed,
                cultureInfo
            );

        internal Task<IFluidPDFPrototype> NewEagerByteArrayAsync<T>(string template, T model, bool toBeCompressed, CultureInfo? cultureInfo = null)
            where T : notnull =>
            NewPrototypeAsync
            (
                async (page, options, content, compress) =>
                {
                    byte[] data = await page.PdfDataAsync(options).ConfigureAwait(false);
                    return new FluidPDFEagerByteArrayPrototype(data, content, compress);
                },
                template,
                model,
                toBeCompressed,
                cultureInfo
            );

        private async Task<IFluidPDFPrototype> NewPrototypeAsync<T>(Func<IPage, PdfOptions, string, bool, Task<IFluidPDFPrototype>> prototypeFactory, string template, T model, bool toBeCompressed, CultureInfo? cultureInfo = null)
            where T : notnull
        {
            string reportContent = await FluidTemplateHelper.RenderTemplateByTypeAsync(template, model, cultureInfo: cultureInfo, encodeHtml: true).ConfigureAwait(false);

            using IBrowser browser = await ChromiumRetriever.RetrieveBrowserInstanceAsync(_chromiumRetrieverOptions).ConfigureAwait(false);
            using IPage page = await browser.NewPageAsync().ConfigureAwait(false);

            try
            {
                await page.SetContentAsync(reportContent).ConfigureAwait(false);

                PdfOptions pdfOptions = NewPdfOptions();

                IFluidPDFPrototype prototype = await prototypeFactory(page, pdfOptions, reportContent, toBeCompressed).ConfigureAwait(false);
                return prototype;
            }
            finally
            {
                await page.CloseAsync().ConfigureAwait(false);
                await browser.CloseAsync().ConfigureAwait(false);
            }
        }

        private PdfOptions NewPdfOptions() =>
            new()
            {
                PreferCSSPageSize = true,
                PrintBackground = true,
                Format = _fluidPdfOptions.Format,
                Landscape = _fluidPdfOptions.Landscape,
                MarginOptions = _fluidPdfOptions.MarginOptions,
                Scale = _fluidPdfOptions.Scale
            };
    }
}
