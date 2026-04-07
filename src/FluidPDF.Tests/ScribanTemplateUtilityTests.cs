using FluidPDF.Scriban;
using FluidPDF.Templating;

namespace FluidPDF.Tests
{
    public class ScribanTemplateUtilityTests : TemplateUtilityTests
    {
        protected override IFluidPDFTemplateEngine CreateEngine() => new ScribanTemplateEngine();

        // Scriban syntax: {{ v = <value>; v | <filterName> [arg1 [arg2]] }}
        // Named args work without the leading colon of Fluid, e.g. "format: 'F2'"
        protected override string AssignAndFilter(string assignValue, string filterName, params string[] filterArgs)
        {
            string argsStr = filterArgs.Length > 0 ? " " + string.Join(" ", filterArgs) : string.Empty;
            return $"{{{{ v = {assignValue}; v | {filterName}{argsStr} }}}}";
        }

        // {{ v = <value>; v | filter1 | filter2 }}
        protected override string AssignThenChainFilters(string assignValue, params string[] filters) =>
            $"{{{{ v = {assignValue}; v | {string.Join(" | ", filters)} }}}}";

        // Scriban function call: {{ function_call }}
        protected override string CallFunction(string functionCall) =>
            $"{{{{ {functionCall} }}}}";

        protected override string Literal(string content) => content;

        // {{ lines = path | file_read_all_lines ['file']; for l in lines; l.LineContent + ','; end }}
        protected override string ForEachLine(string source, string? fileArg, string lineContentAccess)
        {
            string filterCall = fileArg is not null
                ? $"{source} | file_read_all_lines {fileArg}"
                : $"{source} | file_read_all_lines";
            return $"{{{{- lines = {filterCall} -}}}}" +
                   $"{{{{ for l in lines -}}}}{{{{ l.{lineContentAccess} }}}},{{{{ end -}}}}";
        }

        protected override string GuidNew() => "guid 'new'";
        protected override string GuidEmpty() => "guid 'empty'";
        protected override string IntRandomCall(int min, int max) => $"int_random {min} {max}";
    }
}
