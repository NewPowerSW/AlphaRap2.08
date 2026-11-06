using System.Runtime.InteropServices;

namespace APS168_W64
{
	public struct PDO_OFFSET
	{
		public ushort DataType;

		public uint ByteSize;

		public uint ByteOffset;

		public uint Index;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
		public byte[] NameArr;
	}
}
