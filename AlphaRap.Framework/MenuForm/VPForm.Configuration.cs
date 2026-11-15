using System;
using AlphaRap.Configuration;

namespace AlphaRap
{
    public partial class VPForm : IConfigurationParticipant
    {
        string IConfigurationParticipant.ConfigurationId { get { return "framework.vision"; } }
        int IConfigurationParticipant.ConfigurationOrder { get { return 10; } }
        /// <summary>通过既有视觉草稿校验，不启动相机。</summary>
        void IConfigurationParticipant.ValidateConfiguration() { ValidateVisionDraft(); }
        /// <summary>保留视觉文件准备和接受快照逻辑。</summary>
        ConfigurationChange IConfigurationParticipant.PrepareConfigurationSave() { return ConfigurationChange.FromPending(PrepareConfigurationSave(), AcceptSaved); }
        /// <summary>放弃操作通过既有视觉恢复入口执行。</summary>
        Action IConfigurationParticipant.PrepareConfigurationDiscard() { return RevertVpConfig; }
    }
}
