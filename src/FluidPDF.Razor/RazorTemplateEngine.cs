using FluidPDF.Templating;
using RazorEngineCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace FluidPDF.Razor
{
    public sealed class RazorTemplateEngine(IRazorTemplateCache templateCache) : IFluidPDFTemplateEngine
    {
        private readonly IRazorTemplateCache _templateCache = templateCache;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _compileLocks = new();

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

            IInternalFluidPDFRazorCompiledTemplate compiled = await GetOrCompileAsync(template).ConfigureAwait(false);
            RazorRuntimeModel runtimeModel = new(models);

            string result = await compiled.RunAsync(runtimeModel, options.EncodeHtml).ConfigureAwait(false);

            return result;
        }

        private async Task<IInternalFluidPDFRazorCompiledTemplate> GetOrCompileAsync(string template)
        {
            IFluidPDFRazorCompiledTemplate? cached =
                await _templateCache
                    .GetRazorTemplateAsync(template)
                    .ConfigureAwait(false);

            if (cached is IInternalFluidPDFRazorCompiledTemplate hit)
            {
                return hit;
            }

            SemaphoreSlim semaphore = _compileLocks.GetOrAdd(template, _ => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                // Double-check: another thread may have compiled and cached while we were waiting.
                cached = await _templateCache.GetRazorTemplateAsync(template).ConfigureAwait(false);
                if (cached is IInternalFluidPDFRazorCompiledTemplate doubleCheckHit)
                {
                    return doubleCheckHit;
                }

                IInternalFluidPDFRazorCompiledTemplate compiled = await CompileAsync(template).ConfigureAwait(false);
                await _templateCache.SetRazorTemplateAsync(template, compiled).ConfigureAwait(false);
                return compiled;
            }
            finally
            {
                semaphore.Release();
            }
        }

        private static async Task<IInternalFluidPDFRazorCompiledTemplate> CompileAsync(string template)
        {
            IRazorEngineCompiledTemplate<FluidPDFRazorTemplateBase> compiledTemplate =
                await new RazorEngine()
                .CompileAsync<FluidPDFRazorTemplateBase>(template)
                .ConfigureAwait(false);

            return new FluidPDFRazorCompiledTemplate(compiledTemplate);
        }
    }
}

