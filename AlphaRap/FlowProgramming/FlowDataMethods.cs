using System;
using System.Collections.Generic;
using System.Linq;
using NPSDK;
using AlphaRap.FlowProgramming;

namespace AlphaRap
{
    public partial class DataForm : IFlowObject
    {
        /// <summary>生产保存方法的输入字段直接来自 DataForm，编辑器可读草稿而运行只读已保存定义。</summary>
        public IEnumerable<FlowMethod> GetFlowMethods(bool includeDraft = false)
        {
            List<ProductionField> fields = includeDraft ? GetProductionDraft() : GetProductionSchema();
            yield return FlowMethods.Sync("data.save", "保存 DataForm 生产记录", "Save DataForm production record", "Guardar registro de producción",
                fields.Select(field => new FlowField(field.Name, field.Type == "Number" ? typeof(double) : field.Type == "Boolean" ? typeof(bool) : typeof(string))).ToArray(),
                new[] { new FlowField("RecordId", typeof(string)) { DisplayName = new[] { "记录编号", "Record identifier", "Identificador de registro" } },
                    new FlowField("Path", typeof(string)) { DisplayName = new[] { "记录文件路径", "Record file path", "Ruta del archivo de registro" } } }, call => {
                    string invocation = string.IsNullOrEmpty(call.InvocationId) ? call.Context.ExecutionId : call.InvocationId;
                    string id = call.Context.RunId + "/" + call.Context.ModuleId + "/" + invocation;
                    string path = SaveProductionRecord(id, call.Context.CycleId, call.Context.RecipeName, call.Inputs, call.Simulation);
                    return new Dictionary<string, object> { { "RecordId", id }, { "Path", path } };
                }, false);
        }
    }
}
