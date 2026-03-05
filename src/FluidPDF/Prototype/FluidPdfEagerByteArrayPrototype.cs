using FluidPDF.PDF;
using System.IO;
using System.Threading.Tasks;

namespace FluidPDF.Prototype
{
    internal sealed class FluidPdfEagerByteArrayPrototype(byte[] data, string renderedContent, bool toBeCompressed) : IFluidPdfPrototype
    {
        private byte[] _data = data;

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
                return _data;
            }

            using MemoryStream ms = new(_data, false);
            return await PDFRegenHelper.RegeneratePDFAsync(ms).ConfigureAwait(false);
        }

        public async ValueTask ToFileAsync(string filePath)
        {
            if (ToBeCompressed)
            {
                using MemoryStream ms = new(_data, false);
                using FileStream outputStream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
                await PDFRegenHelper.RegeneratePDFAsync(ms, outputStream).ConfigureAwait(false);
            }
            else
            {
                File.WriteAllBytes(filePath, _data);
            }
        }

        public async ValueTask ToStreamAsync(Stream outputStream)
        {
            await outputStream.WriteAsync(_data, 0, _data.Length).ConfigureAwait(false);
            outputStream.Position = 0;
        }

        public void Dispose() => _data = null!;
    }
}
