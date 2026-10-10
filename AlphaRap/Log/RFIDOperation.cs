using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace AlphaRap.Classes
{
    public class RFIDOperation
    {
        public RFIDOperation()
        {
        }
        #region 数据定义
        //RFID 本站标准数据    
        //每机设定
        public readonly string FlwCnt_Last = "15";
        public readonly string FlwCnt = "16";
        public readonly string NGFlag_OK = "1";
        public readonly string NGFlag_NG = "0";


        public readonly string WKPlaceNo = "31";
        public readonly string RFIDProcType = "s31";
        public readonly string WkPlaceMk = "1";      //增加

        //RFID Command

        private const string RFIDCmdCfgUnit = "CU";
        private const string RFIDCmdCfgIO = "CI";
        private const string RFIDCmdRdUID = "RU";
        private const string RFIDCmdRead = "RD";
        private const string RFIDCmdWrite = "WR";
        private const string RFIDCmdGetAlarm = "DI";



        //RFID Configure unit string: [00] = 主机未发送标签编号;分隔符"_";[AS] = ASCII UNICODE UTF-8 代码页 437 格式;
        private const string CfgUnitStr = "00_00_00_00_00_AS";

        //RFID Read head IO configuraion
        private const string HeadEnable = "11";
        private const string Holdms = "0000";
        private const string BlockLength = "008";
        private const string Blocknumber = "250";
        private const string OverLoad = "00";
        private const string Overcurrent = "00";
        private const string TPUIDHold = "00";
        private const string CfgHeadIO = (HeadEnable + "_" + Holdms + "_" + BlockLength + "_" + Blocknumber + "_" + OverLoad + "_" + Overcurrent + "_" + TPUIDHold);

        //data Name

        public readonly string[] RFIDDataName = {PALLETNo,
            FLOWCount,
            NGFlagLast,
            PONENGFlag,
            //work place data
            WORKPlaceNo,
            PROCType,
            PONEStatus,
            WkPlaceMark,        //增加   [7]
            //product one  data
            PONEBarCode1,
            PONEBarCode2,
            PONETestData1,
            PONETestData2,
            //product two  data
            PTWOBarCode1,
            PTWOBarCode2,
            PTWOTestData1,
            PTWOTestData2,
            POneInterInsulBarcode,
            PTwoInterInsulBarcode,
            PONECavityFlag,
            PTWOCavityFlag,
            PTWOStatus,
            PTWONGFlag,
        };
        private const string PALLETNo = "palletNo";
        private const string FLOWCount = "flowCount";
        private const string NGFlagLast = "NGFlagLast";
        private const string PONENGFlag = "POneNGFlag";
        private const string PTWONGFlag = "PTwoNGFlag";

        private const string WORKPlaceNo = "PlaceNo";
        private const string PROCType = "ProcType";
        private const string PONEStatus = "POneStatus";
        private const string PTWOStatus = "PTwoStatus";
        private const string WkPlaceMark = "WkPlaceMark";     //增加

        private const string PONEBarCode1 = "POneBarCode1";
        private const string PONEBarCode2 = "POneBarCode2";
        private const string PONETestData1 = "POneTestData1";
        private const string PONETestData2 = "POneTestData2";

        private const string PTWOBarCode1 = "PTwoBarCode1";
        private const string PTWOBarCode2 = "PTwoBarCode2";
        private const string PTWOTestData1 = "PTwoTestData1";
        private const string PTWOTestData2 = "PTwoTestData2";

        private const string POneInterInsulBarcode = "POneInterInsulBarcode";
        private const string PTwoInterInsulBarcode = "PTwoInterInsulBarcode";

        private const string PONECavityFlag = "POneCavityFlag";
        private const string PTWOCavityFlag = "PTwoCavityFlag";

        //地址五位16进制，长度4位10进制 ,在此输入定义的数据地址
        // process data adress 
        //每机设定, 以st22 为例
        private const string ADDRPalletNo = "00052";
        private const string ADDRFlowCount = "00592";
        private const string ADDRNGFlagLast = "00622";
        private const string ADDRPOneNGFlag = "00623";
        private const string ADDRPTwoNGFlag = "00626";

        private const string LNGPalletNo = "0008";
        private const string LNGFlowCount = "0002";
        private const string LNGNGFlagLast = "0001";
        private const string LNGNGFlag = "0001";

        //work station data ADDRess       
        private const string ADDRWkPlaceNo = "01428";
        private const string ADDRProcType = "01430";
        private const string ADDRPOneStatus = "01436";
        private const string ADDRPTwoStatus = "01438";
        private const string ADDRWkPlaceMk = "01747";

        //work station data LNGess       
        private const string LNGWkPlaceNo = "0002";
        private const string LNGProcType = "0004";
        private const string LNGProdStatus = "0002";
        private const string LNGWkPlaceMk = "0002";

        //prodct one address & Length ;
        private const string ADDRPOneBarcode1 = "00000";
        private const string ADDRPOneBarcode2 = "00000";
        private const string ADDRPOneTestData1 = "00480";
        private const string ADDRPOneTestData2 = "00496";

        private const string LNGPOneBarcode1 = "0028";
        private const string LNGPOneBarcode2 = "0028";
        private const string LNGPOneTestData1 = "0008";
        private const string LNGPOneTestData2 = "0008";

        //prodct two address and length ;
        private const string ADDRPTwoBarcode1 = "00000";
        private const string ADDRPTwoBarcode2 = "00000";
        private const string ADDRPTwoTestData1 = "00488";
        private const string ADDRPTwoTestData2 = "00504";

        private const string LNGPTwoBarcode1 = "0028";
        private const string LNGPTwoBarcode2 = "0028";
        private const string LNGPTwoTestData1 = "0008";
        private const string LNGPTwoTestData2 = "0008";

        //Internal Insulation Barcode address
        private const string ADDRPOneInterInsuBarcode = "00128";
        private const string ADDRPTwoInterInsuBarcode = "00160";
        private const string LNGPInterInsuBarcode = "0028";

        //two Cavity Flag address
        private const string ADDRPOneCavityFlag = "00640";
        private const string ADDRPTwoCavityFlag = "00641";
        private const string LNGCavityFlag = "0001";
        #endregion

        //private TcpipClient RFIDHead= new TcpipClient();    //declare TCP socket
        private TcpClient clientRFIDHead = new TcpClient();
        private NetworkStream clientStreamRFID;

        //string send or receive 
        //private string strRFIDSend;                   //RFID command string
        private string strRFIDFeedResult = "";   //RFD feedback data   
        private String[] rFIDSplitdata;                //cut the feedback data to string arry

        //error message 
        #region  Error Message 
        public bool AlarmRFID { get; private set; }
        private string _errorMsg;
        public string ErrorMessage
        {
            get
            {
                return _errorMsg;
            }
        }
        #endregion

        //declare separator to parse answer strings
        private readonly char[] delimiterChars = { '_', '\r', '\n' };
        //declare RFID final staiton 
        public RFIDFinalStation RFIDFinal = new RFIDFinalStation();

        //上层操作
        #region 读RFID add final
        public void RFIDRead(string headIO, int RFIDReadType, ref List<RFIDRecord> rFIDRecords)
        {
            AlarmRFID = false;
            if (!RFIDIsConnected())
            {
                //ConnectRFID();
                //Delay(1000);
                InitialRFID(headIO);
                if (!clientRFIDHead.Connected)
                {
                    return;
                }
            }

            // 获取读数据地址
            List<RFIDAddrLng> listAddrLngs = new List<RFIDAddrLng>();
            switch (RFIDReadType)
            {
                case 0:
                    GetRFIDReadList(ref listAddrLngs);
                    break;
                case 1:
                    GetRFIDWriteList(ref listAddrLngs);
                    break;
                case 2:
                    RFIDFinal.GetRFIDFinalList(ref listAddrLngs);
                    break;
                default:
                    break;
            }

            //读取数据，存入records list
            foreach (RFIDAddrLng addrLng in listAddrLngs)
            {
                AlarmRFID = false;
                RFIDRecord record = new RFIDRecord
                {
                    dataName = addrLng.dataName,
                    data = ReadData(headIO, addrLng, out _errorMsg)
                };

                if (!AlarmRFID)
                {
                    rFIDRecords.Add(record);
                }
                else
                {
                    // 写报警             //读到的记录加入list
                }
            }
        }
        #endregion

        #region 写RFID
        public void RFIDWrite(string headIO, List<RFIDRecord> rFIDRecords)
        {
            AlarmRFID = false;
            if (!RFIDIsConnected())
            {
                //ConnectRFID();
                //Delay(100);
                InitialRFID(headIO);
            }

            //获取写数据地址
            List<RFIDAddrLng> listAddrLngs = new List<RFIDAddrLng>();
            GetRFIDWriteList(ref listAddrLngs);
            //查找对应的
            foreach (RFIDAddrLng addrLng in listAddrLngs)
            {
                string data = "";
                int Index = rFIDRecords.FindIndex(Record => (Record.dataName == addrLng.dataName));
                if (Index >= 0)
                {
                    data = rFIDRecords[Index].data;
                    WriteData(headIO, addrLng, data, out _errorMsg);
                    if (AlarmRFID)
                    {
                        // 写报警
                        return;
                    }
                }
            }
        }
        #endregion

        #region Final Clear
        public void RFIDFinalClear(string headIO)
        {
            AlarmRFID = false;
            //初始化一个clearData
            char SpaceChar = ' ';
            char[] Chars = new char[16];
            for (int i = 0; i < Chars.Count(); i++)
            {
                Chars[i] = SpaceChar;
            }
            string clearData = new string(Chars);
            //final address list
            List<RFIDAddrLng> listAddrLngs = new List<RFIDAddrLng>();
            RFIDFinal.GetRFIDFinalClearList(ref listAddrLngs);
            if (!RFIDIsConnected())
            {
                InitialRFID(headIO);
            }
            for (int i = 0; i < listAddrLngs.Count(); i++)
            {
                WriteData(headIO, listAddrLngs[i], clearData, out _errorMsg);
            }

            //foreach (RFIDAddrLng addrLng in listAddrLngs)
            //{
            //    WriteData(headIO, addrLng, clearData, out _errorMsg);
            //}
            ResetOKFlagWorkPlace(headIO);
        }
        private void ResetOKFlagWorkPlace(string headIO)
        {
            //初始化一个ResetData
            char ResetChar = '0';
            char[] Chars = new char[16];
            for (int i = 0; i < Chars.Count(); i++)
            {
                Chars[i] = ResetChar;
            }
            string ResetData = new string(Chars);
            //初始化一个 SetData
            char SetChar = '1';
            for (int i = 0; i < Chars.Count(); i++)
            {
                Chars[i] = SetChar;
            }
            string SetData = new string(Chars);

            //address list
            List<RFIDAddrLng> listAddrLngs = new List<RFIDAddrLng>();
            //RFIDFinal.GetFianlResetAddrList(ref listAddrLngs);
            //Wkplace reset "0"
            RFIDFinal.GetWkPlaceMkAddrList(ref listAddrLngs);
            foreach (RFIDAddrLng addrLng in listAddrLngs)
            {
                WriteData(headIO, addrLng, ResetData, out _errorMsg);
            }
            // FlagAll set "1"
            RFIDFinal.GetFlagAllAddrList(ref listAddrLngs);
            foreach (RFIDAddrLng addrLng in listAddrLngs)
            {
                WriteData(headIO, addrLng, SetData, out _errorMsg);
            }
        }
        #endregion

        #region 获取读地址
        private void GetRFIDReadList(ref List<RFIDAddrLng> listAddrLngs)
        {
            List<RFIDAddrLng> listAddrLngsTemp = new List<RFIDAddrLng>();
            //创建地址长度list
            listAddrLngsTemp.Add(new RFIDAddrLng() { dataName = RFIDDataName[0], address = ADDRPalletNo, length = LNGPalletNo });
            listAddrLngsTemp.Add(new RFIDAddrLng() { dataName = RFIDDataName[1], address = ADDRFlowCount, length = LNGFlowCount });
            listAddrLngsTemp.Add(new RFIDAddrLng() { dataName = RFIDDataName[2], address = ADDRNGFlagLast, length = LNGNGFlagLast });
            listAddrLngsTemp.Add(new RFIDAddrLng() { dataName = RFIDDataName[16], address = ADDRPOneInterInsuBarcode, length = LNGPInterInsuBarcode });
            listAddrLngsTemp.Add(new RFIDAddrLng() { dataName = RFIDDataName[17], address = ADDRPTwoInterInsuBarcode, length = LNGPInterInsuBarcode });
            listAddrLngsTemp.Add(new RFIDAddrLng() { dataName = RFIDDataName[18], address = ADDRPOneCavityFlag, length = LNGCavityFlag });
            listAddrLngsTemp.Add(new RFIDAddrLng() { dataName = RFIDDataName[19], address = ADDRPTwoCavityFlag, length = LNGCavityFlag });
            listAddrLngs = listAddrLngsTemp;
        }
        #endregion

        #region 获取写地址
        private void GetRFIDWriteList(ref List<RFIDAddrLng> listaddrLngs)
        {
            List<RFIDAddrLng> listAddrLngsTemp = new List<RFIDAddrLng>();
            //创建Address&Length list
            //Pallet No.   仅用于手动读
            listAddrLngsTemp.Add(new RFIDAddrLng() { dataName = RFIDDataName[0], address = ADDRPalletNo, length = LNGPalletNo });
            listAddrLngsTemp.Add(new RFIDAddrLng() { dataName = RFIDDataName[2], address = ADDRNGFlagLast, length = LNGNGFlagLast });
            // process flow
            listAddrLngsTemp.Add(new RFIDAddrLng() { dataName = RFIDDataName[1], address = ADDRFlowCount, length = LNGFlowCount });
            listAddrLngsTemp.Add(new RFIDAddrLng() { dataName = RFIDDataName[3], address = ADDRPOneNGFlag, length = LNGNGFlag });
            listAddrLngsTemp.Add(new RFIDAddrLng() { dataName = RFIDDataName[21], address = ADDRPTwoNGFlag, length = LNGNGFlag });
            //station data
            listAddrLngsTemp.Add(new RFIDAddrLng() { dataName = RFIDDataName[4], address = ADDRWkPlaceNo, length = LNGWkPlaceNo });
            listAddrLngsTemp.Add(new RFIDAddrLng() { dataName = RFIDDataName[5], address = ADDRProcType, length = LNGProcType });
            listAddrLngsTemp.Add(new RFIDAddrLng() { dataName = RFIDDataName[6], address = ADDRPOneStatus, length = LNGProdStatus });
            listAddrLngsTemp.Add(new RFIDAddrLng() { dataName = RFIDDataName[20], address = ADDRPTwoStatus, length = LNGProdStatus });
            listAddrLngsTemp.Add(new RFIDAddrLng() { dataName = RFIDDataName[7], address = ADDRWkPlaceMk, length = LNGWkPlaceMk });
            //product one
            listAddrLngsTemp.Add(new RFIDAddrLng() { dataName = RFIDDataName[8], address = ADDRPOneBarcode1, length = LNGPOneBarcode1 });
            listAddrLngsTemp.Add(new RFIDAddrLng() { dataName = RFIDDataName[9], address = ADDRPOneBarcode2, length = LNGPOneBarcode2 });
            listAddrLngsTemp.Add(new RFIDAddrLng() { dataName = RFIDDataName[10], address = ADDRPOneTestData1, length = LNGPOneTestData1 });
            listAddrLngsTemp.Add(new RFIDAddrLng() { dataName = RFIDDataName[11], address = ADDRPOneTestData2, length = LNGPOneTestData2 });
            //product two
            listAddrLngsTemp.Add(new RFIDAddrLng() { dataName = RFIDDataName[12], address = ADDRPTwoBarcode1, length = LNGPTwoBarcode1 });
            listAddrLngsTemp.Add(new RFIDAddrLng() { dataName = RFIDDataName[13], address = ADDRPTwoBarcode2, length = LNGPTwoBarcode2 });
            listAddrLngsTemp.Add(new RFIDAddrLng() { dataName = RFIDDataName[14], address = ADDRPTwoTestData1, length = LNGPTwoTestData1 });
            listAddrLngsTemp.Add(new RFIDAddrLng() { dataName = RFIDDataName[15], address = ADDRPTwoTestData2, length = LNGPTwoTestData2 });
            //cavity result
            listAddrLngsTemp.Add(new RFIDAddrLng() { dataName = RFIDDataName[18], address = ADDRPOneCavityFlag, length = LNGCavityFlag });
            listAddrLngsTemp.Add(new RFIDAddrLng() { dataName = RFIDDataName[19], address = ADDRPTwoCavityFlag, length = LNGCavityFlag });
            listaddrLngs = listAddrLngsTemp;
        }
        #endregion
                
        #region 初始化 RFID 
        public bool InitialRFID(String headIO)
        {
            bool bOK = false;
            AlarmRFID = false;
            if (!clientRFIDHead.Connected)
            {
                ConnectRFID();
                Delay(2000);
            }
            //ConnectRFID();
            CfgRFIDUnit(out _errorMsg);
            ConfigureHead(headIO, out _errorMsg);
            if (!AlarmRFID)
            {
                bOK = true;
            }
            return bOK;
        }
        #endregion

        //中间操作 
        #region 读UIDw
        public string ReadUID(string HeadIO)
        {
            string alarmTag;
            string erroMsg = "";
            AlarmRFID = false;
            string UID = "";
            //strRFIDRdHead = RFIDHead1;
            string strRFIDSend = "";
            strRFIDSend = RFIDCmdRdUID + "_" + HeadIO + "\r\n";       // UID指令 
            if (!RFIDIsConnected())
            {
                InitialRFID(HeadIO);
                if (!RFIDIsConnected())
                {
                    return "";
                }
            }
            try
            {
                if (clientRFIDHead.Connected)
                {
                    strRFIDFeedResult = SendCmdRFID(strRFIDSend);
                    rFIDSplitdata = strRFIDFeedResult.Split(delimiterChars);
                    alarmTag = rFIDSplitdata[2];
                    //check if a diagnosis event happen
                    if (alarmTag == "01")
                    {
                        AlarmRFID = true;
                        //read out diagnosis if error happen
                        erroMsg = ReadRFIDAlarm(HeadIO);
                        UID = "";
                    }
                    else
                    {
                        AlarmRFID = false;
                        erroMsg = "";
                        UID = rFIDSplitdata[4];
                    }
                }
            }
            catch (Exception)
            { }

            _errorMsg = erroMsg;
            return UID;
        }
        #endregion

        #region Readdata 
        //读数据
        public string ReadData(string headIO, RFIDAddrLng addrLng, out string ErrorMsg)
        {
            string userData = "";
            string alarmTag;
            string erroMsg = "";
            string strRFIDSend = "";
            AlarmRFID = false;
            //Read string
            strRFIDSend = RFIDCmdRead + "_" + headIO + "_" + addrLng.address + "_" + addrLng.length + "\r\n";
            strRFIDFeedResult = SendCmdRFID(strRFIDSend);
            rFIDSplitdata = strRFIDFeedResult.Split(delimiterChars);
            try
            {
                alarmTag = rFIDSplitdata[2];
                //check if a diagnosis event happen
                if (alarmTag == "01")
                {
                    AlarmRFID = true;
                    //read out diagnosis if error happen
                    erroMsg = ReadRFIDAlarm(headIO);
                    userData = "";
                }
                else
                {
                    AlarmRFID = false;
                    erroMsg = "";
                    userData = rFIDSplitdata[5];
                }
            }
            catch (Exception)
            {  }
            ErrorMsg = erroMsg;
            return userData;
        }
        #endregion

        #region WriteData 
        //写数据
        private void WriteData(string headIO, RFIDAddrLng addrLng, string writeUserData, out string ErrorMsg)
        {
            string userData;
            string alarmTag;
            string erroMsg = "";
            AlarmRFID = false;
            string strRFIDSend = "";

            Int32 length = Int32.Parse(addrLng.length);
            //补齐
            userData = writeUserData.PadRight(length, ' ');

            //write string
            strRFIDSend = RFIDCmdWrite + "_" + headIO + "_" + addrLng.address + "_" + addrLng.length + "_" + userData + "\r\n";
            strRFIDFeedResult = SendCmdRFID(strRFIDSend);
            rFIDSplitdata = strRFIDFeedResult.Split(delimiterChars);
            try
            {
                alarmTag = rFIDSplitdata[2];
                //check if a diagnosis event happen
                if (alarmTag == "01")
                {
                    AlarmRFID = true;
                    //read out diagnosis if error happen
                    erroMsg = ReadRFIDAlarm(headIO);
                }
                else
                {
                    AlarmRFID = false;
                    erroMsg = "";
                }
            }
            catch (Exception)
            {            }
            ErrorMsg = erroMsg;
        }
        #endregion

        #region 读Error message
        // 读取error message
        private string ReadRFIDAlarm(string headIO)
        {
            string recv;
            string[] recvArray;
            string erroMesg = "";
            string strRFIDSend = "";

            //发送读Erro message 指令
            strRFIDSend = RFIDCmdGetAlarm + "_" + headIO + "\r\n";
            recv = SendCmdRFID(strRFIDSend);
            recvArray = recv.Split(delimiterChars);
            try
            {
                erroMesg = recvArray[4];
            }
            catch (Exception)
            {   }
            return erroMesg;
        }
        #endregion

        #region 配置DTE104
        private bool CfgRFIDUnit(out string ErrorMsg)
        {
            bool cfgOK = false;
            string receive;
            string[] receiveArray;
            string alarmTag;
            string erroMsg = "";
            string strRFIDSend = "";

            AlarmRFID = false;
            //配置DTE04
            strRFIDSend = RFIDCmdCfgUnit + "_" + CfgUnitStr + "\r\n";
            receive = SendCmdRFID(strRFIDSend);
            receiveArray = receive.Split(delimiterChars);
            try
            {
                alarmTag = receiveArray[2];
                //check if a diagnosis event happen
                if (alarmTag == "01")
                {
                    AlarmRFID = true;
                    //read out diagnosis if error happen
                    erroMsg = ReadRFIDAlarm("01");        // 配置unit, 错误代码headIO 暂用“01”
                    cfgOK = false;
                }
                else
                {
                    AlarmRFID = false;
                    erroMsg = "";
                    cfgOK = true;
                }
            }
            catch (Exception)
            {  }
            ErrorMsg = erroMsg;
            return cfgOK;
        }
        #endregion

        #region 配置读写头
        private bool ConfigureHead(String headIO, out string ErrorMsg)
        {
            bool cfgOK = false;
            string receive;
            string[] receiveArray;
            string alarmTag;
            string erroMsg = "";
            string strRFIDSend = "";

            AlarmRFID = false;
            //配置HeadIO
            strRFIDSend = (RFIDCmdCfgIO + "_" + headIO + "_" + CfgHeadIO + "\r\n");
            receive = SendCmdRFID(strRFIDSend);
            receiveArray = receive.Split(delimiterChars);
            try
            {
                alarmTag = receiveArray[2];

                //check if a diagnosis event happen
                if (alarmTag == "01")
                {
                    AlarmRFID = true;
                    //read out diagnosis if error happen
                    erroMsg = ReadRFIDAlarm(headIO);
                    cfgOK = false;
                }
                else
                {
                    AlarmRFID = false;
                    erroMsg = "";
                    cfgOK = true;
                }
            }
            catch (Exception)
            { }
            ErrorMsg = erroMsg;
            return cfgOK;
        }
        #endregion

        // 低层
        #region 发送指令
        private string SendCmdRFID(string msg)
        {
            if (!RFIDIsConnected())
            {
                return "";
            }
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] buffer = encoder.GetBytes(msg);    //ASCII 编码
            try
            {
                //NetworkStream 
                clientStreamRFID = clientRFIDHead.GetStream();
                clientStreamRFID.Write(buffer, 0, buffer.Length);       // 数组，开始位置，长度
                clientStreamRFID.Flush();
            }
            catch (Exception)
            {           
            }

            // Receive the TcpServer.response. 
            // Buffer to store the response bytes. 
            Byte[] data = new Byte[2048];                //
            // String to store the response ASCII representation. 
            String responseData = String.Empty;
            // Read the first batch of the TcpServer response bytes. 
            try
            {
                Int32 bytes = 0;
                bytes = clientStreamRFID.Read(data, 0, data.Length);      //返回值是成功读取的字节数
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);    //截取字节
            }
            catch (Exception)
            {            }
            return responseData;        //返回接受数据
        }
        #endregion

        #region 通讯
        public void ConnectRFID()
        {
            clientRFIDHead = new TcpClient();
            clientRFIDHead.ReceiveTimeout = 3000;
            string txtIP = MiddleLayer.ParF.GetSettingValue("PSet", "txtRFIDIP1") + "." +
                           MiddleLayer.ParF.GetSettingValue("PSet", "txtRFIDIP2") + "." +
                           MiddleLayer.ParF.GetSettingValue("PSet", "txtRFIDIP3") + "." +
                           MiddleLayer.ParF.GetSettingValue("PSet", "txtRFIDIP4");

            //string txtIP = "192.168.0.79";
            int port = 33000;
            port = (int)MiddleLayer.ParF.GetSettingValue("PSet", "RFIDPort");
            //int.TryParse(MiddleLayer.ParF.GetSettingValue("PSet", "RFIDPort"),out port);
            if (!clientRFIDHead.Connected)
            {
                try
                {
                    clientRFIDHead.Connect(IPAddress.Parse(txtIP), port);
                    if (clientRFIDHead.Connected) SysPara.RFIDConnected = true;
                }
                catch (Exception)
                { }
            }
        }
        public void DisconnectRFID()
        {
            // bool bBypassBarcode = false;    //MiddleLayer.SystemS.GetSettingValue("Pset", "BypassRFID");   // Bypass需要修改
            try
            {
                if (clientRFIDHead.Connected)
                {
                    clientRFIDHead.Close();
                    SysPara.RFIDConnected = false;
                }
            }
            catch (Exception)
            { }
        }

        public bool RFIDIsConnected()
        {
            bool ret = false;
            try
            {
                if (clientRFIDHead.Connected)
                {
                    ret = true;
                }
                else { ret = false; }
            }
            catch
            {
            }
            return ret;
        }
        #endregion
        private void Delay(int Millisecond) //延迟系统时间，但系统又能同时能执行其它任务；
        {
            DateTime current = DateTime.Now;
            while (current.AddMilliseconds(Millisecond) > DateTime.Now)
            {
                Application.DoEvents();//转让控制权            
            }
            return;
        }
    }
}
