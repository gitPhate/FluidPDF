using RazorEngineCore;
using System.Net;

namespace FluidPDF.Razor
{
    public abstract class FluidPDFRazorTemplateBase : RazorEngineTemplateBase
    {
        public bool EncodeHtml { get; set; }

        public dynamic? Resx { get; set; }

        public override void Write(object? value)
        {
            if (EncodeHtml && value is string str)
            {
                base.Write(WebUtility.HtmlEncode(str));
                return;
            }

            base.Write(value);
        }
    }
}
