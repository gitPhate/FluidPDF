namespace FluidPDF.Tests
{
    internal class TestConsts
    {
        internal const string TestTemplate = @"
<!doctype html>
<html lang=""en"">
<body>
    Model value: {{Model.Value}}
</body>
</html>
";
    }

#nullable disable

    public class Model
    {
        public string Value { get; set; }
    }
}
