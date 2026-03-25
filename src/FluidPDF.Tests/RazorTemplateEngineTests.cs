using FluentAssertions;
using FluidPDF.Razor;
using FluidPDF.Templating;
using FluidPDF.Tests.Mothers;
using RazorEngineCore;
using System.Data;

namespace FluidPDF.Tests
{
    public class RazorTemplateEngineTests : TemplateEngineTests
    {
        protected override IFluidPDFTemplateEngine CreateEngine() => new RazorTemplateEngine(new RecordingRazorTemplateCache());

        protected override string SimpleTemplate => TemplateModelMother.RazorSimpleTemplate;
        protected override string TwoModelTemplate => TemplateModelMother.RazorTwoModelTemplate;
        protected override string HtmlSpecialCharsTemplate => TemplateModelMother.RazorHtmlSpecialCharsTemplate;
        protected override string DataTableTemplate => TemplateModelMother.RazorDataTableTemplate();
        protected override string LocalizationTemplate => TemplateModelMother.RazorLocalizationTemplate;

        // --- Engine-specific: invalid template throws RazorEngineCompilationException ---

        [Fact]
        public async Task RenderTemplateAsync_ShouldThrowFluidTemplateRenderException_WhenTemplateIsInvalid()
        {
            // Arrange
            object model = TemplateModelMother.SimpleObject();
            using RazorTemplateEngine templateEngine = new(new RecordingRazorTemplateCache());

            // Act
            Func<Task> act = async () =>
                await templateEngine.RenderTemplateAsync("@{ var x = }", model, new());

            // Assert
            await act.Should().ThrowAsync<RazorEngineCompilationException>();
        }

        // --- Model name restriction: non-default model name throws NotSupportedException ---

        [Fact]
        public async Task RenderTemplateAsync_WithObjectModel_ShouldThrowNotSupportedException_WhenModelNameIsNotDefault()
        {
            // Arrange
            object model = TemplateModelMother.SimpleObject();
            using RazorTemplateEngine templateEngine = new(new RecordingRazorTemplateCache());

            // Act
            Func<Task> act = async () =>
                await templateEngine.RenderTemplateAsync(TemplateModelMother.RazorSimpleTemplate, model, new(), "CustomName");

            // Assert
            await act.Should().ThrowAsync<NotSupportedException>()
                .WithMessage($"*'{ModelNames.DefaultModelName}'*");
        }

        [Fact]
        public async Task RenderTemplateAsync_WithDictionaryModel_ShouldThrowNotSupportedException_WhenModelNameIsNotDefault()
        {
            // Arrange
            Dictionary<string, object> model = TemplateModelMother.SimpleDictionary();
            using RazorTemplateEngine templateEngine = new(new RecordingRazorTemplateCache());

            // Act
            Func<Task> act = async () =>
                await templateEngine.RenderTemplateAsync(TemplateModelMother.RazorSimpleTemplate, model, new(), "CustomName");

            // Assert
            await act.Should().ThrowAsync<NotSupportedException>()
                .WithMessage($"*'{ModelNames.DefaultModelName}'*");
        }

        [Fact]
        public async Task RenderTemplateAsync_WithJsonStringModel_ShouldThrowNotSupportedException_WhenModelNameIsNotDefault()
        {
            // Arrange
            using RazorTemplateEngine templateEngine = new(new RecordingRazorTemplateCache());

            // Act
            Func<Task> act = async () =>
                await templateEngine.RenderTemplateAsync(TemplateModelMother.RazorSimpleTemplate, TemplateModelMother.SimpleJsonString, new(), "CustomName");

            // Assert
            await act.Should().ThrowAsync<NotSupportedException>()
                .WithMessage($"*'{ModelNames.DefaultModelName}'*");
        }

        [Fact]
        public async Task RenderTemplateAsync_WithDataTableModel_ShouldThrowNotSupportedException_WhenModelNameIsNotDefault()
        {
            // Arrange
            DataTable model = TemplateModelMother.SimpleDataTable();
            using RazorTemplateEngine templateEngine = new(new RecordingRazorTemplateCache());

            // Act
            Func<Task> act = async () =>
                await templateEngine.RenderTemplateAsync(TemplateModelMother.RazorDataTableTemplate(), model, new(), "CustomName");

            // Assert
            await act.Should().ThrowAsync<NotSupportedException>()
                .WithMessage($"*'{ModelNames.DefaultModelName}'*");
        }

        [Fact]
        public async Task RenderTemplateAsync_WithFluidPDFTemplateModelArray_ShouldThrowNotSupportedException_WhenModelNameIsNotDefault()
        {
            // Arrange
            FluidPDFTemplateModel[] models = TemplateModelMother.TwoModelArray();
            using RazorTemplateEngine templateEngine = new(new RecordingRazorTemplateCache());

            // Act
            Func<Task> act = async () =>
                await templateEngine.RenderTemplateAsync(TemplateModelMother.RazorTwoModelTemplate, models, new(), "CustomName");

            // Assert
            await act.Should().ThrowAsync<NotSupportedException>()
                .WithMessage($"*'{ModelNames.DefaultModelName}'*");
        }

        // --- Cache: cache miss compiles and stores the compiled template ---

        [Fact]
        public async Task RenderTemplateAsync_WithTemplateCache_ShouldStoreCompiledTemplateOnFirstRender()
        {
            // Arrange
            object model = TemplateModelMother.SimpleObject();
            RecordingRazorTemplateCache cache = new();
            using RazorTemplateEngine templateEngine = new(cache);

            // Act
            await templateEngine.RenderTemplateAsync(SimpleTemplate, model, new());

            // Assert
            cache.SetCalls.Should().Be(1);
            cache.Count.Should().Be(1);
        }

        // --- Cache: cache hit reuses the stored compiled template ---

        [Fact]
        public async Task RenderTemplateAsync_WithTemplateCache_ShouldReuseStoredTemplateOnSubsequentRender()
        {
            // Arrange
            object model = TemplateModelMother.SimpleObject();
            RecordingRazorTemplateCache cache = new();

            // First render — compiles and stores
            using RazorTemplateEngine firstEngine = new(cache);
            await firstEngine.RenderTemplateAsync(TemplateModelMother.RazorSimpleTemplate, model, new());

            // Act — second render with a fresh engine instance pointing at the same cache
            using RazorTemplateEngine secondEngine = new(cache);
            await secondEngine.RenderTemplateAsync(TemplateModelMother.RazorSimpleTemplate, model, new());

            // Assert
            cache.SetCalls.Should().Be(1);
            cache.GetCalls.Should().Be(3);
        }

        // --- Cache: round-trip still renders expected output ---

        [Fact]
        public async Task RenderTemplateAsync_WithTemplateCache_ShouldReturnCorrectOutputFromCachedTemplate()
        {
            // Arrange
            object model = TemplateModelMother.SimpleObject();
            RecordingRazorTemplateCache cache = new();

            // First render — populates the cache
            using RazorTemplateEngine firstEngine = new(cache);
            await firstEngine.RenderTemplateAsync(TemplateModelMother.RazorSimpleTemplate, model, new());

            // Act — fresh engine instance, same cache
            using RazorTemplateEngine secondEngine = new(cache);
            string result = await secondEngine.RenderTemplateAsync(TemplateModelMother.RazorSimpleTemplate, model, new());

            // Assert
            result.Should().Be(TemplateModelMother.SimpleObjectExpectedOutput);
        }

        // --- Cache: distinct templates produce distinct entries ---

        [Fact]
        public async Task RenderTemplateAsync_WithTemplateCache_ShouldCreateSeparateEntriesForDistinctTemplates()
        {
            // Arrange
            object model = TemplateModelMother.SimpleObject();
            RecordingRazorTemplateCache cache = new();
            using RazorTemplateEngine templateEngine = new(cache);

            // Act
            await templateEngine.RenderTemplateAsync(TemplateModelMother.RazorSimpleTemplate, model, new());
            await templateEngine.RenderTemplateAsync(TemplateModelMother.RazorHtmlSpecialCharsTemplate, TemplateModelMother.HtmlSpecialCharsObject(), new());

            // Assert
            cache.Count.Should().Be(2);
        }

        [Fact]
        public async Task RenderTemplateAsync_WithTemplateCache_ShouldReuseSingleEntry_WhenEncodeHtmlChanges()
        {
            // Arrange
            object model = TemplateModelMother.HtmlSpecialCharsObject();
            RecordingRazorTemplateCache cache = new();
            using RazorTemplateEngine templateEngine = new(cache);

            // Act
            string encoded = await templateEngine.RenderTemplateAsync(TemplateModelMother.RazorHtmlSpecialCharsTemplate, model, new() { EncodeHtml = true });
            string raw = await templateEngine.RenderTemplateAsync(TemplateModelMother.RazorHtmlSpecialCharsTemplate, model, new() { EncodeHtml = false });

            // Assert
            encoded.Should().Be(TemplateModelMother.HtmlEncodedExpectedOutput);
            raw.Should().Be(TemplateModelMother.HtmlSpecialCharsRawExpectedOutput);

            cache.Count.Should().Be(1);
        }

        [Fact]
        public async Task RenderTemplateAsync_ShouldBypassEncoding_WhenValueIsWrappedWithRawAndEncodeHtmlIsTrue()
        {
            // Arrange
            object model = TemplateModelMother.HtmlSpecialCharsObject();
            string template = "<p>@Raw(Model.Value)</p>";
            using RazorTemplateEngine templateEngine = new(new RecordingRazorTemplateCache());
            FluidPDFTemplateRenderOptions options = new() { EncodeHtml = true };

            // Act
            string result = await templateEngine.RenderTemplateAsync(template, model, options);

            // Assert
            result.Should().Be(TemplateModelMother.HtmlSpecialCharsRawExpectedOutput);
        }

        [Fact]
        public async Task RenderTemplateAsync_ShouldEncodeString_WhenEncodeHtmlIsTrue()
        {
            // Arrange
            object model = TemplateModelMother.HtmlSpecialCharsObject();
            string template = TemplateModelMother.RazorHtmlSpecialCharsTemplate;
            using RazorTemplateEngine templateEngine = new(new RecordingRazorTemplateCache());
            FluidPDFTemplateRenderOptions options = new() { EncodeHtml = true };

            // Act
            string result = await templateEngine.RenderTemplateAsync(template, model, options);

            // Assert
            result.Should().Be(TemplateModelMother.HtmlEncodedExpectedOutput);
        }

        [Fact]
        public async Task RenderTemplateAsync_ShouldNotEncodeString_WhenEncodeHtmlIsFalse()
        {
            // Arrange
            object model = TemplateModelMother.HtmlSpecialCharsObject();
            string template = TemplateModelMother.RazorHtmlSpecialCharsTemplate;
            using RazorTemplateEngine templateEngine = new(new RecordingRazorTemplateCache());
            FluidPDFTemplateRenderOptions options = new() { EncodeHtml = false };

            // Act
            string result = await templateEngine.RenderTemplateAsync(template, model, options);

            // Assert
            result.Should().Be(TemplateModelMother.HtmlSpecialCharsRawExpectedOutput);
        }

        [Fact]
        public void Dispose_ShouldDisposeCompileLocksAndAllowRecreationOfEngine()
        {
            // Arrange
            RecordingRazorTemplateCache cache = new();
            RazorTemplateEngine templateEngine = new(cache);

            // Act
            templateEngine.Dispose();

            // Assert
            cache.Count.Should().Be(0);
        }

        private sealed class RecordingRazorTemplateCache : IRazorTemplateCache
        {
            private readonly Dictionary<string, IFluidPDFRazorCompiledTemplate> _templates = new(StringComparer.Ordinal);

            public int GetCalls { get; private set; }

            public int SetCalls { get; private set; }

            public int Count => _templates.Count;

            public Task<IFluidPDFRazorCompiledTemplate?> GetRazorTemplateAsync(string template)
            {
                GetCalls++;
                _templates.TryGetValue(template, out IFluidPDFRazorCompiledTemplate? compiledTemplate);
                return Task.FromResult<IFluidPDFRazorCompiledTemplate?>(compiledTemplate);
            }

            public Task SetRazorTemplateAsync(string template, IFluidPDFRazorCompiledTemplate compiledTemplate)
            {
                SetCalls++;
                _templates[template] = compiledTemplate;
                return Task.CompletedTask;
            }
        }
    }
}
