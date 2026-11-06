using System;
using csLTDMC;
using NPSDK.Motion;

namespace NPSDK
{
	/// <summary>
	/// 雷赛 DO，实现统一 IOBaseInterface。
	/// Port 格式与凌华一致：前 3 位卡号，其后为点位号。
	/// </summary>
	internal class LeadshineNP_Output : IOBaseInterface
	{
		private ushort cardNum = 1;
		private ushort bitPort;
		private ushort bitIndex;
		private bool ready;

		public override bool Initial(string Port, string ip, string fieldName)
		{
			try
			{
				if (!NPHub.IO_ConnectStatus && !NPHub.IsInitialized)
				{
					if (!NPHub.Initial(NPVendor.Leadshine))
					{
						return false;
					}
				}
				NPHub.SelectVendor(NPVendor.Leadshine);
				if (string.IsNullOrEmpty(Port) || Port.Length < 5)
				{
					return false;
				}
				cardNum = Convert.ToUInt16(Port.Substring(0, 3));
				bitPort = Convert.ToUInt16(Port.Substring(3));
				if (cardNum == 0)
				{
					cardNum = 1;
				}
				bitIndex = (ushort)((cardNum - 1) * 32 + bitPort + 8);
				ready = NPHub.IO_ConnectStatus || NPHub.IsInitialized;
				return ready;
			}
			catch
			{
				ready = false;
				return false;
			}
		}

		public override bool GetState()
		{
			if (!ready)
			{
				return false;
			}
			return LTDMC.dmc_read_outbit(0, bitIndex) != 0;
		}

		public override bool On()
		{
			if (!ready)
			{
				return false;
			}
			LTDMC.dmc_write_outbit(0, bitIndex, 1);
			return true;
		}

		public override bool Off()
		{
			if (!ready)
			{
				return false;
			}
			LTDMC.dmc_write_outbit(0, bitIndex, 0);
			return true;
		}

		public override bool IsOn()
		{
			return GetState();
		}

		public override bool IsOff()
		{
			return !GetState();
		}

		public override void Dispose()
		{
			ready = false;
		}
	}
}
