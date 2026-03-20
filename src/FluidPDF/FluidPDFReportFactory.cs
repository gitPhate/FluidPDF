using FluidPDF.Exceptions;
using FluidPDF.Support;
using FluidPDF.Support.IO;
using FluidPDF.Support.PDF;
using FluidPDF.Support.PuppeteerSharp;
using FluidPDF.Templating;
using FluidPDF.Templating.Localization;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FluidPDF
{
    public sealed class FluidPDFReportFactory
    {
        private readonly IFluidPDFTemplateEngine _templateEngine;
        private readonly IChromiumRetriever _chromiumRetriever;
        private readonly PdfOptions _pdfOptions;
        private readonly ILocalizationProvider? _localizationProvider;

        public FluidPDFReportFactory(IFluidPDFTemplateEngine templateEngine, ChromiumRetrieverOptions chromiumRetrieverOptions, FluidPDFReportOptions fluidPdfReportOptions, ILocalizationProvider? localizationProvider = null)
            : this(templateEngine, new ChromiumRetriever(chromiumRetrieverOptions), fluidPdfReportOptions, localizationProvider)
        {
        }

        internal FluidPDFReportFactory(IFluidPDFTemplateEngine templateEngine, IChromiumRetriever chromiumRetriever, FluidPDFReportOptions fluidPdfReportOptions, ILocalizationProvider? localizationProvider = null)
        {
            _templateEngine = templateEngine;
            _chromiumRetriever = chromiumRetriever ?? throw new ArgumentNullException(nameof(chromiumRetriever));
            _localizationProvider = localizationProvider;

            _pdfOptions = new PdfOptions()
            {
                PreferCSSPageSize = true,
                PrintBackground = true,
                Format = fluidPdfReportOptions.GetNonNullOrThrow(nameof(fluidPdfReportOptions)).Format,
                Landscape = fluidPdfReportOptions.Landscape,
                MarginOptions = fluidPdfReportOptions.MarginOptions,
                Scale = fluidPdfReportOptions.Scale
            };
        }

        /// <summary>
        /// The implementation of PuppeteerSharp generates the PDF file as a byte array, the stream method is just a wrapper.
        /// That's why the main method here returns a byte array
        /// </summary>
        public async Task<byte[]> CompileReportAsync(string template, FluidPDFTemplateModel model, bool toBeCompressed = false, CultureInfo? cultureInfo = null, bool encodeHtml = false)
        {
            if (_localizationProvider is null && cultureInfo is not null)
            {
                throw new FluidPDFMissingLocalizationProviderException("Culture was provided, but no localization provider was configured.");
            }

            FluidPDFTemplateRenderOptions options = new()
            {
                CultureInfo = cultureInfo,
                EncodeHtml = encodeHtml
            };

            FluidPDFTemplateModel? resxModel = await BuildResxModelAsync(cultureInfo).ConfigureAwait(false);

            string reportContent;
            try
            {
                FluidPDFTemplateModel[] models = resxModel is not null ? [model, resxModel] : [model];
                reportContent = await _templateEngine.RenderTemplateAsync(template, models, options).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new FluidPDFTemplateRenderException("An error occurred in rendering the template", ex);
            }

            using IBrowser browser = await _chromiumRetriever.LaunchBrowserAsync().ConfigureAwait(false);
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

        public async Task CompileReportAsync(string template, FluidPDFTemplateModel model, Stream destinationStream, bool toBeCompressed = false, CultureInfo? cultureInfo = null, bool encodeHtml = false)
        {
            byte[] data = await CompileReportAsync(template, model, toBeCompressed, cultureInfo, encodeHtml).ConfigureAwait(false);
            await FileHelper.WriteStreamAsync(destinationStream, data).ConfigureAwait(false);
        }

        private async ValueTask<FluidPDFTemplateModel?> BuildResxModelAsync(CultureInfo? cultureInfo)
        {
            if (_localizationProvider is null)
            {
                return null;
            }

            Dictionary<string, string> localizedStrings = await LocalizationResolver.ResolveResourcesAsync(_localizationProvider, cultureInfo).ConfigureAwait(false);
            Dictionary<string, object> resxData = localizedStrings.ToDictionary(
                item => item.Key,
                item => (object)item.Value,
                StringComparer.Ordinal);

            return FluidPDFTemplateModel.FromDictionary(resxData, ModelNames.ResxModelName);
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
