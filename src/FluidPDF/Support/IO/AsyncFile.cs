using Kyklos.Kernel.Core.Asserts;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FluidPDF.Core.Support.IO
{
    internal static class AsyncFile
    {
        private const int _StreamWriterDefaultBufferSize = 1024;

        public static Task<string> ReadAllTextAsync(string path) => InternalReadAllTextAsync(path);

        public static Task<string> ReadAllTextAsync(string path, Encoding encoding) => InternalReadAllTextAsync(path, encoding);

        private static async Task<string> InternalReadAllTextAsync(string path, Encoding? encoding = null)
        {
            path.AssertArgumentHasText(nameof(path));

            using StreamReader sr = new(path, encoding ?? Encoding.Default, true, _StreamWriterDefaultBufferSize);
            return await sr.ReadToEndAsync().ConfigureAwait(false);
        }
    }
}
