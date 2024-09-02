using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FluidPDF.Support.IO
{
    internal static class AsyncFile
    {
        private const int _streamWriterDefaultBufferSize = 4096;

        public static Task<string> ReadAllTextAsync(string path) => InternalReadAllTextAsync(path);

        public static Task<string> ReadAllTextAsync(string path, Encoding encoding) => InternalReadAllTextAsync(path, encoding);

        private static async Task<string> InternalReadAllTextAsync(string path, Encoding? encoding = null)
        {
            using StreamReader sr = new(path, encoding ?? Encoding.Default, true, _streamWriterDefaultBufferSize);
            return await sr.ReadToEndAsync().ConfigureAwait(false);
        }
    }
}
