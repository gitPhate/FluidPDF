#nullable disable

using RazorEngineCore;
using System.IO;
using System.Threading.Tasks;

namespace FluidPDF.Razor
{
    /// <summary>
    /// Opaque handle returned by <see cref="IRazorTemplateCache"/>.
    /// Implementors of <see cref="IRazorTemplateCache"/> store and return objects
    /// of this type; the engine resolves the full compiled template internally.
    /// </summary>
    public interface IFluidPDFRazorCompiledTemplate { }

    internal interface IInternalFluidPDFRazorCompiledTemplate : IFluidPDFRazorCompiledTemplate
    {
        void EnableDebugging(string debuggingOutputDirectory = null);
        string Run(RazorRuntimeModel model, bool encodeHtml = false);
        Task<string> RunAsync(RazorRuntimeModel model, bool encodeHtml = false);
        void SaveToFile(string fileName);
        Task SaveToFileAsync(string fileName);
        void SaveToStream(Stream stream);
        Task SaveToStreamAsync(Stream stream);
    }

    internal sealed class FluidPDFRazorCompiledTemplate(IRazorEngineCompiledTemplate<FluidPDFRazorTemplateBase> obj) : IInternalFluidPDFRazorCompiledTemplate
    {
        public void EnableDebugging(string debuggingOutputDirectory = null) => obj.EnableDebugging(debuggingOutputDirectory);

        public string Run(RazorRuntimeModel model, bool encodeHtml = false) =>
            obj.Run(instance =>
            {
                instance.Model = model.DefaultModelBuild;
                instance.Resx = model.ResxModelBuild;
                instance.EncodeHtml = encodeHtml;
            });

        public Task<string> RunAsync(RazorRuntimeModel model, bool encodeHtml = false) =>
            obj.RunAsync(instance =>
            {
                instance.Model = model.DefaultModelBuild;
                instance.Resx = model.ResxModelBuild;
                instance.EncodeHtml = encodeHtml;
            });

        public void SaveToFile(string fileName) => obj.SaveToFile(fileName);

        public Task SaveToFileAsync(string fileName) => obj.SaveToFileAsync(fileName);

        public void SaveToStream(Stream stream) => obj.SaveToStream(stream);

        public Task SaveToStreamAsync(Stream stream) => obj.SaveToStreamAsync(stream);
    }

    internal sealed class FluidPDFRazorCachedCompiledTemplate(RazorEngineCompiledTemplate obj) : IInternalFluidPDFRazorCompiledTemplate
    {
        public void EnableDebugging(string debuggingOutputDirectory = null) => obj.EnableDebugging(debuggingOutputDirectory);

        public string Run(RazorRuntimeModel model, bool encodeHtml = false) =>
            obj.Run(model.UnifiedModelBuild);

        public Task<string> RunAsync(RazorRuntimeModel model = null, bool encodeHtml = false) =>
            obj.RunAsync(model.UnifiedModelBuild);

        public void SaveToFile(string fileName) => obj.SaveToFile(fileName);

        public Task SaveToFileAsync(string fileName) => obj.SaveToFileAsync(fileName);

        public void SaveToStream(Stream stream) => obj.SaveToStream(stream);

        public Task SaveToStreamAsync(Stream stream) => obj.SaveToStreamAsync(stream);
    }
}
