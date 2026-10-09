using System.Windows.Forms;

namespace AlphaRap
{
	public partial class DataForm : Form
	{
		string Errordata = string.Empty;
		string Rundata = string.Empty;
		string MESdata = string.Empty;
		readonly object LogErrorLock = new object();
		readonly object LogRunLock = new object();
		readonly object MesLock = new object();
		readonly object UserLoginLock = new object();
		readonly object UserOperationLock = new object();
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

		/// <summary>
		/// 界面操作日志：每个界面一个目录（见 <see cref="OperationLog.RootPath"/>），
		/// 列 = 时间（库自动填）/ 工号 / 权限 / 操作内容；落盘复用 DataSave 库，文件按天生成。
		/// </summary>
		public void AddOperationLog(string formName, string action)
		{
			lock (UserOperationLock)
			{
				if (UserOperationLog == null) return;
				UserOperationLog.SavePath = OperationLog.RootPath + "\\" + SafeFolder(formName);
				string[] dataArray = { SysPara.UserName, SysPara.UserPermission.ToString(), action };
				UserOperationLog.SaveDataToFile(dataArray);
			}
		}

		/// <summary>界面名转成合法目录名。</summary>
		private static string SafeFolder(string name)
		{
			if (string.IsNullOrEmpty(name)) return "Unknown";
			char[] bad = System.IO.Path.GetInvalidFileNameChars();
			for (int i = 0; i < bad.Length; i++) name = name.Replace(bad[i], '_');
			return name;
		}
	}
}
