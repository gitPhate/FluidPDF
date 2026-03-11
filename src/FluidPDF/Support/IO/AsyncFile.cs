using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FluidPDF.Support.IO
{
    internal static class AsyncFile
    {
        public static async Task<string> ReadAllTextAsync(string path, Encoding? encoding = null)
        {
#if NETSTANDARD2_0
            const int _streamWriterDefaultBufferSize = 4096;
            using StreamReader sr = new(path, encoding ?? Encoding.Default, true, _streamWriterDefaultBufferSize);
            return await sr.ReadToEndAsync().ConfigureAwait(false);
#else
            return await File.ReadAllTextAsync(path, encoding ?? Encoding.Default).ConfigureAwait(false);
#endif
        }
    }
}
