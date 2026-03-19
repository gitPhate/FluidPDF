using FluidPDF.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace FluidPDF.Templating.Localization
{
    public sealed class JsonFileLocalizationProvider(string basePath) : ILocalizationProvider
    {
        private readonly string _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));

        public Dictionary<string, string> GetStrings(CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(_basePath) || !Directory.Exists(_basePath))
            {
                throw new FluidPDFMissingLocalizationProviderException($"Localization directory '{_basePath}' does not exist.");
            }

            string cultureName = culture?.Name ?? "en-US";
            string filePath = Path.Combine(_basePath, $"{cultureName}.json");

            if (!File.Exists(filePath))
            {
                return [];
            }

            string json = File.ReadAllText(filePath);
            Dictionary<string, string>? strings = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (strings is null)
            {
                throw new FluidPDFMissingLocalizationProviderException($"Localization file '{filePath}' is invalid.");
            }

            return strings;
        }
    }
}
