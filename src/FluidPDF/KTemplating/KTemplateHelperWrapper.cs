using KTemplating.Core.Support;
using Kyklos.Kernel.Core.Asserts;
using Kyklos.Kernel.Core.Strings;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;

namespace FluidPDF.Core.KTemplating
{
    internal class KTemplateHelperWrapperOptions
    {
        internal CultureInfo? CultureInfo { get; }
        internal string ModelName { get; }

        internal KTemplateHelperWrapperOptions(CultureInfo? cultureInfo = null, string? modelName = null)
        {
            CultureInfo = cultureInfo;
            ModelName = modelName.ToNullIfBlank() ?? "Model";
        }
    }

    internal class KTemplateHelperWrapper
    {
        private readonly KTemplateHelperWrapperOptions _options;

        internal KTemplateHelperWrapper(KTemplateHelperWrapperOptions options)
        {
            _options = options.GetNonNullOrThrow(nameof(options));
        }

        internal ValueTask<string> RenderTemplateWithDataRowAsync(string templateContent, DataRow dataRow) =>
            KTemplateHelper
                .RenderTemplateWithDataRowAsync(templateContent, dataRow, _options.ModelName, cultureInfo: _options.CultureInfo, encodeHtml: true);

        internal ValueTask<string> RenderTemplateWithDataTableAsync(string templateContent, DataTable dataTable) =>
            KTemplateHelper
                .RenderTemplateWithDataTableAsync(templateContent, dataTable, _options.ModelName, cultureInfo: _options.CultureInfo, encodeHtml: true);

        internal ValueTask<string> RenderTemplateWithDictionaryAsync(string templateContent, IDictionary<string, object> dictionary) =>
            KTemplateHelper
                .RenderTemplateWithDictionaryAsync(templateContent, dictionary, _options.ModelName, cultureInfo: _options.CultureInfo, encodeHtml: true);

        internal ValueTask<string> RenderTemplateWithJsonObjectAsync(string templateContent, JObject jsonObject) =>
            KTemplateHelper
                .RenderTemplateWithJsonObjectAsync(templateContent, jsonObject, _options.ModelName, cultureInfo: _options.CultureInfo, encodeHtml: true);

        internal ValueTask<string> RenderTemplateWithJsonStringAsync(string templateContent, string jsonString) =>
            KTemplateHelper
                .RenderTemplateWithJsonStringAsync(templateContent, jsonString, _options.ModelName, cultureInfo: _options.CultureInfo, encodeHtml: true);

        internal ValueTask<string> RenderTemplateWithMultipleModelsAsync(string templateContent, IEnumerable<KTemplatingModel> models) =>
            KTemplateHelper
                .RenderTemplateWithMultipleModelsAsync(templateContent, models, cultureInfo: _options.CultureInfo, encodeHtml: true);
    }
}
