using Kyklos.Kernel.Core.Asserts;
using Kyklos.Kernel.Core.Strings;
using PuppeteerSharp;
using PuppeteerSharp.BrowserData;
using System.IO;
using System.Threading.Tasks;

namespace FluidPDF.PuppeteerSharp
{
    internal record ChromiumRetrieverOptions(string? ExternalExecutablePath, string? DownloadPath, string? Revision = null)
    {
        internal ChromiumRetrieverOptions(string? externalExecutablePath) : this(externalExecutablePath, null, null) { }
    }

    internal static class ChromiumRetriever
    {
        internal static async Task<IBrowser> RetrieveBrowserInstanceAsync(ChromiumRetrieverOptions options)
        {
            string? exePath = options.ExternalExecutablePath;

            if (options.ExternalExecutablePath.IsNullOrBlankString() || !File.Exists(options.ExternalExecutablePath))
            {
                InstalledBrowser browser = await FetchCromiumAsync(options).ConfigureAwait(false);
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

        private static async Task<InstalledBrowser> FetchCromiumAsync(ChromiumRetrieverOptions options)
        {
            options.AssertArgumentNotNull(nameof(options));

            InstalledBrowser browser =
                await new BrowserFetcher
                (
                    new BrowserFetcherOptions
                    {
                        Path = options.DownloadPath,
                    }
                )
                .DownloadAsync(options.Revision ?? Chrome.DefaultBuildId)
                .ConfigureAwait(false);

            return browser;
        }
    }
}
