using FluidPDF.Builder;

namespace FluidPDF.Razor
{
    public static class FluidPDFBuilderRazorExtensions
    {
        public static IFluidPDFBuilder WithRazorTemplateEngine(this IFluidPDFBuilder builder) =>
            builder.WithTemplateEngine(new RazorTemplateEngine());

        public static IFluidPDFBuilder WithRazorTemplateEngine(this IFluidPDFBuilder builder, RazorCompiledTemplateCacheOptions cacheOptions) =>
            builder.WithTemplateEngine(new RazorTemplateEngine(cacheOptions));
    }
}
