using FluentAssertions;
using FluidPDF.Fluid;
using FluidPDF.Templating;

namespace FluidPDF.Tests
{
    public class FluidTagsTests
    {
        private static async Task<string> RenderAsync(string template, object? model = null)
        {
            using FluidTemplateEngine engine = new();
            FluidPDFTemplateRenderOptions options = new() { EncodeHtml = false };
            return await engine.RenderTemplateAsync(template, model ?? new { }, options).ConfigureAwait(false);
        }

        // ── backslash ────────────────────────────────────────────────────────────

        [Fact]
        public async Task BackslashTag_ShouldOutputBackslash()
        {
            string result = await RenderAsync("{% backslash %}");
            result.Should().Be("\\");
        }

        // ── slash ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SlashTag_ShouldOutputForwardSlash()
        {
            string result = await RenderAsync("{% slash %}");
            result.Should().Be("/");
        }

        // ── pipe ─────────────────────────────────────────────────────────────────

        [Fact]
        public async Task PipeTag_ShouldOutputPipeCharacter()
        {
            string result = await RenderAsync("{% pipe %}");
            result.Should().Be("|");
        }

        // ── double_quote ─────────────────────────────────────────────────────────

        [Fact]
        public async Task DoubleQuoteTag_ShouldOutputDoubleQuote()
        {
            string result = await RenderAsync("{% double_quote %}");
            result.Should().Be("\"");
        }

        // ── single_quote ─────────────────────────────────────────────────────────

        [Fact]
        public async Task SingleQuoteTag_ShouldOutputSingleQuote()
        {
            string result = await RenderAsync("{% single_quote %}");
            result.Should().Be("'");
        }

        // ── path_separator ───────────────────────────────────────────────────────

        [Fact]
        public async Task PathSeparatorTag_ShouldOutputOsDirectorySeparator()
        {
            string result = await RenderAsync("{% path_separator %}");
            result.Should().Be(Path.DirectorySeparatorChar.ToString());
        }

        // ── string_empty ─────────────────────────────────────────────────────────

        [Fact]
        public async Task StringEmptyTag_ShouldOutputEmptyString()
        {
            string result = await RenderAsync("A{% string_empty %}B");
            result.Should().Be("AB");
        }

        // ── float_random ─────────────────────────────────────────────────────────

        [Fact]
        public async Task FloatRandomTag_ShouldOutputValueBetweenZeroAndOne()
        {
            string result = await RenderAsync("{% float_random %}");
            double.TryParse(result, out double value).Should().BeTrue();
            value.Should().BeGreaterThanOrEqualTo(0.0).And.BeLessThan(1.0);
        }

        [Fact]
        public async Task FloatRandomTag_ShouldOutputDifferentValuesAcrossMultipleRenders()
        {
            // Very unlikely to produce the same float twice; if this ever flakes use a counter approach.
            List<string> results = [];
            for (int i = 0; i < 10; i++)
            {
                results.Add(await RenderAsync("{% float_random %}"));
            }

            results.Distinct().Should().HaveCountGreaterThan(1);
        }

        // ── guid ─────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GuidTag_New_ShouldOutputValidGuid()
        {
            string result = await RenderAsync("{% guid new %}");
            Guid.TryParse(result, out _).Should().BeTrue();
        }

        [Fact]
        public async Task GuidTag_New_ShouldOutputDifferentGuidsOnEachRender()
        {
            string first = await RenderAsync("{% guid new %}");
            string second = await RenderAsync("{% guid new %}");
            first.Should().NotBe(second);
        }

        [Fact]
        public async Task GuidTag_Empty_ShouldOutputAllZerosGuid()
        {
            string result = await RenderAsync("{% guid empty %}");
            result.Should().Be(Guid.Empty.ToString());
        }

        [Fact]
        public async Task GuidTag_UnknownIdentifier_ShouldOutputNothing()
        {
            string result = await RenderAsync("{% guid unknown %}");
            result.Should().BeEmpty();
        }

        // ── int_random ───────────────────────────────────────────────────────────

        [Fact]
        public async Task IntRandomTag_ShouldOutputAnInteger_WhenMinAndMaxAreProvided()
        {
            string result = await RenderAsync("{% int_random min_value: 0, max_value: 100 %}");
            int.TryParse(result, out _).Should().BeTrue("result '{0}' should be parseable as an integer", result);
        }

        [Fact]
        public async Task IntRandomTag_ShouldOutputValueWithinRange_WhenMinAndMaxAreProvided()
        {
            // Run several times to reduce the chance of accidental pass.
            for (int i = 0; i < 20; i++)
            {
                string result = await RenderAsync("{% int_random min_value: 10, max_value: 20 %}");
                int value = int.Parse(result);
                value.Should().BeGreaterThanOrEqualTo(10).And.BeLessThan(20);
            }
        }

        [Fact]
        public async Task IntRandomTag_ShouldOutputMinValue_WhenMinAndMaxAreEqual()
        {
            string result = await RenderAsync("{% int_random min_value: 5, max_value: 5 %}");
            result.Should().Be("5");
        }
    }
}
