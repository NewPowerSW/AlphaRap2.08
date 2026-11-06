using System.Runtime.InteropServices;

namespace APS168_W64
{
	public struct PTA3CE
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
		public byte[] Index;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
		public double[] Center;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
		public double[] End;

		public short Dir;
	}
}
