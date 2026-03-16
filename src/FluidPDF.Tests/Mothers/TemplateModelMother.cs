using FluidPDF.Templating;
using System.Data;

namespace FluidPDF.Tests.Mothers
{
    internal static class TemplateModelMother
    {
        internal const string SimpleTemplate = "<p>{{ Model.Name }} is {{ Model.Age }}</p>";

        internal const string SimpleObjectExpectedOutput = "<p>Alice is 30</p>";

        internal const string SimpleDictionaryExpectedOutput = "<p>Carol is 40</p>";

        internal const string TwoModelTemplate = "<p>{{ Greeting }}, {{ Person.Name }}</p>";

        internal const string TwoModelExpectedOutput = "<p>Hello, Dave</p>";

        internal const string HtmlSpecialCharsTemplate = "<p>{{ Model.Value }}</p>";

        internal const string HtmlEncodedExpectedOutput = "<p>&lt;script&gt;</p>";

        internal const string InvalidTemplate = "{% if %}";

        internal static string SimpleDataTableTemplate() => "<ul>" + Environment.NewLine
+ "{% for item in Model.Rows %}" + Environment.NewLine
+ "<li>{{ item.Name }}</li>" + Environment.NewLine
+ "{% endfor %}"  + Environment.NewLine
+ "</ul>";

        internal static string SimpleDataTableExpectedOutput() => $"<ul>{Environment.NewLine}<li>Eve</li>{Environment.NewLine}<li>Sarah</li>{Environment.NewLine}</ul>";

        internal const string SimpleDataRowExpectedOutput = "<p>Frank is 45</p>";

        internal const string SimpleJsonStringExpectedOutput = "<p>Grace is 28</p>";

        // --- Scriban-specific fixtures (template syntax and expected output differ from Fluid) ---

        internal const string ScribanDataTableTemplate =
            """
<ul>
{{ for item in Model.Rows -}}
    <li>{{ item.Name }}</li>
{{ end -}}
</ul>
""";

        internal const string ScribanHtmlSpecialCharsExpectedOutput = "<p><script></p>";

        // --- Razor-specific fixtures (template syntax differs from Fluid/Scriban) ---

        internal const string RazorSimpleTemplate = "<p>@Model.Name is @Model.Age</p>";

        internal const string RazorTwoModelTemplate = "<p>@Model.Greeting, @Model.Person.Name</p>";

        internal const string RazorHtmlSpecialCharsTemplate = "<p>@Model.Value</p>";

        internal const string RazorHtmlSpecialCharsExpectedOutput = "<p><script></p>";

        internal static string RazorDataTableTemplate() =>
            "<ul>" + Environment.NewLine
            + "@foreach (var item in Model.Rows) {" + Environment.NewLine
            + "<li>@item.Name</li>" + Environment.NewLine
            + "}" + Environment.NewLine
            + "</ul>";

        // --- Object/collection factories (must remain methods — allocate new instances each call) ---

        internal const string SimpleJsonString = "{\"Name\":\"Alice\", \"Age\": 30}";

        internal static object SimpleObject() => new { Name = "Alice", Age = 30 };

        internal static Dictionary<string, object> SimpleDictionary() =>
            new()
            {
                { "Name", "Carol" },
                { "Age", 40 },
            };

        internal static FluidPDFTemplateModel[] TwoModelArray()
        {
            FluidPDFTemplateModel person = FluidPDFTemplateModel.FromObject("Person", new { Name = "Dave" });
            FluidPDFTemplateModel greeting = FluidPDFTemplateModel.FromPlainValue("Greeting", "Hello");
            return [person, greeting];
        }

        internal static object HtmlSpecialCharsObject() => new { Value = "<script>" };

        internal static DataTable SimpleDataTable()
        {
            DataTable table = new();
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Age", typeof(int));
            DataRow row = table.NewRow();
            row["Name"] = "Eve";
            row["Age"] = 31;
            table.Rows.Add(row);
            DataRow row2 = table.NewRow();
            row2["Name"] = "Sarah";
            row2["Age"] = 22;
            table.Rows.Add(row2);
            return table;
        }

        internal static DataRow SimpleDataRow()
        {
            DataTable table = new();
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Age", typeof(int));
            DataRow row = table.NewRow();
            row["Name"] = "Frank";
            row["Age"] = 45;
            table.Rows.Add(row);
            return row;
        }
    }
}
