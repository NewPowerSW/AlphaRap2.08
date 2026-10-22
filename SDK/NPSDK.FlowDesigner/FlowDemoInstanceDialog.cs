using System.Windows.Forms;

namespace NPSDK.Design
{
    public sealed partial class FlowDemoInstanceDialog : Form
    {
        /// <summary>在设计器中展示纯示例对象。</summary>
        public FlowDemoInstanceDialog() : this(new FlowDemoInstances())
        {
        }

        /// <summary>仅编辑示例对象属性，不访问设备。</summary>
        public FlowDemoInstanceDialog(FlowDemoInstances instances)
        {
            InitializeComponent();
            instanceProperties.SelectedObject = instances;
        }
    }
}
