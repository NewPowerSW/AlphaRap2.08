
namespace AlphaRap.TaskProcess
{
	public partial class DownConveryor : NPSDK.Flow_BaseForm
	{
		public DownConveryor()
		{
			CheckForIllegalCrossThreadCalls = false;
			InitializeComponent();
            UiTheme.ApplyDpi(this);
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
			StationApplication.Manual.OB_DownConveyor_SpeedSwitch.Off();
			StationApplication.Manual.OB_DownConveyor_Forward.Off();
			//SMEMA交互off
			StationApplication.Manual.OB_DownConveyor_LocalMachineAvailable_SMEMA.Off();
			StationApplication.Manual.OB_DownConveyor_LocalMachineReady_SMEMA.Off();
		}

		private NPSDK.Flow_Chart.ResultType DownConveyInit_Flow1_1_FlowChartRun()
		{
			StationData.DownConveyorInitialOk = false;

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyInit_Flow1_2_FlowChartRun()
		{
			StationApplication.Manual.OB_DownConveyor_Reverse.Off();
			StationApplication.Manual.OB_DownConveyor_Forward.Off();

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyInit_Flow1_3_FlowChartRun()
		{
			StationApplication.Manual.OB_DownConveyor_Stop_Cylinder.On();
			bool r = StationApplication.Manual.IB_DownConveyor_StopUp_Sensor.IsOn();
			if (r)
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}

			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyInit_Flow1_4_FlowChartRun()
		{
			StationApplication.Manual.OB_DownConveyor_Forward.On();

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyInit_Flow1_5_FlowChartRun()
		{
			if (DownConveyInit_Flow1_5.FlowChart_ElapsedMilliseconds > 3000)
			{
				StationApplication.Manual.OB_DownConveyor_Forward.Off();

				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyInit_Flow1_6_FlowChartRun()
		{
			StationData.DownConveyorInitialOk = true;
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow1_1_FlowChartRun()
		{
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow1_2_FlowChartRun()
		{
			bool r = StationApplication.Manual.IB_DownConveyor_BoardStop_Sensor.IsOn();

			if (!r)
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.CASE1;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow1_4_FlowChartRun()
		{
			StationApplication.Manual.OB_DownConveyor_Stop_Cylinder.On();
			bool r = StationApplication.Manual.IB_DownConveyor_StopUp_Sensor.IsOn();
			if (r)
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			if (DownConveyorAuto_Flow1_4.FlowChart_ElapsedMilliseconds > 5000)
			{
				ApplicationHost.Data.AddLogError("Alarm:" + ApplicationHost.GetAlarmText("4") + "- " + "Position:" + DownConveyorAuto_Flow1_4.text);
				NPSDK.Alarm.Show("4");
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow1_5_FlowChartRun()
		{
			StationApplication.Manual.OB_DownConveyor_LocalMachineReady_SMEMA.On();

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow1_6_FlowChartRun()
		{
			bool r = StationApplication.Manual.IB_DownConveyor_UpMachineAvailable_SMEMA.IsOn();
			if (r)
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow1_7_FlowChartRun()
		{
			StationApplication.Manual.OB_DownConveyor_Forward.On();

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow1_8_FlowChartRun()
		{
			bool r = StationApplication.Manual.IB_DownConveyor_BoardIn_Sensor.IsOn();
			if (r)
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow1_9_FlowChartRun()
		{
			bool r = StationApplication.Manual.IB_DownConveyor_BoardIn_Sensor.IsOn();
			if (!r)
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow2_0_FlowChartRun()
		{
			StationApplication.Manual.OB_DownConveyor_LocalMachineReady_SMEMA.Off();

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow2_1_FlowChartRun()
		{
			StationApplication.Manual.IB_DownConveyor_BoardSpeedSwitch_Sensor.IsOn();

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow2_2_FlowChartRun()
		{
			StationApplication.Manual.OB_DownConveyor_SpeedSwitch.On();

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow2_3_FlowChartRun()
		{
			bool r = StationApplication.Manual.IB_DownConveyor_BoardStop_Sensor.IsOn();
			if (r)
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow2_4_FlowChartRun()
		{
			StationApplication.Manual.OB_DownConveyor_SpeedSwitch.Off();
			StationApplication.Manual.OB_DownConveyor_Forward.Off();

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow1_3_FlowChartRun()
		{
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow2_5_FlowChartRun()
		{
			StationApplication.Manual.OB_DownConveyor_LocalMachineAvailable_SMEMA.On();

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow2_6_FlowChartRun()
		{
			bool r = StationApplication.Manual.IB_DownConveyor_DownMachineReady_SMEMA.IsOn();
			if (r)
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow2_7_FlowChartRun()
		{
			StationApplication.Manual.OB_DownConveyor_Stop_Cylinder.Off();
			bool r = StationApplication.Manual.IB_DownConveyor_StopDown_Sensor.IsOn();
			if (r)
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			if (DownConveyorAuto_Flow2_7.FlowChart_ElapsedMilliseconds > 5000)
			{
				ApplicationHost.Data.AddLogError("Alarm:" + ApplicationHost.GetAlarmText("11") + "- " + "Position:" + DownConveyorAuto_Flow2_7.text);
				NPSDK.Alarm.Show("11");
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow2_8_FlowChartRun()
		{
			StationApplication.Manual.OB_DownConveyor_Forward.On();

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow2_9_FlowChartRun()
		{
			bool r = StationApplication.Manual.IB_DownConveyor_BoardOut_Sensor.IsOn();
			if (r)
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow3_0_FlowChartRun()
		{
			bool r = StationApplication.Manual.IB_DownConveyor_BoardOut_Sensor.IsOn();
			if (!r)
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			if (DownConveyorAuto_Flow3_0.FlowChart_ElapsedMilliseconds > 8000)
			{
				ApplicationHost.Data.AddLogError("Alarm:" + ApplicationHost.GetAlarmText("12") + "- " + "Position:" + DownConveyorAuto_Flow3_0.text);
				NPSDK.Alarm.Show("12");
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow3_1_FlowChartRun()
		{
			bool r = StationApplication.Manual.IB_DownConveyor_DownMachineReady_SMEMA.IsOn();
			if (!r)
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType DownConveyorAuto_Flow3_2_FlowChartRun()
		{
			StationApplication.Manual.OB_DownConveyor_Forward.Off();

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
