using System.Drawing;
using System.Windows.Forms;

namespace AlphaRap.Configuration
{
    partial class HardwareResourcePanel
    {
        private FlowLayoutPanel toolbar,canvas;
        private Button addAxis,addInput,addOutput,edit;
        private ContextMenuStrip resourceMenu;
        private ToolStripMenuItem resourceDelete, resourceProperties;
        private ComboBox selected;
        private Label hint,status,inputHeader,outputHeader;

        /// <summary>释放资源右键菜单及其项。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && resourceMenu != null) resourceMenu.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>固定管理入口与滚动组合控件画布。</summary>
        private void InitializeComponent()
        {
            toolbar = new FlowLayoutPanel { Name = "resourceToolbar",Dock = DockStyle.Top,Height = 78,WrapContents = true,Padding = new Padding(4) };
            addAxis = new Button { Name = "resourceAddAxis",Tag = "addAxis",Text = "新增轴",AutoSize = true };
            addInput = new Button { Name = "resourceAddInput",Tag = "addInput",Text = "新增输入",AutoSize = true };
            addOutput = new Button { Name = "resourceAddOutput",Tag = "addOutput",Text = "新增输出",AutoSize = true };
            selected = new ComboBox { Name = "resourceSelected",DropDownStyle = ComboBoxStyle.DropDownList,Width = 220 };
            edit = new Button { Name = "resourceEdit",Tag = "edit",Text = "参数",AutoSize = true };
            resourceMenu = new ContextMenuStrip { Name = "resourceMenu" };
            resourceDelete = new ToolStripMenuItem { Name = "resourceDelete", Text = "删除资源" };
            resourceProperties = new ToolStripMenuItem { Name = "resourceProperties", Text = "属性编辑" };
            resourceMenu.Items.AddRange(new ToolStripItem[] { resourceProperties, new ToolStripSeparator(), resourceDelete });
            toolbar.Controls.AddRange(new Control[] { addAxis,addInput,addOutput,selected,edit });
            canvas = new FlowLayoutPanel { Name = "resourceCanvas",Dock = DockStyle.Fill,AutoScroll = true,WrapContents = true,Padding = new Padding(5) };
            inputHeader = new Label { Name = "inputHeader_dyn", Text = "输入", Width = 1200, Height = 32, Font = new Font("Microsoft YaHei",10F,FontStyle.Bold), Visible = false };
            outputHeader = new Label { Name = "outputHeader_dyn", Text = "输出", Width = 1200, Height = 32, Font = new Font("Microsoft YaHei",10F,FontStyle.Bold), Visible = false };
            canvas.Controls.Add(inputHeader); canvas.Controls.Add(outputHeader);
            canvas.SetFlowBreak(inputHeader,true); canvas.SetFlowBreak(outputHeader,true);
            hint = new Label { Name = "resourceHint",Tag = "hint",Text = "双击标题或右键属性编辑，右键删除；配置统一使用主界面【保存】。轴点位随当前配方切换。新资源使用主界面【初始化】连接；已连接资源修改驱动参数后需保存并重启。",Dock = DockStyle.Bottom,Height = 64,Padding = new Padding(5) };
            status = new Label { Name = "resourceStatus_dyn",Dock = DockStyle.Bottom,Height = 28,AutoEllipsis = true };
            Controls.Add(canvas); Controls.Add(toolbar); Controls.Add(status); Controls.Add(hint);
            Name = "HardwareResourcePanel"; Size = new Size(1250,700); Font = new Font("Microsoft YaHei",9F); AutoScaleMode = AutoScaleMode.None;
        }
    }
}
