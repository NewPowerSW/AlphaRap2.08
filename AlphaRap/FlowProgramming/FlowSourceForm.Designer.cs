using System.Drawing;
using System.Windows.Forms;
namespace AlphaRap.FlowProgramming
{
    partial class FlowSourceForm
    {
        private TreeView tree;
        private TextBox value;
        private Button ok;
        private Button cancel;
        private FlowLayoutPanel buttons;
        private Panel footer;
        /// <summary>Build the fixed layout shared by designer preview and runtime.</summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            tree = new TreeView();
            tree.Name = "sources";
            tree.Dock = DockStyle.Fill;
            tree.HideSelection = false;
            value = new TextBox();
            value.Name = "referenceValue";
            value.Dock = DockStyle.Top;
            footer = new Panel();
            footer.Name = "footer";
            footer.Dock = DockStyle.Bottom;
            footer.Height = 80;
            footer.Padding = new Padding(8);
            buttons = new FlowLayoutPanel();
            buttons.Name = "buttons";
            buttons.Dock = DockStyle.Bottom;
            buttons.Height = 32;
            buttons.FlowDirection = FlowDirection.RightToLeft;
            ok = new Button();
            ok.Name = "ok";
            ok.Tag = "ok";
            ok.Text = "确定";
            ok.AutoSize = true;
            cancel = new Button();
            cancel.Name = "cancel";
            cancel.Tag = "cancel";
            cancel.Text = "取消";
            cancel.AutoSize = true;
            cancel.DialogResult = DialogResult.Cancel;
            AcceptButton = ok;
            CancelButton = cancel;
            buttons.Controls.Add(cancel);
            buttons.Controls.Add(ok);
            footer.Controls.Add(value);
            footer.Controls.Add(buttons);
            Controls.Add(tree);
            Controls.Add(footer);
            Name = "FlowSourceForm";
            Tag = "title";
            Text = "流程工程";
            Font = new Font("Microsoft YaHei", 9F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(700, 500);
            MinimumSize = new Size(700, 500);
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            MaximizeBox = false;
            ResumeLayout(false);
        }
    }
}
