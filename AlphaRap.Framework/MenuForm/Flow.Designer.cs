using System.Drawing;
using System.Windows.Forms;
namespace AlphaRap
{
    partial class Flow
    {
        private Label initialEntryEntryLabel;
        private Label autoEntryEntryLabel;
        private TabControl tabControl1;
        private FlowLayoutPanel toolbar;
        private FlowLayoutPanel entries;
        private CheckBox edit;
        private CheckBox moduleEnabled;
        private Button initialValue;
        private Button autoValue;
        private Label status;
        private ListBox executionPositions;
        private Timer timer;
        private Button addModule;
        private ContextMenuStrip nodeMenu;
        private ContextMenuStrip moduleMenu;
        private ToolStripMenuItem moduleDelete;
        private ToolStripMenuItem nodeDelete, nodeProperties;
        private ToolStripMenuItem nodeBreakpoint;
        private CheckBox debugMode;
        private FlowProgramming.FlowDebugPanel debugPanel;
        private Button reload;
        private Button validate;
        private Button pause;
        private Button stop;
        private Button schema;
        private System.Windows.Forms.SplitContainer workspace;
        private System.Windows.Forms.Panel workArea;
        private System.Windows.Forms.Panel toolboxRail;
        private System.Windows.Forms.Button toolboxToggle;
        private FlowProgramming.FlowToolboxControl toolbox;
        private System.Windows.Forms.Button viewSources;
        private System.Windows.Forms.ToolStripSeparator nodeSeparator;
        private System.Windows.Forms.ToolTip sourceHint;
        /// <summary>Release the UI refresh timer and child controls.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && timer != null) timer.Dispose();
            if (disposing && nodeMenu != null) nodeMenu.Dispose();
            if (disposing && moduleMenu != null) moduleMenu.Dispose();
            if (disposing && sourceHint != null) sourceHint.Dispose();
            base.Dispose(disposing);
        }
        /// <summary>Build the fixed flow engineering toolbar and canvas host.</summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Flow));
            this.workspace = new System.Windows.Forms.SplitContainer();
            this.toolbox = new AlphaRap.FlowProgramming.FlowToolboxControl();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.workArea = new System.Windows.Forms.Panel();
            this.toolboxRail = new System.Windows.Forms.Panel();
            this.toolboxToggle = new System.Windows.Forms.Button();
            this.viewSources = new System.Windows.Forms.Button();
            this.sourceHint = new System.Windows.Forms.ToolTip(this.components);
            this.nodeSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.toolbar = new System.Windows.Forms.FlowLayoutPanel();
            this.addModule = new System.Windows.Forms.Button();
            this.edit = new System.Windows.Forms.CheckBox();
            this.reload = new System.Windows.Forms.Button();
            this.validate = new System.Windows.Forms.Button();
            this.pause = new System.Windows.Forms.Button();
            this.stop = new System.Windows.Forms.Button();
            this.schema = new System.Windows.Forms.Button();
            this.debugMode = new System.Windows.Forms.CheckBox();
            this.entries = new System.Windows.Forms.FlowLayoutPanel();
            this.moduleEnabled = new System.Windows.Forms.CheckBox();
            this.initialEntryEntryLabel = new System.Windows.Forms.Label();
            this.initialValue = new System.Windows.Forms.Button();
            this.autoEntryEntryLabel = new System.Windows.Forms.Label();
            this.autoValue = new System.Windows.Forms.Button();
            this.status = new System.Windows.Forms.Label();
            this.executionPositions = new System.Windows.Forms.ListBox();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.nodeMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.nodeProperties = new System.Windows.Forms.ToolStripMenuItem();
            this.nodeBreakpoint = new System.Windows.Forms.ToolStripMenuItem();
            this.nodeDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.moduleMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.moduleDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.debugPanel = new AlphaRap.FlowProgramming.FlowDebugPanel();
            ((System.ComponentModel.ISupportInitialize)(this.workspace)).BeginInit();
            this.workspace.Panel1.SuspendLayout();
            this.workspace.Panel2.SuspendLayout();
            this.workspace.SuspendLayout();
            this.workArea.SuspendLayout();
            this.toolboxRail.SuspendLayout();
            this.toolbar.SuspendLayout();
            this.entries.SuspendLayout();
            this.nodeMenu.SuspendLayout();
            this.moduleMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // workspace
            // 
            this.workspace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.workspace.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.workspace.Location = new System.Drawing.Point(40, 0);
            this.workspace.Name = "workspace";
            // 
            // workspace.Panel1
            // 
            this.workspace.Panel1.Controls.Add(this.toolbox);
            this.workspace.Panel1Collapsed = true;
            this.workspace.Panel1MinSize = 220;
            // 
            // workspace.Panel2
            // 
            this.workspace.Panel2.Controls.Add(this.tabControl1);
            this.workspace.Panel2MinSize = 180;
            this.workspace.Size = new System.Drawing.Size(1240, 572);
            this.workspace.SplitterDistance = 280;
            this.workspace.TabIndex = 0;
            // 
            // toolbox
            // 
            this.toolbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolbox.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.toolbox.Location = new System.Drawing.Point(0, 0);
            this.toolbox.MinimumSize = new System.Drawing.Size(220, 240);
            this.toolbox.Name = "toolbox";
            this.toolbox.Size = new System.Drawing.Size(280, 240);
            this.toolbox.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.Size = new System.Drawing.Size(1240, 572);
            this.tabControl1.TabIndex = 0;
            // 
            // workArea
            // 
            this.workArea.Controls.Add(this.workspace);
            this.workArea.Controls.Add(this.toolboxRail);
            this.workArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.workArea.Location = new System.Drawing.Point(0, 91);
            this.workArea.Name = "workArea";
            this.workArea.Size = new System.Drawing.Size(1280, 572);
            this.workArea.TabIndex = 0;
            // 
            // toolboxRail
            // 
            this.toolboxRail.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(248)))));
            this.toolboxRail.Controls.Add(this.toolboxToggle);
            this.toolboxRail.Dock = System.Windows.Forms.DockStyle.Left;
            this.toolboxRail.Location = new System.Drawing.Point(0, 0);
            this.toolboxRail.Name = "toolboxRail";
            this.toolboxRail.Size = new System.Drawing.Size(40, 572);
            this.toolboxRail.TabIndex = 1;
            // 
            // toolboxToggle
            // 
            this.toolboxToggle.AccessibleDescription = "展开工具箱";
            this.toolboxToggle.AccessibleName = "流程工具箱";
            this.toolboxToggle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(248)))));
            this.toolboxToggle.FlatAppearance.BorderSize = 0;
            this.toolboxToggle.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(236)))), ((int)(((byte)(247)))));
            this.toolboxToggle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.toolboxToggle.Image = ((System.Drawing.Image)(resources.GetObject("toolboxToggle.Image")));
            this.toolboxToggle.Location = new System.Drawing.Point(3, 4);
            this.toolboxToggle.Name = "toolboxToggle";
            this.toolboxToggle.Size = new System.Drawing.Size(34, 36);
            this.toolboxToggle.TabIndex = 0;
            this.sourceHint.SetToolTip(this.toolboxToggle, "展开工具箱");
            this.toolboxToggle.UseVisualStyleBackColor = false;
            // 
            // viewSources
            // 
            this.viewSources.AutoSize = true;
            this.viewSources.Location = new System.Drawing.Point(624, 9);
            this.viewSources.Name = "viewSources";
            this.viewSources.Size = new System.Drawing.Size(90, 27);
            this.viewSources.TabIndex = 7;
            this.viewSources.Text = "查看生成源码";
            // 
            // nodeSeparator
            // 
            this.nodeSeparator.Name = "nodeSeparator";
            this.nodeSeparator.Size = new System.Drawing.Size(158, 6);
            // 
            // toolbar
            // 
            this.toolbar.AutoSize = true;
            this.toolbar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.toolbar.Controls.Add(this.addModule);
            this.toolbar.Controls.Add(this.edit);
            this.toolbar.Controls.Add(this.reload);
            this.toolbar.Controls.Add(this.validate);
            this.toolbar.Controls.Add(this.pause);
            this.toolbar.Controls.Add(this.stop);
            this.toolbar.Controls.Add(this.schema);
            this.toolbar.Controls.Add(this.viewSources);
            this.toolbar.Controls.Add(this.debugMode);
            this.toolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolbar.Location = new System.Drawing.Point(0, 0);
            this.toolbar.Name = "toolbar";
            this.toolbar.Padding = new System.Windows.Forms.Padding(6);
            this.toolbar.Size = new System.Drawing.Size(1280, 45);
            this.toolbar.TabIndex = 2;
            // 
            // addModule
            // 
            this.addModule.AutoSize = true;
            this.addModule.Location = new System.Drawing.Point(9, 9);
            this.addModule.Name = "addModule";
            this.addModule.Size = new System.Drawing.Size(90, 27);
            this.addModule.TabIndex = 0;
            this.addModule.Tag = "addModule";
            this.addModule.Text = "添加流程模块";
            // 
            // edit
            // 
            this.edit.AutoSize = true;
            this.edit.Location = new System.Drawing.Point(105, 9);
            this.edit.Name = "edit";
            this.edit.Size = new System.Drawing.Size(75, 21);
            this.edit.TabIndex = 1;
            this.edit.Tag = "edit";
            this.edit.Text = "编辑流程";
            // 
            // reload
            // 
            this.reload.AutoSize = true;
            this.reload.Location = new System.Drawing.Point(186, 9);
            this.reload.Name = "reload";
            this.reload.Size = new System.Drawing.Size(75, 27);
            this.reload.TabIndex = 2;
            this.reload.Tag = "reload";
            this.reload.Text = "重新加载";
            // 
            // validate
            // 
            this.validate.AutoSize = true;
            this.validate.Location = new System.Drawing.Point(267, 9);
            this.validate.Name = "validate";
            this.validate.Size = new System.Drawing.Size(75, 27);
            this.validate.TabIndex = 3;
            this.validate.Tag = "validate";
            this.validate.Text = "校验";
            // 
            // pause
            // 
            this.pause.AutoSize = true;
            this.pause.Location = new System.Drawing.Point(348, 9);
            this.pause.Name = "pause";
            this.pause.Size = new System.Drawing.Size(105, 27);
            this.pause.TabIndex = 4;
            this.pause.Tag = "pause";
            this.pause.Text = "边界暂停 / 继续";
            // 
            // stop
            // 
            this.stop.AutoSize = true;
            this.stop.Location = new System.Drawing.Point(459, 9);
            this.stop.Name = "stop";
            this.stop.Size = new System.Drawing.Size(75, 27);
            this.stop.TabIndex = 5;
            this.stop.Tag = "stop";
            this.stop.Text = "停止";
            // 
            // schema
            // 
            this.schema.AutoSize = true;
            this.schema.Location = new System.Drawing.Point(540, 9);
            this.schema.Name = "schema";
            this.schema.Size = new System.Drawing.Size(78, 27);
            this.schema.TabIndex = 6;
            this.schema.Tag = "schema";
            this.schema.Text = "生产表字段";
            // 
            // debugMode
            // 
            this.debugMode.AutoSize = true;
            this.debugMode.Location = new System.Drawing.Point(729, 12);
            this.debugMode.Margin = new System.Windows.Forms.Padding(12, 6, 3, 3);
            this.debugMode.Name = "debugMode";
            this.debugMode.Size = new System.Drawing.Size(75, 21);
            this.debugMode.TabIndex = 8;
            this.debugMode.Tag = "debugMode";
            this.debugMode.Text = "节点调试";
            // 
            // entries
            // 
            this.entries.AutoSize = true;
            this.entries.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.entries.Controls.Add(this.moduleEnabled);
            this.entries.Controls.Add(this.initialEntryEntryLabel);
            this.entries.Controls.Add(this.initialValue);
            this.entries.Controls.Add(this.autoEntryEntryLabel);
            this.entries.Controls.Add(this.autoValue);
            this.entries.Dock = System.Windows.Forms.DockStyle.Top;
            this.entries.Location = new System.Drawing.Point(0, 45);
            this.entries.Name = "entries";
            this.entries.Padding = new System.Windows.Forms.Padding(6);
            this.entries.Size = new System.Drawing.Size(1280, 46);
            this.entries.TabIndex = 1;
            // 
            // moduleEnabled
            // 
            this.moduleEnabled.AutoSize = true;
            this.moduleEnabled.Checked = true;
            this.moduleEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.moduleEnabled.Location = new System.Drawing.Point(9, 12);
            this.moduleEnabled.Margin = new System.Windows.Forms.Padding(3, 6, 16, 3);
            this.moduleEnabled.Name = "moduleEnabled";
            this.moduleEnabled.Size = new System.Drawing.Size(75, 21);
            this.moduleEnabled.TabIndex = 0;
            this.moduleEnabled.Tag = "moduleEnabled";
            this.moduleEnabled.Text = "启用流程";
            // 
            // initialEntryEntryLabel
            // 
            this.initialEntryEntryLabel.AutoSize = true;
            this.initialEntryEntryLabel.Location = new System.Drawing.Point(103, 6);
            this.initialEntryEntryLabel.Name = "initialEntryEntryLabel";
            this.initialEntryEntryLabel.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.initialEntryEntryLabel.Size = new System.Drawing.Size(68, 23);
            this.initialEntryEntryLabel.TabIndex = 1;
            this.initialEntryEntryLabel.Tag = "initialFlows";
            this.initialEntryEntryLabel.Text = "初始化流程";
            // 
            // initialValue
            // 
            this.initialValue.AutoEllipsis = true;
            this.initialValue.Location = new System.Drawing.Point(177, 9);
            this.initialValue.Name = "initialValue";
            this.initialValue.Size = new System.Drawing.Size(220, 28);
            this.initialValue.TabIndex = 2;
            this.initialValue.Text = "选择启动流程…";
            this.initialValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // autoEntryEntryLabel
            // 
            this.autoEntryEntryLabel.AutoSize = true;
            this.autoEntryEntryLabel.Location = new System.Drawing.Point(403, 6);
            this.autoEntryEntryLabel.Name = "autoEntryEntryLabel";
            this.autoEntryEntryLabel.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.autoEntryEntryLabel.Size = new System.Drawing.Size(80, 23);
            this.autoEntryEntryLabel.TabIndex = 3;
            this.autoEntryEntryLabel.Tag = "automaticFlows";
            this.autoEntryEntryLabel.Text = "自动运行流程";
            // 
            // autoValue
            // 
            this.autoValue.AutoEllipsis = true;
            this.autoValue.Location = new System.Drawing.Point(489, 9);
            this.autoValue.Name = "autoValue";
            this.autoValue.Size = new System.Drawing.Size(220, 28);
            this.autoValue.TabIndex = 4;
            this.autoValue.Text = "选择启动流程…";
            this.autoValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // status
            // 
            this.status.AutoEllipsis = true;
            this.status.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.status.Location = new System.Drawing.Point(0, 898);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(1280, 22);
            this.status.TabIndex = 4;
            this.status.Visible = false;
            // 
            // executionPositions
            //
            this.executionPositions.AccessibleName = "流程运行位置";
            this.executionPositions.BackColor = System.Drawing.SystemColors.Window;
            this.executionPositions.Dock = System.Windows.Forms.DockStyle.Top;
            this.executionPositions.HorizontalScrollbar = true;
            this.executionPositions.IntegralHeight = false;
            this.executionPositions.Items.AddRange(new object[] { "尚未运行" });
            this.executionPositions.Location = new System.Drawing.Point(0, 89);
            this.executionPositions.Name = "executionPositions_dyn";
            this.executionPositions.Size = new System.Drawing.Size(1280, 54);
            this.executionPositions.TabIndex = 5;
            this.executionPositions.SelectionMode = System.Windows.Forms.SelectionMode.None;
            //
            // timer
            // 
            this.timer.Interval = 250;
            // 
            // nodeMenu
            // 
            this.nodeMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nodeProperties,
            this.nodeBreakpoint,
            this.nodeSeparator,
            this.nodeDelete});
            this.nodeMenu.Name = "nodeMenu";
            this.nodeMenu.Size = new System.Drawing.Size(162, 76);
            // 
            // nodeProperties
            // 
            this.nodeProperties.Name = "nodeProperties";
            this.nodeProperties.Size = new System.Drawing.Size(161, 22);
            this.nodeProperties.Text = "属性编辑";
            // 
            // nodeBreakpoint
            // 
            this.nodeBreakpoint.Name = "nodeBreakpoint";
            this.nodeBreakpoint.Size = new System.Drawing.Size(161, 22);
            this.nodeBreakpoint.Tag = "nodeBreakpoint";
            this.nodeBreakpoint.Text = "设置 / 取消断点";
            // 
            // nodeDelete
            // 
            this.nodeDelete.Name = "nodeDelete";
            this.nodeDelete.Size = new System.Drawing.Size(161, 22);
            this.nodeDelete.Text = "删除节点";
            // 
            // moduleMenu
            // 
            this.moduleMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.moduleDelete});
            this.moduleMenu.Name = "moduleMenu";
            this.moduleMenu.Size = new System.Drawing.Size(149, 26);
            // 
            // moduleDelete
            // 
            this.moduleDelete.Name = "moduleDelete";
            this.moduleDelete.Size = new System.Drawing.Size(148, 22);
            this.moduleDelete.Text = "删除流程模块";
            // 
            // debugPanel
            // 
            this.debugPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.debugPanel.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.debugPanel.Location = new System.Drawing.Point(0, 663);
            this.debugPanel.Name = "debugPanel";
            this.debugPanel.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.debugPanel.Size = new System.Drawing.Size(1280, 235);
            this.debugPanel.TabIndex = 3;
            this.debugPanel.Visible = false;
            // 
            // Flow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1280, 920);
            this.ContextMenuStrip = this.nodeMenu;
            this.Controls.Add(this.workArea);
            this.Controls.Add(this.executionPositions);
            this.Controls.Add(this.entries);
            this.Controls.Add(this.toolbar);
            this.Controls.Add(this.debugPanel);
            this.Controls.Add(this.status);
            this.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Flow";
            this.Text = "Flow";
            this.workspace.Panel1.ResumeLayout(false);
            this.workspace.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.workspace)).EndInit();
            this.workspace.ResumeLayout(false);
            this.workArea.ResumeLayout(false);
            this.toolboxRail.ResumeLayout(false);
            this.toolbar.ResumeLayout(false);
            this.toolbar.PerformLayout();
            this.entries.ResumeLayout(false);
            this.entries.PerformLayout();
            this.nodeMenu.ResumeLayout(false);
            this.moduleMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.ComponentModel.IContainer components;
    }
}
