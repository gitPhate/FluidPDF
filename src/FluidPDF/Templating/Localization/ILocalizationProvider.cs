using System.Collections.Generic;
using System.Globalization;

namespace FluidPDF.Templating.Localization
{
    public interface ILocalizationProvider
    {
        Dictionary<string, string> GetStrings(CultureInfo culture);
    }
}
