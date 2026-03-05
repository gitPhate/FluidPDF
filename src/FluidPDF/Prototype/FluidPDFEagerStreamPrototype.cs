using FluidPDF.PDF;
using System.IO;
using System.Threading.Tasks;

namespace FluidPDF.Prototype
{
    internal sealed class FluidPDFEagerStreamPrototype(Stream stream, string renderedContent, bool toBeCompressed) : IFluidPDFPrototype
    {
        private readonly Stream _stream = stream;

        public string RenderedContent { get; } = renderedContent;

        internal bool ToBeCompressed { get; } = toBeCompressed;

        public ValueTask DisposeAsync()
        {
            Dispose();
            return default;
        }

        public async ValueTask<byte[]> ToByteArrayAsync()
        {
            if (!ToBeCompressed)
            {
                using MemoryStream ms = new();
                await _stream.CopyToAsync(ms).ConfigureAwait(false);
                return ms.ToArray();
            }

            return await PDFRegenHelper.RegeneratePDFAsync(_stream).ConfigureAwait(false);
        }

        public async ValueTask ToFileAsync(string filePath)
        {
            using FileStream outputStream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);

            if (ToBeCompressed)
            {
                await PDFRegenHelper.RegeneratePDFAsync(_stream, outputStream).ConfigureAwait(false);
            }
            else
            {
                await _stream.CopyToAsync(outputStream).ConfigureAwait(false);
            }

            await outputStream.FlushAsync().ConfigureAwait(false);
        }

        public async ValueTask ToStreamAsync(Stream outputStream)
        {
            if (!ToBeCompressed)
            {
                await _stream.CopyToAsync(outputStream).ConfigureAwait(false);
            }
            else
            {
                await PDFRegenHelper.RegeneratePDFAsync(_stream, outputStream).ConfigureAwait(false);
            }

            outputStream.Position = 0;
        }

        public void Dispose() => _stream?.Dispose();
    }
}
