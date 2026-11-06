using System;
using System.Collections.Generic;
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
        public string Directory { get; set; } = "";
        public List<ProductionField> Fields { get; set; } = new List<ProductionField>();

        /// <summary>显示用户定义的日志名称，不翻译业务键。</summary>
        public override string ToString() { return Name; }
    }

    /// <summary>机器级日志模块定义；运行读取已提交快照，编辑器操作独立草稿。</summary>
    internal sealed class LogModuleStore
    {
        private readonly object sync = new object();
        private List<LogModuleDefinition> saved;
        private List<LogModuleDefinition> draft;
        internal static string FilePath { get { return Path.Combine(string.IsNullOrEmpty(SysPara.SettingDataDirectory) ? @".\ModuleData\SettingData" : SysPara.SettingDataDirectory, "DataForm.LogModules.xml"); } }

        /// <summary>读取独立配置副本，损坏配置报错并保留原文件。</summary>
        internal List<LogModuleDefinition> Read(bool includeDraft)
        {
            lock (sync)
            {
                if (saved == null)
                {
                    List<LogModuleDefinition> loaded = new List<LogModuleDefinition>();
                    if (File.Exists(FilePath))
                        using (FileStream stream = File.OpenRead(FilePath)) loaded = (List<LogModuleDefinition>)new XmlSerializer(typeof(List<LogModuleDefinition>)).Deserialize(stream);
                    Validate(loaded); saved = loaded;
                }
                return Copy(includeDraft && draft != null ? draft : saved);
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
                Guid id;
                if (module == null || !Guid.TryParseExact(module.Id, "N", out id) || !ids.Add(module.Id) || string.IsNullOrWhiteSpace(module.Name)
                    || !names.Add(module.Name) || string.IsNullOrWhiteSpace(module.Directory) || module.Fields == null || module.Fields.Count == 0)
                    throw new InvalidDataException(FlowText.T("logInvalid"));
                string root = Path.GetFullPath(Path.IsPathRooted(module.Directory) ? module.Directory : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, module.Directory)).TrimEnd(Path.DirectorySeparatorChar);
                if (!paths.Add(root)) throw new InvalidDataException(FlowText.T("logInvalid"));
                HashSet<string> fields = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Time" };
                foreach (ProductionField field in module.Fields)
                    if (field == null || string.IsNullOrWhiteSpace(field.Name) || field.Name.IndexOfAny(new[] { '\r', '\n' }) >= 0 || !fields.Add(field.Name)
                        || !new[] { "Text", "Number", "Boolean" }.Contains(field.Type)) throw new InvalidDataException(FlowText.T("logInvalid"));
            }
        }

        /// <summary>复制定义，防止窗口或流程实例修改存储层集合。</summary>
        private static List<LogModuleDefinition> Copy(IEnumerable<LogModuleDefinition> modules)
        {
            return modules.Select(module => new LogModuleDefinition { Id = module.Id, Name = module.Name, Directory = module.Directory,
                Fields = module.Fields.Select(field => new ProductionField { Name = field.Name, Type = field.Type }).ToList() }).ToList();
        }
    }
}
