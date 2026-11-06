using System;
using System.Collections.Generic;

namespace NPSDK
{
	/// <summary>欧姆龙输入。通讯用同目录 OmronPLC，OmronAddress 原样读取。</summary>
	internal class OmronNP_Input : IOBaseInterface
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
			return GetState();
		}

		public override bool Off()
		{
			return !GetState();
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

	/// <summary>同一 IP 共用一个 OmronPLC 连接。</summary>
	internal static class OmronPlcShare
	{
		private static readonly Dictionary<string, OmronPLC> clients = new Dictionary<string, OmronPLC>();

		public static OmronPLC Open(string ip, int port)
		{
			if (string.IsNullOrWhiteSpace(ip) || port <= 0)
			{
				return null;
			}
			string key = ip.Trim() + ":" + port;
			lock (clients)
			{
				OmronPLC plc;
				if (clients.TryGetValue(key, out plc) && plc.IsConnected)
				{
					return plc;
				}
				plc = new OmronPLC(key);
				plc.IP = ip.Trim();
				plc.Port = port;
				plc.Open();
				clients[key] = plc;
				return plc.IsConnected ? plc : null;
			}
		}
	}
}
