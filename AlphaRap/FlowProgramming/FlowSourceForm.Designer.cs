using System.Drawing;
using System.Windows.Forms;

namespace AlphaRap.FlowProgramming
{
    partial class FlowSourceForm
    {
        private TreeView tree;
        private Label contextHint;
        private Panel searchPanel;
        private Label searchLabel;
        private TextBox searchValue;
        private Label valueLabel;
        private TextBox value;
        private Label statusHint;
        private Button ok;
        private Button cancel;
        private FlowLayoutPanel buttons;
        private Panel footer;

        /// <summary>固定布局供设计器与运行时共用，所有绑定来源在同一窗口显示。</summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            tree = new TreeView();
            tree.Name = "sources";
            tree.Dock = DockStyle.Fill;
            tree.HideSelection = false;
            tree.FullRowSelect = true;
            tree.PathSeparator = " / ";
            contextHint = new Label();
            contextHint.Name = "sourceContext_dyn";
            contextHint.Dock = DockStyle.Top;
            contextHint.Height = 52;
            contextHint.Padding = new Padding(8, 5, 8, 4);
            contextHint.Text = "选择固定值、机器设置、当前配方或节点结果。";
            searchPanel = new Panel();
            searchPanel.Name = "searchPanel";
            searchPanel.Dock = DockStyle.Top;
            searchPanel.Height = 38;
            searchPanel.Padding = new Padding(8, 4, 8, 5);
            searchLabel = new Label();
            searchLabel.Name = "searchLabel";
            searchLabel.Tag = "bindingSearch";
            searchLabel.Text = "搜索名称";
            searchLabel.Dock = DockStyle.Left;
            searchLabel.Width = 115;
            searchLabel.TextAlign = ContentAlignment.MiddleLeft;
            searchValue = new TextBox();
            searchValue.Name = "searchValue";
            searchValue.Dock = DockStyle.Fill;
            searchPanel.Controls.Add(searchValue);
            searchPanel.Controls.Add(searchLabel);
            footer = new Panel();
            footer.Name = "footer";
            footer.Dock = DockStyle.Bottom;
            footer.Height = 136;
            footer.Padding = new Padding(8);
            valueLabel = new Label();
            valueLabel.Name = "valueLabel";
            valueLabel.Tag = "bindingValue";
            valueLabel.Text = "当前选择 / 固定值";
            valueLabel.Dock = DockStyle.Top;
            valueLabel.Height = 24;
            value = new TextBox();
            value.Name = "referenceValue";
            value.Dock = DockStyle.Top;
            statusHint = new Label();
            statusHint.Name = "sourceStatus_dyn";
            statusHint.Dock = DockStyle.Fill;
            statusHint.Padding = new Padding(0, 4, 0, 0);
            statusHint.ForeColor = SystemColors.GrayText;
            buttons = new FlowLayoutPanel();
            buttons.Name = "buttons";
            buttons.Dock = DockStyle.Bottom;
            buttons.Height = 34;
            buttons.FlowDirection = FlowDirection.RightToLeft;
            ok = new Button();
            ok.Name = "ok";
            ok.Tag = "ok";
            ok.Text = "确定";
            ok.AutoSize = true;
            ok.MinimumSize = new Size(80, 28);
            cancel = new Button();
            cancel.Name = "cancel";
            cancel.Tag = "cancel";
            cancel.Text = "取消";
            cancel.AutoSize = true;
            cancel.MinimumSize = new Size(80, 28);
            cancel.DialogResult = DialogResult.Cancel;
            AcceptButton = ok;
            CancelButton = cancel;
            buttons.Controls.Add(cancel);
            buttons.Controls.Add(ok);
            footer.Controls.Add(statusHint);
            footer.Controls.Add(value);
            footer.Controls.Add(valueLabel);
            footer.Controls.Add(buttons);
            Controls.Add(tree);
            Controls.Add(searchPanel);
            Controls.Add(contextHint);
            Controls.Add(footer);
            Name = "FlowSourceForm";
            Tag = "bindingTitle";
            Text = "绑定数据";
            Font = new Font("Microsoft YaHei", 9F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(740, 560);
            MinimumSize = new Size(700, 520);
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            MaximizeBox = false;
            ResumeLayout(false);
        }
    }
}
