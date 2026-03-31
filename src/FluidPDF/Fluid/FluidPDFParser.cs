using Fluid;
using Fluid.Ast;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace FluidPDF.Fluid
{
    internal sealed class FluidPDFParser : FluidParser
    {
        public void RegisterArgumentsTag(
            string tagName,
            Func<IReadOnlyList<FilterArgument>, TextWriter, TextEncoder, TemplateContext, ValueTask<Completion>> render)
        {
            RegisterParserTag(tagName, ArgumentsList, render);
        }
    }
}
