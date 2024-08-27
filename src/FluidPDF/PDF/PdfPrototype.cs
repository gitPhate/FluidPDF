using Kyklos.Kernel.Core.Asserts;
using PuppeteerSharp;
using Sisifo.PDF;
using System.IO;
using System.Threading.Tasks;

namespace FluidPDF.PDF
{
    internal sealed class PdfPrototype : IPdfPrototype
    {
        internal IBrowser Browser { get; }
        internal IPage Page { get; }
        internal PdfOptions PdfOptions { get; }
        internal bool ToBeCompressed { get; }

        internal PdfPrototype(IBrowser browser, IPage page, PdfOptions pdfOptions, bool toBeCompressed)
        {
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
