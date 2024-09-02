using System;
using System.Runtime.Serialization;

namespace FluidPDF.Fluid
{
    internal class FluidRenderException : Exception
    {
        public FluidRenderException()
        {
        }

        public FluidRenderException(string message) : base(message)
        {
        }

        public FluidRenderException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected FluidRenderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
