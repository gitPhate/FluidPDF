using FluidPDF.Support.Hashing;
using FluidPDF.Support.IO;
using RazorEngineCore;
using System.IO;
using System.Threading.Tasks;

namespace FluidPDF.Razor
{
    public sealed class RazorTemplateCacheOptions(string cachePath)
    {
        public string CachePath { get; } = cachePath;
    }

    public interface IRazorTemplateCache
    {
        Task<IFluidPDFRazorCompiledTemplate?> GetRazorTemplateAsync(string template);
        Task SetRazorTemplateAsync(string template, IFluidPDFRazorCompiledTemplate compiledTemplate);
    }

    internal sealed class RazorTemplateCache(RazorTemplateCacheOptions options) : IRazorTemplateCache
    {
        public async Task<IFluidPDFRazorCompiledTemplate?> GetRazorTemplateAsync(string template)
        {
            string cacheFilePath = GetCacheFilePath(template);

            if (!File.Exists(cacheFilePath))
            {
                return null;
            }

            RazorEngineCompiledTemplate cached =
                await RazorEngineCompiledTemplate
                    .LoadFromFileAsync(cacheFilePath)
                    .ConfigureAwait(false);

            return new FluidPDFRazorCachedCompiledTemplate(cached);
        }

        public async Task SetRazorTemplateAsync(string template, IFluidPDFRazorCompiledTemplate compiledTemplate)
        {
            string cacheFilePath = GetCacheFilePath(template);
            await SaveToFileAtomicAsync((IInternalFluidPDFRazorCompiledTemplate)compiledTemplate, cacheFilePath).ConfigureAwait(false);
        }

        private async Task SaveToFileAtomicAsync(IInternalFluidPDFRazorCompiledTemplate compiledTemplate, string targetPath)
        {
            Directory.CreateDirectory(options.CachePath);
            string tempPath = targetPath + ".tmp";
            await compiledTemplate.SaveToFileAsync(tempPath).ConfigureAwait(false);
            FileHelper.Move(tempPath, targetPath);
        }

        private string GetCacheFilePath(string template)
        {
            string key = ComputeCacheKey(template);
            return Path.Combine(options.CachePath, key);
        }

        private static string ComputeCacheKey(string template) =>
            HashHelper.HashSHA256(template);
    }

    internal sealed class NullRazorTemplateCache : IRazorTemplateCache
    {
        public Task<IFluidPDFRazorCompiledTemplate?> GetRazorTemplateAsync(string template) =>
            Task.FromResult<IFluidPDFRazorCompiledTemplate?>(null);

        public Task SetRazorTemplateAsync(string template, IFluidPDFRazorCompiledTemplate compiledTemplate) =>
            Task.CompletedTask;
    }
}
