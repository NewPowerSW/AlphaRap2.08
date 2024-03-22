using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlphaRap.Classes
{
    public class NPSerialPort
    {

        //create an Serial Port object        
        private SerialPort sp = new SerialPort();

       
        //  Serial communication connection        
        public bool Connect(string strPortName, int iRate, int iDataBits, int iParity, int iStopBits)
        {
            try
            {
                sp.Dispose();
            }
            catch (Exception)
            {  }
            Parity par = Parity.None;
            switch (iParity)
            {
                case 0:
                    par = Parity.None;
                    break;
                case 1:
                    par = Parity.Odd;
                    break;
                case 2:
                    par = Parity.Even;
                    break;
                case 3:
                    par = Parity.Mark;
                    break;
                case 4:
                    par = Parity.Space;
                    break;
                default:

                    break;
            }

         
            StopBits sb = StopBits.None;
            switch (iStopBits)
            {
                case 0:
                    sb = StopBits.None;
                    break;
                case 1:
                    sb = StopBits.One;
                    break;
                case 2:
                    sb = StopBits.Two;
                    break;
                case 3:
                    sb = StopBits.OnePointFive;
                    break;
                default:
                    break;
            }

            sp.PortName = strPortName;
            sp.BaudRate = iRate;       
            sp.DataBits = iDataBits;   
            sp.Parity = par;  
            sp.StopBits = sb;  

            try
            {
                sp.Open();
                return sp.IsOpen;
            }
            catch (Exception)
            {
                return false;
            }
        }
       
        public void Dispose()
        {
            try
            {
                sp.Dispose();
            }
            catch (Exception)
            {  }
        }
        /// <summary>
        /// write string
        /// </summary>
        /// <param name="strWrite"></param>
        /// <returns></returns>
        public bool Write(string strWrite)
        {
            try
            {
                sp.DiscardInBuffer();
                sp.DiscardOutBuffer();
                sp.Write(strWrite);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// HEX write
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public bool Write(byte[] cmd)
        {
            try
            {
                sp.DiscardInBuffer();
                sp.DiscardOutBuffer();
                sp.Write(cmd, 0, cmd.Length);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// HEX write
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public bool Write_StringToHex(string cmd)
        {
            try
            {
                byte[] b = strToHexByte(cmd);
                sp.DiscardInBuffer();
                sp.DiscardOutBuffer();
                sp.Write(b, 0, b.Length);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool ConnectStates()
        {
            try
            {
                return sp.IsOpen;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// read string
        /// </summary>
        /// <returns></returns>
        public string readSting()
        {
            try
            {
                UTF8Encoding utf8 = new UTF8Encoding();
                Byte[] readBytes = new Byte[sp.BytesToRead];
                sp.Read(readBytes, 0, readBytes.Length);
                string decodedString = utf8.GetString(readBytes);
                return decodedString;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// read string in hexadecimal format
        /// </summary>
        /// <returns></returns>
        public string readStingByHex()
        {
            try
            {
               UTF8Encoding utf8 = new UTF8Encoding();
                Byte[] readBytes = new Byte[sp.BytesToRead];
                sp.Read(readBytes, 0, readBytes.Length);               
                string decodedString = ToHexStrFromByte(readBytes);
                return decodedString;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        /// <summary>
        ///Read serial string data
        /// </summary>
        /// <returns></returns>
        public string ReadStr()
        {
            try
            {
                string indata = sp.ReadExisting();
                return indata;
            }
            catch (Exception ex)
            {
                return "Err";
            }
        }
        /// <summary>
        /// read array of hexadecimal characters
        /// </summary>
        /// <returns></returns>
        public byte[] readChar()
        {
            try
            {
                int iBufferSize = sp.ReadBufferSize;
                byte[] buf = new byte[iBufferSize];
                sp.Read(buf, 0, iBufferSize);
                sp.DiscardInBuffer();
                return buf;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void ClearInBuffer()
        {
            try
            {
                sp.DiscardInBuffer();
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void ClearOutBuffer()
        {
            try
            {
                sp.DiscardOutBuffer();
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool ConnectComPressL(string strPortName, string CommunicationPara)
        {
            try
            {
                string[] condition = { "," };
                string[] result = CommunicationPara.Split(condition, StringSplitOptions.RemoveEmptyEntries);
                int iRate = Convert.ToInt32(result[0]);
                int iDataBits = Convert.ToInt32(result[1]);
                int iParity = Convert.ToInt32(result[2]);
                int iStopBits = Convert.ToInt32(result[3]);
                return Connect(strPortName, iRate, iDataBits, iParity, iStopBits);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public byte[] strToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        /// <summary>
        /// Byte array to hexadecimal string: space separated
        /// </summary>
        /// <param name="byteDatas"></param>
        /// <returns></returns>
        public string ToHexStrFromByte(byte[] byteDatas)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < byteDatas.Length; i++)
            {
                builder.Append(string.Format("{0:X2} ", byteDatas[i]));
            }
            return builder.ToString().Trim();
        }
    }
}
