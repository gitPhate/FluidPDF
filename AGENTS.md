# FluidPDF – Agent Guidelines

## Project Overview

FluidPDF is a .NET class library (NuGet package) for PDF generation. It uses the Fluid templating
engine (Liquid syntax) to render HTML from a data model, then uses PuppeteerSharp (headless
Chromium) to print the HTML to a PDF. PDFsharp is used optionally for compression. Two optional
adapter packages add Scriban and Razor template engine support.

- **Solution:** `src/FluidPDF.sln`
- **Library:** `src/FluidPDF/` (targets `netstandard2.0;net9.0;net10.0`, C# 14 via PolySharp 1.15.0)
- **Scriban adapter:** `src/FluidPDF.Scriban/` (targets `netstandard2.0` only)
- **Razor adapter:** `src/FluidPDF.Razor/` (targets `netstandard2.0;net9.0;net10.0`)
- **Tests:** `src/FluidPDF.Tests/` (targets `net8.0`, xUnit v3 + FluentAssertions + NSubstitute)

### Key Dependencies

| Project | Package | Version |
|---|---|---|
| FluidPDF | Fluid.Core | 2.31.0 |
| FluidPDF | PuppeteerSharp | 21.1.1 |
| FluidPDF | PDFsharp | 6.2.4 |
| FluidPDF | Microsoft.Bcl.AsyncInterfaces | 10.0.3 |
| FluidPDF | PolySharp *(analyzer only)* | 1.15.0 |
| FluidPDF.Scriban | Scriban | 6.5.5 |
| FluidPDF.Razor | RazorEngineCore | 2026.1.1 |
| FluidPDF.Tests | xunit.v3 | 3.2.2 |
| FluidPDF.Tests | FluentAssertions | 8.8.0 |
| FluidPDF.Tests | NSubstitute | 5.3.0 |

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
dotnet pack src/FluidPDF.Scriban/FluidPDF.Scriban.csproj -c Release
dotnet pack src/FluidPDF.Razor/FluidPDF.Razor.csproj -c Release
```

---

## Test Commands

```bash
# Run all tests
dotnet test src/FluidPDF.sln

# Run without rebuilding
dotnet test src/FluidPDF.sln --no-build

# Run a single test by fully-qualified name
dotnet test src/FluidPDF.sln --filter "FullyQualifiedName=FluidPDF.Tests.FluidTemplateEngineTests.RenderWithObject_ReturnsRenderedTemplate"

# Run all tests in a class
dotnet test src/FluidPDF.sln --filter "ClassName=FluidPDF.Tests.FluidTemplateEngineTests"

# Run a single test by method name (matches across all classes)
dotnet test src/FluidPDF.sln --filter "Name=RenderWithObject_ReturnsRenderedTemplate"
```

Test classes: `FluidPDFBuilderTests`, `FluidPDFReportFactoryTests`, `FluidPDFTemplateModelTests`,
`FluidTemplateEngineTests`, `ScribanTemplateEngineTests`, `RazorTemplateEngineTests`,
`InternalExtensionMethodsTests`, `ExpandoObjectConverterTests`.

---

## Lint / Format

There are no configured linters or formatters. The compiler enforces null safety via
`<Nullable>enable</Nullable>`. No `.editorconfig`, StyleCop, or Roslyn analyzer packages are
present in the library project.

---

## Architecture

The library exposes **two independent public APIs**:

### 1. `FluidPDFReportFactory` (direct factory)
Instantiated with `IFluidPDFTemplateEngine`, `ChromiumRetrieverOptions`, and `FluidPDFReportOptions`.

```csharp
FluidPDFReportFactory factory = new(templateEngine, chromiumRetrieverOptions, fluidPdfReportOptions);
byte[] pdf = await factory.CompileReportAsync(template, model);
// or write directly to a stream:
await factory.CompileReportAsync(template, model, destinationStream);
```

- `FluidPDFReportOptions` — configures paper format, landscape, margins, scale
- `ChromiumRetrieverOptions` — configures the Chromium executable path or standalone download
- `IChromiumRetriever` — interface for the browser launcher; `ChromiumRetriever` is the default
  implementation; can be replaced for testing

### 2. `FluidPDFBuilder` (fluent builder)
Static entry point; configured via `With*()` chain; delegates to `FluidPDFReportFactory` internally.

```csharp
byte[] pdf = await FluidPDFBuilder.NewWithModel(model)
    .WithStandaloneChromium()
    .WithTemplate(templateString)
    .BuildAsync();
```

### Supporting subsystems
- **`FluidTemplateEngine`** — renders Liquid templates via Fluid; implements `IFluidPDFTemplateEngine`
- **`ScribanTemplateEngine`** (`FluidPDF.Scriban`) — renders Scriban templates; implements same interface
- **`RazorTemplateEngine`** (`FluidPDF.Razor`) — renders Razor templates; implements same interface
- **`FluidPDFTemplateModel`** — discriminated-union sealed class; factory methods `FromDataRow`,
  `FromDataTable`, `FromDictionary`, `FromJsonString`, `FromObject`, `FromPlainValue`
- **`IFluidPDFTemplateEngine`** — interface with `RenderTemplateAsync` overloads returning
  `ValueTask<string>`; accepts `(string template, FluidPDFTemplateModel model, FluidPDFTemplateRenderOptions?)`
- **`PDFCompressHelper`** (`Support/PDF/`) — re-encodes a PDF via PDFsharp to compress it
- **`ChromiumRetriever`** (`Support/PuppeteerSharp/`) — downloads or locates Chromium, launches
  a headless browser; implements `IChromiumRetriever`
- **`AsyncFile`** (`Support/IO/`) — async text file reader
- **`InternalExtensionMethods`** (`Support/`) — string guards (`IsNullOrBlankString`,
  `IsNotNullAndNotBlank`, `ToNullIfBlank`) and null guards (`GetNonNullOrThrow<T>`)
- **`ExpandoObjectConverter`** (`Support/Json/`) — converts various model types to `ExpandoObject`
  for template engine consumption

---

## Directory Structure

```
src/
├── FluidPDF/
│   ├── Builder/              FluidPDFBuilder.cs, IFluidPDFBuilder.cs
│   ├── Exceptions/           FluidPDFBuilderConfigException.cs
│   ├── Fluid/                FluidTemplateEngine.cs
│   ├── Support/
│   │   ├── InternalExtensionMethods.cs
│   │   ├── IO/               AsyncFile.cs
│   │   ├── Json/             ExpandoObjectConverter.cs
│   │   ├── PDF/              PDFCompressHelper.cs
│   │   └── PuppeteerSharp/   ChromiumRetriever.cs (+ IChromiumRetriever, ChromiumRetrieverOptions)
│   ├── Templating/           FluidPDFTemplateModel.cs, FluidPDFTemplateRenderException.cs,
│   │                         IFluidPDFTemplateRenderOptions.cs (contains IFluidPDFTemplateEngine
│   │                         and FluidPDFTemplateRenderOptions)
│   └── FluidPDFReportFactory.cs  (main public factory + FluidPDFReportOptions)
├── FluidPDF.Scriban/
│   └── ScribanTemplateEngine.cs
├── FluidPDF.Razor/
│   ├── RazorTemplateEngine.cs
│   └── FluidPDFBuilderRazorExtensions.cs
└── FluidPDF.Tests/
    ├── Mocks/                ChromiumRetrieverMock.cs
    └── Mothers/              PDFDocumentMother.cs, TemplateModelMother.cs
```

---

## Code Style Guidelines

### Indentation and Formatting

- **Indent:** 4 spaces (no tabs)
- **Braces:** Allman style — opening brace on its own line for classes, methods, and control flow
- **Expression-bodied members:** use liberally for single-expression methods and properties:
  ```csharp
  public bool IsObject => Type == FluidPDFTemplateModelType.Object;
  private bool IsFluidModelType(FluidPDFTemplateModelType value) => Type == value;
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
- **Blank lines:** one blank line between methods; no blank line between namespace and class
- **Line endings:** CRLF

### Imports (`using` directives)

- All `using` directives go at the top of the file, outside the namespace
- Namespaces are braced (not file-scoped)
- Convention groups project/third-party namespaces before `System.*` namespaces
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
| Static factory methods | `NewXxx()` or `FromXxx()` | `NewWithModel()`, `FromObject()` |
| Interface names | `I` prefix | `IFluidPDFBuilder`, `IChromiumRetriever` |
| Files | Match primary class name exactly | `FluidPDFBuilder.cs`, `ChromiumRetriever.cs` |
| Directories | `PascalCase` | `Builder/`, `Support/IO/`, `Support/PDF/` |
| Enum values | `PascalCase` | `ZeroPoint5`, `DataRow`, `JsonString` |
| Test classes | `<Subject>Tests` | `FluidTemplateEngineTests` |
| Test methods | `<Action>_<ExpectedResult>` or `<Action>_Should<What>_When<Condition>` | `RenderWithObject_ReturnsRenderedTemplate`, `BuildAsync_ShouldThrowFluidPDFBuilderConfigException_WhenNoTemplateIsSet` |

### Types

- **Nullable reference types** are enabled (`<Nullable>enable</Nullable>`) — honour all warnings
- Prefer **explicit types** over `var` in library code (tests may use implicit types)
- Use **`ValueTask<string>`** for `IFluidPDFTemplateEngine` render methods (high-frequency interface
  paths); use `Task<T>` for factory and builder terminal methods
- Use **`decimal`** for scale/ratio values (not `double` or `float`)
- Apply **`sealed`** to all concrete implementation classes (e.g. `ChromiumRetriever`,
  `FluidPDFTemplateModel`)
- Mark stateless helper/utility classes as **`static`** (e.g. `PDFCompressHelper`, `AsyncFile`,
  `InternalExtensionMethods`)
- Apply **`where T : notnull`** generic constraint where nullability must be excluded
- Prefer **primary constructors** (C# 12+) for simple classes and records:
  ```csharp
  internal sealed class ChromiumRetriever(ChromiumRetrieverOptions options) : IChromiumRetriever { }
  public class FluidPDFBuilderConfigException(string message) : Exception(message) { }
  ```
- Prefer **collection expressions** (`[...]`) over `new List<T>()` or array initializers
- Use **switch expressions** for exhaustive enum/type dispatch; use `ArgumentOutOfRangeException`
  as the default arm (never `NotImplementedException`)
- Use `#if NETSTANDARD2_0` / `#else` for TFM-conditional APIs:
  ```csharp
  #if NETSTANDARD2_0
      await stream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
  #else
      await stream.WriteAsync(data).ConfigureAwait(false);
  #endif
  ```

### Async and ConfigureAwait

- **Always call `.ConfigureAwait(false)`** on every `await` in library code
- Use `try/finally` blocks to guarantee `IPage.CloseAsync()` / `IBrowser.CloseAsync()` are called
  even when an exception is thrown mid-method

### Error Handling

- Throw **domain exceptions** (`FluidPDFTemplateRenderException`, `FluidPDFBuilderConfigException`)
  for library-level errors; wrap low-level exceptions as `innerException`
- Custom exceptions: use a primary constructor for simple message-only exceptions
- Use the internal extension guard `GetNonNullOrThrow<T>()` for null argument validation;
  two overloads exist — one taking a `string paramName`, one taking a `Func<Exception>` factory:
  ```csharp
  _options = options.GetNonNullOrThrow(nameof(options));
  _value = value.GetNonNullOrThrow(() => new InvalidOperationException("..."));
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
- Do not use `NotImplementedException` as a switch catch-all — use `ArgumentOutOfRangeException`
