using Fluid;
using Fluid.Ast;
using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace FluidPDF.Fluid.Tags
{
    internal static class FluidBaseSimpleTag
    {
        public static Func<TextWriter, TextEncoder, TemplateContext, ValueTask<Completion>> TagRenderFunction(
            Func<TemplateContext, string> valueTagFx) =>
            async (writer, encoder, context) =>
            {
                string value = valueTagFx(context);
                await writer.WriteAsync(value).ConfigureAwait(false);
                return Completion.Normal;
            };
    }
}
