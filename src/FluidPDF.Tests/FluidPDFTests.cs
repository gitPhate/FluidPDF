using FluidPDF.Builder;
using FluidPDF.Prototype;

namespace FluidPDF.Tests
{
    public class FluidPDFTests
    {
        [Fact]
        public async Task TestHelloWorld()
        {
            Model model = new() { Value = "Hello World" };

            using IPdfPrototype report =
                await FluidPDFBuilder
                .NewWithModel(model)
                .WithTemplate(TestConsts.TestTemplate)
                .WithStandaloneChromium()
                .BuildAsync();

            using MemoryStream stream = new();
            await report.ToStreamAsync(stream);
            byte[] bytes = stream.ToArray();
            File.WriteAllBytes(@"C:\temp\apeevo\lol.pdf", bytes);
        }
    }
}