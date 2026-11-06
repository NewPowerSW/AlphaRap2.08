using System.Runtime.InteropServices;

namespace APS168_W64
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct VAO_DATA
	{
		public int outputType;

		public int inputType;

		public int config;

		public int inputSrc;

		public int minVel;

		public int velInterval;

		public int totalPoints;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		public int[] mappingDataArr;
	}
}
