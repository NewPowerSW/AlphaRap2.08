
using System;
using System.Collections.Generic;

using System.Windows.Forms;

namespace AlphaRap
{
    public class SysPara
    {
        //Run Parameter
        public static bool IsInSafeArea_Head;
        public static bool ConveyorCanHome;

        //ByPass
        public static bool bByPass = false;

        //System Parameter
        public static string MdbPath = "SystemData.mdb";
        public static string ProgramDir = System.IO.Directory.GetCurrentDirectory();
        public static bool Simulation;
        public static string ProjectName;
        public static string RecipeName;
        public static string Product_RunMode;

        //Product Message
        public static string ProductName;
        public static double iProductTotal;
        public static double iProductOK;
        public static double iProductNG;

        public static double[] iProductHourlyInput = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public static double[] iProductHourlyOutput = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public static double[] iProductHourlyReject = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public static double[] iProductHourlyYield = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        // public 
        public static int iCleanNumber;
        //saveItems
        public static int items = 1;
        public static int items2 = 1;

        //System Path
        public static string LogFilePath;
        public static string VisionFileDirectory;
        public static string AlarmTableDirectory;
        public static string SettingDataDirectory;
        public static string RecipeDataDirectory;
        public static string MESDirectory;
        public static string LanguageDataDirectory;
        public static string IOPortDirectory;
        public static string FilePath;

        public static string RunMessagePath;
        public static string AlarmMessagePath;
        public static string ProductMessagePath;
        public static string UserMessagePath;
        public static string RFIDPath;
        public static string RFIDFinalPath;
        //RFID
        //RFID
        public static string strRFIDReadPallet;      //读到的Pallet NO.
        public static string strRFIDReadFlwCnt;    //读到的上站flow count
        public static string strRFIDReadLastFlg;    //读到的上站结果标志
        public static string strRFIDWritePOneStatus;  // 写的本站生产结果
        public static string strRFIDWritePTwoStatus;  // 增加
        public static bool RFIDDisable;                    //停RFID
        public static bool RFIDReadOK = false;
        public static bool RFIDWriteOK = false;
        public static RFIDCsvInfo RFIDCsvInfoH_1;    //读写
        public static RFIDCsvInfo RFIDCsvInfoH_2;    //读写
        public static RFIDProdResult RFIDPOneResultH_1;   //Head_1的生产结果
        public static RFIDProdResult RFIDPTwoResultH_1;   //Head_1的生产结果
        public static RFIDProdResult RFIDPOneResultH_2;       //Head_2的生产结果
        public static RFIDProdResult RFIDPTwoResultH_2;       //Head_2的生产结果
        public static bool RFIDConnected = false;
        public static RFIDCsvInfoFinal RFIDCsvInfoFinalH_1;   //末站数据处理
        public static RFIDCsvInfoFinal RFIDCsvInfoFinalH_2;   //末站数据处理
        public static bool BypassFlowCtl = false;
        public static bool BypassRFID = false;
        public static string RFIDLastPalletUid;
        public static RFIDProdResult RFIDProdResultH_2;
        public static RFIDProdResult RFIDProdResultH_1;

        //Login Info = "D:\\Log\\UserLoginData\\"
        public static PermissionType UserPermission = PermissionType.Operator;
        public static string UserName = "None";
        public static string UserLoginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        public static bool bLogin = false;
        public static bool UseFingerprint = true;
        //RunMessage
        public static string RunMessageTime;

        //Scanner
        public static bool ScannerResult = false;
        public static string strBarcodeResult;

        //Langurage
        public static LanguageType LanguageShow = LanguageType.English;
        public static string LanguageName;
        //PlatSetting
        public static string bPlat;
        public static List<ComponentTextInfo>[] ComponentLangurageList;
        public static List<TStripItemTextInfo>[] TStripLangurageList;
        public static List<CylinderCtrlTextInfo>[] CyCtrlLangList;
        public static List<CyldCtrlLangIniData> cyCtrlLangIniDataList;

        //Flow Control
        public static bool IsMaintenanceMode = false;
        public static bool bSaftyReady = false;
        public static bool SystemRun = false;
        public static RunMode SystemMode = RunMode.IDLE;
        public static bool UpConveyorInitialOk = false;
		public static bool DownConveyorInitialOk = false;
		public static bool GantryInitialOK = false;
		public static bool IsTryRun = false;

        public static bool ErrorWindowStatus = false;

        //Operation Time Record
        public static int ScanTime = 0;
        public static UInt64 OperationSecond = 0;
        public static UInt64 RunSecond = 0;
        public static UInt64 StopSecond = 0;
        public static DateTime StartWorkTM;
        public static DateTime EndWorkTM;
        //public static string T1;
        //public static string T2;
        //public static string T3;
        public static string CircleTime;
        //public static string ProcessTime;
        //public static JTimer RunTM = new JTimer();
    }

    public struct ComponentTextInfo
    {
        public string FormName;
        public string ComponentName;
        public string ComponentText;
        public Control Component;
    }
    public struct TStripItemTextInfo
    {
        public string FormName;
        public string ComponentName;
        public string ComponentText;
        public ToolStripItem Component;
    }
    public struct CylinderCtrlTextInfo
    {
        public string FormName;
        public string ComponentName;
        public string CyOff_BtnText;
        public string CyOn_BtnText;
    }

    public struct CyldCtrlLangIniData
    {
        public LanguageType Language;
        public string ComponentName;
        public string CyOff_BtnText;
        public string CyOn_BtnText;
    }
    public struct IOPortInfo
    {
        public string FormName;
        public string ComponentName;
        public string Port;
    }
    public struct Pos4D
    {
        public double x;
        public double y;
        public double z;
        public double u;

        public double x2;
        public double y2;
        public double z2;
        public double u2;

        public double x3;
        public double y3;
        public double z3;
        public double u3;

        public string barcode;
    }
    public struct CalibrationData
    {
        public double PixelX;
        public double PixelY;
        public double MotorPosX;
        public double MotorPosY;
    }
    public enum PermissionType
    {
        Operator = 0,
        Maintenance,		
        Administrator,
        None,
    }
    public enum LanguageType
    {
        Chinese = 0,
        English,
		Español
	}
    public enum RunMode
    {
        IDLE = 0,
        RUN,
        PAUSE,
        INITIAL,
    }
    public enum ResultCode
    {
        OK = 0,
        NG,
        Overtime,
        VisonNG
    }
    public struct FastenData
    {
        public string Point;
        public double Torque;
        public double Angle;
        public ResultCode Result;
    }
    public class BoardInfo
    {
        public bool bBoardInNG;
        public string SerialNumber;
        public List<FastenData> Data = new List<FastenData>();
        public bool bResultNG;
    }

    // RFID data 
    #region    RFID data 
    public struct RFIDCsvInfo
    {
        public DateTime RecordDateTime;
        // public DateTime WriteDateTime;
        public string UID;
        public string PalletNo;
        public string ProcessFlowCount;    //工序计数,完成加1
        public string WorkPlaceNo;
        public string ProcessType;           //   例如：“st22”
        //public string POneStatus;       //   OK/NG/RK，RK为返修后OK
        public string POneStatus;
        public string PTwoStatus;
        public string WkPlaceMark;      //增加
        public string POneNGFlag;       // 12工序，“0”: Pass;“1”: NG;
        public string PTwoNGFlag;       // 12工序，“0”: Pass;“1”: NG;
        public string POneCavityFlag;
        public string PTwoCavityFlag;
        public string POneInterInsulBarcode;   //增加
        public string PTwoInterInsulBarcode;   //增加
        public string POneBarcode1;
        public string POneBarcode2;
        public string POneTestData1;
        public string POneTestData2;
        public string PTwoBarcode1;
        public string PTwoBarcode2;
        public string PTwoTestData1;
        public string PTwoTestData2;
    }
    public struct RFIDCsvInfoFinal
    {
        public DateTime RecordDateTime;
        public string UID;
        public string PalletNo;
        public string WkPlaceMark1;
        public string WkPlaceMark2;
        public string ProductResult;
        public string POneOKFlagAll;
        public string PTwoOKFlagAll;
        public string POneCavityFlag;
        public string PTwoCavityFlag;
        //product one
        public string POneBarcode1;
        public string POneBarcode2;
        public string POneBarcode3;
        //public string POneBarcode4;
        public string POneTestData1;
        public string POneTestData2;
        //public string POneTestData3;
        public string POneTestData4;
        public string POneTestData5;
        //public string POneTestData6;
        //public string POneTestData7;
        //public string POneTestData8;

        //product Two
        public string PTwoBarcode1;
        public string PTwoBarcode2;
        public string PTwoBarcode3;
        //public string PTwoBarcode4;
        public string PTwoTestData1;
        public string PTwoTestData2;
        //public string PTwoTestData3;
        public string PTwoTestData4;
        public string PTwoTestData5;
        //public string PTwoTestData6;
        //public string PTwoTestData7;
        //public string PTwoTestData8;
    }

    public struct RFIDRecord
    {
        public string dataName;
        public string data;
    }
    public struct RFIDUIDRecords
    {
        public string UID;
        public List<RFIDRecord> rFIDRecords;
    }
    public struct RFIDAddrLng
    {
        public string dataName;
        public string address;
        public string length;
    }

    public enum RFIDProcessData
    {
        POneBarCode1 = 0,
        POneBarCode2,
        POneTestData1,
        POneTestData2,
        PTwoBarCode1,
        PTwoBarCode2,
        PTwoTestData1,
        PTwoTestData2,
    }
    public enum RFIDProdResult
    {
        NG = 0,
        OK,
        RK,      //Rework OK,代表返修后OK
    }
    public enum RFIDTwoCavityResult
    {
        Both_NG = 0,
        Prod_1_OK,
        Prod_2_OK,
        Both_OK,
    }
    #endregion
}
