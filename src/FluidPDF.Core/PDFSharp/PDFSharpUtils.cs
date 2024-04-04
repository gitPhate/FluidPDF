using Kyklos.Kernel.Core.Support;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FluidPDF.Core.PDFSharp
{
    public static class PDFSharpUtils
    {
        private static PdfDocument RegeneratePDFImpl(PdfDocument inputDocument)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            IEnumerable<PdfPage> inputPages = inputDocument.Pages;
            PdfDocument outputDocument = new();
            foreach (PdfPage page in inputPages)
            {
                outputDocument.AddPage(page);
            }
            return outputDocument;
        }

        public static void RegeneratePDF(Stream pdfStream, Stream outputDocumentStream)
        {
            PdfDocument inputDocument = PdfReader.Open(pdfStream, PdfDocumentOpenMode.Import);
            PdfDocument outputDocument = RegeneratePDFImpl(inputDocument);
            outputDocument.Save(outputDocumentStream);
        }

        public static byte[] RegeneratePDF(Stream pdfStream)
        {
            using Stream outputDocumentStream = new MemoryStream();
            RegeneratePDF(pdfStream, outputDocumentStream);
            return outputDocumentStream.ToByteArray();
        }

    }
}
