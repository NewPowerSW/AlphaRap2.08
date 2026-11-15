using System;
using AlphaRap.Configuration;

namespace AlphaRap
{
    public partial class AddUserForm : IConfigurationParticipant
    {
        string IConfigurationParticipant.ConfigurationId { get { return "framework.permissions"; } }
        int IConfigurationParticipant.ConfigurationOrder { get { return 30; } }
        /// <summary>权限表编辑及数据库状态由保存准备入口统一校验。</summary>
        void IConfigurationParticipant.ValidateConfiguration() { }
        /// <summary>保留数据库副本事务与权限快照接受逻辑。</summary>
        ConfigurationChange IConfigurationParticipant.PrepareConfigurationSave() { return ConfigurationChange.FromPending(new[] { PreparePermissionSave() }, AcceptPermissionSave); }
        /// <summary>先读取权限映射，全部参与者预检成功后才绑定恢复数据。</summary>
        Action IConfigurationParticipant.PrepareConfigurationDiscard() { return PreparePermissionDiscard(); }
    }
}
