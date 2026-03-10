using System;

namespace FluidPDF.Templating
{
    internal class FluidPDFTemplateRenderException : Exception
    {
        public FluidPDFTemplateRenderException()
        {
        }

        public FluidPDFTemplateRenderException(string message) : base(message)
        {
        }

        public FluidPDFTemplateRenderException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
