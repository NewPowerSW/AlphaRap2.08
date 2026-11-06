using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NPSDK;

namespace NPSDK.Motion
{
	/// <summary>轴 IO 状态灯。绑定 WorkMotor，与板卡无关。</summary>
	public class NP_MotionIOStatus : UserControl
	{
		private readonly PictureBox pbAlm;
		private readonly PictureBox pbOrg;
		private readonly PictureBox pbPel;
		private readonly PictureBox pbMel;
		private readonly PictureBox pbSvon;
		private readonly Timer tmr;

		[Browsable(true)]
		[Category("#NP")]
		public NP_Motor WorkMotor { get; set; }

		public NP_MotionIOStatus()
		{
			pbAlm = MakeLamp("ALM", 0);
			pbOrg = MakeLamp("ORG", 1);
			pbPel = MakeLamp("PEL", 2);
			pbMel = MakeLamp("MEL", 3);
			pbSvon = MakeLamp("SVON", 4);
			Size = new Size(280, 36);

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
			AxisIOState io = WorkMotor.GetAxisIOState();
			pbAlm.BackgroundImage = io.ALM ? Resource.ledon : Resource.ledoff;
			pbOrg.BackgroundImage = io.ORG ? Resource.ledon : Resource.ledoff;
			pbPel.BackgroundImage = io.PEL ? Resource.ledon : Resource.ledoff;
			pbMel.BackgroundImage = io.MEL ? Resource.ledon : Resource.ledoff;
			pbSvon.BackgroundImage = io.SVON ? Resource.ledon : Resource.ledoff;
		}

		private PictureBox MakeLamp(string name, int index)
		{
			var lbl = new Label
			{
				Text = name,
				Location = new Point(index * 56, 0),
				Size = new Size(40, 14),
				TextAlign = ContentAlignment.MiddleCenter,
				Font = new Font("Microsoft YaHei UI", 7.5f)
			};
			var pb = new PictureBox
			{
				Location = new Point(index * 56 + 8, 14),
				Size = new Size(24, 20),
				BackgroundImage = Resource.ledoff,
				BackgroundImageLayout = ImageLayout.Zoom
			};
			Controls.Add(lbl);
			Controls.Add(pb);
			return pb;
		}
	}
}
