using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlphaRap
{
	public partial class DataForm : Form
	{
		string Errordata = string.Empty;
		string Rundata = string.Empty;
		string MESdata = string.Empty;
		string UserLogindata = string.Empty;
		readonly object LogErrorLock = new object();
		readonly object LogRunLock = new object();
		readonly object MesLock = new object();
		readonly object UserLoginLock = new object();
		public string[] UserLoginLog = new string[2];
		public DataForm()
		{
			InitializeComponent();

		}
		public void AddLogError(string data)
		{
			
			lock (LogErrorLock)
			{
				if (Errordata != data)
				{
					Errordata = data;
					string[] dataArray = { data };
					LogError.SaveDataToFile(dataArray);
					MiddleLayer.MainF.WriteErrorMessageText(data);

				}
			}

		}
		public void AddRunLog(string data)
		{
			
			lock (LogRunLock)
			{
				if (Rundata != data)
				{
					Rundata = data;
					string[] dataArray = { data };
					LogRun.SaveDataToFile(dataArray);
					MiddleLayer.MainF.WriteRUNMessageText(data);
				}
			}

		}
		public void SaveMesLog(string data)
		{
			lock (MesLock)
			{
				if (MESdata != data)
				{
					MESdata = data;
					string[] dataArray = { data };
					MESLOG.SaveDataToFile(dataArray);

				}
			}

		}
		public void SaveUserLoginLog(string[] data)
		{	
			lock (UserLoginLock)
			{
				UserLogin.SaveDataToFile(data);
			}
		}


	}
}
