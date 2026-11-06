using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NPSDK
{
	/// <summary>基恩士 KV Host Link TCP。按 IP:Port 复用连接。</summary>
	internal static class KeyenceHostLink
	{
		private class Client
		{
			public Socket Socket;
			public readonly object Sync = new object();
			public bool Connected;
			public string Ip;
			public int Port;
		}

		public enum DeviceType
		{
			R,
			MR,
			DM
		}

		public enum DataFormat
		{
			U,
			S,
			D,
			L,
			H
		}

		private static readonly Dictionary<string, Client> clients = new Dictionary<string, Client>();

		private static string Key(string ip, int port)
		{
			return ip + ":" + port;
		}

		public static bool Connect(string ip, int port)
		{
			if (string.IsNullOrWhiteSpace(ip) || port <= 0)
			{
				return false;
			}
			string key = Key(ip.Trim(), port);
			lock (clients)
			{
				Client client;
				if (clients.TryGetValue(key, out client) && client.Connected && client.Socket != null && client.Socket.Connected)
				{
					return true;
				}
				client = new Client { Ip = ip.Trim(), Port = port };
				try
				{
					var wait = new ManualResetEvent(false);
					var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
					socket.SendTimeout = 200;
					socket.ReceiveTimeout = 200;
					socket.BeginConnect(IPAddress.Parse(client.Ip), client.Port, ar =>
					{
						try { ((Socket)ar.AsyncState).EndConnect(ar); } catch { }
						wait.Set();
					}, socket);
					if (!wait.WaitOne(800) || !socket.Connected)
					{
						try { socket.Close(); } catch { }
						return false;
					}
					client.Socket = socket;
					client.Connected = true;
					clients[key] = client;
					return true;
				}
				catch
				{
					return false;
				}
			}
		}

		public static string Read(string ip, int port, DeviceType device, int address, DataFormat format)
		{
			Client client = GetConnected(ip, port);
			if (client == null)
			{
				return "0";
			}
			lock (client.Sync)
			{
				string cmd = device == DeviceType.DM
					? "RDS " + device + address + "." + format + " 1\r\n"
					: "RDS " + device + address + " 1\r\n";
				try
				{
					byte[] buffer = new byte[1024];
					client.Socket.Send(Encoding.ASCII.GetBytes(cmd));
					int n = client.Socket.Receive(buffer);
					return Encoding.ASCII.GetString(buffer, 0, n).Trim();
				}
				catch
				{
					client.Connected = false;
					Connect(ip, port);
					return "";
				}
			}
		}

		public static bool Write(string ip, int port, DeviceType device, int address, DataFormat format, string value)
		{
			Client client = GetConnected(ip, port);
			if (client == null)
			{
				return false;
			}
			lock (client.Sync)
			{
				string cmd = device == DeviceType.DM
					? "WRS " + device + address + "." + format + " 1 " + value + "\r\n"
					: "WRS " + device + address + " 1 " + value + "\r\n";
				try
				{
					byte[] buffer = new byte[1024];
					client.Socket.Send(Encoding.ASCII.GetBytes(cmd));
					client.Socket.Receive(buffer);
					return true;
				}
				catch
				{
					client.Connected = false;
					Connect(ip, port);
					return false;
				}
			}
		}

		private static Client GetConnected(string ip, int port)
		{
			if (!Connect(ip, port))
			{
				return null;
			}
			lock (clients)
			{
				Client client;
				return clients.TryGetValue(Key(ip.Trim(), port), out client) ? client : null;
			}
		}
	}
}
