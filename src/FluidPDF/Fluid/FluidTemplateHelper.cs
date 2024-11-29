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
    internal static class FluidTemplateHelper
    {
        private const string _modelName = "Model";

        private static readonly FluidParser _parser = new();

        //needed to ensure _parser singleton's thread safety
        static FluidTemplateHelper()
        {
        }

        public static ValueTask<string> RenderTemplateByTypeAsync<T>(string template, T model, string modelName = _modelName, CultureInfo? cultureInfo = null, TimeZoneInfo? timeZone = null, bool encodeHtml = false)
            where T : notnull =>
            model switch
            {
                DataRow => RenderWithDataRowAsync(template, (model as DataRow)!, modelName, cultureInfo, timeZone, encodeHtml),
                Dictionary<string, object> => RenderWithDictionaryAsync(template, (model as Dictionary<string, object>)!, modelName, cultureInfo, timeZone, encodeHtml),
                string => RenderWithJsonStringAsync(template, (model as string)!, modelName, cultureInfo, timeZone, encodeHtml),
                FluidModel[] => RenderWithMultipleModelsAsync(template, model as FluidModel[] ?? [], cultureInfo, timeZone, encodeHtml),
                _ => RenderWithObjectAsync(template, model, modelName, cultureInfo, timeZone, encodeHtml)
            };

        public static ValueTask<string> RenderWithDataRowAsync(string template, DataRow dataRow, string modelName = _modelName, CultureInfo? cultureInfo = null, TimeZoneInfo? timeZone = null, bool encodeHtml = false) =>
            RenderTemplateAsync
            (
                [FluidModel.FromDataRow(modelName, dataRow)],
                template,
                encodeHtml,
                cultureInfo,
                timeZone
            );

        public static ValueTask<string> RenderWithDictionaryAsync(string template, Dictionary<string, object> dictionary, string modelName = _modelName, CultureInfo? cultureInfo = null, TimeZoneInfo? timeZone = null, bool encodeHtml = false) =>
            RenderTemplateAsync
            (
                [FluidModel.FromDictionary(modelName, dictionary)],
                template,
                encodeHtml,
                cultureInfo,
                timeZone
            );

        public static ValueTask<string> RenderWithJsonStringAsync(string template, string jsonString, string modelName = _modelName, CultureInfo? cultureInfo = null, TimeZoneInfo? timeZone = null, bool encodeHtml = false) =>
            RenderTemplateAsync
            (
                [FluidModel.FromJsonString(modelName, jsonString)],
                template,
                encodeHtml,
                cultureInfo,
                timeZone
            );

        public static ValueTask<string> RenderWithObjectAsync(string template, object obj, string modelName = _modelName, CultureInfo? cultureInfo = null, TimeZoneInfo? timeZone = null, bool encodeHtml = false) =>
            RenderTemplateAsync
            (
                [FluidModel.FromObject(modelName, obj)],
                template,
                encodeHtml,
                cultureInfo,
                timeZone
            );

        public static ValueTask<string> RenderWithMultipleModelsAsync(string template, FluidModel[] models, CultureInfo? cultureInfo = null, TimeZoneInfo? timeZone = null, bool encodeHtml = false) =>
            RenderTemplateAsync
            (
                models,
                template,
                encodeHtml,
                cultureInfo,
                timeZone
            );

        private static async ValueTask<string> RenderTemplateAsync(FluidModel[] models, string template, bool encodeHtml = false, CultureInfo? cultureInfo = null, TimeZoneInfo? timeZone = null)
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
                throw new FluidRenderException("An error occurred in rendering a template", new Exception(error));
            }
        }

        private static TemplateContext NewTemplateContext(FluidModel[] models, CultureInfo? cultureInfo = null, TimeZoneInfo? timeZone = null)
        {
            TemplateContext context = new(FluidTemplateOptions.Instance)
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
