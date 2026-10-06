using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
//using mscorlib;
namespace AlphaRap.PLC
{
	public enum SoftElemType
	{

		ELEM_QX = 0,     //QX元件
		ELEM_MW = 1,     //MW元件
		ELEM_X = 2,      //X元件(对应QX200~QX300)
		ELEM_Y = 3,      //Y元件(对应QX300~QX400)

		//H3U
		REGI_H3U_Y = 0x20,       //Y元件的定义	
		REGI_H3U_X = 0x21,      //X元件的定义							
		REGI_H3U_S = 0x22,      //S元件的定义				
		REGI_H3U_M = 0x23,      //M元件的定义							
		REGI_H3U_TB = 0x24,     //T位元件的定义				
		REGI_H3U_TW = 0x25,     //T字元件的定义				
		REGI_H3U_CB = 0x26,     //C位元件的定义				
		REGI_H3U_CW = 0x27,     //C字元件的定义				
		REGI_H3U_DW = 0x28,     //D字元件的定义				
		REGI_H3U_CW2 = 0x29,        //C双字元件的定义
		REGI_H3U_SM = 0x2a,     //SM
		REGI_H3U_SD = 0x2b,     //
		REGI_H3U_R = 0x2c,      //

								//H5u
		REGI_H5U_Y = 0x30,       //Y元件的定义	
		REGI_H5U_X = 0x31,      //X元件的定义							
		REGI_H5U_S = 0x32,      //S元件的定义				
		REGI_H5U_M = 0x33,      //M元件的定义	
		REGI_H5U_B = 0x34,       //B元件的定义
		REGI_H5U_D = 0x35,       //D字元件的定义
		REGI_H5U_R = 0x36,       //R字元件的定义

	}
	/// <summary>
	/// 具体PLC类：汇川5系列，使用网口 Tcp 通讯，接收指令
	/// 可配置的属性包括：网口连接参数
	/// V1.0 初始版本
	/// 修改时间2023-11-19----------------
	/// </summary>
	public class INOVANCE
    {
        #region
        [DllImport("StandardModbusApi.dll", EntryPoint = "Init_ETH_String", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool Init_ETH_String(string sIpAddr, int nNetId = 0, int IpPort = 502);

		[DllImport("StandardModbusApi.dll", EntryPoint = "Exit_ETH", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool Exit_ETH(int nNetId = 0);

		[DllImport("StandardModbusApi.dll", EntryPoint = "H5u_Write_Soft_Elem", CallingConvention = CallingConvention.Cdecl)]
		private static extern int H5u_Write_Soft_Elem(SoftElemType eType, int nStartAddr, int nCount, byte[] pValue, int nNetId = 0);

		[DllImport("StandardModbusApi.dll", EntryPoint = "H5u_Read_Soft_Elem", CallingConvention = CallingConvention.Cdecl)]
		private static extern int H5u_Read_Soft_Elem(SoftElemType eType, int nStartAddr, int nCount, byte[] pValue, int nNetId = 0);

		[DllImport("StandardModbusApi.dll", EntryPoint = "H5u_Read_Device_Block", CallingConvention = CallingConvention.Cdecl)]
		private static extern int H5u_Read_Device_Block(SoftElemType eType, int nStartAddr, int nCount, byte[] pValue, int nNetId = 0);

		[DllImport("StandardModbusApi.dll", EntryPoint = "H5u_Write_Device_Block", CallingConvention = CallingConvention.Cdecl)]
		private static extern int H5u_Write_Device_Block(SoftElemType eType, int nStartAddr, int nCount, byte[] pValue, int nNetId = 0);
		#endregion

        public static bool Connet(string sIpAddr, int nNetId = 0, int IpPort = 502)
        {
            return Init_ETH_String(sIpAddr, nNetId, IpPort);


        }

        readonly object ReadPLC_Lock = new object();
        /// <summary>
        /// 写入返回的错误
        /// </summary>
        public string WritErrorMessage = string.Empty;
        /// <summary>
        /// 写入返回的错误
        /// </summary>
        public string ReadErrorMessage = string.Empty;
        /// <summary>
        /// 读取PLC值
        /// </summary>
        /// <param name="nStartAddr">类型+地址，例如:D1010/param>
        /// <param name="nCount">长度</param>
        /// <returns></returns>
        public string ReadPlc(string nStartAddr, int nCount, string type = "int")
		{
			lock(ReadPLC_Lock)
			{
				byte[] pBuf = new byte[16000];
				bool bIsWord = false;
				ReadErrorMessage = string.Empty;	
				string readType = nStartAddr.Substring(0, 1);

				#region 写入类型
				SoftElemType ElemType = SoftElemType.REGI_H5U_Y;
				if (readType == "Y")
				{
					ElemType = SoftElemType.REGI_H5U_Y;
				}
				else if (readType == "X")
				{
					ElemType = SoftElemType.REGI_H5U_X;
				}
				else if (readType == "S")
				{
					ElemType = SoftElemType.REGI_H5U_S;
				}
				else if (readType == "M")
				{
					ElemType = SoftElemType.REGI_H5U_M;
				}

				else if (readType == "D")
				{
					bIsWord = true;
					ElemType = SoftElemType.REGI_H5U_D;
				}
                else if (readType == "R")
                {
                    bIsWord = true;
                    ElemType = SoftElemType.REGI_H5U_R;
                }
                #endregion

                int ReadAddrd = Convert.ToInt32(nStartAddr.Substring(1));
				int nRet = H5u_Read_Device_Block(ElemType, ReadAddrd, nCount, pBuf, 0);

				if (nRet != 1)
				{
					ReadErrorMessage = DateTime.Now.ToString() + "：ErrorCode " + nRet.ToString() + "\r\n";
					return "-999";
				}
				string strData = "";

				if (type == "int" || type == "float")
				{
					nCount = nCount / 2;
                  

                }
				for (int i = 0; i < nCount; i++)
				{
					if (bIsWord)
					{
						if (type == "bit" )//16位整形
						{
							byte[] databuf = new byte[2] { 0, 0 };
							databuf[0] = pBuf[i * 2];
							databuf[1] = pBuf[i * 2 + 1];
							int iTemp = BitConverter.ToInt16(databuf, 0);
							strData = strData + iTemp.ToString() + ",";
							continue;
						}
						else if (type == "int")//读取32位整形
						{
							byte[] databuf = new byte[4] { 0, 0, 0, 0 };
							databuf[0] = pBuf[i * 4];
							databuf[1] = pBuf[i * 4 + 1];
							databuf[2] = pBuf[i * 4 + 2];
							databuf[3] = pBuf[i * 4 + 3];
							int iTemp = BitConverter.ToInt32(databuf, 0);
							strData = strData + iTemp.ToString();
							continue;
						}
						else if ( type == "float")//读取浮点型
						{
							byte[] databuf = new byte[4] { 0, 0, 0, 0 };
							databuf[0] = pBuf[i * 4];
							databuf[1] = pBuf[i * 4 + 1];
							databuf[2] = pBuf[i * 4 + 2];
							databuf[3] = pBuf[i * 4 + 3];
							float fTemp = BitConverter.ToSingle(databuf, 0);
							strData = strData + fTemp.ToString();
							continue;
						}
					}
					else
					{
						int nVal = 0;
						nVal = pBuf[i];
						strData = strData + nVal.ToString() ;
					}
				}

				ReadErrorMessage = DateTime.Now.ToString() + "读取成功！\r\n";
				return strData;
			}
			
		}

        public string ReadPlcString(string nStartAddr, int nCount)   //test
        {

            lock (ReadPLC_Lock)
            {
                byte[] pValue = new byte[nCount * 2];



                bool bIsWord = false;
                ReadErrorMessage = string.Empty;
                string readType = nStartAddr.Substring(0, 1);

                #region 写入类型
                SoftElemType ElemType = SoftElemType.REGI_H5U_Y;
                if (readType == "Y")
                {
                    ElemType = SoftElemType.REGI_H5U_Y;
                }
                else if (readType == "X")
                {
                    ElemType = SoftElemType.REGI_H5U_X;
                }
                else if (readType == "S")
                {
                    ElemType = SoftElemType.REGI_H5U_S;
                }
                else if (readType == "M")
                {
                    ElemType = SoftElemType.REGI_H5U_M;
                }

                else if (readType == "D")
                {
                    bIsWord = true;
                    ElemType = SoftElemType.REGI_H5U_D;
                }
                else if (readType == "R")
                {
                    bIsWord = true;
                    ElemType = SoftElemType.REGI_H5U_R;
                }
                #endregion
                int ReadAddrd = Convert.ToInt32(nStartAddr.Substring(1));
                int nRet = H5u_Read_Device_Block(ElemType, ReadAddrd, nCount, pValue, 0);

                if (nRet != 1)
                {
                    ReadErrorMessage = DateTime.Now.ToString() + "：ErrorCode " + nRet.ToString() + "\r\n";
                    return "-999";
                }
                string strData = "";

                strData = Encoding.UTF8.GetString(pValue);
				int Steing_Index = strData.IndexOf('\0');
				strData = strData.Substring(0, Steing_Index);
                return strData;
            }

        }
        /// <summary>
        /// WritPLCdata
        /// </summary>
        /// <param name="star_nub">【Fist Port】</param>
        /// <param name="count">【ReadLenght】</param>
        /// <param name="S_pValue"></param>
        /// <returns></returns>
        public bool WritePlcString(string nStartAddr, string writeValue)
        {
            lock (PLCWriteLock)
            {
                byte[] pBu = Encoding.UTF8.GetBytes(writeValue.ToCharArray());

                int addr = Convert.ToInt32(nStartAddr.Substring(1));

                string[] arr = writeValue.Split(',');
                string writeType = nStartAddr.Substring(0, 1);
                SoftElemType ElemType = SoftElemType.REGI_H5U_Y;
                if (writeType == "Y")
                {
                    ElemType = SoftElemType.REGI_H5U_Y;
                }
                else if (writeType == "X")
                {
                    ElemType = SoftElemType.REGI_H5U_X;
                }
                else if (writeType == "S")
                {
                    ElemType = SoftElemType.REGI_H5U_S;
                }
                else if (writeType == "M")
                {
                    ElemType = SoftElemType.REGI_H5U_M;
                }
                else if (writeType == "D")
                {
                   
                    ElemType = SoftElemType.REGI_H5U_D;
                }
                else if (writeType == "R")
                { 
                    ElemType = SoftElemType.REGI_H5U_R;
                }
                int nRet = H5u_Write_Device_Block(ElemType, addr, pBu.Length, pBu, 0);

                if (nRet != 1)
                {
                    WritErrorMessage = DateTime.Now.ToString() + "：ErrorCode " + nRet.ToString() + "\r\n";
                    return false;
                }
                return true;
            }

        }

        readonly object PLCWriteLock = new object();
		public bool WritePlc(string nStartAddr, string writeValue, string type="int")
		{
			lock(PLCWriteLock)
			{

				byte[] pBuf = new byte[16000];
				WritErrorMessage = string.Empty;
				bool bIsWord = false;//是否字元件
                string nDataType = "1";

                if (type == "bit" || type == "int16")
				{
					nDataType = "0";
				}
				else if (type == "int32")
				{
					nDataType = "1";
				}
				else if (type == "float")
				{
					nDataType = "2";
				}
				string writeType = nStartAddr.Substring(0, 1);

				SoftElemType ElemType = SoftElemType.REGI_H5U_Y;
				if (writeType == "Y")
				{
					ElemType = SoftElemType.REGI_H5U_Y;
				}
				else if (writeType == "X")
				{
					ElemType = SoftElemType.REGI_H5U_X;
				}
				else if (writeType == "S")
				{
					ElemType = SoftElemType.REGI_H5U_S;
				}
				else if (writeType == "M")
				{
					ElemType = SoftElemType.REGI_H5U_M;
				}
				else if (writeType == "D")
				{
					bIsWord = true;
					ElemType = SoftElemType.REGI_H5U_D;
				}
                else if (writeType == "R")
                {
                    bIsWord = true;
                    ElemType = SoftElemType.REGI_H5U_R;
                }

                string[] arr = writeValue.Split(',');

				GetDataFromUI(pBuf, arr, bIsWord, int.Parse(nDataType));
				int addr = Convert.ToInt32(nStartAddr.Substring(1));

				int nRet = H5u_Write_Device_Block(ElemType, addr, arr.Length * 2, pBuf, 0);

				if (nRet != 1)
				{
					WritErrorMessage = DateTime.Now.ToString() + "：ErrorCode " + nRet.ToString() + "\r\n";
					return false;
				}
				return true;
			}
		}

		private string GetDataFromUI(byte[] pBuf, string[] arr, bool bIsWord, int nDataType)
		{
			for (int i = 0; i < arr.Length; i++)
			{
				if (arr[i] == string.Empty)
				{
					break;
				}
				double nVal = Convert.ToDouble(arr[i]);
				if (bIsWord)
				{
					if (nDataType == 0)//16位整形
					{
						if (nVal > 32767)
						{
							return "当前大于16位符号整数32767，值超出范围。";
						}
						int idata = Convert.ToInt16(arr[i]);
						byte[] dataBuf = new byte[2] { 0, 0 };
						dataBuf = BitConverter.GetBytes(idata);
						pBuf[2 * i] = dataBuf[0];
						pBuf[2 * i + 1] = dataBuf[1];
					}
					else if (nDataType == 1)//32位整形
					{
						if (nVal > 65535)
						{
							return "当前大于16位无符号整数65535，值超出范围。";
						}
						int idata = Convert.ToInt32(arr[i]);
						byte[] dataBuf = new byte[4] { 0, 0, 0, 0 };
						dataBuf = BitConverter.GetBytes(idata);
						pBuf[4 * i] = dataBuf[0];
						pBuf[4 * i + 1] = dataBuf[1];
						pBuf[4 * i + 2] = dataBuf[2];
						pBuf[4 * i + 3] = dataBuf[3];
					}
					else if (nDataType == 2)//浮点数
					{
						float fdata = Convert.ToSingle(arr[i]);
						byte[] dataBuf = new byte[4] { 0, 0, 0, 0 };
						dataBuf = BitConverter.GetBytes(fdata);
						pBuf[4 * i] = dataBuf[0];
						pBuf[4 * i + 1] = dataBuf[1];
						pBuf[4 * i + 2] = dataBuf[2];
						pBuf[4 * i + 3] = dataBuf[3];
					}
				}
				else
				{
					pBuf[i] = (byte)nVal;
				}
			}
			return "";
		}

		private string getErrName(int err)
		{
			switch (err)
			{
				case 0: return "读写失败";
				case 1: return "读写成功";
				case 2: return "未连接PLC";
				case 3: return "元件类型错误";
				case 4: return "元件地址溢出";
				case 5: return "元件个数超限";
				case 6: return "通讯异常";
				default: return "无返异常代码";
			}

		}

		private int asciiToInt16(int nStartAddr, string strValue)
		{
			int count = 0;
			int nRet = -1;
			byte[] dataBuf = null;
			if (strValue.Length % 2 == 1)
			{

				count = (strValue.Length / 2) + 1;
				strValue = strValue.PadLeft(strValue.Length + 1, '0');
				dataBuf = new byte[count];
			}
			else
			{
				count = strValue.Length / 2;
				dataBuf = new byte[count];
			}
			dataBuf = System.Text.Encoding.ASCII.GetBytes(strValue);
			SoftElemType ElemType = SoftElemType.REGI_H5U_D;
			nRet = H5u_Write_Device_Block(ElemType, nStartAddr, count, dataBuf, 0);
			return nRet;
		}
		private int outAsciiCode(int nStartAddr, int nCount, out string Value)
		{
			int nRet = -1;
			Value = string.Empty;
			byte[] pBuf = new byte[nCount * 2];
			SoftElemType ElemType = SoftElemType.REGI_H5U_D;
			nRet = H5u_Read_Device_Block(ElemType, nStartAddr, nCount, pBuf, 0);
			Value = Encoding.ASCII.GetString(pBuf);
			return nRet;
		}

		private void Closed()
		{
			int nNetId = 0;
			bool result = Exit_ETH(nNetId);
			if (result == true)
			{

				MessageBox.Show(MiddleLayer.LangMsg("PLC", "msg_PlcCloseOk", "关闭连接成功", "Connection closed", "Conexión cerrada"));

			}
			else
			{
				MessageBox.Show(MiddleLayer.LangMsg("PLC", "msg_PlcCloseFail", "关闭连接失败", "Failed to close connection", "Error al cerrar la conexión"));

			}

		}



	}
}
