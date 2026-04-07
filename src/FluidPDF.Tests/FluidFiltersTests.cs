using FluentAssertions;
using FluidPDF.Fluid;
using FluidPDF.Templating;
using System.Globalization;
using System.Text;

namespace FluidPDF.Tests
{
    public class FluidFiltersTests
    {
        private static async Task<string> RenderAsync(string template, object? model = null, CultureInfo? culture = null)
        {
            using FluidTemplateEngine engine = new();
            FluidPDFTemplateRenderOptions options = new()
            {
                CultureInfo = culture,
                EncodeHtml = false,
            };
            return await engine.RenderTemplateAsync(template, model ?? new { }, options);
        }

        // Render a template using string variables assigned inside the template itself so
        // the value flows through Fluid as StringValue, not as a JSON-serialised ObjectValue.
        private static Task<string> RenderWithLiteralAsync(string template, CultureInfo? culture = null)
            => RenderAsync(template, new { }, culture);

        // ── to_number ───────────────────────────────────────────────────────────

        [Fact]
        public async Task ToNumber_ShouldReturnNumericValue_WhenStringIsInteger()
        {
            // Use assign so the value is a StringValue inside Fluid.
            string result = await RenderWithLiteralAsync("{% assign v = '42' %}{{ v | to_number }}");
            result.Should().Be("42");
        }

        [Fact]
        public async Task ToNumber_ShouldReturnNumericValue_WhenValueIsAlreadyANumber()
        {
            string result = await RenderWithLiteralAsync("{% assign v = 3.14 %}{{ v | to_number }}");
            result.Should().Be("3.14");
        }

        // ── to_date_time ─────────────────────────────────────────────────────────

        [Fact]
        public async Task ToDateTime_ShouldReturnFormattedDate_WhenGivenIsoDateString()
        {
            string result = await RenderWithLiteralAsync(
                "{% assign v = '2024-06-15T00:00:00+00:00' %}{{ v | to_date_time: 'yyyy-MM-dd' }}");

            result.Should().Be("2024-06-15");
        }

        [Fact]
        public async Task ToDateTime_ShouldReturnNil_WhenValueCannotBeParsed()
        {
            string result = await RenderWithLiteralAsync(
                "{% assign v = 'not-a-date' %}{{ v | to_date_time: 'yyyy-MM-dd' }}");
            result.Should().BeEmpty();
        }

        // ── to_string ────────────────────────────────────────────────────────────

        [Fact]
        public async Task ToString_ShouldFormatDecimal_WhenFormatIsSpecified()
        {
            string result = await RenderWithLiteralAsync(
                "{% assign v = 3.14159 %}{{ v | to_string: format: 'F2' }}",
                CultureInfo.InvariantCulture);

            result.Should().Be("3.14");
        }

        [Fact]
        public async Task ToString_ShouldPadWithWidth_WhenLenIsSpecified()
        {
            // Use a string literal so Fluid sees it as a StringValue.
            string result = await RenderWithLiteralAsync(
                "{% assign v = 'Hi' %}{{ v | to_string: len: 10 }}",
                CultureInfo.InvariantCulture);

            // string.Format("{0,10}", "Hi") right-aligns in 10 chars
            result.Should().Be("        Hi");
        }

        [Fact]
        public async Task ToString_ShouldReturnInputUnchanged_WhenInputIsNil()
        {
            string result = await RenderWithLiteralAsync("{{ nil_var | to_string }}");
            result.Should().BeEmpty();
        }

        // ── extract_file_name ────────────────────────────────────────────────────

        [Fact]
        public async Task ExtractFileName_ShouldReturnFileName_WhenPathIsProvided()
        {
            string result = await RenderWithLiteralAsync(
                "{% assign v = 'C:\\\\reports\\\\invoice.pdf' %}{{ v | extract_file_name }}");

            result.Should().Be("invoice.pdf");
        }

        [Fact]
        public async Task ExtractFileName_ShouldReturnInputUnchanged_WhenInputIsNotAString()
        {
            // When input is not a string the filter returns the input unchanged.
            // A non-string (e.g. number) passed directly is rendered as its numeric value.
            string result = await RenderWithLiteralAsync("{% assign v = 42 %}{{ v | extract_file_name }}");
            result.Should().Be("42");
        }

        // ── extract_directory_name ───────────────────────────────────────────────

        [Fact]
        public async Task ExtractDirectoryName_ShouldReturnDirectory_WhenPathIsProvided()
        {
            string result = await RenderWithLiteralAsync(
                "{% assign v = 'C:\\\\reports\\\\invoice.pdf' %}{{ v | extract_directory_name }}");

            result.Should().Be(@"C:\reports");
        }

        // ── starts_with ──────────────────────────────────────────────────────────

        [Fact]
        public async Task StartsWith_ShouldReturnTrue_WhenStringStartsWithPrefix()
        {
            string result = await RenderWithLiteralAsync(
                "{% assign v = 'Hello World' %}{{ v | starts_with: 'Hello' }}");
            result.Should().Be("true");
        }

        [Fact]
        public async Task StartsWith_ShouldReturnFalse_WhenStringDoesNotStartWithPrefix()
        {
            string result = await RenderWithLiteralAsync(
                "{% assign v = 'Hello World' %}{{ v | starts_with: 'World' }}");
            result.Should().Be("false");
        }

        [Fact]
        public async Task StartsWith_ShouldReturnFalse_WhenInputIsNotAString()
        {
            string result = await RenderWithLiteralAsync("{% assign v = 123 %}{{ v | starts_with: '1' }}");
            result.Should().Be("false");
        }

        // ── ends_with ────────────────────────────────────────────────────────────

        [Fact]
        public async Task EndsWith_ShouldReturnTrue_WhenStringEndsWithSuffix()
        {
            string result = await RenderWithLiteralAsync(
                "{% assign v = 'Hello World' %}{{ v | ends_with: 'World' }}");
            result.Should().Be("true");
        }

        [Fact]
        public async Task EndsWith_ShouldReturnFalse_WhenStringDoesNotEndWithSuffix()
        {
            string result = await RenderWithLiteralAsync(
                "{% assign v = 'Hello World' %}{{ v | ends_with: 'Hello' }}");
            result.Should().Be("false");
        }

        // ── contains ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Contains_ShouldReturnTrue_WhenStringContainsSubstring()
        {
            string result = await RenderWithLiteralAsync(
                "{% assign v = 'Hello World' %}{{ v | contains: 'lo W' }}");
            result.Should().Be("true");
        }

        [Fact]
        public async Task Contains_ShouldReturnFalse_WhenStringDoesNotContainSubstring()
        {
            string result = await RenderWithLiteralAsync(
                "{% assign v = 'Hello World' %}{{ v | contains: 'xyz' }}");
            result.Should().Be("false");
        }

        // ── to_base64 / from_base64 ───────────────────────────────────────────────

        [Fact]
        public async Task ToBase64_ShouldEncodeString_WhenUtf8EncodingIsUsed()
        {
            string result = await RenderWithLiteralAsync("{% assign v = 'Hello' %}{{ v | to_base64 }}");
            string expected = Convert.ToBase64String(Encoding.UTF8.GetBytes("Hello"));
            result.Should().Be(expected);
        }

        [Fact]
        public async Task FromBase64_ShouldDecodeString_WhenUtf8EncodingIsUsed()
        {
            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes("Hello"));
            string result = await RenderWithLiteralAsync($"{{% assign v = '{encoded}' %}}{{{{ v | from_base64 }}}}");
            result.Should().Be("Hello");
        }

        [Fact]
        public async Task ToBase64_ThenFromBase64_ShouldRoundTrip()
        {
            const string text = "FluidPDF";
            string result = await RenderWithLiteralAsync($"{{% assign v = '{text}' %}}{{{{ v | to_base64 | from_base64 }}}}");
            result.Should().Be(text);
        }

        [Fact]
        public async Task ToBase64_ShouldReturnEmpty_WhenInputIsNil()
        {
            string result = await RenderWithLiteralAsync("{{ nil_var | to_base64 }}");
            result.Should().BeEmpty();
        }

        // ── file_read_all_text ────────────────────────────────────────────────────

        [Fact]
        public async Task FileReadAllText_ShouldReturnFileContent_WhenFileExists()
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "test content");
                string escapedPath = tempFile.Replace("\\", "\\\\");
                string result = await RenderWithLiteralAsync(
                    $"{{% assign v = '{escapedPath}' %}}{{{{ v | file_read_all_text }}}}");
                result.Should().Be("test content");
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task FileReadAllText_ShouldReturnEmpty_WhenFileDoesNotExist()
        {
            string result = await RenderWithLiteralAsync(
                @"{% assign v = 'C:\\nonexistent\\file.txt' %}{{ v | file_read_all_text }}");
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task FileReadAllText_ShouldReturnEmpty_WhenInputIsNotAString()
        {
            string result = await RenderWithLiteralAsync("{% assign v = 42 %}{{ v | file_read_all_text }}");
            result.Should().BeEmpty();
        }

        // ── file_read_all_lines ───────────────────────────────────────────────────

        [Fact]
        public async Task FileReadAllLines_ShouldReturnLines_WhenReadingFromStringSource()
        {
            string result = await RenderWithLiteralAsync(
                "{% assign src = 'line1\nline2\nline3' %}{% assign lines = src | file_read_all_lines %}{% for l in lines %}{{ l.LineContent }},{% endfor %}");

            result.Should().Be("line1,line2,line3,");
        }

        [Fact]
        public async Task FileReadAllLines_ShouldReturnLinesFromFile_WhenFileArgumentIsProvided()
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "alpha\nbeta\ngamma");
                string escapedPath = tempFile.Replace("\\", "\\\\");
                string result = await RenderWithLiteralAsync(
                    $"{{% assign v = '{escapedPath}' %}}{{% assign lines = v | file_read_all_lines: 'file' %}}{{% for l in lines %}}{{{{ l.LineContent }}}},{{% endfor %}}");
                result.Should().Be("alpha,beta,gamma,");
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task FileReadAllLines_ShouldReturnEmpty_WhenInputIsNotAString()
        {
            string result = await RenderWithLiteralAsync(
                "{% assign lines = 42 | file_read_all_lines %}{{ lines.size }}");

            result.Should().Be("0");
        }
    }
}
