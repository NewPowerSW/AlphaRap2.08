using NPSDK;
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

namespace AlphaRap.TaskProcess
{
	public partial class UpConveryor : NPSDK.Flow_BaseForm
	{
		public UpConveryor()
		{
			CheckForIllegalCrossThreadCalls = false;
			InitializeComponent();
		}
		public override void Initial()
		{
			UPConveyInit_Flow1_1.FlowChart_Run();

		}
		public override void PauseRun()
		{

		}
		public override void StartRun()
		{
			
			
			//UPConveyorAuto_Flow2_01.FlowChart_Run();
			//UPConveyorAuto_Flow1_01.FlowChart_Run();

		}
	
		public override void StopRun()
		{
			//流道停止
			ConveyorStop(1);
			ConveyorStop(2);

			//SMEMA交互off
			MiddleLayer.ManualF.OB_UpConveyor_LocalMachineReady_SMEMA.Off();
			MiddleLayer.ManualF.OB_UpConveyor_LocalMachineAvailable_SMEMA.Off();
		
		}
		#region 流道
	
		readonly object obj = new object();
		bool UpConveyor2StartStatus = false;
		bool UpConveyor1StartStatus = false;

		public void ConveyorStart(int num)
		{
			lock (obj)
			{
				if (num == 1)
				{

					UpConveyor1StartStatus = true;


				}
				else
				{

					UpConveyor2StartStatus = true;


				}

				if (!MiddleLayer.ManualF.IB_UpConveyor_Work_BoardAlarm.On())
				{
					MiddleLayer.ManualF.OB_UpConveyor_Work_ConveyorForward.On();
				}
				else
				{
					MiddleLayer.ManualF.OB_Upconveyor_AlarmClear.On();
					Thread.Sleep(300);
					NPSDK.Alarm.Show("7020");
				}
			}
		}
		public void ConveyorStop(int num)
		{
			lock (obj)
			{

				if (num == 1)
				{
					if (UpConveyor2StartStatus == false)
					{

						MiddleLayer.ManualF.OB_UpConveyor_Work_ConveyorForward.Off();

					}

					UpConveyor1StartStatus = false;
				}

				if (num == 2)
				{

					if (UpConveyor1StartStatus == false)
					{


						MiddleLayer.ManualF.OB_UpConveyor_Work_ConveyorForward.Off();


					}

					UpConveyor2StartStatus = false;

				}

			}

		}
		#endregion
		/// <summary>
		/// Interactive signal 
		/// </summary>
		public bool BufferConveyorReady = false;
		public bool WorkConveyorReady = false;
		public bool UpConveyorProductReady = false;
		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_30_FlowChartRun()
		{
			return SysPara.bByPass ? NPSDK.Flow_Chart.ResultType.CASE1 : NPSDK.Flow_Chart.ResultType.NEXT;
		}
		private NPSDK.Flow_Chart.ResultType UPConveyInit_Flow1_1_FlowChartRun()
		{
			UpConveyorProductReady = false;
			BufferConveyorReady = false;
			SysPara.UpConveyorInitialOk = false;
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyInit_Flow1_3_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_UpConveyor_Buffer_ConveyorForward.Off();
			MiddleLayer.ManualF.OB_UpConveyor_Buffer_ConveyorReverse.Off();
		
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyInit_Flow1_4_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_UpConveyor_Work_ConveyorForward.Off();
			MiddleLayer.ManualF.OB_UpConveyor_Work_ConveyorReverse.Off();
			
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow1_01_FlowChartRun()
		{
			
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyInit_Flow2_3_FlowChartRun()
		{
			SysPara.UpConveyorInitialOk = true;
		
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyInit_Flow1_6_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_UpConveyor_Buffer_Stop_Cylinder.On();
			bool r = MiddleLayer.ManualF.IB_UpConveyor_Buffer_StopUp_Sensor.On();
			if (r)
			{

			
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyInit_Flow1_7_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_UpConveyor_Work_Jacking_Cylinder.Off();
			bool r = MiddleLayer.ManualF.IB_UpConveyor_Work_JackingDown_Sensor.On();
			if (r)
			{
				

				return NPSDK.Flow_Chart.ResultType.NEXT;
			}

			
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyInit_Flow1_8_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_UpConveyor_Work_Stop_Cylinder.On();

			bool r = MiddleLayer.ManualF.IB_UpConveyor_Work_StopUp_Sensor.On();
			if (r)
			{
				
				return NPSDK.Flow_Chart.ResultType.NEXT;

			}

			
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		

		private NPSDK.Flow_Chart.ResultType UPConveyInit_Flow2_1_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_UpConveyor_Buffer_ConveyorForward.On();
			MiddleLayer.ManualF.OB_UpConveyor_Work_ConveyorForward.On();
		
		
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyInit_Flow2_2_FlowChartRun()
		{
			if (UPConveyInit_Flow2_2.FlowChart_ElapsedMilliseconds > 3000)
			{
				MiddleLayer.ManualF.OB_UpConveyor_Buffer_ConveyorForward.Off();
				MiddleLayer.ManualF.OB_UpConveyor_Work_ConveyorForward.Off();
			
				
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow1_02_FlowChartRun()
		{

			if (!MiddleLayer.ManualF.IB_UpConveyor_Buffer_BoardStop_Sensor.On())
			{
				
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.CASE1;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow1_04_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_UpConveyor_Buffer_Stop_Cylinder.On();

			bool Buffer_StopUp_Sensor_Staus = MiddleLayer.ManualF.IB_UpConveyor_Buffer_StopUp_Sensor.On();
			if (Buffer_StopUp_Sensor_Staus)
			{
				
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			
			return NPSDK.Flow_Chart.ResultType.IDLE;

		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow1_05_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_UpConveyor_LocalMachineReady_SMEMA.On();
		
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}
		
		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow1_06_FlowChartRun()
		{
			bool r = MiddleLayer.ManualF.IB_UpConveyor_UpMachineAvailable_SMEMA.On();
			if (r)
			{
				
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow1_07_FlowChartRun()
		{
			ConveyorStart(1);

			return NPSDK.Flow_Chart.ResultType.NEXT;

		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow1_08_FlowChartRun()
		{
			bool r = MiddleLayer.ManualF.IB_UpConveyor_Buffer_BoardIn_Sensor.On();
			if (r)
			{
			
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow1_09_FlowChartRun()
		{
			bool r = MiddleLayer.ManualF.IB_UpConveyor_Buffer_BoardIn_Sensor.On();
			if (!r)
			{
				
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow1_10_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_UpConveyor_LocalMachineReady_SMEMA.Off();
			
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow1_11_FlowChartRun()
		{
			bool r = MiddleLayer.ManualF.IB_UpConveyor_Buffer_BoardSpeedSwitch_Sensor.On();
			if (r)
			{
				
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow1_12_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_UpConveyor_Buffer_ConveyorSpeedSwitch.On();
		
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow1_13_FlowChartRun()
		{
			bool r = MiddleLayer.ManualF.IB_UpConveyor_Buffer_BoardStop_Sensor.On();
			if (r)
			{
				Thread.Sleep(1500);
				
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow1_14_FlowChartRun()
		{
			ConveyorStop(1);

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow1_03_FlowChartRun()
		{
			
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow1_15_FlowChartRun()
		{
			BufferConveyorReady = true;
		
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow1_16_FlowChartRun()
		{
			if (WorkConveyorReady)
			{
				
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow1_17_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_UpConveyor_Buffer_Stop_Cylinder.Off();
			bool r = MiddleLayer.ManualF.IB_UpConveyor_Buffer_StopDown_Sensor.On();
			if (r)
			{

				
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow1_18_FlowChartRun()
		{
	
			ConveyorStart(1);
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow1_19_FlowChartRun()
		{
			if (!WorkConveyorReady)
			{
				
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow1_20_FlowChartRun()
		{
			ConveyorStop(1);

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow1_21_FlowChartRun()
		{
		
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow1_22_FlowChartRun()
		{
		
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow1_23_FlowChartRun()
		{
			
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_01_FlowChartRun()
		{
			
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_02_FlowChartRun()
		{
			bool r = MiddleLayer.ManualF.IB_UpConveyor_Work_BoardStop_Sensor.On();
			
			if (!r)
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.CASE1;

		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_04_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_UpConveyor_Work_Stop_Cylinder.On();

			bool r = MiddleLayer.ManualF.IB_UpConveyor_Work_StopUp_Sensor.On();
			if (r)
			{
			
				return NPSDK.Flow_Chart.ResultType.NEXT;

			}

			
			return NPSDK.Flow_Chart.ResultType.IDLE;


		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_05_FlowChartRun()
		{
			WorkConveyorReady = true;
			
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}


		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_06_FlowChartRun()
		{
			if (BufferConveyorReady)
			{
				
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_07_FlowChartRun()
		{
			ConveyorStart(2);

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_08_FlowChartRun()
		{
			bool r = MiddleLayer.ManualF.IB_UpConveyor_Work_BoardOut_Sensor.On();
			if (r)
			{
				
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_09_FlowChartRun()
		{
			bool r = MiddleLayer.ManualF.IB_UpConveyor_Work_BoardOut_Sensor.On();
			if (!r)
			{
				
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_10_FlowChartRun()
		{
			WorkConveyorReady = false;
			BufferConveyorReady = false;
			
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_11_FlowChartRun()
		{
			bool r = MiddleLayer.ManualF.IB_UpConveyor_Work_BoardSpeedSwitch_Sensor.On();
			if (r)
			{
			
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_12_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_UpConveyor_Work_ConveyorSpedSwitch.On();
		
			return NPSDK.Flow_Chart.ResultType.NEXT;

		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_13_FlowChartRun()
		{
			bool r = MiddleLayer.ManualF.IB_UpConveyor_Work_BoardStop_Sensor.On();
			if (r)
			{
				Thread.Sleep(1500);
				
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_14_FlowChartRun()
		{
			ConveyorStop(2);

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_15_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_UpConveyor_Work_Jacking_Cylinder.On();
			bool r = MiddleLayer.ManualF.IB_UpConveyor_Work_JackingUp_Sensor.On();
			if (r)
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_03_FlowChartRun()
		{
			
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_16_FlowChartRun()
		{
			UpConveyorProductReady = true;
			
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_17_FlowChartRun()
		{
			if (!UpConveyorProductReady)
			{
			
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_18_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_UpConveyor_Work_Jacking_Cylinder.On();
		
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_19_FlowChartRun()
		{
			bool r = MiddleLayer.ManualF.IB_UpConveyor_DownMachineReady_SMEMA.On();
			if (r)
			{
			
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_20_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_UpConveyor_Work_Stop_Cylinder.Off();

			bool r = MiddleLayer.ManualF.IB_UpConveyor_Work_StopDown_Sensor.On();
			if (r)
			{
			
				return NPSDK.Flow_Chart.ResultType.NEXT;

			}

			
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_21_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_UpConveyor_Work_Jacking_Cylinder.Off();
			bool r = MiddleLayer.ManualF.IB_UpConveyor_Work_JackingDown_Sensor.On();
			if (r)
			{
			

				return NPSDK.Flow_Chart.ResultType.NEXT;
			}

			
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_23_FlowChartRun()
		{
			bool r = MiddleLayer.ManualF.IB_UpConveyor_Work_BoardOut_Sensor.On();
			if (r)
			{
				

				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_24_FlowChartRun_1()
		{
			bool r = MiddleLayer.ManualF.IB_UpConveyor_Work_BoardOut_Sensor.On();
			if (!r)
			{
				

				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_25_FlowChartRun()
		{
			bool r = MiddleLayer.ManualF.IB_UpConveyor_DownMachineReady_SMEMA.On();
			if (!r)
			{
				
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_22_FlowChartRun()
		{
			ConveyorStart(2);

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_26_FlowChartRun()
		{
			ConveyorStop(2); ;

			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_27_FlowChartRun()
		{
		
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_28_FlowChartRun()
		{
		
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyAuto_Flow2_29_FlowChartRun()
		{
		
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

	

		private NPSDK.Flow_Chart.ResultType UPConveyInit_Flow1_2_FlowChartRun()
		{
			if (SysPara.GantryInitialOK)
			{
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyInit_Flow2_21_FlowChartRun()
		{
			if(SysPara.DownConveyorInitialOk)
			{
			
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		private void FlowChartTable_SelectedIndexChanged(object sender, EventArgs e)
		{



		}

		private NPSDK.Flow_Chart.ResultType UPConveyInit_Flow1_9_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_UpConveyor_Work_ConveyorReverse.On();
			Thread.Sleep(1500);
			MiddleLayer.ManualF.OB_UpConveyor_Work_ConveyorReverse.Off();
			return NPSDK.Flow_Chart.ResultType.NEXT;
		}

		private NPSDK.Flow_Chart.ResultType UPConveyInit_Flow2_0_FlowChartRun()
		{
			MiddleLayer.ManualF.OB_UpConveyor_Buffer_Stop_Cylinder.On();

			bool Buffer_StopUp_Sensor_Staus = MiddleLayer.ManualF.IB_UpConveyor_Buffer_StopUp_Sensor.On();
			if (Buffer_StopUp_Sensor_Staus)
			{
				
				return NPSDK.Flow_Chart.ResultType.NEXT;
			}
			
			return NPSDK.Flow_Chart.ResultType.IDLE;
		}

		
	}
}
