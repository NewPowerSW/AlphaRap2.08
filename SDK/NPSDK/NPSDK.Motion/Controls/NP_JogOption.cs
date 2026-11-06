using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace NPSDK.Motion
{
	/// <summary>
	/// 点动方式。勾选 Continus 时按住方向连续走，松开停止；未勾选时只走 Distance 里的毫米数。
	/// </summary>
	public class NP_JogOption : UserControl
	{
		private Label labelContinus;
		private CheckBox chkContinus;
		private Label labelDistance;
		private TextBox txtDistance;
		private Label labelUnit;

		[Browsable(true)]
		[Category("#NP")]
		[Description("点动绑定的轴")]
		public NP_Motor WorkMotor { get; set; }

		[Browsable(true)]
		[Category("#NP")]
		[Description("勾选后为连续点动")]
		[DefaultValue(false)]
		public bool Continuous
		{
			get { return chkContinus.Checked; }
			set { chkContinus.Checked = value; }
		}

		[Browsable(true)]
		[Category("#NP")]
		[Description("未勾选 Continus 时，每次点动的距离，单位 mm")]
		[DefaultValue(1.0)]
		public double Distance
		{
			get
			{
				double value;
				if (double.TryParse(txtDistance.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
				{
					return Math.Abs(value);
				}
				return 1.0;
			}
			set { txtDistance.Text = value.ToString(CultureInfo.CurrentCulture); }
		}

		public NP_JogOption()
		{
			InitializeComponent();
			LayoutItems();
			Resize += OnResize;
		}

		/// <summary>
		/// 正向为 true。勾选 Continus 时开始连续点动，否则按 Distance 做一次相对移动。
		/// </summary>
		public bool BeginMove(NP_Motor motor, bool positive)
		{
			if (motor == null)
			{
				motor = WorkMotor;
			}
			if (motor == null || motor.GetBusy())
			{
				return false;
			}
			if (chkContinus.Checked)
			{
				if (positive)
				{
					motor.JogP();
				}
				else
				{
					motor.JogN();
				}
				return true;
			}
			double distance = Distance;
			double target = motor.GetPos() + (positive ? distance : -distance);
			return motor.Goto(target);
		}

		/// <summary>连续点动松开时停止。定距移动不会被松开打断。</summary>
		public void EndMove(NP_Motor motor)
		{
			if (motor == null)
			{
				motor = WorkMotor;
			}
			if (motor != null && chkContinus.Checked)
			{
				motor.Stop();
			}
		}

		private void TxtDistance_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == '\b')
			{
				return;
			}
			if (e.KeyChar == '-' && (txtDistance.SelectionStart != 0 || txtDistance.Text.IndexOf('-') >= 0))
			{
				e.Handled = true;
				return;
			}
			if (e.KeyChar == '.' && (txtDistance.SelectionStart == 0 || txtDistance.Text.IndexOf('.') >= 0))
			{
				e.Handled = true;
				return;
			}
			if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != '.' && e.KeyChar != '-')
			{
				e.Handled = true;
			}
		}

		private void InitializeComponent()
		{
			labelContinus = new Label();
			chkContinus = new CheckBox();
			labelDistance = new Label();
			txtDistance = new TextBox();
			labelUnit = new Label();
			SuspendLayout();

			labelContinus.AutoSize = true;
			labelContinus.Margin = new Padding(2, 6, 2, 0);
			labelContinus.Name = "labelContinus";
			labelContinus.Text = "Continus";

			chkContinus.AutoSize = true;
			chkContinus.Margin = new Padding(0, 4, 8, 0);
			chkContinus.Name = "chkContinus";
			chkContinus.Size = new Size(15, 14);

			labelDistance.AutoSize = true;
			labelDistance.Margin = new Padding(2, 6, 2, 0);
			labelDistance.Name = "labelDistance";
			labelDistance.Text = "Distance";

			txtDistance.Margin = new Padding(2, 2, 2, 0);
			txtDistance.Name = "txtDistance";
			txtDistance.Size = new Size(48, 21);
			txtDistance.Text = "1";
			txtDistance.TextAlign = HorizontalAlignment.Center;
			txtDistance.KeyPress += TxtDistance_KeyPress;

			labelUnit.AutoSize = true;
			labelUnit.Margin = new Padding(2, 6, 2, 0);
			labelUnit.Name = "labelUnit";
			labelUnit.Text = "mm";

			AutoScaleMode = AutoScaleMode.None;
			BackColor = SystemColors.Control;
			Controls.Add(labelContinus);
			Controls.Add(chkContinus);
			Controls.Add(labelDistance);
			Controls.Add(txtDistance);
			Controls.Add(labelUnit);
			Font = new Font("Arial", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
			Name = "NP_JogOption";
			Size = new Size(280, 32);
			ResumeLayout(false);
			PerformLayout();
		}

		private void OnResize(object sender, EventArgs e)
		{
			LayoutItems();
		}

		private void LayoutItems()
		{
			int x = 4;
			int y = 4;
			int rowHeight = 24;
			Place(labelContinus, ref x, ref y, rowHeight);
			Place(chkContinus, ref x, ref y, rowHeight);
			Place(labelDistance, ref x, ref y, rowHeight);
			Place(txtDistance, ref x, ref y, rowHeight);
			Place(labelUnit, ref x, ref y, rowHeight);
		}

		private void Place(Control control, ref int x, ref int y, int rowHeight)
		{
			int width = control.Width;
			if (control.AutoSize)
			{
				width = TextRenderer.MeasureText(control.Text, Font).Width + 4;
				if (control is CheckBox)
				{
					width = 18;
				}
			}
			if (x > 4 && x + width > ClientSize.Width - 2)
			{
				x = 4;
				y += rowHeight;
			}
			control.Location = new Point(x, y);
			x += width + 4;
		}
	}
}
