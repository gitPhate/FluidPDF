using FluidPDF.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FluidPDF.Templating.Localization
{
    internal static class LocalizationResolver
    {
        private static readonly CultureInfo _enUsCulture = new("en-US");

        private static readonly Regex HtmlTagRegex = new Regex("<\\s*/?\\s*[a-zA-Z][^>]*>", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static async ValueTask<Dictionary<string, string>> ResolveResourcesAsync(ILocalizationProvider provider, CultureInfo? requestedCulture)
        {
            CultureInfo cultureToUse = requestedCulture ?? _enUsCulture;

            Dictionary<string, string> strings = await provider.GetResourcesAsync(cultureToUse).ConfigureAwait(false);
            if (strings.Count == 0 && !string.Equals(cultureToUse.Name, _enUsCulture.Name, StringComparison.OrdinalIgnoreCase))
            {
                strings = await provider.GetResourcesAsync(_enUsCulture).ConfigureAwait(false);
            }

            if (strings.Count == 0)
            {
                throw new FluidPDFLocalizationException($"No localization strings were found for culture '{cultureToUse.Name}', and no 'en-US' fallback was available.");
            }

            ValidateNoHtml(strings);

            return strings;
        }

        private static void ValidateNoHtml(Dictionary<string, string> strings)
        {
            foreach (KeyValuePair<string, string> pair in strings)
            {
                string value = pair.Value ?? string.Empty;
                if (HtmlTagRegex.IsMatch(value))
                {
                    throw new FluidPDFLocalizationException($"Localization value for key '{pair.Key}' contains HTML markup, which is not allowed.");
                }
            }
        }
    }
}
