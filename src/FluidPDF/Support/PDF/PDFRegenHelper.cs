using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.IO;
using System.Threading.Tasks;

namespace FluidPDF.Support.PDF
{
    public static class PDFRegenHelper
    {
        private static PdfDocument RegeneratePDFImpl(PdfDocument inputDocument)
        {
            PdfDocument outputDocument = new();
            foreach (PdfPage page in inputDocument.Pages)
            {
                outputDocument.AddPage(page);
            }
            return outputDocument;
        }

        public static async Task RegeneratePDFAsync(Stream pdfStream, Stream outputDocumentStream)
        {
            using PdfDocument inputDocument = PdfReader.Open(pdfStream, PdfDocumentOpenMode.Import);
            using PdfDocument outputDocument = RegeneratePDFImpl(inputDocument);
            await outputDocument.SaveAsync(outputDocumentStream).ConfigureAwait(false);
        }

        public static async Task<byte[]> RegeneratePDFAsync(Stream pdfStream)
        {
            using MemoryStream outputDocumentStream = new();
            await RegeneratePDFAsync(pdfStream, outputDocumentStream).ConfigureAwait(false);
            return outputDocumentStream.ToArray();
        }
    }
}
