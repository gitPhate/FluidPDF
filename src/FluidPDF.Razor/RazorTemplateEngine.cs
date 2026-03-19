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

        public async ValueTask<string> RenderTemplateAsync(string template, DataTable model, FluidPDFTemplateRenderOptions options, string modelName = ModelNames.DefaultModelName)
        {
            if (modelName != ModelNames.DefaultModelName)
            {
                throw new NotSupportedException($"Razor template engine only supports model name '{ModelNames.DefaultModelName}'.");
            }

            FluidPDFTemplateModel managedModel = FluidPDFTemplateModel.FromDataTable(model, modelName);
            return await RenderTemplateAsync(template, [managedModel], options).ConfigureAwait(false);
        }

        public async ValueTask<string> RenderTemplateAsync(string template, IDictionary<string, object> model, FluidPDFTemplateRenderOptions options, string modelName = ModelNames.DefaultModelName)
        {
            if (modelName != ModelNames.DefaultModelName)
            {
                throw new NotSupportedException($"Razor template engine only supports model name '{ModelNames.DefaultModelName}'.");
            }

            FluidPDFTemplateModel managedModel = FluidPDFTemplateModel.FromDictionary(model, modelName);
            return await RenderTemplateAsync(template, [managedModel], options).ConfigureAwait(false);
        }

        public async ValueTask<string> RenderTemplateAsync(string template, string jsonModel, FluidPDFTemplateRenderOptions options, string modelName = ModelNames.DefaultModelName)
        {
            if (modelName != ModelNames.DefaultModelName)
            {
                throw new NotSupportedException($"Razor template engine only supports model name '{ModelNames.DefaultModelName}'.");
            }

            FluidPDFTemplateModel managedModel = FluidPDFTemplateModel.FromJsonString(jsonModel, modelName);
            return await RenderTemplateAsync(template, [managedModel], options).ConfigureAwait(false);
        }

        public async ValueTask<string> RenderTemplateAsync(string template, object model, FluidPDFTemplateRenderOptions options, string modelName = ModelNames.DefaultModelName)
        {
            if (modelName != ModelNames.DefaultModelName)
            {
                throw new NotSupportedException($"Razor template engine only supports model name '{ModelNames.DefaultModelName}'.");
            }

            FluidPDFTemplateModel managedModel = FluidPDFTemplateModel.FromObject(model, modelName);
            return await RenderTemplateAsync(template, [managedModel], options).ConfigureAwait(false);
        }

        public async ValueTask<string> RenderTemplateAsync(string template, FluidPDFTemplateModel[] models, FluidPDFTemplateRenderOptions options, string modelName = ModelNames.DefaultModelName)
        {
            if (modelName != ModelNames.DefaultModelName)
            {
                throw new NotSupportedException($"Razor template engine only supports model name '{ModelNames.DefaultModelName}'.");
            }

            if (models.Length == 0)
            {
                throw new ArgumentException("At least one model must be provided.", nameof(models));
            }

            IFluidPDFRazorCompiledTemplate compiled = await GetOrCompileAsync(template).ConfigureAwait(false);

            object? modelValue = BuildModelValue(models);
            dynamic? resxValue = BuildResxValue(models);

            string result = await compiled.RunAsync(modelValue, resxValue, options.EncodeHtml).ConfigureAwait(false);

            return result;
        }

        private async Task<IFluidPDFRazorCompiledTemplate> GetOrCompileAsync(string template)
        {
            if (_cacheOptions is null)
            {
                return await CompileAsync(template).ConfigureAwait(false);
            }

            string cacheFilePath = GetCacheFilePath(template);

            if (File.Exists(cacheFilePath))
            {
                RazorEngineCompiledTemplate cached =
                    await RazorEngineCompiledTemplate
                    .LoadFromFileAsync(cacheFilePath)
                    .ConfigureAwait(false);

                return new FluidPDFRazorCachedCompiledTemplate(cached);
            }

            IFluidPDFRazorCompiledTemplate compiled = await CompileAsync(template).ConfigureAwait(false);

            await SaveToFileAtomicAsync(compiled, cacheFilePath).ConfigureAwait(false);

            return compiled;
        }

        private static async Task<IFluidPDFRazorCompiledTemplate> CompileAsync(string template)
        {
            IRazorEngineCompiledTemplate<FluidPDFRazorTemplateBase> compiledTemplate =
                await new RazorEngine()
                .CompileAsync<FluidPDFRazorTemplateBase>(template)
                .ConfigureAwait(false);

            return new FluidPDFRazorCompiledTemplate(compiledTemplate);
        }

        private async Task SaveToFileAtomicAsync(IFluidPDFRazorCompiledTemplate compiledTemplate, string targetPath)
        {
            Directory.CreateDirectory(_cacheOptions!.CachePath);
            string tempPath = targetPath + ".tmp";
            await compiledTemplate.SaveToFileAsync(tempPath).ConfigureAwait(false);
            FileHelper.Move(tempPath, targetPath);
        }

        private string GetCacheFilePath(string template)
        {
            string key = ComputeCacheKey(template);
            return Path.Combine(_cacheOptions!.CachePath, key);
        }

        private static string ComputeCacheKey(string template) =>
            HashHelper.HashSHA256(template);

        private static object? BuildModelValue(FluidPDFTemplateModel[] models)
        {
            FluidPDFTemplateModel[] nonResxModels = models
                .Where(model => !string.Equals(model.Name, ModelNames.ResxModelName, StringComparison.Ordinal))
                .ToArray();

            if (nonResxModels.Length == 1 && string.Equals(nonResxModels[0].Name, ModelNames.DefaultModelName, StringComparison.Ordinal))
            {
                return ConvertModel(nonResxModels[0]);
            }

            ExpandoObject expando = new();
            IDictionary<string, object?> expandoDict = expando;

            foreach (FluidPDFTemplateModel model in nonResxModels)
            {
                if (expandoDict.ContainsKey(model.Name))
                {
                    throw new ArgumentException("Some models with the same name have already been added");
                }

                expandoDict[model.Name] = ConvertModel(model);
            }

            return expando;
        }

        private static object? BuildResxValue(FluidPDFTemplateModel[] models)
        {
            FluidPDFTemplateModel? resxModel = models.FirstOrDefault(model =>
                string.Equals(model.Name, ModelNames.ResxModelName, StringComparison.Ordinal));

            if (resxModel is null)
            {
                return null;
            }

            return ConvertModel(resxModel);
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
