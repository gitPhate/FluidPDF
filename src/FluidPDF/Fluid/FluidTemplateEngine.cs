using Fluid;
using Fluid.Values;
using FluidPDF.Exceptions;
using FluidPDF.Support;
using FluidPDF.Templating;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace FluidPDF.Fluid
{
    public sealed class FluidTemplateEngine : IFluidPDFTemplateEngine
    {
        private static readonly FluidParser _parser = new();
        private static readonly TemplateOptions _templateOptions = NewTemplateOptions();

        public async ValueTask<string> RenderTemplateAsync(string template, DataTable model, FluidPDFTemplateRenderOptions options)
        {
            FluidPDFTemplateModel fluidPDFModel = FluidPDFTemplateModel.FromDataTable(model, options.ModelName);
            return await RenderTemplateAsync([fluidPDFModel], template, options).ConfigureAwait(false);
        }

        public async ValueTask<string> RenderTemplateAsync(string template, IDictionary<string, object> model, FluidPDFTemplateRenderOptions options)
        {
            FluidPDFTemplateModel fluidPDFModel = FluidPDFTemplateModel.FromDictionary(model, options.ModelName);
            return await RenderTemplateAsync([fluidPDFModel], template, options).ConfigureAwait(false);
        }

        public ValueTask<string> RenderTemplateAsync(string template, FluidPDFTemplateModel[] models, FluidPDFTemplateRenderOptions options) =>
            RenderTemplateAsync(models, template, options);

        public async ValueTask<string> RenderTemplateAsync(string template, object model, FluidPDFTemplateRenderOptions options)
        {
            FluidPDFTemplateModel fluidPDFModel = FluidPDFTemplateModel.FromObject(model, options.ModelName);
            return await RenderTemplateAsync([fluidPDFModel], template, options).ConfigureAwait(false);
        }

        public async ValueTask<string> RenderTemplateAsync(string template, string jsonModel, FluidPDFTemplateRenderOptions options)
        {
            FluidPDFTemplateModel fluidPDFModel = FluidPDFTemplateModel.FromJsonString(jsonModel, options.ModelName);
            return await RenderTemplateAsync([fluidPDFModel], template, options).ConfigureAwait(false);
        }

        private static async ValueTask<string> RenderTemplateAsync(FluidPDFTemplateModel[] models, string template, FluidPDFTemplateRenderOptions options)
        {
            if (_parser.TryParse(template, out IFluidTemplate? fluidTemplate, out string? error))
            {
                TemplateContext context = NewTemplateContext(models, options.CultureInfo, null);

                using StringWriter writer = new();

                TextEncoder encoder = options.EncodeHtml ? HtmlEncoder.Default : NullEncoder.Default;

                await fluidTemplate
                    .RenderAsync(writer, encoder, context)
                    .ConfigureAwait(false);

                string renderedValue = writer.ToString();
                return renderedValue;
            }
            else
            {
                throw new FluidTemplateRenderException(error);
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

            if (models.Select(x => x.Name).Distinct().Count() != models.Length)
            {
                throw new ArgumentException("Some models with the same name have already been added");
            }

            foreach (FluidPDFTemplateModel model in models)
            {
                object value =
                    (model.Type switch
                    {
                        FluidPDFTemplateModelType.DataRow => model.DataRow,
                        FluidPDFTemplateModelType.DataTable => model.DataTable,
                        FluidPDFTemplateModelType.Dictionary => model.Dictionary,
                        FluidPDFTemplateModelType.JsonString => JsonNode.Parse(model.JsonString!),
                        FluidPDFTemplateModelType.Object => JsonSerializer.SerializeToNode(model.ObjectValue),
                        FluidPDFTemplateModelType.PlainValue => model.PlainValue,
                        _ => throw new InvalidOperationException($"Invalid {nameof(FluidPDFTemplateModelType)}")
                    })
                    .GetNonNullOrThrow(nameof(value));

                context.SetValue(model.Name, value);
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
