using System;
using System.Diagnostics;
using System.IO.Ports;

namespace NP_PressureSensor
{
    public class FFU
    {
        private SerialPort sp = null;
        private string _data;
        public int Timeout = 100;

        public double FFUSpeedRate = 250.0 / 1350.0;
        /// <summary>
        /// 打开串口
        /// </summary>
        /// <param name="PortName">COM号</param>
        /// <param name="BaudRate">波特率</param>
        /// <param name="paity">奇偶位</param>
        /// <param name="DataBits">数据位</param>
        /// <param name="stopBits">停止位</param>
        /// <returns></returns>
        public bool OpenPort(string PortName, int BaudRate, Parity paity, int DataBits, StopBits stopBits)//打开串口
        {
            sp = new SerialPort(PortName, BaudRate, paity, DataBits, stopBits);
            sp.ReadTimeout = 10;
            try
            {
                if (!sp.IsOpen)
                {
                    sp.Open();
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
                return false;
            }
            return true;
        }
        /// <summary>
        /// 关闭串口
        /// </summary>
        public bool ClosePort()//关闭串口
        {
            try
            {
                sp.Close();
                return true;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
            }
            return false;
        }
        private static string ByteToHex(byte[] Bytes)
        {
            string str = string.Empty;
            //foreach (byte Byte in Bytes)
            //{
            //    str += String.Format("{0:X2}", Byte) + " ";
            //}
            for (int i = 0; i < Bytes.Length; i++)
            {
                str += String.Format("{0:X2}", Bytes[i]) + " ";
            }
            return str.Trim("0".ToCharArray());
        }
        /// <summary>
        /// 数据接收
        /// </summary>
        /// <returns></returns>
        public string DataReceiveFunction()//数据接收
        {
            Byte[] data = new Byte[1024];

            try
            {
                if (sp.IsOpen)
                {
                    sp.ReadTimeout = Timeout;
                    int bytes = sp.Read(data, 0, data.Length);

                    if (bytes != 0)
                    {
                        _data = ByteToHex(data);
                        string str = string.Empty;
                        for (int i = 0; i < bytes; i++)
                        {
                            str += String.Format("{0:X2}", data[i]) + " ";
                        }

                        //string SPlit_String = str.Replace(" ", "").Substring(6, 8);
                        //double int_10 = Convert.ToInt32(SPilt_String, 16);
                        //string sb = (int_10 / 100).ToString();
                        //return sb;
                        //return SPlit_String;
                        return str;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {                
                //MessageBox.Show(ex.Message);
            }
            return null;
        }

        public double ConvertSpeed(string dataSpeed)
        {
            double speed = 0;
            string[] separator = new string[] { " " };
            //separator[0] = " ";
            if (!string.IsNullOrEmpty(dataSpeed))
            {
                string[] command = dataSpeed.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                if (command.Length >= 4) { speed = To16Convert10(command[3]) / FFUSpeedRate; }
            }
            return speed;
        }

        static int To16Convert10(string str)
        {
            int res = 0;

            try
            {
                str = str.Trim().Replace(" ", "");
                res = int.Parse(str, System.Globalization.NumberStyles.AllowHexSpecifier);
            }
            catch (Exception)
            {
                res = 0;
            }

            return res;
        }

        public static byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }

        /// <summary>
        /// 数据发送
        /// </summary>
        /// <param name="data">发送的数据</param>
        public bool SendSerialPortData(string data)
        {
            try
            {
                if (sp.IsOpen)
                {
                    HexStringToBytes(data);
                    sp.Write(HexStringToBytes(data), 0, HexStringToBytes(data).Length);
                    //sp.WriteLine(data + "\r");
                    return true;
                }
            }
            catch (Exception)
            {
                //MessageBox.Show(ex.Message);
            }
            return false;
        }
        private static byte[] HexStringToBytes(string hs)
        {
            string[] strArr = hs.Trim().Split(' ');
            byte[] b = new byte[strArr.Length];
            //逐个字符变为16进制字节数据
            for (int i = 0; i < strArr.Length; i++)
            {
                b[i] = Convert.ToByte(strArr[i], 16);
            }
            //按照指定编码将字节数组变为字符串
            return b;
        }
    }
}
