using System.Runtime.InteropServices;

namespace APS168_W32
{
	public struct PTLINE
	{
		public int Dim;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
		public double[] Pos;
	}
}
