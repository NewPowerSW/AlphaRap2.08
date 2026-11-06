using System;
#if NP_VENDOR_ADLINK
using APS168_W64;
#endif

namespace NPSDK
{
	internal class AdlinkNP_Input : IOBaseInterface
	{
		private short CardID = 0;

		private short s_BitNo;

		private short sCardNumber;

		public override bool Initial(string Port, string ip, string fieldName)
		{
#if !NP_VENDOR_ADLINK
			return false;
#else
			try
			{
				if (!NPSDKPara.bConnectNETIO)
				{
					int BoardID_InBits = 0;
					APS168.APS_stop_field_bus(CardID, 0);
					if (!NPSDKPara.bAPSInitial && APS168.APS_initial(ref BoardID_InBits, 0) == 0)
					{
						NPSDKPara.bAPSInitial_Ok = true;
						NPSDKPara.bAPSInitial = true;
					}
					if (NPSDKPara.bAPSInitial_Ok && APS168.APS_start_field_bus(CardID, 0, 0) == 0)
					{
						NPSDKPara.bConnectNETIO_Ok = true;
						NPSDKPara.bConnectNETMotion_Ok = true;
					}
					NPSDKPara.bConnectNETIO = true;
				}
				if (NPSDKPara.bConnectNETIO_Ok && Port.Length >= 5)
				{
					sCardNumber = Convert.ToInt16(Port.Substring(0, 3));
					s_BitNo = Convert.ToInt16(Port.Substring(3));
					return true;
				}
			}
			catch (Exception)
			{
			}
			return false;
#endif
		}

		public override bool GetState()
		{
#if !NP_VENDOR_ADLINK
			return false;
#else
			int DI_Value = 0;
			if (APS168.APS_get_field_bus_d_channel_input(CardID, 0, sCardNumber, s_BitNo, ref DI_Value) == 0)
			{
				return DI_Value == 1;
			}
			return false;
#endif
		}

		public override bool On()
		{
			return GetState();
		}

		public override bool Off()
		{
			return !GetState();
		}

		public override void Dispose()
		{
#if NP_VENDOR_ADLINK
			APS168.APS_stop_field_bus(CardID, 0);
			APS168.APS_close();
#endif
		}
	}
}
