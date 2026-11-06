namespace NPSDK.Motion
{
	/// <summary>
	/// 与 NSDK / Jabil JSDK 相同入口，转发到 NSDK。
	/// </summary>
	public static class MotionSDK
	{
		public static void InitialControls()
		{
			NSDK.InitialControls();
		}

		public static int LoadMotorParameters()
		{
			return NSDK.LoadMotorParameters();
		}

		public static void RefreshControls()
		{
			NSDK.RefreshControls();
		}

		public static void DisposeControls()
		{
			NSDK.DisposeControls();
		}
	}
}
