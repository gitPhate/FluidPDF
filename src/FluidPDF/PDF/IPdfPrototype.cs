using System;
using System.IO;
using System.Threading.Tasks;

namespace FluidPDF.Core.PDF
{
    public interface IPdfPrototype : IDisposable, IAsyncDisposable
    {
        Task<byte[]> ToByteArrayAsync();
        Task<Stream> ToStreamAsync();
        Task ToFileAsync(string filePath);
    }
}
