using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace FluidPDF.Templating.Localization
{
    public interface ILocalizationProvider
    {
        ValueTask<Dictionary<string, string>> GetResourcesAsync(CultureInfo culture);
    }
}
