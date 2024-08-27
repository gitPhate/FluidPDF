using FluidPDF.Builder;
using FluidPDF.PDF;
using Sisifo.Json;

namespace FluidPDF.Tests
{
    public class MainTests
    {
        [Fact]
        public async Task TestHelloWorld()
        {
            string template = @"
<!doctype html>
<html lang=""en"">
<body>
    Model value: {{Model.Value}}
</body>
</html>
";
            Model model = new() { Value = "Hello World" };

            using IPdfPrototype report =
                await FluidPDFBuilder
                .NewWithJsonModel(model)
                .WithTemplate(template)
                .WithCustomMargin(FluidPDFMargins.ZeroPoint3)
                .WithStandaloneChromium()
                //.WithCompression()
                .BuildAsync();

            using MemoryStream stream = new();
            await report.ToStreamAsync(stream);
            byte[] bytes = stream.ToArray();
            File.WriteAllBytes(@"C:\temp\apeevo\lol.pdf", bytes);
        }
    }

#nullable disable

    public class Model
    {
        public string Value { get; set; }
    }
}