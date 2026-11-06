using FluentAssertions;
using FluidPDF.Exceptions;
using FluidPDF.Fluid;
using FluidPDF.Templating;
using FluidPDF.Tests.Mothers;

namespace FluidPDF.Tests
{
    public class FluidTemplateEngineTests : TemplateEngineTests
    {
        protected override IFluidPDFTemplateEngine CreateEngine() => new FluidTemplateEngine();
        protected override string DataTableTemplate => TemplateModelMother.SimpleDataTableTemplate();
        protected override string LocalizationTemplate => TemplateModelMother.LocalizationTemplate;

        // --- Engine-specific: invalid template throws domain exception ---

        [Fact]
        public async Task RenderTemplateAsync_ShouldThrowFluidPDFTemplateRenderException_WhenTemplateIsInvalid()
        {
            // Arrange
            object model = TemplateModelMother.SimpleObject();
            string template = TemplateModelMother.InvalidTemplate;
            using FluidTemplateEngine templateEngine = new();

            // Act
            Func<Task> act = async () =>
                await templateEngine.RenderTemplateAsync(template, model, new());

            // Assert
            await act.Should().ThrowAsync<FluidTemplateRenderException>();
        }

        // --- Engine-specific: JSON scalars behave as real Liquid values ---

        [Fact]
        public async Task RenderTemplateAsync_ShouldDoArithmeticOnJsonNumbers_WhenJsonArrayIsProvided()
        {
            // Arrange
            const string json = """[{"Price": 2.5, "Qty": 4}, {"Price": 1, "Qty": 3}]""";
            const string template = "{% for r in Model %}{{ r.Price | times: r.Qty | round }}|{% endfor %}";
            using FluidTemplateEngine templateEngine = new();

            // Act
            string result = await templateEngine.RenderTemplateAsync(template, json, new());

            // Assert
            result.Should().Be("10|3|");
        }

        [Fact]
        public async Task RenderTemplateAsync_ShouldGroupJsonValues_WhenUsingUniqAndWhere()
        {
            // Arrange
            const string json = """[{"G": 1, "N": "a"}, {"G": 2, "N": "b"}, {"G": 1, "N": "c"}]""";
            const string template =
                "{% assign keys = Model | map: 'G' | uniq %}" +
                "{% for k in keys %}{{ k }}:{% assign rows = Model | where: 'G', k %}{{ rows | size }};{% endfor %}";
            using FluidTemplateEngine templateEngine = new();

            // Act
            string result = await templateEngine.RenderTemplateAsync(template, json, new());

            // Assert
            result.Should().Be("1:2;2:1;");
        }
    }
}
