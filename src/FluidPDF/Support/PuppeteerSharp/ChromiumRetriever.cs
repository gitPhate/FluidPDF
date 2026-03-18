using PuppeteerSharp;
using PuppeteerSharp.BrowserData;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FluidPDF.Support.PuppeteerSharp
{
    public record ChromiumRetrieverOptions(string? ExternalExecutablePath, string? DownloadPath, string? Revision = null)
    {
        internal ChromiumRetrieverOptions(string? externalExecutablePath) : this(externalExecutablePath, null, null) { }
    }

    internal interface IChromiumRetriever
    {
        Task<IBrowser> LaunchBrowserAsync();
    }

    internal sealed class ChromiumRetriever(ChromiumRetrieverOptions options) : IChromiumRetriever
    {
        private readonly ChromiumRetrieverOptions _options = options.GetNonNullOrThrow(nameof(options));

        public async Task<IBrowser> LaunchBrowserAsync()
        {
            string? exePath = _options.ExternalExecutablePath;

            if (_options.ExternalExecutablePath.IsNullOrBlankString() || !File.Exists(_options.ExternalExecutablePath))
            {
                InstalledBrowser browser = await FetchChromiumAsync(_options).ConfigureAwait(false);
                exePath = browser.GetExecutablePath();
            }

            LaunchOptions browserOptions =
                new()
                {
                    Headless = true,
                    ExecutablePath = exePath
                };

            return await Puppeteer.LaunchAsync(browserOptions).ConfigureAwait(false);
        }

        private static async Task<InstalledBrowser> FetchChromiumAsync(ChromiumRetrieverOptions opts)
        {
            InstalledBrowser browser =
                await new BrowserFetcher
                (
                    new BrowserFetcherOptions
                    {
                        Path = opts.DownloadPath,
                    }
                )
                .DownloadAsync(opts.Revision ?? Chrome.DefaultBuildId)
                .ConfigureAwait(false);

            return browser;
        }
    }
}
