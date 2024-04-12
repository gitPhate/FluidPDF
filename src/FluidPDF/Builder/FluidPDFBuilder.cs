using Kyklos.Kernel.Serialization.Json;

namespace FluidPDF.Core.Builder
{
    public class FluidPDFBuilder
    {
        public static IFluidPDFBuilder NewWithModel<T>(T model) => new FluidPDFInternalBuilder<T>(model);
        public static IFluidPDFBuilder NewWithJsonModel(object model)
        {
            string jsonModel = model.FormatAsJSon();
            return NewWithModel(jsonModel);
        }
    }
}
