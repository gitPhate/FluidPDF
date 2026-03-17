using FluidPDF.Support;
using System.Collections.Generic;
using System.Data;

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
        public const string DefaultName = "Model";

        public string Name { get; }
        public FluidPDFTemplateModelType Type { get; }
        public DataTable? DataTable { get; }
        public DataRow? DataRow { get; }
        public IDictionary<string, object>? Dictionary { get; }
        public string? JsonString { get; }
        public object? ObjectValue { get; }
        public object? PlainValue { get; }

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

        public static FluidPDFTemplateModel FromDictionary(IDictionary<string, object> dictionary, string modelName = DefaultName) =>
            new(
                modelName: modelName,
                modelType: FluidPDFTemplateModelType.Dictionary,
                dictionary: dictionary.GetNonNullOrThrow(nameof(dictionary))
            );

        public static FluidPDFTemplateModel FromJsonString(string jsonString, string modelName = DefaultName) =>
            new(
                modelName: modelName,
                modelType: FluidPDFTemplateModelType.JsonString,
                jsonString: jsonString.GetNonNullOrThrow(nameof(jsonString))
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
                plainValue: value
            );
    }
}
