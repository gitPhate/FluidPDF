using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace FluidPDF.Tests.Mothers
{
    /// <summary>
    /// Builds minimal but structurally valid PDF documents in memory using PdfPig,
    /// so that mock pages can return realistic bytes instead of placeholder magic bytes.
    /// </summary>
    internal static class PDFDocumentMother
    {
        /// <summary>
        /// Creates a valid single-page PDF in memory and returns the raw bytes.
        /// The document contains one blank page and no embedded content.
        /// </summary>
        internal static byte[] CreateSinglePagePDF() =>
            CreateMultiPagePDF(1);

        /// <summary>
        /// Creates a valid multi-page PDF in memory and returns the raw bytes.
        /// </summary>
        internal static byte[] CreateMultiPagePDF(int pageCount)
        {
            using PdfDocumentBuilder builder = new();
            for (int i = 0; i < pageCount; i++)
            {
                builder.AddPage(PageSize.A4);
            }

            return builder.Build();
        }

        /// <summary>
        /// Creates a multi-page PDF where every page writes <paramref name="text"/> with its own
        /// copy of the same Standard 14 font, so the document contains duplicated objects.
        /// </summary>
        internal static byte[] CreateMultiPagePDFWithDuplicatedFonts(int pageCount, string text)
        {
            using PdfDocumentBuilder builder = new();
            for (int i = 0; i < pageCount; i++)
            {
                PdfDocumentBuilder.AddedFont font = builder.AddStandard14Font(Standard14Font.Helvetica);
                PdfPageBuilder page = builder.AddPage(PageSize.A4);
                page.AddText(text, 12, new PdfPoint(25, 700), font);
            }

            return builder.Build();
        }

        /// <summary>
        /// Returns true when <paramref name="bytes"/> can be opened as a PDF by PdfPig.
        /// </summary>
        internal static bool IsValidPDF(byte[] bytes)
        {
            try
            {
                using PdfDocument document = PdfDocument.Open(bytes);
                return document.NumberOfPages > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Opens <paramref name="bytes"/> as a PDF document and returns the page count.
        /// Throws if the bytes are not a valid PDF.
        /// </summary>
        internal static int GetPageCount(byte[] bytes)
        {
            using PdfDocument document = PdfDocument.Open(bytes);
            return document.NumberOfPages;
        }

        /// <summary>
        /// Opens <paramref name="bytes"/> as a PDF document and returns the text of every page.
        /// </summary>
        internal static string[] GetPagesText(byte[] bytes)
        {
            using PdfDocument document = PdfDocument.Open(bytes);
            return document.GetPages().Select(page => page.Text).ToArray();
        }

        /// <summary>
        /// Async variant of <see cref="CreateSinglePagePDF"/> for use in async test methods.
        /// </summary>
        internal static Task<byte[]> CreateSinglePagePdfAsync() =>
            Task.FromResult(CreateSinglePagePDF());
    }
}
