namespace FluidPDF.Builder
{
    public class FluidPDFBuilder
    {
        public static IFluidPDFBuilder NewWithModel<T>(T model) where T : notnull => new FluidPDFInternalBuilder<T>(model);
    }
}
