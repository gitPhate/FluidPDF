using FluidPDF.Exceptions;
using FluidPDF.Support.IO;
using FluidPDF.Support.PDF;
using FluidPDF.Support.PuppeteerSharp;
using FluidPDF.Templating;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace FluidPDF
{
    public sealed class FluidPDFReportFactory
    {
        private readonly IFluidPDFTemplateEngine _templateEngine;
        private readonly IChromiumRetriever _chromiumRetriever;
        private readonly PdfOptions _pdfOptions;

        public FluidPDFReportFactory(IFluidPDFTemplateEngine templateEngine, ChromiumRetrieverOptions chromiumRetrieverOptions, FluidPDFReportOptions fluidPdfReportOptions)
            : this(templateEngine, new ChromiumRetriever(chromiumRetrieverOptions), fluidPdfReportOptions)
        {
        }

        internal FluidPDFReportFactory(IFluidPDFTemplateEngine templateEngine, IChromiumRetriever chromiumRetriever, FluidPDFReportOptions fluidPdfReportOptions)
        {
            _templateEngine = templateEngine;
            _chromiumRetriever = chromiumRetriever ?? throw new ArgumentNullException(nameof(chromiumRetriever));

            if (fluidPdfReportOptions is null)
            {
                throw new ArgumentNullException(nameof(fluidPdfReportOptions));
            }

            if (fluidPdfReportOptions.Scale < 0.1M || fluidPdfReportOptions.Scale > 2.0M)
            {
                throw new ArgumentOutOfRangeException(nameof(fluidPdfReportOptions), fluidPdfReportOptions.Scale, "Scale must be between 0.1 and 2.0.");
            }

            _pdfOptions = new PdfOptions()
            {
                PreferCSSPageSize = true,
                PrintBackground = true,
                Format = fluidPdfReportOptions.Format,
                Landscape = fluidPdfReportOptions.Landscape,
                MarginOptions = fluidPdfReportOptions.MarginOptions,
                Scale = fluidPdfReportOptions.Scale
            };
        }

        /// <summary>
        /// The implementation of PuppeteerSharp generates the PDF file as a byte array, the stream method is just a wrapper.
        /// That's why the main method here returns a byte array
        /// </summary>
        public async Task<byte[]> CompileReportAsync<T>(string template, T model, bool toBeCompressed = false, CultureInfo? cultureInfo = null)
            where T : notnull
        {
            FluidPDFTemplateRenderOptions options = new()
            {
                CultureInfo = cultureInfo
            };

            string reportContent;
            try
            {
                reportContent = await _templateEngine.RenderTemplateAsync(template, model, options).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new FluidPDFTemplateRenderException("An error occurred in rendering the template", ex);
            }

            using IBrowser browser = await _chromiumRetriever.RetrieveBrowserInstanceAsync().ConfigureAwait(false);
            using IPage page = await browser.NewPageAsync().ConfigureAwait(false);

            try
            {
                await page.SetContentAsync(reportContent).ConfigureAwait(false);

                byte[] data = await page.PdfDataAsync(_pdfOptions).ConfigureAwait(false);

                if (toBeCompressed)
                {
                    data = await PDFCompressHelper.CompressPDFAsync(data).ConfigureAwait(false);
                }

                return data;
            }
            finally
            {
                await page.CloseAsync().ConfigureAwait(false);
                await browser.CloseAsync().ConfigureAwait(false);
            }
        }

        public async Task CompileReportAsync<T>(string template, T model, Stream destinationStream, bool toBeCompressed = false, CultureInfo? cultureInfo = null)
            where T : notnull
        {
            byte[] data = await CompileReportAsync(template, model, toBeCompressed, cultureInfo).ConfigureAwait(false);
            await AsyncFile.WriteStreamAsync(destinationStream, data).ConfigureAwait(false);
        }
    }

    public sealed class FluidPDFReportOptions
    {
        public PaperFormat Format { get; set; } = PaperFormat.A4;
        public bool Landscape { get; set; }
        public MarginOptions MarginOptions { get; set; } = new MarginOptions { Bottom = "0.4 in", Left = "0.4 in", Right = "0.4 in", Top = "0.4 in" };
        public decimal Scale { get; set; } = 1M;
    }
}
