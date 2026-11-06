using System;

namespace NPSDK
{
	/// <summary>欧姆龙输出。通讯用同目录 OmronPLC，OmronAddress 原样读写。</summary>
	internal class OmronNP_Output : IOBaseInterface
	{
		private OmronPLC plc;
		private string address;
		private bool ready;

		public override bool Initial(string Port, string ip, string fieldName)
		{
			address = string.IsNullOrWhiteSpace(Port) ? string.Empty : Port.Trim();
			int plcPort = 44818;
			int parsed;
			if (int.TryParse(fieldName, out parsed) && parsed > 0)
			{
				plcPort = parsed;
			}
			plc = OmronPlcShare.Open(ip, plcPort);
			ready = plc != null && plc.IsConnected && address.Length > 0;
			return ready;
		}

		public override bool GetState()
		{
			return ready && plc.OmronReadBoolean(address);
		}

		public override bool On()
		{
			return ready && plc.WriteBool(address, true);
		}

		public override bool Off()
		{
			return ready && plc.WriteBool(address, false);
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
			plc = null;
		}
	}
}
