using System.Runtime.InteropServices;

namespace APS168_W64
{
	public struct EC_Sub_MODULE_INFO
	{
		public int VendorID;

		public int ProductCode;

		public int RevisionNo;

		public int TotalSubModuleNum;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		public int[] SubModuleID;
	}
}
