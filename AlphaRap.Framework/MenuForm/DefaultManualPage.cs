using AlphaRap.Configuration;

namespace AlphaRap
{
    /// <summary>只使用动态 IO 的通用手动页面。</summary>
    public partial class DefaultManualPage : ManualPage
    {
        /// <summary>创建通用手动面板并应用框架缩放。</summary>
        public DefaultManualPage()
        {
            InitializeComponent();
            UiTheme.ApplyDpi(this);
        }

        /// <summary>复用硬件页登记的 IO 实例。</summary>
        public override void BindResources(HardwareResources resources)
        {
            ioResourcePanel.BindResources(resources);
        }
    }
}
