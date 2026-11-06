using System.Runtime.InteropServices;

namespace APS168_W32
{
	public struct PTA3CA
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
		public byte[] Index;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
		public double[] Center;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
		public double[] Normal;

		public double Angle;
	}
}
