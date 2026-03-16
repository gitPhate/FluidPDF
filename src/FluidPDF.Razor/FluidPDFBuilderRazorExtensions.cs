using FluidPDF.Builder;

namespace FluidPDF.Razor
{
    public static class FluidPDFBuilderRazorExtensions
    {
        public static IFluidPDFBuilder WithRazorTemplateEngine(this IFluidPDFBuilder builder) =>
            builder.WithTemplateEngine(new RazorTemplateEngine());
    }
}
