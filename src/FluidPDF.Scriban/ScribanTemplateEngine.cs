using FluidPDF.Templating;
using Scriban;
using Scriban.Runtime;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Threading.Tasks;

namespace FluidPDF.Scriban
{
    public sealed class ScribanTemplateEngine : IFluidPDFTemplateEngine
    {
        public async ValueTask<string> RenderTemplateAsync(string template, DataTable model, FluidPDFTemplateRenderOptions options, string modelName = FluidPDFTemplateModel.DefaultName)
        {
            FluidPDFTemplateModel managedModel = FluidPDFTemplateModel.FromDataTable(model, modelName);
            return await RenderTemplateAsync([managedModel], template, options).ConfigureAwait(false);
        }

        public async ValueTask<string> RenderTemplateAsync(string template, IDictionary<string, object> model, FluidPDFTemplateRenderOptions options, string modelName = FluidPDFTemplateModel.DefaultName)
        {
            FluidPDFTemplateModel managedModel = FluidPDFTemplateModel.FromDictionary(model, modelName);
            return await RenderTemplateAsync([managedModel], template, options).ConfigureAwait(false);
        }

        public async ValueTask<string> RenderTemplateAsync(string template, object model, FluidPDFTemplateRenderOptions options, string modelName = FluidPDFTemplateModel.DefaultName)
        {
            FluidPDFTemplateModel managedModel = FluidPDFTemplateModel.FromObject(model, modelName);
            return await RenderTemplateAsync([managedModel], template, options).ConfigureAwait(false);
        }

        public async ValueTask<string> RenderTemplateAsync(string template, string jsonModel, FluidPDFTemplateRenderOptions options, string modelName = FluidPDFTemplateModel.DefaultName)
        {
            FluidPDFTemplateModel managedModel = FluidPDFTemplateModel.FromJsonString(jsonModel, modelName);
            return await RenderTemplateAsync([managedModel], template, options).ConfigureAwait(false);
        }

        public ValueTask<string> RenderTemplateAsync(string template, FluidPDFTemplateModel[] models, FluidPDFTemplateRenderOptions options, string modelName = FluidPDFTemplateModel.DefaultName) =>
            RenderTemplateAsync(models, template, options);

        private static async ValueTask<string> RenderTemplateAsync(FluidPDFTemplateModel[] models, string template, FluidPDFTemplateRenderOptions options)
        {
            ScriptObject mainObject = [];

            foreach (FluidPDFTemplateModel model in models)
            {
                switch (model.Type)
                {
                    case FluidPDFTemplateModelType.PlainValue:
                        mainObject.Add(model.Name, model.PlainValue!);
                        break;
                    default:
                        ScriptObject modelObject = CreateModelScriptObject(model);
                        mainObject.Add(model.Name, modelObject);
                        break;
                }
            }

            TemplateContext context;
            if (options.EncodeHtml)
            {
                context = new HTMLEncodedTemplateContext()
                {
                    MemberRenamer = x => x.Name
                };
            }
            else
            {
                context = new()
                {
                    MemberRenamer = x => x.Name
                };
            }

            if (options.CultureInfo is not null)
            {
                context.PushCulture(options.CultureInfo);
            }

            context.PushGlobal(mainObject);

            Template compiledTemplate = Template.Parse(template);
            string result = await compiledTemplate.RenderAsync(context).ConfigureAwait(false);
            return result;
        }

        private static ScriptObject CreateModelScriptObject(FluidPDFTemplateModel model) =>
            model.Type switch
            {
                FluidPDFTemplateModelType.DataRow => DataRowToScriptObject(model.DataRow!),
                FluidPDFTemplateModelType.DataTable => DataTableToScriptObject(model.DataTable!),
                FluidPDFTemplateModelType.Dictionary => ScriptObject.From(model.Dictionary!),
                FluidPDFTemplateModelType.Object => ScriptObject.From(JsonSerializer.SerializeToElement(model.ObjectValue)),
                FluidPDFTemplateModelType.JsonString => JsonStringToScriptObject(model.JsonString!),
                _ => throw new InvalidOperationException($"Unsupported model type: {model.Type}")
            };

        private static ScriptObject DataRowToScriptObject(DataRow row)
        {
            ScriptObject scriptObject = [];

            foreach (DataColumn column in row.Table.Columns)
            {
                object value = row.IsNull(column) ? null! : row[column];
                scriptObject.Add(column.ColumnName, value);
            }

            return scriptObject;
        }

        private static ScriptObject DataTableToScriptObject(DataTable table)
        {
            ScriptArray rows = [];

            foreach (DataRow row in table.Rows)
            {
                rows.Add(DataRowToScriptObject(row));
            }

            ScriptObject scriptObject = [];
            scriptObject.Add(nameof(DataTable.Rows), rows);
            return scriptObject;
        }

        private static ScriptObject JsonStringToScriptObject(string json)
        {
            JsonElement root = JsonSerializer.Deserialize<JsonElement>(json);
            return ScriptObject.From(root);
        }
    }
}
