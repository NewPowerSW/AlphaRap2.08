using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NPSDK;

namespace NPSDK.Motion
{
	/// <summary>
	/// 手动调试按钮。设置 WorkMotor 时只调试这一轴；未设置时递归扫描所属窗体里的 NP_Motor。
	/// </summary>
	public class NP_ManualControl : Button
	{
		[Browsable(true)]
		[Category("#NP")]
		public NP_Motor WorkMotor { get; set; }

		[Browsable(true)]
		[Category("#NP")]
		public double DefaultSpeed { get; set; } = 30;

		[Browsable(true)]
		[Category("#NP")]
		public double MaximumJogSpeed { get; set; } = 100;

		/// <summary>创建手动调试入口，不读取或初始化硬件。</summary>
		public NP_ManualControl()
		{
			Text = "ManualControl";
			Size = new Size(134, 48);
			BackColor = Color.White;
			Click += (s, e) => Open();
		}

		/// <summary>每次打开重新读取当前轴列表，关闭后释放窗口及刷新计时器。</summary>
		public void Open()
		{
			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime || IsDisposed) return;
			using (MotorControlForm form = new MotorControlForm())
			{
				if (WorkMotor != null) form.Initial(WorkMotor, DefaultSpeed, MaximumJogSpeed);
				else form.Initial(this, DefaultSpeed, MaximumJogSpeed);
				form.ShowDialog(FindForm());
			}
		}
	}
}
