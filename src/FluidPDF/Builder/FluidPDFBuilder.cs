using Sisifo.Json;

namespace FluidPDF.Builder
{
    public class FluidPDFBuilder
    {
        public static IFluidPDFBuilder NewWithModel<T>(T model) => new FluidPDFInternalBuilder<T>(model);
        public static IFluidPDFBuilder NewWithJsonModel(object model)
        {
            string jsonModel = model.ToJson();
            return NewWithModel(jsonModel);
        }
    }
}
