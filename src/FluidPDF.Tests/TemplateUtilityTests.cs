using FluentAssertions;
using FluidPDF.Templating;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace FluidPDF.Tests
{
    /// <summary>
    /// Abstract base for testing the shared filter/tag utilities (to_number, to_date_time,
    /// extract_file_name, etc.) across all template engines.
    /// Concrete subclasses supply an engine and produce engine-specific template snippets.
    /// </summary>
    public abstract class TemplateUtilityTests
    {
        protected abstract IFluidPDFTemplateEngine CreateEngine();

        /// <summary>
        /// Produces a template that assigns <paramref name="assignValue"/> to a variable,
        /// then pipes it through <paramref name="filterName"/> with optional
        /// <paramref name="filterArgs"/> and outputs the result.
        /// </summary>
        protected abstract string AssignAndFilter(string assignValue, string filterName, params string[] filterArgs);

        /// <summary>
        /// Produces a template that assigns <paramref name="assignValue"/> to a variable,
        /// then pipes it sequentially through each filter in <paramref name="filters"/>
        /// (no extra arguments), and outputs the final result.
        /// </summary>
        protected abstract string AssignThenChainFilters(string assignValue, params string[] filters);

        /// <summary>
        /// Produces a template that calls a no-argument or argument-carrying function
        /// (equivalent to a Fluid tag) and outputs its return value.
        /// </summary>
        protected abstract string CallFunction(string functionCall);
        protected abstract string Literal(string content);
        /// <summary>
        /// Produces a template that calls <c>file_read_all_lines</c> on <paramref name="source"/>
        /// (with optional <paramref name="fileArg"/>), iterates the result, and emits each
        /// <paramref name="lineContentAccess"/> followed by a comma.
        /// </summary>
        protected abstract string ForEachLine(string source, string? fileArg, string lineContentAccess);

        private async Task<string> RenderAsync(string template, CultureInfo? culture = null)
        {
            using IFluidPDFTemplateEngine engine = CreateEngine();
            FluidPDFTemplateRenderOptions options = new()
            {
                CultureInfo = culture,
                EncodeHtml = false,
            };
            return await engine.RenderTemplateAsync(template, new { }, options);
        }

        // ── to_number ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task ToNumber_ShouldReturnNumericValue_WhenStringIsInteger()
        {
            string template = AssignAndFilter("'42'", "to_number");
            string result = await RenderAsync(template);
            result.Should().Be("42");
        }

        [Fact]
        public async Task ToNumber_ShouldReturnNumericValue_WhenValueIsAlreadyANumber()
        {
            string template = AssignAndFilter("3.14", "to_number");
            string result = await RenderAsync(template);
            result.Should().Be("3.14");
        }

        // ── to_date_time ──────────────────────────────────────────────────────────

        [Fact]
        public async Task ToDateTime_ShouldReturnFormattedDate_WhenGivenIsoDateString()
        {
            string template = AssignAndFilter("'2024-06-15T00:00:00+00:00'", "to_date_time", "'yyyy-MM-dd'");
            string result = await RenderAsync(template);
            result.Should().Be("2024-06-15");
        }

        [Fact]
        public async Task ToDateTime_ShouldReturnNil_WhenValueCannotBeParsed()
        {
            string template = AssignAndFilter("'not-a-date'", "to_date_time", "'yyyy-MM-dd'");
            string result = await RenderAsync(template);
            result.Should().BeEmpty();
        }

        // ── to_string ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task ToString_ShouldFormatDecimal_WhenFormatIsSpecified()
        {
            string template = AssignAndFilter("3.14159", "to_string", "format: 'F2'");
            string result = await RenderAsync(template, CultureInfo.InvariantCulture);
            result.Should().Be("3.14");
        }

        [Fact]
        public async Task ToString_ShouldPadWithWidth_WhenLenIsSpecified()
        {
            string template = AssignAndFilter("'Hi'", "to_string", "len: 10");
            string result = await RenderAsync(template, CultureInfo.InvariantCulture);
            result.Should().Be("        Hi");
        }

        // ── extract_file_name ─────────────────────────────────────────────────────

        [Fact]
        public async Task ExtractFileName_ShouldReturnFileName_WhenPathIsProvided()
        {
            string template = AssignAndFilter("'C:\\\\reports\\\\invoice.pdf'", "extract_file_name");
            string result = await RenderAsync(template);
            result.Should().Be("invoice.pdf");
        }

        // ── extract_directory_name ────────────────────────────────────────────────

        [Fact]
        public async Task ExtractDirectoryName_ShouldReturnDirectory_WhenPathIsProvided()
        {
            string template = AssignAndFilter("'C:\\\\reports\\\\invoice.pdf'", "extract_directory_name");
            string result = await RenderAsync(template);
            result.Should().Be(@"C:\reports");
        }

        // ── starts_with ───────────────────────────────────────────────────────────

        [Fact]
        public async Task StartsWith_ShouldReturnTrue_WhenStringStartsWithPrefix()
        {
            string template = AssignAndFilter("'Hello World'", "starts_with", "'Hello'");
            string result = await RenderAsync(template);
            result.Should().Be("true");
        }

        [Fact]
        public async Task StartsWith_ShouldReturnFalse_WhenStringDoesNotStartWithPrefix()
        {
            string template = AssignAndFilter("'Hello World'", "starts_with", "'World'");
            string result = await RenderAsync(template);
            result.Should().Be("false");
        }

        // ── ends_with ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task EndsWith_ShouldReturnTrue_WhenStringEndsWithSuffix()
        {
            string template = AssignAndFilter("'Hello World'", "ends_with", "'World'");
            string result = await RenderAsync(template);
            result.Should().Be("true");
        }

        [Fact]
        public async Task EndsWith_ShouldReturnFalse_WhenStringDoesNotEndWithSuffix()
        {
            string template = AssignAndFilter("'Hello World'", "ends_with", "'Hello'");
            string result = await RenderAsync(template);
            result.Should().Be("false");
        }

        // ── contains ──────────────────────────────────────────────────────────────

        [Fact]
        public async Task Contains_ShouldReturnTrue_WhenStringContainsSubstring()
        {
            string template = AssignAndFilter("'Hello World'", "contains", "'lo W'");
            string result = await RenderAsync(template);
            result.Should().Be("true");
        }

        [Fact]
        public async Task Contains_ShouldReturnFalse_WhenStringDoesNotContainSubstring()
        {
            string template = AssignAndFilter("'Hello World'", "contains", "'xyz'");
            string result = await RenderAsync(template);
            result.Should().Be("false");
        }

        // ── to_base64 / from_base64 ───────────────────────────────────────────────

        [Fact]
        public async Task ToBase64_ShouldEncodeString_WhenUtf8EncodingIsUsed()
        {
            string template = AssignAndFilter("'Hello'", "to_base64");
            string result = await RenderAsync(template);
            string expected = Convert.ToBase64String(Encoding.UTF8.GetBytes("Hello"));
            result.Should().Be(expected);
        }

        [Fact]
        public async Task FromBase64_ShouldDecodeString_WhenUtf8EncodingIsUsed()
        {
            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes("Hello"));
            string template = AssignAndFilter($"'{encoded}'", "from_base64");
            string result = await RenderAsync(template);
            result.Should().Be("Hello");
        }

        [Fact]
        public async Task ToBase64_ThenFromBase64_ShouldRoundTrip()
        {
            const string text = "FluidPDF";
            string template = AssignThenChainFilters($"'{text}'", "to_base64", "from_base64");
            string result = await RenderAsync(template);
            result.Should().Be(text);
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
                string template = AssignAndFilter($"'{escapedPath}'", "file_read_all_text");
                string result = await RenderAsync(template);
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
            string template = AssignAndFilter("'C:\\\\nonexistent\\\\file.txt'", "file_read_all_text");
            string result = await RenderAsync(template);
            result.Should().BeEmpty();
        }

        // ── file_read_all_lines ───────────────────────────────────────────────────

        [Fact]
        public async Task FileReadAllLines_ShouldReturnLines_WhenReadingFromStringSource()
        {
            string template = ForEachLine("'line1\\nline2\\nline3'", null, "LineContent");
            string result = await RenderAsync(template);
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
                string template = ForEachLine($"'{escapedPath}'", "'file'", "LineContent");
                string result = await RenderAsync(template);
                result.Should().Be("alpha,beta,gamma,");
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        // ── backslash / slash / pipe / double_quote / single_quote / path_separator ──

        [Fact]
        public async Task Backslash_ShouldOutputBackslash()
        {
            string result = await RenderAsync(CallFunction("backslash"));
            result.Should().Be("\\");
        }

        [Fact]
        public async Task Slash_ShouldOutputForwardSlash()
        {
            string result = await RenderAsync(CallFunction("slash"));
            result.Should().Be("/");
        }

        [Fact]
        public async Task Pipe_ShouldOutputPipeCharacter()
        {
            string result = await RenderAsync(CallFunction("pipe"));
            result.Should().Be("|");
        }

        [Fact]
        public async Task DoubleQuote_ShouldOutputDoubleQuote()
        {
            string result = await RenderAsync(CallFunction("double_quote"));
            result.Should().Be("\"");
        }

        [Fact]
        public async Task SingleQuote_ShouldOutputSingleQuote()
        {
            string result = await RenderAsync(CallFunction("single_quote"));
            result.Should().Be("'");
        }

        [Fact]
        public async Task PathSeparator_ShouldOutputOsDirectorySeparator()
        {
            string result = await RenderAsync(CallFunction("path_separator"));
            result.Should().Be(Path.DirectorySeparatorChar.ToString());
        }

        [Fact]
        public async Task StringEmpty_ShouldOutputEmptyString()
        {
            string result = await RenderAsync($"A{CallFunction("string_empty")}B");
            result.Should().Be("AB");
        }

        // ── float_random ──────────────────────────────────────────────────────────

        [Fact]
        public async Task FloatRandom_ShouldOutputValueBetweenZeroAndOne()
        {
            string result = await RenderAsync(CallFunction("float_random"));
            double.TryParse(result, NumberStyles.Any, CultureInfo.InvariantCulture, out double value).Should().BeTrue();
            value.Should().BeGreaterThanOrEqualTo(0.0).And.BeLessThan(1.0);
        }

        [Fact]
        public async Task FloatRandom_ShouldOutputDifferentValuesAcrossMultipleRenders()
        {
            List<string> results = [];
            for (int i = 0; i < 10; i++)
            {
                results.Add(await RenderAsync(CallFunction("float_random")));
            }

            results.Distinct().Should().HaveCountGreaterThan(1);
        }

        // ── guid ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task Guid_New_ShouldOutputValidGuid()
        {
            string result = await RenderAsync(CallFunction(GuidNew()));
            Guid.TryParse(result, out _).Should().BeTrue();
        }

        [Fact]
        public async Task Guid_New_ShouldOutputDifferentGuidsOnEachRender()
        {
            string first = await RenderAsync(CallFunction(GuidNew()));
            string second = await RenderAsync(CallFunction(GuidNew()));
            first.Should().NotBe(second);
        }

        [Fact]
        public async Task Guid_Empty_ShouldOutputAllZerosGuid()
        {
            string result = await RenderAsync(CallFunction(GuidEmpty()));
            result.Should().Be(System.Guid.Empty.ToString());
        }

        // ── int_random ────────────────────────────────────────────────────────────

        [Fact]
        public async Task IntRandom_ShouldOutputValueWithinRange_WhenMinAndMaxAreProvided()
        {
            for (int i = 0; i < 20; i++)
            {
                string result = await RenderAsync(CallFunction(IntRandomCall(10, 20)));
                int value = int.Parse(result);
                value.Should().BeGreaterThanOrEqualTo(10).And.BeLessThan(20);
            }
        }

        [Fact]
        public async Task IntRandom_ShouldOutputMinValue_WhenMinAndMaxAreEqual()
        {
            string result = await RenderAsync(CallFunction(IntRandomCall(5, 5)));
            result.Should().Be("5");
        }

        // ── Guid/IntRandom call-site helpers (engine-specific syntax) ─────────────

        protected abstract string GuidNew();
        protected abstract string GuidEmpty();
        protected abstract string IntRandomCall(int min, int max);
    }
}
