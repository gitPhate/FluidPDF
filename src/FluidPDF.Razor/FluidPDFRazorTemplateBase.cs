using RazorEngineCore;
using System.Text.Encodings.Web;

namespace FluidPDF.Razor
{
    public abstract class FluidPDFRazorTemplateBase : RazorEngineTemplateBase
    {
        private static readonly HtmlEncoder _htmlEncoder = HtmlEncoder.Default;

        public bool EncodeHtml { get; set; }

        public dynamic? Resx { get; set; }

        public static object Raw(object? value) => new RawContent(value);

        public override void Write(object? value)
        {
            if (value is RawContent rawContent)
            {
                base.Write(rawContent.Value);
                return;
            }

            if (value is string str)
            {
                base.Write(EncodeHtml ? _htmlEncoder.Encode(str) : str);
                return;
            }

            base.Write(value);
        }

        public override void WriteAttributeValue(string prefix, int prefixOffset, object value, int valueOffset, int valueLength, bool isLiteral)
        {
            if (value is RawContent rawContent)
            {
                base.WriteAttributeValue(prefix, prefixOffset, rawContent.Value, valueOffset, valueLength, isLiteral);
                return;
            }

            if (value is string str)
            {
                base.WriteAttributeValue(prefix, prefixOffset, EncodeHtml ? _htmlEncoder.Encode(str) : str, valueOffset, valueLength, isLiteral);
                return;
            }

            base.WriteAttributeValue(prefix, prefixOffset, value, valueOffset, valueLength, isLiteral);
        }
    }

    internal sealed class RawContent(object? value)
    {
        public object? Value { get; } = value;
    }
}
