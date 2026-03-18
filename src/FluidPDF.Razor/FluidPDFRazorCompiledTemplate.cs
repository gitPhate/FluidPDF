#nullable disable

using RazorEngineCore;
using System.IO;
using System.Threading.Tasks;

namespace FluidPDF.Razor
{
    internal interface IFluidPDFRazorCompiledTemplate
    {
        void EnableDebugging(string debuggingOutputDirectory = null);
        string Run(object model = null);
        Task<string> RunAsync(object model = null);
        void SaveToFile(string fileName);
        Task SaveToFileAsync(string fileName);
        void SaveToStream(Stream stream);
        Task SaveToStreamAsync(Stream stream);
    }

    internal class FluidPDFRazorCompiledTemplate(IRazorEngineCompiledTemplate obj) : IFluidPDFRazorCompiledTemplate
    {
        public void EnableDebugging(string debuggingOutputDirectory = null) => obj.EnableDebugging(debuggingOutputDirectory);

        public string Run(object model = null) => obj.Run(model);

        public Task<string> RunAsync(object model = null) => obj.RunAsync(model);

        public void SaveToFile(string fileName) => obj.SaveToFile(fileName);

        public Task SaveToFileAsync(string fileName) => obj.SaveToFileAsync(fileName);

        public void SaveToStream(Stream stream) => obj.SaveToStream(stream);

        public Task SaveToStreamAsync(Stream stream) => obj.SaveToStreamAsync(stream);
    }

    internal class FluidPDFRazorHTMLEncodedCompiledTemplate(IRazorEngineCompiledTemplate<HTMLEncodedTemplate> obj) : IFluidPDFRazorCompiledTemplate
    {
        public void EnableDebugging(string debuggingOutputDirectory = null) => obj.EnableDebugging(debuggingOutputDirectory);

        public string Run(object model = null) => obj.Run(t => t.Model = model);

        public Task<string> RunAsync(object model = null) => obj.RunAsync(t => t.Model = model);

        public void SaveToFile(string fileName) => obj.SaveToFile(fileName);

        public Task SaveToFileAsync(string fileName) => obj.SaveToFileAsync(fileName);

        public void SaveToStream(Stream stream) => obj.SaveToStream(stream);

        public Task SaveToStreamAsync(Stream stream) => obj.SaveToStreamAsync(stream);
    }

    internal class FluidPDFRazorCachedCompiledTemplate(RazorEngineCompiledTemplate obj) : IFluidPDFRazorCompiledTemplate
    {
        public void EnableDebugging(string debuggingOutputDirectory = null) => obj.EnableDebugging(debuggingOutputDirectory);

        public string Run(object model = null) => obj.Run(model);

        public Task<string> RunAsync(object model = null) => obj.RunAsync(model);

        public void SaveToFile(string fileName) => obj.SaveToFile(fileName);

        public Task SaveToFileAsync(string fileName) => obj.SaveToFileAsync(fileName);

        public void SaveToStream(Stream stream) => obj.SaveToStream(stream);

        public Task SaveToStreamAsync(Stream stream) => obj.SaveToStreamAsync(stream);
    }
}
