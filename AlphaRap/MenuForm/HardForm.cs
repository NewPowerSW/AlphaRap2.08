using System;
using AlphaRapLibrary;

namespace AlphaRap
{

    public partial class HardForm : ModuleBaseForm
    {
        /// <summary>访问当前页面管理的动态硬件目录，IO 视图复用同一目录。</summary>
        public Configuration.HardwareResources Resources { get { return hardwareResourcePanel.Resources; } }


        /// <summary>创建动态轴页面和固定的全轴手动调试入口，设计时不连接硬件。</summary>
        public HardForm()
        {
            InitializeComponent();
            if (UiTheme.IsDesignTime) return;
            MiddleLayer.RegisterAndApplyLanguage(this, Name);
            MiddleLayer.LanguageChanged += ApplyManualLanguage;
            Disposed += (sender,args) => MiddleLayer.LanguageChanged -= ApplyManualLanguage;
            ApplyManualLanguage(null, EventArgs.Empty);
        }

        /// <summary>刷新固定调试入口的三语标题，保留轴实例和点位草稿。</summary>
        private void ApplyManualLanguage(object sender, EventArgs args)
        {
            nP_ManualControl1.Text = Configuration.HardwareText.T("manualDebug");
        }

        /// <summary>在加载配方之前创建动态资源及其点位表结构。</summary>
        public override void ModuleInitialize(string moduleName)
        {
            base.ModuleInitialize(moduleName);
            hardwareResourcePanel.BindOwner(this);
            if (MiddleLayer.ManualF != null) MiddleLayer.ManualF.BindResources(Resources);
        }

        /// <summary>停止当前框架登记的全部轴，包括动态新增的轴。</summary>
        public void StopAllMotor()
        {
            MiddleLayer.StopAllMotor();
        }

    }
}
