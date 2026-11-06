using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;

namespace DataSave
{
    public struct CSVInfo
    {
        public DateTime m_DateTime;
        public DateTime m_AlarmTime;
        public string m_Barcode;
        public string m_nT1;
        public string m_nT2;
        public string m_nT3;
        public string m_nCircleTime;
        public string m_nProcessTime;
        public string m_Data;
        public string m_nStatus;
        public string Type;
        public string Code;
        public string Content;
        public DateTime m_LogTime;
        public string m_UserName;
        public string m_Permission;
        public DateTime m_RunDataTime;
        public string m_InfoDes;
      
    }
    
    public class CCSVObj
    {
        private List<CSVInfo> m_csvInfo = null;
        public CCSVObj()
        {
            m_csvInfo = new List<CSVInfo>();
           
        }

        public List<string[]> ReadCSV(string strPath)
        {
            List<string[]> returndata = null;
            string[] strTemp = null;
            if (!File.Exists(strPath))
            {

                return returndata;
            }
            string[] strContent = File.ReadAllLines(strPath, Encoding.GetEncoding("GB2312"));
           
            for(int i=1;i<strContent.Length;i++)
            {
                strTemp = strContent[i].Split(',');
                returndata.Add(strTemp);


            }
            return returndata;
        }
       
        
        public bool ReadFilePatch(string strPath)
        {
            if (!Directory.Exists(strPath))
            {
                return false;
            }
            else
            {
                return true;
            }

        }
        public List<CSVInfo> GetCSVObjs()
        {
            return m_csvInfo;
        }
      
        public void writeCSV(string newPach,string[] strInfo  )

        {
            //创建文件完全路径名称
            string mrPath = string.Format("{0}\\{1}.csv", newPach, DateTime.Now.ToString("yyyy-MM-dd"));//字符串格式化生成一个CSV文件 
                                                                                                     //判断文件流是否存在,不存在创建文件
            if (!System.IO.File.Exists(newPach))
            {
                //创建文件流对向
               
                FileStream fs = new FileStream(newPach, FileMode.Create, FileAccess.Write);
                //创建文件流操作对象,并绑定文件流
                StreamWriter sw = new StreamWriter(fs);
                //创建数据存储对象,并向其中添加数据
                StringBuilder sb = new StringBuilder();
               //sb.Append(strInfo[0]).Append(",").Append(strInfo[1]).Append(",").Append(strInfo[2]).Append(",").Append(strInfo[3]).Append(",").
                //    Append(strInfo[4]).Append(",").Append(strInfo[5]).Append(",").Append(strInfo[6]).Append(",").Append(strInfo[7]).Append(",").
                //    Append(strInfo[8]);
                //将数据写入文件缓冲流中
                //sw.WriteLine(sb);
                sw.Flush();
                sw.Close();
                fs.Close();
            }
            //创建文件流操作对象,并绑定文件,指明文件数据添加模式
            StreamWriter msw = new StreamWriter(newPach, true, Encoding.Default);
            //创建数据存储对象,并向其中添加数据
            StringBuilder msb = new StringBuilder();
            msb.Append(strInfo[0]).Append(",").Append(strInfo[1]).Append(",").Append(strInfo[2]).Append(",").Append(strInfo[3]).Append(",").
                    Append(strInfo[4]).Append(",").Append(strInfo[5]).Append(",").Append(strInfo[6]).Append(",").Append(strInfo[7]).Append(",").
                    Append(strInfo[8]);
            //将数据写入文件流中
            msw.WriteLine(msb);
            msw.Flush();
            msw.Close();

        }
        
    
    }

}

