using System.Runtime.InteropServices;

namespace APS168_W64
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct POINT_DATA2
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public int[] i32_pos;

		public int i32_initSpeed;

		public int i32_maxSpeed;

		public int i32_angle;

		public uint u32_dwell;

		public int i32_opt;
	}
}
