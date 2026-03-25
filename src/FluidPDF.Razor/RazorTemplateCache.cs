using FluidPDF.Support.Hashing;
using FluidPDF.Support.IO;
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
        Task<Stream?> GetRazorTemplateAsync(string template);
        Task SetRazorTemplateAsync(string template, Stream compiledTemplateStream);
    }

    internal sealed class RazorTemplateCache(RazorTemplateCacheOptions options) : IRazorTemplateCache
    {
        public Task<Stream?> GetRazorTemplateAsync(string template)
        {
            string cacheFilePath = GetCacheFilePath(template);

            if (!File.Exists(cacheFilePath))
            {
                return Task.FromResult<Stream?>(null);
            }

            Stream stream = File.OpenRead(cacheFilePath);
            return Task.FromResult<Stream?>(stream);
        }

        public async Task SetRazorTemplateAsync(string template, Stream compiledTemplateStream)
        {
            string cacheFilePath = GetCacheFilePath(template);
            await SaveToFileAtomicAsync(compiledTemplateStream, cacheFilePath).ConfigureAwait(false);
        }

        private async Task SaveToFileAtomicAsync(Stream compiledTemplateStream, string targetPath)
        {
            Directory.CreateDirectory(options.CachePath);
            string tempPath = targetPath + ".tmp";
            if (compiledTemplateStream.CanSeek)
            {
                compiledTemplateStream.Position = 0;
            }

            using FileStream fileStream = File.Create(tempPath);
            await compiledTemplateStream.CopyToAsync(fileStream).ConfigureAwait(false);
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
        public Task<Stream?> GetRazorTemplateAsync(string template) =>
            Task.FromResult<Stream?>(null);

        public Task SetRazorTemplateAsync(string template, Stream compiledTemplateStream) =>
            Task.CompletedTask;
    }
}
