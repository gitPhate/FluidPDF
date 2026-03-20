using FluidPDF.Exceptions;
using FluidPDF.Support.IO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace FluidPDF.Templating.Localization
{
    public sealed class JsonFileLocalizationProvider(string basePath) : ILocalizationProvider
    {
        private readonly string _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));

        public async ValueTask<Dictionary<string, string>> GetResourcesAsync(CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(_basePath) || !Directory.Exists(_basePath))
            {
                throw new FluidPDFMissingLocalizationProviderException($"Localization directory '{_basePath}' does not exist.");
            }

            string cultureName = culture.Name;
            string filePath = Path.Combine(_basePath, $"{cultureName}.json");

            if (!File.Exists(filePath))
            {
                return [];
            }

            string json = await FileHelper.ReadAllTextAsync(filePath).ConfigureAwait(false);
            JsonLocalizationProvider jsonProvider = new(json);
            return await jsonProvider.GetResourcesAsync(culture).ConfigureAwait(false);
        }
    }
}
