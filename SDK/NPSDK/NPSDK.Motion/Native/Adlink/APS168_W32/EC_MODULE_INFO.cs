using System.Runtime.InteropServices;

namespace APS168_W32
{
	public struct EC_MODULE_INFO
	{
		public int VendorID;

		public int ProductCode;

		public int RevisionNo;

		public int TotalAxisNum;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public int[] Axis_ID;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public int[] Axis_ID_manual;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		public int[] All_ModuleType;

		public int DI_ModuleNum;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		public int[] DI_ModuleType;

		public int DO_ModuleNum;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		public int[] DO_ModuleType;

		public int AI_ModuleNum;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		public int[] AI_ModuleType;

		public int AO_ModuleNum;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		public int[] AO_ModuleType;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		public string Name;
	}
}
