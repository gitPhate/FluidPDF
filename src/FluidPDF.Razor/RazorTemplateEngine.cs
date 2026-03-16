using FluidPDF.Support.Json;
using FluidPDF.Templating;
using RazorEngineCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace FluidPDF.Razor
{
    public sealed class RazorTemplateEngine : IFluidPDFTemplateEngine
    {
        public async ValueTask<string> RenderTemplateAsync(string template, DataTable model, FluidPDFTemplateRenderOptions options)
        {
            FluidPDFTemplateModel managedModel = FluidPDFTemplateModel.FromDataTable(options.ModelName, model);
            return await RenderTemplateAsync(template, [managedModel], options).ConfigureAwait(false);
        }

        public async ValueTask<string> RenderTemplateAsync(string template, IDictionary<string, object> model, FluidPDFTemplateRenderOptions options)
        {
            FluidPDFTemplateModel managedModel = FluidPDFTemplateModel.FromDictionary(options.ModelName, model);
            return await RenderTemplateAsync(template, [managedModel], options).ConfigureAwait(false);
        }

        public async ValueTask<string> RenderTemplateAsync(string template, string jsonModel, FluidPDFTemplateRenderOptions options)
        {
            FluidPDFTemplateModel managedModel = FluidPDFTemplateModel.FromJsonString(options.ModelName, jsonModel);
            return await RenderTemplateAsync(template, [managedModel], options).ConfigureAwait(false);
        }

        public async ValueTask<string> RenderTemplateAsync(string template, object model, FluidPDFTemplateRenderOptions options)
        {
            FluidPDFTemplateModel managedModel = FluidPDFTemplateModel.FromObject(options.ModelName, model);
            return await RenderTemplateAsync(template, [managedModel], options).ConfigureAwait(false);
        }

        public ValueTask<string> RenderTemplateAsync(string template, FluidPDFTemplateModel[] models, FluidPDFTemplateRenderOptions options) =>
            RenderCoreAsync(template, models);

        private static async ValueTask<string> RenderCoreAsync(string template, FluidPDFTemplateModel[] models)
        {
            if (models.Length == 0)
            {
                throw new ArgumentException("At least one model must be provided.", nameof(models));
            }

            object? modelValue = BuildModelValue(models);

            RazorEngine razorEngine = new();
            IRazorEngineCompiledTemplate compiled =
                await razorEngine
                    .CompileAsync(template)
                    .ConfigureAwait(false);

            string result = await compiled
                .RunAsync(modelValue)
                .ConfigureAwait(false);

            return result;
        }

        private static object? BuildModelValue(FluidPDFTemplateModel[] models)
        {
            if (models.Length == 1)
            {
                return ConvertModel(models.First());
            }

            // Multiple models: merge into a single ExpandoObject keyed by model.Name
            ExpandoObject expando = new();
            IDictionary<string, object?> expandoDict = expando;

            foreach (FluidPDFTemplateModel model in models)
            {
                if (expandoDict.ContainsKey(model.Name))
                {
                    throw new ArgumentException("Some models with the same name have already been added");
                }

                expandoDict[model.Name] = ConvertModel(model);
            }

            return expando;
        }

        private static object? ConvertModel(FluidPDFTemplateModel model)
        {
            return model.Type switch
            {
                FluidPDFTemplateModelType.Object => SerializeToExpando(model.ObjectValue!),
                FluidPDFTemplateModelType.Dictionary => DictionaryToExpando(model.Dictionary!),
                FluidPDFTemplateModelType.JsonString => JsonStringToExpando(model.JsonString!),
                FluidPDFTemplateModelType.DataRow => DataRowToExpando(model.DataRow!),
                FluidPDFTemplateModelType.DataTable => DataTableToExpando(model.DataTable!),
                FluidPDFTemplateModelType.PlainValue => model.PlainValue,
                _ => throw new ArgumentOutOfRangeException(nameof(model), model.Type, $"Unsupported model type: {model.Type}")
            };
        }

        private static ExpandoObject SerializeToExpando(object obj)
        {
            string json = JsonSerializer.Serialize(obj);
            return JsonStringToExpando(json);
        }

        private static ExpandoObject JsonStringToExpando(string json)
        {
            ExpandoObject? result = JsonSerializer.Deserialize<ExpandoObject>(json, _jsonOptions);
            return result ?? new ExpandoObject();
        }

        private static ExpandoObject DictionaryToExpando(IDictionary<string, object> dictionary)
        {
            ExpandoObject expando = new();
            IDictionary<string, object?> expandoDict = expando;

            foreach (KeyValuePair<string, object> kvp in dictionary)
            {
                expandoDict[kvp.Key] = kvp.Value;
            }

            return expando;
        }

        private static ExpandoObject DataRowToExpando(DataRow row)
        {
            ExpandoObject expando = new();
            IDictionary<string, object?> expandoDict = expando;

            foreach (DataColumn column in row.Table.Columns)
            {
                expandoDict[column.ColumnName] = row.IsNull(column) ? null : row[column];
            }

            return expando;
        }

        private static ExpandoObject DataTableToExpando(DataTable table)
        {
            List<ExpandoObject> rows = [];

            foreach (DataRow row in table.Rows)
            {
                rows.Add(DataRowToExpando(row));
            }

            ExpandoObject expando = new();
            IDictionary<string, object?> expandoDict = expando;
            expandoDict[nameof(DataTable.Rows)] = rows;
            return expando;
        }

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            Converters = { new ExpandoObjectConverter() }
        };
    }
}
