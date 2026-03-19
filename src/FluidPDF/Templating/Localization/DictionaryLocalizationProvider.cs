using FluidPDF.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace FluidPDF.Templating.Localization
{
    public sealed class DictionaryLocalizationProvider(Dictionary<string, Dictionary<string, string>> localizations) : ILocalizationProvider
    {
        private readonly Dictionary<string, Dictionary<string, string>> _localizations = localizations ?? throw new ArgumentNullException(nameof(localizations));

        public Dictionary<string, string> GetStrings(CultureInfo culture)
        {
            if (_localizations.Count == 0)
            {
                throw new FluidPDFMissingLocalizationProviderException("Localization source is empty.");
            }

            if (!_localizations.ContainsKey("en-US"))
            {
                throw new FluidPDFMissingLocalizationProviderException("Localization source must contain the required fallback culture 'en-US'.");
            }

            string cultureName = culture?.Name ?? "en-US";
            if (_localizations.TryGetValue(cultureName, out Dictionary<string, string>? values))
            {
                return values;
            }

            return [];
        }
    }
}
