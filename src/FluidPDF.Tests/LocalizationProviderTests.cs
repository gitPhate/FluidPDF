using FluentAssertions;
using FluidPDF.Exceptions;
using FluidPDF.Templating.Localization;
using System.Globalization;
using System.Text.Json;

namespace FluidPDF.Tests
{
    public class LocalizationProviderTests : IDisposable
    {
        private readonly string _tempDir = Path.Combine(AppContext.BaseDirectory, "FluidPDF_Localization_" + Guid.NewGuid().ToString("N"));

        [Fact]
        public async ValueTask DictionaryLocalizationProvider_GetStrings_ShouldReturnCultureDictionary_WhenCultureExists()
        {
            // Arrange
            DictionaryLocalizationProvider provider = new(
                new Dictionary<string, Dictionary<string, string>>
                {
                    ["en-US"] = new()
                    {
                        ["label_title"] = "Invoice"
                    },
                    ["it-IT"] = new()
                    {
                        ["label_title"] = "Fattura"
                    }
                });

            // Act
            Dictionary<string, string> strings = await provider.GetResourcesAsync(new CultureInfo("it-IT"));

            // Assert
            strings["label_title"].Should().Be("Fattura");
        }

        [Fact]
        public async Task DictionaryLocalizationProvider_GetStrings_ShouldThrow_WhenEnUsIsMissing()
        {
            // Arrange
            DictionaryLocalizationProvider provider = new(
                new Dictionary<string, Dictionary<string, string>>
                {
                    ["it-IT"] = new()
                    {
                        ["label_title"] = "Fattura"
                    }
                });

            // Act
            Func<Task> act = async () => await provider.GetResourcesAsync(new CultureInfo("it-IT"));

            // Assert
            await act.Should().ThrowAsync<FluidPDFMissingLocalizationProviderException>();
        }

        [Fact]
        public async ValueTask JsonFileLocalizationProvider_GetStrings_ShouldReadCultureFile_WhenFileExists()
        {
            // Arrange
            Directory.CreateDirectory(_tempDir);
            string filePath = Path.Combine(_tempDir, "en-US.json");
            string json = JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["label_title"] = "Invoice"
            });

            File.WriteAllText(filePath, json);
            JsonFileLocalizationProvider provider = new(_tempDir);

            // Act
            Dictionary<string, string> strings = await provider.GetResourcesAsync(new CultureInfo("en-US"));

            // Assert
            strings["label_title"].Should().Be("Invoice");
        }

        [Fact]
        public async ValueTask JsonFileLocalizationProvider_GetStrings_ShouldReturnEmpty_WhenCultureFileDoesNotExist()
        {
            // Arrange
            Directory.CreateDirectory(_tempDir);
            string filePath = Path.Combine(_tempDir, "en-US.json");
            string json = JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["label_title"] = "Invoice"
            });

            File.WriteAllText(filePath, json);
            JsonFileLocalizationProvider provider = new(_tempDir);

            // Act
            Dictionary<string, string> strings = await provider.GetResourcesAsync(new CultureInfo("it-IT"));

            // Assert
            strings.Should().BeEmpty();
        }

        [Fact]
        public async Task JsonFileLocalizationProvider_GetStrings_ShouldThrow_WhenDirectoryIsMissing()
        {
            // Arrange
            JsonFileLocalizationProvider provider = new(Path.Combine(_tempDir, "missing"));

            // Act
            Func<Task> act = async () => await provider.GetResourcesAsync(new CultureInfo("it-IT"));

            // Assert
            await act.Should().ThrowAsync<FluidPDFMissingLocalizationProviderException>();
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }
    }
}
