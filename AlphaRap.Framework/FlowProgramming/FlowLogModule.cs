using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AlphaRap.Configuration;
using DataSave;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    /// <summary>将一个已有日志实例的字段和写入能力提供给工具箱。</summary>
    public sealed class FlowLogModule : IFlowObject
    {
        private readonly TypeCollection1 log;
        private readonly List<ProductionField> fields;
        public string Id { get; private set; }
        public string Name { get { return log.Text; } }

        /// <summary>绑定实际查询与写入使用的日志实例，不创建设备或执行日志。</summary>
        internal FlowLogModule(string id, TypeCollection1 instance, IEnumerable<ProductionField> schema)
        {
            Id = "log/" + id; log = instance; fields = schema.ToList();
        }

        /// <summary>字段直接来自选定日志模块，支持固定值、常量及其他节点输出绑定。</summary>
        public IEnumerable<FlowMethod> GetFlowMethods(bool includeDraft = false)
        {
            yield return FlowMethods.Sync("log.append", "写入一条日志", "Write a log record", "Escribir un registro",
                fields.Select(field => new FlowField(field.Name, field.Type == "Number" ? typeof(double) : field.Type == "Boolean" ? typeof(bool) : typeof(string))).ToArray(),
                new[] { new FlowField("Path", typeof(string)) { DisplayName = new[] { "日志文件路径", "Log file path", "Ruta del registro" } } }, Append, false);
        }

        /// <summary>按声明类型转换输入并完成落盘，模拟运行写入独立子目录。</summary>
        private IDictionary<string, object> Append(FlowCall call)
        {
            string[] values = fields.Select(field => {
                object value;
                if (!call.Inputs.TryGetValue(field.Name, out value)) throw new InvalidDataException(FlowText.T("logMissingField") + " " + field.Name);
                if (field.Type == "Number") value = FlowActions.Number(value);
                if (field.Type == "Boolean") value = FlowActions.Boolean(value);
                return FlowValue.Text(value);
            }).ToArray();
            call.Token.ThrowIfCancellationRequested();
            if (!call.Simulation) return new Dictionary<string, object> { { "Path", log.AppendRecord(values) } };
            using (TypeCollection1 preview = new TypeCollection1 { SavePath = Path.Combine(log.SavePath, "Simulation") })
            {
                preview.Items.AddRange(log.Items.Select(item => new DataFormat { Name = item.Name }));
                return new Dictionary<string, object> { { "Path", preview.AppendRecord(values) } };
            }
        }

        /// <summary>从自定义模块定义创建与现有 Data 控件兼容的查询对象。</summary>
        internal static TypeCollection1 Create(LogModuleDefinition definition)
        {
            TypeCollection1 log = new TypeCollection1 { Text = definition.Name, SavePath = definition.Directory };
            log.Items.Add(new DataFormat { Name = "Time" });
            log.Items.AddRange(definition.Fields.Select(field => new DataFormat { Name = field.Name }));
            return log;
        }
    }
}
