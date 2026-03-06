# FluidPDF – Agent Guidelines

## Project Overview

FluidPDF is a .NET class library (NuGet package) for PDF generation. It uses the Fluid templating
engine (Liquid syntax) to render HTML from a data model, then uses PuppeteerSharp (headless
Chromium) to print the HTML to a PDF. PDFsharp is used optionally for compression.

- **Solution:** `src/FluidPDF.sln`
- **Library:** `src/FluidPDF/` (targets `netstandard2.0;net9.0;net10.0`, C# 14 via PolySharp 1.15.0)
- **Tests:** no test project is currently in the solution

### Key Dependencies

| Project | Package | Version |
|---|---|---|
| FluidPDF | Fluid.Core | 2.31.0 |
| FluidPDF | PuppeteerSharp | 21.1.1 |
| FluidPDF | PDFsharp | 6.2.4 |
| FluidPDF | Microsoft.Bcl.AsyncInterfaces | 10.0.3 |
| FluidPDF | PolySharp *(analyzer only)* | 1.15.0 |

---

## Build Commands

Run all commands from the repository root.

```bash
# Restore packages
dotnet restore src/FluidPDF.sln

# Build (Debug)
dotnet build src/FluidPDF.sln

# Build (Release)
dotnet build src/FluidPDF.sln -c Release

# Pack as NuGet
dotnet pack src/FluidPDF/FluidPDF.csproj -c Release
```

---

## Test Commands

There is no test project in the solution at this time. When a test project is added it should
target `net8.0` or later and use xUnit + FluentAssertions (the prior convention). The commands
below will apply once a test project exists:

```bash
dotnet test src/FluidPDF.sln
dotnet test src/FluidPDF.sln --no-build
dotnet test src/FluidPDF.sln --filter "FullyQualifiedName=<namespace>.<class>.<method>"
dotnet test src/FluidPDF.sln --filter "ClassName=<namespace>.<class>"
dotnet test src/FluidPDF.sln --filter "Name=<method>"
```

---

## Lint / Format

There are no configured linters or formatters. The compiler enforces null safety via
`<Nullable>enable</Nullable>`. No `.editorconfig`, StyleCop, or Roslyn analyzer packages are
present in the library project.

---

## Architecture

The library exposes **two independent public APIs**:

### 1. `FluidPDFReportFactory` (direct factory)
Instantiated directly with options; renders a Liquid template and returns the PDF.

```csharp
FluidPDFReportFactory factory = new(chromiumRetrieverOptions, fluidPdfReportOptions);
byte[]  pdf    = await factory.CompileReportAsync(template, model);
// or write directly to a stream:
await factory.CompileReportAsync(template, model, destinationStream);
```

- `FluidPDFReportOptions` — configures paper format, landscape, margins, scale
- `ChromiumRetrieverOptions` — configures the Chromium executable path or standalone download
- `IChromiumRetriever` — interface for the browser launcher; `ChromiumRetriever` is the default
  implementation; can be replaced for testing

### 2. `FluidPDFBuilder` (fluent builder)
Static entry point returning `IFluidPDFBuilder`; configured via `With*()` chain; delegates to
`FluidPDFReportFactory` internally. Two terminal methods:

```csharp
byte[] pdf = await FluidPDFBuilder.NewWithModel(model)
    .WithStandaloneChromium()
    .WithTemplate(templateString)
    .BuildAsync();

await FluidPDFBuilder.NewWithModel(model)
    .WithStandaloneChromium()
    .WithTemplate(templateString)
    .BuildAsync(destinationStream);
```

### Supporting subsystems
- **`FluidTemplateHelper`** (`public static`) — renders Liquid templates; dispatches by model type
- **`FluidModel`** — discriminated-union sealed class; factory methods `FromDataRow`,
  `FromDictionary`, `FromJsonString`, `FromObject`, `FromPlainValue`
- **`PDFCompressHelper`** (`Support/PDF/`) — re-encodes a PDF via PDFsharp to compress it
- **`ChromiumRetriever`** (`Support/PuppeteerSharp/`) — downloads or locates Chromium, launches
  a headless browser; implements `IChromiumRetriever`
- **`AsyncFile`** (`Support/IO/`) — async text file reader

---

## Directory Structure

```
src/FluidPDF/
├── Builder/                  FluidPDFBuilder.cs, IFluidPDFBuilder.cs
├── Exceptions/               FluidPDFBuilderConfigException.cs
├── Fluid/                    FluidModel.cs, FluidTemplateHelper.cs,
│                             FluidTemplateOptions.cs, FluidRenderException.cs
├── Support/
│   ├── IO/                   AsyncFile.cs
│   ├── PDF/                  PDFCompressHelper.cs
│   └── PuppeteerSharp/       ChromiumRetriever.cs (+ IChromiumRetriever, ChromiumRetrieverOptions)
├── FluidPDFReportFactory.cs  (main public factory + FluidPDFReportOptions)
└── FluidPDF.csproj
```

---

## Code Style Guidelines

### Indentation and Formatting

- **Indent:** 4 spaces (no tabs)
- **Braces:** Allman style — opening brace on its own line for classes, methods, and control flow
- **Expression-bodied members:** use liberally for single-expression methods, properties, and
  constructors:
  ```csharp
  public bool IsObject => IsFluidModelType(FluidModelType.Object);
  private bool IsFluidModelType(FluidModelType value) => Type == value;
  ```
- **Object initializers:** single-line when short; multi-line with trailing comma when longer:
  ```csharp
  new MarginOptions { Bottom = "0.4 in", Left = "0.4 in", Right = "0.4 in", Top = "0.4 in" }

  new PdfOptions
  {
      Format = fluidPdfReportOptions.Format,
      Landscape = fluidPdfReportOptions.Landscape,
      MarginOptions = fluidPdfReportOptions.MarginOptions,
  };
  ```
- **Blank lines:** one blank line between methods; no blank line between namespace declaration and
  class declaration
- **Line endings:** CRLF

### Imports (`using` directives)

- All `using` directives go at the top of the file, outside the namespace
- Namespaces are braced (not file-scoped)
- No strict ordering is enforced; the convention groups project/third-party namespaces before
  `System.*` namespaces
- No `using static`; no `#region` blocks

### Naming Conventions

| Element | Convention | Examples |
|---|---|---|
| Classes, interfaces, enums, records | `PascalCase` | `FluidPDFBuilder`, `IChromiumRetriever` |
| Abbreviations in names | All-caps | `PDF`, `HTML`, `IO` — e.g. `PDFCompressHelper` |
| Private / protected fields | `_camelCase` | `_landscape`, `_chromiumRetriever` |
| Parameters and local variables | `camelCase` | `modelName`, `cultureInfo` |
| Properties and methods | `PascalCase` | `CompileReportAsync()`, `RenderedContent` |
| Async methods | Suffix `Async` | `CompileReportAsync()`, `RetrieveBrowserInstanceAsync()` |
| Static factory methods | `NewXxx()` or `FromXxx()` | `NewFluidPDFReportFactory()`, `FromObject()` |
| Interface names | `I` prefix | `IFluidPDFBuilder`, `IChromiumRetriever` |
| Files | Match class name exactly | `FluidPDFBuilder.cs`, `ChromiumRetriever.cs` |
| Directories | `PascalCase` | `Builder/`, `Support/IO/`, `Support/PDF/` |
| Enum values | `PascalCase` | `ZeroPoint5`, `DataRow`, `JsonString` |

### Types

- **Nullable reference types** are enabled (`<Nullable>enable</Nullable>`) — honour all warnings
- Prefer **explicit types** over `var` in library code
- Use **`ValueTask<T>`** for high-frequency / interface-level async paths (e.g. `FluidTemplateHelper`
  render methods); use `Task<T>` for lower-frequency factory and builder methods
- Use **`decimal`** for scale/ratio values (not `double` or `float`)
- Apply **`sealed`** to all concrete implementation classes (e.g. `ChromiumRetriever`)
- Mark stateless helper/utility classes as **`static`** (e.g. `FluidTemplateHelper`,
  `PDFCompressHelper`, `AsyncFile`)
- Apply **`where T : notnull`** generic constraint where nullability must be excluded
- Prefer **primary constructors** (C# 12, backported via PolySharp) for simple classes and records:
  ```csharp
  internal sealed class ChromiumRetriever(ChromiumRetrieverOptions options) : IChromiumRetriever { }
  public class FluidPDFBuilderConfigException(string message) : Exception(message) { }
  ```
- Prefer **collection expressions** (`[...]`) over `new List<T>()` or array initializers:
  ```csharp
  FluidModel[] models = [model1, model2];
  ```
- Use **switch expressions** for exhaustive enum/type dispatch
- Use `#if NETSTANDARD2_0` / `#else` blocks when a newer API is preferred on modern TFMs:
  ```csharp
  #if NETSTANDARD2_0
      await stream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
  #else
      await stream.WriteAsync(data).ConfigureAwait(false);
  #endif
  ```

### Async and ConfigureAwait

- **Always call `.ConfigureAwait(false)`** on every `await` in library code:
  ```csharp
  byte[] data = await page.PdfDataAsync(_pdfOptions).ConfigureAwait(false);
  ```
- Use `try/finally` blocks to guarantee `IPage.CloseAsync()` / `IBrowser.CloseAsync()` are called
  even when an exception is thrown mid-method

### Error Handling

- Throw **domain exceptions** (`FluidRenderException`, `FluidPDFBuilderConfigException`) for
  library-level errors; wrap low-level exceptions as `innerException`
- Custom exceptions: use a primary constructor for simple message-only exceptions; provide all
  four standard constructors (default, message, message + inner, serialization) when the exception
  is serializable
- Use the internal extension guard `GetNonNullOrThrow<T>()` for null argument validation:
  ```csharp
  _chromiumRetriever = chromiumRetriever ?? throw new ArgumentNullException(nameof(chromiumRetriever));
  // or via extension:
  _options = options.GetNonNullOrThrow(nameof(options));
  ```
- Use `File.Exists()` before accepting file paths; throw `FileNotFoundException` with the path
- Validate configuration before use; compute boolean guards into named local variables first

### What to Avoid

- Do not use `var` in library code
- Do not use file-scoped namespaces — keep braced `namespace` blocks
- Do not use `using static`
- Do not remove `.ConfigureAwait(false)` from `await` calls in library code
- Do not use `float` or `double` for margin, scale, or ratio values — use `decimal`
- Do not add `#region` blocks
- Do not use `NotImplementedException` as a switch catch-all for invalid enum values — use
  `ArgumentOutOfRangeException` instead
