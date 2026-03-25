using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FluidPDF.Support.IO
{
    internal static class FileHelper
    {
        internal static async Task<string> ReadAllTextAsync(string path, Encoding? encoding = null)
        {
#if NETSTANDARD2_0
            const int _streamWriterDefaultBufferSize = 4096;
            using StreamReader sr = new(path, encoding ?? Encoding.Default, true, _streamWriterDefaultBufferSize);
            return await sr.ReadToEndAsync().ConfigureAwait(false);
#else
            return await File.ReadAllTextAsync(path, encoding ?? Encoding.Default).ConfigureAwait(false);
#endif
        }

        internal static async Task WriteStreamAsync(Stream destinationStream, byte[] data)
        {
#if NETSTANDARD2_0
            await destinationStream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
#else
            await destinationStream.WriteAsync(data).ConfigureAwait(false);
#endif
        }

        internal static void Move(string sourceFileName, string destFileName)
        {
#if NETSTANDARD2_0
            if (File.Exists(destFileName))
            {
                File.Delete(destFileName);
            }
            File.Move(sourceFileName, destFileName);
#else
            File.Move(sourceFileName, destFileName, overwrite: true);
#endif
        }
    }
}
