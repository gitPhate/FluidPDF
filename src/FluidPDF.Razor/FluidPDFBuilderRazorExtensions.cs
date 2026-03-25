using FluidPDF.Builder;

namespace FluidPDF.Razor
{
    public static class FluidPDFBuilderRazorExtensions
    {
        public static IFluidPDFBuilder WithRazorTemplateEngine(this IFluidPDFBuilder builder) =>
            builder.WithRazorTemplateEngine(new NullRazorTemplateCache());

        public static IFluidPDFBuilder WithRazorTemplateEngine(this IFluidPDFBuilder builder, IRazorTemplateCache razorTemplateCache) =>
            builder.WithTemplateEngine(new RazorTemplateEngine(razorTemplateCache));

        public static IFluidPDFBuilder WithRazorTemplateEngine(this IFluidPDFBuilder builder, RazorTemplateCacheOptions cacheOptions) =>
            builder.WithRazorTemplateEngine(new RazorTemplateCache(cacheOptions));
    }
}
