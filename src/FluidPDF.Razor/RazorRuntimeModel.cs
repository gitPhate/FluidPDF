using FluidPDF.Support.Json;
using FluidPDF.Templating;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text.Json;

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
            DefaultModelBuild = BuildDefaultModel(models, resxModel is not null);
        }

        private static object BuildDefaultModel(FluidPDFTemplateModel[] models, bool hasResxModel)
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

            return (ExpandoObject)ConvertModel(resxModel); //will never be of type PlainObject
        }

        private static object ConvertModel(FluidPDFTemplateModel model) =>
            model.Type switch
            {
                FluidPDFTemplateModelType.Object => SerializeToExpando(model.ObjectValue!),
                FluidPDFTemplateModelType.Dictionary => DictionaryToExpando(model.Dictionary!),
                FluidPDFTemplateModelType.JsonString => JsonStringToExpando(model.JsonString!),
                FluidPDFTemplateModelType.DataRow => DataRowToExpando(model.DataRow!),
                FluidPDFTemplateModelType.DataTable => DataTableToExpando(model.DataTable!),
                FluidPDFTemplateModelType.PlainValue => model.PlainValue!,
                _ => throw new NotSupportedException($"Unsupported model type: {model.Type}")
            };

        private static ExpandoObject SerializeToExpando(object obj)
        {
            string json = JsonSerializer.Serialize(obj);
            return JsonStringToExpando(json);
        }

        private static ExpandoObject JsonStringToExpando(string json)
        {
            ExpandoObject? result = JsonSerializer.Deserialize<ExpandoObject>(json, _jsonOptions);
            return result ?? new();
        }

        private static ExpandoObject DictionaryToExpando(IDictionary<string, object> dictionary)
        {
            IDictionary<string, object?> expandoDict = new ExpandoObject();

            foreach (KeyValuePair<string, object> kvp in dictionary)
            {
                expandoDict[kvp.Key] = kvp.Value;
            }

            return (ExpandoObject)expandoDict;
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
    }
}
