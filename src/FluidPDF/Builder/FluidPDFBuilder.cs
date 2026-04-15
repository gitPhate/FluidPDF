using Fluid;
using Fluid.Ast;
using FluidPDF.Exceptions;
using FluidPDF.Fluid;
using FluidPDF.Support;
using FluidPDF.Support.IO;
using FluidPDF.Support.PuppeteerSharp;
using FluidPDF.Templating;
using FluidPDF.Templating.Localization;
using PuppeteerSharp.Media;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
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

        private FluidPDFTemplateModel[] _models;
        private string? _chromeExePath;
        private ILocalizationProvider? _localizationProvider;
        private string? _templateFilePath;
        private string? _template;
        private IFluidPDFTemplateEngine _templateEngine;
        private bool _isDisposed;
        private FluidTemplateEngineOptions? _fluidEngineOptions;
        private readonly FluidPDFReportOptions _options;

        internal FluidPDFBuilder(IChromiumRetriever? chromiumRetriever = null)
        {
            _models = [];
            _templateEngine = new FluidTemplateEngine();
            _chromiumRetriever = chromiumRetriever;
            _options = new FluidPDFReportOptions();
        }

        public IFluidPDFBuilder WithDataRowModel(DataRow dataRow, string modelName = ModelNames.DefaultModelName)
        {
            _models = [FluidPDFTemplateModel.FromDataRow(dataRow, modelName)];
            return this;
        }

        public IFluidPDFBuilder WithDataTableModel(DataTable dataTable, string modelName = ModelNames.DefaultModelName)
        {
            _models = [FluidPDFTemplateModel.FromDataTable(dataTable, modelName)];
            return this;
        }

        public IFluidPDFBuilder WithDictionaryModel(IDictionary<string, object?> dictionary, string modelName = ModelNames.DefaultModelName)
        {
            _models = [FluidPDFTemplateModel.FromDictionary(dictionary, modelName)];
            return this;
        }

        public IFluidPDFBuilder WithJsonStringModel(string jsonString, string modelName = ModelNames.DefaultModelName)
        {
            _models = [FluidPDFTemplateModel.FromJsonString(jsonString, modelName)];
            return this;
        }

        public IFluidPDFBuilder WithObjectModel(object obj, string modelName = ModelNames.DefaultModelName)
        {
            _models = [FluidPDFTemplateModel.FromObject(obj, modelName)];
            return this;
        }

        public IFluidPDFBuilder WithArrayModel(IEnumerable<object?> array, string modelName = ModelNames.DefaultModelName)
        {
            _models = [FluidPDFTemplateModel.FromArray(array, modelName)];
            return this;
        }

        public IFluidPDFBuilder WithModel(FluidPDFTemplateModel model)
        {
            _models = [model.GetNonNullOrThrow(nameof(model))];
            return this;
        }

        public IFluidPDFBuilder WithModels(FluidPDFTemplateModel[] models)
        {
            _models = models.GetNonNullOrThrow(nameof(models));
            return this;
        }

        public IFluidPDFBuilder WithTemplateEngine(IFluidPDFTemplateEngine templateEngine)
        {
            _templateEngine.Dispose();
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
            _options.Landscape = true;
            return this;
        }

        public IFluidPDFBuilder WithHtmlEncode()
        {
            _options.EncodeHtml = true;
            return this;
        }

        public IFluidPDFBuilder WithA2Format()
        {
            _options.Format = PaperFormat.A2;
            return this;
        }

        public IFluidPDFBuilder WithA3Format()
        {
            _options.Format = PaperFormat.A3;
            return this;
        }

        public IFluidPDFBuilder WithA5Format()
        {
            _options.Format = PaperFormat.A5;
            return this;
        }

        public IFluidPDFBuilder WithA6Format()
        {
            _options.Format = PaperFormat.A6;
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
            _options.MarginOptions = new MarginOptions
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
            _options.Scale = scale;
            return this;
        }

        public IFluidPDFBuilder WithLocalization(ILocalizationProvider provider)
        {
            _localizationProvider = provider.GetNonNullOrThrow(nameof(provider));
            return this;
        }

        public IFluidPDFBuilder WithCulture(CultureInfo culture)
        {
            _options.CultureInfo = culture.GetNonNullOrThrow(nameof(culture));
            return this;
        }

        public IFluidPDFBuilder WithCulture(string cultureCode)
        {
            return WithCulture(new CultureInfo(cultureCode));
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
            _options.ToBeCompressed = true;
            return this;
        }

        public IFluidPDFBuilder WithFluidFilter(string name, FilterDelegate filter)
        {
            FluidEngineOptions.AddFilter(
                name.GetNonNullOrThrow(nameof(name)),
                filter.GetNonNullOrThrow(nameof(filter)));
            ReplaceWithConfiguredFluidEngine();
            return this;
        }

        public IFluidPDFBuilder WithFluidEmptyTag(string name, Func<TextWriter, TextEncoder, TemplateContext, ValueTask<Completion>> render)
        {
            FluidEngineOptions.AddEmptyTag(
                name.GetNonNullOrThrow(nameof(name)),
                render.GetNonNullOrThrow(nameof(render)));
            ReplaceWithConfiguredFluidEngine();
            return this;
        }

        public IFluidPDFBuilder WithFluidIdentifierTag(string name, Func<string, TextWriter, TextEncoder, TemplateContext, ValueTask<Completion>> render)
        {
            FluidEngineOptions.AddIdentifierTag(
                name.GetNonNullOrThrow(nameof(name)),
                render.GetNonNullOrThrow(nameof(render)));
            ReplaceWithConfiguredFluidEngine();
            return this;
        }

        public IFluidPDFBuilder WithFluidArgumentTag(string name, Func<IReadOnlyList<FilterArgument>, TextWriter, TextEncoder, TemplateContext, ValueTask<Completion>> render)
        {
            FluidEngineOptions.AddArgumentTag(
                name.GetNonNullOrThrow(nameof(name)),
                render.GetNonNullOrThrow(nameof(render)));
            ReplaceWithConfiguredFluidEngine();
            return this;
        }

        private FluidTemplateEngineOptions FluidEngineOptions =>
            _fluidEngineOptions ??= new FluidTemplateEngineOptions();

        private void ReplaceWithConfiguredFluidEngine()
        {
            if (_templateEngine is not FluidTemplateEngine)
            {
                throw new FluidPDFBuilderConfigException(
                    "Fluid-specific registrations cannot be used when a custom template engine has been set via WithTemplateEngine.");
            }

            _templateEngine.Dispose();
            _templateEngine = new FluidTemplateEngine(_fluidEngineOptions!);
        }

        public async Task<byte[]> BuildAsync()
        {
            try
            {
                Verify();

                string template = await GetTemplateAsync().ConfigureAwait(false);
                FluidPDFReportFactory factory = NewFluidPDFReportFactory();
                return await factory.CompileReportAsync(template, _models, NewFluidPDFReportOptions()).ConfigureAwait(false);
            }
            finally
            {
                Dispose();
            }
        }

        public async Task BuildAsync(Stream stream)
        {
            try
            {
                Verify();

                string template = await GetTemplateAsync().ConfigureAwait(false);
                FluidPDFReportFactory factory = NewFluidPDFReportFactory();
                await factory.CompileReportAsync(template, _models, stream, NewFluidPDFReportOptions()).ConfigureAwait(false);
            }
            finally
            {
                Dispose();
            }
        }

        private FluidPDFReportFactory NewFluidPDFReportFactory()
        {
            if (_chromiumRetriever is not null)
            {
                return new(_templateEngine, _chromiumRetriever, _localizationProvider);
            }

            return new(_templateEngine, NewChromiumRetrieverOptions(), _localizationProvider);
        }

        private ChromiumRetrieverOptions NewChromiumRetrieverOptions() => new(_chromeExePath);

        internal FluidPDFReportOptions NewFluidPDFReportOptions() => _options;

        private async ValueTask<string> GetTemplateAsync()
        {
            if (_template.IsNotNullAndNotBlank())
            {
                return _template!;
            }

            string template =
                await FileHelper
                    .ReadAllTextAsync(_templateFilePath!)
                    .ConfigureAwait(false);

            return template;
        }

        private void Verify()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(FluidPDFBuilder));
            }

            StringBuilder builder = new();

            if (_template.IsNullOrBlankString())
            {
                builder.AppendLine("The template is missing (file or string)");
            }

            if (_models is null || _models.Length == 0)
            {
                builder.AppendLine("One or more models are missing");
            }

            if (_options.Scale < 10 || _options.Scale > 200)
            {
                builder.AppendLine("Scale must be between 10 and 200");
            }

            if (builder.Length > 0)
            {
                throw new FluidPDFBuilderConfigException($"One or more info are missing or wrong:{Environment.NewLine}{builder}");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                _templateEngine.Dispose();
            }

            _isDisposed = true;
        }
    }
}
