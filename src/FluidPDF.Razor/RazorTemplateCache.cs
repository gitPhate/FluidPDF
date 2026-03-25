using FluidPDF.Support.Hashing;
using FluidPDF.Support.IO;
using System.Collections.Concurrent;
using System;
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
        ValueTask<Stream?> GetRazorTemplateAsync(string template);
        ValueTask SetRazorTemplateAsync(string template, Stream compiledTemplateStream);
    }

    internal sealed class RazorTemplateCache(RazorTemplateCacheOptions options) : IRazorTemplateCache
    {
        public ValueTask<Stream?> GetRazorTemplateAsync(string template)
        {
            string cacheFilePath = GetCacheFilePath(template);

            if (!File.Exists(cacheFilePath))
            {
                return default;
            }

            Stream stream = File.OpenRead(cacheFilePath);
            return new(stream);
        }

        public async ValueTask SetRazorTemplateAsync(string template, Stream compiledTemplateStream)
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
        public ValueTask<Stream?> GetRazorTemplateAsync(string template) => default;

        public ValueTask SetRazorTemplateAsync(string template, Stream compiledTemplateStream) => default;
    }

    public sealed class InMemoryRazorTemplateCache : IRazorTemplateCache
    {
        private readonly ConcurrentDictionary<string, byte[]> _templates = new(StringComparer.Ordinal);

        public ValueTask<Stream?> GetRazorTemplateAsync(string template)
        {
            if (!_templates.TryGetValue(template, out byte[]? compiledTemplate))
            {
                return default;
            }

            Stream stream = new MemoryStream(compiledTemplate, writable: false);
            return new(stream);
        }

        public async ValueTask SetRazorTemplateAsync(string template, Stream compiledTemplateStream)
        {
            if (compiledTemplateStream.CanSeek)
            {
                compiledTemplateStream.Position = 0;
            }

            using MemoryStream memoryStream = new();
            await compiledTemplateStream.CopyToAsync(memoryStream).ConfigureAwait(false);
            _templates[template] = memoryStream.ToArray();
        }
    }
}
