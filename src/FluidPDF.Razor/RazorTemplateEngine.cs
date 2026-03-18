using FluidPDF.Support.Hashing;
using FluidPDF.Support.IO;
using FluidPDF.Support.Json;
using FluidPDF.Templating;
using RazorEngineCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace FluidPDF.Razor
{
    public sealed class RazorTemplateEngine(RazorCompiledTemplateCacheOptions? cacheOptions = null) : IFluidPDFTemplateEngine
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            Converters = { new ExpandoObjectConverter() }
        };

        private readonly RazorCompiledTemplateCacheOptions? _cacheOptions = cacheOptions;

        public async ValueTask<string> RenderTemplateAsync(string template, DataTable model, FluidPDFTemplateRenderOptions options, string modelName = FluidPDFTemplateModel.DefaultName)
        {
            if (modelName != FluidPDFTemplateModel.DefaultName)
            {
                throw new NotSupportedException($"Razor template engine only supports model name '{FluidPDFTemplateModel.DefaultName}'.");
            }

            FluidPDFTemplateModel managedModel = FluidPDFTemplateModel.FromDataTable(model, modelName);
            return await RenderTemplateAsync(template, [managedModel], options).ConfigureAwait(false);
        }

        public async ValueTask<string> RenderTemplateAsync(string template, IDictionary<string, object> model, FluidPDFTemplateRenderOptions options, string modelName = FluidPDFTemplateModel.DefaultName)
        {
            if (modelName != FluidPDFTemplateModel.DefaultName)
            {
                throw new NotSupportedException($"Razor template engine only supports model name '{FluidPDFTemplateModel.DefaultName}'.");
            }

            FluidPDFTemplateModel managedModel = FluidPDFTemplateModel.FromDictionary(model, modelName);
            return await RenderTemplateAsync(template, [managedModel], options).ConfigureAwait(false);
        }

        public async ValueTask<string> RenderTemplateAsync(string template, string jsonModel, FluidPDFTemplateRenderOptions options, string modelName = FluidPDFTemplateModel.DefaultName)
        {
            if (modelName != FluidPDFTemplateModel.DefaultName)
            {
                throw new NotSupportedException($"Razor template engine only supports model name '{FluidPDFTemplateModel.DefaultName}'.");
            }

            FluidPDFTemplateModel managedModel = FluidPDFTemplateModel.FromJsonString(jsonModel, modelName);
            return await RenderTemplateAsync(template, [managedModel], options).ConfigureAwait(false);
        }

        public async ValueTask<string> RenderTemplateAsync(string template, object model, FluidPDFTemplateRenderOptions options, string modelName = FluidPDFTemplateModel.DefaultName)
        {
            if (modelName != FluidPDFTemplateModel.DefaultName)
            {
                throw new NotSupportedException($"Razor template engine only supports model name '{FluidPDFTemplateModel.DefaultName}'.");
            }

            FluidPDFTemplateModel managedModel = FluidPDFTemplateModel.FromObject(model, modelName);
            return await RenderTemplateAsync(template, [managedModel], options).ConfigureAwait(false);
        }

        public async ValueTask<string> RenderTemplateAsync(string template, FluidPDFTemplateModel[] models, FluidPDFTemplateRenderOptions options, string modelName = FluidPDFTemplateModel.DefaultName)
        {
            if (modelName != FluidPDFTemplateModel.DefaultName)
            {
                throw new NotSupportedException($"Razor template engine only supports model name '{FluidPDFTemplateModel.DefaultName}'.");
            }

            if (models.Length == 0)
            {
                throw new ArgumentException("At least one model must be provided.", nameof(models));
            }

            IFluidPDFRazorCompiledTemplate compiled = await GetOrCompileAsync(template, options.EncodeHtml).ConfigureAwait(false);

            object? modelValue = BuildModelValue(models);

            string result = await compiled.RunAsync(modelValue).ConfigureAwait(false);

            return result;
        }

        private async Task<IFluidPDFRazorCompiledTemplate> GetOrCompileAsync(string template, bool encodeHtml)
        {
            if (_cacheOptions is null)
            {
                return await CompileAsync(template, encodeHtml).ConfigureAwait(false);
            }

            string cacheFilePath = GetCacheFilePath(template, encodeHtml);

            if (File.Exists(cacheFilePath))
            {
                RazorEngineCompiledTemplate cached =
                    await RazorEngineCompiledTemplate
                    .LoadFromFileAsync(cacheFilePath)
                    .ConfigureAwait(false);

                return new FluidPDFRazorCachedCompiledTemplate(cached);
            }

            IFluidPDFRazorCompiledTemplate compiled = await CompileAsync(template, encodeHtml).ConfigureAwait(false);

            await SaveToFileAtomicAsync(compiled, cacheFilePath).ConfigureAwait(false);

            return compiled;
        }

        private static async Task<IFluidPDFRazorCompiledTemplate> CompileAsync(string template, bool encodeHtml)
        {
            if (encodeHtml)
            {
                IRazorEngineCompiledTemplate<HTMLEncodedTemplate> compiled = await new RazorEngine()
                .CompileAsync<HTMLEncodedTemplate>(template)
                .ConfigureAwait(false);

                return new FluidPDFRazorHTMLEncodedCompiledTemplate(compiled);
            }
            else
            {
                IRazorEngineCompiledTemplate compiledTemplate =
                    await new RazorEngine()
                    .CompileAsync(template)
                    .ConfigureAwait(false);

                return new FluidPDFRazorCompiledTemplate(compiledTemplate);
            }
        }

        private async Task SaveToFileAtomicAsync(IFluidPDFRazorCompiledTemplate compiledTemplate, string targetPath)
        {
            Directory.CreateDirectory(_cacheOptions!.CachePath);
            string tempPath = targetPath + ".tmp";
            await compiledTemplate.SaveToFileAsync(tempPath).ConfigureAwait(false);
            FileHelper.Move(tempPath, targetPath);
        }

        private string GetCacheFilePath(string template, bool encodeHtml)
        {
            string key = ComputeCacheKey(template, encodeHtml);
            return Path.Combine(_cacheOptions!.CachePath, key);
        }

        private static string ComputeCacheKey(string template, bool encodeHtml) =>
            HashHelper.HashSHA256(template + (encodeHtml? ":encode" : ":plain"));

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
    }
}
