using Fluid;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace FluidPDF.Fluid
{
    internal static class FluidHelper
    {
        private const string _modelName = "Model";

        private static readonly FluidParser _parser = new();

        internal static ValueTask<string> RenderTemplateWithDataTableAsync(string templateContent, DataTable dataTable, string modelName = _modelName, CultureInfo? cultureInfo = null, TimeZoneInfo? timeZone = null, bool encodeHtml = false) =>
            RenderTemplateAsync
            (
                [FluidModel.FromDataTable(modelName, dataTable)],
                templateContent,
                encodeHtml,
                cultureInfo,
                timeZone
            );

        internal static ValueTask<string> RenderTemplateWithDataRowAsync(string templateContent, DataRow dataRow, string modelName = _modelName, CultureInfo? cultureInfo = null, TimeZoneInfo? timeZone = null, bool encodeHtml = false) =>
            RenderTemplateAsync
            (
                [FluidModel.FromDataRow(modelName, dataRow)],
                templateContent,
                encodeHtml,
                cultureInfo,
                timeZone
            );

        internal static ValueTask<string> RenderTemplateWithDictionaryAsync(string templateContent, Dictionary<string, object> dictionary, string modelName = _modelName, CultureInfo? cultureInfo = null, TimeZoneInfo? timeZone = null, bool encodeHtml = false) =>
            RenderTemplateAsync
            (
                [FluidModel.FromDictionary(modelName, dictionary)],
                templateContent,
                encodeHtml,
                cultureInfo,
                timeZone
            );

        internal static ValueTask<string> RenderTemplateWithJsonStringAsync(string templateContent, string jsonString, string modelName = _modelName, CultureInfo? cultureInfo = null, TimeZoneInfo? timeZone = null, bool encodeHtml = false) =>
            RenderTemplateAsync
            (
                [FluidModel.FromJsonString(modelName, jsonString)],
                templateContent,
                encodeHtml,
                cultureInfo,
                timeZone
            );

        internal static ValueTask<string> RenderTemplateWithObjectAsync(string templateContent, object obj, string modelName = _modelName, CultureInfo? cultureInfo = null, TimeZoneInfo? timeZone = null, bool encodeHtml = false) =>
            RenderTemplateAsync
            (
                [FluidModel.FromObject(modelName, obj)],
                templateContent,
                encodeHtml,
                cultureInfo,
                timeZone
            );

        internal static ValueTask<string> RenderTemplateWithMultipleModelsAsync(string templateContent, FluidModel[] models, string modelName = _modelName, CultureInfo? cultureInfo = null, TimeZoneInfo? timeZone = null, bool encodeHtml = false) =>
            RenderTemplateAsync
            (
                models,
                templateContent,
                encodeHtml,
                cultureInfo,
                timeZone
            );

        private static async ValueTask<string> RenderTemplateAsync(FluidModel[] models, string templateContent, bool encodeHtml = false, CultureInfo? cultureInfo = null, TimeZoneInfo? timeZone = null)
        {
            if (_parser.TryParse(templateContent, out IFluidTemplate? template, out string? error))
            {
                TemplateContext context = NewTemplateContext(models, cultureInfo, timeZone);

                using StringWriter writer = new();

                TextEncoder encoder = encodeHtml ? HtmlEncoder.Default : NullEncoder.Default;

                await template
                    .RenderAsync(writer, encoder, context)
                    .ConfigureAwait(false);

                string renderedValue = writer.ToString();
                return renderedValue;
            }
            else
            {
                throw new FluidRenderException("An error occurred in rendering a template", new Exception(error));
            }
        }

        private static TemplateContext NewTemplateContext(FluidModel[] models, CultureInfo? cultureInfo = null, TimeZoneInfo? timeZone = null)
        {
            TemplateContext context = new()
            {
                CultureInfo = cultureInfo ?? CultureInfo.InvariantCulture
            };

            if (timeZone is not null)
            {
                context.TimeZone = timeZone;
            }

            foreach (FluidModel model in models ?? [])
            {
                context.SetValue(model.Name, model.Value);
            }

            return context;
        }
    }
}
