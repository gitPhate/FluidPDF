using FluidPDF.Fluid;
using FluidPDF.Templating;

namespace FluidPDF.Tests
{
    public class FluidTemplateUtilityTests : TemplateUtilityTests
    {
        protected override IFluidPDFTemplateEngine CreateEngine() => new FluidTemplateEngine();

        // Fluid (Liquid) syntax: {% assign v = <value> %}{{ v | <filterName> [: arg1 [: arg2]] }}
        protected override string AssignAndFilter(string assignValue, string filterName, params string[] filterArgs)
        {
            string argsStr = filterArgs.Length > 0 ? ": " + string.Join(", ", filterArgs) : string.Empty;
            return $"{{% assign v = {assignValue} %}}{{{{ v | {filterName}{argsStr} }}}}";
        }

        // {% assign v = <value> %}{{ v | filter1 | filter2 }}
        protected override string AssignThenChainFilters(string assignValue, params string[] filters) =>
            $"{{% assign v = {assignValue} %}}{{{{ v | {string.Join(" | ", filters)} }}}}";
        // Fluid empty/identifier/argument tags: {% tagname [args] %}
        protected override string CallFunction(string functionCall) =>
            $"{{% {functionCall} %}}";

        protected override string Literal(string content) => content;

        // {% assign lines = <path> | file_read_all_lines[: 'file'] %}
        // {% for l in lines %}{{ l.LineContent }},{% endfor %}
        protected override string ForEachLine(string source, string? fileArg, string lineContentAccess)
        {
            string filterCall = fileArg is not null
                ? $"{source} | file_read_all_lines: {fileArg}"
                : $"{source} | file_read_all_lines";
            return $"{{% assign lines = {filterCall} %}}{{% for l in lines %}}{{{{ l.{lineContentAccess} }}}},{{% endfor %}}";
        }

        protected override string GuidNew() => "guid new";
        protected override string GuidEmpty() => "guid empty";
        protected override string IntRandomCall(int min, int max) => $"int_random min_value: {min}, max_value: {max}";
    }
}
