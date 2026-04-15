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
        private readonly ILocalizationProvider? _localizationProvider;

        public FluidPDFReportFactory(IFluidPDFTemplateEngine templateEngine, ChromiumRetrieverOptions chromiumRetrieverOptions, ILocalizationProvider? localizationProvider = null)
            : this(templateEngine, new ChromiumRetriever(chromiumRetrieverOptions), localizationProvider)
        {
        }

        internal FluidPDFReportFactory(IFluidPDFTemplateEngine templateEngine, IChromiumRetriever chromiumRetriever, ILocalizationProvider? localizationProvider = null)
        {
            _templateEngine = templateEngine;
            _chromiumRetriever = chromiumRetriever ?? throw new ArgumentNullException(nameof(chromiumRetriever));
            _localizationProvider = localizationProvider;
        }

        /// <summary>
        /// The implementation of PuppeteerSharp generates the PDF file as a byte array, the stream method is just a wrapper.
        /// That's why the main method here returns a byte array
        /// </summary>
        public Task<byte[]> CompileReportAsync(string template, FluidPDFTemplateModel model, FluidPDFReportOptions reportOptions)
            => CompileReportAsync(template, [model], reportOptions);

        public async Task<byte[]> CompileReportAsync(string template, FluidPDFTemplateModel[] models, FluidPDFReportOptions reportOptions)
        {
            reportOptions.GetNonNullOrThrow(nameof(reportOptions));

            if (_localizationProvider is null && reportOptions.CultureInfo is not null)
            {
                throw new FluidPDFMissingLocalizationProviderException("Culture was provided, but no localization provider was configured.");
            }

            FluidPDFTemplateRenderOptions renderOptions = new()
            {
                CultureInfo = reportOptions.CultureInfo,
                EncodeHtml = reportOptions.EncodeHtml
            };

            FluidPDFTemplateModel? resxModel = await BuildResxModelAsync(reportOptions.CultureInfo).ConfigureAwait(false);

            string reportContent;
            try
            {
                FluidPDFTemplateModel[] combined = resxModel is not null ? [.. models, resxModel] : models;
                reportContent = await _templateEngine.RenderTemplateAsync(template, combined, renderOptions).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new FluidPDFTemplateRenderException("An error occurred in rendering the template", ex);
            }

            PdfOptions pdfOptions = new()
            {
                PreferCSSPageSize = true,
                PrintBackground = true,
                Format = reportOptions.Format,
                Landscape = reportOptions.Landscape,
                MarginOptions = reportOptions.MarginOptions,
                Scale = Math.Min(Math.Max(reportOptions.Scale / 100M, 0.1M), 2)
            };

            using IBrowser browser = await _chromiumRetriever.LaunchBrowserAsync().ConfigureAwait(false);
            using IPage page = await browser.NewPageAsync().ConfigureAwait(false);

            try
            {
                await page.SetContentAsync(reportContent).ConfigureAwait(false);

                byte[] data = await page.PdfDataAsync(pdfOptions).ConfigureAwait(false);

                if (reportOptions.ToBeCompressed)
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

        public Task CompileReportAsync(string template, FluidPDFTemplateModel model, Stream destinationStream, FluidPDFReportOptions reportOptions)
            => CompileReportAsync(template, [model], destinationStream, reportOptions);

        public async Task CompileReportAsync(string template, FluidPDFTemplateModel[] models, Stream destinationStream, FluidPDFReportOptions reportOptions)
        {
            byte[] data = await CompileReportAsync(template, models, reportOptions).ConfigureAwait(false);
            await FileHelper.WriteStreamAsync(destinationStream, data).ConfigureAwait(false);
        }

        private async ValueTask<FluidPDFTemplateModel?> BuildResxModelAsync(CultureInfo? cultureInfo)
        {
            if (_localizationProvider is null)
            {
                return null;
            }

            Dictionary<string, string> localizedStrings = await LocalizationResolver.ResolveResourcesAsync(_localizationProvider, cultureInfo).ConfigureAwait(false);
            Dictionary<string, object?> resxData = localizedStrings.ToDictionary(
                item => item.Key,
                item => (object?)(item.Value as object),
                StringComparer.Ordinal);

            return FluidPDFTemplateModel.FromDictionary(resxData, ModelNames.ResxModelName);
        }
    }

    public sealed class FluidPDFReportOptions
    {
        public PaperFormat Format { get; set; } = PaperFormat.A4;
        public bool Landscape { get; set; }
        public MarginOptions MarginOptions { get; set; } = new() { Bottom = "0.3 in", Left = "0.3 in", Right = "0.3 in", Top = "0.3 in" };
        public int Scale { get; set; } = 100;
        public bool ToBeCompressed { get; set; }
        public CultureInfo? CultureInfo { get; set; }
        public bool EncodeHtml { get; set; }
    }
}
