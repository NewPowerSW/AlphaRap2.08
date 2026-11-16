using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AlphaRap.FlowProgramming;
using AlphaRapLibrary;
using NPSDK;

namespace AlphaRap.Configuration
{
    /// <summary>将兼容模块数据、流程工程及设备配置接入同一配置扩展协议。</summary>
    internal static class FrameworkConfigurationParticipants
    {
        /// <summary>捕获框架已有配置功能，模块集合在本次操作期间保持一致。</summary>
        internal static IEnumerable<IConfigurationParticipant> Capture()
        {
            return new IConfigurationParticipant[] { new DeviceParticipant(), new ModuleDataParticipant(), new FlowParticipant() };
        }

        private sealed class ModuleDataParticipant : IConfigurationParticipant
        {
            private readonly IConfigurationModule[] modules = ProjectConfiguration.ConfigurationModules().ToArray();
            private readonly ModuleConfiguration[] configurations = ProjectConfiguration.ModuleConfigurations().ToArray();
            public string ConfigurationId { get { return "framework.module-data"; } }
            public int ConfigurationOrder { get { return 50; } }
            /// <summary>通过模块能力接口校验，无窗口模块不要求页面存在。</summary>
            public void ValidateConfiguration() { foreach (IConfigurationModule module in modules) module.ValidateConfiguration(); }
            /// <summary>为本次保存复制数据集，正式文件成功后再接受原表版本。</summary>
            public ConfigurationChange PrepareConfigurationSave()
            {
                var recipe = new DataSet();
                var files = new List<ConfigurationFile>();
                foreach (ModuleConfiguration configuration in configurations)
                {
                    recipe.Merge(configuration.Recipe.Copy());
                    DataSet settings = configuration.Settings.Copy(); settings.AcceptChanges();
                    files.Add(new ConfigurationFile(configuration.SettingsPath(ModuleManager.SettingDataDirectory), settings.WriteXml));
                }
                recipe.AcceptChanges();
                files.Add(new ConfigurationFile(SysPara.FilePath, recipe.WriteXml));
                return new ConfigurationChange(files, AcceptSaved);
            }
            /// <summary>只接受已提交快照对应的模块，操作中新增模块留待下一次保存。</summary>
            private void AcceptSaved()
            {
                foreach (ModuleConfiguration configuration in configurations) { configuration.AcceptRecipeSaved(); configuration.Settings.AcceptChanges(); }
            }
            /// <summary>保留既有统一配方与机器设置恢复入口。</summary>
            public Action PrepareConfigurationDiscard()
            {
                return () => { foreach (ModuleConfiguration configuration in configurations) { configuration.LoadRecipe(SysPara.FilePath); configuration.LoadSettings(ModuleManager.SettingDataDirectory); } };
            }
        }

        private sealed class FlowParticipant : IConfigurationParticipant
        {
            public string ConfigurationId { get { return "framework.flow"; } }
            public int ConfigurationOrder { get { return 70; } }
            /// <summary>校验流程草稿与当前资源引用。</summary>
            public void ValidateConfiguration() { FlowPrograms.ValidateForSave(); }
            /// <summary>XML、源码及运行发布共同使用同一份流程快照。</summary>
            public ConfigurationChange PrepareConfigurationSave()
            {
                FlowProject snapshot = FlowPrograms.CaptureForSave();
                var files = new List<ProjectConfiguration.PendingFile> {
                    new ProjectConfiguration.PendingFile { Path = FlowPrograms.FilePath, Write = path => FlowProgramFile.Save(path, snapshot) }
                };
                files.AddRange(FlowSourceGenerator.Prepare(snapshot));
                return ConfigurationChange.FromPending(files, () => { FlowPrograms.PublishSaved(snapshot); FlowSourceGenerator.AcceptSaved(); });
            }
            /// <summary>先预检生成源码冲突，执行恢复动作时再放弃流程草稿。</summary>
            public Action PrepareConfigurationDiscard()
            {
                Action source = FlowSourceGenerator.PrepareDiscard();
                return () => { FlowPrograms.DiscardChanges(); source(); };
            }
        }

        private sealed class DeviceParticipant : IConfigurationParticipant
        {
            public string ConfigurationId { get { return "framework.devices"; } }
            public int ConfigurationOrder { get { return 20; } }
            /// <summary>具体配置及序列化校验由保存准备入口完成。</summary>
            public void ValidateConfiguration() { }
            /// <summary>只捕获既有设备配置，不执行连接或收发。</summary>
            public ConfigurationChange PrepareConfigurationSave()
            {
                return ConfigurationChange.FromPending(new[] { DeviceControl.PrepareConfigurationSave() });
            }
            /// <summary>预检重载能力，恢复动作仍使用原有面板重载入口。</summary>
            public Action PrepareConfigurationDiscard() { DeviceControl.ValidateConfigurationReload(); return DeviceControl.ReloadAll; }
        }
    }
}
