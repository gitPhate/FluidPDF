# FluidPDF

A .NET class library for generating PDFs from HTML templates. Write your report layout in HTML/CSS,
bind it to a data model, and get a PDF back.

FluidPDF renders an HTML template against a data model using a template engine of your choice, then
prints the resulting HTML to a PDF through a headless Chromium browser. An
optional compression step can further reduce output file size.

## Features

- Use familiar web technologies (HTML + CSS) to design reports
- Three template engine options out of the box: **Fluid** (default), **Scriban**, or **Razor**.
- Bring your own data: objects, JSON strings, `DataTable`, `DataRow`, or dictionaries are all
  supported as template models.
- A simple fluent builder lets you configure and generate a PDF in a single chain of calls.
- Full `netstandard2.0`, `net9.0`, and `net10.0` targeting.

## Quick Start

You can generate a PDF from a Liquid template and a plain C# object:

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

FluidPDFReportFactory factory = new(engine, chromiumOptions);

FluidPDFReportOptions reportOptions = new()
{
    Format           = PaperFormat.A4,
    Landscape        = false,
    Scale            = 1M,
    ToBeCompressed   = false,
    CultureInfo      = null,
    EncodeHtml       = false
};

FluidPDFTemplateModel model = FluidPDFTemplateModel.FromObject(myModel);
byte[] pdf = await factory.CompileReportAsync(templateString, model, reportOptions);
// or:
await factory.CompileReportAsync(templateString, model, destinationStream, reportOptions);
```

You can use multiple models by creating an array and passing them to `CompileReportAsync`:

```csharp
FluidPDFTemplateModel[] models = 
[
    FluidPDFTemplateModel.FromObject(myModel),
    FluidPDFTemplateModel.FromDictionary(myDict, "CustomName")
];

byte[] pdf = await factory.CompileReportAsync(templateString, models, reportOptions);
```

All options are part of the `FluidPDFReportOptions` class, including compression, culture, and HTML encoding.


### Template Engines

There are 3 template engines, with Fluid being the default one.

#### Fluid

This is the default template engine, it is the most simple and light to use.
It uses the [Fluid](https://github.com/sebastienros/fluid) library,
which implements the Liquid template syntax.

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

#### Scriban

Available with the separate package `FluidPDF.Scriban`, it uses the [Scriban](https://github.com/scriban/scriban) package to render templates. Scriban is cool to handle little more advanced templates.

Note: the default convention to access properties of the model has been changed from the Scriban default, to have it similar to C# syntax and the other template engines.

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
<html>
  <body>
    <h1>{{ Model.Title }}</h1>
    <ul>
      {{ for item in Model.Items }}
        <li>{{ item.Name }} — {{ item.Price | math.format "C" }}</li>
      {{ end }}
    </ul>
  </body>
</html>
```

#### Razor

Available with the separate package `FluidPDF.Razor`, it uses the library [RazorEngineCore](https://github.com/adoconnection/RazorEngineCore) to render templates using _Razor_. This is the most advanced template engine, capable of handling complex templates.

Note: The _RazorEngineCore_ library directly compiles templates using the Razor compiler and Roslyn, so a cache is required to keep the compiled templates; at the moment only the filesystem is supported.

```csharp
using FluidPDF.Builder;
using FluidPDF.Razor;

byte[] pdf =
await FluidPDF
  .NewReport()
  .WithObjectModel(myModel)
  .WithRazorTemplateEngine(new RazorCompiledTemplateCacheOptions(@"C:\Temp"))
  .WithTemplate(razorTemplate)
  .BuildAsync();
```

Razor template example (model is passed as `dynamic`):

```html
<html>
  <body>
    <h1>@Model.Title</h1>
    <ul>
      @foreach (var item in Model.Items)
      {
        <li>@item.Name — @item.Price.ToString("C")</li>
      }
    </ul>
  </body>
</html>
```


## Model Types
Models are the data available inside the template. They can be passed in different formats:
- DataTable: a generic data table, useful for dynamic data possibly returned from a query
- DataRow:  a generic data row, added because required to make the `DataTable` model type work
- Dictionary: dictionary of values (untyped object values) and string keys
- JsonString: a json representing the model object
- Object: a custom object that will be available as the model object
- PlainValue: a raw value which is directly bound to the model object

When using `FluidPDFReportFactory` directly, wrap your data in a `FluidPDFTemplateModel`.
Multiple models can be passed as an array to `RenderTemplateAsync(string, FluidPDFTemplateModel[], ...)`,
each accessible in the template by its assigned name.


## History
This library is born after my frustration with existing reporting tools, especially SSRS which was widely used in my company. I had recently discovered _PuppeteerSharp_ and an idea came to my mind - what if I can create PDF reports from HTML?<br/>
The core idea of this library has remained the same: make reports quick and easy.<br/>
FluidPDF v2.x and lower are the production versions before going open source, they are not released in this repo.

## API Reference

### Builder method reference

All model methods have a `modelName` property with default value `Model` to change the defaul model object name. The Razor template engine does not support this feature.

| Method | Description |
|---|---|
| `WithDataRowModel(row, modelName)` | `DataRow` as the template model |
| `WithDataTableModel(table, modelName)` | `DataTable` as the template model |
| `WithDictionaryModel(dict, modelName)` | `IDictionary<string, object>` as the template model |
| `WithJsonStringModel(json), modelName)` | JSON string as the template model |
| `WithObjectModel(obj, modelName)` | C# object as the template model |
| `WithtModel(obj, modelName)` | `FluidPDFTemplateModel` as the template model |
| `WithtModels(objs, modelName)` | `FluidPDFTemplateModel` array as template models (multiple models) |
| `WithTemplate(string)` | Inline template string |
| `WithTemplateFile(string)` | Path to an HTML template file |
| `WithTemplateEngine(IFluidPDFTemplateEngine)` | Swap the default Fluid engine |
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
| `WithHtmlEncode()` | Encode HTML tags |
| `BuildAsync()` | Generate and return `byte[]` |
| `BuildAsync(Stream)` | Generate and write to a stream |

### Model creation reference

| Factory method | Source type |
|---|---|
| `FluidPDFTemplateModel.FromObject(obj)` | Any C# object, optional custom name |
| `FluidPDFTemplateModel.FromJsonString(json)` | JSON string with a custom name |
| `FluidPDFTemplateModel.FromDictionary(dict)` | `IDictionary<string, object>` |
| `FluidPDFTemplateModel.FromDataTable(table)` | `System.Data.DataTable` |
| `FluidPDFTemplateModel.FromDataRow(row)` | `System.Data.DataRow` |
| `FluidPDFTemplateModel.FromPlainValue(value)` | Primitive / scalar value |