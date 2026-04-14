using Fluid;
using Fluid.Ast;
using FluidPDF.Templating;
using FluidPDF.Templating.Localization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace FluidPDF.Builder
{
    public interface IFluidPDFBuilder : IDisposable
    {
        IFluidPDFBuilder WithDataRowModel(DataRow dataRow, string modelName = ModelNames.DefaultModelName);
        IFluidPDFBuilder WithDataTableModel(DataTable dataTable, string modelName = ModelNames.DefaultModelName);
        IFluidPDFBuilder WithDictionaryModel(IDictionary<string, object?> dictionary, string modelName = ModelNames.DefaultModelName);
        IFluidPDFBuilder WithJsonStringModel(string jsonString, string modelName = ModelNames.DefaultModelName);
        IFluidPDFBuilder WithObjectModel(object obj, string modelName = ModelNames.DefaultModelName);
        IFluidPDFBuilder WithArrayModel(IEnumerable<object?> array, string modelName = ModelNames.DefaultModelName);
        IFluidPDFBuilder WithModel(FluidPDFTemplateModel model);
        IFluidPDFBuilder WithModels(FluidPDFTemplateModel[] models);
        IFluidPDFBuilder WithTemplateEngine(IFluidPDFTemplateEngine templateEngine);
        IFluidPDFBuilder WithExternalChromeProcess(string chromeExePath);
        IFluidPDFBuilder WithLandscapeOrientation();
        IFluidPDFBuilder WithHtmlEncode();
        IFluidPDFBuilder WithA2Format();
        IFluidPDFBuilder WithA3Format();
        IFluidPDFBuilder WithA5Format();
        IFluidPDFBuilder WithA6Format();
        IFluidPDFBuilder WithPixelMargin(decimal margin);
        IFluidPDFBuilder WithPixelMargin(decimal bottom, decimal left, decimal right, decimal top);
        IFluidPDFBuilder WithInchMargin(decimal margin);
        IFluidPDFBuilder WithInchMargin(decimal bottom, decimal left, decimal right, decimal top);
        IFluidPDFBuilder WithScalePercentage(int scale);
        IFluidPDFBuilder WithLocalization(ILocalizationProvider provider);
        IFluidPDFBuilder WithCulture(CultureInfo culture);
        IFluidPDFBuilder WithCulture(string cultureCode);
        IFluidPDFBuilder WithTemplate(string template);
        IFluidPDFBuilder WithTemplateFile(string filePath);
        IFluidPDFBuilder WithPDFCompression();

        /// <summary>
        /// Registers a custom Fluid filter on the default <see cref="Fluid.FluidTemplateEngine"/>.
        /// Throws <see cref="Exceptions.FluidPDFBuilderConfigException"/> if a non-Fluid template
        /// engine was set via <see cref="WithTemplateEngine"/>.
        /// </summary>
        IFluidPDFBuilder WithFluidFilter(string name, FilterDelegate filter);

        /// <summary>
        /// Registers a custom Fluid empty tag (no arguments) on the default
        /// <see cref="Fluid.FluidTemplateEngine"/>.
        /// Throws <see cref="Exceptions.FluidPDFBuilderConfigException"/> if a non-Fluid template
        /// engine was set via <see cref="WithTemplateEngine"/>.
        /// </summary>
        IFluidPDFBuilder WithFluidEmptyTag(string name, Func<TextWriter, TextEncoder, TemplateContext, ValueTask<Completion>> render);

        /// <summary>
        /// Registers a custom Fluid identifier tag on the default
        /// <see cref="Fluid.FluidTemplateEngine"/>.
        /// Throws <see cref="Exceptions.FluidPDFBuilderConfigException"/> if a non-Fluid template
        /// engine was set via <see cref="WithTemplateEngine"/>.
        /// </summary>
        IFluidPDFBuilder WithFluidIdentifierTag(string name, Func<string, TextWriter, TextEncoder, TemplateContext, ValueTask<Completion>> render);

        /// <summary>
        /// Registers a custom Fluid argument tag on the default
        /// <see cref="Fluid.FluidTemplateEngine"/>.
        /// Throws <see cref="Exceptions.FluidPDFBuilderConfigException"/> if a non-Fluid template
        /// engine was set via <see cref="WithTemplateEngine"/>.
        /// </summary>
        IFluidPDFBuilder WithFluidArgumentTag(string name, Func<IReadOnlyList<FilterArgument>, TextWriter, TextEncoder, TemplateContext, ValueTask<Completion>> render);

        Task<byte[]> BuildAsync();
        Task BuildAsync(Stream stream);
    }
}
