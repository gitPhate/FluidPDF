using FluentAssertions;
using FluidPDF.Razor;
using FluidPDF.Templating;
using FluidPDF.Tests.Mothers;
using RazorEngineCore;
using System.Collections.Generic;
using System.Data;

namespace FluidPDF.Tests
{
    public class RazorTemplateEngineTests : TemplateEngineTests, IDisposable
    {
        private readonly string _cacheDir = Path.Combine(AppContext.BaseDirectory, "FluidPDF_RazorCache_" + Guid.NewGuid().ToString("N"));

        protected override IFluidPDFTemplateEngine CreateEngine() => new RazorTemplateEngine();

        protected override string SimpleTemplate => TemplateModelMother.RazorSimpleTemplate;
        protected override string TwoModelTemplate => TemplateModelMother.RazorTwoModelTemplate;
        protected override string HtmlSpecialCharsTemplate => TemplateModelMother.RazorHtmlSpecialCharsTemplate;
        protected override string DataTableTemplate => TemplateModelMother.RazorDataTableTemplate();
        protected override string LocalizationTemplate => TemplateModelMother.RazorLocalizationTemplate;

        public void Dispose()
        {
            if (Directory.Exists(_cacheDir))
            {
                Directory.Delete(_cacheDir, true);
            }
        }

        // --- Engine-specific: invalid template throws RazorEngineCompilationException ---

        [Fact]
        public async Task RenderTemplateAsync_ShouldThrowFluidTemplateRenderException_WhenTemplateIsInvalid()
        {
            // Arrange
            object model = TemplateModelMother.SimpleObject();
            RazorTemplateEngine templateEngine = new();

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
            RazorTemplateEngine templateEngine = new();

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
            RazorTemplateEngine templateEngine = new();

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
            RazorTemplateEngine templateEngine = new();

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
            RazorTemplateEngine templateEngine = new();

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
            RazorTemplateEngine templateEngine = new();

            // Act
            Func<Task> act = async () =>
                await templateEngine.RenderTemplateAsync(TemplateModelMother.RazorTwoModelTemplate, models, new(), "CustomName");

            // Assert
            await act.Should().ThrowAsync<NotSupportedException>()
                .WithMessage($"*'{ModelNames.DefaultModelName}'*");
        }

        // --- Disk cache: cache miss compiles and saves the file ---

        [Fact]
        public async Task RenderTemplateAsync_WithCacheOptions_ShouldSaveCompiledFileOnFirstRender()
        {
            // Arrange
            object model = TemplateModelMother.SimpleObject();
            RazorCompiledTemplateCacheOptions cacheOptions = new(_cacheDir);
            RazorTemplateEngine templateEngine = new(cacheOptions);

            // Act
            await templateEngine.RenderTemplateAsync(SimpleTemplate, model, new());

            // Assert
            string[] files = Directory.GetFiles(_cacheDir, "*.dll");
            files.Should().ContainSingle();
        }

        // --- Disk cache: cache hit loads from file instead of recompiling ---

        [Fact]
        public async Task RenderTemplateAsync_WithCacheOptions_ShouldLoadFromCachedFileOnSubsequentRender()
        {
            // Arrange
            object model = TemplateModelMother.SimpleObject();
            RazorCompiledTemplateCacheOptions cacheOptions = new(_cacheDir);

            // First render — compiles and saves
            RazorTemplateEngine firstEngine = new(cacheOptions);
            await firstEngine.RenderTemplateAsync(TemplateModelMother.RazorSimpleTemplate, model, new());

            string[] filesAfterFirst = Directory.GetFiles(_cacheDir, "*.dll");
            filesAfterFirst.Should().ContainSingle();
            string cachedFile = filesAfterFirst.First();
            DateTime writtenAt = File.GetLastWriteTimeUtc(cachedFile);

            // Act — second render with a fresh engine instance pointing at the same cache
            await Task.Delay(10, TestContext.Current.CancellationToken); // ensure a recompile would produce a different timestamp
            RazorTemplateEngine secondEngine = new(cacheOptions);
            await secondEngine.RenderTemplateAsync(TemplateModelMother.RazorSimpleTemplate, model, new());

            // Assert — file must not have been overwritten (loaded, not recompiled)
            File.GetLastWriteTimeUtc(cachedFile).Should().Be(writtenAt);
        }

        // --- Disk cache: end-to-end round-trip produces correct output ---

        [Fact]
        public async Task RenderTemplateAsync_WithCacheOptions_ShouldReturnCorrectOutputFromCachedFile()
        {
            // Arrange
            object model = TemplateModelMother.SimpleObject();
            RazorCompiledTemplateCacheOptions cacheOptions = new(_cacheDir);

            // First render — populates the cache
            RazorTemplateEngine firstEngine = new(cacheOptions);
            await firstEngine.RenderTemplateAsync(TemplateModelMother.RazorSimpleTemplate, model, new());

            // Act — fresh engine instance, same cache directory
            RazorTemplateEngine secondEngine = new(cacheOptions);
            string result = await secondEngine.RenderTemplateAsync(TemplateModelMother.RazorSimpleTemplate, model, new());

            // Assert
            result.Should().Be(TemplateModelMother.SimpleObjectExpectedOutput);
        }

        // --- Disk cache: two distinct templates produce two separate cache files ---

        [Fact]
        public async Task RenderTemplateAsync_WithCacheOptions_ShouldCreateSeparateCacheFilesForDistinctTemplates()
        {
            // Arrange
            object model = TemplateModelMother.SimpleObject();
            RazorCompiledTemplateCacheOptions cacheOptions = new(_cacheDir);
            RazorTemplateEngine templateEngine = new(cacheOptions);

            // Act
            await templateEngine.RenderTemplateAsync(TemplateModelMother.RazorSimpleTemplate, model, new());
            await templateEngine.RenderTemplateAsync(TemplateModelMother.RazorHtmlSpecialCharsTemplate, TemplateModelMother.HtmlSpecialCharsObject(), new());

            // Assert
            string[] files = Directory.GetFiles(_cacheDir, "*.dll");
            files.Should().HaveCount(2);
        }

        [Fact]
        public async Task RenderTemplateAsync_WithCacheOptions_ShouldReuseSingleCacheFile_WhenEncodeHtmlChanges()
        {
            // Arrange
            object model = TemplateModelMother.HtmlSpecialCharsObject();
            RazorCompiledTemplateCacheOptions cacheOptions = new(_cacheDir);
            RazorTemplateEngine templateEngine = new(cacheOptions);

            // Act
            string encoded = await templateEngine.RenderTemplateAsync(TemplateModelMother.RazorHtmlSpecialCharsTemplate, model, new() { EncodeHtml = true });
            string raw = await templateEngine.RenderTemplateAsync(TemplateModelMother.RazorHtmlSpecialCharsTemplate, model, new() { EncodeHtml = false });

            // Assert
            encoded.Should().Be(TemplateModelMother.HtmlEncodedExpectedOutput);
            raw.Should().Be(TemplateModelMother.HtmlSpecialCharsRawExpectedOutput);

            string[] files = Directory.GetFiles(_cacheDir, "*.dll");
            files.Should().ContainSingle();
        }
    }
}
