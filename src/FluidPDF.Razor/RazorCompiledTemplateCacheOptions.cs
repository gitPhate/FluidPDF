namespace FluidPDF.Razor
{
    public sealed class RazorCompiledTemplateCacheOptions(string cachePath)
    {
        public string CachePath { get; } = cachePath;
    }
}
