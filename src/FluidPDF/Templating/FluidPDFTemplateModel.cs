using FluidPDF.Support;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FluidPDF.Templating
{
    public enum FluidPDFTemplateModelType
    {
        DataRow,
        DataTable,
        Dictionary,
        JsonNode,
        Object,
        PlainValue
    }

    public sealed class FluidPDFTemplateModel
    {
        public const string DefaultName = "Model";

        public string Name { get; }
        public FluidPDFTemplateModelType Type { get; }
        public DataTable? DataTable { get; }
        public DataRow? DataRow { get; }
        public IDictionary<string, object?>? Dictionary { get; }
        public JsonNode? JsonNode { get; }
        public object? ObjectValue { get; }
        public object? PlainValue { get; }

        private FluidPDFTemplateModel
        (
            string modelName,
            FluidPDFTemplateModelType modelType,
            DataRow? dataRow = null,
            DataTable? dataTable = null,
            IDictionary<string, object?>? dictionary = null,
            JsonNode? jsonNode = null,
            object? objectValue = null,
            object? plainValue = null
        )
        {
            Name = modelName;
            Type = modelType;
            DataRow = dataRow;
            DataTable = dataTable;
            Dictionary = dictionary;
            JsonNode = jsonNode;
            ObjectValue = objectValue;
            PlainValue = plainValue;
        }

        public bool IsDataRow => IsFluidPDFTemplateModelType(FluidPDFTemplateModelType.DataRow);
        public bool IsDataTable => IsFluidPDFTemplateModelType(FluidPDFTemplateModelType.DataTable);
        public bool IsDictionary => IsFluidPDFTemplateModelType(FluidPDFTemplateModelType.Dictionary);
        public bool IsPlainValue => IsFluidPDFTemplateModelType(FluidPDFTemplateModelType.PlainValue);
        public bool IsObject => IsFluidPDFTemplateModelType(FluidPDFTemplateModelType.Object);
        public bool IsJsonNode => IsFluidPDFTemplateModelType(FluidPDFTemplateModelType.JsonNode);

        private bool IsFluidPDFTemplateModelType(FluidPDFTemplateModelType value) => Type == value;

        public static FluidPDFTemplateModel FromDataRow(DataRow dataRow, string modelName = DefaultName) =>
            new(
                modelName: modelName,
                modelType: FluidPDFTemplateModelType.DataRow,
                dataRow: dataRow.GetNonNullOrThrow(nameof(dataRow))
            );

        public static FluidPDFTemplateModel FromDataTable(DataTable dataTable, string modelName = DefaultName) =>
            new(
                modelName: modelName,
                modelType: FluidPDFTemplateModelType.DataTable,
                dataTable: dataTable.GetNonNullOrThrow(nameof(dataTable))
            );

        public static FluidPDFTemplateModel FromDictionary(IDictionary<string, object?> dictionary, string modelName = DefaultName) =>
            new(
                modelName: modelName,
                modelType: FluidPDFTemplateModelType.Dictionary,
                dictionary: dictionary.GetNonNullOrThrow(nameof(dictionary))
            );

        public static FluidPDFTemplateModel FromJsonString(string jsonString, string modelName = DefaultName) =>
            new(
                modelName: modelName,
                modelType: FluidPDFTemplateModelType.JsonNode,
                jsonNode: JsonNode.Parse(jsonString.GetNonNullOrThrow(nameof(jsonString)))
            );

        public static FluidPDFTemplateModel FromArray(IEnumerable<object?> array, string modelName = DefaultName) =>
            new(
                modelName: modelName,
                modelType: FluidPDFTemplateModelType.JsonNode,
                jsonNode: JsonSerializer.SerializeToNode(array.GetNonNullOrThrow(nameof(array)))
            );

        public static FluidPDFTemplateModel FromObject(object obj, string modelName = DefaultName) =>
            new(
                modelName: modelName,
                modelType: FluidPDFTemplateModelType.Object,
                objectValue: obj.GetNonNullOrThrow(nameof(obj))
            );

        public static FluidPDFTemplateModel FromPlainValue(object value, string modelName = DefaultName) =>
            new(
                modelName: modelName,
                modelType: FluidPDFTemplateModelType.PlainValue,
                plainValue: value.GetNonNullOrThrow(nameof(value))
            );

        public static FluidPDFTemplateModel FromJsonNode(JsonNode node, string modelName = DefaultName) =>
            new(
                modelName: modelName,
                modelType: FluidPDFTemplateModelType.JsonNode,
                jsonNode: node.GetNonNullOrThrow(nameof(node))
            );
    }
}
