using FluidPDF.Support.Json;
using FluidPDF.Templating;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FluidPDF.Razor
{
    internal class RazorRuntimeModel
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            Converters = { new ExpandoObjectConverter() }
        };

        public object DefaultModelBuild { get; }
        public object? ResxModelBuild { get; }

        public object UnifiedModelBuild
        {
            get
            {
                if (ResxModelBuild is null)
                {
                    return DefaultModelBuild;
                }

                IDictionary<string, object?> expandoDict = (ExpandoObject)DefaultModelBuild;
                expandoDict[ModelNames.ResxModelName] = ResxModelBuild;
                return DefaultModelBuild;
            }
        }

        internal RazorRuntimeModel(FluidPDFTemplateModel[] models)
        {
            FluidPDFTemplateModel? resxModel = models.FirstOrDefault(x => x.Name == ModelNames.ResxModelName);

            ResxModelBuild = BuildResxModel(resxModel);
            DefaultModelBuild = BuildDefaultModel(models);
        }

        private static object BuildDefaultModel(FluidPDFTemplateModel[] models)
        {
            Dictionary<string, object> modelsMap = [];
            foreach (FluidPDFTemplateModel model in models)
            {
                if (model.Name == ModelNames.ResxModelName)
                {
                    continue;
                }

                if (modelsMap.ContainsKey(model.Name))
                {
                    throw new ArgumentException($"Some models with the same name have already been added. Key: {model.Name}");
                }

                modelsMap.Add(model.Name, ConvertModel(model));
            }

            if (modelsMap.Count == 1)
            {
                return ConvertModel(models.First());
            }

            IDictionary<string, object?> expandoDict = new ExpandoObject();

            foreach (var kvp in modelsMap)
            {
                expandoDict[kvp.Key] = kvp.Value;
            }

            return (ExpandoObject)expandoDict;
        }

        private static ExpandoObject? BuildResxModel(FluidPDFTemplateModel? resxModel)
        {
            if (resxModel is null)
            {
                return null;
            }

            return (ExpandoObject)ConvertModel(resxModel); //will never be of type PlainValue
        }

        private static object ConvertModel(FluidPDFTemplateModel model) =>
            model.Type switch
            {
                FluidPDFTemplateModelType.Object => ObjectToExpando(model.ObjectValue!),
                FluidPDFTemplateModelType.Dictionary => DictionaryToExpando(model.Dictionary!),
                FluidPDFTemplateModelType.JsonNode => JsonNodeToObject(model.JsonNode!)!,
                FluidPDFTemplateModelType.DataRow => DataRowToExpando(model.DataRow!),
                FluidPDFTemplateModelType.DataTable => DataTableToExpando(model.DataTable!),
                FluidPDFTemplateModelType.PlainValue => model.PlainValue!,
                _ => throw new NotSupportedException($"Unsupported model type: {model.Type}")
            };

        private static ExpandoObject DictionaryToExpando(IDictionary<string, object?> dictionary)
        {
            string json = JsonSerializer.Serialize(dictionary);
            ExpandoObject? result = JsonSerializer.Deserialize<ExpandoObject>(json, _jsonOptions);
            return result ?? new();
        }

        private static ExpandoObject ObjectToExpando(object obj)
        {
            JsonNode node = JsonSerializer.SerializeToNode(obj) ?? throw new InvalidOperationException("JSON parsing failed");
            return JsonObjectToExpando(node.AsObject());
        }

        private static ExpandoObject DataRowToExpando(DataRow row)
        {
            IDictionary<string, object?> expandoDict = new ExpandoObject();

            foreach (DataColumn column in row.Table.Columns)
            {
                expandoDict[column.ColumnName] = row.IsNull(column) ? null : row[column];
            }

            return (ExpandoObject)expandoDict;
        }

        private static ExpandoObject DataTableToExpando(DataTable table)
        {
            List<ExpandoObject> rows = [];

            foreach (DataRow row in table.Rows)
            {
                rows.Add(DataRowToExpando(row));
            }

            IDictionary<string, object?> expandoDict = new ExpandoObject();
            expandoDict[nameof(DataTable.Rows)] = rows;
            return (ExpandoObject)expandoDict;
        }

        private static object? JsonNodeToObject(JsonNode? node) =>
            node switch
            {
                JsonObject obj => JsonObjectToExpando(obj),
                JsonArray arr => arr.Select(JsonNodeToObject).ToList(),
                JsonValue val => GetJsonValue(val),
                null => null,
                _ => node.ToString()
            };

        private static ExpandoObject JsonObjectToExpando(JsonObject obj)
        {
            IDictionary<string, object?> expando = new ExpandoObject();

            foreach (KeyValuePair<string, JsonNode?> kvp in obj)
            {
                expando[kvp.Key] = JsonNodeToObject(kvp.Value);
            }

            return (ExpandoObject)expando;
        }

        private static object? GetJsonValue(JsonValue val)
        {
            if (val.TryGetValue<bool>(out bool b))     return b;
            if (val.TryGetValue<long>(out long l))     return l;
            if (val.TryGetValue<double>(out double d)) return d;
            if (val.TryGetValue<string>(out string? s)) return s;
            return val.ToString();
        }
    }
}
