using Fluid;
using Fluid.Values;
using System;
using System.Data;
using System.Linq;
using System.Text.Json.Nodes;

namespace FluidPDF.Fluid
{
    internal static class FluidTemplateOptions
    {
        internal static TemplateOptions Instance { get; } = Factory();

        static FluidTemplateOptions() { }

        private static TemplateOptions Factory()
        {
            TemplateOptions templateOptions = new()
            {
                Trimming = TrimmingFlags.TagRight
            };

            templateOptions.ValueConverters.Add(x => x is DBNull o ? NilValue.Instance : null);

            templateOptions
                .MemberAccessStrategy
                .Register<DataRow, object>
                (
                    (row, fieldName) =>
                    {
                        if (row.IsNull(fieldName))
                        {
                            return DBNull.Value;
                        }

                        return row[fieldName];
                    }
                );

            templateOptions.MemberAccessStrategy.Register<JsonValue, object>((src, name) => src[name]!);
            templateOptions.MemberAccessStrategy.Register<JsonObject, object>((src, name) => src[name]!);
            templateOptions.MemberAccessStrategy.Register<JsonArray, object>((src, name) => src[name]!);
            templateOptions.MemberAccessStrategy.Register<JsonNode, object>((src, name) => src[name]!);

            templateOptions.ValueConverters.Add(x => x is JsonArray o ? new ArrayValue(o.Select(x => new ObjectValue(x)).ToArray()) : null);
            templateOptions.ValueConverters.Add(x => x is JsonNode o ? new ObjectValue(o) : null);
            templateOptions.ValueConverters.Add(x => x is JsonValue o ? new ObjectValue(o) : null);
            templateOptions.ValueConverters.Add(x => x is JsonObject o ? new ObjectValue(o) : null);

            return templateOptions;
        }
    }
}
