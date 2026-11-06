using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace NPSDK.Motion
{
	/// <summary>气缸。只组合 NP_Input / NP_Output，不依赖板卡。</summary>
	public class NP_Cylinder : Panel
	{
		private NP_Input[] cyOnInputs;
		private NP_Output[] cyOnOutputs;
		private NP_Input[] cyOffInputs;
		private NP_Output[] cyOffOutputs;
		private readonly Button btnOn;
		private readonly Button btnOff;

		[Browsable(true)]
		[Category("#NP")]
		public int CyTimeoutTime { get; set; } = 30000;

		[Browsable(true)]
		[Category("#NP-On")]
		public NP_Input CyOn_IB1 { get; set; }

		[Browsable(true)]
		[Category("#NP-On")]
		public NP_Input CyOn_IB2 { get; set; }

		[Browsable(true)]
		[Category("#NP-On")]
		public NP_Input CyOn_IB3 { get; set; }

		[Browsable(true)]
		[Category("#NP-On")]
		public NP_Input CyOn_IB4 { get; set; }

		[Browsable(true)]
		[Category("#NP-On")]
		public NP_Output CyOn_OB1 { get; set; }

		[Browsable(true)]
		[Category("#NP-On")]
		public NP_Output CyOn_OB2 { get; set; }

		[Browsable(true)]
		[Category("#NP-Off")]
		public NP_Input CyOff_IB1 { get; set; }

		[Browsable(true)]
		[Category("#NP-Off")]
		public NP_Input CyOff_IB2 { get; set; }

		[Browsable(true)]
		[Category("#NP-Off")]
		public NP_Input CyOff_IB3 { get; set; }

		[Browsable(true)]
		[Category("#NP-Off")]
		public NP_Input CyOff_IB4 { get; set; }

		[Browsable(true)]
		[Category("#NP-Off")]
		public NP_Output CyOff_OB1 { get; set; }

		[Browsable(true)]
		[Category("#NP-Off")]
		public NP_Output CyOff_OB2 { get; set; }

		public NP_Cylinder()
		{
			btnOn = new Button { Text = "ON", Dock = DockStyle.Fill };
			btnOff = new Button { Text = "OFF", Dock = DockStyle.Fill };
			btnOn.Click += (s, e) => On();
			btnOff.Click += (s, e) => Off();
			var layout = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 2,
				RowCount = 1
			};
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
			layout.Controls.Add(btnOn, 0, 0);
			layout.Controls.Add(btnOff, 1, 0);
			Controls.Add(layout);
			Size = new Size(180, 36);
			BorderStyle = BorderStyle.FixedSingle;
		}

		public bool On()
		{
			return Drive(true);
		}

		public bool Off()
		{
			return Drive(false);
		}

		public void Stop()
		{
			Bind();
			foreach (NP_Output output in cyOnOutputs)
			{
				if (output != null)
				{
					output.Off();
				}
			}
			foreach (NP_Output output in cyOffOutputs)
			{
				if (output != null)
				{
					output.Off();
				}
			}
		}

		private bool Drive(bool on)
		{
			Bind();
			foreach (NP_Output output in cyOnOutputs)
			{
				if (output == null)
				{
					continue;
				}
				if (on) output.On(); else output.Off();
			}
			foreach (NP_Output output in cyOffOutputs)
			{
				if (output == null)
				{
					continue;
				}
				if (on) output.Off(); else output.On();
			}
			bool ok = true;
			foreach (NP_Input input in cyOnInputs)
			{
				if (input != null)
				{
					ok &= on ? input.IsOn() : !input.IsOn();
				}
			}
			foreach (NP_Input input in cyOffInputs)
			{
				if (input != null)
				{
					ok &= on ? !input.IsOn() : input.IsOn();
				}
			}
			return ok;
		}

		private void Bind()
		{
			cyOnInputs = new[] { CyOn_IB1, CyOn_IB2, CyOn_IB3, CyOn_IB4 };
			cyOffInputs = new[] { CyOff_IB1, CyOff_IB2, CyOff_IB3, CyOff_IB4 };
			cyOnOutputs = new[] { CyOn_OB1, CyOn_OB2 };
			cyOffOutputs = new[] { CyOff_OB1, CyOff_OB2 };
		}
	}
}
