using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

public class TcpipClient
{
	private class StateObject
	{
		public Socket workSocket = null;

		public const int BufferSize = 256;

		public byte[] buffer = new byte[256];

		public StringBuilder sb = new StringBuilder();
	}

	public string IP = "127.0.0.1";

	public int PORT = 23;

	private Socket _Client;

	private bool _IsConnect = false;

	private bool _IsReceived = false;

	private string _ReceiveString = string.Empty;

	public void Connect()
	{
		try
		{
			Disconnect();
			IPAddress iPAddress = IPAddress.Parse(IP);
			IPEndPoint remoteEP = new IPEndPoint(iPAddress, PORT);
			_Client = new Socket(iPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			_Client.BeginConnect(remoteEP, ConnectCallback, _Client);
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.ToString());
		}
	}

	public void Disconnect()
	{
		try
		{
			_IsConnect = false;
			if (_Client != null)
			{
				try
				{
					_Client.Shutdown(SocketShutdown.Both);
					_Client.Close();
					_Client = null;
					return;
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
					return;
				}
			}
		}
		catch (Exception ex2)
		{
			MessageBox.Show(ex2.ToString());
		}
	}

	private void ConnectCallback(IAsyncResult ar)
	{
		try
		{
			Socket socket = (Socket)ar.AsyncState;
			socket.EndConnect(ar);
			_IsConnect = true;
		}
		catch (Exception ex)
		{
			_IsConnect = false;
			Console.WriteLine(ex.ToString());
		}
	}

	public bool IsConnected()
	{
		return _IsConnect;
	}

	public void Receive()
	{
		try
		{
			_IsReceived = false;
			if (_IsConnect)
			{
				StateObject stateObject = new StateObject();
				stateObject.workSocket = _Client;
				IAsyncResult asyncResult = _Client.BeginReceive(stateObject.buffer, 0, 256, SocketFlags.None, ReceiveCallback, stateObject);
				Thread.Sleep(10);
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
	}

	private void ReceiveCallback(IAsyncResult ar)
	{
		try
		{
			StateObject stateObject = (StateObject)ar.AsyncState;
			Socket workSocket = stateObject.workSocket;
			int num = workSocket.EndReceive(ar);
			if (num > 0)
			{
				stateObject.sb.Append(Encoding.ASCII.GetString(stateObject.buffer, 0, num));
				if (stateObject.sb.Length > 1)
				{
					_ReceiveString = stateObject.sb.ToString();
				}
			}
			_IsReceived = true;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
	}

	public bool IsReceived()
	{
		return _IsReceived;
	}

	public string GetReceiveString()
	{
		return _ReceiveString;
	}

	public void Send(string data)
	{
		if (_IsConnect)
		{
			try
			{
				byte[] bytes = Encoding.ASCII.GetBytes(data);
				_Client.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, SendCallback, _Client);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
	}

	private void SendCallback(IAsyncResult ar)
	{
		try
		{
			Socket socket = (Socket)ar.AsyncState;
			int num = socket.EndSend(ar);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
	}
}
