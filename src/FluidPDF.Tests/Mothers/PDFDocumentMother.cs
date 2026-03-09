using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace FluidPDF.Tests.Mothers
{
    /// <summary>
    /// Builds minimal but structurally valid PDF documents in memory using PDFsharp,
    /// so that mock pages can return realistic bytes instead of placeholder magic bytes.
    /// </summary>
    internal static class PDFDocumentMother
    {
        /// <summary>
        /// Creates a valid single-page PDF in memory and returns the raw bytes.
        /// The document contains one blank page and no embedded content.
        /// </summary>
        internal static byte[] CreateSinglePagePDF()
        {
            using PdfDocument document = new();
            document.AddPage();

            using MemoryStream stream = new();
            document.Save(stream);
            return stream.ToArray();
        }

        /// <summary>
        /// Creates a valid multi-page PDF in memory and returns the raw bytes.
        /// </summary>
        internal static byte[] CreateMultiPagePDF(int pageCount)
        {
            using PdfDocument document = new();
            for (int i = 0; i < pageCount; i++)
            {
                document.AddPage();
            }

            using MemoryStream stream = new();
            document.Save(stream);
            return stream.ToArray();
        }

        /// <summary>
        /// Returns true when <paramref name="bytes"/> can be parsed as a valid PDF by PDFsharp.
        /// Uses PdfReader.TestPdfFile which checks the header and basic structure without
        /// loading the full document into memory.
        /// </summary>
        internal static bool IsValidPDF(byte[] bytes) =>
            PdfReader.TestPdfFile(bytes) > 0;

        /// <summary>
        /// Opens <paramref name="bytes"/> as a PDF document and returns the page count.
        /// Throws if the bytes are not a valid PDF.
        /// </summary>
        internal static int GetPageCount(byte[] bytes)
        {
            using MemoryStream stream = new(bytes);
            using PdfDocument document = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
            return document.PageCount;
        }

        /// <summary>
        /// Async variant of <see cref="CreateSinglePagePDF"/> for use in async test methods.
        /// </summary>
        internal static Task<byte[]> CreateSinglePagePdfAsync() =>
            Task.FromResult(CreateSinglePagePDF());
    }
}
