# FluidPDF

A .NET class library for generating PDFs from HTML templates. Write your report layout in HTML/CSS,
bind it to a data model, and get a PDF back — with full support for Liquid, Scriban, and Razor
template engines.

FluidPDF renders an HTML template against a data model using a template engine of your choice, then
prints the resulting HTML to a PDF through a headless Chromium browser. An
optional compression step can further reduce output file size.

**Capabilities**

- Use familiar web technologies (HTML + CSS) to design documents — no proprietary report DSL.
- Three template engine options out of the box: **Liquid** (default), **Scriban**, or **Razor**.
- Bring your own data: objects, JSON strings, `DataTable`, `DataRow`, or dictionaries are all
  supported as template models.
- A simple fluent builder lets you configure and generate a PDF in a single chain of calls.
- Full `netstandard2.0`, `net9.0`, and `net10.0` targeting.

**Dependencies**
- [PuppeteerSharp](https://github.com/hardkoded/puppeteer-sharp) for PDF rendering through the Chromium APIs
- [PDFsharp](https://github.com/empira/PDFsharp) for optional PDF compressing (reducing size)
- [Fluid](https://github.com/sebastienros/fluid) is the default templating engine
- [Scriban](https://github.com/scriban/scriban) is an optional templating engine available in the dedicated package _FluidPDF.Scriban_
- [RazorEngineCore](https://github.com/adoconnection/RazorEngineCore) is an optional templating engine available in the dedicated package _FluidPDF.Razor_

## Quick Start

Generate a PDF from a Liquid template and a plain C# object in four lines:

```csharp
using FluidPDF.Builder;

string template = "<html><body><h1>Hello, {{ Model.Name }}!</h1></body></html>";
var model = new { Name = "World" };

byte[] pdf =
  await FluidPDF
    .NewReport()
    .WithObjectModel(model)
    .WithTemplate(template)
    .BuildAsync();

await File.WriteAllBytesAsync("output.pdf", pdf);
```

## Usage

### Fluent Builder API

`FluidPDF.NewReport()` is the recommended entry point. Chain `With*()` methods to configure every
aspect of the PDF, then call `BuildAsync()`.

```csharp
using FluidPDF.Builder;

byte[] pdf =
  await FluidPDF
    .NewReport()
    .WithObjectModel(myModel)
    .WithTemplateFile("templates/invoice.html")   // load template from a file
    .WithA4Format()                               // default; also A2, A3, A5, A6
    .WithLandscapeOrientation()
    .WithInchMargin(0.5m)                         // uniform 0.5 in on all sides
    .WithScalePercentage(90)                      // 90 % zoom (10–200)
    .WithCulture("en-US")
    .WithPDFCompression()
    .BuildAsync();
```

Write directly to a stream instead of returning a byte array:

```csharp
await FluidPDF
  .NewReport()
  .WithObjectModel(myModel)
  .WithTemplate(templateString)
  .BuildAsync(outputStream);
```

Use an existing Chrome/Chromium executable to avoid the automatic download:

```csharp
await FluidPDF
  .NewReport()
  .WithObjectModel(myModel)
  .WithExternalChromeProcess(@"C:\Program Files\Google\Chrome\Application\chrome.exe")
  .WithTemplate(templateString)
  .BuildAsync();
```

#### Builder method reference

| Method | Description |
|---|---|
| `WithObjectModel(obj)` | C# object as the template model (name defaults to `"Model"`) |
| `WithObjectModel(obj, modelName)` | C# object with a custom model name |
| `WithJsonStringModel(json)` | JSON string as the template model |
| `WithJsonStringModel(json, modelName)` | JSON string with a custom model name |
| `WithDictionaryModel(dict)` | `IDictionary<string, object>` as the template model |
| `WithDictionaryModel(dict, modelName)` | Dictionary with a custom model name |
| `WithDataTableModel(table)` | `DataTable` as the template model |
| `WithDataTableModel(table, modelName)` | `DataTable` with a custom model name |
| `WithDataRowModel(row)` | `DataRow` as the template model |
| `WithDataRowModel(row, modelName)` | `DataRow` with a custom model name |
| `WithTemplate(string)` | Inline template string |
| `WithTemplateFile(string)` | Path to an HTML template file |
| `WithTemplateEngine(IFluidPDFTemplateEngine)` | Swap the default Liquid engine |
| `WithExternalChromeProcess(string)` | Path to an existing Chrome/Chromium executable |
| `WithLandscapeOrientation()` | Landscape page orientation |
| `WithA2Format()` / `WithA3Format()` / `WithA5Format()` / `WithA6Format()` | Paper size (default: A4) |
| `WithInchMargin(decimal)` | Uniform margin in inches |
| `WithInchMargin(bottom, left, right, top)` | Per-side margin in inches |
| `WithPixelMargin(decimal)` | Uniform margin in pixels |
| `WithPixelMargin(bottom, left, right, top)` | Per-side margin in pixels |
| `WithScalePercentage(int)` | Page scale 10–200 (default: 100) |
| `WithCulture(string)` | Culture code for number/date formatting |
| `WithPDFCompression()` | Re-encode the PDF via PDFsharp to reduce file size |
| `BuildAsync()` | Generate and return `byte[]` |
| `BuildAsync(Stream)` | Generate and write to a stream |


### Direct Factory API

For more control — or for dependency injection — instantiate `FluidPDFReportFactory` directly:

```csharp
using FluidPDF;
using FluidPDF.Fluid;
using FluidPDF.Support.PuppeteerSharp;

IFluidPDFTemplateEngine engine = new FluidTemplateEngine();

ChromiumRetrieverOptions chromiumOptions = new(
    ExternalExecutablePath: null,       // null = auto-download Chromium
    DownloadPath: "./chromium"          // local download directory
);

FluidPDFReportOptions pdfOptions = new()
{
    Format    = PaperFormat.A4,
    Landscape = false,
    Scale     = 1M
};

FluidPDFReportFactory factory = new(engine, chromiumOptions, pdfOptions);

FluidPDFTemplateModel model = FluidPDFTemplateModel.FromObject(myModel);
byte[] pdf = await factory.CompileReportAsync(templateString, model);
// or:
await factory.CompileReportAsync(templateString, model, destinationStream);
```

Both `CompileReportAsync` overloads accept an optional `toBeCompressed` flag and a `CultureInfo`.


### Template Engines

#### Liquid (default — `FluidPDF` package)

Uses the Fluid library, which implements the Liquid
template syntax.

```html
<html>
  <body>
    <h1>{{ Model.Title }}</h1>
    <ul>
      {% for item in Model.Items %}
        <li>{{ item.Name }} — {{ item.Price | currency }}</li>
      {% endfor %}
    </ul>
  </body>
</html>
```

#### Scriban (`FluidPDF.Scriban` package)

```csharp
using FluidPDF.Builder;
using FluidPDF.Scriban;

byte[] pdf =
await FluidPDF
  .NewReport()
  .WithObjectModel(myModel)
  .WithScribanTemplateEngine()
  .WithTemplate(scribanTemplate)
  .BuildAsync();
```

Scriban template example:

```html
<h1>{{ model.title }}</h1>
```

#### Razor (`FluidPDF.Razor` package)

```csharp
using FluidPDF.Builder;
using FluidPDF.Razor;

byte[] pdf =
await FluidPDF
  .NewReport()
  .WithObjectModel(myModel)
  .WithRazorTemplateEngine()
  .WithTemplate(razorTemplate)
  .BuildAsync();
```

Razor template example (model is passed as `dynamic`):

```html
<h1>@Model.Title</h1>
```


### Model Types

When using `FluidPDFReportFactory` directly, wrap your data in a `FluidPDFTemplateModel`.
The data argument comes first; `modelName` is optional and defaults to `"Model"`
(`FluidPDFTemplateModel.DefaultName`).

| Factory method | Source type |
|---|---|
| `FluidPDFTemplateModel.FromObject(obj)` | Any C# object, optional custom name |
| `FluidPDFTemplateModel.FromJsonString(json)` | JSON string with a custom name |
| `FluidPDFTemplateModel.FromDictionary(dict)` | `IDictionary<string, object>` |
| `FluidPDFTemplateModel.FromDataTable(table)` | `System.Data.DataTable` |
| `FluidPDFTemplateModel.FromDataRow(row)` | `System.Data.DataRow` |
| `FluidPDFTemplateModel.FromPlainValue(value)` | Primitive / scalar value |

Multiple models can be passed as an array to `RenderTemplateAsync(string, FluidPDFTemplateModel[], ...)`,
each accessible in the template by its assigned name.


### PDF Options

`FluidPDFReportOptions` (used by the factory) and the equivalent builder methods share the same
set of options:

| Option | Type | Default | Description |
|---|---|---|---|
| `Format` | `PaperFormat` | `A4` | Paper size |
| `Landscape` | `bool` | `false` | Landscape orientation |
| `MarginOptions` | `MarginOptions` | 0.4 in all sides | Page margins |
| `Scale` | `decimal` | `1M` | Page scale (0.1 – 2.0) |

## History
This library is born after my frustration with existing reporting tools, especially SSRS which was widely used in my company. I had recently discovered _PuppeteerSharp_ and an idea came to my mind - what if I can create PDF reports from HTML?<br/>
The core idea of this library has remained the same: make reports quick and easy.<br/>
FluidPDF v2.x and lower are the production versions before going open source, they are not released in this repo.