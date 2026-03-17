# FluidPDF

A .NET class library for generating PDFs from HTML templates. Write your report layout in HTML/CSS,
bind it to a data model, and get a PDF back — with full support for Liquid, Scriban, and Razor
template engines.

---

## Table of Contents

- [Description](#description)
- [Packages](#packages)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Usage](#usage)
  - [Fluent Builder API](#fluent-builder-api)
  - [Direct Factory API](#direct-factory-api)
  - [Template Engines](#template-engines)
  - [Model Types](#model-types)
  - [PDF Options](#pdf-options)
- [Running the Tests](#running-the-tests)
- [License](#license)

---

## Description

FluidPDF renders an HTML template against a data model using a template engine of your choice, then
prints the resulting HTML to a PDF through a headless Chromium browser (PuppeteerSharp). An
optional compression step via PDFsharp can further reduce output file size.

**Why FluidPDF?**

- Use familiar web technologies (HTML + CSS) to design documents — no proprietary report DSL.
- Three template engine options out of the box: **Liquid** (default), **Scriban**, or **Razor**.
- Bring your own data: objects, JSON strings, `DataTable`, `DataRow`, or dictionaries are all
  supported as template models.
- A simple fluent builder lets you configure and generate a PDF in a single chain of calls.
- Full `netstandard2.0`, `net9.0`, and `net10.0` targeting.

---

## Packages

| Package | NuGet | Description |
|---|---|---|
| `FluidPDF` | [![NuGet](https://img.shields.io/nuget/v/FluidPDF)](https://www.nuget.org/packages/FluidPDF) | Core library — Liquid template engine + PDF generation |
| `FluidPDF.Scriban` | [![NuGet](https://img.shields.io/nuget/v/FluidPDF.Scriban)](https://www.nuget.org/packages/FluidPDF.Scriban) | Scriban template engine adapter |
| `FluidPDF.Razor` | [![NuGet](https://img.shields.io/nuget/v/FluidPDF.Razor)](https://www.nuget.org/packages/FluidPDF.Razor) | Razor (RazorEngineCore) template engine adapter |

---

## Installation

Install the core package (includes the Liquid engine):

```bash
dotnet add package FluidPDF
```

Optionally add an adapter for a different template engine:

```bash
dotnet add package FluidPDF.Scriban
dotnet add package FluidPDF.Razor
```

> **Chromium:** On first use without an external Chrome executable, PuppeteerSharp downloads a
> compatible Chromium build automatically. You can also point FluidPDF to an existing Chrome/
> Chromium installation via `WithExternalChromeProcess()` to skip the download.

---

## Quick Start

Generate a PDF from a Liquid template and a plain C# object in four lines:

```csharp
using FluidPDF.Builder;

string template = "<html><body><h1>Hello, {{ Model.Name }}!</h1></body></html>";
var model = new { Name = "World" };

byte[] pdf = await FluidPDFBuilder
    .NewWithModel(model)
    .WithTemplate(template)
    .BuildAsync();

await File.WriteAllBytesAsync("output.pdf", pdf);
```

---

## Usage

### Fluent Builder API

`FluidPDFBuilder` is the recommended entry point. Chain `With*()` methods to configure every
aspect of the PDF, then call `BuildAsync()`.

```csharp
using FluidPDF.Builder;

byte[] pdf = await FluidPDFBuilder
    .NewWithModel(myModel)
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
await FluidPDFBuilder
    .NewWithModel(myModel)
    .WithTemplate(templateString)
    .BuildAsync(outputStream);
```

Use an existing Chrome/Chromium executable to avoid the automatic download:

```csharp
await FluidPDFBuilder
    .NewWithModel(myModel)
    .WithExternalChromeProcess(@"C:\Program Files\Google\Chrome\Application\chrome.exe")
    .WithTemplate(templateString)
    .BuildAsync();
```

#### Builder method reference

| Method | Description |
|---|---|
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

---

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

byte[] pdf = await factory.CompileReportAsync(templateString, myModel);
// or:
await factory.CompileReportAsync(templateString, myModel, destinationStream);
```

Both `CompileReportAsync` overloads accept an optional `toBeCompressed` flag and a `CultureInfo`.

---

### Template Engines

#### Liquid (default — `FluidPDF` package)

Uses the [Fluid](https://github.com/sebastienros/fluid) library, which implements the Liquid
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

The default model variable name is `Model`. Pass a custom name through
`FluidPDFTemplateRenderOptions.ModelName` when using the factory directly, or when injecting a
template engine via `WithTemplateEngine()`.

#### Scriban (`FluidPDF.Scriban` package)

```csharp
using FluidPDF.Builder;
using FluidPDF.Scriban;

byte[] pdf = await FluidPDFBuilder
    .NewWithModel(myModel)
    .WithTemplateEngine(new ScribanTemplateEngine())
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

byte[] pdf = await FluidPDFBuilder
    .NewWithModel(myModel)
    .WithTemplateEngine(new RazorTemplateEngine())
    .WithTemplate(razorTemplate)
    .BuildAsync();
```

Razor template example (model is passed as `dynamic`):

```html
<h1>@Model.Title</h1>
```

---

### Model Types

When using `FluidPDFReportFactory` directly, wrap your data in a `FluidPDFTemplateModel`:

| Factory method | Source type |
|---|---|
| `FluidPDFTemplateModel.FromObject("Model", obj)` | Any C# object |
| `FluidPDFTemplateModel.FromJsonString("Model", json)` | JSON string |
| `FluidPDFTemplateModel.FromDictionary("Model", dict)` | `IDictionary<string, object>` |
| `FluidPDFTemplateModel.FromDataTable("Model", table)` | `System.Data.DataTable` |
| `FluidPDFTemplateModel.FromDataRow("Model", row)` | `System.Data.DataRow` |
| `FluidPDFTemplateModel.FromPlainValue("Model", value)` | Primitive / scalar value |

Multiple models can be passed as an array to `RenderTemplateAsync(string, FluidPDFTemplateModel[], ...)`,
each accessible in the template by its assigned name.

---

### PDF Options

`FluidPDFReportOptions` (used by the factory) and the equivalent builder methods share the same
set of options:

| Option | Type | Default | Description |
|---|---|---|---|
| `Format` | `PaperFormat` | `A4` | Paper size |
| `Landscape` | `bool` | `false` | Landscape orientation |
| `MarginOptions` | `MarginOptions` | 0.4 in all sides | Page margins |
| `Scale` | `decimal` | `1M` | Page scale (0.1 – 2.0) |

---

## Running the Tests

```bash
# Run all tests
dotnet test src/FluidPDF.sln

# Run a specific test class
dotnet test src/FluidPDF.sln --filter "ClassName=FluidPDF.Tests.FluidTemplateEngineTests"

# Run a single test by name
dotnet test src/FluidPDF.sln --filter "Name=RenderWithObject_ReturnsRenderedTemplate"
```

The test project targets `net8.0` and uses xUnit v3, FluentAssertions, and NSubstitute.
Integration tests that print a real PDF are guarded by a `ChromiumRetrieverMock` so no Chromium
download is required to run the suite.

---

## License

Distributed under the [MIT License](LICENSE). See `LICENSE` for full details.
