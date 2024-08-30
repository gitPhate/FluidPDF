using Kyklos.Kernel.Core.Asserts;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FluidPDF.Fluid
{
    public enum FluidModelType
    {
        DataTable = 0,
        DataRow,
        Dictionary,
        JsonString,
        Object,
        PlainValue
    }

    public sealed class FluidModel
    {
        public string Name { get; }
        public FluidModelType Type { get; }

        public DataTable? DataTable { get; }
        public DataRow? DataRow { get; }
        public Dictionary<string, object>? Dictionary { get; }
        public string? JsonString { get; }
        public object? ObjectValue { get; }
        public object? PlainValue { get; }

        public object? Value =>
            Type switch
            {
                FluidModelType.DataTable => DataTable,
                FluidModelType.DataRow => DataRow,
                FluidModelType.Dictionary => Dictionary,
                FluidModelType.JsonString => JsonNode.Parse(JsonString!),
                FluidModelType.Object => JsonNode.Parse(JsonSerializer.Serialize(ObjectValue)),
                FluidModelType.PlainValue => PlainValue,
                _ => null
            };

        private FluidModel
        (
            string modelName,
            FluidModelType kModelType,
            DataTable? dataTable = null,
            DataRow? dataRow = null,
            Dictionary<string, object>? dictionary = null,
            string? jsonString = null,
            object? objectValue = null,
            object? plainValue = null
        )
        {
            Name = modelName;
            Type = kModelType;
            DataTable = dataTable;
            DataRow = dataRow;
            Dictionary = dictionary;
            JsonString = jsonString;
            ObjectValue = objectValue;
            PlainValue = plainValue;
        }

        public bool IsDataTable => IsFluidModelType(FluidModelType.DataTable);
        public bool IsDataRowe => IsFluidModelType(FluidModelType.DataRow);
        public bool IsDictionary => IsFluidModelType(FluidModelType.Dictionary);
        public bool IsJsonString => IsFluidModelType(FluidModelType.JsonString);
        public bool IsPlainValue => IsFluidModelType(FluidModelType.PlainValue);
        public bool IsObject => IsFluidModelType(FluidModelType.Object);

        private bool IsFluidModelType(FluidModelType value) => Type == value;

        public static FluidModel FromDataTable(string modelName, DataTable dataTable) =>
            new(
                modelName: modelName,
                kModelType: FluidModelType.DataTable,
                dataTable: dataTable.GetNonNullOrThrow(nameof(dataTable))
            );

        public static FluidModel FromDataRow(string modelName, DataRow dataRow) =>
            new(
                modelName: modelName,
                kModelType: FluidModelType.DataRow,
                dataRow: dataRow.GetNonNullOrThrow(nameof(dataRow))
            );

        public static FluidModel FromDictionary(string modelName, Dictionary<string, object> dictionary) =>
            new(
                modelName: modelName,
                kModelType: FluidModelType.Dictionary,
                dictionary: dictionary.GetNonNullOrThrow(nameof(dictionary))
            );

        public static FluidModel FromJsonString(string modelName, string jsonString) =>
            new(
                modelName: modelName,
                kModelType: FluidModelType.JsonString,
                jsonString: jsonString.GetNonNullOrThrow(nameof(jsonString))
            );

        public static FluidModel FromObject(string modelName, object obj) =>
            new(
                modelName: modelName,
                kModelType: FluidModelType.Object,
                objectValue: obj.GetNonNullOrThrow(nameof(obj))
            );


        public static FluidModel FromPlainValue(string modelName, object value) =>
            new(
                modelName: modelName,
                kModelType: FluidModelType.PlainValue,
                plainValue: value
            );
    }
}
