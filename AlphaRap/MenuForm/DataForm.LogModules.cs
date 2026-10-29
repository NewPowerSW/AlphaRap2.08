using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AlphaRap.Configuration;
using AlphaRap.FlowProgramming;
using DataSave;

namespace AlphaRap
{
    public partial class DataForm
    {
        private readonly LogModuleStore logStore = new LogModuleStore();
        private readonly List<TypeCollection1> customLogs = new List<TypeCollection1>();

        /// <summary>读取日志配置独立副本，模块身份不随名称或配方改变。</summary>
        public List<LogModuleDefinition> GetLogModules(bool includeDraft = true) { return logStore.Read(includeDraft); }

        /// <summary>保留内置报警、MES 和操作日志，动态模块只追加到数据查询入口。</summary>
        private IEnumerable<KeyValuePair<string, TypeCollection1>> BuiltinLogs()
        {
            yield return new KeyValuePair<string, TypeCollection1>("MESLOG", MESLOG);
            yield return new KeyValuePair<string, TypeCollection1>("Alarm", Alarm);
            yield return new KeyValuePair<string, TypeCollection1>("LogRun", LogRun);
            yield return new KeyValuePair<string, TypeCollection1>("LogError", LogError);
            yield return new KeyValuePair<string, TypeCollection1>("UserLogin", UserLogin);
            yield return new KeyValuePair<string, TypeCollection1>("OperationLog", UserOperationLog);
        }

        /// <summary>按实例提供日志模块能力；编辑读取草稿，运行只读取已保存定义。</summary>
        public IEnumerable<FlowLogModule> GetFlowLogs(bool includeDraft = false)
        {
            foreach (KeyValuePair<string, TypeCollection1> entry in BuiltinLogs())
                yield return new FlowLogModule("builtin/" + entry.Key, entry.Value,
                    entry.Value.Items.Skip(1).Select(field => new ProductionField { Name = field.Name, Type = "Text" }));
            List<LogModuleDefinition> definitions = logStore.Read(includeDraft); ValidateBuiltinLogs(definitions);
            foreach (LogModuleDefinition definition in definitions)
                yield return new FlowLogModule(definition.Id, FlowLogModule.Create(definition), definition.Fields);
        }

        /// <summary>提交窗口草稿并刷新查询模块，尚未保存的定义不供运行流程使用。</summary>
        public void SetLogModulesDraft(List<LogModuleDefinition> definitions)
        {
            FlowPrograms.RequireIdle();
            if (definitions == null) throw new InvalidOperationException(FlowText.T("logInvalid"));
            ValidateBuiltinLogs(definitions);
            HashSet<string> retained = new HashSet<string>(definitions.Select(module => "log/" + module.Id));
            foreach (FlowModuleEditor module in FlowPrograms.Modules.Where(module => !module.Program.Removed))
                if (module.Program.Steps.Any(step => step.Target != null && step.Target.StartsWith("log/") && !step.Target.StartsWith("log/builtin/") && !retained.Contains(step.Target)))
                    throw new InvalidOperationException(FlowText.T("logInUse"));
            logStore.SetDraft(definitions); RefreshLogSources(true);
        }

        /// <summary>校验并暂存日志草稿，由主保存事务决定何时发布。</summary>
        internal void ValidateLogModulesDraft() { logStore.ValidateDraft(); ValidateBuiltinLogs(logStore.Read(true)); }
        /// <summary>将日志定义写入主保存的暂存文件。</summary>
        internal void WriteLogModulesDraft(string path) { logStore.WriteDraft(path); }
        /// <summary>主保存完成后发布日志定义并刷新查询入口。</summary>
        internal void AcceptLogModulesDraft() { logStore.Accept(); RefreshLogSources(false); }
        /// <summary>放弃未保存的日志修改并恢复查询入口。</summary>
        internal void DiscardLogModulesDraft() { logStore.Discard(); RefreshLogSources(false); }

        /// <summary>自定义模块不能覆盖内置报警或其他系统日志的名称与目录。</summary>
        private void ValidateBuiltinLogs(IEnumerable<LogModuleDefinition> definitions)
        {
            foreach (LogModuleDefinition module in definitions)
            {
                if (module == null || string.IsNullOrWhiteSpace(module.Directory)) throw new InvalidOperationException(FlowText.T("logInvalid"));
                string path = LogDirectory(module.Directory);
                if (BuiltinLogs().Any(entry => string.Equals(entry.Value.Text, module.Name, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(LogDirectory(entry.Value.SavePath), path, StringComparison.OrdinalIgnoreCase))) throw new InvalidOperationException(FlowText.T("logInvalid"));
            }
        }

        /// <summary>统一相对目录基准并比较日志目录身份。</summary>
        private static string LogDirectory(string path)
        {
            return System.IO.Path.GetFullPath(System.IO.Path.IsPathRooted(path) ? path : System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path)).TrimEnd(System.IO.Path.DirectorySeparatorChar);
        }

        /// <summary>保持内置日志实例，替换动态查询对象并释放旧组件。</summary>
        private void RefreshLogSources(bool includeDraft)
        {
            data1.ParameterSet.Clear();
            foreach (KeyValuePair<string, TypeCollection1> entry in BuiltinLogs()) data1.ParameterSet.Add(entry.Value);
            foreach (TypeCollection1 log in customLogs) log.Dispose();
            customLogs.Clear();
            List<LogModuleDefinition> definitions = logStore.Read(includeDraft); ValidateBuiltinLogs(definitions);
            foreach (LogModuleDefinition definition in definitions)
            {
                TypeCollection1 log = FlowLogModule.Create(definition); customLogs.Add(log); data1.ParameterSet.Add(log);
            }
            data1.ReloadSources();
        }

        /// <summary>打开单层模块与字段编辑窗口；确定只更新草稿。</summary>
        private void ManageLogModules(object sender, EventArgs args)
        {
            try
            {
                FlowPrograms.RequireIdle();
                using (LogModulesForm dialog = new LogModulesForm(this)) dialog.ShowDialog(this);
            }
            catch (Exception error) { MessageBox.Show(string.Format(FlowText.T("error"), error.Message), FlowText.T("log")); }
        }
    }
}
