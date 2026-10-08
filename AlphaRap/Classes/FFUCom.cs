using System;
using System.Threading;

namespace AlphaRap.Classes
{
   public class FFUCom
    {
        NPSerialPort FFUCOM = new NPSerialPort();

        public static string com = "";
        public static string para = "";
        private double FFUSpeedRate = 250.0 / 1350.0;       //速度转化比

        public bool connectStatus = false;
        public bool ConnectStatus
        {
            get
            {
                if (FFUCOM != null) { return FFUCOM.ConnectStates(); }
                else { return false; }
            }
        }

        //断开连接
        public bool Connect(string comStr,string paraStr)
        {
            com = comStr;
            para = paraStr;
            try
            {
                if (FFUCOM.ConnectComPressL(comStr, paraStr))
                {
                    return true;
                }
                else
                    return false;
            }
            catch (Exception)
            {
                //Log.log.Write(ex.ToString(), Color.Red);
                return false;
            }
        }
        //断开连接
        public void DisConnect()
        {
            if (FFUCOM.ConnectStates())
            {
                FFUCOM.Dispose();
            }
        }        
        //打开FFU（最大风速）
        public bool OpenFFUMaxSpeed()
        {
            if (!FFUCOM.ConnectStates())
                Connect(com, para);
            bool b1 = FFUCOM.Write_StringToHex("35 41 01 FA 70");
            bool b2 = FFUCOM.Write_StringToHex("35 42 01 FA 73");
            Thread.Sleep(100);
            string strMid = FFUCOM.readStingByHex();
            if (strMid.Length>8)
            {
                return true;               
            }
            else { return false; }
        }
        //获取风速
        public double GetFFUSpeed()
        {
            try
            {
                if (!FFUCOM.ConnectStates())
                    Connect(com, para);
                bool b1 = FFUCOM.Write_StringToHex("15 21 01 CA");
                Thread.Sleep(100);
                string strMid = FFUCOM.readStingByHex();
                string[] command = strMid.Split(' ');
                double speed = 0;
                if (command.Length >= 4){ speed = To16Convert10(command[3]) / FFUSpeedRate; }
                return speed;
            }
            catch (Exception)
            {
                //Log.log.Write(ex.ToString(),Color.Red);
                return 0.0 ;
            }
        }

        public double GetFFUSpeed2()
        {
            try
            {
                if (!FFUCOM.ConnectStates())
                    Connect(com, para);
                bool b1 = FFUCOM.Write_StringToHex("15 22 01 C9");
                Thread.Sleep(100);
                string strMid = FFUCOM.readStingByHex();
                string[] command = strMid.Split(' ');
                double speed = 0;
                if (command.Length >= 4) { speed = To16Convert10(command[3]) / FFUSpeedRate; }
                return speed;
            }
            catch (Exception)
            {
                //Log.log.Write(ex.ToString(),Color.Red);
                return 0.0;
            }
        }
        /// <summary>
        /// 设置风速
        /// </summary>
        /// <param name="speed"></param>
        public bool SetFFUSpeed(Int32 speed)
        {
            if (!FFUCOM.ConnectStates())
                Connect(com, para);
            string speedcmd = ((int)(speed * FFUSpeedRate)).ToString("X2");
            string basecmd = "35 41 01 "+ speedcmd;
            string basecmd2 = "35 42 01 " + speedcmd;
            string checkCode = GetCheckCode(basecmd);
            string checkCode2 = GetCheckCode(basecmd2);
            bool b1 = FFUCOM.Write_StringToHex(basecmd + " " + checkCode);
            bool b2 = FFUCOM.Write_StringToHex(basecmd2 + " " + checkCode2);
            Thread.Sleep(100);
            string strMid = FFUCOM.readStingByHex();
            if (strMid.Length>8)
            {
                return true;
            }
            else { return false; }
        }
        #region 十六进制字符串转十进制
        /// <summary>
        /// 十六进制字符串转十进制
        /// </summary>
        /// <param name="str">十六进制字符</param>
        /// <returns></returns>
       private static int To16Convert10(string str)
        {
            int res = 0;

            try
            {
                str = str.Trim().Replace(" ", "");//移除空字符
                //方法1
                res = int.Parse(str, System.Globalization.NumberStyles.AllowHexSpecifier);
            }
            catch (Exception)
            {
                res = 0;
            }

            return res;
        }
        #endregion

        #region 校验码计算
        private string GetCheckCode(string command)
        {
            command += " FF";
            byte[] cmd = strToHexByte(command);
            byte x;
            x = 0;
            for (int i = 0; i < cmd.Length; i++)
            {
                x ^= cmd[i];
            }
            return string.Format("{0:X2} ", x);
        }
#endregion
        private byte[] strToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }
    }
}
