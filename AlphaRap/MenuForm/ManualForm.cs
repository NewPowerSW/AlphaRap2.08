using AlphaRap.Configuration;

namespace AlphaRap
{
    public partial class ManualForm : ManualPage
    {
        /// <summary>手动页展示动态硬件目录，并复用它的 IO 对象。</summary>
        public override void BindResources(HardwareResources resources)
        {
            ioResourcePanel.BindResources(resources);
        }

        /// <summary>创建手动页的固定容器，项目点位由硬件配置生成。</summary>
        public ManualForm()
        {
            InitializeComponent();
            UiTheme.StyleTabControl(tabControl1, true);
            UiTheme.ApplyDpi(this);
            if (!UiTheme.IsDesignTime)
                ApplicationHost.RegisterLanguage(this, Name);
        }
    }
}
