using System.ComponentModel;
using System.Windows.Forms;

namespace NPSDK.Motion
{
	/// <summary>点位表列绑定（对应 DMC3000 Collection）。</summary>
	public class PositionColumn
	{
		[Browsable(true)]
		[Category("#NP")]
		[Description("该列对应的轴")]
		public NP_Motor Axis { get; set; }

		[Browsable(true)]
		[Category("Design")]
		[DefaultValue("Columns")]
		public string Name { get; set; }

		[Browsable(true)]
		[Category("Parameter")]
		[DefaultValue(10000.0)]
		public double Limit_High { get; set; }

		[Browsable(true)]
		[Category("Parameter")]
		[DefaultValue(-10000.0)]
		public double Limit_Low { get; set; }

		public PositionColumn()
		{
			Name = "Columns";
			Limit_High = 10000.0;
			Limit_Low = -10000.0;
		}
	}
}
