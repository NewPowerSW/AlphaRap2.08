using System.Runtime.InteropServices;

namespace APS168_W64
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct STR_SAMP_DATA_8CH
	{
		public int tick;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		public int[] data;
	}
}
