using FluidPDF.Exceptions;
using FluidPDF.Fluid;
using FluidPDF.Support;
using FluidPDF.Support.IO;
using FluidPDF.Support.PuppeteerSharp;
using FluidPDF.Templating;
using PuppeteerSharp.Media;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FluidPDF.Builder
{
    public static class FluidPDF
    {
        public static IFluidPDFBuilder NewReport() => new FluidPDFBuilder();
    }

    internal class FluidPDFBuilder : IFluidPDFBuilder
    {
        private readonly IChromiumRetriever? _chromiumRetriever;

        private FluidPDFTemplateModel? _model;
        private string? _chromeExePath;
        private bool _landscape;
        private PaperFormat _paperFormat;
        private MarginOptions _marginOptions;
        private int _scale;
        private CultureInfo? _cultureInfo;
        private string? _templateFilePath;
        private string? _template;
        private bool _toBeCompressed;
        private IFluidPDFTemplateEngine _templateEngine;

        internal FluidPDFBuilder(IChromiumRetriever? chromiumRetriever = null)
        {
            _model = null;
            _paperFormat = PaperFormat.A4;
            _landscape = false;
            _marginOptions = new MarginOptions { Bottom = "0.4 in", Left = "0.4 in", Right = "0.4 in", Top = "0.4 in" };
            _scale = 1; //100%
            _cultureInfo = null;
            _toBeCompressed = false;
            _templateEngine = new FluidTemplateEngine();
            _chromiumRetriever = chromiumRetriever;
        }

        public IFluidPDFBuilder WithDataRowModel(DataRow dataRow, string modelName = FluidPDFTemplateModel.DefaultName)
        {
            _model = FluidPDFTemplateModel.FromDataRow(dataRow, modelName);
            return this;
        }

        public IFluidPDFBuilder WithDataTableModel(DataTable dataTable, string modelName = FluidPDFTemplateModel.DefaultName)
        {
            _model = FluidPDFTemplateModel.FromDataTable(dataTable, modelName);
            return this;
        }

        public IFluidPDFBuilder WithDictionaryModel(IDictionary<string, object> dictionary, string modelName = FluidPDFTemplateModel.DefaultName)
        {
            _model = FluidPDFTemplateModel.FromDictionary(dictionary, modelName);
            return this;
        }

        public IFluidPDFBuilder WithJsonStringModel(string jsonString, string modelName = FluidPDFTemplateModel.DefaultName)
        {
            _model = FluidPDFTemplateModel.FromJsonString(jsonString, modelName);
            return this;
        }

        public IFluidPDFBuilder WithObjectModel(object obj, string modelName = FluidPDFTemplateModel.DefaultName)
        {
            _model = FluidPDFTemplateModel.FromObject(obj, modelName);
            return this;
        }

        public IFluidPDFBuilder WithTemplateEngine(IFluidPDFTemplateEngine templateEngine)
        {
            _templateEngine = templateEngine.GetNonNullOrThrow(nameof(templateEngine));
            return this;
        }

        public IFluidPDFBuilder WithExternalChromeProcess(string chromeExePath)
        {
            _chromeExePath = chromeExePath.GetNonNullOrThrow(nameof(chromeExePath));
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

        public IFluidPDFBuilder WithScalePercentage(int scale)
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

        public IFluidPDFBuilder WithPDFCompression()
        {
            _toBeCompressed = true;
            return this;
        }

        public async Task<byte[]> BuildAsync()
        {
            Verify();

            string template = await GetTemplateAsync().ConfigureAwait(false);
            FluidPDFReportFactory factory = NewFluidPDFReportFactory();
            return await factory.CompileReportAsync(template, _model!, _toBeCompressed, _cultureInfo).ConfigureAwait(false);
        }

        public async Task BuildAsync(Stream stream)
        {
            Verify();

            string template = await GetTemplateAsync().ConfigureAwait(false);
            FluidPDFReportFactory factory = NewFluidPDFReportFactory();
            await factory.CompileReportAsync(template, _model!, stream, _toBeCompressed, _cultureInfo).ConfigureAwait(false);
        }

        private FluidPDFReportFactory NewFluidPDFReportFactory()
        {
            if (_chromiumRetriever is not null)
            {
                return new(_templateEngine, _chromiumRetriever, NewFluidPDFReportOptions());
            }

            return new(_templateEngine, NewChromiumRetrieverOptions(), NewFluidPDFReportOptions());
        }

        private ChromiumRetrieverOptions NewChromiumRetrieverOptions() => new(_chromeExePath);

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
            StringBuilder builder = new();

            if (_template.IsNullOrBlankString())
            {
                builder.AppendLine("The template is missing (file or string)");
            }

            if (_model is null)
            {
                builder.AppendLine("The model is missing");
            }

            if (_scale < 0.1M || _scale > 2.0M)
            {
                builder.AppendLine("Scale must be between 0.1 and 2.0");
            }

            if (builder.Length > 0)
            {
                throw new FluidPDFBuilderConfigException($"One or more info are missing or wrong:{Environment.NewLine}{builder.ToString()}");
            }
        }
    }
}
