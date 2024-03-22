using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlphaRap
{
    class TextLogWrite
    {
        static object objLock = new object();
        static public void AppendLog(string strMessage)
        {
            lock (objLock)
            {
                if (!Directory.Exists(@".//RunMessage/"))
                {
                    Directory.CreateDirectory(@".//RunMessage/");
                }
                string strFileName = Application.StartupPath + "//RunMessage//" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                File.AppendAllText(strFileName, DateTime.Now.ToString("HH:mm:ss") + "     " + strMessage + "\r\n");

            }
        }
        static public void AppendLog(string strMessage, String strFilePath)
        {
            lock (objLock)
            {
                if (!Directory.Exists(strFilePath))
                {
                    Directory.CreateDirectory(strFilePath);
                }
                string strFileName = strFilePath + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                File.AppendAllText(strFileName, DateTime.Now.ToString("HH:mm:ss") + "     " + strMessage + "\r\n");
            }
        }
    }
}
