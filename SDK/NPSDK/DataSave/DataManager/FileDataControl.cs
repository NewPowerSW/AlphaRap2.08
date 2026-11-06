using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataSave
{
    public class FileDataControl
    {
        public FileDataControl()
        {
        }
        //write a new file, existed file will be overwritten  
        public  void WriteCSV(string filePathName, String[] ls)
        {
            WriteCSV(filePathName, true, ls);
        }
        //write a file, existed file will be overwritten if append = false  
        public  void WriteCSV(string filePathName, bool append, String[] ls)
        {
            try
            {
    

                StreamWriter fileWriter = new StreamWriter(filePathName, append, Encoding.Default);

                fileWriter.WriteLine(String.Join(",", ls));

                fileWriter.Flush();
                fileWriter.Close();
                
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        } 
     
      

        public  List<String[]> ReadCSV(string filePathName)
        {
            List<String[]> ls = new List<String[]>();
           
            string[] strContent = File.ReadAllLines(filePathName, Encoding.GetEncoding("GB2312"));

            for (int i = 0; i < strContent.Length; i++)
            {
                string[] strTemp = strContent[i].Split(',');
                ls.Add(strTemp);

            }

           
            return ls;
        }
        
        
        public  void WriteExcelData(string FileName, string[] data)
        {

            FileInfo fi = new FileInfo(FileName);
            var di = fi.Directory;
            if(!di.Exists)
            {
                di.Create();

            }
            WriteCSV(FileName, data);
            
        }
    }
}
