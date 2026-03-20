using System;

namespace FluidPDF.Exceptions
{
    public class FluidPDFBuilderConfigException(string message) : Exception(message)
    {
    }

    public sealed class FluidPDFTemplateRenderException(string message, Exception innerException) : Exception(message, innerException)
    {
    }

    public sealed class FluidTemplateRenderException(string message) : Exception(message)
    {
    }

    public sealed class FluidPDFLocalizationException(string message) : Exception(message)
    {
    }

    public sealed class FluidPDFMissingLocalizationProviderException(string? message, Exception? innerException) : Exception(message, innerException)
    {
        public FluidPDFMissingLocalizationProviderException(string message) : this(message, null)
        {
        }
    }
}
