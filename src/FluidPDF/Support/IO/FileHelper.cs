using System.Collections.Generic;
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

        internal static async Task<string[]> ReadAllLinesAsync(string path)
        {
#if NETSTANDARD2_0
            const int bufferSize = 4096;
            List<string> lines = [];
            using (StreamReader reader = new(path, Encoding.Default, true, bufferSize))
            {
                string? line;
                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    lines.Add(line);
                }
            }
            return lines.ToArray();
#else
            return await File.ReadAllLinesAsync(path).ConfigureAwait(false);
#endif
        }

        internal static async Task WriteAllBytesAsync(string path, byte[] bytes)
        {
#if NETSTANDARD2_0
            using FileStream fs = new(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
            await fs.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
#else
            await File.WriteAllBytesAsync(path, bytes).ConfigureAwait(false);
#endif
        }
    }
}
