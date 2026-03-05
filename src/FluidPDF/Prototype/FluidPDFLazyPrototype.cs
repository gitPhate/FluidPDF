using FluidPDF.PDF;
using FluidPDF.Support;
using PuppeteerSharp;
using System.IO;
using System.Threading.Tasks;

namespace FluidPDF.Prototype
{
    internal sealed class FluidPDFLazyPrototype(string renderedContent, IBrowser browser, IPage page, PdfOptions pdfOptions, bool toBeCompressed) : IFluidPDFPrototype
    {
        public string RenderedContent { get; } = renderedContent;

        internal IBrowser Browser { get; } = browser.GetNonNullOrThrow(nameof(browser));
        internal IPage Page { get; } = page.GetNonNullOrThrow(nameof(page));
        internal PdfOptions PdfOptions { get; } = pdfOptions.GetNonNullOrThrow(nameof(pdfOptions));
        internal bool ToBeCompressed { get; } = toBeCompressed;

        public async ValueTask<byte[]> ToByteArrayAsync()
        {
            if (!ToBeCompressed)
            {
                return await Page.PdfDataAsync(PdfOptions).ConfigureAwait(false);
            }

            using Stream stream = await Page.PdfStreamAsync(PdfOptions).ConfigureAwait(false);
            return await PDFRegenHelper.RegeneratePDFAsync(stream).ConfigureAwait(false);
        }

        public async ValueTask ToStreamAsync(Stream outputStream)
        {
            using Stream stream = await Page.PdfStreamAsync(PdfOptions).ConfigureAwait(false);
            if (ToBeCompressed)
            {
                await PDFRegenHelper.RegeneratePDFAsync(stream, outputStream).ConfigureAwait(false);
            }
            else
            {
                await stream.CopyToAsync(outputStream).ConfigureAwait(false);
            }

            outputStream.Position = 0;
        }

        public async ValueTask ToFileAsync(string filePath)
        {
            if (!ToBeCompressed)
            {
                await Page.PdfAsync(filePath, PdfOptions).ConfigureAwait(false);
                return;
            }

            using FileStream outputStream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
            using Stream stream = await Page.PdfStreamAsync(PdfOptions).ConfigureAwait(false);
            await PDFRegenHelper.RegeneratePDFAsync(stream, outputStream).ConfigureAwait(false);
            await outputStream.FlushAsync().ConfigureAwait(false);
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
