using System.Runtime.InteropServices;

namespace APS168_W32
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct SAMP_PARAM
	{
		public int rate;

		public int edge;

		public int level;

		public int trigCh;

		public int[,] sourceByCh;

		public SAMP_PARAM(int x, int y)
		{
			this = default(SAMP_PARAM);
			sourceByCh = new int[x, y];
		}
	}
}
