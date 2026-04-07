using FluidPDF.Templating;
using RazorEngineCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FluidPDF.Razor
{
    public sealed class RazorTemplateEngine(IRazorTemplateCache templateCache) : IFluidPDFTemplateEngine
    {
        private readonly IRazorTemplateCache _templateCache = templateCache;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _compileLocks = [];

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
            IFluidPDFRazorCompiledTemplate? cached =
                await TryLoadFromCacheAsync(template)
                    .ConfigureAwait(false);

            if (cached is not null)
            {
                return cached;
            }

            SemaphoreSlim semaphore = _compileLocks.GetOrAdd(template, _ => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                // Double-check: another thread may have compiled and cached while we were waiting.
                cached = await TryLoadFromCacheAsync(template).ConfigureAwait(false);
                if (cached is not null)
                {
                    return cached;
                }

                IFluidPDFRazorCompiledTemplate compiled = await CompileAsync(template).ConfigureAwait(false);
                await SaveToCacheAsync(template, compiled).ConfigureAwait(false);
                return compiled;
            }
            finally
            {
                semaphore.Release();
            }
        }

        private async Task<IFluidPDFRazorCompiledTemplate?> TryLoadFromCacheAsync(string template)
        {
            Stream? cachedStream =
                await _templateCache
                    .GetRazorTemplateAsync(template)
                    .ConfigureAwait(false);

            if (cachedStream is null)
            {
                return null;
            }

            using (cachedStream)
            {
                RazorEngineCompiledTemplate compiledTemplate =
                    await RazorEngineCompiledTemplate
                        .LoadFromStreamAsync(cachedStream)
                        .ConfigureAwait(false);

                return new FluidPDFRazorCachedCompiledTemplate(compiledTemplate);
            }
        }

        private async Task SaveToCacheAsync(string template, IFluidPDFRazorCompiledTemplate compiledTemplate)
        {
            using MemoryStream stream = new();
            await compiledTemplate.SaveToStreamAsync(stream).ConfigureAwait(false);
            stream.Position = 0;
            await _templateCache.SetRazorTemplateAsync(template, stream).ConfigureAwait(false);
        }

        private static async Task<IFluidPDFRazorCompiledTemplate> CompileAsync(string template)
        {
            IRazorEngineCompiledTemplate<FluidPDFRazorTemplateBase> compiledTemplate =
                await new RazorEngine()
                .CompileAsync<FluidPDFRazorTemplateBase>(template)
                .ConfigureAwait(false);

            return new FluidPDFRazorEngineCompiledTemplate(compiledTemplate);
        }

        public void Dispose()
        {
            foreach (SemaphoreSlim semaphore in _compileLocks.Values)
            {
                semaphore.Dispose();
            }

            _compileLocks.Clear();
        }
    }
}

