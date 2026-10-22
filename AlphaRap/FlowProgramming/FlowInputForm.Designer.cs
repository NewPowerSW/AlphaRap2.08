using System.Drawing;
using System.Windows.Forms;
namespace AlphaRap.FlowProgramming
{
    partial class FlowInputForm
    {
        private Label prompt;
        private TextBox value;
        private Button ok;
        private Button cancel;
        private FlowLayoutPanel buttons;
        /// <summary>Build the fixed layout shared by designer preview and runtime.</summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            prompt = new Label();
            prompt.Name = "prompt_dyn";
            prompt.Text = "输入产品信息";
            prompt.Dock = DockStyle.Top;
            prompt.Height = 56;
            prompt.Padding = new Padding(12);
            value = new TextBox();
            value.Name = "operatorValue";
            value.Dock = DockStyle.Top;
            value.Margin = new Padding(12);
            buttons = new FlowLayoutPanel();
            buttons.Name = "buttons";
            buttons.Dock = DockStyle.Bottom;
            buttons.Height = 44;
            buttons.FlowDirection = FlowDirection.RightToLeft;
            buttons.Padding = new Padding(6);
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
            ok.DialogResult = DialogResult.OK;
            buttons.Controls.Add(cancel);
            buttons.Controls.Add(ok);
            Controls.Add(value);
            Controls.Add(prompt);
            Controls.Add(buttons);
            Name = "FlowInputForm";
            Tag = "title";
            Text = "流程工程";
            Font = new Font("Microsoft YaHei", 9F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(640, 190);
            MinimumSize = new Size(640, 190);
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            MaximizeBox = false;
            ResumeLayout(false);
        }
    }
}
