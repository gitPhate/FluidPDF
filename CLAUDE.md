# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

FluidPDF is a .NET library (NuGet) that renders an HTML template against a data model with a template engine, then prints the HTML to PDF via headless Chromium (PuppeteerSharp). PdfPig is used for optional compression (page regeneration with object de-duplication) and, in tests, to build/inspect PDFs.

Solution: `src/FluidPDF.sln`

- `src/FluidPDF` — core + Fluid (Liquid) engine; `netstandard2.0;net9.0;net10.0`, `LangVersion 14` (PolySharp polyfills on netstandard2.0)
- `src/FluidPDF.Scriban`, `src/FluidPDF.Razor` — optional adapter packages for the other engines (same TFMs)
- `src/FluidPDF.Tests` — xUnit v3 + FluentAssertions + NSubstitute; `net48;net10.0`

## Commands

Run from the repo root.

```bash
dotnet build src/FluidPDF.sln
dotnet test src/FluidPDF.sln
dotnet test src/FluidPDF.sln --framework net10.0
dotnet test src/FluidPDF.sln --filter "FullyQualifiedName~FluidTemplateEngineTests.RenderWithObject_ReturnsRenderedTemplate"   # single test
dotnet test src/FluidPDF.sln --filter "Category!=E2E"    # skip tests needing real Chrome
dotnet pack src/FluidPDF/FluidPDF.csproj -c Release      # likewise Scriban / Razor csproj
```

No linters/formatters/.editorconfig are configured.

## Architecture

Two public entry points, the builder delegating to the factory:

- `FluidPDFReportFactory` — `CompileReportAsync(template, model(s), [stream], options)`; takes an `IFluidPDFTemplateEngine`, `ChromiumRetrieverOptions`, `FluidPDFReportOptions`.
- `FluidPDF.NewReport()` (`Builder/FluidPDFBuilder`) — fluent `With*()` chain ending in `BuildAsync()`. Engine adapters add `With*TemplateEngine()` extension methods (`FluidPDFBuilderRazorExtensions`, `FluidPDFBuilderScribanExtensions`).

Pipeline: template + `FluidPDFTemplateModel` → `IFluidPDFTemplateEngine.RenderTemplateAsync` (returns `ValueTask<string>`) → HTML → `ChromiumRetriever` (downloads or uses external Chrome; launches headless browser) → PDF bytes → optional `PDFCompressHelper`.

- `FluidPDFTemplateModel` is a discriminated-union-style sealed class built with `FromObject/FromJsonString/FromDataTable/FromDataRow/FromDictionary/FromPlainValue`. Multiple named models can be passed. `ExpandoObjectConverter` (`Support/Json`) normalizes models to `ExpandoObject` for the engines.
- Engines each have their own subsystem: Fluid (`Fluid/` — custom parser, `Filters/`, `Tags/`), Scriban (`ScribanFunctions`, `HTMLEncodedTemplateContext`), Razor (runtime compile with `RazorTemplateCache`, `FluidPDFRazorTemplateBase`).
- Domain exceptions live in `Exceptions/` (`FluidPDFTemplateRenderException`, `FluidPDFBuilderConfigException`).
- `InternalsVisibleTo FluidPDF.Tests`.

## Tests

- `TemplateEngineTests` is an abstract base with shared `[Fact]`s inherited by the Fluid/Scriban/Razor engine test classes; same for `TemplateUtilityTests`.
- Helpers: `Mocks/ChromiumRetrieverMock`, `Mothers/` (`TemplateModelMother`, `PDFDocumentMother`).
- `E2E/` tests (`[Trait("Category","E2E")]`) use real Chrome at a hardcoded `C:\Program Files\Google\Chrome\Application\chrome.exe` and write PDFs to `<bin>/E2E`; data/templates in `E2E/Data` and `E2E/Templates`.
- Test names: `<Action>_<Result>` or `<Action>_Should<What>_When<Condition>`.

## Conventions (library code)

- Explicit types, never `var` (except LINQ results); prefer arrays over `IReadOnlyList/Collection`.
- Braced (not file-scoped) namespaces, `using`s outside the namespace, no `using static`, no `#region`, Allman braces, 4 spaces, CRLF.
- Abbreviations all-caps (`PDF`, `HTML`); private fields `_camelCase`; async methods suffixed `Async`.
- Always `.ConfigureAwait(false)` on every `await`; close `IPage`/`IBrowser` in `try/finally`.
- Concrete classes `sealed`; stateless helpers `static`; primary constructors and collection expressions preferred.
- `decimal` for scale/margin values, never `float`/`double`.
- Exhaustive switches: default arm throws `ArgumentOutOfRangeException`, never `NotImplementedException`.
- Null/arg validation via `GetNonNullOrThrow` (`InternalExtensionMethods`); wrap low-level exceptions as `innerException` in domain exceptions.
- TFM-specific APIs via `#if NETSTANDARD2_0`.
