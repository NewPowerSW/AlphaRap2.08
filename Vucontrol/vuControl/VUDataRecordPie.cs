using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using Visifire.Charts;
using Visifire.Commons;

namespace VUControl
{
	public class VUDataRecordPie : System.Windows.Forms.UserControl, INotifyPropertyChanged
	{
		private List<string> strHeader = new List<string> { "OK数", "NG数", "总数", "良率" };

		private List<string> pieHeader;

		public static Chart dataChart;

		private int okNum;

		private int ngNum;

		private int totalNum;

		private float yield;

		private IContainer components = null;

		private ElementHost elementHost1;

		public List<string> Header
		{
			get
			{
				return pieHeader;
			}
			set
			{
				if (pieHeader != null || pieHeader != value)
				{
					pieHeader = value;
				}
			}
		}

		public int OkNum
		{
			get
			{
				return okNum;
			}
			set
			{
				if (okNum != value)
				{
					okNum = value;
					RaisePropertyChangedEvent("OkNum");
				}
			}
		}

		public int NgNum
		{
			get
			{
				return ngNum;
			}
			set
			{
				if (ngNum != value)
				{
					ngNum = value;
					RaisePropertyChangedEvent("NgNum");
				}
			}
		}

		public int TotalNum
		{
			get
			{
				return totalNum;
			}
			set
			{
				if (totalNum != value)
				{
					totalNum = value;
					RaisePropertyChangedEvent("TotalNum");
				}
			}
		}

		public float Yield
		{
			get
			{
				return yield;
			}
			set
			{
				if (yield != value)
				{
					RaisePropertyChangedEvent("Yield");
				}
			}
		}

		private event PropertyChangedEventHandler m_propertyChanged;

		public event PropertyChangedEventHandler PropertyChanged
		{
			add
			{
				m_propertyChanged += value;
			}
			remove
			{
				m_propertyChanged -= value;
			}
		}

		public VUDataRecordPie()
		{
			InitializeComponent();
			InitdataSeries(strHeader, elementHost1);
		}

		private void InitdataSeries(List<string> strHeader, ElementHost elementHost)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Expected O, but got Unknown
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Expected O, but got Unknown
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Expected O, but got Unknown
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Expected O, but got Unknown
			dataChart = new Chart();
			((Collection<DataSeries>)(object)dataChart.Series).Clear();
			((Collection<Title>)(object)dataChart.Titles).Clear();
			((VisifireControl)dataChart).ToolBarEnabled = false;
			dataChart.ScrollingEnabled = true;
			dataChart.View3D = true;
			((Collection<Title>)(object)dataChart.Titles).Add(new Title
			{
				Text = "数据统计",
				FontColor = System.Windows.Media.Brushes.Tomato,
				FontSize = 20.0
			});
			DataSeries val = new DataSeries();
			DataPointCollection val2 = new DataPointCollection();
			for (int i = 0; i < strHeader.Count; i++)
			{
				DataPoint dataPoint = new DataPoint();
				dataPoint.Enabled = true;
				dataPoint.Exploded = false;
				dataPoint.AxisXLabel = strHeader[i];
				dataPoint.YValue = 0.0;
				//((System.Windows.Controls.Control)(object)dataPoint).Background = System.Windows.Media.Brushes.Blue;
				//((System.Windows.Controls.Control)(object)dataPoint).FontSize = 30.0;
				((VisifireElement)dataPoint).MouseLeftButtonDown += delegate
				{
					dataPoint.Exploded = !dataPoint.Exploded;
				};
				((Collection<IDataPoint>)(object)val2).Add((IDataPoint)(object)dataPoint);
			}
			val.DataPoints = val2;
			val.LabelFontColor = System.Windows.Media.Brushes.Black;
			val.LabelAngle = 15.0;
			val.RenderAs = (RenderAs)2;
			val.LightingEnabled = true;
			val.LabelEnabled = true;
			((Collection<DataSeries>)(object)dataChart.Series).Add(val);
			elementHost.Child = (UIElement)(object)dataChart;
		}

		public void DataDisp(Chart chart)
		{
			foreach (DataSeries item in (Collection<DataSeries>)(object)chart.Series)
			{
				for (int i = 0; i < ((Collection<IDataPoint>)(object)item.DataPoints).Count; i++)
				{
					if (((Collection<IDataPoint>)(object)item.DataPoints)[i].AxisXLabel == "良率")
					{
						((Collection<IDataPoint>)(object)item.DataPoints)[i].YValue = ((Collection<IDataPoint>)(object)item.DataPoints)[i - 3].YValue / ((Collection<IDataPoint>)(object)item.DataPoints)[i - 1].YValue;
					}
					else if (((Collection<IDataPoint>)(object)item.DataPoints)[i].AxisXLabel == "总数")
					{
						((Collection<IDataPoint>)(object)item.DataPoints)[i].YValue = ((Collection<IDataPoint>)(object)item.DataPoints)[i - 2].YValue + ((Collection<IDataPoint>)(object)item.DataPoints)[i - 1].YValue;
					}
					else if (((Collection<IDataPoint>)(object)item.DataPoints)[i].AxisXLabel == "OK数")
					{
						((Collection<IDataPoint>)(object)item.DataPoints)[i].YValue = OkNum;
					}
					else if (((Collection<IDataPoint>)(object)item.DataPoints)[i].AxisXLabel == "NG数")
					{
						((Collection<IDataPoint>)(object)item.DataPoints)[i].YValue = NgNum;
					}
				}
			}
		}

		public void RaisePropertyChangedEvent(string PropertyName)
		{
			this.m_propertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
			DataDisp(dataChart);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
			base.SuspendLayout();
			this.elementHost1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.elementHost1.Font = new System.Drawing.Font("宋体", 13.8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
			this.elementHost1.Location = new System.Drawing.Point(0, 0);
			this.elementHost1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.elementHost1.Name = "elementHost1";
			this.elementHost1.Size = new System.Drawing.Size(372, 274);
			this.elementHost1.TabIndex = 0;
			this.elementHost1.Text = "elementHost1";
			this.elementHost1.Child = null;
			base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 15f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.Controls.Add(this.elementHost1);
			base.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			base.Name = "DataRecordPie";
			base.Size = new System.Drawing.Size(372, 274);
			base.ResumeLayout(false);
		}
	}
}
