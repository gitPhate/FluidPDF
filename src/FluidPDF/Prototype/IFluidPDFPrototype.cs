using System;
using System.IO;
using System.Threading.Tasks;

namespace FluidPDF.Prototype
{
    public interface IFluidPDFPrototype : IDisposable, IAsyncDisposable
    {
        string RenderedContent { get; }

        ValueTask<byte[]> ToByteArrayAsync();
        ValueTask ToStreamAsync(Stream outputStream);
        ValueTask ToFileAsync(string filePath);
    }
}
