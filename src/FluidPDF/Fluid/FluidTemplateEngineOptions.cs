using Fluid;
using Fluid.Ast;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace FluidPDF.Fluid
{
    /// <summary>
    /// Carries user-defined Fluid filter and tag registrations to be applied on top of the
    /// built-in ones when constructing a <see cref="FluidTemplateEngine"/>.
    /// </summary>
    public sealed class FluidTemplateEngineOptions
    {
        internal List<(string Name, FilterDelegate Delegate)> Filters { get; }
            = [];

        internal List<(string Name, Func<TextWriter, TextEncoder, TemplateContext, ValueTask<Completion>> Render)> EmptyTags { get; }
            = [];

        internal List<(string Name, Func<string, TextWriter, TextEncoder, TemplateContext, ValueTask<Completion>> Render)> IdentifierTags { get; }
            = [];

        internal List<(string Name, Func<IReadOnlyList<FilterArgument>, TextWriter, TextEncoder, TemplateContext, ValueTask<Completion>> Render)> ArgumentTags { get; }
            = [];

        internal bool IsEmpty =>
            Filters.Count == 0 &&
            EmptyTags.Count == 0 &&
            IdentifierTags.Count == 0 &&
            ArgumentTags.Count == 0;

        public FluidTemplateEngineOptions AddFilter(string name, FilterDelegate filterDelegate)
        {
            Filters.Add((name, filterDelegate));
            return this;
        }

        public FluidTemplateEngineOptions AddEmptyTag(
            string name,
            Func<TextWriter, TextEncoder, TemplateContext, ValueTask<Completion>> render)
        {
            EmptyTags.Add((name, render));
            return this;
        }

        public FluidTemplateEngineOptions AddIdentifierTag(
            string name,
            Func<string, TextWriter, TextEncoder, TemplateContext, ValueTask<Completion>> render)
        {
            IdentifierTags.Add((name, render));
            return this;
        }

        public FluidTemplateEngineOptions AddArgumentTag(
            string name,
            Func<IReadOnlyList<FilterArgument>, TextWriter, TextEncoder, TemplateContext, ValueTask<Completion>> render)
        {
            ArgumentTags.Add((name, render));
            return this;
        }
    }
}
