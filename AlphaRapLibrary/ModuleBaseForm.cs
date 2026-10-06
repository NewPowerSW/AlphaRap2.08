using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
namespace AlphaRapLibrary
{
	public partial class ModuleBaseForm : Form
	{

		private static readonly object readdata = new object();
		private Task ManualTask;
		public CancellationTokenSource StopManualTask = new CancellationTokenSource();
		public ModuleBaseForm()
		{
			InitializeComponent();
			ModuleManager.ModuleList.Add(this);
		
     

        }
        public virtual void ModuleInitialize(string ModuleName)
		{

			// plMaintenance.AutoScroll = true;
			_ModuleName = ModuleName;

			for (int i = 0; i < RecipeData.Tables.Count; i++)
				RecipeData.Tables[i].Namespace = this.Text;
			ReadSettingData();
		}
		public virtual void ModuleDispose() { }

		protected bool bInitialOk { get; set; }
		protected string _ModuleName { get; set; }
		protected JTimer RunTM = new JTimer();

		//continue scan function
		public virtual void AlwaysRun() { }

		//initial function
		public virtual void InitialReset() { }
		public virtual void Initial() { bInitialOk = true; }
		public bool GetInitialOk() { return bInitialOk; }

		//running function
		public virtual void ServoOn() { }
		public virtual void ServoOff() { }
		public virtual void RunReset() { }
		public virtual void Run() { }
		public virtual void StartRun() { }
		public virtual void StopRun() { }
		public virtual void SetSpeed(int SpeedRate) { }

		//page contron function
		public virtual void IntoRecipeEditorPage() { }
		public virtual void ExitRecipeEditorPage() { }

		//btn click function
		public virtual void BeforRecipeEditor() { }
		public virtual void AfterRecipeEditor() { }

		//function
		public void InitialParameterReset()
		{
			bInitialOk = false;
		}

		public void ExecuteManual(Action ation)
		{
			if (ManualTask != null)
				if (ManualTask.Status == TaskStatus.Running)
					return;
			StopManualTask.Dispose();
			StopManualTask = new CancellationTokenSource();
			ManualTask = Task.Factory.StartNew(ation, StopManualTask.Token);
		}

		public bool IsManualRun()
		{
			if (ManualTask != null)
				if (ManualTask.Status == TaskStatus.Running)
					return true;
			return false;
		}

		public void ReadSettingData()
		{
			lock (readdata)
			{

				SettingData.Clear();
				string sSettingDataPath = String.Format("{0}\\{1}.xml", ModuleManager.SettingDataDirectory, _ModuleName);
				if (File.Exists(sSettingDataPath))
					SettingData.ReadXml(sSettingDataPath);
				for (int i = 0; i < SettingData.Tables.Count; i++)
					if (SettingData.Tables[i].Rows.Count == 0)
					{
						DataRow NewRow = SettingData.Tables[i].NewRow();

						SettingData.Tables[i].Rows.Add(NewRow);
					}
				SettingData.AcceptChanges();



			}

		}

		public void WriteSettingData()
		{
			lock (readdata)
			{
				string sSettingDataPath = String.Format("{0}\\{1}.xml", ModuleManager.SettingDataDirectory, _ModuleName);
				FileInfo fileInfo = new FileInfo(sSettingDataPath);
				if (fileInfo.Directory.Exists == false)
					fileInfo.Directory.Create();
				SettingData.AcceptChanges();
				SettingData.WriteXml(sSettingDataPath);


			}

		}

		public void ReadRecipeData(string RecipePath)
		{
			lock (readdata)
			{
				if (File.Exists(RecipePath))
				{
					DataSet ds = new DataSet();
					for (int i = 0; i < ModuleManager.ModuleList.Count; i++)
					{
						if (ModuleManager.ModuleList[i]._ModuleName == _ModuleName)
							ModuleManager.ModuleList[i].RecipeData.Clear();
						ds.Merge(ModuleManager.ModuleList[i].RecipeData);
					}
					ds.ReadXml(RecipePath);
					for (int i = 0; i < ds.Tables.Count; i++)
						if (ds.Tables[i].Namespace == _ModuleName)
						{
							try
							{
								DataTable dt = ds.Tables[i].Copy();
								RecipeData.Tables[ds.Tables[i].TableName].Clear();
								RecipeData.Merge(dt);
							}
							catch (Exception) { }
						}
				}
				for (int i = 0; i < RecipeData.Tables.Count; i++)
					if (RecipeData.Tables[i].Rows.Count == 0)
					{
						DataRow NewRow = RecipeData.Tables[i].NewRow();

						RecipeData.Tables[i].Rows.Add(NewRow);
					}
				RecipeData.AcceptChanges();
			}

		}

		public void WriteRecipeData(string RecipePath)
		{
			lock (readdata)
			{
				FileInfo fiTmp1 = new FileInfo(RecipePath);
				if (fiTmp1.Directory.Exists == false)
					fiTmp1.Directory.Create();
				RecipeData.AcceptChanges();

				DataSet ds = new DataSet();
				for (int i = 0; i < ModuleManager.ModuleList.Count; i++)
					ds.Merge(ModuleManager.ModuleList[i].RecipeData);
				ds.WriteXml(RecipePath);

			}
		}

		private dynamic GetDefaultValue(Type tp)
		{
			dynamic dmc = null;
			if (tp == typeof(Boolean))
				dmc = false;
			else if (tp == typeof(Int16))
				dmc = 0;
			else if (tp == typeof(Int32))
				dmc = 0;
			else if (tp == typeof(Int64))
				dmc = 0;
			else if (tp == typeof(double))
				dmc = 0;
			else if (tp == typeof(byte))
				dmc = 0;
			else if (tp == typeof(string))
				dmc = "";
			return dmc;
		}

		public dynamic GetSettingValue(string TableName, string ColumnName, int RowIndex = 0)
		{
			dynamic Value = null;
			if (SettingData.Tables[TableName].Columns.IndexOf(ColumnName) >= 0)
			{
				Value = SettingData.Tables[TableName].Rows[RowIndex][ColumnName, DataRowVersion.Original];
				if (Value.ToString() == "")
				{
					NPSDK.Alarm.Show("2025", "Setting data was not set value, ColumnName=\"" + ColumnName + "\"");
					Value = GetDefaultValue(SettingData.Tables[TableName].Columns[ColumnName].DataType);
				}
			}
			else
				NPSDK.Alarm.Show("2023", "Setting data was not found the column, ColumnName=\"" + ColumnName + "\"");
			return Value;
		}

		public dynamic GetRecipeValue(string TableName, string ColumnName, int RowIndex = 0)
		{
			dynamic Value = null;
			if (RecipeData.Tables[TableName].Columns.IndexOf(ColumnName) >= 0)
			{
				Value = RecipeData.Tables[TableName].Rows[RowIndex][ColumnName, DataRowVersion.Original];
				if (Value.ToString() == "")
				{
					NPSDK.Alarm.Show("2026", "Recipe data was not set value, ColumnName=\"" + ColumnName + "\"");
					Value = GetDefaultValue(RecipeData.Tables[TableName].Columns[ColumnName].DataType);
				}
			}
			else
				NPSDK.Alarm.Show("2024", "Recipe data was not found the column, ColumnName=\"" + ColumnName + "\"");
			return Value;
		}
		public bool SetSettingValue(string TableName, string ColumnName, object value, int RowIndex = 0)
		{

			if (SettingData.Tables[TableName].Columns.IndexOf(ColumnName) >= 0)
			{
				try
				{
					SettingData.Tables[TableName].Rows[RowIndex][ColumnName] = value;
				}
				catch (Exception ex)
				{

					NPSDK.Alarm.Show("2025", "Setting data Fail, ColumnName=\"" + ColumnName + "\"" + ex);
					return false;
				}

			}
			else
			{
				NPSDK.Alarm.Show("2025", "Setting data Fail, ColumnName=\"" + ColumnName + "\"");
				return false;
			}

			return true;
		}

		public bool SetRecipeValue(string TableName, string ColumnName, object value, int RowIndex = 0)
		{
			if (RecipeData.Tables[TableName].Columns.IndexOf(ColumnName) >= 0)
			{
				try
				{
					RecipeData.Tables[TableName].Rows[RowIndex][ColumnName] = value;
				}
				catch (Exception ex)
				{

					NPSDK.Alarm.Show("2025", "Setting data Fail, ColumnName=\"" + ColumnName + "\"" + ex);
					return false;
				}

			}
			else
			{
				NPSDK.Alarm.Show("2025", "Setting data Fail, ColumnName=\"" + ColumnName + "\"");
				return false;
			}

			return true;
		}
	}
}
