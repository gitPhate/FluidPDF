using FluidPDF.Exceptions;
using FluidPDF.Fluid;
using FluidPDF.Support;
using FluidPDF.Support.IO;
using FluidPDF.Support.PuppeteerSharp;
using FluidPDF.Templating;
using PuppeteerSharp.Media;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace FluidPDF.Builder
{
    public static class FluidPDFBuilder
    {
        public static IFluidPDFBuilder NewWithModel<T>(T model) where T : notnull => new FluidPDFBuilder<T>(model);
    }

    internal class FluidPDFBuilder<T> : IFluidPDFBuilder
        where T : notnull
    {
        private const string _standaloneChromePath = "standalone";

        private string? _chromeExePath;
        private bool _landscape;
        private PaperFormat _paperFormat;
        private MarginOptions _marginOptions;
        private int _scale;
        private CultureInfo? _cultureInfo;
        private string? _templateFilePath;
        private string? _template;
        private bool _toBeCompressed;
        private readonly T _model;
        private readonly IFluidPDFTemplateEngine _templateEngine;
        private readonly IChromiumRetriever? _chromiumRetriever;

        internal FluidPDFBuilder(T model) : this(model, null) { }

        internal FluidPDFBuilder(T model, IChromiumRetriever? chromiumRetriever)
        {
            _chromeExePath = null;
            _paperFormat = PaperFormat.A4;
            _landscape = false;
            _marginOptions = new MarginOptions { Bottom = "0.4 in", Left = "0.4 in", Right = "0.4 in", Top = "0.4 in" };
            _scale = 100;
            _cultureInfo = null;
            _toBeCompressed = false;
            _model = model;
            _templateEngine = new FluidTemplateEngine();
            _chromiumRetriever = chromiumRetriever;
        }

        public IFluidPDFBuilder WithExternalChromeProcess(string chromeExePath)
        {
            _chromeExePath = chromeExePath.GetNonNullOrThrow(nameof(chromeExePath));
            return this;
        }

        public IFluidPDFBuilder WithStandaloneChromium()
        {
            _chromeExePath = _standaloneChromePath;
            return this;
        }

        public IFluidPDFBuilder WithLandscapeOrientation()
        {
            _landscape = true;
            return this;
        }

        public IFluidPDFBuilder WithA2Format()
        {
            _paperFormat = PaperFormat.A2;
            return this;
        }

        public IFluidPDFBuilder WithA3Format()
        {
            _paperFormat = PaperFormat.A3;
            return this;
        }

        public IFluidPDFBuilder WithA5Format()
        {
            _paperFormat = PaperFormat.A5;
            return this;
        }

        public IFluidPDFBuilder WithA6Format()
        {
            _paperFormat = PaperFormat.A6;
            return this;
        }

        public IFluidPDFBuilder WithPixelMargin(decimal margin) =>
            WithPixelMargin(margin, margin, margin, margin);

        public IFluidPDFBuilder WithPixelMargin(decimal bottom, decimal left, decimal right, decimal top) =>
            WithMargin(bottom, left, right, top, "px");

        public IFluidPDFBuilder WithInchMargin(decimal margin) =>
            WithInchMargin(margin, margin, margin, margin);

        public IFluidPDFBuilder WithInchMargin(decimal bottom, decimal left, decimal right, decimal top) =>
            WithMargin(bottom, left, right, top, "in");

        private IFluidPDFBuilder WithMargin(decimal bottom, decimal left, decimal right, decimal top, string unit)
        {
            _marginOptions = new MarginOptions
            {
                Bottom = $"{bottom.ToString(CultureInfo.InvariantCulture)} {unit}",
                Left = $"{left.ToString(CultureInfo.InvariantCulture)} {unit}",
                Right = $"{right.ToString(CultureInfo.InvariantCulture)} {unit}",
                Top = $"{top.ToString(CultureInfo.InvariantCulture)} {unit}",
            };

            return this;
        }

        public IFluidPDFBuilder WithCustomScalePercentage(int scale)
        {
            _scale = scale;
            return this;
        }

        public IFluidPDFBuilder WithCulture(string cultureCode)
        {
            _cultureInfo = new CultureInfo(cultureCode);
            return this;
        }

        public IFluidPDFBuilder WithTemplate(string template)
        {
            _template = template.GetNonNullOrThrow(nameof(template));
            return this;
        }

        public IFluidPDFBuilder WithTemplateFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The file was not found", filePath);
            }

            _templateFilePath = filePath;
            return this;
        }

        public IFluidPDFBuilder WithCompression()
        {
            _toBeCompressed = true;
            return this;
        }

        public async Task<byte[]> BuildAsync()
        {
            Verify();

            string template = await GetTemplateAsync().ConfigureAwait(false);
            FluidPDFReportFactory factory = NewFluidPDFReportFactory();
            return await factory.CompileReportAsync(template, _model, _toBeCompressed, _cultureInfo).ConfigureAwait(false);
        }

        public async Task BuildAsync(Stream stream)
        {
            Verify();

            string template = await GetTemplateAsync().ConfigureAwait(false);
            FluidPDFReportFactory factory = NewFluidPDFReportFactory();
            await factory.CompileReportAsync(template, _model, stream, _toBeCompressed, _cultureInfo).ConfigureAwait(false);
        }

        private FluidPDFReportFactory NewFluidPDFReportFactory()
        {
            if (_chromiumRetriever is not null)
            {
                return new(_templateEngine, _chromiumRetriever, NewFluidPDFReportOptions());
            }

            return new(_templateEngine, NewChromiumRetrieverOptions(), NewFluidPDFReportOptions());
        }

        private ChromiumRetrieverOptions NewChromiumRetrieverOptions() => new(_chromeExePath == _standaloneChromePath ? null : _chromeExePath);

        internal FluidPDFReportOptions NewFluidPDFReportOptions() =>
            new()
            {
                Format = _paperFormat,
                Landscape = _landscape,
                MarginOptions = _marginOptions,
                Scale = Math.Min(Math.Max(_scale / 100M, 0.1M), 2) //between 0.1 and 2
            };

        private async ValueTask<string> GetTemplateAsync()
        {
            if (_template.IsNotNullAndNotBlank())
            {
                return _template!;
            }

            string template =
                await AsyncFile
                    .ReadAllTextAsync(_templateFilePath!)
                    .ConfigureAwait(false);

            return template;
        }

        private void Verify()
        {
            bool hasTemplate = _template.IsNotNullAndNotBlank() || _templateFilePath.IsNotNullAndNotBlank();
            bool hasChromeSetting = _chromiumRetriever is not null || _chromeExePath.IsNotNullAndNotBlank();

            bool finalCondition = hasTemplate && hasChromeSetting;
            if (!finalCondition)
            {
                string? missingInfo = null;
                if (!hasTemplate)
                {
                    missingInfo = "template (file or string)";
                }
                else if (!hasChromeSetting)
                {
                    missingInfo = "chrome info";
                }

                throw new FluidPDFBuilderConfigException($"One or more information is missing: {missingInfo}");
            }
        }
    }
}
