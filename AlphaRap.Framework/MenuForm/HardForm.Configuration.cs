using System;
using AlphaRap.Configuration;

namespace AlphaRap
{
    public partial class HardForm : IConfigurationParticipant
    {
        string IConfigurationParticipant.ConfigurationId { get { return "framework.hardware"; } }
        int IConfigurationParticipant.ConfigurationOrder { get { return 40; } }
        /// <summary>由硬件页面提供资源校验，保存核心不识别具体页面。</summary>
        void IConfigurationParticipant.ValidateConfiguration() { Resources?.ValidateForSave(); }
        /// <summary>捕获资源写入和接受动作，文件成功前保留脏标记。</summary>
        ConfigurationChange IConfigurationParticipant.PrepareConfigurationSave()
        {
            if (Resources == null) return new ConfigurationChange(new ConfigurationFile[0]);
            var resources = Resources;
            return new ConfigurationChange(new[] { new ConfigurationFile(resources.PathName, resources.WriteDraft) }, resources.AcceptSaved);
        }
        /// <summary>保持硬件恢复先于模块配方表恢复的顺序。</summary>
        Action IConfigurationParticipant.PrepareConfigurationDiscard() { return () => Resources?.DiscardChanges(); }
    }
}
