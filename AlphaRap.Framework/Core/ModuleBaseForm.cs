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
using AlphaRap;
using AlphaRap.Configuration;
namespace AlphaRapLibrary
{
	public partial class ModuleBaseForm : Form
	{

		private static readonly object readdata = new object();
        private ModuleConfiguration configuration;
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IConfigurationModule RuntimeModule { get; private set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ModuleConfiguration ConfigurationData
        {
            get { return configuration ?? (configuration = new ModuleConfiguration(_ModuleName ?? Text, SettingData, RecipeData)); }
        }

        /// <summary>以独立模块的数据绑定兼容字段，界面和主保存使用同一份模型。</summary>
        protected void BindModule(IConfigurationModule module)
        {
            if (RuntimeModule != null) throw new InvalidOperationException(ModuleText.AlreadyBound);
            RuntimeModule = module ?? throw new ArgumentNullException(nameof(module));
            configuration = module.Configuration;
            SettingData = configuration.Settings;
            RecipeData = configuration.Recipe;
        }
		private Task ManualTask;
		private readonly object manualSync = new object();
		private Task manualCancellation = Task.FromResult(0);
		public CancellationTokenSource StopManualTask = new CancellationTokenSource();
		public ModuleBaseForm()
		{
			InitializeComponent();
			ModuleManager.ModuleList.Add(this);
		
     

        }
        /// <summary>加载绑定模块或兼容数据；配置身份不跟随页面显示标题改变。</summary>
        public virtual void ModuleInitialize(string ModuleName)
		{

			// plMaintenance.AutoScroll = true;
			_ModuleName = ModuleName;

            foreach (DataTable table in RecipeData.Tables) table.Namespace = ConfigurationData.StorageName;
            if (RuntimeModule != null) RuntimeModule.Initialize(new ModuleContext(ModuleManager.SettingDataDirectory));
            else ReadSettingData();
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

		/// <summary>同一模块只允许一个未退出的手动操作，停止清理期间不创建新会话。</summary>
		public void ExecuteManual(Action ation)
		{
			if (ation == null) throw new ArgumentNullException(nameof(ation));
			CancellationTokenSource source;
			StartManual(ation, out source);
		}

		/// <summary>异步等待一份手动动作及其取消清理，忙碌时明确拒绝，不丢弃调用方任务。</summary>
		public async Task ExecuteManualAsync(Action action)
		{
			if (action == null) throw new ArgumentNullException(nameof(action));
			CancellationTokenSource source;
			Task work = StartManual(action, out source);
			if (work == null) throw new InvalidOperationException(AlphaRap.FrameworkLanguage.LangMsg("DeviceControl", "manual_Busy",
				"当前模块仍有未结束的操作。", "The module still has an unfinished operation.", "El módulo aún tiene una operación pendiente."));
			try { await work.ConfigureAwait(false); }
			finally
			{
				Task cancellation; lock (manualSync) cancellation = ReferenceEquals(source, StopManualTask) ? manualCancellation : Task.FromResult(0);
				await cancellation.ConfigureAwait(false);
			}
		}

		/// <summary>在同一锁中检查占用并启动唯一手动会话。</summary>
		private Task StartManual(Action action, out CancellationTokenSource source)
		{
			lock (manualSync)
			{
				source = null;
				if (AlphaRap.SysPara.SystemMode == AlphaRap.RunMode.STOPPING || IsManualRun()) return null;
				StopManualTask.Dispose();
				StopManualTask = new CancellationTokenSource();
				source = StopManualTask;
				CancellationToken token = StopManualTask.Token;
				ManualTask = Task.Factory.StartNew(() =>
				{
					using (NPSDK.Flow_Chart.UseExecutionToken(token)) action();
				}, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
				return ManualTask;
			}
		}

		/// <summary>排队、等待中的手动任务及其取消清理都计入实际占用。</summary>
		public bool IsManualRun()
		{
			lock (manualSync) return (ManualTask != null && !ManualTask.IsCompleted) || !manualCancellation.IsCompleted;
		}

		/// <summary>立即登记手动停止，隔离可能阻塞的取消回调并保留清理任务。</summary>
		public Task RequestManualStop()
		{
			lock (manualSync)
			{
				if (!manualCancellation.IsCompleted || StopManualTask == null) return manualCancellation;
				if (StopManualTask.IsCancellationRequested) return Task.FromResult(0);
				CancellationTokenSource source = StopManualTask;
				manualCancellation = Task.Factory.StartNew(() => source.Cancel(), CancellationToken.None,
					TaskCreationOptions.LongRunning, TaskScheduler.Default);
				return manualCancellation;
			}
		}

        /// <summary>通过无窗口配置对象加载设置，绑定的数据表实例保持不变。</summary>
        public void ReadSettingData() { lock (readdata) ConfigurationData.LoadSettings(ModuleManager.SettingDataDirectory); }

        /// <summary>兼容写入机器设置，使用稳定的模块存储路径。</summary>
		public void WriteSettingData()
		{
			lock (readdata)
			{
				string sSettingDataPath = ConfigurationData.SettingsPath(ModuleManager.SettingDataDirectory);
				FileInfo fileInfo = new FileInfo(sSettingDataPath);
				if (fileInfo.Directory.Exists == false)
					fileInfo.Directory.Create();
				SettingData.AcceptChanges();
				SettingData.WriteXml(sSettingDataPath);


			}

		}

		/// <summary>加载模块配方，普通参数表补齐默认行，允许空行集合的明细表保持为空。</summary>
		public void ReadRecipeData(string RecipePath)
		{
			lock (readdata) ConfigurationData.LoadRecipe(RecipePath);
		}

        /// <summary>兼容写入合并后的模块配方，不遗漏无窗口模块的数据。</summary>
		public void WriteRecipeData(string RecipePath)
		{
			lock (readdata)
			{
				FileInfo fiTmp1 = new FileInfo(RecipePath);
				if (fiTmp1.Directory.Exists == false)
					fiTmp1.Directory.Create();
                using (DataSet snapshot = ProjectConfiguration.CaptureModuleRecipes()) snapshot.WriteXml(RecipePath);
				RecipeData.AcceptChanges();

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
