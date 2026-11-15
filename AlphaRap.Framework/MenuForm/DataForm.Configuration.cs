using System;
using AlphaRap.Configuration;

namespace AlphaRap
{
    public partial class DataForm : IConfigurationParticipant
    {
        string IConfigurationParticipant.ConfigurationId { get { return "framework.data"; } }
        int IConfigurationParticipant.ConfigurationOrder { get { return 60; } }
        /// <summary>将生产字段和日志定义一并纳入主保存前校验。</summary>
        void IConfigurationParticipant.ValidateConfiguration() { ValidateProductionDraft(); ValidateLogModulesDraft(); }
        /// <summary>声明两份配置文件，业务运行日志不属于配置事务。</summary>
        ConfigurationChange IConfigurationParticipant.PrepareConfigurationSave()
        {
            return new ConfigurationChange(new[] {
                new ConfigurationFile(ProductionSchemaPath, WriteProductionDraft),
                new ConfigurationFile(LogModuleStore.FilePath, WriteLogModulesDraft)
            }, () => { AcceptProductionDraft(); AcceptLogModulesDraft(); });
        }
        /// <summary>恢复字段与日志草稿，不改变生产记录。</summary>
        Action IConfigurationParticipant.PrepareConfigurationDiscard() { return () => { DiscardProductionDraft(); DiscardLogModulesDraft(); }; }
    }
}
