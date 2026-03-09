using FluidPDF.Fluid;
using System.Data;

namespace FluidPDF.Tests.Mothers
{
    internal static class TemplateModelMother
    {
        internal static object SimpleObject() => new { Name = "Alice", Age = 30 };

        internal static string SimpleObjectTemplate() => "<p>{{ Model.Name }} is {{ Model.Age }}</p>";

        internal static string SimpleObjectExpectedOutput() => "<p>Alice is 30</p>";

        internal static string SimpleJsonString() => """{"Name":"Bob","Age":25}""";

        internal static string SimpleJsonTemplate() => "<p>{{ Model.Name }} is {{ Model.Age }}</p>";

        internal static string SimpleJsonExpectedOutput() => "<p>Bob is 25</p>";

        internal static Dictionary<string, object> SimpleDictionary() =>
            new()
            {
                { "Name", "Carol" },
                { "Age", 40 },
            };

        internal static string SimpleDictionaryTemplate() => "<p>{{ Model.Name }} is {{ Model.Age }}</p>";

        internal static string SimpleDictionaryExpectedOutput() => "<p>Carol is 40</p>";

        internal static FluidModel[] TwoModelArray()
        {
            FluidModel person = FluidModel.FromObject("Person", new { Name = "Dave" });
            FluidModel greeting = FluidModel.FromPlainValue("Greeting", "Hello");
            return [person, greeting];
        }

        internal static string TwoModelTemplate() => "<p>{{ Greeting }}, {{ Person.Name }}</p>";

        internal static string TwoModelExpectedOutput() => "<p>Hello, Dave</p>";

        internal static string HtmlSpecialCharsTemplate() => "<p>{{ Model.Value }}</p>";

        internal static object HtmlSpecialCharsObject() => new { Value = "<script>" };

        internal static string HtmlEncodedExpectedOutput() => "<p>&lt;script&gt;</p>";

        internal static string InvalidTemplate() => "{% if %}";

        internal static DataRow SimpleDataRow()
        {
            DataTable table = new();
            table.Columns.Add("Name", typeof(string));
            DataRow row = table.NewRow();
            row["Name"] = "Eve";
            return row;
        }
    }
}
