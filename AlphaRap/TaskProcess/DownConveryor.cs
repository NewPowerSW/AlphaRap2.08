
namespace AlphaRap.TaskProcess
{
	public partial class DownConveryor : NPSDK.Flow_BaseForm
	{
		public DownConveryor()
		{
			CheckForIllegalCrossThreadCalls = false;
			InitializeComponent();
		}
		public override void Initial()
		{
		}
		public override void PauseRun()
		{
		}
		public override void StartRun()
		{
		}
		public override void StopRun()
		{
			//流道停止
			MiddleLayer.ManualF.OB_DownConveyor_SpeedSwitch.Off();
			MiddleLayer.ManualF.OB_DownConveyor_Forward.Off();
			//SMEMA交互off
			MiddleLayer.ManualF.OB_DownConveyor_LocalMachineAvailable_SMEMA.Off();
			MiddleLayer.ManualF.OB_DownConveyor_LocalMachineReady_SMEMA.Off();
		}

		private NPSDK.Flow_Chart.ResultType DownConveyInit_Flow1_1_FlowChartRun()
		{
			SysPara.DownConveyorInitialOk = false;

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyInit_Flow1_2_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_DownConveyor_Reverse.Off();
			MiddleLayer.ManualF.OB_DownConveyor_Forward.Off();

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyInit_Flow1_3_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_DownConveyor_Stop_Cylinder.On();
			bool r = MiddleLayer.ManualF.IB_DownConveyor_StopUp_Sensor.On();
			if (r)
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}

			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyInit_Flow1_4_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_DownConveyor_Forward.On();

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyInit_Flow1_5_FlowChartRun()
		{
			if (DownConveyInit_Flow1_5.FlowChart_ElapsedMilliseconds > 3000)
			{
				MiddleLayer.ManualF.OB_DownConveyor_Forward.Off();

				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyInit_Flow1_6_FlowChartRun()
		{
			SysPara.DownConveyorInitialOk = true;
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow1_1_FlowChartRun()
		{
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow1_2_FlowChartRun()
		{
			bool r = MiddleLayer.ManualF.IB_DownConveyor_BoardStop_Sensor.On();

			if (!r)
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.CASE1;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow1_4_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_DownConveyor_Stop_Cylinder.On();
			bool r = MiddleLayer.ManualF.IB_DownConveyor_StopUp_Sensor.On();
			if (r)
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			if (DownConveyorAuto_Flow1_4.FlowChart_ElapsedMilliseconds > 5000)
			{
				MiddleLayer.MainF.AddErrorLog("Alarm:" + MiddleLayer.HomeF.GetAlarmConent("4") + "- " + "Position:" + DownConveyorAuto_Flow1_4.text);
				NPSDK.Alarm.Show("4");
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow1_5_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_DownConveyor_LocalMachineReady_SMEMA.On();

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow1_6_FlowChartRun()
		{
			bool r = MiddleLayer.ManualF.IB_DownConveyor_UpMachineAvailable_SMEMA.On();
			if (r)
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow1_7_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_DownConveyor_Forward.On();

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow1_8_FlowChartRun()
		{
			bool r = MiddleLayer.ManualF.IB_DownConveyor_BoardIn_Sensor.On();
			if (r)
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow1_9_FlowChartRun()
		{
			bool r = MiddleLayer.ManualF.IB_DownConveyor_BoardIn_Sensor.On();
			if (!r)
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow2_0_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_DownConveyor_LocalMachineReady_SMEMA.Off();

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow2_1_FlowChartRun()
		{
			MiddleLayer.ManualF.IB_DownConveyor_BoardSpeedSwitch_Sensor.On();

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow2_2_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_DownConveyor_SpeedSwitch.On();

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow2_3_FlowChartRun()
		{
			bool r = MiddleLayer.ManualF.IB_DownConveyor_BoardStop_Sensor.On();
			if (r)
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow2_4_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_DownConveyor_SpeedSwitch.Off();
			MiddleLayer.ManualF.OB_DownConveyor_Forward.Off();

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow1_3_FlowChartRun()
		{
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow2_5_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_DownConveyor_LocalMachineAvailable_SMEMA.On();

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow2_6_FlowChartRun()
		{
			bool r = MiddleLayer.ManualF.IB_DownConveyor_DownMachineReady_SMEMA.On();
			if (r)
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow2_7_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_DownConveyor_Stop_Cylinder.Off();
			bool r = MiddleLayer.ManualF.IB_DownConveyor_StopDown_Sensor.On();
			if (r)
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			if (DownConveyorAuto_Flow2_7.FlowChart_ElapsedMilliseconds > 5000)
			{
				MiddleLayer.MainF.AddErrorLog("Alarm:" + MiddleLayer.HomeF.GetAlarmConent("11") + "- " + "Position:" + DownConveyorAuto_Flow2_7.text);
				NPSDK.Alarm.Show("11");
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow2_8_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_DownConveyor_Forward.On();

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow2_9_FlowChartRun()
		{
			bool r = MiddleLayer.ManualF.IB_DownConveyor_BoardOut_Sensor.On();
			if (r)
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow3_0_FlowChartRun()
		{
			bool r = MiddleLayer.ManualF.IB_DownConveyor_BoardOut_Sensor.On();
			if (!r)
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			if (DownConveyorAuto_Flow3_0.FlowChart_ElapsedMilliseconds > 8000)
			{
				MiddleLayer.MainF.AddErrorLog("Alarm:" + MiddleLayer.HomeF.GetAlarmConent("12") + "- " + "Position:" + DownConveyorAuto_Flow3_0.text);
				NPSDK.Alarm.Show("12");
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow3_1_FlowChartRun()
		{
			bool r = MiddleLayer.ManualF.IB_DownConveyor_DownMachineReady_SMEMA.On();
			if (!r)
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow3_2_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_DownConveyor_Forward.Off();

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow3_3_FlowChartRun()
		{
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow3_4_FlowChartRun()
		{
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow3_5_FlowChartRun()
		{
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}
	}
}
