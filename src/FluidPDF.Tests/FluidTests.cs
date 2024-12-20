using Fluid;
using Fluid.Values;
using FluidPDF.Fluid;
using System.Data;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FluidPDF.Tests
{
    public class FluidTests
    {
        [Fact]
        public async Task TestFluid()
        {
            var templateOptions = new TemplateOptions
            {
                Trimming = TrimmingFlags.TagRight
            };

            templateOptions.ValueConverters.Add(x => x is DBNull o ? NilValue.Instance : null);

            templateOptions
                .MemberAccessStrategy
                .Register<DataRow, object>
                (
                    (row, fieldName) =>
                    {
                        if (row.IsNull(fieldName))
                        {
                            return DBNull.Value;
                        }

                        return row[fieldName];
                    }
                );

            templateOptions.MemberAccessStrategy.Register<DataTable>();


            templateOptions.MemberAccessStrategy.Register<JsonNode, object>((src, name) => src[name]!);

            templateOptions.ValueConverters.Add(x => x is JsonNode o ? new ObjectValue(o) : null);

            //args
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            TimeZoneInfo timeZone = TimeZoneInfo.Local;

            Model testModel = new() { Value = "Hello World" };

            string jsonModel = JsonSerializer.Serialize(testModel);
            FluidModel[] models = [FluidModel.FromJsonString("Model", jsonModel)];

            var context = new TemplateContext();

            context.CultureInfo = cultureInfo ?? CultureInfo.InvariantCulture;

            if (timeZone is not null)
            {
                context.TimeZone = timeZone;
            }

            foreach (var model in models ?? [])
            {
                context.SetValue(model.Name, model.Value);
            }

            //args
            bool encodeHtml = false;

            //TODO: must be shared to all application
            var parser = new FluidParser();
            if (parser.TryParse(TestConsts.TestTemplate, out var template, out var error))
            {
                //TODO: create the TemplateContext here
                using var writer = new StringWriter();

                TextEncoder encoder = encodeHtml ? HtmlEncoder.Default : NullEncoder.Default;

                await
                    template
                    .RenderAsync(writer, encoder, context);

                string renderedValue = writer.ToString();
            }
            else
            {
                //TODO: improve error
                throw new Exception(error);
            }
        }
    }
}