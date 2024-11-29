using System;
using System.IO;
using System.Threading.Tasks;

namespace FluidPDF.Prototype
{
    public interface IPdfPrototype : IDisposable, IAsyncDisposable
    {
        string RenderedContent { get; }

        Task<byte[]> ToByteArrayAsync();
        Task ToStreamAsync(Stream outputStream);
        Task ToFileAsync(string filePath);
    }
}
