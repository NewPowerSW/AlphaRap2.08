using System.Runtime.InteropServices;

namespace APS168_W32
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct POINT_DATA3
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public int[] i32_pos;

		public int i32_maxSpeed;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public int[] i32_endPos;

		public int i32_dir;

		public int i32_opt;
	}
}
