using AlphaRap.PLC;
using System.Threading;
using static AlphaRap.VPForm;

namespace AlphaRap.TaskProcess
{
	public partial class Gantry : NPSDK.Flow_BaseForm  
	{
		public Gantry()
		{
			CheckForIllegalCrossThreadCalls = false;
			InitializeComponent();
            UiTheme.ApplyDpi(this);
		}

		INOVANCE plc = new INOVANCE();
		private const int VisionTimeoutMs = 5000;
		private long _visionRunId;
		private bool _visionFailed;
		private VisionPostData _visionCriterion;
		private VpStation _productionStation;
		public override void Initial()
		{
			GantryInit_Flow1_1.FlowChart_Run();
		}
		public override void PauseRun()
		{
		}
		public override void StartRun()
		{
			_visionFailed = false;
		}
		public override void StopRun()
		{
			if (_productionStation != null) _productionStation.CancelRun(_visionRunId);
			_productionStation = null;
			_visionRunId = 0;
			_visionFailed = false;
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
            NPSDK.Alarm.Show("3021");
            return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType GantryInit_Flow1_3_FlowChartRun()
		{
			
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType GantryInit_Flow1_4_FlowChartRun()
		{
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		/// <summary>校验本次判据、清除 PLC 完成标志，再启动具有独立编号的拍照请求。</summary>
		private NPSDK.Flow_Chart.ResultType FlowAuto_Vison3_2_FlowChartRun()
		{
			if (_visionFailed) return NPSDK.Flow_Chart.ResultType.IDLE;
			string error;
			if (!MiddleLayer.VPF.TryGetProductionStation(out _productionStation, out error)) return FailVision(error);
			if (!MiddleLayer.VPF.TryGetProductionVisionData(out _visionCriterion, out error)) return FailVision(error);
			if (!plc.WritePlc("D6210", "0")) return FailVision("清除视觉完成标志失败：" + plc.WritErrorMessage);
			if (!_productionStation.TryRunTB(out _visionRunId)) return FailVision(_productionStation.LastError);
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType GantryInit_Flow1_5_FlowChartRun()
		{
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}
		/// <summary>检查有限数值是否处于配方指定的开区间。</summary>
		private bool IsNumberInRange(double number, double minValue, double maxValue)
		{
			return IsFinite(number) && number > minValue && number < maxValue;
		}
		/// <summary>检查工位坐标是否满足生产判据，供现有调用方使用。</summary>
		public bool CheckVisionData(VisionPostData ts, VpStation h1)
		{
			return CheckVisionPosition(ts, h1.Fiducial.x, h1.Fiducial.y, h1.Fiducial.u);
		}
		/// <summary>判定结果快照，禁用范围检查时仍拒绝无效数值。</summary>
		private bool CheckVisionPosition(VisionPostData data, double x, double y, double angle)
		{
			if (!IsFinite(x) || !IsFinite(y) || !IsFinite(angle)) return false;
			if (!data.Enable) return true;
			return IsNumberInRange(x, data.X_Low, data.X_Hi)
				&& IsNumberInRange(y, data.Y_Low, data.Y_Hi)
				&& IsNumberInRange(angle, data.R_Low, data.R_Hi);
		}
		/// <summary>数值必须既不是 NaN 也不是无穷值。</summary>
		private static bool IsFinite(double value)
		{
			return !double.IsNaN(value) && !double.IsInfinity(value);
		}
		/// <summary>等待同编号拍照的终止状态；NG、异常及超时报警停留，不提交旧坐标。</summary>
		private NPSDK.Flow_Chart.ResultType FlowAuto_Vison3_3_FlowChartRun()
		{
			if (_visionFailed) return NPSDK.Flow_Chart.ResultType.IDLE;
			VpStation station = _productionStation;
			if (station == null) return FailVision("本周期生产工位已失效。");
			VisionRunResult result = station.LastRunResult;
			if (result == null || result.RunId != _visionRunId) return FailVision("视觉运行编号不一致。");
			if (result.State == VisionRunState.Running && FlowAuto_Vison3_3.FlowChart_ElapsedMilliseconds < VisionTimeoutMs)
				return NPSDK.Flow_Chart.ResultType.IDLE;
			station.TimeoutRun(_visionRunId);
			result = station.LastRunResult;
			if (result.State != VisionRunState.Succeeded) return FailVision(result.Error);
			if (!CheckVisionPosition(_visionCriterion, result.X, result.Y, result.Angle)) return FailVision("视觉定位超出配方允许范围。");
			if (!WriteVisionResult(result)) return FailVision("写入视觉结果失败：" + plc.WritErrorMessage);
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		/// <summary>坐标全部写入成功后才设置完成标志，任一写入失败均不发布完成。</summary>
		private bool WriteVisionResult(VisionRunResult result)
		{
			if (!plc.WritePlc("D6212", result.X.ToString())) return false;
			if (!plc.WritePlc("D6214", result.Y.ToString())) return false;
			if (!plc.WritePlc("D6216", result.Angle.ToString())) return false;
			return plc.WritePlc("D6210", "1");
		}

		/// <summary>记录本周期唯一一次视觉故障并触发已有视觉报警。</summary>
		private NPSDK.Flow_Chart.ResultType FailVision(string error)
		{
			if (_visionFailed) return NPSDK.Flow_Chart.ResultType.IDLE;
			_visionFailed = true;
			OperationLog.Write("Vision", "生产视觉失败：" + error);
			NPSDK.Alarm.Show("2009");
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
