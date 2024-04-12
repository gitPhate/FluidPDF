using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.IO;
using System.Text;

namespace FluidPDF.Core.PDFSharp
{
    public static class PDFSharpUtils
    {
        private static PdfDocument RegeneratePDFImpl(PdfDocument inputDocument)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            PdfDocument outputDocument = new();
            foreach (PdfPage page in inputDocument.Pages)
            {
                outputDocument.AddPage(page);
            }
            return outputDocument;
        }

        public static void RegeneratePDF(Stream pdfStream, Stream outputDocumentStream)
        {
            using PdfDocument inputDocument = PdfReader.Open(pdfStream, PdfDocumentOpenMode.Import);
            using PdfDocument outputDocument = RegeneratePDFImpl(inputDocument);
            outputDocument.Save(outputDocumentStream);
        }

        public static byte[] RegeneratePDF(Stream pdfStream)
        {
            using MemoryStream outputDocumentStream = new();
            RegeneratePDF(pdfStream, outputDocumentStream);
            return outputDocumentStream.ToArray();
        }
    }
}
