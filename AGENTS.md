# FluidPDF – Agent Guidelines

## Project Overview

FluidPDF is a .NET Standard 2.0 class library (NuGet package) for PDF generation. It uses the Fluid
templating engine (Liquid syntax) to render HTML from a data model, then uses PuppeteerSharp
(headless Chromium) to print the HTML to a PDF. PDFsharp is used optionally for post-processing.

- **Solution:** `src/FluidPDF.sln`
- **Library:** `src/FluidPDF/` (targets `netstandard2.0`, C# 14 via PolySharp 1.15.0)
- **Tests:** `src/FluidPDF.Tests/` (targets `net8.0`, xUnit v3 + FluentAssertions)

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

Tests are integration tests; they download Chromium at runtime and write output to `C:\temp\`.

```bash
# Run all tests
dotnet test src/FluidPDF.sln

# Run all tests (skip rebuild)
dotnet test src/FluidPDF.sln --no-build

# Run a single test by fully-qualified name
dotnet test src/FluidPDF.sln --filter "FullyQualifiedName=FluidPDF.Tests.FluidTests.TestFluid"

# Run all tests in a class
dotnet test src/FluidPDF.sln --filter "ClassName=FluidPDF.Tests.FluidPDFTests"

# Run a test by partial display name
dotnet test src/FluidPDF.sln --filter "Name=TestMultipleModels"
```

Existing tests: `FluidPDFTests.TestObjectModel` (Theory × 4), `FluidPDFTests.TestMultipleModels`
(Fact), `FluidTests.TestFluid` (Fact).

---

## Lint / Format

There are no configured linters or formatters. The compiler enforces null safety via
`<Nullable>enable</Nullable>`. No `.editorconfig`, StyleCop, or Roslyn analyzer packages are
present in the main library project.

---

## Architecture

The library follows a **Fluent Builder → Internal Factory → Prototype** pattern:

1. **`FluidPDFBuilder.NewWithModel<T>(model)`** — static entry point, returns `IFluidPDFBuilder`
2. **`With*()`** — fluent configuration methods on `IFluidPDFBuilder`, each returns `this`
3. **`Build*Async()`** — three terminal methods producing an `IFluidPDFPrototype`:
   - `BuildAsync()` → lazy prototype (browser stays open, PDF generated on demand)
   - `BuildEagerStreamAsync()` → eager prototype holding a `Stream`
   - `BuildEagerByteArrayAsync()` → eager prototype holding a `byte[]`
4. **`IFluidPDFPrototype`** — exposes `ToByteArrayAsync()`, `ToStreamAsync()`, `ToFileAsync()`,
   `RenderedContent`; implements both `IDisposable` and `IAsyncDisposable`

`FluidPDFPrototypeFactory` is the internal orchestrator. `FluidModel` is a discriminated-union
sealed class with factory methods (`FromDataRow`, `FromDictionary`, `FromJsonString`, `FromObject`,
`FromPlainValue`). There is **no DI container** — all objects are constructed directly.

---

## Code Style Guidelines

### Indentation and Formatting

- **Indent:** 4 spaces (no tabs)
- **Braces:** Allman style — opening brace on its own line for classes, methods, and control flow
- **Expression-bodied members:** use liberally for single-expression methods, properties, and
  constructors:
  ```csharp
  public bool IsObject => IsFluidModelType(FluidModelType.Object);
  public void Dispose() => _stream?.Dispose();
  ```
- **Object initializers:** single-line when short; multi-line with trailing comma when longer:
  ```csharp
  new MarginOptions { Bottom = "0.4 in", Left = "0.4 in", Right = "0.4 in", Top = "0.4 in" }

  new PdfOptions
  {
      Format = _paperFormat,
      Landscape = _landscape,
      MarginOptions = _marginOptions,
  };
  ```
- **Blank lines:** one blank line between methods; no blank line between namespace declaration and
  class declaration
- **Line endings:** CRLF

### Imports (`using` directives)

- All `using` directives go at the top of the file, outside the namespace
- Namespaces are braced (not file-scoped)
- No strict ordering is enforced, but the observed convention groups third-party/project namespaces
  before `System.*` namespaces
- No `using static`; no `#region` blocks

### Naming Conventions

| Element | Convention | Examples |
|---|---|---|
| Classes, interfaces, enums, records | `PascalCase` | `FluidPDFBuilder`, `IFluidPDFPrototype` |
| Abbreviations in names | All-caps | `PDF`, `HTML`, `IO` — e.g. `PDFRegenHelper` |
| Private / protected fields | `_camelCase` | `_landscape`, `_chromeExePath` |
| Parameters and local variables | `camelCase` | `modelName`, `cultureInfo` |
| Properties and methods | `PascalCase` | `RenderedContent`, `BuildAsync()` |
| Async methods | Suffix `Async` | `BuildAsync()`, `ToByteArrayAsync()` |
| Static factory methods | `NewXxx()` or `FromXxx()` | `NewFluidPDFPrototypeFactory()`, `FromObject()` |
| Interface names | `I` prefix | `IFluidPDFBuilder`, `IFluidPDFPrototype` |
| Files | Match class name exactly | `FluidPDFBuilder.cs`, `IFluidPDFPrototype.cs` |
| Directories | `PascalCase` | `Builder/`, `PuppeteerSharp/`, `Support/IO/` |
| Enum values | `PascalCase` | `ZeroPoint5`, `DataRow`, `JsonString` |

### Types

- **Nullable reference types** are enabled (`<Nullable>enable</Nullable>`) — honour all warnings
- Prefer **explicit types** over `var` in library code; `var` is acceptable in test files
- Use **`ValueTask<T>`** for high-frequency / interface-level async paths; use `Task<T>` for
  lower-frequency builder and factory methods
- Use **`decimal`** for scale/ratio values (not `double` or `float`)
- Apply **`sealed`** to all concrete prototype and implementation classes
- Mark stateless helper/utility classes as **`static`**
- Apply **`where T : notnull`** generic constraint where nullability must be excluded
- Prefer **primary constructors** (C# 12, backported via PolySharp) for simple classes and records:
  ```csharp
  internal sealed class FluidPDFBuilderConfigException(string message) : Exception(message) { }
  ```
- Prefer **collection expressions** (`[...]`) over `new List<T>()` or array initializers:
  ```csharp
  FluidModel[] models = [model1, model2];
  ```
- Use **switch expressions** for exhaustive enum/type dispatch

### Async and ConfigureAwait

- **Always call `.ConfigureAwait(false)`** on every `await` in library code (not required in tests):
  ```csharp
  var result = await SomeMethodAsync().ConfigureAwait(false);
  ```
- Use `finally` blocks to guarantee disposal of `IBrowser` / `IPage` resources

### Error Handling

- Throw **domain exceptions** (`FluidRenderException`, `FluidPDFBuilderConfigException`) for
  library-level errors; wrap low-level exceptions as `innerException`
- Custom exceptions inherit from `Exception` and provide all standard constructors (default,
  message, message + inner, serialization) unless using a primary constructor for brevity
- Use the internal extension guard `GetNonNullOrThrow<T>()` for null argument validation:
  ```csharp
  value.GetNonNullOrThrow(nameof(value));
  ```
- Use `File.Exists()` before accepting file paths; throw `FileNotFoundException` with the path
- Validate configuration booleans before throwing `FluidPDFBuilderConfigException`; compute them
  into named local variables first for readability

### Tests

- Use **xUnit** (`[Fact]` / `[Theory]` / `[InlineData]`) — `Xunit` is globally imported; no
  explicit `using Xunit;` required
- Use **FluentAssertions** for all assertions (`.Should().Be(...)`, `.Should().NotBeNull()`, etc.)
- Test class names: `<Subject>Tests` (e.g. `FluidPDFTests`, `FluidTests`)
- Test method names: `Test<Scenario>` (e.g. `TestObjectModel`, `TestFluid`)
- Place shared test models / helpers in `TestObjects.cs`
- `#nullable disable` is acceptable in test support files with uninitialized model properties

### What to Avoid

- Do not introduce a DI container — the library constructs objects directly
- Do not use `var` in library (non-test) code
- Do not use file-scoped namespaces — keep braced `namespace` blocks
- Do not use `using static`
- Do not remove `.ConfigureAwait(false)` from `await` calls in library code
- Do not use `float` or `double` for margin, scale, or ratio values — use `decimal`
- Do not add `#region` blocks
