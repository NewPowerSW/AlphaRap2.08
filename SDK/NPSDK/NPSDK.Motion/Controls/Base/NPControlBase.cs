using NPSDK;

namespace NPSDK.Motion
{
	/// <summary>
	/// 按 Vendor new 出接口实现。Keyence、Omron 仅 IO，无 Motor。
	/// </summary>
	public static class NPControlBase
	{
		public static NPVendor GetVendor()
		{
			return NPHub.ActiveVendor;
		}

		public static void SetVendor(NPVendor vendor)
		{
			NPHub.SelectVendor(vendor);
		}

		public static bool SupportsMotor(NPVendor vendor)
		{
			return vendor == NPVendor.Adlink || vendor == NPVendor.Leadshine;
		}

		public static IOBaseInterface CreateInput(NPVendor vendor)
		{
			switch (vendor)
			{
#if NP_VENDOR_ADLINK
				case NPVendor.Adlink:
					return new AdlinkNP_Input();
#endif
#if NP_VENDOR_LEADSHINE
				case NPVendor.Leadshine:
					return new LeadshineNP_Input();
#endif
				case NPVendor.Keyence:
					return new KeyenceNP_Input();
				case NPVendor.Omron:
					return new OmronNP_Input();
				default:
					return null;
			}
		}

		public static IOBaseInterface CreateOutput(NPVendor vendor)
		{
			switch (vendor)
			{
#if NP_VENDOR_ADLINK
				case NPVendor.Adlink:
					return new AdlinkNP_Output();
#endif
#if NP_VENDOR_LEADSHINE
				case NPVendor.Leadshine:
					return new LeadshineNP_Output();
#endif
				case NPVendor.Keyence:
					return new KeyenceNP_Output();
				case NPVendor.Omron:
					return new OmronNP_Output();
				default:
					return null;
			}
		}

		public static MotorBaseInterface CreateAxis(NPVendor vendor)
		{
			if (!SupportsMotor(vendor))
			{
				return null;
			}
			switch (vendor)
			{
#if NP_VENDOR_ADLINK
				case NPVendor.Adlink:
					return new AdlinkNP_Motor();
#endif
#if NP_VENDOR_LEADSHINE
				case NPVendor.Leadshine:
					return new LeadshineNP_Motor();
#endif
				default:
					return null;
			}
		}
	}
}
