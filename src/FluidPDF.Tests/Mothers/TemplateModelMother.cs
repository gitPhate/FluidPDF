using FluidPDF.Fluid;
using FluidPDF.Templating;
using System.Data;

namespace FluidPDF.Tests.Mothers
{
    internal static class TemplateModelMother
    {
        internal static object SimpleObject() => new { Name = "Alice", Age = 30 };

        internal static string SimpleTemplate() => "<p>{{ Model.Name }} is {{ Model.Age }}</p>";

        internal static string SimpleObjectExpectedOutput() => "<p>Alice is 30</p>";

        internal static Dictionary<string, object> SimpleDictionary() =>
            new()
            {
                { "Name", "Carol" },
                { "Age", 40 },
            };

        internal static string SimpleDictionaryExpectedOutput() => "<p>Carol is 40</p>";

        internal static FluidPDFTemplateModel[] TwoModelArray()
        {
            FluidPDFTemplateModel person = FluidPDFTemplateModel.FromObject("Person", new { Name = "Dave" });
            FluidPDFTemplateModel greeting = FluidPDFTemplateModel.FromPlainValue("Greeting", "Hello");
            return [person, greeting];
        }

        internal static string TwoModelTemplate() => "<p>{{ Greeting }}, {{ Person.Name }}</p>";

        internal static string TwoModelExpectedOutput() => "<p>Hello, Dave</p>";

        internal static string HtmlSpecialCharsTemplate() => "<p>{{ Model.Value }}</p>";

        internal static object HtmlSpecialCharsObject() => new { Value = "<script>" };

        internal static string HtmlEncodedExpectedOutput() => "<p>&lt;script&gt;</p>";

        internal static string InvalidTemplate() => "{% if %}";

        internal static DataTable SimpleDataTable()
        {
            DataTable table = new();
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Age", typeof(int));
            DataRow row = table.NewRow();
            row["Name"] = "Eve";
            row["Age"] = 31;
            return table;
        }

        internal static string SimpleDataTableExpectedOutput() => "<p>Eve is 31</p>";
    }
}
