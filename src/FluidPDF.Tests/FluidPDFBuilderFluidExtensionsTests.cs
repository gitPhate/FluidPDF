using FluentAssertions;
using Fluid.Ast;
using Fluid.Values;
using FluidPDF.Builder;
using FluidPDF.Exceptions;
using FluidPDF.Fluid;
using FluidPDF.Support.PuppeteerSharp;
using FluidPDF.Templating;
using FluidPDF.Tests.Mocks;
using NSubstitute;

namespace FluidPDF.Tests
{
    public class FluidPDFBuilderFluidExtensionsTests
    {
        // ── WithFluidFilter ──────────────────────────────────────────────────────

        [Fact]
        public async Task WithFluidFilter_ShouldRegisterCustomFilter_WhenCalledBeforeBuild()
        {
            // Arrange
            IChromiumRetriever retriever = ChromiumRetrieverMock.CreateWithSinglePagePdfAndContentCapture(
                out _,
                out _,
                out string?[] capturedContent);

            using FluidPDFBuilder builder = new(retriever);
            builder.WithObjectModel(new { val = "hello" });
            builder.WithTemplate("{{ Model.val | shout }}");
            builder.WithFluidFilter("shout", (input, args, ctx) =>
                new ValueTask<FluidValue>(new StringValue(input.ToStringValue().ToUpperInvariant())));

            // Act
            await builder.BuildAsync();

            // Assert
            capturedContent[0].Should().Be("HELLO");
        }

        [Fact]
        public void WithFluidFilter_ShouldThrowFluidPDFBuilderConfigException_WhenCustomEngineAlreadySet()
        {
            // Arrange
            IFluidPDFTemplateEngine customEngine = Substitute.For<IFluidPDFTemplateEngine>();

            using FluidPDFBuilder builder = new();
            builder.WithTemplateEngine(customEngine);

            // Act
            Action act = () => builder.WithFluidFilter("shout", (input, args, ctx) =>
                new ValueTask<FluidValue>(new StringValue(input.ToStringValue())));

            // Assert
            act.Should().Throw<FluidPDFBuilderConfigException>();
        }

        // ── WithFluidEmptyTag ────────────────────────────────────────────────────

        [Fact]
        public async Task WithFluidEmptyTag_ShouldRegisterCustomTag_WhenCalledBeforeBuild()
        {
            // Arrange
            IChromiumRetriever retriever = ChromiumRetrieverMock.CreateWithSinglePagePdfAndContentCapture(
                out _,
                out _,
                out string?[] capturedContent);

            using FluidPDFBuilder builder = new(retriever);
            builder.WithObjectModel(new { });
            builder.WithTemplate("{% my_empty %}");
            builder.WithFluidEmptyTag("my_empty", async (writer, encoder, ctx) =>
            {
                await writer.WriteAsync("EMPTY_TAG").ConfigureAwait(false);
                return Completion.Normal;
            });

            // Act
            await builder.BuildAsync();

            // Assert
            capturedContent[0].Should().Be("EMPTY_TAG");
        }

        [Fact]
        public void WithFluidEmptyTag_ShouldThrowFluidPDFBuilderConfigException_WhenCustomEngineAlreadySet()
        {
            // Arrange
            IFluidPDFTemplateEngine customEngine = Substitute.For<IFluidPDFTemplateEngine>();

            using FluidPDFBuilder builder = new();
            builder.WithTemplateEngine(customEngine);

            // Act
            Action act = () => builder.WithFluidEmptyTag(
                "my_tag",
                async (writer, encoder, ctx) =>
                {
                    await writer.WriteAsync("x").ConfigureAwait(false);
                    return Completion.Normal;
                });

            // Assert
            act.Should().Throw<FluidPDFBuilderConfigException>();
        }

        // ── WithFluidIdentifierTag ───────────────────────────────────────────────

        [Fact]
        public async Task WithFluidIdentifierTag_ShouldRegisterCustomTag_WhenCalledBeforeBuild()
        {
            // Arrange
            IChromiumRetriever retriever = ChromiumRetrieverMock.CreateWithSinglePagePdfAndContentCapture(
                out _,
                out _,
                out string?[] capturedContent);

            using FluidPDFBuilder builder = new(retriever);
            builder.WithObjectModel(new { });
            builder.WithTemplate("{% greet World %}");
            builder.WithFluidIdentifierTag("greet", async (identifier, writer, encoder, ctx) =>
            {
                await writer.WriteAsync($"Hello {identifier}").ConfigureAwait(false);
                return Completion.Normal;
            });

            // Act
            await builder.BuildAsync();

            // Assert
            capturedContent[0].Should().Be("Hello World");
        }

        [Fact]
        public void WithFluidIdentifierTag_ShouldThrowFluidPDFBuilderConfigException_WhenCustomEngineAlreadySet()
        {
            // Arrange
            IFluidPDFTemplateEngine customEngine = Substitute.For<IFluidPDFTemplateEngine>();

            using FluidPDFBuilder builder = new();
            builder.WithTemplateEngine(customEngine);

            // Act
            Action act = () => builder.WithFluidIdentifierTag(
                "my_id_tag",
                async (id, writer, encoder, ctx) =>
                {
                    await writer.WriteAsync(id).ConfigureAwait(false);
                    return Completion.Normal;
                });

            // Assert
            act.Should().Throw<FluidPDFBuilderConfigException>();
        }

        // ── WithFluidArgumentTag ─────────────────────────────────────────────────

        [Fact]
        public async Task WithFluidArgumentTag_ShouldRegisterCustomTag_WhenCalledBeforeBuild()
        {
            // Arrange
            IChromiumRetriever retriever = ChromiumRetrieverMock.CreateWithSinglePagePdfAndContentCapture(
                out _,
                out _,
                out string?[] capturedContent);

            using FluidPDFBuilder builder = new(retriever);
            builder.WithObjectModel(new { });
            builder.WithTemplate("{% repeat text: 'AB', count: 3 %}");
            builder.WithFluidArgumentTag("repeat", async (args, writer, encoder, ctx) =>
            {
                FilterArgument? textArg = args.FirstOrDefault(a => a.Name == "text");
                FilterArgument? countArg = args.FirstOrDefault(a => a.Name == "count");
                string text = textArg is { } t
                    ? (await t.Expression.EvaluateAsync(ctx).ConfigureAwait(false)).ToStringValue()
                    : string.Empty;
                int count = countArg is { } c
                    ? Convert.ToInt32((await c.Expression.EvaluateAsync(ctx).ConfigureAwait(false)).ToNumberValue())
                    : 0;
                for (int i = 0; i < count; i++)
                {
                    await writer.WriteAsync(text).ConfigureAwait(false);
                }
                return Completion.Normal;
            });

            // Act
            await builder.BuildAsync();

            // Assert
            capturedContent[0].Should().Be("ABABAB");
        }

        [Fact]
        public void WithFluidArgumentTag_ShouldThrowFluidPDFBuilderConfigException_WhenCustomEngineAlreadySet()
        {
            // Arrange
            IFluidPDFTemplateEngine customEngine = Substitute.For<IFluidPDFTemplateEngine>();

            using FluidPDFBuilder builder = new();
            builder.WithTemplateEngine(customEngine);

            // Act
            Action act = () => builder.WithFluidArgumentTag(
                "my_arg_tag",
                async (args, writer, encoder, ctx) =>
                {
                    await writer.WriteAsync("ok").ConfigureAwait(false);
                    return Completion.Normal;
                });

            // Assert
            act.Should().Throw<FluidPDFBuilderConfigException>();
        }

        // ── FluidTemplateEngineOptions wiring ────────────────────────────────────

        [Fact]
        public async Task FluidTemplateEngineOptions_ShouldRenderCustomEmptyTag_WhenRegisteredOnEngine()
        {
            // Arrange
            FluidTemplateEngineOptions options = new FluidTemplateEngineOptions()
                .AddEmptyTag("my_empty",
                    async (writer, encoder, ctx) =>
                    {
                        await writer.WriteAsync("EMPTY_TAG").ConfigureAwait(false);
                        return Completion.Normal;
                    });

            using FluidTemplateEngine engine = new(options);

            // Act
            string result = await engine.RenderTemplateAsync(
                "{% my_empty %}",
                new { },
                new FluidPDFTemplateRenderOptions());

            // Assert
            result.Should().Be("EMPTY_TAG");
        }

        [Fact]
        public async Task FluidTemplateEngineOptions_ShouldRenderCustomIdentifierTag_WhenRegisteredOnEngine()
        {
            // Arrange
            FluidTemplateEngineOptions options = new FluidTemplateEngineOptions()
                .AddIdentifierTag("greet",
                    async (identifier, writer, encoder, ctx) =>
                    {
                        await writer.WriteAsync($"Hello {identifier}").ConfigureAwait(false);
                        return Completion.Normal;
                    });

            using FluidTemplateEngine engine = new(options);

            // Act
            string result = await engine.RenderTemplateAsync(
                "{% greet World %}",
                new { },
                new FluidPDFTemplateRenderOptions());

            // Assert
            result.Should().Be("Hello World");
        }

        [Fact]
        public async Task FluidTemplateEngineOptions_ShouldStillRenderBuiltInFilters_WhenUserOptionsAreProvided()
        {
            // Arrange
            FluidTemplateEngineOptions options = new FluidTemplateEngineOptions()
                .AddFilter("noop", (input, args, ctx) => new ValueTask<FluidValue>(input));

            using FluidTemplateEngine engine = new(options);

            // Act — use built-in to_base64 to confirm it is still registered; use assign
            // so the value flows as StringValue, not as a JSON-serialised ObjectValue.
            string result = await engine.RenderTemplateAsync(
                "{% assign v = 'A' %}{{ v | to_base64 }}",
                new { },
                new FluidPDFTemplateRenderOptions());

            // Assert
            result.Should().Be(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("A")));
        }

        [Fact]
        public async Task FluidTemplateEngineOptions_ShouldStillRenderBuiltInTags_WhenUserOptionsAreProvided()
        {
            // Arrange
            FluidTemplateEngineOptions options = new FluidTemplateEngineOptions()
                .AddFilter("noop", (input, args, ctx) => new ValueTask<FluidValue>(input));

            using FluidTemplateEngine engine = new(options);

            // Act — use built-in backslash tag
            string result = await engine.RenderTemplateAsync(
                "{% backslash %}",
                new { },
                new FluidPDFTemplateRenderOptions());

            // Assert
            result.Should().Be("\\");
        }

        // ── Null-guard checks ────────────────────────────────────────────────────

        [Fact]
        public void WithFluidFilter_ShouldThrowArgumentNullException_WhenNameIsNull()
        {
            using FluidPDFBuilder builder = new();
            Action act = () => builder.WithFluidFilter(null!, (input, args, ctx) => new ValueTask<FluidValue>(input));
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WithFluidFilter_ShouldThrowArgumentNullException_WhenDelegateIsNull()
        {
            using FluidPDFBuilder builder = new();
            Action act = () => builder.WithFluidFilter("some_filter", null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WithFluidEmptyTag_ShouldThrowArgumentNullException_WhenNameIsNull()
        {
            using FluidPDFBuilder builder = new();
            Action act = () => builder.WithFluidEmptyTag(null!,
                async (w, e, c) => { await w.WriteAsync("x").ConfigureAwait(false); return Completion.Normal; });
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WithFluidIdentifierTag_ShouldThrowArgumentNullException_WhenNameIsNull()
        {
            using FluidPDFBuilder builder = new();
            Action act = () => builder.WithFluidIdentifierTag(null!,
                async (id, w, e, c) => { await w.WriteAsync(id).ConfigureAwait(false); return Completion.Normal; });
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WithFluidArgumentTag_ShouldThrowArgumentNullException_WhenNameIsNull()
        {
            using FluidPDFBuilder builder = new();
            Action act = () => builder.WithFluidArgumentTag(null!,
                async (args, w, e, c) => { await w.WriteAsync("x").ConfigureAwait(false); return Completion.Normal; });
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
