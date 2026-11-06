using System;
using System.Collections.Generic;

namespace AlphaRap
{
    internal static class StationData
    {
        public static bool RFIDConnected;
        public static bool UpConveyorInitialOk;
        public static bool DownConveyorInitialOk;
        public static bool GantryInitialOK;
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
    #region RFID data
    public struct RFIDCsvInfo
    {
        public DateTime RecordDateTime;
        public string UID;
        public string PalletNo;
        public string ProcessFlowCount;    //工序计数,完成加1
        public string WorkPlaceNo;
        public string ProcessType;           //   例如：“st22”
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
        public string POneTestData1;
        public string POneTestData2;
        public string POneTestData4;
        public string POneTestData5;

        //product Two
        public string PTwoBarcode1;
        public string PTwoBarcode2;
        public string PTwoBarcode3;
        public string PTwoTestData1;
        public string PTwoTestData2;
        public string PTwoTestData4;
        public string PTwoTestData5;
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
