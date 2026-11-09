using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using System.Threading;
using System.CodeDom;
using System.Diagnostics;
using static NPSDK.Alarm;

namespace NPSDK
{

	[DefaultEvent("FlowChartRun")]
	public partial class Flow_Chart : UserControl
	{
		private static readonly object write = new object();
		Stopwatch Elapsedtimer = new Stopwatch();
		System.Timers.Timer Timer;
		private Flow_Chart _Next;
		private Flow_Chart _Casse1;
		private Flow_Chart _Casse2;
		private Flow_Chart _Casse3;

		public bool Flow_ChartIsCanPause;
			
		public bool Flow_ChartIsCanStop;

		public volatile bool RunningStatus = false;



		public delegate ResultType FlowChartHandle();

		public event FlowChartHandle FlowChartRun;

		/// <summary>读取原生事件处理器的独立快照，不执行事件或启动流程。</summary>
		public FlowChartHandle[] GetFlowChartHandlers()
		{
			FlowChartHandle handlers = FlowChartRun;
			return handlers == null ? new FlowChartHandle[0] : handlers.GetInvocationList().Cast<FlowChartHandle>().ToArray();
		}



		public DateTime FlowChart_StartTime = new DateTime();
		public DateTime FlowChart_EndTime = new DateTime();

		/// <summary>初始化节点界面并登记控件，不创建执行线程。</summary>
		public Flow_Chart()
		{

			InitializeComponent();
			this.FlowChartRun = null;
			Flow_Module.FlowChart_List.Add(this);

		}
		public enum ResultType
		{
			NEXT = 0,
			CASE1 = 1,
			CASE2 = 2,
			CASE3 = 3,
			IDLE = 4,
			End = 5
		}

		private string gName;
		[Browsable(true)]
		[Description("设置显示信息"), DefaultValue(100)]

		public string Name
		{
			get
			{
				return gName;
			}
			set
			{
				gName = value;

			}
		}

		private string gText;
		[Browsable(true)]
		[Description(""), Category("外观"), DefaultValue("")]
		public string text
		{
			get
			{
				gText = button1.Text;
				return button1.Text;
			}
			set
			{

				gText = value;
				button1.Text = gText;
				this.Refresh();

			}
		}

		[Browsable(true)]
		[Editor(typeof(ScopeDropDownEditor), typeof(UITypeEditor))]

		[Category("#Flow_Chart")]

		public Flow_Chart Casse1
		{
			get
			{
				return _Casse1;
			}
			set
			{

				_Casse1 = value;
				Invalidate();
				Parent?.Invalidate();
			}
		}

		[Browsable(true)]
		[Editor(typeof(ScopeDropDownEditor), typeof(UITypeEditor))]
		// [TypeConverter(typeof(FundStatusConverter))]
		[Category("#Flow_Chart")]
		// [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]

		public Flow_Chart Next
		{
			get
			{

				return _Next;
			}
			set
			{

				_Next = value;
				Invalidate();
				Parent?.Invalidate();
			}
		}

		[Browsable(true)]
		[Editor(typeof(ScopeDropDownEditor), typeof(UITypeEditor))]
		// [TypeConverter(typeof(FundStatusConverter))]
		[Category("#Flow_Chart")]
		// [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]

		public Flow_Chart Casse2
		{
			get
			{
				return _Casse2;
			}
			set
			{
				_Casse2 = value;
				Invalidate();
				Parent?.Invalidate();
			}
		}

		[Browsable(true)]
		[Editor(typeof(ScopeDropDownEditor), typeof(UITypeEditor))]
		// [TypeConverter(typeof(FundStatusConverter))]
		[Category("#Flow_Chart")]
		// [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]

		public Flow_Chart Casse3
		{
			get
			{
				return _Casse3;

			}
			set
			{

				_Casse3 = value;
				Invalidate();
				Parent?.Invalidate();
			}
		}

		private bool gAlarmStaus;
		[Browsable(true)]
		[Description("IsAlarm"), Category("Alarm"), DefaultValue("")]
		public bool AlarmStaus
		{
			get
			{

				return gAlarmStaus;
			}
			set
			{

				gAlarmStaus = value;

			}
		}
		private string gAlarmCode;
		[Browsable(true)]
		[Description("Alarm Code"), Category("Alarm"), DefaultValue("")]
		public string AlarmCode
		{
			get
			{

				return gAlarmCode;
			}
			set
			{

				gAlarmCode = value;

			}
		}
		private int gAlarmTimeOut;
		[Browsable(true)]
		[Description("Alarm TimeOut/ms"), Category("Alarm"), DefaultValue("")]
		public int AlarmTimeOut
		{
			get
			{

				return gAlarmTimeOut;
			}
			set
			{

				gAlarmTimeOut = value;

			}
		}
		private Form FindForm(Control c)
		{
			if (c is Form)
				return c as Form;
			else if (c.Parent != null)
				return FindForm(c.Parent);
			else
				return null;
		}


		public static bool bStopWork = false;

		public long FlowChart_ElapsedMilliseconds
		{
			get
			{
                lock (Elapsedtimer) return Elapsedtimer.ElapsedMilliseconds;
			}

			set
			{
                lock (Elapsedtimer) Elapsedtimer.Restart();
			}
		}

		private void FlowChart_Load(object sender, EventArgs e)
		{
			// this.Font = this.Parent.Font;
			if (this.GetService(typeof(System.ComponentModel.Design.IDesignerHost)) != null || System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)

			{      //design mode
				   //Timer = new System.Timers.Timer(100);
				   //Timer.Elapsed += new System.Timers.ElapsedEventHandler(theout); //到达时间的时候执行事件；   
				   //Timer.AutoReset = true;                                         //设置是执行一次（false）还是一直执行(true)；   
				   //Timer.Enabled = true;
			}

		}
		public void theout(object source, System.Timers.ElapsedEventArgs e)
		{
			Timer.Enabled = false;
			Draw();
			Timer.Enabled = true;

		}
		private void FlowChart_Paint(object sender, PaintEventArgs e)
		{

			if (this.Parent != null && this.Parent.Visible)
			{
				Draw();
			}

		}
		void Draw()
		{
			lock (write)
			{
				try
				{

					if (Next != null)
					{

						if (!_Next.IsDisposed)
						{

							DrawLine(Next, Brushes.Black);
						}
					}

					if (Casse1 != null)
					{
						if (!_Casse1.IsDisposed)
						{
							DrawLine(Casse1, Brushes.SpringGreen);
						}
					}

					if (Casse2 != null)
					{
						if (!_Casse2.IsDisposed)
						{
							DrawLine(Casse2, Brushes.Red);
						}
					}

					if (Casse3 != null)
					{
						if (!_Casse3.IsDisposed)
						{
							DrawLine(Casse3, Brushes.Blue);
						}
					}

				}

				catch (Exception e)
				{



				}

				finally
				{

				}
			}
		}
		void DrawLine(Control _Control, Brush _Brush)
		{

			Graphics g = this.Parent.CreateGraphics();
			Pen _pen = new Pen(Brushes.Black, 2);
			System.Drawing.Drawing2D.AdjustableArrowCap lineCap =
			new System.Drawing.Drawing2D.AdjustableArrowCap(4, 4, true);
			_pen.CustomEndCap = lineCap;

			PointF px1;
			PointF px2;
			////下边,左边
			if (_Control.Location.X + _Control.Width < this.Location.X && _Control.Location.Y > this.Location.Y + this.Height)
			{
				px1 = new PointF(this.Location.X, this.Location.Y + this.Height);
				px2 = new PointF(_Control.Location.X + _Control.Width, _Control.Location.Y);
			}




			////下边,右边
			else if (_Control.Location.X > this.Location.X + this.Width && _Control.Location.Y > this.Location.Y + this.Height)
			{
				px1 = new PointF(this.Location.X + this.Width, this.Location.Y + this.Height);
				px2 = new PointF(_Control.Location.X, _Control.Location.Y);
			}


			////下边,中间
			else if (((_Control.Location.X + _Control.Width > this.Location.X && _Control.Location.X + _Control.Width < this.Location.X + this.Width) || (_Control.Location.X > this.Location.X && _Control.Location.X < this.Location.X + this.Width)) && _Control.Location.Y > this.Location.Y + this.Height)
			{
				px1 = new PointF(this.Location.X + this.Width / 2, this.Location.Y + this.Height);
				px2 = new PointF(_Control.Location.X + _Control.Width / 2, _Control.Location.Y);
			}


			////上边,左边
			else if (_Control.Location.X + _Control.Width < this.Location.X && _Control.Location.Y + _Control.Height < this.Location.Y)
			{
				px1 = new PointF(this.Location.X, this.Location.Y);
				px2 = new PointF(_Control.Location.X + _Control.Width, _Control.Location.Y + _Control.Height);

				/*  px1 = new PointF(this.Location.X, this.Location.Y + this.Height / 2);
                  px2 = new PointF(_Control.Location.X + _Control.Width / 2, this.Location.Y + this.Height / 2);
                  Pen _pen1 = new Pen(_Brush, 2);
                  _pen.Brush = _Brush;
                  g.DrawLine(_pen1, px1, px2);
                  _pen1.Dispose();

                  px1 = px2;
                  px2 = new PointF(_Control.Location.X + _Control.Width / 2, _Control.Location.Y + _Control.Height);*/


			}
			////上边,右边
			else if (_Control.Location.X > this.Location.X + this.Width && _Control.Location.Y + _Control.Height < this.Location.Y)
			{
				px1 = new PointF(this.Location.X + this.Width, this.Location.Y);
				px2 = new PointF(_Control.Location.X, _Control.Location.Y + _Control.Height);

				/*   px1 = new PointF(this.Location.X + this.Width, this.Location.Y+ this.Height/2);
                   px2 = new PointF(_Control.Location.X+ _Control.Width/2, this.Location.Y + this.Height / 2);
                   Pen _pen1 = new Pen(_Brush, 2);
                   _pen.Brush = _Brush;
                   g.DrawLine(_pen1, px1, px2);
                   _pen1.Dispose();

                   px1 = px2;
                   px2 = new PointF(_Control.Location.X+ _Control.Width / 2, _Control.Location.Y + _Control.Height);*/
			}



			////上边,中间
			else if (((_Control.Location.X + _Control.Width >= this.Location.X && _Control.Location.X + _Control.Width <= this.Location.X + this.Width) || (_Control.Location.X >= this.Location.X && _Control.Location.X <= this.Location.X + this.Width)) && _Control.Location.Y + _Control.Height <= this.Location.Y)
			{
				px1 = new PointF(this.Location.X + this.Width / 2, this.Location.Y);
				px2 = new PointF(_Control.Location.X + _Control.Width / 2, _Control.Location.Y + _Control.Height);
			}

			////侧右边，中间
			else if (((_Control.Location.Y >= this.Location.Y && _Control.Location.Y <= this.Location.Y + this.Height) || (_Control.Location.Y + _Control.Height >= this.Location.Y && _Control.Location.Y + _Control.Height <= this.Location.Y + this.Height)) && _Control.Location.X > this.Location.X + this.Width)
			{
				px1 = new PointF(this.Location.X + this.Width, this.Location.Y + this.Height / 2);
				px2 = new PointF(_Control.Location.X, _Control.Location.Y + _Control.Height / 2);
			}
			////侧左边，中间
			else if (((_Control.Location.Y >= this.Location.Y && _Control.Location.Y <= this.Location.Y + this.Height) || (_Control.Location.Y + _Control.Height >= this.Location.Y && _Control.Location.Y + _Control.Height <= this.Location.Y + this.Height)) && _Control.Location.X + _Control.Width < this.Location.X)
			{
				px1 = new PointF(this.Location.X, this.Location.Y + this.Height / 2);
				px2 = new PointF(_Control.Location.X + _Control.Width, _Control.Location.Y + _Control.Height / 2);
			}

			else
			{
				px1 = new PointF(this.Location.X + this.Width / 2, this.Location.Y + this.Height);
				px2 = new PointF(_Control.Location.X + _Control.Width / 2, _Control.Location.Y);

			}

			_pen.Brush = _Brush;
			g.DrawLine(_pen, px1, px2);

			g.Dispose();
			_pen.Dispose();

		}

		private void Flow_Chart_FontChanged(object sender, EventArgs e)
		{
			button1.Font = Font;

		}


	}


}




