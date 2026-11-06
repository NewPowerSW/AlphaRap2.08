using System.Runtime.InteropServices;

namespace APS168_W32
{
	public struct PNT_DATA_2D_EXT
	{
		public uint u32_opt;

		public double f64_x;

		public double f64_y;

		public double f64_theta;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public double[] f64_acc;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public double[] f64_dec;

		public int crossover;

		public int Iboundary;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public double[] f64_vi;

		public uint vi_cmpr;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public double[] f64_vm;

		public uint vm_cmpr;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public double[] f64_ve;

		public uint ve_cmpr;

		public int Eboundary;

		public double f64_dist;

		public double f64_angle;

		public double f64_radius;

		public int i32_arcstate;

		public uint spt;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public double[] t;

		public int HorizontalFlag;

		public int VerticalFlag;
	}
}
