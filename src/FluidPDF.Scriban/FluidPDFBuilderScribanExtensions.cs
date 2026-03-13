using FluidPDF.Builder;

namespace FluidPDF.Scriban
{
    public static class FluidPDFBuilderScribanExtensions
    {
        public static IFluidPDFBuilder WithScribanTemplateEngine(this IFluidPDFBuilder builder) =>
            builder.WithTemplateEngine(new ScribanTemplateEngine());
    }
}
