using System.Runtime.InteropServices;

namespace APS168_W64
{
	public struct EC_Sub_MODULE_OD_INFO
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
		public byte[] DataName;

		public int BitLength;

		public int DataType;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public byte[] DataTypeName;
	}
}
