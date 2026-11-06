using System.Collections.Generic;

namespace NPSDK
{
	public class SDKPara
	{
		public static List<int> IOCardList = new List<int>();

		public static bool bAPSInitial = false;

		public static bool bAPSInitial_Ok = false;

		public static bool bConnectNETMotion = false;

		public static bool bConnectNETMotion_Ok = false;

		public static bool bConnectNETIO = false;

		public static bool bConnectNETIO_Ok = false;

		public static Dictionary<string, bool[]> ModbusDiscreteValues = new Dictionary<string, bool[]>();

		public static Dictionary<string, Dictionary<string, byte[]>> OmronInputValues = new Dictionary<string, Dictionary<string, byte[]>>();

		public static bool bConnectEcnavi = false;

		public static bool bConnectEcnavi_OK = false;

		public static bool bDisconnectEcnaviOk = false;

		public static ushort s_CardNo = 0;

		public static bool bConnectDMC = false;

		public static bool bConnectDMC_OK = false;

		public static bool bConnectDMCMotion_Ok = false;

		public static bool bConnectFindMotion_Ok = false;

		public static bool bConnectFindInPut_Ok = false;

		public static bool bConnectFindOutPut_Ok = false;

		public static short g_Ret_DMC = 0;

		public static short Ret_DMC = 0;

		public static ushort uInitialDone_DMC = 0;

		public static short g_nDMCExistCards = 0;

		public static ushort g_uDMCCardNo = 0;

		public static bool g_bInitialFlag_DMC = false;

		public static ushort[] g_uDMCCardNoList = new ushort[32];

		public static ushort[] g_uDMCNodeID = new ushort[10];

		public static ushort[] g_uDMCSlotID = new ushort[10];

		public static bool bConnectDMCIO_Ok = false;

		public static ushort g_DMCCardNo_IO = 0;

		public static ushort g_DMCNodeID_IO = 0;

		public static ushort g_DMCSlotID_IO = 0;

		public static ushort[] CmbNodeID_DMC = new ushort[9];

		public static int CheckIO_DMC;

		public static List<ControlBaseInterface> ControlList = new List<ControlBaseInterface>();

        /// <summary>登记可动态增删的硬件控件。</summary>
        public static void RegisterControl(ControlBaseInterface control) { lock (ControlList) ControlList.Add(control); }

        /// <summary>移除已释放的控件，后续刷新不再访问失效实例。</summary>
        public static void UnregisterControl(ControlBaseInterface control) { lock (ControlList) ControlList.Remove(control); }

        /// <summary>给后台扫描提供稳定快照，避免界面增删控件时枚举集合失效。</summary>
        public static ControlBaseInterface[] ControlsSnapshot()
        {
            lock (ControlList) return ControlList.FindAll(control => control != null && !control.IsDisposed).ToArray();
        }

		

		public static List<MotorSPDPara> MotorSPDList = new List<MotorSPDPara>();

		public static List<MotorMaxSPDPara> MotorMaxSPDList = new List<MotorMaxSPDPara>();

		public static bool bShowFlowchartMenu = false;
	}
}
