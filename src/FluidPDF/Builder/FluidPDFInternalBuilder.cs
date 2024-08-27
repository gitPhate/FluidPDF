using FluidPDF.Exceptions;
using FluidPDF.KTemplating;
using FluidPDF.PDF;
using FluidPDF.PuppeteerSharp;
using FluidPDF.Support.IO;
using Kyklos.Kernel.Core.Asserts;
using Kyklos.Kernel.Core.Strings;
using PuppeteerSharp.Media;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace FluidPDF.Builder
{
    internal class FluidPDFInternalBuilder<T> : IFluidPDFBuilder
    {
        private const string _standaloneChromePath = "standalone";

        private string? _chromeExePath;
        private bool _landscape;
        private PaperFormat _paperFormat;
        private MarginOptions _marginOptions;
        private int _scale;
        private CultureInfo? _cultureInfo;
        private string? _templateFilePath = null;
        private string? _template = null;
        private bool _toBeCompressed;
        private readonly T _model;

        internal FluidPDFInternalBuilder(T model)
        {
            _chromeExePath = null;
            _paperFormat = PaperFormat.A4;
            _landscape = false;
            _marginOptions = new MarginOptions { Bottom = "0.4 in", Left = "0.4 in", Right = "0.4 in", Top = "0.4 in" };
            _scale = 100;
            _cultureInfo = null;
            _toBeCompressed = false;
            _model = model;
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

        public IFluidPDFBuilder WithLanscapeOrientation()
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

        //public IFluidPDFBuilder WithPixelMargin(double margin) =>
        //    WithPixelMargin(margin, margin, margin, margin);

        //public IFluidPDFBuilder WithPixelMargin(double bottom, double left, double right, double top) =>
        //    WithMargin(bottom, left, right, top, "px");

        //public IFluidPDFBuilder WithInchMargin(double margin) =>
        //    WithInchMargin(margin, margin, margin, margin);

        //public IFluidPDFBuilder WithInchMargin(double bottom, double left, double right, double top) =>
        //    WithMargin(bottom, left, right, top, "in");

        //internal IFluidPDFBuilder WithMargin(double bottom, double left, double right, double top, string unit)
        //{
        //    string strBottom = (Math.Round(bottom * 10) / 10).ToString();
        //    string strLeft = (Math.Round(left * 10) / 10).ToString();
        //    string strRight = (Math.Round(right * 10) / 10).ToString();
        //    string strTop = (Math.Round(top * 10) / 10).ToString();

        //    byte[] bytes = Encoding.Default.GetBytes(strBottom + " " + unit);
        //    string measure = Encoding.ASCII.GetString(bytes);

        //    _marginOptions =
        //        new()
        //        {
        //            Bottom = $"{Math.Round(bottom * 10) / 10} {unit}",
        //            Left = $"{Math.Round(left * 10) / 10} {unit}",
        //            Right = $"{Math.Round(right * 10) / 10} {unit}",
        //            Top = $"{Math.Round(top * 10) / 10} {unit}",
        //        };

        //    return this;
        //}

        //public IFluidPDFBuilder WithCustomMargin(MarginOptions marginOptions)
        //{
        //    _marginOptions = marginOptions.GetNonNullOrThrow(nameof(marginOptions));
        //    return this;
        //}

        public IFluidPDFBuilder WithCustomMargin(FluidPDFMargins margins)
        {
            _marginOptions = margins switch
            {
                FluidPDFMargins.None => new MarginOptions(),
                FluidPDFMargins.ZeroPoint5 => new MarginOptions { Bottom = "0.5 in", Left = "0.5 in", Right = "0.5 in", Top = "0.5 in" },
                FluidPDFMargins.ZeroPoint4 => new MarginOptions { Bottom = "0.4 in", Left = "0.4 in", Right = "0.4 in", Top = "0.4 in" },
                FluidPDFMargins.ZeroPoint3 => new MarginOptions { Bottom = "0.3 in", Left = "0.3 in", Right = "0.3 in", Top = "0.3 in" },
                FluidPDFMargins.ZeroPoint2 => new MarginOptions { Bottom = "0.2 in", Left = "0.2 in", Right = "0.2 in", Top = "0.2 in" },
                FluidPDFMargins.ZeroPoint1 => new MarginOptions { Bottom = "0.1 in", Left = "0.1 in", Right = "0.1 in", Top = "0.1 in" },
                _ => throw new NotImplementedException()
            };

            return this;
        }

        public IFluidPDFBuilder WithCustomScalePercentage(int scale)
        {
            _scale = scale.GetNonNullOrThrow(nameof(scale));
            return this;
        }

        public IFluidPDFBuilder WithCulture(string cultureCode)
        {
            cultureCode.AssertArgumentHasText(nameof(cultureCode));
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
            filePath.AssertArgumentHasText(nameof(filePath));

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

        public async Task<IPdfPrototype> BuildAsync()
        {
            Verify();

            string? chromePath = _chromeExePath == _standaloneChromePath ? null : _chromeExePath;
            ChromiumRetrieverOptions chromiumRetrieverOptions = new(chromePath);

            PdfPrototypeFactoryOptions pdfPrototypeFactoryOptions =
                new()
                {
                    Format = _paperFormat,
                    Landscape = _landscape,
                    MarginOptions = _marginOptions,
                    Scale = Math.Min(Math.Max(_scale / 100M, 0.1M), 2) //between 0.1 and 2
                };

            KTemplateHelperWrapperOptions wrapperOptions = new(_cultureInfo);
            KTemplateHelperWrapper wrapper = new(wrapperOptions);

            PdfPrototypeFactory factory = new(chromiumRetrieverOptions, pdfPrototypeFactoryOptions, wrapper);

            string template = await GetTemplateAsync().ConfigureAwait(false);
            IPdfPrototype prototype = await factory.NewPdfPrototypeAsync(template, _model, _toBeCompressed).ConfigureAwait(false);
            return prototype;
        }

        private async ValueTask<string> GetTemplateAsync()
        {
            if (!_template.IsNullOrBlankString())
            {
                return _template;
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
            bool hasChromeSetting = _chromeExePath.IsNotNullAndNotBlank();

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

    public enum FluidPDFMargins
    {
        None,
        ZeroPoint5,
        ZeroPoint4,
        ZeroPoint3,
        ZeroPoint2,
        ZeroPoint1
    }
}
