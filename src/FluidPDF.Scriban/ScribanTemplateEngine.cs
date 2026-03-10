using FluidPDF.Templating;
using Scriban;
using Scriban.Runtime;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Threading.Tasks;

namespace FluidPDF.Scriban
{
    public class ScribanTemplateEngine : IFluidPDFTemplateEngine
    {
        private async ValueTask<string> RenderTemplateAsync(FluidPDFTemplateModel[] models, string template, FluidPDFTemplateRenderOptions options)
        {
            ScriptObject mainObject = [];

            foreach (FluidPDFTemplateModel model in models)
            {
                ScriptObject modelObject = CreateModelScriptObject(model);
                mainObject.Add(options.ModelName, modelObject);
            }

            TemplateContext context =
                new()
                {
                    MemberRenamer = x => x.Name
                };

            if (options.CultureInfo is not null)
            {
                context.PushCulture(options.CultureInfo);
            }

            context.PushGlobal(mainObject);

            Template compiledTemplate = Template.Parse(template);
            string result = await compiledTemplate.RenderAsync(context).ConfigureAwait(false);
            return result;
        }

        public async ValueTask<string> RenderTemplateAsync(string template, DataTable model, FluidPDFTemplateRenderOptions options)
        {
            FluidPDFTemplateModel managedModel = FluidPDFTemplateModel.FromDataTable(options.ModelName, model);
            return await RenderTemplateAsync([managedModel], template, options).ConfigureAwait(false);
        }

        public async ValueTask<string> RenderTemplateAsync(string template, IDictionary<string, object> model, FluidPDFTemplateRenderOptions options)
        {
            FluidPDFTemplateModel managedModel = FluidPDFTemplateModel.FromDictionary(options.ModelName, model);
            return await RenderTemplateAsync([managedModel], template, options).ConfigureAwait(false);
        }

        public async ValueTask<string> RenderTemplateAsync(string template, object model, FluidPDFTemplateRenderOptions options)
        {
            FluidPDFTemplateModel managedModel = FluidPDFTemplateModel.FromObject(options.ModelName, model);
            return await RenderTemplateAsync([managedModel], template, options).ConfigureAwait(false);
        }

        public async ValueTask<string> RenderTemplateAsync(string template, FluidPDFTemplateModel[] models, FluidPDFTemplateRenderOptions options)
        {
            return await RenderTemplateAsync(models, template, options).ConfigureAwait(false);
        }

        private ScriptObject CreateModelScriptObject(FluidPDFTemplateModel model) =>
            model.Type switch
            {
                FluidPDFTemplateModelType.DataRow    => DataRowToScriptObject(model.DataRow!),
                FluidPDFTemplateModelType.DataTable  => DataTableToScriptObject(model.DataTable!),
                FluidPDFTemplateModelType.Dictionary => ScriptObject.From(model.Dictionary!),
                FluidPDFTemplateModelType.Object     => ScriptObject.From(JsonSerializer.SerializeToElement(model.ObjectValue)),
                _                                    => throw new System.InvalidOperationException($"Unsupported model type: {model.Type}")
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
    }
}
