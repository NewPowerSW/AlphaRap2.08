using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using AlphaRap.FlowProgramming;
using AlphaRapLibrary;
using NPSDK;

namespace AlphaRap.Configuration
{
    public static class ProjectConfiguration
    {
        internal sealed class PendingFile
        {
            public string Path;
            public Action<string> Write;
            public string Temporary;
            public byte[] Original;
            public byte[] ExpectedOriginal;
            public bool CheckExpectedOriginal;
            public byte[] PreparedContents;
        }

        private static int operation;
        public static bool IsBusy { get { return System.Threading.Volatile.Read(ref operation) != 0; } }
        private static volatile bool recoveryRequired;
        public static bool RecoveryRequired { get { return recoveryRequired; } private set { recoveryRequired = value; } }

        /// <summary>主保存前按配置能力接口检查草稿，不识别具体页面类型。</summary>
        public static void Validate() { FlowPrograms.RequireIdle(); Validate(Participants()); }

        /// <summary>固定本次参与者和快照，所有文件提交后才接受内存版本。</summary>
        public static void Save()
        {
            BeginOperation();
            try
            {
                FlowPrograms.RequireIdle();
                IConfigurationParticipant[] participants = Participants();
                Validate(participants);
                ConfigurationChange[] changes = CaptureChanges(participants);
                ConfigurationJournal journal = CommitFiles(BuildFiles(changes));
                try
                {
                    foreach (ConfigurationChange change in changes) change.AcceptSaved();
                    journal.MarkPublished();
                }
                catch (Exception error)
                {
                    RecoveryRequired = true; MachineState.Global.BeginStop();
                    throw new InvalidOperationException(ConfigurationText.PublicationFailed, error);
                }
            }
            finally { System.Threading.Volatile.Write(ref operation, 0); }
        }

        /// <summary>拒绝保存或放弃回调中的重入，避免嵌套事务覆盖当前文件和草稿。</summary>
        private static void BeginOperation()
        {
            if (RecoveryRequired) throw new InvalidOperationException(ConfigurationText.RecoveryBlocked);
            if (System.Threading.Interlocked.CompareExchange(ref operation, 1, 0) != 0)
                throw new InvalidOperationException(ConfigurationText.Busy);
        }

        /// <summary>无法证明配置状态完整时锁住运行和后续配置操作，普通停止完成不能解除。</summary>
        internal static void RequireRecovery() { RecoveryRequired = true; MachineState.Global.BeginStop(); }

        /// <summary>在任何写入准备之前校验本次操作的全部参与者。</summary>
        private static void Validate(IEnumerable<IConfigurationParticipant> participants)
        {
            foreach (IConfigurationParticipant participant in participants) participant.ValidateConfiguration();
        }

        /// <summary>捕获兼容事务清单，供离线故障注入检查目标文件。</summary>
        private static List<PendingFile> PrepareFiles() { return BuildFiles(CaptureChanges(Participants())); }

        /// <summary>准备全部保存快照；任何准备失败都不写正式文件或接受草稿。</summary>
        private static ConfigurationChange[] CaptureChanges(IEnumerable<IConfigurationParticipant> participants)
        {
            return participants.Select(participant => participant.PrepareConfigurationSave()
                ?? throw new InvalidOperationException(ConfigurationText.MissingChange)).ToArray();
        }

        /// <summary>合并声明并拒绝目标路径冲突，执行状态不与配置提供方共享。</summary>
        private static List<PendingFile> BuildFiles(IEnumerable<ConfigurationChange> changes)
        {
            List<PendingFile> files = changes.SelectMany(change => change.Files).Select(file => file.CreatePending()).ToList();
            if (files.GroupBy(file => file.Path, StringComparer.OrdinalIgnoreCase).Any(group => group.Count() > 1))
                throw new IOException(HardwareText.T("duplicatePath"));
            return files;
        }

        /// <summary>发现已登记页面和业务模块声明的配置能力，顺序与身份在操作开始时固定。</summary>
        private static IConfigurationParticipant[] Participants()
        {
            IConfigurationParticipant[] participants = FrameworkConfigurationParticipants.Capture()
                .Concat(ApplicationHost.ModulePages.Where(page => !page.IsDisposed).OfType<IConfigurationParticipant>())
                .Concat(ApplicationHost.Modules.Modules.OfType<IConfigurationParticipant>()).Distinct().ToArray();
            if (participants.Any(participant => string.IsNullOrWhiteSpace(participant.ConfigurationId)))
                throw new InvalidOperationException(ConfigurationText.InvalidId);
            var duplicate = participants.GroupBy(participant => participant.ConfigurationId, StringComparer.Ordinal).FirstOrDefault(group => group.Count() > 1);
            if (duplicate != null) throw new InvalidOperationException(string.Format(ConfigurationText.DuplicateId, duplicate.Key));
            return participants.OrderBy(participant => participant.ConfigurationOrder).ThenBy(participant => participant.ConfigurationId, StringComparer.Ordinal).ToArray();
        }
        /// <summary>暂存完成后替换正式文件，失败时恢复已经替换的文件。</summary>
        private static void WriteFiles(List<PendingFile> files)
        {
            CommitFiles(files).MarkPublished();
        }

        /// <summary>持久记录覆盖所有替换阶段，已提交后的发布失败保留记录供重启恢复。</summary>
        private static ConfigurationJournal CommitFiles(List<PendingFile> files)
        {
            ConfigurationJournal journal = null;
            try
            {
                ConfigurationJournal.RecoverPending();
                foreach (PendingFile file in files) StageFile(file);
                journal = ConfigurationJournal.Prepare(files);
                for (int i = 0; i < files.Count; i++)
                {
                    journal.BeforeReplace(i); CommitFile(files[i]); journal.AfterReplace(i);
                }
                journal.MarkCommitted();
                return journal;
            }
            catch (Exception error)
            {
                List<Exception> errors = new List<Exception> { error };
                if (journal != null && !journal.IsCommitted)
                    try { journal.Rollback(); } catch (Exception restore) { errors.Add(restore); RecoveryRequired = true; MachineState.Global.BeginStop(); }
                if (journal != null && journal.IsCommitted) { RecoveryRequired = true; MachineState.Global.BeginStop(); }
                if (errors.Count > 1) throw new AggregateException(errors);
                throw;
            }
            finally { foreach (PendingFile file in files) DeleteTemporaryFile(file); }
        }

        /// <summary>在任何配置模型读取之前恢复磁盘事务；仅用于新应用生命周期。</summary>
        internal static void RecoverAtStartup() { ConfigurationJournal.RecoverPending(); RecoveryRequired = false; }

        /// <summary>配方选择等框架操作复用持久文件事务，不发布其他配置草稿。</summary>
        public static void CommitFileChanges(IEnumerable<ConfigurationFile> files)
        {
            BeginOperation();
            try { WriteFiles(BuildFiles(new[] { new ConfigurationChange(files) })); }
            finally { System.Threading.Volatile.Write(ref operation, 0); }
        }

        /// <summary>使用同目录短名称和原扩展名暂存文件，不提交内存版本。</summary>
        private static void StageFile(PendingFile file)
        {
            string directory = System.IO.Path.GetDirectoryName(file.Path);
            Directory.CreateDirectory(directory);
            file.Original = File.Exists(file.Path) ? File.ReadAllBytes(file.Path) : null;
            CheckOriginal(file);
            file.Temporary = System.IO.Path.Combine(directory,Guid.NewGuid().ToString("N")+System.IO.Path.GetExtension(file.Path));
            file.Write(file.Temporary);
            using (var staged = new FileStream(file.Temporary, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) staged.Flush(true);
            if (file.CheckExpectedOriginal) file.PreparedContents = File.ReadAllBytes(file.Temporary);
        }

        /// <summary>替换单个正式文件，错误包含对应目标文件以便主保存定位失败。</summary>
        private static void CommitFile(PendingFile file)
        {
            try
            {
                CheckOriginal(file);
                if (File.Exists(file.Path)) File.Replace(file.Temporary,file.Path,null);
                else File.Move(file.Temporary,file.Path);
            }
            catch (Exception error)
            {
                throw new IOException(string.Format(FrameworkLanguage.LangMsg("ProjectConfiguration","msg_CommitFailed",
                    "无法提交配置文件：{0}","Cannot commit configuration file: {0}","No se puede confirmar el archivo de configuración: {0}"),file.Path),error);
            }
        }

        /// <summary>生成源码在准备后被外部修改时拒绝覆盖，保留现场文件和编辑草稿。</summary>
        private static void CheckOriginal(PendingFile file)
        {
            if (!file.CheckExpectedOriginal) return;
            byte[] current = File.Exists(file.Path) ? File.ReadAllBytes(file.Path) : null;
            if (current == null && file.ExpectedOriginal == null) return;
            if (current != null && file.ExpectedOriginal != null && current.SequenceEqual(file.ExpectedOriginal)) return;
            throw new IOException(string.Format(FrameworkLanguage.LangMsg("ProjectConfiguration","msg_ExternalSourceChange",
                "源码或生成清单在保存准备后被修改，请保留文件并重新处理：{0}","Source or generation manifest changed after save preparation; preserve the file and resolve the conflict: {0}","El código o manifiesto cambió después de preparar el guardado; conserve el archivo y resuelva el conflicto: {0}"),file.Path));
        }

        /// <summary>尽力清理尚未提交的暂存文件，保留原始保存错误。</summary>
        private static void DeleteTemporaryFile(PendingFile file)
        {
            if (file.Temporary == null || !File.Exists(file.Temporary)) return;
            try { File.Delete(file.Temporary); }
            catch (IOException error) { System.Diagnostics.Trace.WriteLine(error); }
            catch (UnauthorizedAccessException error) { System.Diagnostics.Trace.WriteLine(error); }
        }

        /// <summary>恢复失败保存前的文件内容，累计回滚错误便于恢复。</summary>
        private static void RestoreFile(PendingFile file,List<Exception> errors)
        {
            try
            {
                if (file.CheckExpectedOriginal)
                {
                    byte[] current = File.Exists(file.Path) ? File.ReadAllBytes(file.Path) : null;
                    if (current == null || file.PreparedContents == null || !current.SequenceEqual(file.PreparedContents))
                        throw new IOException(string.Format(FrameworkLanguage.LangMsg("ProjectConfiguration","msg_SourceRollbackConflict",
                            "回滚期间检测到外部源码修改，已保留该文件，请手动恢复：{0}","External source changes were detected during rollback; the file was preserved for manual recovery: {0}","Se detectaron cambios externos durante la reversión; se conservó el archivo para recuperación manual: {0}"),file.Path));
                }
                if (file.Original == null) File.Delete(file.Path);
                else File.WriteAllBytes(file.Path,file.Original);
            }
            catch (Exception error) { errors.Add(error); }
        }

        /// <summary>先准备全部恢复动作，任一预检失败时保留所有尚未恢复的草稿。</summary>
        public static void Discard()
        {
            BeginOperation();
            try
            {
                FlowPrograms.RequireIdle();
                Action[] restore = Participants().Select(participant => participant.PrepareConfigurationDiscard()).Where(action => action != null).ToArray();
                foreach (Action action in restore) action();
            }
            finally { System.Threading.Volatile.Write(ref operation, 0); }
        }
        /// <summary>合并已登记业务模块及兼容页面，窗口绑定不产生第二份配置。</summary>
        internal static IEnumerable<IConfigurationModule> ConfigurationModules()
        {
            return ApplicationHost.Modules.Modules.OfType<IConfigurationModule>()
                .Concat(ModuleManager.ModuleList.Select(form => form.RuntimeModule).Where(module => module != null)).Distinct();
        }

        /// <summary>以配置对象身份去重，页面和无窗口模块共用主保存清单。</summary>
        internal static IEnumerable<ModuleConfiguration> ModuleConfigurations()
        {
            return ModuleManager.ModuleList.Select(form => form.ConfigurationData)
                .Concat(ConfigurationModules().Select(module => module.Configuration)).Distinct().ToArray();
        }

        public static bool HasModuleRecipeChanges { get { return ModuleConfigurations().Any(module => module.HasRecipeDraft); } }

        /// <summary>配方入口加载全部注册模块，不要求业务模块有页面实例。</summary>
        public static void LoadModuleRecipes(string path)
        {
            var changes = new List<RecipeChange>();
            byte[] content = File.Exists(path) ? File.ReadAllBytes(path) : null;
            try { foreach (ModuleConfiguration module in ModuleConfigurations()) changes.Add(module.PrepareRecipeContent(content)); }
            catch { RecipeChange.Release(changes); throw; }
            RecipeChange.Commit(changes, () => { });
        }

        /// <summary>统一配方入口，在同一操作占用中准备数据和视觉模型，最后提交身份与持久选择。</summary>
        public static void ChangeRecipe(string path, Action publish, IEnumerable<ConfigurationFile> selection = null)
        {
            BeginOperation();
            var changes = new List<RecipeChange>();
            try
            {
                if (ApplicationHost.IsShuttingDown || FlowPrograms.AnyRunning || DeviceOperations.HasPending || SysPara.SystemMode != RunMode.IDLE
                    || ModuleManager.ModuleList.Any(module => module.IsManualRun())) throw new InvalidOperationException(FlowText.T("busy"));
                if (!File.Exists(path)) throw new FileNotFoundException(ConfigurationText.RecipeMissing, path);
                if (HasModuleRecipeChanges || VisionproInterface.VList.Any(station => station.HasToolBlockDraft)) throw new InvalidOperationException(ConfigurationText.RecipeDraft);
                byte[] content = File.ReadAllBytes(path);
                foreach (ModuleConfiguration module in ModuleConfigurations()) changes.Add(module.PrepareRecipeContent(content));
                string name = Path.GetFileNameWithoutExtension(path);
                foreach (VisionproInterface station in VisionproInterface.VList.ToArray()) changes.Add(station.PrepareRecipe(name));
                ConfigurationFile[] files = selection == null ? new ConfigurationFile[0] : selection.ToArray();
                bool committed = false;
                RecipeChange.Commit(changes, () =>
                {
                    if (files.Length > 0) { WriteFiles(BuildFiles(new[] { new ConfigurationChange(files) })); committed = true; }
                    try { publish(); }
                    catch { if (committed) { RecoveryRequired = true; MachineState.Global.BeginStop(); } throw; }
                });
            }
            finally { try { RecipeChange.Release(changes); } finally { System.Threading.Volatile.Write(ref operation, 0); } }
        }

        /// <summary>为兼容写入入口捕获全部模块配方，不改变任何内存行版本。</summary>
        internal static DataSet CaptureModuleRecipes()
        {
            var snapshot = new DataSet();
            foreach (ModuleConfiguration module in ModuleConfigurations()) snapshot.Merge(module.Recipe.Copy());
            return snapshot;
        }
    }
}
