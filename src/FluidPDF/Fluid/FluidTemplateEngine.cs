using Fluid;
using Fluid.Values;
using FluidPDF.Templating;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace FluidPDF.Fluid
{
    public sealed class FluidTemplateEngine : IFluidPDFTemplateEngine
    {
        private const string _modelName = "Model";

        private static readonly FluidParser _parser = new();
        private static readonly TemplateOptions _templateOptions = NewTemplateOptions();

        public ValueTask<string> RenderTemplateAsync(string template, DataTable model, FluidPDFTemplateRenderOptions options) =>
            RenderWithDataTableAsync(template, model, options.ModelName, options.CultureInfo, null, true);

        public ValueTask<string> RenderTemplateAsync(string template, IDictionary<string, object> model, FluidPDFTemplateRenderOptions options) =>
            RenderWithDictionaryAsync(template, model, options.ModelName, options.CultureInfo, null, true);

        public ValueTask<string> RenderTemplateAsync(string template, FluidPDFTemplateModel[] models, FluidPDFTemplateRenderOptions options) =>
            RenderWithMultipleModelsAsync(template, models, options.CultureInfo, null, true);

        public ValueTask<string> RenderTemplateAsync(string template, object model, FluidPDFTemplateRenderOptions options) =>
            RenderWithObjectAsync(template, model, options.ModelName, options.CultureInfo, null, true);

        public static ValueTask<string> RenderWithDataRowAsync(string template, DataRow dataRow, string modelName = _modelName, CultureInfo? cultureInfo = null, TimeZoneInfo? timeZone = null, bool encodeHtml = false) =>
            RenderTemplateAsync
            (
                [FluidPDFTemplateModel.FromDataRow(modelName, dataRow)],
                template,
                encodeHtml,
                cultureInfo,
                timeZone
            );

        public static ValueTask<string> RenderWithDataTableAsync(string template, DataTable dataTable, string modelName = _modelName, CultureInfo? cultureInfo = null, TimeZoneInfo? timeZone = null, bool encodeHtml = false) =>
            RenderTemplateAsync
            (
                [FluidPDFTemplateModel.FromDataTable(modelName, dataTable)],
                template,
                encodeHtml,
                cultureInfo,
                timeZone
            );

        public static ValueTask<string> RenderWithDictionaryAsync(string template, IDictionary<string, object> dictionary, string modelName = _modelName, CultureInfo? cultureInfo = null, TimeZoneInfo? timeZone = null, bool encodeHtml = false) =>
            RenderTemplateAsync
            (
                [FluidPDFTemplateModel.FromDictionary(modelName, dictionary)],
                template,
                encodeHtml,
                cultureInfo,
                timeZone
            );

        public static ValueTask<string> RenderWithJsonStringAsync(string template, string jsonString, string modelName = _modelName, CultureInfo? cultureInfo = null, TimeZoneInfo? timeZone = null, bool encodeHtml = false) =>
            RenderTemplateAsync
            (
                [FluidPDFTemplateModel.FromJsonString(modelName, jsonString)],
                template,
                encodeHtml,
                cultureInfo,
                timeZone
            );

        public static ValueTask<string> RenderWithObjectAsync(string template, object obj, string modelName = _modelName, CultureInfo? cultureInfo = null, TimeZoneInfo? timeZone = null, bool encodeHtml = false) =>
            RenderTemplateAsync
            (
                [FluidPDFTemplateModel.FromObject(modelName, obj)],
                template,
                encodeHtml,
                cultureInfo,
                timeZone
            );

        public static ValueTask<string> RenderWithMultipleModelsAsync(string template, FluidPDFTemplateModel[] models, CultureInfo? cultureInfo = null, TimeZoneInfo? timeZone = null, bool encodeHtml = false) =>
            RenderTemplateAsync
            (
                models,
                template,
                encodeHtml,
                cultureInfo,
                timeZone
            );

        private static async ValueTask<string> RenderTemplateAsync(FluidPDFTemplateModel[] models, string template, bool encodeHtml = false, CultureInfo? cultureInfo = null, TimeZoneInfo? timeZone = null)
        {
            try
            {
                if (_parser.TryParse(template, out IFluidTemplate? fluidTemplate, out string? error))
                {
                    TemplateContext context = NewTemplateContext(models, cultureInfo, timeZone);

                    using StringWriter writer = new();

                    TextEncoder encoder = encodeHtml ? HtmlEncoder.Default : NullEncoder.Default;

                    await fluidTemplate
                        .RenderAsync(writer, encoder, context)
                        .ConfigureAwait(false);

                    string renderedValue = writer.ToString();
                    return renderedValue;
                }
                else
                {
                    throw new Exception(error);
                }
            }
            catch (Exception ex)
            {
                if (ex is FluidPDFTemplateRenderException) throw;
                throw new FluidPDFTemplateRenderException("An error occurred in rendering the template", ex);
            }
        }

        private static TemplateContext NewTemplateContext(FluidPDFTemplateModel[] models, CultureInfo? cultureInfo = null, TimeZoneInfo? timeZone = null)
        {
            TemplateContext context = new(_templateOptions)
            {
                CultureInfo = cultureInfo ?? CultureInfo.InvariantCulture
            };

            if (timeZone is not null)
            {
                context.TimeZone = timeZone;
            }

            foreach (FluidPDFTemplateModel model in models)
            {
                context.SetValue(model.Name, model.Value);
            }

            return context;
        }

        private static TemplateOptions NewTemplateOptions()
        {
            TemplateOptions templateOptions = new()
            {
                Trimming = TrimmingFlags.TagRight
            };

            templateOptions.ValueConverters.Add(x => x is DBNull o ? NilValue.Instance : null);

            templateOptions
                .MemberAccessStrategy
                .Register<DataRow, object>
                (
                    (row, fieldName) =>
                    {
                        if (row.IsNull(fieldName))
                        {
                            return DBNull.Value;
                        }

                        return row[fieldName];
                    }
                );

            templateOptions
                .MemberAccessStrategy
                .Register<DataTable, object>
                (
                    (table, fieldName) =>
                    {
                        if (!fieldName.Equals(nameof(DataTable.Rows)))
                        {
                            return DBNull.Value;
                        }

                        return table.Rows.Cast<DataRow>().ToArray();
                    }
                );

            templateOptions.MemberAccessStrategy.Register<JsonObject, object>((src, name) => src[name]!);
            templateOptions.MemberAccessStrategy.Register<JsonArray, object>((src, name) => src[name]!);
            templateOptions.MemberAccessStrategy.Register<JsonNode, object>((src, name) => src[name]!);

            templateOptions.ValueConverters.Add(x => x is JsonArray o ? new ArrayValue(o.Select(x => new ObjectValue(x)).ToArray()) : null);
            templateOptions.ValueConverters.Add(x => x is JsonNode o ? new ObjectValue(o) : null);
            templateOptions.ValueConverters.Add(x => x is JsonValue o ? new ObjectValue(o) : null);
            templateOptions.ValueConverters.Add(x => x is JsonObject o ? new ObjectValue(o) : null);

            return templateOptions;
        }
    }
}
