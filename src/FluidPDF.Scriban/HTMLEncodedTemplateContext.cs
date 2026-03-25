using Scriban;
using Scriban.Parsing;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FluidPDF.Scriban
{
    internal class HTMLEncodedTemplateContext : TemplateContext
    {
        public override TemplateContext Write(SourceSpan span, object? textAsObject)
            => base.Write(span, EncodeHtml(textAsObject));

        public override ValueTask<TemplateContext> WriteAsync(SourceSpan span, object? textAsObject) =>
            base.WriteAsync(span, EncodeHtml(textAsObject));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static object? EncodeHtml(object? textAsObject) => textAsObject is not null && textAsObject is string text ? WebUtility.HtmlEncode(text) : textAsObject;
    }
}
