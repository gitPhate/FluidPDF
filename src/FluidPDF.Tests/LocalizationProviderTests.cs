using FluentAssertions;
using FluidPDF.Exceptions;
using FluidPDF.Templating.Localization;
using System.Globalization;
using System.Text.Json;

namespace FluidPDF.Tests
{
    public class LocalizationProviderTests : IDisposable
    {
        private readonly string _tempDir = Path.Combine(Path.GetTempPath(), "FluidPDF_Localization_" + Guid.NewGuid().ToString("N"));

        [Fact]
        public void DictionaryLocalizationProvider_GetStrings_ShouldReturnCultureDictionary_WhenCultureExists()
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
            Dictionary<string, string> strings = provider.GetStrings(new CultureInfo("it-IT"));

            // Assert
            strings["label_title"].Should().Be("Fattura");
        }

        [Fact]
        public void DictionaryLocalizationProvider_GetStrings_ShouldThrow_WhenEnUsIsMissing()
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
            Action act = () => provider.GetStrings(new CultureInfo("it-IT"));

            // Assert
            act.Should().Throw<FluidPDFMissingLocalizationProviderException>();
        }

        [Fact]
        public void JsonFileLocalizationProvider_GetStrings_ShouldReadCultureFile_WhenFileExists()
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
            Dictionary<string, string> strings = provider.GetStrings(new CultureInfo("en-US"));

            // Assert
            strings["label_title"].Should().Be("Invoice");
        }

        [Fact]
        public void JsonFileLocalizationProvider_GetStrings_ShouldReturnEmpty_WhenCultureFileDoesNotExist()
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
            Dictionary<string, string> strings = provider.GetStrings(new CultureInfo("it-IT"));

            // Assert
            strings.Should().BeEmpty();
        }

        [Fact]
        public void JsonFileLocalizationProvider_GetStrings_ShouldThrow_WhenDirectoryIsMissing()
        {
            // Arrange
            JsonFileLocalizationProvider provider = new(Path.Combine(_tempDir, "missing"));

            // Act
            Action act = () => provider.GetStrings(new CultureInfo("en-US"));

            // Assert
            act.Should().Throw<FluidPDFMissingLocalizationProviderException>();
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
