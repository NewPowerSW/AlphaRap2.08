namespace NPSDK
{
	/// <summary>
	/// 程序启动入口：NSDK.SetSimulation / NSDK.InitialControls。
	/// </summary>
	public static class NSDK
	{
		public static bool isSimulation
		{
			get { return SDKKernal.isSimulation; }
			set { SDKKernal.isSimulation = value; }
		}

		public static void SetSimulation(bool isSimu)
		{
			SDKKernal.SetSimulation(isSimu);
		}

		/// <summary>先读取各轴保存参数，再初始化全部控件。</summary>
		public static void InitialControls()
		{
			LoadMotorParameters();
			foreach (ControlBaseInterface control in SDKPara.ControlsSnapshot())
			{
				if (!control.InitializeWithSdk) continue;
				control.Initial();
			}
		}

		/// <summary>从参数文件载入已登记轴的设置并返回成功数量。</summary>
		public static int LoadMotorParameters()
		{
			SDKPara.IOCardList.Clear();
			int count = 0;
			foreach (ControlBaseInterface control in SDKPara.ControlsSnapshot())
			{
				if (!control.InitializeWithSdk) continue;
				Motion.NP_Motor motor = control as Motion.NP_Motor;
				if (motor != null && Motion.MotorParaForm.TryLoadFile(motor))
				{
					count++;
				}
			}
			return count;
		}

		public static void RefreshControls()
		{
			SDKKernal.RefreshControls();
		}

		public static void RefreshIO()
		{
			RefreshControls();
		}

		public static void DisposeControls()
		{
			SDKKernal.DisposeComponent();
		}
	}
}
