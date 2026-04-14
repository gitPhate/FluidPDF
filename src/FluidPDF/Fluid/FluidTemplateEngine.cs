using Fluid;
using Fluid.Ast;
using Fluid.Values;
using FluidPDF.Exceptions;
using FluidPDF.Fluid.Filters;
using FluidPDF.Fluid.Tags;
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
        // Shared defaults — built-in filters and tags, no user extras.
        // Initialised once via the static constructor to guarantee ordering.
        private static readonly FluidPDFParser _sharedParser;
        private static readonly TemplateOptions _sharedTemplateOptions;

        // Instance overrides — non-null only when the engine was constructed with
        // user-supplied FluidTemplateEngineOptions that contain at least one entry.
        private readonly FluidPDFParser? _instanceParser;
        private readonly TemplateOptions? _instanceTemplateOptions;

        private FluidPDFParser ActiveParser => _instanceParser ?? _sharedParser;
        private TemplateOptions ActiveTemplateOptions => _instanceTemplateOptions ?? _sharedTemplateOptions;

        static FluidTemplateEngine()
        {
            _sharedParser = new FluidPDFParser();
            _sharedTemplateOptions = BuildTemplateOptions(_sharedParser, userOptions: null);
        }

        /// <summary>
        /// Creates an engine that uses only the built-in filters and tags.
        /// </summary>
        public FluidTemplateEngine() { }

        /// <summary>
        /// Creates an engine that uses the built-in filters and tags plus any extras
        /// declared in <paramref name="options"/>. A fresh parser/options pair is created
        /// for this instance only when <paramref name="options"/> is non-empty.
        /// </summary>
        public FluidTemplateEngine(FluidTemplateEngineOptions options)
        {
            options.GetNonNullOrThrow(nameof(options));

            if (!options.IsEmpty)
            {
                _instanceParser = new FluidPDFParser();
                _instanceTemplateOptions = BuildTemplateOptions(_instanceParser, options);
            }
        }

        public async ValueTask<string> RenderTemplateAsync(string template, DataTable model, FluidPDFTemplateRenderOptions options, string modelName = ModelNames.DefaultModelName)
        {
            FluidPDFTemplateModel fluidPDFModel = FluidPDFTemplateModel.FromDataTable(model, modelName);
            return await RenderTemplateAsync([fluidPDFModel], template, options).ConfigureAwait(false);
        }

        public async ValueTask<string> RenderTemplateAsync(string template, IDictionary<string, object?> model, FluidPDFTemplateRenderOptions options, string modelName = ModelNames.DefaultModelName)
        {
            FluidPDFTemplateModel fluidPDFModel = FluidPDFTemplateModel.FromDictionary(model, modelName);
            return await RenderTemplateAsync([fluidPDFModel], template, options).ConfigureAwait(false);
        }

        public ValueTask<string> RenderTemplateAsync(string template, FluidPDFTemplateModel[] models, FluidPDFTemplateRenderOptions options, string modelName = ModelNames.DefaultModelName) =>
            RenderTemplateAsync(models, template, options);

        public async ValueTask<string> RenderTemplateAsync(string template, object model, FluidPDFTemplateRenderOptions options, string modelName = ModelNames.DefaultModelName)
        {
            FluidPDFTemplateModel fluidPDFModel = FluidPDFTemplateModel.FromObject(model, modelName);
            return await RenderTemplateAsync([fluidPDFModel], template, options).ConfigureAwait(false);
        }

        public async ValueTask<string> RenderTemplateAsync(string template, string jsonModel, FluidPDFTemplateRenderOptions options, string modelName = ModelNames.DefaultModelName)
        {
            FluidPDFTemplateModel fluidPDFModel = FluidPDFTemplateModel.FromJsonString(jsonModel, modelName);
            return await RenderTemplateAsync([fluidPDFModel], template, options).ConfigureAwait(false);
        }

        private async ValueTask<string> RenderTemplateAsync(FluidPDFTemplateModel[] models, string template, FluidPDFTemplateRenderOptions options)
        {
            if (ActiveParser.TryParse(template, out IFluidTemplate? fluidTemplate, out string? error))
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

        private TemplateContext NewTemplateContext(FluidPDFTemplateModel[] models, CultureInfo? cultureInfo = null, TimeZoneInfo? timeZone = null)
        {
            TemplateContext context = new(ActiveTemplateOptions)
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
                        FluidPDFTemplateModelType.JsonNode => model.JsonNode,
                        FluidPDFTemplateModelType.Object => JsonSerializer.SerializeToNode(model.ObjectValue),
                        FluidPDFTemplateModelType.PlainValue => model.PlainValue,
                        _ => throw new ArgumentOutOfRangeException(nameof(model.Type), model.Type, $"Unhandled {nameof(FluidPDFTemplateModelType)}")
                    })
                    .GetNonNullOrThrow(nameof(value));

                context.SetValue(model.Name, value);
            }

            return context;
        }

        private static TemplateOptions BuildTemplateOptions(FluidPDFParser parser, FluidTemplateEngineOptions? userOptions)
        {
            TemplateOptions templateOptions = new()
            {
                Trimming = TrimmingFlags.TagRight
            };

            templateOptions.ValueConverters.Add(x => x is DBNull ? NilValue.Instance : null);

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

            // Register built-in filters and tags
            FluidFilters.Register(templateOptions);
            FluidTags.Register(parser);

            // Register user-supplied extras when present
            if (userOptions is not null)
            {
                foreach ((string name, FilterDelegate @delegate) in userOptions.Filters)
                {
                    templateOptions.Filters.AddFilter(name, @delegate);
                }

                foreach ((string name, Func<TextWriter, TextEncoder, TemplateContext, ValueTask<Completion>> render) in userOptions.EmptyTags)
                {
                    parser.RegisterEmptyTag(name, render);
                }

                foreach ((string name, Func<string, TextWriter, TextEncoder, TemplateContext, ValueTask<Completion>> render) in userOptions.IdentifierTags)
                {
                    parser.RegisterIdentifierTag(name, render);
                }

                foreach ((string name, Func<IReadOnlyList<FilterArgument>, TextWriter, TextEncoder, TemplateContext, ValueTask<Completion>> render) in userOptions.ArgumentTags)
                {
                    parser.RegisterArgumentsTag(name, render);
                }
            }

            return templateOptions;
        }

        public void Dispose()
        {
        }
    }
}
