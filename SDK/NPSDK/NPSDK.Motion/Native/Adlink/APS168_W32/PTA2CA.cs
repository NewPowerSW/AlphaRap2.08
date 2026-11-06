using System.Runtime.InteropServices;

namespace APS168_W32
{
	public struct PTA2CA
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public byte[] Index;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public double[] Center;

		public double Angle;
	}
}
