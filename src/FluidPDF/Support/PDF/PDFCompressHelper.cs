using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.IO;
using System.Threading.Tasks;

namespace FluidPDF.Support.PDF
{
    public static class PDFCompressHelper
    {
        public static async Task<byte[]> CompressPDFAsync(byte[] data)
        {
            using MemoryStream pdfStream = new(data);
            using MemoryStream compressedStream = new();
            using PdfDocument inputDocument = PdfReader.Open(pdfStream, PdfDocumentOpenMode.Import);
            using PdfDocument outputDocument = new();

            foreach (PdfPage page in inputDocument.Pages)
            {
                outputDocument.AddPage(page);
            }

            await outputDocument.SaveAsync(compressedStream).ConfigureAwait(false);

            return compressedStream.ToArray();
        }
    }
}
