using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace NPSDK.Motion
{
	/// <summary>位置显示。绑定 WorkMotor，与板卡无关。</summary>
	public class NP_PositionShow : UserControl
	{
		private readonly TextBox txtCommand;
		private readonly TextBox txtEncoder;
		private readonly Timer tmr;

		[Browsable(true)]
		[Category("#NP")]
		public NP_Motor WorkMotor { get; set; }

		[Browsable(true)]
		[Category("#NP")]
		public bool ShowEncoder { get; set; } = true;

		public NP_PositionShow()
		{
			var lblCmd = new Label
			{
				Text = "Command",
				Location = new Point(0, 4),
				Size = new Size(70, 22),
				TextAlign = ContentAlignment.MiddleRight
			};
			txtCommand = new TextBox
			{
				Location = new Point(74, 2),
				Size = new Size(100, 24),
				ReadOnly = true,
				TextAlign = HorizontalAlignment.Center
			};
			var lblEnc = new Label
			{
				Text = "Encoder",
				Location = new Point(0, 32),
				Size = new Size(70, 22),
				TextAlign = ContentAlignment.MiddleRight
			};
			txtEncoder = new TextBox
			{
				Location = new Point(74, 30),
				Size = new Size(100, 24),
				ReadOnly = true,
				TextAlign = HorizontalAlignment.Center
			};
			Controls.Add(lblCmd);
			Controls.Add(txtCommand);
			Controls.Add(lblEnc);
			Controls.Add(txtEncoder);
			Size = new Size(180, 60);

			tmr = new Timer { Interval = 100 };
			tmr.Tick += (s, e) => RefreshShow();
			VisibleChanged += (s, e) => tmr.Enabled = Visible;
			tmr.Enabled = true;
		}

		public void RefreshShow()
		{
			if (WorkMotor == null)
			{
				return;
			}
			txtCommand.Text = WorkMotor.GetPos().ToString("F3");
			txtEncoder.Visible = ShowEncoder;
			if (ShowEncoder)
			{
				txtEncoder.Text = WorkMotor.GetEncPos().ToString("F3");
			}
		}
	}
}
