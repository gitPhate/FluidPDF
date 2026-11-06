using System.IO;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Writer;

namespace FluidPDF.Support.PDF
{
    internal static class PDFCompressHelper
    {
        /// <summary>
        /// Regenerates the PDF by copying every page into a new document written with
        /// <see cref="PdfWriterType.ObjectInMemoryDedup"/>, so identical objects repeated across
        /// pages (fonts, images) are stored once. Annotations (e.g. links) are preserved.
        /// </summary>
        public static byte[] CompressPDF(byte[] data)
        {
            using PdfDocument inputDocument = PdfDocument.Open(data);
            using MemoryStream outputStream = new();
            PdfDocumentBuilder.AddPageOptions pageOptions = new() { KeepAnnotations = true };

            using (PdfDocumentBuilder outputDocument = new(outputStream, false, PdfWriterType.ObjectInMemoryDedup))
            {
                for (int pageNumber = 1; pageNumber <= inputDocument.NumberOfPages; pageNumber++)
                {
                    outputDocument.AddPage(inputDocument, pageNumber, pageOptions);
                }

                outputDocument.Build();
            }

            return outputStream.ToArray();
        }
    }
}
