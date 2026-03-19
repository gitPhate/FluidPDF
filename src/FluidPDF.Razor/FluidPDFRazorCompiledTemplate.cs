#nullable disable

using RazorEngineCore;
using System.IO;
using System.Threading.Tasks;

namespace FluidPDF.Razor
{
    internal interface IFluidPDFRazorCompiledTemplate
    {
        void EnableDebugging(string debuggingOutputDirectory = null);
        string Run(object model = null, dynamic resx = null, bool encodeHtml = false);
        Task<string> RunAsync(object model = null, dynamic resx = null, bool encodeHtml = false);
        void SaveToFile(string fileName);
        Task SaveToFileAsync(string fileName);
        void SaveToStream(Stream stream);
        Task SaveToStreamAsync(Stream stream);
    }

    internal class FluidPDFRazorCompiledTemplate(IRazorEngineCompiledTemplate<FluidPDFRazorTemplateBase> obj) : IFluidPDFRazorCompiledTemplate
    {
        public void EnableDebugging(string debuggingOutputDirectory = null) => obj.EnableDebugging(debuggingOutputDirectory);

        public string Run(object model = null, dynamic resx = null, bool encodeHtml = false) =>
            obj.Run(instance =>
            {
                instance.Model = FluidPDFRazorRuntimeModel.EnrichModel(model, resx, encodeHtml);
                instance.Resx = resx;
                instance.EncodeHtml = encodeHtml;
            });

        public Task<string> RunAsync(object model = null, dynamic resx = null, bool encodeHtml = false) =>
            obj.RunAsync(instance =>
            {
                instance.Model = FluidPDFRazorRuntimeModel.EnrichModel(model, resx, encodeHtml);
                instance.Resx = resx;
                instance.EncodeHtml = encodeHtml;
            });

        public void SaveToFile(string fileName) => obj.SaveToFile(fileName);

        public Task SaveToFileAsync(string fileName) => obj.SaveToFileAsync(fileName);

        public void SaveToStream(Stream stream) => obj.SaveToStream(stream);

        public Task SaveToStreamAsync(Stream stream) => obj.SaveToStreamAsync(stream);
    }

    internal class FluidPDFRazorCachedCompiledTemplate(RazorEngineCompiledTemplate obj) : IFluidPDFRazorCompiledTemplate
    {
        public void EnableDebugging(string debuggingOutputDirectory = null) => obj.EnableDebugging(debuggingOutputDirectory);

        public string Run(object model = null, dynamic resx = null, bool encodeHtml = false) =>
            obj.Run(FluidPDFRazorRuntimeModel.EnrichModel(model, resx, encodeHtml));

        public Task<string> RunAsync(object model = null, dynamic resx = null, bool encodeHtml = false) =>
            obj.RunAsync(FluidPDFRazorRuntimeModel.EnrichModel(model, resx, encodeHtml));

        public void SaveToFile(string fileName) => obj.SaveToFile(fileName);

        public Task SaveToFileAsync(string fileName) => obj.SaveToFileAsync(fileName);

        public void SaveToStream(Stream stream) => obj.SaveToStream(stream);

        public Task SaveToStreamAsync(Stream stream) => obj.SaveToStreamAsync(stream);
    }
}
