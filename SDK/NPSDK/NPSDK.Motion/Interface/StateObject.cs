using System.Net.Sockets;
using System.Text;

namespace NPSDK
{
	public class StateObject
	{
		public readonly TcpClient Client;

		public readonly byte[] Buffer;

		public readonly StringBuilder Data;

		public readonly int BufferSize = 1024;

		public StateObject(TcpClient client)
		{
			Client = client;
			Data = new StringBuilder();
			Buffer = new byte[BufferSize];
		}
	}
}
