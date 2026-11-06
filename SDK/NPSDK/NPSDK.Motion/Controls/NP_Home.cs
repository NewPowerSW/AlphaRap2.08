using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace NPSDK.Motion
{
	/// <summary>回零按钮。绑定 WorkMotor，与板卡无关。</summary>
	public class NP_Home : Button
	{
		private bool runHome;
		private readonly Timer tmr;

		[Browsable(true)]
		[Category("#NP")]
		public NP_Motor WorkMotor { get; set; }

		public NP_Home()
		{
			Text = "Home";
			Size = new Size(100, 28);
			Click += (s, e) => StartHome();
			tmr = new Timer { Interval = 100 };
			tmr.Tick += (s, e) =>
			{
				if (!runHome || WorkMotor == null)
				{
					tmr.Enabled = false;
					return;
				}
				if (WorkMotor.Home())
				{
					runHome = false;
					tmr.Enabled = false;
				}
			};
		}

		public void StartHome()
		{
			if (WorkMotor == null || WorkMotor.GetBusy())
			{
				return;
			}
			WorkMotor.HomeReset();
			runHome = true;
			tmr.Enabled = true;
		}

		public void Stop()
		{
			runHome = false;
			tmr.Enabled = false;
			if (WorkMotor != null)
			{
				WorkMotor.Stop();
			}
		}
	}
}
