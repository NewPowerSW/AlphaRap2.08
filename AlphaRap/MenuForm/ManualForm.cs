using AlphaRap.Configuration;
using AlphaRap.TaskProcess;

namespace AlphaRap
{
    public partial class ManualForm : ManualPage
    {
        internal StationIo Io { get; private set; }

        /// <summary>手动页展示动态硬件目录，并复用它的 IO 对象。</summary>
        public override void BindResources(HardwareResources resources)
        {
            if (resources == null || Io != null) return;
            ioResourcePanel.BindResources(resources);
            Io = new StationIo(resources, ioResourcePanel);
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
