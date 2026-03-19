using RazorEngineCore;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;

namespace FluidPDF.Razor
{
    public abstract class FluidPDFRazorTemplateBase : RazorEngineTemplateBase
    {
        private dynamic? _resx;

        public bool EncodeHtml { get; set; }

        public dynamic? Resx
        {
            get
            {
                if (_resx is not null)
                {
                    return _resx;
                }

                return TryReadRuntimeValue(FluidPDFRazorRuntimeModel.ResxKey);
            }
            set => _resx = value;
        }

        public override void Write(object? value)
        {
            if (ShouldEncodeHtml() && value is string str)
            {
                base.Write(WebUtility.HtmlEncode(str));
                return;
            }

            base.Write(value);
        }

        private bool ShouldEncodeHtml()
        {
            if (EncodeHtml)
            {
                return true;
            }

            object? runtimeEncodeHtml = TryReadRuntimeValue(FluidPDFRazorRuntimeModel.EncodeHtmlKey);
            return runtimeEncodeHtml is true;
        }

        private object? TryReadRuntimeValue(string key)
        {
            if (Model is IDictionary<string, object?> genericDictionary && genericDictionary.TryGetValue(key, out object? genericValue))
            {
                return genericValue;
            }

            if (Model is IDictionary<string, object> dictionary && dictionary.TryGetValue(key, out object? value))
            {
                return value;
            }

            if (Model is ExpandoObject expando)
            {
                IDictionary<string, object?> expandoDictionary = expando;
                if (expandoDictionary.TryGetValue(key, out object? expandoValue))
                {
                    return expandoValue;
                }
            }

            return null;
        }
    }

    internal static class FluidPDFRazorRuntimeModel
    {
        public const string EncodeHtmlKey = "__fluidpdf_encode_html";
        public const string ResxKey = "__fluidpdf_resx";

        public static object? EnrichModel(object? model, dynamic? resx, bool encodeHtml)
        {
            if (model is IDictionary<string, object?> genericDictionary)
            {
                genericDictionary[EncodeHtmlKey] = encodeHtml;
                genericDictionary[ResxKey] = resx;
                return model;
            }

            if (model is IDictionary<string, object> dictionary)
            {
                dictionary[EncodeHtmlKey] = encodeHtml;

                if (resx is not null)
                {
                    dictionary[ResxKey] = resx;
                }
                else
                {
                    dictionary.Remove(ResxKey);
                }

                return model;
            }

            ExpandoObject wrapped = new();
            IDictionary<string, object?> wrappedDictionary = wrapped;

            wrappedDictionary["Value"] = model;
            wrappedDictionary[EncodeHtmlKey] = encodeHtml;
            wrappedDictionary[ResxKey] = resx;

            return wrapped;
        }
    }
}
