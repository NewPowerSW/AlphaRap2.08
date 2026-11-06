using System.Runtime.InteropServices;

namespace APS168_W32
{
	public struct OD_DESC_ENTRY
	{
		public uint DataTypeNum;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public byte[] DataTypeName;

		public uint BitLen;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public byte[] Description;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public byte[] Access;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public byte[] PdoMapInfo;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public byte[] UnitType;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public byte[] DefaultValue;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public byte[] MinValue;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public byte[] MaxValue;
	}
}
