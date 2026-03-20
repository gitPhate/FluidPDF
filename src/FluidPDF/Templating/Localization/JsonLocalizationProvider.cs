using FluidPDF.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;

namespace FluidPDF.Templating.Localization
{
    public sealed class JsonLocalizationProvider(string json) : ILocalizationProvider
    {
        public ValueTask<Dictionary<string, string>> GetResourcesAsync(CultureInfo culture)
        {
            try
            {
                Dictionary<string, string> data =
                    JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                    ?? throw new ArgumentException("Unable to deserialize the data");

                return new ValueTask<Dictionary<string, string>>(data);
            }
            catch (Exception ex)
            {
                throw new FluidPDFMissingLocalizationProviderException("An error occurred in deserializing the data", ex);
            }
        }
    }
}
