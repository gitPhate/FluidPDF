using FluidPDF.Support.Hashing;
using FluidPDF.Support.IO;
using FluidPDF.Templating;
using RazorEngineCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace FluidPDF.Razor
{
    public sealed class RazorTemplateEngine(RazorCompiledTemplateCacheOptions? cacheOptions = null) : IFluidPDFTemplateEngine
    {
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
            RazorRuntimeModel runtimeModel = new(models);

            string result = await compiled.RunAsync(runtimeModel, options.EncodeHtml).ConfigureAwait(false);

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
    }
}
