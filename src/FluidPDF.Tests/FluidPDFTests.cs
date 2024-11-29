using FluidPDF.Builder;
using FluidPDF.Fluid;
using FluidPDF.Prototype;
using System.Data;

namespace FluidPDF.Tests
{
    public class FluidPDFTests
    {
        public static IEnumerable<object[]> ObjectModel = [[new Model { Value = "Hello World" }]];
        public static IEnumerable<object[]> DictionaryModel = [[new Dictionary<string, object>() { { "Value", "Hello World" } }]];
        public static IEnumerable<object[]> JsonStringModel = [["{ \"Value\": \"Hello World\" }"]];

        public static IEnumerable<object[]> GetDataRowModel()
        {
            DataTable dt = new();
            dt.Columns.Add("Value");
            dt.Rows.Add("Hello World");

            yield return [dt.Rows[0]];
        }

        [Theory]
        [MemberData(nameof(ObjectModel))]
        [MemberData(nameof(DictionaryModel))]
        [MemberData(nameof(JsonStringModel))]
        [MemberData(nameof(GetDataRowModel))]
        public async Task TestObjectModel<T>(T model)
            where T : notnull
        {
            using IPdfPrototype report =
                await FluidPDFBuilder
                .NewWithModel(model)
                .WithTemplate(TestConsts.TestTemplate)
                .WithStandaloneChromium()
                .BuildAsync();

            using MemoryStream stream = new();
            byte[] bytes = await report.ToByteArrayAsync();
            File.WriteAllBytes(@$"C:\temp\lol-{model.GetType().Name}.pdf", bytes);
        }

        [Fact]
        public async Task TestMultipleModels()
        {
            const string multipleModelTemplate = @"
<!doctype html>
<html lang=""en"">
<body>
    Model value: {{Model.Value}}<br/>
    Model2 value: {{Model2.Value}}<br/>
</body>
</html>
";

            FluidModel model1 = FluidModel.FromObject("Model", new Model { Value = "Hello World" });
            FluidModel model2 = FluidModel.FromObject("Model2", new Model { Value = "Hello World2" });
            FluidModel[] models = [model1, model2];

            using IPdfPrototype report =
                await FluidPDFBuilder
                .NewWithModel(models)
                .WithTemplate(multipleModelTemplate)
                .WithStandaloneChromium()
                .BuildAsync();

            using MemoryStream stream = new();
            byte[] bytes = await report.ToByteArrayAsync();
            File.WriteAllBytes(@$"C:\temp\lol-multi-model.pdf", bytes);
        }
    }
}