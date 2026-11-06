using System.Runtime.InteropServices;

namespace APS168_W64
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct PTA2CE
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public byte[] Index;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public double[] Center;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public double[] End;

		public short Dir;
	}
}
