using System.Drawing;
using System.Windows.Forms;

namespace NPSDK.Design
{
    partial class FlowDemoInstanceDialog
    {
        private PropertyGrid instanceProperties;
        /// <summary>提供可预览的示例参数面板。</summary>
        private void InitializeComponent()
        {
            instanceProperties = new PropertyGrid
            {
                Dock = DockStyle.Fill,
                Name = "instanceProperties"
            };
            var hint = new Label
            {
                Dock = DockStyle.Top,
                Name = "demoHint",
                Height = 75,
                Padding = new Padding(8),
                Text = "这些参数只作用于 DEMO 实例。可将 VisionX 改为 5，测试条件不满足及超时报警。"
            };
            Font = new Font("Microsoft YaHei UI", 9);
            AutoScaleMode = AutoScaleMode.Font;
            Name = "FlowDemoInstanceDialog";
            Text = "示例实例参数（DEMO）";
            ClientSize = new Size(470, 440);
            StartPosition = FormStartPosition.CenterParent;
            Controls.Add(instanceProperties);
            Controls.Add(hint);
        }
    }
}
