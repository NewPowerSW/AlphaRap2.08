using System;
using System.Windows.Forms;
using AlphaRapLibrary;

namespace AlphaRap
{

    public partial class HardForm : ModuleBaseForm
    {
        public Configuration.HardwareResources Resources { get { return resourcePanel.Resources; } }


        /// <summary>创建旧硬件页面及组合资源入口，不在设计期连接硬件。</summary>
        public HardForm()
        {
            InitializeComponent();
            if (UiTheme.IsDesignTime) return;
            MiddleLayer.RegisterAndApplyLanguage(this, Name);
            X1_PositionTable1.CanEdit = () => FlowProgramming.FlowPrograms.CanEdit && SysPara.SystemMode == RunMode.IDLE && !FlowProgramming.FlowPrograms.AnyRunning && !NPSDK.Flow_Module.Module_GetRunningStatus;
            MiddleLayer.LanguageChanged += ApplyPointLanguage;
            Disposed += (sender,args) => MiddleLayer.LanguageChanged -= ApplyPointLanguage;
            ApplyPointLanguage(null, EventArgs.Empty);
            legacyResources.Click += (sender,args) => { resourcePanel.Visible = false; X1_Group.Visible = true; nP_ManualControl1.Visible = true; };
            configuredResources.Click += (sender,args) => { X1_Group.Visible = false; nP_ManualControl1.Visible = false; resourcePanel.Visible = true; resourcePanel.BringToFront(); };

        }

        /// <summary>旧点位表沿用相同的三语菜单，保持现有轴和点位绑定身份。</summary>
        private void ApplyPointLanguage(object sender, EventArgs args)
        {
            X1_PositionTable1.ApplyLanguage(Configuration.HardwareText.Translate);
        }

        /// <summary>在加载配方之前创建动态资源及其点位表结构。</summary>
        public override void ModuleInitialize(string moduleName)
        {
            base.ModuleInitialize(moduleName);
            resourcePanel.BindOwner(this);
            if (MiddleLayer.ManualF != null) MiddleLayer.ManualF.BindResources(Resources);
        }

        public void StopAllMotor()
        {
            MiddleLayer.StopAllMotor();
        }

        public void SaveHardData()
        {
        }
    }
}
