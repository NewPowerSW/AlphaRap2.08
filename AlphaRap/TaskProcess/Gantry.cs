using AlphaRap.PLC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AlphaRap.VPForm;

namespace AlphaRap.TaskProcess
{
	public partial class Gantry : NPSDK.Flow_BaseForm
	{
		public Gantry()
		{
			CheckForIllegalCrossThreadCalls = false;
			InitializeComponent();
		}

		INOVANCE plc = new INOVANCE();
		/// <summary>
		/// 产品数据
		/// </summary>
		struct ProductMessage
		{
			public string barcode;
		}
		ProductMessage product = new ProductMessage();
		struct VisonData
		{
			double X;
			double Y;
			double R;
		}
		public override void Initial()
		{
			GantryInit_Flow1_1.FlowChart_Run();

		}
		public override void PauseRun()
		{
			
		}
		public override void StartRun()
		{

		}
		public override void StopRun()
		{

		}

		private NPSDK.Flow_Chart.ResultType ScrewInit_Flow1_1_FlowChartRun()
		{

			SysPara.GantryInitialOK = false;

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType ScrewInit_Flow1_2_FlowChartRun()
		{

			SysPara.GantryInitialOK = true;
			return NPSDK.Flow_Chart.ResultType.NEXT;


		}

		private NPSDK.Flow_Chart.ResultType FlowAuto_Scann2_1_FlowChartRun()
		{
			string readPLCValue = plc.ReadPlc("D6000", 2);

			if (readPLCValue == "1")
			{
				plc.WritePlc("D6000", "2");
				NPSDK.Alarm.Show("4015");
			}
			if (readPLCValue == "-999")
			{
				MiddleLayer.DataF.AddLogError(MiddleLayer.HomeF.GetAlarmConent("4013") + "---------------->>>" + FlowAuto_Scann2_1.text);
				NPSDK.Alarm.Show("4013");
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType FlowAuto_Scann2_2_FlowChartRun()
		{
			string IP = MiddleLayer.ParF.GetSettingValue("PSet", "txtIP1");
			int Port = MiddleLayer.ParF.GetSettingValue("PSet", "Port");
			MiddleLayer.ParF.OPTScann.Connect(IP, Port);
			if (!MiddleLayer.ParF.OPTScann.ConnectStatus())
			{
				bool r = MiddleLayer.ParF.OPTScann.Connect(IP, Port);
				if (!r)
				{
					MiddleLayer.DataF.AddLogError(MiddleLayer.HomeF.GetAlarmConent("4014"));
					NPSDK.Alarm.Show("4014");
					return NPSDK.Flow_Chart.ResultType.IDLE;
				}

			}

			MiddleLayer.ParF.OPTScann.Sent("start");
			Thread.Sleep(300);
			product.barcode = MiddleLayer.ParF.OPTScann.Receive();
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType FlowAuto_Scann2_3_FlowChartRun()
		{
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType FlowAuto_Scann2_4_FlowChartRun()
		{
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType FlowAuto_Scann2_5_FlowChartRun()
		{
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType FlowAuto_Vison3_1_FlowChartRun()
		{
			string readPLCValue = plc.ReadPlc("D6020", 2);

			if (readPLCValue == "1")
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			if (readPLCValue == "-999")
			{
				NPSDK.Alarm.Show("4013");
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType FlowAuto_Heart5_1_FlowChartRun()
		{
			plc.WritePlc("D6802", "1");
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType GantryInit_Flow1_2_FlowChartRun()
		{
			string IP = MiddleLayer.ParF.GetSettingValue("MSet", "ScannIP");
			int Port = MiddleLayer.ParF.GetSettingValue("MSet", "ScannPort");
			if (!MiddleLayer.ParF.OPTScann.ConnectStatus())
			{
				if (MiddleLayer.ParF.OPTScann.Connect(IP, Port))
				{
					return NPSDK.Flow_Chart.ResultType.NEXT;
				}
				NPSDK.Alarm.Show("4014");
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;


		}

		private NPSDK.Flow_Chart.ResultType GantryInit_Flow1_3_FlowChartRun()
		{
			string PLCIP = MiddleLayer.ParF.GetSettingValue("MSet", "PLCIP");
			int nIpPort = MiddleLayer.ParF.GetSettingValue("MSet", "PLCPort"); ;


			if (!MiddleLayer.ParF.OPTScann.ConnectStatus())
			{
				if (INOVANCE.Init_ETH_String(PLCIP, 0, nIpPort))
				{
					return NPSDK.Flow_Chart.ResultType.NEXT;
				}
				NPSDK.Alarm.Show("4013");
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;


		}

		private NPSDK.Flow_Chart.ResultType GantryInit_Flow1_4_FlowChartRun()
		{
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType FlowAuto_Vison3_2_FlowChartRun()
		{
			MiddleLayer.VPF.H1_VFiducial.RunTB();

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType GantryInit_Flow1_5_FlowChartRun()
		{
			MiddleLayer.VPF.Getdgv_H1_VisionDataList();
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}
		private bool IsNumberInRange(double number, double minValue, double maxValue)
		{
			if (number > minValue && number < maxValue)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		public bool CheckVisionData(VisionPostData ts, H1_Vision_Fiducial h1)
		{
			if (!ts.Enable)
				return true;
			if (IsNumberInRange(h1.Fiducial.x, ts.X_Low, ts.X_Hi) && IsNumberInRange(h1.Fiducial.y, ts.Y_Low, ts.Y_Hi) && IsNumberInRange(h1.Fiducial.u, ts.R_Low, ts.R_Hi))
			{
				return true;
			}
			return false;
		}
		private NPSDK.Flow_Chart.ResultType FlowAuto_Vison3_3_FlowChartRun()
		{
			if (MiddleLayer.VPF.H1_VFiducial.RunTBOk())
			{
				if (MiddleLayer.VPF.H1_VFiducial.IsAccept)
				{
					if (CheckVisionData(MiddleLayer.VPF.dgv_H1_VisionData_List[0], MiddleLayer.VPF.H1_VFiducial))
					{

						plc.WritePlc("D6210", "1");
						plc.WritePlc("D6212", MiddleLayer.VPF.H1_VFiducial.Fiducial.x.ToString());
						plc.WritePlc("D6214", MiddleLayer.VPF.H1_VFiducial.Fiducial.y.ToString());
						plc.WritePlc("D6216", MiddleLayer.VPF.H1_VFiducial.Fiducial.u.ToString());
						return NPSDK.Flow_Chart.ResultType.NEXT;
					}
				}

			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType FlowAuto_Vison3_4_FlowChartRun()
		{
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType FlowAuto_Vison3_5_FlowChartRun()
		{
			return NPSDK.Flow_Chart.ResultType.NEXT;

		}

		private NPSDK.Flow_Chart.ResultType FlowAuto_Heart5_2_FlowChartRun()
		{
			if(FlowAuto_Heart5_2.FlowChart_ElapsedMilliseconds>5000)
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType FlowAuto_Heart5_3_FlowChartRun()
		{
			string readPLCValue = plc.ReadPlc("6804", 2);

			if (readPLCValue == "1")
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			if (readPLCValue == "-999")
			{
				MiddleLayer.DataF.AddLogError(MiddleLayer.HomeF.GetAlarmConent("4013") + "---------------->>>" + FlowAuto_Heart5_3.text);
				NPSDK.Alarm.Show("4013");
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType FlowAuto_MES4_4_FlowChartRun()
		{
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType FlowAuto_Heart5_4_FlowChartRun()
		{
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}
	}
}

