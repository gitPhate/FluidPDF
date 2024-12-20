using FluidPDF.PDF;
using FluidPDF.Support;
using PuppeteerSharp;
using System.IO;
using System.Threading.Tasks;

namespace FluidPDF.Prototype
{
    internal sealed class PdfPrototype : IPdfPrototype
    {
        public string RenderedContent { get; }

        internal IBrowser Browser { get; }
        internal IPage Page { get; }
        internal PdfOptions PdfOptions { get; }
        internal bool ToBeCompressed { get; }

        internal PdfPrototype(string renderedContent, IBrowser browser, IPage page, PdfOptions pdfOptions, bool toBeCompressed)
        {
            RenderedContent = renderedContent;
            Browser = browser.GetNonNullOrThrow(nameof(browser));
            Page = page.GetNonNullOrThrow(nameof(page));
            PdfOptions = pdfOptions.GetNonNullOrThrow(nameof(pdfOptions));
            ToBeCompressed = toBeCompressed;
        }

        public async Task<byte[]> ToByteArrayAsync()
        {
            if (!ToBeCompressed)
            {
                return await Page.PdfDataAsync(PdfOptions).ConfigureAwait(false);
            }

            using Stream stream = await Page.PdfStreamAsync(PdfOptions).ConfigureAwait(false);
            return PDFRegenHelper.RegeneratePDF(stream);
        }

        public async Task ToStreamAsync(Stream outputStream)
        {
            using Stream stream = await Page.PdfStreamAsync(PdfOptions).ConfigureAwait(false);
            if (ToBeCompressed)
            {
                PDFRegenHelper.RegeneratePDF(stream, outputStream);
            }
            else
            {
                await stream.CopyToAsync(outputStream).ConfigureAwait(false);
            }
        }

        public async Task ToFileAsync(string filePath)
        {
            if (!ToBeCompressed)
            {
                await Page.PdfAsync(filePath, PdfOptions);
            }
            using FileStream outputStream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
            using Stream stream = await Page.PdfStreamAsync(PdfOptions).ConfigureAwait(false);
            PDFRegenHelper.RegeneratePDF(stream, outputStream);
        }

        public void Dispose()
        {
            if (!Page.IsClosed)
            {
                Page.CloseAsync().GetAwaiter().GetResult();
            }

            if (!Browser.IsClosed)
            {
                Browser.CloseAsync().GetAwaiter().GetResult();
            }

            Page.Dispose();
            Browser.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await Page.CloseAsync().ConfigureAwait(false);
            await Browser.CloseAsync().ConfigureAwait(false);
            await Page.DisposeAsync().ConfigureAwait(false);
            await Browser.DisposeAsync().ConfigureAwait(false);
        }
    }
}
