using System;
using NPSDK.Motion;

namespace NPSDK
{
	/// <summary>基恩士 PLC 输出（仅 IO，无轴）。</summary>
	internal class KeyenceNP_Output : IOBaseInterface
	{
		private string plcIp = "192.168.3.100";
		private int plcPort = 8501;
		private int address;
		private KeyenceHostLink.DeviceType deviceType = KeyenceHostLink.DeviceType.R;
		private KeyenceHostLink.DataFormat format = KeyenceHostLink.DataFormat.U;
		private bool ready;

		public override bool Initial(string Port, string ip, string fieldName)
		{
			try
			{
				if (!string.IsNullOrWhiteSpace(ip))
				{
					plcIp = ip.Trim();
				}
				ParseField(fieldName);
				address = string.IsNullOrWhiteSpace(Port) ? 0 : Convert.ToInt32(Port.Trim());
				ready = KeyenceHostLink.Connect(plcIp, plcPort);
				return ready;
			}
			catch
			{
				ready = false;
				return false;
			}
		}

		private void ParseField(string fieldName)
		{
			if (string.IsNullOrWhiteSpace(fieldName))
			{
				return;
			}
			string[] parts = fieldName.Split('|');
			if (parts.Length > 0)
			{
				int p;
				if (int.TryParse(parts[0], out p))
				{
					plcPort = p;
				}
			}
			if (parts.Length > 1)
			{
				KeyenceHostLink.DeviceType dt;
				if (Enum.TryParse(parts[1], true, out dt))
				{
					deviceType = dt;
				}
			}
			if (parts.Length > 2)
			{
				KeyenceHostLink.DataFormat fmt;
				if (Enum.TryParse(parts[2], true, out fmt))
				{
					format = fmt;
				}
			}
		}

		public override bool GetState()
		{
			if (!ready)
			{
				return false;
			}
			return KeyenceHostLink.Read(plcIp, plcPort, deviceType, address, format) == "1";
		}

		public override bool On()
		{
			return ready && KeyenceHostLink.Write(plcIp, plcPort, deviceType, address, format, "1");
		}

		public override bool Off()
		{
			return ready && KeyenceHostLink.Write(plcIp, plcPort, deviceType, address, format, "0");
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
