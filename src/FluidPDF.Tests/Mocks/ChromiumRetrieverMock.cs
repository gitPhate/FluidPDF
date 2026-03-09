using FluidPDF.Support.PuppeteerSharp;
using FluidPDF.Tests.Mothers;
using NSubstitute;
using PuppeteerSharp;

namespace FluidPDF.Tests.Mocks
{
    internal static class ChromiumRetrieverMock
    {
        /// <summary>
        /// Builds a fully wired NSubstitute chain:
        ///   IChromiumRetriever → IBrowser → IPage
        /// The page returns a valid single-page PDF from PdfDocumentFixture.
        /// </summary>
        internal static IChromiumRetriever CreateWithSinglePagePdf(
            out IBrowser browser,
            out IPage page)
        {
            IChromiumRetriever retriever = Substitute.For<IChromiumRetriever>();
            browser = Substitute.For<IBrowser>();
            page = Substitute.For<IPage>();

            retriever
                .RetrieveBrowserInstanceAsync()
                .Returns(Task.FromResult(browser));

            browser
                .NewPageAsync()
                .Returns(Task.FromResult(page));

            page
                .SetContentAsync(Arg.Any<string>())
                .Returns(Task.CompletedTask);

            page
                .PdfDataAsync(Arg.Any<PdfOptions>())
                .Returns(callInfo => PDFDocumentMother.CreateSinglePagePdfAsync());

            page
                .CloseAsync()
                .Returns(Task.CompletedTask);

            browser
                .CloseAsync()
                .Returns(Task.CompletedTask);

            return retriever;
        }

        /// <summary>
        /// Builds a wired chain where IPage.SetContentAsync throws, simulating a
        /// mid-render failure. Used to verify the try/finally cleanup path.
        /// </summary>
        internal static IChromiumRetriever CreateWithPageThatThrowsOnSetContent(
            out IBrowser browser,
            out IPage page)
        {
            IChromiumRetriever retriever = Substitute.For<IChromiumRetriever>();
            browser = Substitute.For<IBrowser>();
            page = Substitute.For<IPage>();

            retriever
                .RetrieveBrowserInstanceAsync()
                .Returns(Task.FromResult(browser));

            browser
                .NewPageAsync()
                .Returns(Task.FromResult(page));

            page
                .SetContentAsync(Arg.Any<string>())
                .Returns(_ => Task.FromException(new Exception("Simulated render failure")));

            page
                .CloseAsync()
                .Returns(Task.CompletedTask);

            browser
                .CloseAsync()
                .Returns(Task.CompletedTask);

            return retriever;
        }
    }
}
