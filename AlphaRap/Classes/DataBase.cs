using System;
using System.Data;
using System.Data.OleDb;

namespace AlphaRap
{
    class DataBase
    {
        public static DataTable ReadAllData_Adapter(string tableName, string mdbPath, ref bool success)
        {
            DataTable dt = new DataTable();
            try
            {
                //1、建立连接 
                string strConn = "Provider=Microsoft.ACE.OLEDB.12.0; Data Source=" + AppDomain.CurrentDomain.BaseDirectory + mdbPath;
                OleDbConnection odcConnection = new OleDbConnection(strConn);
                //2、打开连接 
                odcConnection.Open();
                //建立SQL查询 
                string strSQL = "select * from " + tableName;
                OleDbDataAdapter oleDa = new OleDbDataAdapter(strSQL, odcConnection);
                oleDa.Fill(dt);
                //关闭连接 
                oleDa.Dispose();
                odcConnection.Close();
                success = true;
                return dt;
            }
            catch (Exception)
            {
                success = false;
                return dt;
            }
        }
        public static DataTable ReadData_Adapter(string mdbPath, string SQL, ref bool Successful)
        {
            DataTable dt = new DataTable();
            try
            {
                //1、建立连接 
                string strConn = "Provider=Microsoft.ACE.OLEDB.12.0; Data Source=" + AppDomain.CurrentDomain.BaseDirectory + mdbPath;
                OleDbConnection odcConnection = new OleDbConnection(strConn);
                //2、打开连接 
                odcConnection.Open();
                //建立SQL查询 
                OleDbDataAdapter oleDa = new OleDbDataAdapter(SQL, odcConnection);
                oleDa.Fill(dt);

                //关闭连接 
                oleDa.Dispose();
                odcConnection.Close();
                Successful = true;
                return dt;
            }
            catch (Exception)
            {
                Successful = false;
                return dt;
            }
        }
        public static int DataBaseExecute(string mdbPath, string ExecuteSQL)
        {
            //1、建立连接 
            string strConn = "Provider=Microsoft.ACE.OLEDB.12.0; Data Source=" + AppDomain.CurrentDomain.BaseDirectory + mdbPath;
            OleDbConnection odcConnection = new OleDbConnection(strConn);
            //2、打开连接 
            odcConnection.Open();
            //建立SQL查询 
            OleDbCommand odCommand = odcConnection.CreateCommand();
            try
            {
                odCommand.CommandText = ExecuteSQL;
                odCommand.ExecuteNonQuery();
                return 0;
            }
            catch (Exception)
            {
                return -1;
            }
            finally
            {
                odCommand.Dispose();
                odcConnection.Dispose();
            }
        }
    }
}
