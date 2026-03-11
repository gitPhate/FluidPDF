using System;

namespace FluidPDF.Templating
{
    public sealed class FluidPDFTemplateRenderException(string message, Exception innerException) : Exception(message, innerException)
    {
    }
}
