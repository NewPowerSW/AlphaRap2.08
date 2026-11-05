using AlphaRap;
using AlphaRap.FlowProgramming;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

namespace AtlasLibrary
{
	public class MTF6000_1 : AbstractDevice
	{
		private Socket clientSocket;
		public bool bConnectStatus;
		private string sReadData = null;
		private System.Timers.Timer time;

		private readonly object read = new object();
		private readonly object reading = new object();
		private const string StartCommunicateSend = "002000010060        \0";
		private const string KeepAliveSend = "002099990010        \0";
		private const string UnSubscribeSend = "002900090010        120100100\0";
		private const string SubscribeSend = "006000080010        1201001310000000000000000000000000000001\0";

		/// <summary>创建可由旧代码直接使用的拧紧控制器。</summary>
		public MTF6000_1() : this(nameof(MTF6000_1)) { }

		/// <summary>创建由设备管理器命名的拧紧控制器。</summary>
		public MTF6000_1(string deviceName) : base(deviceName) { }

		public string IP { get; set; } = string.Empty;
		public int Port { get; set; }

		public override bool IsConnected
		{
			get { lock (read) return bConnectStatus && clientSocket != null && clientSocket.Connected; }
		}

		/// <summary>使用设备面板配置的地址打开连接。</summary>
		public override void Open() { Atlas_Connect(IP, Port); }

		/// <summary>关闭连接并停止心跳。</summary>
		public override void Close() { Disconnect(); }

		public struct _Result
		{
			public string TotalResultString;
			public string PeekTorque;
			public string TotalAngle;
			public bool TotalStatus;
		}
		public _Result Atlas_ResultStruct = new _Result();
		/// <summary>建立 TCP 会话并启动本机型的心跳定时器。</summary>
		[FlowToolIgnore]
		public void Atlas_Connect(string IP, int Port)
		{
			lock (read)
			{
				if (IsConnected) return;
				Disconnect();

				IPAddress address;
				if (!IPAddress.TryParse(IP, out address) || address.AddressFamily != AddressFamily.InterNetwork ||
					Port <= 0 || Port > IPEndPoint.MaxPort) return;

				try
				{
					clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
					{
						SendTimeout = 200,
						ReceiveTimeout = 200
					};
					ConnectSocket(address, Port);
					Thread.Sleep(1000);
					bConnectStatus = true;

					SendCommand(StartCommunicateSend);
					if (!IsConnected) { Disconnect(); return; }

					time = new System.Timers.Timer(5000);
					time.Elapsed += theout;
					time.Start();
				}
				catch (SocketException error)
				{
					Disconnect();
					Debug.WriteLine(error.Message);
				}
			}
		}

		/// <summary>限制 TCP 连接等待时间，并读取当前连接尝试的实际结果。</summary>
		private void ConnectSocket(IPAddress address, int port)
		{
			IAsyncResult attempt = clientSocket.BeginConnect(address, port, null, null);
			using (WaitHandle completed = attempt.AsyncWaitHandle)
			{
				if (!completed.WaitOne(1000)) throw new SocketException((int)SocketError.TimedOut);
				clientSocket.EndConnect(attempt);
			}
		}

		/// <summary>释放当前套接字和心跳，允许断开后再次连接。</summary>
		[FlowToolIgnore]
		public void Disconnect()
		{
			lock (read)
			{
				bConnectStatus = false;
				System.Timers.Timer timer = time;
				time = null;
				timer?.Dispose();
				Socket socket = clientSocket;
				clientSocket = null;
				socket?.Dispose();
			}
		}

		/// <summary>在设备命令之间发送当前会话的心跳。</summary>
		[FlowToolIgnore]
		public void theout(object source, System.Timers.ElapsedEventArgs e)
		{
			lock (reading)
				lock (read)
					if (ReferenceEquals(source, time)) SendCommand(KeepAliveSend, 100);
		}

		/// <summary>发送一条拧紧协议命令并读取响应。</summary>
		public string SendCommand(string commandStr, int timeout = 0)
		{
			lock (read)
			{
				if (!IsConnected) return string.Empty;
				sReadData = string.Empty;
				try
				{
					byte[] command = Encoding.ASCII.GetBytes(commandStr);
					clientSocket.Send(command);
					Thread.Sleep(timeout);
					ReceiveData();
					return sReadData;
				}
				catch (Exception)
				{
					bConnectStatus = false;
					return "ERROR";
				}
			}
		}

		/// <summary>读取当前套接字的响应内容。</summary>
		[FlowToolIgnore]
		public void ReceiveData()
		{
			lock (read)
			{
				if (clientSocket == null) return;
				try
				{
					byte[] buffer = new byte[1024];
					int count = clientSocket.Receive(buffer);
					if (count == 0) bConnectStatus = false;
					sReadData += Encoding.ASCII.GetString(buffer, 0, count).Trim();
				}
				catch (SocketException)
				{
				}
			}
		}

		public int MID0006_Tightening_Program_Upload()
		{
			string SpeedString = "0";
			try
			{
				string MID0006 = "003600060010        2501001070001301 \0";

				string returnstring;
				returnstring = SendCommand(MID0006, 500);
				returnstring = returnstring + SendCommand(MID0006, 500);
				SpeedString = returnstring.Substring(returnstring.LastIndexOf("30100001020000002230104001020000002030101004021010002") + 53, 4);
			}
			catch (SocketException)
			{
			}
			return Convert.ToInt32(SpeedString);
		}

		public void MID0006_Tightening_Program_Download(int Speed)
		{
			try
			{
				string MID0006 = "003600060010        2501001070001301 \0";

				string returnstring = "";
				string returnstring2 = "";
				int index = 0;
				bool flag = true;

				returnstring = SendCommand(MID0006, 500);
				returnstring = returnstring + SendCommand(MID0006, 500);

				index = returnstring.IndexOf("30108001060000001030101004021010001", index + 20);
				flag = index > 10;
				if (flag)
				{
					returnstring2 = returnstring.Substring(index + 35, 4);
					returnstring = returnstring.Replace("30108001060000001030101004021010001" + returnstring2, "30108001060000001030101004021010001" + String.Format("{0:D4}", Speed));
				}

				index = returnstring.IndexOf("30104001020000002030101004021010002", index + 20);
				flag = index > 10;
				if (flag)
				{
					returnstring2 = returnstring.Substring(index + 35, 4);
					returnstring = returnstring.Replace("30104001020000002030101004021010002" + returnstring2, "30104001020000002030101004021010002" + String.Format("{0:D4}", Speed));
				}

				returnstring = returnstring.Replace("18702501", "18702500");
				returnstring = returnstring.Replace("\0", " \0");
				string returnstring4 = SendCommand(returnstring, 500);
			}
			catch (SocketException)
			{
			}
		}

		public _Result Atlas_ReadTighteningResult(int timeout = 0)
		{
			lock (reading)
			{
				try
				{
					Stopwatch timer = new Stopwatch();
					timer.Stop();
					timer.Reset();
					timer.Start();
					Atlas_ResultStruct.PeekTorque = "0";
					Atlas_ResultStruct.TotalAngle = "0";

					String returnstring = "";
					String Torquestring;
					Decimal dData;

					while (!returnstring.Contains("30202") || !returnstring.Contains("30230") || !returnstring.Contains("30231")
						|| (returnstring.LastIndexOf("30202") + 17 + 12) >= returnstring.Length
						|| (returnstring.LastIndexOf("30230") + 17 + 12) >= returnstring.Length
						|| (returnstring.LastIndexOf("30231") + 17 + 12) >= returnstring.Length)
					{
						SendCommand(UnSubscribeSend, 20);
						SendCommand(UnSubscribeSend, 20);
						SendCommand(UnSubscribeSend, 20);
						returnstring = SendCommand(SubscribeSend, timeout);

						if (timer.ElapsedMilliseconds > 5000)
						{
							Atlas_ResultStruct.PeekTorque = "0";
							Atlas_ResultStruct.TotalAngle = "0";
							Atlas_ResultStruct.TotalStatus = false;
							return Atlas_ResultStruct;
						}
					}

					Atlas_ResultStruct.TotalResultString = returnstring;

					int q = 0;

					Atlas_ResultStruct.TotalStatus = 1 == 1 ? true : false;
					////////////////////////////////////////////////////////////////////////////Torque////////////////////////////////////////////
					q = returnstring.LastIndexOf("30230");
					if (q != -1 && (q + 17) < returnstring.Length)
					{
						Torquestring = returnstring.Substring(q + 17, 12);
						dData = Convert.ToDecimal(Decimal.Parse(Torquestring.ToString(), System.Globalization.NumberStyles.Float));
						Torquestring = (dData / 1000).ToString();
					}
					else
					{
						Torquestring = "";
						dData = 0;
					}
					Atlas_ResultStruct.PeekTorque = Torquestring.ToString();
					double _dataT = Convert.ToDouble(Torquestring);
					////////////////////////////////////////////////////////////////////////////Angle////////////////////////////////////////////
					q = returnstring.LastIndexOf("30231");
					if (q != -1 && (q + 17) < returnstring.Length)
					{
						Torquestring = returnstring.Substring(q + 17, 12);
						dData = Convert.ToDecimal(Decimal.Parse(Torquestring.ToString(), System.Globalization.NumberStyles.Float));
						Torquestring = (dData).ToString();
					}
					else
					{
						Torquestring = "";
						dData = 0;
					}
					Atlas_ResultStruct.TotalAngle = Torquestring.ToString();

					double _dataa = Convert.ToDouble(Torquestring);

					return Atlas_ResultStruct;
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.ToString());
					Atlas_ResultStruct.PeekTorque = "99999";
					Atlas_ResultStruct.TotalAngle = "99999";
					return Atlas_ResultStruct;
				}
			}
		}
	}
}
