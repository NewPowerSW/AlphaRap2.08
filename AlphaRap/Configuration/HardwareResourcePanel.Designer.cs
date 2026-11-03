using System.Drawing;
using System.Windows.Forms;

namespace AlphaRap.Configuration
{
    partial class HardwareResourcePanel
    {
        private FlowLayoutPanel toolbar,canvas;
        private Button addAxis,addInput,addOutput,edit,addIoPage;
        private ContextMenuStrip resourceMenu;
        private ToolStripMenuItem resourceDelete, resourceProperties;
        private TabControl axisTabs;
        private TabControl ioTabs;
        private TabPage ioDefaultPage;
        private ContextMenuStrip ioPageMenu;
        private ToolStripMenuItem ioPageAdd,ioPageRename,ioPageDelete,resourceMove;
        private Label hint,status,inputHeader,outputHeader;

        /// <summary>释放资源右键菜单及其项。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && resourceMenu != null) resourceMenu.Dispose();
            if (disposing && ioPageMenu != null) ioPageMenu.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>固定管理入口、轴选项卡与独立 IO 画布。</summary>
        private void InitializeComponent()
        {
            this.toolbar = new FlowLayoutPanel();
            this.toolbar.Name = "resourceToolbar";
            this.toolbar.Dock = DockStyle.Top;
            this.toolbar.Height = 42;
            this.toolbar.WrapContents = true;
            this.toolbar.Padding = new Padding(4);
            this.addAxis = new Button();
            this.addAxis.Name = "resourceAddAxis";
            this.addAxis.Tag = "addAxis";
            this.addAxis.Text = "新增轴";
            this.addAxis.AutoSize = true;
            this.addInput = new Button();
            this.addInput.Name = "resourceAddInput";
            this.addInput.Tag = "addInput";
            this.addInput.Text = "新增输入";
            this.addInput.AutoSize = true;
            this.addOutput = new Button();
            this.addOutput.Name = "resourceAddOutput";
            this.addOutput.Tag = "addOutput";
            this.addOutput.Text = "新增输出";
            this.addOutput.AutoSize = true;
            this.edit = new Button();
            this.edit.Name = "resourceEdit";
            this.edit.Tag = "edit";
            this.edit.Text = "参数";
            this.edit.AutoSize = true;
            this.resourceMenu = new ContextMenuStrip();
            this.resourceMenu.Name = "resourceMenu";
            this.resourceDelete = new ToolStripMenuItem();
            this.resourceDelete.Name = "resourceDelete";
            this.resourceDelete.Text = "删除资源";
            this.resourceProperties = new ToolStripMenuItem();
            this.resourceProperties.Name = "resourceProperties";
            this.resourceProperties.Text = "属性编辑";
            this.resourceMove = new ToolStripMenuItem();
            this.resourceMove.Name = "resourceMove";
            this.resourceMove.Text = "移动到选项卡";
            this.resourceMenu.Items.AddRange(new ToolStripItem[] { resourceProperties, resourceMove, new ToolStripSeparator(), resourceDelete });
            this.addIoPage = new Button();
            this.addIoPage.Name = "resourceAddIoPage";
            this.addIoPage.Tag = "addIoPage";
            this.addIoPage.Text = "新增选项卡";
            this.addIoPage.AutoSize = true;
            this.toolbar.Controls.AddRange(new Control[] { addIoPage,addAxis,addInput,addOutput,edit });
            this.ioPageMenu = new ContextMenuStrip();
            this.ioPageMenu.Name = "ioPageMenu";
            this.ioPageAdd = new ToolStripMenuItem();
            this.ioPageAdd.Name = "ioPageAdd";
            this.ioPageAdd.Text = "新增选项卡";
            this.ioPageRename = new ToolStripMenuItem();
            this.ioPageRename.Name = "ioPageRename";
            this.ioPageRename.Text = "重命名选项卡";
            this.ioPageDelete = new ToolStripMenuItem();
            this.ioPageDelete.Name = "ioPageDelete";
            this.ioPageDelete.Text = "删除选项卡";
            this.ioPageMenu.Items.AddRange(new ToolStripItem[] { ioPageAdd,ioPageRename,new ToolStripSeparator(),ioPageDelete });
            this.ioTabs = new TabControl();
            this.ioTabs.Name = "ioResourceTabs";
            this.ioTabs.Dock = DockStyle.Fill;
            this.ioTabs.ShowToolTips = true;
            this.ioTabs.ContextMenuStrip = this.ioPageMenu;
            this.ioDefaultPage = new TabPage();
            this.ioDefaultPage.Name = "ioDefaultPage";
            this.ioDefaultPage.Text = "IO";
            this.ioDefaultPage.Tag = "default";
            this.ioDefaultPage.UseVisualStyleBackColor = true;
            this.ioTabs.TabPages.Add(this.ioDefaultPage);
            this.axisTabs = new TabControl();
            this.axisTabs.Name = "axisResourceTabs";
            this.axisTabs.Dock = DockStyle.Fill;
            this.axisTabs.Multiline = false;
            this.axisTabs.ShowToolTips = true;
            this.axisTabs.ContextMenuStrip = this.resourceMenu;
            this.canvas = new FlowLayoutPanel();
            this.canvas.Name = "resourceCanvas";
            this.canvas.Dock = DockStyle.Fill;
            this.canvas.AutoScroll = true;
            this.canvas.WrapContents = true;
            this.canvas.Padding = new Padding(5);
            this.inputHeader = new Label();
            this.inputHeader.Name = "inputHeader_dyn";
            this.inputHeader.Text = "输入";
            this.inputHeader.Width = 1200;
            this.inputHeader.Height = 32;
            this.inputHeader.Font = new Font("Microsoft YaHei",10F,FontStyle.Bold);
            this.inputHeader.Visible = false;
            this.outputHeader = new Label();
            this.outputHeader.Name = "outputHeader_dyn";
            this.outputHeader.Text = "输出";
            this.outputHeader.Width = 1200;
            this.outputHeader.Height = 32;
            this.outputHeader.Font = new Font("Microsoft YaHei",10F,FontStyle.Bold);
            this.outputHeader.Visible = false;
            canvas.Controls.Add(inputHeader); canvas.Controls.Add(outputHeader);
            canvas.SetFlowBreak(inputHeader,true); canvas.SetFlowBreak(outputHeader,true);
            this.hint = new Label();
            this.hint.Name = "resourceHint";
            this.hint.Tag = "hint";
            this.hint.Text = "双击标题或右键属性编辑，右键删除；配置统一使用主界面【保存】。轴点位随配方切换，连接及运动由流程负责；已连接资源修改驱动参数后需保存并重启。";
            this.hint.Dock = DockStyle.Bottom;
            this.hint.Height = 64;
            this.hint.Padding = new Padding(5);
            this.status = new Label();
            this.status.Name = "resourceStatus_dyn";
            this.status.Dock = DockStyle.Bottom;
            this.status.Height = 28;
            this.status.AutoEllipsis = true;
            this.ioDefaultPage.Controls.Add(this.canvas);
            Controls.Add(axisTabs); Controls.Add(ioTabs); Controls.Add(toolbar); Controls.Add(status); Controls.Add(hint);
            this.Name = "HardwareResourcePanel";
            this.Size = new Size(1250,700);
            this.Font = new Font("Microsoft YaHei",9F);
            this.AutoScaleMode = AutoScaleMode.None;
        }
    }
}
