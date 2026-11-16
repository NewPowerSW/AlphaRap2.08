using System;
using System.Collections.Generic;
using System.Linq;

namespace AlphaRap.Configuration
{
    /// <summary>配置功能的扩展入口，准备阶段不写正式文件或接受草稿。</summary>
    public interface IConfigurationParticipant
    {
        string ConfigurationId { get; }
        int ConfigurationOrder { get; }
        /// <summary>校验编辑草稿，不改变已保存版本。</summary>
        void ValidateConfiguration();
        /// <summary>捕获写入任务及全部落盘成功后才执行的接受动作。</summary>
        ConfigurationChange PrepareConfigurationSave();
        /// <summary>预检放弃修改，返回延迟执行的恢复动作。</summary>
        Action PrepareConfigurationDiscard();
    }

    /// <summary>不可变的目标文件声明，写入委托只接收事务生成的暂存路径。</summary>
    public sealed class ConfigurationFile
    {
        public string Path { get; private set; }
        private readonly Action<string> write;
        private readonly bool checkOriginal;
        private readonly byte[] expectedOriginal;

        /// <summary>声明一般配置写入，不在构造时进行 IO。</summary>
        public ConfigurationFile(string path, Action<string> write) : this(path, write, false, null) { }

        /// <summary>固定文件身份、序列化入口和可选的外部修改检查快照。</summary>
        private ConfigurationFile(string path, Action<string> write, bool checkOriginal, byte[] expectedOriginal)
        {
            Path = System.IO.Path.GetFullPath(path);
            this.write = write ?? throw new ArgumentNullException(nameof(write));
            this.checkOriginal = checkOriginal;
            this.expectedOriginal = expectedOriginal == null ? null : (byte[])expectedOriginal.Clone();
        }

        /// <summary>要求目标仍与捕获内容一致；null 表示准备时要求文件不存在。</summary>
        public static ConfigurationFile CompareAndReplace(string path, byte[] expectedOriginal, Action<string> write)
        {
            return new ConfigurationFile(path, write, true, expectedOriginal);
        }

        /// <summary>保留已有源码生成任务的并发修改检查，兼容既有事务实现。</summary>
        internal static ConfigurationFile FromPending(ProjectConfiguration.PendingFile file)
        {
            return new ConfigurationFile(file.Path, file.Write, file.CheckExpectedOriginal, file.ExpectedOriginal);
        }

        /// <summary>创建本次事务独有的执行状态，不把暂存状态暴露给参与者。</summary>
        internal ProjectConfiguration.PendingFile CreatePending()
        {
            return new ProjectConfiguration.PendingFile { Path = Path, Write = write, CheckExpectedOriginal = checkOriginal,
                ExpectedOriginal = expectedOriginal == null ? null : (byte[])expectedOriginal.Clone() };
        }
    }

    /// <summary>一份保存快照的文件与接受动作，事务失败时不调用接受动作。</summary>
    public sealed class ConfigurationChange
    {
        public IReadOnlyList<ConfigurationFile> Files { get; private set; }
        private readonly Action accept;

        /// <summary>捕获文件集合，空集合可用于仅接受已校验内存版本的功能。</summary>
        public ConfigurationChange(IEnumerable<ConfigurationFile> files, Action accept = null)
        {
            if (files == null) throw new ArgumentNullException(nameof(files));
            ConfigurationFile[] captured = files.ToArray();
            if (captured.Any(file => file == null)) throw new ArgumentException(nameof(files));
            Files = Array.AsReadOnly(captured);
            this.accept = accept;
        }

        /// <summary>适配尚使用内部文件任务的既有配置功能。</summary>
        internal static ConfigurationChange FromPending(IEnumerable<ProjectConfiguration.PendingFile> files, Action accept = null)
        {
            return new ConfigurationChange(files.Where(file => file != null).Select(ConfigurationFile.FromPending), accept);
        }

        /// <summary>所有参与者的文件成功提交后接受本次快照。</summary>
        internal void AcceptSaved() { accept?.Invoke(); }
    }

    internal static class ConfigurationText
    {
        internal static string RecoveryBlocked { get { return FrameworkLanguage.LangMsg("ProjectConfiguration", "recoveryBlocked", "配置状态未能完整提交或恢复，已禁止运行与保存。请检查错误并重启。", "Configuration could not be completely committed or restored. Running and saving are blocked; inspect the error and restart.", "La configuración no pudo confirmarse o restaurarse por completo. Ejecución y guardado bloqueados; revise el error y reinicie."); } }
        internal static string RecipeDraft { get { return FrameworkLanguage.LangMsg("ProjectConfiguration", "recipeDraft", "配方有未保存修改，请先通过主保存处理。", "The recipe has unsaved changes. Use the main Save button first.", "La receta tiene cambios sin guardar. Use Guardar principal primero."); } }
        internal static string RecipeMissing { get { return FrameworkLanguage.LangMsg("ProjectConfiguration", "recipeMissing", "配方文件不存在。", "Recipe file is missing.", "Falta el archivo de receta."); } }
        internal static string RecoveryConflict { get { return FrameworkLanguage.LangMsg("ProjectConfiguration", "recoveryConflict", "配置事务存在无法确认的文件变更，请保留事务记录并修复后重启。", "Configuration recovery found an unknown file version. Preserve the transaction and repair it before restart.", "La recuperación encontró una versión de archivo desconocida. Conserve la transacción y repárela antes de reiniciar."); } }
        internal static string PublicationFailed { get { return FrameworkLanguage.LangMsg("ProjectConfiguration", "publicationFailed", "配置文件已提交，但内存发布未完成。已禁止运行，请重启以加载完整提交版本。", "Configuration files committed but publication failed. Running is blocked; restart to load the complete committed version.", "Los archivos se confirmaron pero la publicación falló. La ejecución está bloqueada; reinicie para cargar la versión confirmada completa."); } }
        internal static string Busy { get { return FrameworkLanguage.LangMsg("ProjectConfiguration", "busy", "正在保存或放弃配置，请等待当前操作结束。", "A configuration save or discard is already in progress.", "Ya hay un guardado o descarte de configuración en curso."); } }
        internal static string InvalidId { get { return FrameworkLanguage.LangMsg("ProjectConfiguration", "participantId", "配置参与者标识不能为空。", "Configuration participant ID is required.", "Se requiere el ID del participante de configuración."); } }
        internal static string DuplicateId { get { return FrameworkLanguage.LangMsg("ProjectConfiguration", "duplicateParticipant", "配置参与者标识重复：{0}", "Duplicate configuration participant ID: {0}", "ID de participante de configuración duplicado: {0}"); } }
        internal static string MissingChange { get { return FrameworkLanguage.LangMsg("ProjectConfiguration", "missingChange", "配置参与者没有提供保存快照。", "A configuration participant did not provide a save snapshot.", "Un participante de configuración no proporcionó una instantánea de guardado."); } }
    }
}
