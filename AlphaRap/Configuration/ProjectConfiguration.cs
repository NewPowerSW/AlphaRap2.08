using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using AlphaRap.FlowProgramming;
using AlphaRapLibrary;

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
        }

        /// <summary>主保存前检查权限、机器状态及全部新功能草稿。</summary>
        public static void Validate()
        {
            FlowPrograms.RequireIdle();
            if (MiddleLayer.SystemS != null) MiddleLayer.SystemS.ValidateConstants();
            if (MiddleLayer.HardF != null && MiddleLayer.HardF.Resources != null) MiddleLayer.HardF.Resources.ValidateForSave();
            if (MiddleLayer.DataF != null) MiddleLayer.DataF.ValidateProductionDraft();
            if (MiddleLayer.VPF != null) MiddleLayer.VPF.ValidateVisionDraft();
            FlowPrograms.ValidateForSave();
        }

        /// <summary>先暂存所有配置文件，全部替换成功后才提交内存和流程版本。</summary>
        public static void Save()
        {
            Validate();
            List<PendingFile> files = PrepareFiles();
            WriteFiles(files);
            foreach (ModuleBaseForm module in ModuleManager.ModuleList)
            {
                module.RecipeData.AcceptChanges();
                module.SettingData.AcceptChanges();
            }
            if (MiddleLayer.HardF != null && MiddleLayer.HardF.Resources != null) MiddleLayer.HardF.Resources.AcceptSaved();
            if (MiddleLayer.DataF != null) MiddleLayer.DataF.AcceptProductionDraft();
            if (MiddleLayer.VPF != null) MiddleLayer.VPF.AcceptSaved();
            if (MiddleLayer.AddF != null) MiddleLayer.AddF.AcceptPermissionSave();
            FlowPrograms.PublishSaved();
        }

        /// <summary>捕获配方、机器设置、视觉、设备和权限草稿，写入任务只操作暂存文件。</summary>
        private static List<PendingFile> PrepareFiles()
        {
            List<PendingFile> files = new List<PendingFile>();
            DataSet recipe = new DataSet();
            foreach (ModuleBaseForm module in ModuleManager.ModuleList)
            {
                recipe.Merge(module.RecipeData.Copy());
                DataSet settings = module.SettingData.Copy();
                settings.AcceptChanges();
                files.Add(new PendingFile
                {
                    Path = System.IO.Path.Combine(ModuleManager.SettingDataDirectory,module.Text+".xml"),
                    Write = settings.WriteXml
                });
            }
            recipe.AcceptChanges();
            files.Add(new PendingFile { Path=SysPara.FilePath,Write=recipe.WriteXml });
            if (MiddleLayer.HardF != null && MiddleLayer.HardF.Resources != null) files.Add(new PendingFile { Path=MiddleLayer.HardF.Resources.PathName,Write=MiddleLayer.HardF.Resources.WriteDraft });
            if (MiddleLayer.DataF != null) files.Add(new PendingFile { Path=DataForm.ProductionSchemaPath,Write=MiddleLayer.DataF.WriteProductionDraft });
            files.Add(new PendingFile { Path=FlowPrograms.FilePath,Write=FlowPrograms.WriteDraft });
            if (MiddleLayer.VPF != null) files.AddRange(MiddleLayer.VPF.PrepareConfigurationSave());
            files.Add(DeviceControl.PrepareConfigurationSave());
            PendingFile permissions = MiddleLayer.AddF == null ? null : MiddleLayer.AddF.PreparePermissionSave();
            if (permissions != null) files.Add(permissions);
            foreach (PendingFile file in files) file.Path = System.IO.Path.GetFullPath(file.Path);
            if (files.GroupBy(file => file.Path,StringComparer.OrdinalIgnoreCase).Any(group => group.Count()>1))
                throw new IOException(HardwareText.T("duplicatePath"));
            return files;
        }

        /// <summary>暂存完成后替换正式文件，失败时恢复已经替换的文件。</summary>
        private static void WriteFiles(List<PendingFile> files)
        {
            List<PendingFile> committed = new List<PendingFile>();
            try
            {
                foreach (PendingFile file in files) StageFile(file);
                foreach (PendingFile file in files)
                {
                    CommitFile(file);
                    committed.Add(file);
                }
            }
            catch (Exception error)
            {
                List<Exception> errors = new List<Exception> { error };
                foreach (PendingFile file in committed.AsEnumerable().Reverse()) RestoreFile(file,errors);
                if (errors.Count > 1) throw new AggregateException(errors);
                throw;
            }
            finally { foreach (PendingFile file in files) DeleteTemporaryFile(file); }
        }

        /// <summary>使用同目录短名称和原扩展名暂存文件，不提交内存版本。</summary>
        private static void StageFile(PendingFile file)
        {
            string directory = System.IO.Path.GetDirectoryName(file.Path);
            Directory.CreateDirectory(directory);
            file.Original = File.Exists(file.Path) ? File.ReadAllBytes(file.Path) : null;
            file.Temporary = System.IO.Path.Combine(directory,Guid.NewGuid().ToString("N")+System.IO.Path.GetExtension(file.Path));
            file.Write(file.Temporary);
        }

        /// <summary>替换单个正式文件，错误包含对应目标文件以便主保存定位失败。</summary>
        private static void CommitFile(PendingFile file)
        {
            try
            {
                if (File.Exists(file.Path)) File.Replace(file.Temporary,file.Path,file.Path+".bak");
                else File.Move(file.Temporary,file.Path);
            }
            catch (Exception error)
            {
                throw new IOException(string.Format(MiddleLayer.LangMsg("ProjectConfiguration","msg_CommitFailed",
                    "无法提交配置文件：{0}","Cannot commit configuration file: {0}","No se puede confirmar el archivo de configuración: {0}"),file.Path),error);
            }
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
                if (file.Original == null) File.Delete(file.Path);
                else File.WriteAllBytes(file.Path,file.Original);
            }
            catch (Exception error) { errors.Add(error); }
        }

        /// <summary>主界面选择放弃时恢复模块、资源、视觉、设备、权限和流程草稿。</summary>
        public static void Discard()
        {
            FlowPrograms.RequireIdle();
            DeviceControl.ValidateConfigurationReload();
            Action discardPermissions = MiddleLayer.AddF == null ? null : MiddleLayer.AddF.PreparePermissionDiscard();
            if (MiddleLayer.VPF != null) MiddleLayer.VPF.RevertVpConfig();
            DeviceControl.ReloadAll();
            if (discardPermissions != null) discardPermissions();
            if (MiddleLayer.HardF != null && MiddleLayer.HardF.Resources != null) MiddleLayer.HardF.Resources.DiscardChanges();
            foreach (ModuleBaseForm module in ModuleManager.ModuleList)
            {
                module.ReadRecipeData(SysPara.FilePath);
                module.ReadSettingData();
            }
            if (MiddleLayer.DataF != null) MiddleLayer.DataF.DiscardProductionDraft();
            FlowPrograms.DiscardChanges();
        }
    }
}
