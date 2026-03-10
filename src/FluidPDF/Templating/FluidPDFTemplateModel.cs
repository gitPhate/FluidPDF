using FluidPDF.Support;
using System;
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
        JsonString,
        Object,
        PlainValue
    }

    public sealed class FluidPDFTemplateModel
    {
        public string Name { get; }
        public FluidPDFTemplateModelType Type { get; }
        public DataTable? DataTable { get; }
        public DataRow? DataRow { get; }
        public IDictionary<string, object>? Dictionary { get; }
        public string? JsonString { get; }
        public object? ObjectValue { get; }
        public object? PlainValue { get; }

        public object? Value =>
            Type switch
            {
                FluidPDFTemplateModelType.DataRow => DataRow,
                FluidPDFTemplateModelType.DataTable => DataTable,
                FluidPDFTemplateModelType.Dictionary => Dictionary,
                FluidPDFTemplateModelType.JsonString => JsonNode.Parse(JsonString!),
                FluidPDFTemplateModelType.Object => JsonSerializer.SerializeToNode(ObjectValue),
                FluidPDFTemplateModelType.PlainValue => PlainValue,
                _ => throw new InvalidOperationException($"Invalid {nameof(FluidPDFTemplateModelType)}")
            };

        private FluidPDFTemplateModel
        (
            string modelName,
            FluidPDFTemplateModelType modelType,
            DataRow? dataRow = null,
            DataTable? dataTable = null,
            IDictionary<string, object>? dictionary = null,
            string? jsonString = null,
            object? objectValue = null,
            object? plainValue = null
        )
        {
            Name = modelName;
            Type = modelType;
            DataRow = dataRow;
            DataTable = dataTable;
            Dictionary = dictionary;
            JsonString = jsonString;
            ObjectValue = objectValue;
            PlainValue = plainValue;
        }

        public bool IsDataRow => IsFluidPDFTemplateModelType(FluidPDFTemplateModelType.DataRow);
        public bool IsDataTable => IsFluidPDFTemplateModelType(FluidPDFTemplateModelType.DataTable);
        public bool IsDictionary => IsFluidPDFTemplateModelType(FluidPDFTemplateModelType.Dictionary);
        public bool IsJsonString => IsFluidPDFTemplateModelType(FluidPDFTemplateModelType.JsonString);
        public bool IsPlainValue => IsFluidPDFTemplateModelType(FluidPDFTemplateModelType.PlainValue);
        public bool IsObject => IsFluidPDFTemplateModelType(FluidPDFTemplateModelType.Object);

        private bool IsFluidPDFTemplateModelType(FluidPDFTemplateModelType value) => Type == value;

        public static FluidPDFTemplateModel FromDataRow(string modelName, DataRow dataRow) =>
            new(
                modelName: modelName,
                modelType: FluidPDFTemplateModelType.DataRow,
                dataRow: dataRow.GetNonNullOrThrow(nameof(dataRow))
            );

        public static FluidPDFTemplateModel FromDataTable(string modelName, DataTable dataTable) =>
            new(
                modelName: modelName,
                modelType: FluidPDFTemplateModelType.DataTable,
                dataTable: dataTable.GetNonNullOrThrow(nameof(dataTable))
            );

        public static FluidPDFTemplateModel FromDictionary(string modelName, IDictionary<string, object> dictionary) =>
            new(
                modelName: modelName,
                modelType: FluidPDFTemplateModelType.Dictionary,
                dictionary: dictionary.GetNonNullOrThrow(nameof(dictionary))
            );

        public static FluidPDFTemplateModel FromJsonString(string modelName, string jsonString) =>
            new(
                modelName: modelName,
                modelType: FluidPDFTemplateModelType.JsonString,
                jsonString: jsonString.GetNonNullOrThrow(nameof(jsonString))
            );

        public static FluidPDFTemplateModel FromObject(string modelName, object obj) =>
            new(
                modelName: modelName,
                modelType: FluidPDFTemplateModelType.Object,
                objectValue: obj.GetNonNullOrThrow(nameof(obj))
            );

        public static FluidPDFTemplateModel FromPlainValue(string modelName, object value) =>
            new(
                modelName: modelName,
                modelType: FluidPDFTemplateModelType.PlainValue,
                plainValue: value
            );
    }
}
