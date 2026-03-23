# FluidPDF – Agent Guidelines

## Project Overview

FluidPDF is a .NET class library (NuGet package) for PDF generation. It uses the Fluid templating
engine (Liquid syntax) to render HTML from a data model, then uses PuppeteerSharp (headless
Chromium) to print the HTML to a PDF. PDFsharp is used optionally for compression. Two optional
adapter packages add Scriban and Razor template engine support.

- **Solution:** `src/FluidPDF.sln`
- **Library:** `src/FluidPDF/` — targets `netstandard2.0;net9.0;net10.0`, C# 14 via PolySharp
- **Scriban adapter:** `src/FluidPDF.Scriban/` — targets `netstandard2.0;net9.0;net10.0`
- **Razor adapter:** `src/FluidPDF.Razor/` — targets `netstandard2.0;net9.0;net10.0`
- **Tests:** `src/FluidPDF.Tests/` — targets `net48;net10.0`, xUnit v3 + FluentAssertions + NSubstitute

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

# Pack NuGet packages
dotnet pack src/FluidPDF/FluidPDF.csproj -c Release
dotnet pack src/FluidPDF.Scriban/FluidPDF.Scriban.csproj -c Release
dotnet pack src/FluidPDF.Razor/FluidPDF.Razor.csproj -c Release

# Clean
dotnet clean src/FluidPDF.sln
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
`InternalExtensionMethodsTests`, `ExpandoObjectConverterTests`, `LocalizationProviderTests`.
`TemplateEngineTests` is an abstract base class with shared `[Fact]` tests inherited by the three
engine test classes. Test helpers live in `Mocks/` (e.g. `ChromiumRetrieverMock`) and `Mothers/`
(e.g. `TemplateModelMother`, `PDFDocumentMother`).

---

## Lint / Format

There are no configured linters or formatters. The compiler enforces null safety globally via
`<Nullable>enable</Nullable>`. No `.editorconfig`, StyleCop, or Roslyn analyzer packages are
present in the library project (test project uses `FluentAssertions.Analyzers` and
`NSubstitute.Analyzers.CSharp`).

---

## Architecture

The library exposes two independent public APIs:

### 1. `FluidPDFReportFactory` (direct factory)
```csharp
FluidPDFReportFactory factory = new(templateEngine, chromiumRetrieverOptions, fluidPdfReportOptions);
byte[] pdf = await factory.CompileReportAsync(template, model);
await factory.CompileReportAsync(template, model, destinationStream);
```

### 2. `FluidPDFBuilder` (fluent builder)
Static entry point `FluidPDF.NewReport()`; configured via `With*()` chain; delegates to
`FluidPDFReportFactory` internally.
```csharp
byte[] pdf = await FluidPDF.NewReport()
    .WithObjectModel(myModel)
    .WithTemplate(templateString)
    .BuildAsync();
```

### Key subsystems
- **`FluidTemplateEngine`** — renders Liquid templates via Fluid; implements `IFluidPDFTemplateEngine`
- **`ScribanTemplateEngine`** / **`RazorTemplateEngine`** — alternative engines in adapter packages
- **`FluidPDFTemplateModel`** — discriminated-union sealed class; factory methods `FromDataRow`,
  `FromDataTable`, `FromDictionary`, `FromJsonString`, `FromObject`, `FromPlainValue`
- **`IFluidPDFTemplateEngine`** — interface with `RenderTemplateAsync` overloads returning `ValueTask<string>`
- **`ChromiumRetriever`** — downloads/locates Chromium, launches headless browser; implements `IChromiumRetriever`
- **`InternalExtensionMethods`** — string guards and null guards (`GetNonNullOrThrow<T>`)
- **`ExpandoObjectConverter`** — converts model types to `ExpandoObject` for template engine consumption

---

## Code Style Guidelines

### Indentation and Formatting

- **Indent:** 4 spaces (no tabs)
- **Braces:** Allman style — opening brace on its own line for classes, methods, and control flow
- **Expression-bodied members:** use liberally for single-expression methods and properties:
  ```csharp
  public bool IsObject => Type == FluidPDFTemplateModelType.Object;
  ```
- **Object initializers:** single-line when short; multi-line with trailing comma when longer
- **Blank lines:** one blank line between methods; no blank line between namespace declaration and class
- **Line endings:** CRLF

### Imports (`using` directives)

- All `using` directives go at the top of the file, **outside** the namespace block
- Namespaces use **braced syntax** (not file-scoped `namespace Foo;`)
- Convention: project/third-party namespaces before `System.*` namespaces
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
| Static factory methods | `NewXxx()` or `FromXxx()` | `NewReport()`, `FromObject()` |
| Interface names | `I` prefix | `IFluidPDFBuilder`, `IChromiumRetriever` |
| Files | Match primary class name exactly | `FluidPDFBuilder.cs`, `ChromiumRetriever.cs` |
| Directories | `PascalCase` | `Builder/`, `Support/IO/`, `Support/PDF/` |
| Enum values | `PascalCase` | `ZeroPoint5`, `DataRow`, `JsonString` |
| Test classes | `<Subject>Tests` | `FluidTemplateEngineTests` |
| Test methods | `<Action>_<Result>` or `<Action>_Should<What>_When<Condition>` | `RenderWithObject_ReturnsRenderedTemplate`, `BuildAsync_ShouldThrowFluidPDFBuilderConfigException_WhenNoTemplateIsSet` |

### Types

- **Nullable reference types** are enabled — honour all warnings; never suppress with `!` without justification
- Prefer **explicit types** over `var` in library code (tests may use `var`)
- Use **`ValueTask<string>`** for `IFluidPDFTemplateEngine` render methods; use `Task<T>` for factory and builder terminal methods
- Use **`decimal`** for scale/ratio values (not `double` or `float`)
- Apply **`sealed`** to all concrete implementation classes
- Mark stateless helper/utility classes as **`static`** (e.g. `PDFCompressHelper`, `InternalExtensionMethods`)
- Apply **`where T : notnull`** generic constraint where nullability must be excluded
- Prefer **primary constructors** (C# 12+) for simple classes and records:
  ```csharp
  internal sealed class ChromiumRetriever(ChromiumRetrieverOptions options) : IChromiumRetriever { }
  public class FluidPDFBuilderConfigException(string message) : Exception(message) { }
  ```
- Prefer **collection expressions** (`[...]`) over `new List<T>()` or array initializers
- Use **switch expressions** for exhaustive enum/type dispatch; default arm uses `ArgumentOutOfRangeException` (never `NotImplementedException`)
- Use **`private static readonly`** for expensive-to-create objects shared across instances
- Use `#if NETSTANDARD2_0` / `#else` for TFM-conditional APIs

### Async and ConfigureAwait

- **Always call `.ConfigureAwait(false)`** on every `await` in library code
- Use `try/finally` to guarantee `IPage.CloseAsync()` / `IBrowser.CloseAsync()` are called even when exceptions occur

### Error Handling

- Throw **domain exceptions** (`FluidPDFTemplateRenderException`, `FluidPDFBuilderConfigException`)
  for library-level errors; wrap low-level exceptions as `innerException`
- Use the internal null-guard extension for argument validation:
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
- Do not unseal concrete classes without a deliberate reason
