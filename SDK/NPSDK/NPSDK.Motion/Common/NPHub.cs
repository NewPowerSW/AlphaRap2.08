namespace NPSDK.Motion
{
	/// <summary>当前厂商与板卡是否已打开。控件本身不在这里创建。</summary>
	public static class NPHub
	{
		public static NPVendor ActiveVendor { get; private set; } = NPVendor.Leadshine;

		public static bool IO_ConnectStatus { get; set; }

		public static bool Motion_ConnectStatus { get; set; }

		public static bool IsInitialized { get; private set; }

		public static void SelectVendor(NPVendor vendor)
		{
			if (ActiveVendor == vendor)
			{
				return;
			}
			ActiveVendor = vendor;
			IO_ConnectStatus = false;
			Motion_ConnectStatus = false;
			IsInitialized = false;
		}

        /// <summary>连接厂商板卡；雷赛返回 1–8 张卡才算成功，失败时清除全部就绪标记。</summary>
		public static bool Initial(NPVendor vendor, bool enableIO = true, bool enableMotion = true)
		{
			SelectVendor(vendor);
            IsInitialized = false;
            IO_ConnectStatus = false;
            Motion_ConnectStatus = false;
			if (vendor == NPVendor.Leadshine)
			{
#if NP_VENDOR_LEADSHINE
				try
				{
                    short boardCount = csLTDMC.LTDMC.dmc_board_init();
                    if (boardCount < 1 || boardCount > 8)
					{
						return false;
					}
				}
				catch
				{
					return false;
				}
#else
				return false;
#endif
			}
			IO_ConnectStatus = enableIO;
			Motion_ConnectStatus = enableMotion;
			IsInitialized = true;
			return true;
		}

		public static void Close()
		{
#if NP_VENDOR_LEADSHINE
			if (ActiveVendor == NPVendor.Leadshine)
			{
				try { csLTDMC.LTDMC.dmc_board_close(); } catch { }
			}
#endif
			IO_ConnectStatus = false;
			Motion_ConnectStatus = false;
			IsInitialized = false;
		}
	}
}
