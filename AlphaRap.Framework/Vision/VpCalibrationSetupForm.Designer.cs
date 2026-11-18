using System.Drawing;
using System.Windows.Forms;
using Cognex.VisionPro;

namespace AlphaRap
{
    partial class VpCalibrationSetupForm
    {
        /// <summary>布局包含照片、可拖动的特征和拟合误差设置。</summary>
        private void InitializeComponent()
        {
            instructions = new Label { Name = "instructions_dyn", Dock = DockStyle.Top, Height = 48,
                Text = "将圆拖到要测量的圆边上，拖动控制点调整半径。" };
            photo = new CogRecordDisplay { Name = "photo", Dock = DockStyle.Fill, ColorMapLowerClipColor = Color.Black };
            options = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 40, WrapContents = false };
            limitLabel = new Label { Name = "limitLabel", Text = "允许的拟合误差 (px)", Width = 248, Height = 28, TextAlign = ContentAlignment.MiddleLeft };
            limit = new TextBox { Name = "fitLimit", Width = 78 };
            options.Controls.Add(limitLabel);
            options.Controls.Add(limit);
            footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 40, FlowDirection = FlowDirection.RightToLeft };
            confirm = new Button { Name = "confirm", Text = "使用此设置", Width = 170, Height = 32 };
            cancel = new Button { Name = "cancel", Text = "取消", Width = 120, Height = 32, DialogResult = DialogResult.Cancel };
            footer.Controls.Add(confirm);
            footer.Controls.Add(cancel);
            Controls.Add(photo);
            Controls.Add(options);
            Controls.Add(footer);
            Controls.Add(instructions);
            AcceptButton = confirm;
            CancelButton = cancel;
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(820, 600);
            MinimumSize = new Size(640, 450);
            Padding = new Padding(12);
            StartPosition = FormStartPosition.CenterParent;
            Name = "VpCalibrationSetupForm";
            Text = "设置找边";
        }

        private Label instructions, limitLabel;
        private CogRecordDisplay photo;
        private FlowLayoutPanel options, footer;
        private TextBox limit;
        private Button confirm, cancel;
    }
}
