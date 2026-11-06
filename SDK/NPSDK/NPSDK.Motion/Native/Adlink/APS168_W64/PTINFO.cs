using System.Runtime.InteropServices;

namespace APS168_W64
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct PTINFO
	{
		public int Dimension;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
		public int[] AxisArr;
	}
}
