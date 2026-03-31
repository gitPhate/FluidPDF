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
                .LaunchBrowserAsync()
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
        /// Builds a fully wired NSubstitute chain where <paramref name="capturedOptionsBox"/>
        /// is a single-element array whose element is populated with the <see cref="PdfOptions"/>
        /// passed to <see cref="IPage.PdfDataAsync"/> once <c>BuildAsync</c> completes.
        /// Read <c>capturedOptionsBox[0]</c> after awaiting <c>BuildAsync</c>.
        /// </summary>
        internal static IChromiumRetriever CreateWithSinglePagePdfAndOptionCapture(
            out IBrowser browser,
            out IPage page,
            out PdfOptions?[] capturedOptionsBox)
        {
            PdfOptions?[] box = [null];

            IChromiumRetriever retriever = Substitute.For<IChromiumRetriever>();
            browser = Substitute.For<IBrowser>();
            page = Substitute.For<IPage>();

            retriever
                .LaunchBrowserAsync()
                .Returns(Task.FromResult(browser));

            browser
                .NewPageAsync()
                .Returns(Task.FromResult(page));

            page
                .SetContentAsync(Arg.Any<string>())
                .Returns(Task.CompletedTask);

            page
                .PdfDataAsync(Arg.Any<PdfOptions>())
                .Returns(callInfo =>
                {
                    box[0] = callInfo.Arg<PdfOptions>();
                    return PDFDocumentMother.CreateSinglePagePdfAsync();
                });

            page
                .CloseAsync()
                .Returns(Task.CompletedTask);

            browser
                .CloseAsync()
                .Returns(Task.CompletedTask);

            capturedOptionsBox = box;
            return retriever;
        }

        /// <summary>
        /// Builds a fully wired NSubstitute chain where <paramref name="capturedContentBox"/>
        /// is a single-element array whose element is populated with the HTML string
        /// passed to <see cref="IPage.SetContentAsync"/> once <c>BuildAsync</c> completes.
        /// Read <c>capturedContentBox[0]</c> after awaiting <c>BuildAsync</c>.
        /// </summary>
        internal static IChromiumRetriever CreateWithSinglePagePdfAndContentCapture(
            out IBrowser browser,
            out IPage page,
            out string?[] capturedContentBox)
        {
            string?[] box = [null];

            IChromiumRetriever retriever = Substitute.For<IChromiumRetriever>();
            browser = Substitute.For<IBrowser>();
            page = Substitute.For<IPage>();

            retriever
                .LaunchBrowserAsync()
                .Returns(Task.FromResult(browser));

            browser
                .NewPageAsync()
                .Returns(Task.FromResult(page));

            page
                .SetContentAsync(Arg.Any<string>())
                .Returns(callInfo =>
                {
                    box[0] = callInfo.Arg<string>();
                    return Task.CompletedTask;
                });

            page
                .PdfDataAsync(Arg.Any<PdfOptions>())
                .Returns(callInfo => PDFDocumentMother.CreateSinglePagePdfAsync());

            page
                .CloseAsync()
                .Returns(Task.CompletedTask);

            browser
                .CloseAsync()
                .Returns(Task.CompletedTask);

            capturedContentBox = box;
            return retriever;
        }

        /// <summary>
        internal static IChromiumRetriever CreateWithPageThatThrowsOnSetContent(
            out IBrowser browser,
            out IPage page)
        {
            IChromiumRetriever retriever = Substitute.For<IChromiumRetriever>();
            browser = Substitute.For<IBrowser>();
            page = Substitute.For<IPage>();

            retriever
                .LaunchBrowserAsync()
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
