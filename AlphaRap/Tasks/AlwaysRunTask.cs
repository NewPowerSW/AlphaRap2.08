using System;
using System.Windows.Forms;
using System.Threading;
namespace AlphaRap
{
	class AlwaysRunTask
	{
		private JTimer BlinkTM = new JTimer();
		private bool BlinkIsOn = false;
		private const int BlinkTime = 500;
		public bool BuzzerNeedWork = true;

		Thread FlowControlThread = null;

		public AlwaysRunTask()
		{
		}

		public void StartAlwaysRun()
		{
			FlowControlThread = new Thread(AlwaysRunWork);
			FlowControlThread.Start();
		}

		public void AlwaysRunWork()
		{
			while (!MiddleLayer.gEXIT)
			{
				AlwaysRun();
			}
		}
		public void AlwaysRun()
		{
			if (SysPara.Simulation)
				return;

			#region BlinkTimer
			if (BlinkTM.IsOn(BlinkTime))
			{
				BlinkIsOn = !BlinkIsOn;
				BlinkTM.Restart();
			}
			#endregion

		}

		public static void RefreshDifferentThreadUI(Control control, Action action)
		{
			if (control.InvokeRequired)
			{
				Action refreshUI = new Action(action);
				control.Invoke(refreshUI);
			}
			else
			{
				action.Invoke();
			}
		}

		public void BuzzOff()
		{
			if (BuzzerNeedWork)
			{
				BuzzerNeedWork = false;
				MiddleLayer.ManualF.OB_Buzz.On();
			}
			else
			{
				BuzzerNeedWork = true;
			}
		}
	}
}
