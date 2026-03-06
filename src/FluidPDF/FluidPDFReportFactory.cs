using FluidPDF.Fluid;
using FluidPDF.Support.PDF;
using FluidPDF.Support.PuppeteerSharp;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;

namespace FluidPDF
{
    public class FluidPDFReportFactory
    {
        private readonly IChromiumRetriever _chromiumRetriever;
        private readonly PdfOptions _pdfOptions;

        public FluidPDFReportFactory(ChromiumRetrieverOptions chromiumRetrieverOptions, FluidPDFReportOptions fluidPdfReportOptions)
            : this(new ChromiumRetriever(chromiumRetrieverOptions), fluidPdfReportOptions)
        {
        }

        internal FluidPDFReportFactory(IChromiumRetriever chromiumRetriever, FluidPDFReportOptions fluidPdfReportOptions)
        {
            _chromiumRetriever = chromiumRetriever ?? throw new ArgumentNullException(nameof(chromiumRetriever));
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
            string reportContent = await FluidTemplateHelper.RenderTemplateByTypeAsync(template, model, cultureInfo: cultureInfo, encodeHtml: true).ConfigureAwait(false);

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
            byte[] data = await CompileReportAsync<T>(template, model, toBeCompressed, cultureInfo).ConfigureAwait(false);
#if NETSTANDARD2_0
            await destinationStream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
#else
            await destinationStream.WriteAsync(data).ConfigureAwait(false);
#endif
        }
    }

    public class FluidPDFReportOptions
    {
        public PaperFormat Format { get; set; } = PaperFormat.A4;
        public bool Landscape { get; set; } = false;
        public MarginOptions MarginOptions { get; set; } = new MarginOptions { Bottom = "0.4 in", Left = "0.4 in", Right = "0.4 in", Top = "0.4 in" };
        public decimal Scale { get; set; } = 1M;
    }
}
