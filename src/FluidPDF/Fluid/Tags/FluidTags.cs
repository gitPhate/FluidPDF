using Fluid;
using Fluid.Ast;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace FluidPDF.Fluid.Tags
{
    internal static class FluidTags
    {
        internal static void Register(FluidPDFParser parser)
        {
            parser.RegisterEmptyTag(BackslashTag.TagName, BackslashTag.TagRenderFunction);
            parser.RegisterEmptyTag(SlashTag.TagName, SlashTag.TagRenderFunction);
            parser.RegisterEmptyTag(PipeTag.TagName, PipeTag.TagRenderFunction);
            parser.RegisterEmptyTag(DoubleQuoteTag.TagName, DoubleQuoteTag.TagRenderFunction);
            parser.RegisterEmptyTag(SingleQuoteTag.TagName, SingleQuoteTag.TagRenderFunction);
            parser.RegisterEmptyTag(PathSeparatorTag.TagName, PathSeparatorTag.TagRenderFunction);
            parser.RegisterEmptyTag(StringEmptyTag.TagName, StringEmptyTag.TagRenderFunction);
            parser.RegisterEmptyTag(FloatRandomTag.TagName, FloatRandomTag.TagRenderFunction);
            parser.RegisterIdentifierTag(GuidTag.TagName, GuidTag.TagRenderFunction);
            parser.RegisterArgumentsTag(IntRandomTag.TagName, IntRandomTag.TagRenderFunction);
        }

        // --- Empty tags ---

        private static class BackslashTag
        {
            public static readonly string TagName = "backslash";

            public static Func<TextWriter, TextEncoder, TemplateContext, ValueTask<Completion>> TagRenderFunction =>
                FluidBaseSimpleTag.TagRenderFunction(_ => "\\");
        }

        private static class SlashTag
        {
            public static readonly string TagName = "slash";

            public static Func<TextWriter, TextEncoder, TemplateContext, ValueTask<Completion>> TagRenderFunction =>
                FluidBaseSimpleTag.TagRenderFunction(_ => "/");
        }

        private static class PipeTag
        {
            public static readonly string TagName = "pipe";

            public static Func<TextWriter, TextEncoder, TemplateContext, ValueTask<Completion>> TagRenderFunction =>
                FluidBaseSimpleTag.TagRenderFunction(_ => "|");
        }

        private static class DoubleQuoteTag
        {
            public static readonly string TagName = "double_quote";

            public static Func<TextWriter, TextEncoder, TemplateContext, ValueTask<Completion>> TagRenderFunction =>
                FluidBaseSimpleTag.TagRenderFunction(_ => "\"");
        }

        private static class SingleQuoteTag
        {
            public static readonly string TagName = "single_quote";

            public static Func<TextWriter, TextEncoder, TemplateContext, ValueTask<Completion>> TagRenderFunction =>
                FluidBaseSimpleTag.TagRenderFunction(_ => "'");
        }

        private static class PathSeparatorTag
        {
            public static readonly string TagName = "path_separator";

            public static Func<TextWriter, TextEncoder, TemplateContext, ValueTask<Completion>> TagRenderFunction =>
                FluidBaseSimpleTag.TagRenderFunction(_ => Path.DirectorySeparatorChar.ToString());
        }

        private static class StringEmptyTag
        {
            public static readonly string TagName = "string_empty";

            public static Func<TextWriter, TextEncoder, TemplateContext, ValueTask<Completion>> TagRenderFunction =>
                FluidBaseSimpleTag.TagRenderFunction(_ => string.Empty);
        }

        private static class FloatRandomTag
        {
            public static readonly string TagName = "float_random";

            private static readonly Random _random = new();

            public static Func<TextWriter, TextEncoder, TemplateContext, ValueTask<Completion>> TagRenderFunction =>
                async (writer, encoder, context) =>
                {
                    string value = _random.NextDouble().ToString(context.CultureInfo);
                    await writer.WriteAsync(value).ConfigureAwait(false);
                    return Completion.Normal;
                };
        }

        // --- Identifier tags ---

        private static class GuidTag
        {
            public static readonly string TagName = "guid";

            public static Func<string, TextWriter, TextEncoder, TemplateContext, ValueTask<Completion>> TagRenderFunction =>
                async (identifier, writer, encoder, context) =>
                {
                    Guid guid;
                    if (identifier == "new")
                    {
                        guid = Guid.NewGuid();
                    }
                    else if (identifier == "empty")
                    {
                        guid = Guid.Empty;
                    }
                    else
                    {
                        return Completion.Continue;
                    }

                    await writer.WriteAsync(guid.ToString()).ConfigureAwait(false);
                    return Completion.Continue;
                };
        }

        // --- Argument tags ---

        private static class IntRandomTag
        {
            public const string TagName = "int_random";

            private static readonly Random _random = new();

            public static Func<IReadOnlyList<FilterArgument>, TextWriter, TextEncoder, TemplateContext, ValueTask<Completion>> TagRenderFunction =>
                async (argumentsList, writer, encoder, context) =>
                {
                    IEnumerable<FilterArgument?> nullableArgs = argumentsList.Select(x => new FilterArgument?(x));

                    int minValue = await GetFilterValue(nullableArgs, context, "min_value", 0).ConfigureAwait(false);
                    int maxValue = await GetFilterValue(nullableArgs, context, "max_value", int.MaxValue).ConfigureAwait(false);

                    string value = _random.Next(minValue, maxValue).ToString(context.CultureInfo);
                    await writer.WriteAsync(value).ConfigureAwait(false);
                    return Completion.Normal;
                };

            private static async ValueTask<int> GetFilterValue(
                IEnumerable<FilterArgument?> nullableArgs,
                TemplateContext context,
                string filterName,
                int defaultValue)
            {
                FilterArgument? valueFilter =
                    nullableArgs.FirstOrDefault(x => x.HasValue && x.Value.Name == filterName);

                if (!valueFilter.HasValue)
                {
                    return defaultValue;
                }

                decimal evaluated =
                    (await valueFilter.Value.Expression
                        .EvaluateAsync(context)
                        .ConfigureAwait(false))
                    .ToNumberValue();

                return Convert.ToInt32(evaluated);
            }
        }
    }
}
