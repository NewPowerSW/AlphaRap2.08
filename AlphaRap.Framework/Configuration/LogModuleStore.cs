using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using AlphaRap.FlowProgramming;

namespace AlphaRap.Configuration
{
    public sealed class LogModuleDefinition
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = "Log";
        [DefaultValue("")]
        public string Text { get; set; } = "";
        public string Directory { get; set; } = "";
        [DefaultValue(false)]
        public bool ShowOnHome { get; set; }
        public List<ProductionField> Fields { get; set; } = new List<ProductionField>();

        /// <summary>按模块 ID 读取可见标题。</summary>
        public override string ToString() => FrameworkLanguage.ResourceText("LogResources", "Module_" + Id, Name, Text);

        /// <summary>按日志 ID 和字段名读取可见表头，保留客户日志列名。</summary>
        internal string ColumnText(string name)
        {
            if (string.Equals(name, "Time", StringComparison.OrdinalIgnoreCase))
                return FrameworkLanguage.ResourceText("LogResources", "Time", "Time", null);
            ProductionField field = Fields.Find(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
            return field == null ? name : FrameworkLanguage.ResourceText("LogResources", "Column_" + Id + "_" + field.Name, field.Name, field.Text);
        }

        /// <summary>让未绑定到首页的日志也能在语言设置中找到其字段。</summary>
        internal void RegisterCaptions()
        {
            ToString();
            ColumnText("Time");
            foreach (ProductionField field in Fields) ColumnText(field.Name);
        }
    }

    /// <summary>机器级日志模块定义；运行读取已提交快照，编辑器操作独立草稿。</summary>
    internal sealed class LogModuleStore
    {
        private readonly object sync = new object();
        private List<LogModuleDefinition> saved;
        private List<LogModuleDefinition> draft;
        internal static string FilePath { get { return Path.GetFullPath(Path.Combine(string.IsNullOrEmpty(SysPara.SettingDataDirectory) ? @".\ModuleData\SettingData" : SysPara.SettingDataDirectory, "DataForm.LogModules.xml")); } }

        /// <summary>读取独立配置副本，损坏配置报错并保留原文件。</summary>
        internal List<LogModuleDefinition> Read(bool includeDraft)
        {
            lock (sync)
            {
                if (saved == null) saved = Load(FilePath);
                return Copy(includeDraft && draft != null ? draft : saved);
            }
        }

        /// <summary>完整读取并校验机器日志定义，错误附带实际读取的文件路径。</summary>
        private static List<LogModuleDefinition> Load(string path)
        {
            try
            {
                List<LogModuleDefinition> loaded;
                using (FileStream stream = File.OpenRead(path))
                    loaded = (List<LogModuleDefinition>)new XmlSerializer(typeof(List<LogModuleDefinition>)).Deserialize(stream);
                Validate(loaded);
                return loaded;
            }
            catch (FileNotFoundException) { return new List<LogModuleDefinition>(); }
            catch (DirectoryNotFoundException) { return new List<LogModuleDefinition>(); }
            catch (Exception error) when (error is InvalidDataException || error is IOException ||
                error is UnauthorizedAccessException || error is InvalidOperationException)
            {
                string details = error.Message;
                if (error.InnerException != null) details += Environment.NewLine + error.InnerException.Message;
                throw new InvalidDataException(string.Format(FlowText.T("logFileReadError"), path, details), error);
            }
        }

        /// <summary>提交窗口草稿，磁盘写入仍由 MainForm 保存事务负责。</summary>
        internal void SetDraft(List<LogModuleDefinition> definitions) { Validate(definitions); lock (sync) draft = Copy(definitions); }
        /// <summary>主保存前检查全部模块及字段。</summary>
        internal void ValidateDraft() { Validate(Read(true)); }
        /// <summary>写入主保存的暂存配置文件。</summary>
        internal void WriteDraft(string path) { using (FileStream stream = File.Create(path)) new XmlSerializer(typeof(List<LogModuleDefinition>)).Serialize(stream, Read(true)); }
        /// <summary>文件事务完成后发布运行定义。</summary>
        internal void Accept() { lock (sync) { saved = Read(true); draft = null; } }
        /// <summary>放弃未保存的配置修改。</summary>
        internal void Discard() { lock (sync) draft = null; }

        /// <summary>校验稳定身份、目录及用户字段，保留 Time 作为自动时间列。</summary>
        private static void Validate(List<LogModuleDefinition> definitions)
        {
            if (definitions == null) throw new InvalidDataException(FlowText.T("logInvalid"));
            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
            HashSet<string> names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (LogModuleDefinition module in definitions)
            {
                ValidateModule(module, ids, names, paths);
            }
            string[] homeModules = definitions.Where(module => module.ShowOnHome).Select(module => module.Name).ToArray();
            if (homeModules.Length > 1)
                throw new InvalidDataException(string.Format(FlowText.T("logHomeConflict"), string.Join(", ", homeModules)));
        }

        /// <summary>校验单个模块及其字段，保留模块名称和稳定身份供故障定位。</summary>
        private static void ValidateModule(LogModuleDefinition module, HashSet<string> ids, HashSet<string> names, HashSet<string> paths)
        {
            if (module == null) throw new InvalidDataException(FlowText.T("logInvalid"));
            string invalid = string.Format(FlowText.T("logModuleInvalid"), module.Name, module.Id);
            Guid id;
            if (!Guid.TryParseExact(module.Id, "N", out id) || !ids.Add(module.Id) || string.IsNullOrWhiteSpace(module.Name) ||
                !names.Add(module.Name) || string.IsNullOrWhiteSpace(module.Directory) || module.Fields == null || module.Fields.Count == 0)
                throw new InvalidDataException(invalid);
            string root;
            try { root = ResolveDirectory(module.Directory); }
            catch (Exception error) when (error is ArgumentException || error is NotSupportedException || error is PathTooLongException)
            {
                throw new InvalidDataException(invalid, error);
            }
            if (!paths.Add(root)) throw new InvalidDataException(invalid);
            var fields = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Time" };
            foreach (ProductionField field in module.Fields)
            {
                if (field == null || string.IsNullOrWhiteSpace(field.Name) || field.Name.IndexOfAny(new[] { '\r', '\n' }) >= 0 ||
                    !fields.Add(field.Name) || !new[] { "Text", "Number", "Boolean" }.Contains(field.Type))
                    throw new InvalidDataException(string.Format(FlowText.T("logFieldInvalid"), module.Name, field?.Name));
            }
        }

        /// <summary>相对日志目录以程序目录为基准，用完整路径比较目录身份。</summary>
        internal static string ResolveDirectory(string path)
        {
            return Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path)).TrimEnd(Path.DirectorySeparatorChar);
        }

        /// <summary>复制定义，防止窗口或流程实例修改存储层集合。</summary>
        private static List<LogModuleDefinition> Copy(IEnumerable<LogModuleDefinition> modules)
        {
            return modules.Select(module => new LogModuleDefinition { Id = module.Id, Name = module.Name, Text = module.Text, Directory = module.Directory, ShowOnHome = module.ShowOnHome,
                Fields = module.Fields.Select(field => new ProductionField { Name = field.Name, Text = field.Text, Type = field.Type }).ToList() }).ToList();
        }
    }
}
