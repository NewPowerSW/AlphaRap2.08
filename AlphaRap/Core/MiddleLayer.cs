
using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using AlphaRap.Classes;
using AlphaRapLibrary;
using NPSDK;
using System.Threading;
using LifeSpan;
using AlphaRap.MenuForm;
using AlphaRap.TaskProcess;
using AlphaRap.MES;
using Alpha;
using NPSDK.Motion;

namespace AlphaRap
{
	partial class MiddleLayer
	{
		//===================Module Form===================

		public static LifeSpanForm SpanLifeF;
		public static Flow FlowF;
		public static MainForm MainF;
		public static SystemForm SystemF;
		public static HomeForm HomeF;
		public static ManualForm ManualF;
		public static HardForm HardF;
		public static CheckForm CheckF;
		public static LogForm LogF;
		public static DataForm DataF;
		public static Parameter ParF;
		public static SystemSetting SystemS;
		public static PlatFormSetting PlatF;
		public static AddUserForm AddF;
		public static VPForm VPF;
		public static ProductManagerForm ProductF;

		public static LockForm1 LockForm1;

		public static LENS MesF;
		public static int LoadProcessRate = 0;
		public static LoadingForm LoadingF;

		public static AlwaysRunTask alTask;
		public static AlarmRunTask alarmRunTask;

		public static List<dynamic> lstForm = new List<dynamic>();

		#region FlowChatForm

		public static UpConveryor UpConveyorF;
		public static DownConveryor DownConveyorF;
		public static Gantry GantryF;
		#endregion
		public static bool bTask = true;
		//任务运行信息
		public static int iTaskStep = 0;

		public static bool bTaskFlowOK = false;
		public static bool bTaskFlowOK2 = false;
		public static bool bConveyorTaskOK = false;
		public static bool gEXIT = false;
		public static AtlasLibrary.MTF6000 Atlas_MTF6000 = new AtlasLibrary.MTF6000();

		public static List<string> AlarmList = new List<string>();

		public static void InitialProject3()
		{
            ReadLanguageSettings();
			#region Load Ini File
			LoadingF.SetCaption(LangMsg("LoadingForm", "stage_Ini", "读取系统配置", "Loading system configuration", "Cargando configuración del sistema"));

			IniFile iniFile = new IniFile(".\\MachineSetup.ini");
			SysPara.ProjectName = iniFile.ReadString("MachineSetup", "ProjectName", "AlphaRap3.0");
			SysPara.RecipeName = iniFile.ReadString("MachineSetup", "RecipeName", "Recipe");
			SysPara.Simulation = iniFile.ReadBoolen("LogicSetup", "Simulation", false);
			SysPara.LogFilePath = iniFile.ReadString("PathSetup", "LogFileDirectory", ".\\LogFile");
			SysPara.VisionFileDirectory = iniFile.ReadString("PathSetup", "VisionDataDirectory", ".\\VisionData");
			SysPara.AlarmTableDirectory = iniFile.ReadString("PathSetup", "AlarmTableDirectory", ".\\AlarmTable");
			SysPara.SettingDataDirectory = iniFile.ReadString("PathSetup", "SettingDataDirectory", ".\\ModuleData\\SettingData");
			SysPara.RecipeDataDirectory = iniFile.ReadString("PathSetup", "RecipeDirectory", ".\\ModuleData\\RecipeData");
			SysPara.MESDirectory = iniFile.ReadString("PathSetup", "MESDirectory", ".\\ModuleData\\MESData");
			SysPara.IOPortDirectory = iniFile.ReadString("PathSetup", "IOPortDirectory", ".\\ModuleData");

			ModuleManager.SettingDataDirectory = SysPara.SettingDataDirectory;
			#endregion

			#region Load Alarm Table
			LoadingF.SetCaption(LangMsg("LoadingForm", "stage_Alarms", "载入报警表", "Loading alarms", "Cargando alarmas"));
			LoadAlarmTable();
			#endregion

			#region Creat Module
			LoadingF.SetCaption(LangMsg("LoadingForm", "stage_Forms", "载入页面", "Loading pages", "Cargando páginas"));
			//=======================Module Create===========================
			SystemF = CreateForm(SystemF, "SystemForm");

			MainF = CreateForm(MainF, "MainForm");

			#endregion

			InitialLanguageData();

			SwitchLanguage(SysPara.LanguageShow);

			SwitchPermission(SysPara.UserPermission);


           

            OpenRecipe(string.Format("{0}\\{1}.xml", SysPara.RecipeDataDirectory, SysPara.RecipeName));
			ServoOn();
		}
		public static FlowControl FlowCtrl;
		/// <summary>
		/// 初始化项目
		/// </summary>
		public static void InitialProject()
		{
            ReadLanguageSettings();
			#region Load Ini File

			IniFile iniFile = new IniFile(".\\MachineSetup.ini");
			SysPara.ProjectName = iniFile.ReadString("MachineSetup", "ProjectName", "AlphaRap2.0");
			SysPara.RecipeName = iniFile.ReadString("MachineSetup", "RecipeName", "Recipe");
			SysPara.Simulation = iniFile.ReadBoolen("LogicSetup", "Simulation", false);
			SysPara.LogFilePath = iniFile.ReadString("PathSetup", "LogFileDirectory", ".\\LogFile");
			SysPara.VisionFileDirectory = iniFile.ReadString("PathSetup", "VisionDataDirectory", ".\\VisionData");
			SysPara.AlarmTableDirectory = iniFile.ReadString("PathSetup", "AlarmTableDirectory", ".\\AlarmTable");
			SysPara.SettingDataDirectory = iniFile.ReadString("PathSetup", "SettingDataDirectory", ".\\ModuleData\\SettingData");
			SysPara.RecipeDataDirectory = iniFile.ReadString("PathSetup", "RecipeDirectory", ".\\ModuleData\\RecipeData");
			SysPara.MESDirectory = iniFile.ReadString("PathSetup", "MESDirectory", ".\\ModuleData\\MESData");
			SysPara.IOPortDirectory = iniFile.ReadString("PathSetup", "IOPortDirectory", ".\\ModuleData");

			ModuleManager.SettingDataDirectory = SysPara.SettingDataDirectory;
			SysPara.FilePath = SysPara.RecipeDataDirectory + "\\" + SysPara.RecipeName + ".xml";
			#endregion

			#region Load Alarm Table
			LoadAlarmTable();
			#endregion
			//=======================Module Create===========================

			#region FlwoChatForm
			GantryF = CreateForm(GantryF, "Gantry");
			UpConveyorF = CreateForm(UpConveyorF, "UpConveryor");
			DownConveyorF = CreateForm(DownConveyorF, "DownConveryor");
			#endregion

			SpanLifeF = new LifeSpanForm();
			LoadingF.SetCaption(LangMsg("LoadingForm", "stage_Products", "载入型号管理", "Loading model management", "Cargando gestión de modelos"));
			ProductF = CreateForm(ProductF, "ProductManagerForm");
			PlatF = CreateForm(PlatF, "PlatFormSetting");
			SystemS = CreateForm(SystemS, "SystemSetting");
			ParF = CreateForm(ParF, "Parameter");
			SystemF = CreateForm(SystemF, "SystemForm");
			LoadingF.SetCaption(LangMsg("LoadingForm", "stage_Main", "载入主界面", "Loading main window", "Cargando ventana principal"));
			MainF = CreateForm(MainF, "MainForm");
			LoadingF.SetCaption(LangMsg("LoadingForm", "stage_Home", "载入主页", "Loading home page", "Cargando página de inicio"));
			HomeF = CreateForm(HomeF, "HomeForm");
			LoadingF.SetCaption(LangMsg("LoadingForm", "stage_Manual", "载入手动页面", "Loading manual controls", "Cargando controles manuales"));
			ManualF = CreateForm(ManualF, "ManualForm");
			HardF = CreateForm(HardF, "HardForm");
			CheckF = CreateForm(CheckF, "CheckForm");
			LogF = CreateForm(LogF, "LogForm");

			AddF = CreateForm(AddF, "AddUserForm");
			VPF = CreateForm(VPF, "VPForm");
			FlowF = new Flow();

			DataF = CreateForm(DataF, "DataForm");
			LockForm1 = new LockForm1();
			MesF = CreateForm(MesF, "LENS");

			LoadingF.SetCaption(LangMsg("LoadingForm", "stage_Languages", "载入语言表", "Loading translations", "Cargando traducciones"));
			InitialLanguageData();

			SwitchLanguage(SysPara.LanguageShow);

			LoadingF.SetCaption(LangMsg("LoadingForm", "stage_Permissions", "载入权限", "Loading permissions", "Cargando permisos"));
			SwitchPermission(SysPara.UserPermission);
			LoadingF.SetCaption(LangMsg("LoadingForm", "stage_IO", "初始化输入输出", "Initializing IO", "Inicializando E/S"));

			SDKKernal.SetSimulation(SysPara.Simulation);
			SDKKernal.InitializeComponent();

			// 界面操作日志：设备初始化（IO / 运动内核装载完成）
			OperationLog.Write("MainForm", "设备初始化完成（仿真=" + SysPara.Simulation + "）");

			alTask = new AlwaysRunTask();
			alarmRunTask = new AlarmRunTask();

			LoadingF.SetCaption(LangMsg("LoadingForm", "stage_Recipe", "载入配方", "Loading recipe", "Cargando receta"));
			OpenRecipe(string.Format("{0}\\{1}.xml", SysPara.RecipeDataDirectory, SysPara.RecipeName));

			FlowCtrl = new FlowControl();
			FlowCtrl.StartThread();

			ServoOn();
			LoadingF.SetCaption(LangMsg("LoadingForm", "stage_Vision", "载入视觉算法", "Loading vision algorithms", "Cargando algoritmos de visión"));
		}

		public static void ServoOn()
		{
			foreach (ModuleBaseForm Module in ModuleManager.ModuleList)
			{
				try
				{
					Module.ServoOn();
				}
				catch (Exception ex)
				{
					MiddleLayer.DataF.AddLogError(ex.ToString());
					NPSDK.Alarm.Show("2011", "An unpredictable error occurred on ServoOn() ! ModuleName=\"" + Module.Name + "\"");
					StopRun();
				}
			}
		}
		public static void ServoOff()
		{
			foreach (ModuleBaseForm Module in ModuleManager.ModuleList)
			{
				try
				{
					Module.ServoOff();
				}
				catch (Exception ex)
				{
					MiddleLayer.DataF.AddLogError(ex.ToString());
					NPSDK.Alarm.Show("2012", "An unpredictable error occurred on ServoOff() ! ModuleName=\"" + Module.Name + "\"");
					StopRun();
				}
			}
		}

		public static void DisposeProject()
		{
			ServoOff();
		}

		#region LanguageData
		#region Convey
		public static void StartRunCV()
		{
			foreach (KeyValuePair<NP_Output, int> item in FlowLineStatus)
			{
				if (item.Value == 0)
				{
					item.Key.On();
				}
			}
		}
		static Dictionary<NP_Output, int> FlowLineStatus = new Dictionary<NP_Output, int>();
		public static void StopRunCV()
		{
			try
			{
				FlowLineStatus.Clear();
				for (int i = 0; i < MiddleLayer.ManualF.Conveyor_List.Count; i++)
				{
					if (FlowLineStatus.ContainsKey(MiddleLayer.ManualF.Conveyor_List[i]))
					{
						FlowLineStatus[MiddleLayer.ManualF.Conveyor_List[i]] = ReturnOutPutStaus(MiddleLayer.ManualF.Conveyor_List[i]);
					}
					else
					{
						FlowLineStatus.Add(MiddleLayer.ManualF.Conveyor_List[i], ReturnOutPutStaus(MiddleLayer.ManualF.Conveyor_List[i]));
					}
					MiddleLayer.ManualF.Conveyor_List[i].Off();
				}
			}
			catch (Exception)
			{
			}
		}

		static int ReturnOutPutStaus(NP_Output output)
		{
			if (output.IsOn())
			{
				return 0;
			}
			return -1;
		}
		#endregion
        /// <summary>初始化三语登记表，扫描所有启动页及框架自定义控件。</summary>
        private static void InitialLanguageData()
        {
            int count = Enum.GetValues(typeof(LanguageType)).Length;
            SysPara.ComponentLangurageList = new List<ComponentTextInfo>[count];
            SysPara.TStripLangurageList = new List<TStripItemTextInfo>[count];
            SysPara.CyCtrlLangList = new List<CylinderCtrlTextInfo>[count];
            for (int i = 0; i < count; i++)
            {
                SysPara.ComponentLangurageList[i] = new List<ComponentTextInfo>();
                SysPara.TStripLangurageList[i] = new List<TStripItemTextInfo>();
                SysPara.CyCtrlLangList[i] = new List<CylinderCtrlTextInfo>();
            }
            foreach (Control form in lstForm) RegisterLanguage(form, form.Name);
            if (LockForm1 != null) RegisterLanguage(LockForm1, "LockForm1");
            if (FlowF != null) RegisterLanguage(FlowF, "Flow");
            if (SpanLifeF != null) RegisterLanguage(SpanLifeF, "SpanLife");
        }

		public static void InitialLangCallbackCyldCtrl(Form FormName, LanguageType Lantype, ref List<CyldCtrlLangIniData> cyCtrlLangIniDataList, ref List<CylinderCtrlTextInfo> CyCtrlLangList)
		{
			List<CylinderCtrlTextInfo> cyCtrlList = new List<CylinderCtrlTextInfo>();
			OtherItemsForLang.GetCylinderCtlItems(ManualF, ManualF.Name, ref cyCtrlList);
			foreach (CylinderCtrlTextInfo cyCtrl in cyCtrlList)
			{
				CylinderCtrlTextInfo AddComLan = new CylinderCtrlTextInfo();
				int Index = SysPara.cyCtrlLangIniDataList.FindIndex(ComLan => ComLan.Language == Lantype && ComLan.ComponentName == cyCtrl.ComponentName);
				if (Index >= 0)
				{
					AddComLan.FormName = FormName.Name;
					AddComLan.ComponentName = cyCtrlLangIniDataList[Index].ComponentName;
					AddComLan.CyOff_BtnText = cyCtrlLangIniDataList[Index].CyOff_BtnText;
					AddComLan.CyOn_BtnText = cyCtrlLangIniDataList[Index].CyOn_BtnText;
					CyCtrlLangList.Add(AddComLan);
				}
			}
		}

		/// <summary>
		/// 按当前语言加载报警表。文件名与 LanguageType 枚举名不同（西班牙语为 Spanish.xml），
		/// 此处显式映射；对应语言的文件缺失时使用 English.xml。
		/// </summary>
		private static void LoadAlarmTable()
		{
			LoadAlarmTable(SysPara.LanguageShow);
		}

		/// <summary>按指定语言加载报警表。</summary>
		private static void LoadAlarmTable(LanguageType lanType)
		{
			string temporary = null;
			try
			{
                if (!File.Exists(AlarmCatalog.FilePath(lanType, true))) return;
                temporary = Path.GetTempFileName();
                AlarmCatalog.RuntimeTable(lanType).Save(temporary);
				NPSDK.Alarm.Initial(temporary);
			}
			catch { }
			finally
			{
				try { if (temporary != null) File.Delete(temporary); }
				catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex); }
			}
		}

		/// <summary>
		/// 固定文案切换完成后触发，页面通过此事件刷新计数、状态和带业务数据的提示。
		/// </summary>
		public static event EventHandler LanguageChanged;

		#region 运行时控件文案：按名字从 LanguageData\*.xml 取
		// 按键从语言包取文字，供启动后才创建的控件使用：
		//     LanguageData\{Chinese|English|Español}.xml  →  /{语言}/{窗体名}/{键}/@ComponentText
		// 键为控件的语义名（如 vpBar_AddCamera）；XML 中没有该键时用代码中的底稿写入，之后以 XML 为准。

		private static XmlDocument[] _langDocs;

		/// <summary>
		/// 控件名带此后缀时不登记进语言表（文字含相机名 / 路径 / 状态等动态内容，由页面在语言变化事件中生成）。
		/// </summary>
		public const string DynTextSuffix = "_dyn";

		/// <summary>语言包文件路径：LanguageData\{语言}.xml（西语枚举是 Español，文件名也是 Español）。</summary>
		internal static string LangFilePath(LanguageType lang)
		{
			string dir = SysPara.LanguageDataDirectory;
			if (string.IsNullOrEmpty(dir)) dir = ".\\LanguageData";
			return Path.Combine(dir, lang.ToString() + ".xml");
		}

		/// <summary>取某语言的 XML（内存缓存；读不到就现建一个只有根节点的空文档）。</summary>
		private static XmlDocument LangDoc(LanguageType lang)
		{
			int i = (int)lang;
			if (_langDocs == null) _langDocs = new XmlDocument[Enum.GetNames(typeof(LanguageType)).Length];
			if (_langDocs[i] != null) return _langDocs[i];

			XmlDocument doc = new XmlDocument();
			try
			{
				doc.LoadXml(LanguageCatalog.ReadDocument(lang).ToString());
			}
			catch (Exception) { doc = new XmlDocument(); }
			if (doc.DocumentElement == null || doc.DocumentElement.Name != lang.ToString())
			{
				XmlDocument fresh = new XmlDocument();
				fresh.AppendChild(fresh.CreateElement(lang.ToString()));
				doc = fresh;
			}
			_langDocs[i] = doc;
			return doc;
		}

		/// <summary>
		/// 按键（控件名）取当前语言的文案（LanguageData\{当前语言}.xml），没有该键时返回 <paramref name="baseText"/>。
		/// 条目的写入由 <see cref="RegisterLanguage"/> 完成。
		/// </summary>
		public static string LangText(string formName, string key, string baseText)
		{
			string s = LangTextOf(SysPara.LanguageShow, formName, key);
			return string.IsNullOrEmpty(s) ? baseText : s;
		}

		/// <summary>取某语言下这个键的文案；XML 里没有（或为空）返回 null。</summary>
        private static string LangTextOf(LanguageType lang, string formName, string key)
        {
            lock (LanguageSync)
            {
                try
                {
                    if (string.IsNullOrEmpty(formName) || string.IsNullOrEmpty(key)) return null;

                    XmlNode node = LangDoc(lang).SelectSingleNode("/" + lang + "/" + formName + "/" + key);
                    if (node == null) return null;

                    string s = ((XmlElement)node).GetAttribute("ComponentText");
                    return string.IsNullOrEmpty(s) ? null : s;
                }
                catch (Exception) { return null; }
            }
        }

		/// <summary>
		/// 取消息类文案（弹窗 / 报警 / ToolTip）：三语底稿写入 LanguageData\{语言}.xml 的 /{语言}/{formName}/{key}
		/// （仅在该键不存在时写入），返回当前语言的文字。翻译可直接修改 XML 或在语言设置页修改。
		/// </summary>
		/// <param name="formName">语言包里的分组名（一般传窗体名）。</param>
		/// <param name="key">这条文案的键，如 "msg_SaveConfirm"。</param>
		/// <param name="zh">中文底稿。</param>
		/// <param name="en">英文底稿。</param>
		/// <param name="es">西语底稿；省略时回落英文。</param>
        public static string LangMsg(string formName, string key, string zh, string en, string es = null)
        {
            lock (LanguageSync)
            {
                string fallback = (SysPara.LanguageShow == LanguageType.Chinese) ? zh
                                : (SysPara.LanguageShow == LanguageType.English) ? en
                                : (string.IsNullOrEmpty(es) ? en : es);
                try
                {
                    SeedLangText(LanguageType.Chinese, formName, key, zh);
                    SeedLangText(LanguageType.English, formName, key, en);
                    SeedLangText(LanguageType.Español, formName, key, string.IsNullOrEmpty(es) ? en : es);

                    string s = LangTextOf(SysPara.LanguageShow, formName, key);
                    if (!string.IsNullOrEmpty(s)) return s;
                    if (SysPara.LanguageShow == LanguageType.Español)
                    {
                        s = LangTextOf(LanguageType.English, formName, key);    // 西语缺翻译时回落英文
                        if (!string.IsNullOrEmpty(s)) return s;
                    }
                }
                catch (Exception) { }
                return fallback;
            }
        }

		/// <summary>LangMsg 的补写一步：该语言还没有这个键时才写底稿，已有（可能已人工翻译）不动。</summary>
		private static void SeedLangText(LanguageType lang, string formName, string key, string text)
		{
			if (string.IsNullOrEmpty(text)) return;
			if (string.IsNullOrEmpty(LangTextOf(lang, formName, key)))
				SetLangText(lang, formName, key, text);
		}

		/// <summary>
		/// 把启动后创建的控件按控件名登记进语言表（文字来自 LanguageData\{语言}.xml 的 /{语言}/{窗体名}/{控件名}，
		/// XML 中没有的键用控件当前 Text 写入三份 XML）。登记后由 SwitchLanguage 更新文字。
		/// </summary>
		public static void RegisterLanguage(Control root, string formName)
		{
			if (root == null) return;
			RegisterLanguageControl(root, formName);
			RegisterLanguageParts(root, formName);
			if (root.GetType() == typeof(NPSDK.Flow_Chart)) return;
			if (root is TextBoxBase || root is FieldBox || root is DeviceControl || root is NP_Cylinder || root is ComboBox || root is DataGridView || root is PropertyGrid) return;
			for (int i = 0; i < root.Controls.Count; i++) RegisterLanguage(root.Controls[i], formName);
		}

		/// <summary>
		/// 把控件登记进语言表并立即按当前语言设置文字，用于按需创建的窗体（登录框 / 设置框 / 报警弹窗）。
		/// </summary>
		public static void RegisterAndApplyLanguage(Control root, string formName)
		{
			RegisterLanguage(root, formName);
			ApplyLanguageTexts(root, formName);
		}

		/// <summary>按当前语言从语言包刷新控件树中可登记控件的文字（逻辑与 SwitchLanguage 一致）。</summary>
		private static void ApplyLanguageTexts(Control root, string formName)
		{
			try
			{
				if (root == null) return;
				if (root.GetType() == typeof(NPSDK.Flow_Chart))
				{
					((NPSDK.Flow_Chart)root).text = LangText(formName, root.Name, ((NPSDK.Flow_Chart)root).text);
					return;
				}

				string key = root.Name;
				if (!string.IsNullOrEmpty(key) && !key.EndsWith(DynTextSuffix) && IsLanguageControl(root))
				{
					string t = LangTextOf(SysPara.LanguageShow, formName, key);
					if (t != null)
					{
						if (root.GetType() == typeof(NPSDK.Flow_Chart))
							((NPSDK.Flow_Chart)root).text = t;
						else
							root.Text = t;
					}
				}

				if (root is TextBoxBase || root is FieldBox || root is DeviceControl || root is NP_Cylinder || root is ComboBox || root is DataGridView || root is PropertyGrid) return;
				for (int i = 0; i < root.Controls.Count; i++) ApplyLanguageTexts(root.Controls[i], formName);
			}
			catch (Exception) { }
		}

		/// <summary>
		/// 判断控件的 Text 是否为界面文案：Form / Label / Button / CheckBox / RadioButton / TabPage / GroupBox /
		/// CheckedListBox / Flow_Chart 及其子类，以及 FlatButton / AlarmChip（二者直接继承 Control，需单独列出）。
		/// TextBox / DataGridView 等文字即数据的控件不计入。
		/// </summary>
		private static bool IsLanguageControl(Control c)
		{
			if (c == null) return false;
			if (c is ModuleBaseForm) return false;
			if (c.DataBindings.Count > 0) return false;
			if (c is TextBoxBase) return false;
			if (c is DataGridView) return false;
			if (c is ComboBox) return false;
			if (c is ListControl) return false;
			if (c is TabPage && (c.Tag is VpCameraConfig || c.Tag is VpVppConfig)) return false;
			if (c is TabPage && c.Tag is DeviceControl) return false;

			return c is Form
				|| c is Label
				|| c is Button
				|| c is CheckBox
				|| c is RadioButton
				|| c is TabPage
				|| c is GroupBox
				|| c is CheckedListBox
				|| c is FlatButton
				|| c is AlarmChip
				|| c is NP_Input
				|| c is NP_Output
				|| c is NP_Motor
				|| c.GetType() == typeof(NPSDK.Flow_Chart);
		}

		/// <summary>按控件身份登记三语固定文字，排除数据绑定、动态内容和模块身份。</summary>
		private static void RegisterLanguageControl(Control c, string formName)
		{
			try
			{
				// 启动期间先记录缺少的条目，控件引用由统一初始化重新登记。
				if (c == null) return;

				string key = c.Name;
				if (string.IsNullOrEmpty(key)) return;              // 没名字的控件不登记（XPath 会拼出空段）
				if (key.EndsWith(DynTextSuffix)) return;            // 文字带动态内容的控件由页面自己重算（见 VPForm）
				if (!IsLanguageControl(c)) return;                  // 输入框之类的"文字是数据"，不能进语言表
				string text = c.Text;
				if (string.IsNullOrEmpty(text)) return;             // 没文字的容器不登记

				string[] langs = Enum.GetNames(typeof(LanguageType));
				int listCount = (SysPara.ComponentLangurageList == null) ? 0 : SysPara.ComponentLangurageList.Length;
				for (int i = 0; i < langs.Length; i++)
				{
					string t = LangTextOf((LanguageType)i, formName, key);
					if (t == null) { SetLangText((LanguageType)i, formName, key, text); t = text; }
					if (i >= listCount) continue;
					List<ComponentTextInfo> list = SysPara.ComponentLangurageList[i];
					if (list == null) continue;

					// 同一个控件只留一条（拆页重建 / 重复登记时不至于越积越多）
					list.RemoveAll(delegate (ComponentTextInfo x) { return ReferenceEquals(x.Component, c); });

					ComponentTextInfo info = new ComponentTextInfo();
					info.FormName = formName;
					info.ComponentName = key;
					info.ComponentText = t;
					info.Component = c;
					list.Add(info);
				}
			}
			catch (Exception) { }
		}

		/// <summary>往某个语言的 XML 里写/更新一条文案（键不存在则新建节点）。</summary>
        private static void SetLangText(LanguageType lang, string formName, string key, string text)
        {
            lock (LanguageSync)
            {
                try
                {
                    XmlDocument doc = LangDoc(lang);
                    XmlNode node = doc.SelectSingleNode("/" + lang + "/" + formName + "/" + key);
                    XmlElement el = (node as XmlElement) ?? XMLExpand.GetElement(doc, lang + "/" + formName + "/" + key);
                    el.SetAttribute("ComponentText", text ?? "");
                    SaveLangDoc(doc, LangFilePath(lang));
                }
                catch (Exception) { }
            }
        }

		/// <summary>存盘（UTF-16，与 InitialLanguageData 一致）。必须先摘掉旧的 XML 声明，否则会写出两条。</summary>
		private static void SaveLangDoc(XmlDocument doc, string file)
		{
			try
			{
				for (int i = doc.ChildNodes.Count - 1; i >= 0; i--)
					if (doc.ChildNodes[i] is XmlDeclaration) doc.RemoveChild(doc.ChildNodes[i]);

				XmlDeclaration decl = doc.CreateXmlDeclaration("1.0", "unicode", "yes");
				XmlElement root = doc.DocumentElement;
				if (root != null) doc.InsertBefore(decl, root);

				string dir = Path.GetDirectoryName(file);
				if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
				doc.Save(file);
			}
			catch (Exception) { }
		}
		#endregion

        /// <summary>切换框架语言，依次刷新控件、报警和动态页面，并保留业务数据。</summary>
        public static void SwitchLanguage(LanguageType LanType)
        {
            if (!Enum.IsDefined(typeof(LanguageType), LanType)) return;
            SysPara.LanguageShow = LanType;
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(
                LanType == LanguageType.Chinese ? "zh-CN" : LanType == LanguageType.English ? "en-US" : "es-ES");
            LoadAlarmTable(LanType);
            if (SysPara.ComponentLangurageList != null)
            {
                foreach (List<ComponentTextInfo> list in SysPara.ComponentLangurageList)
                    if (list != null) list.RemoveAll(entry => entry.Component == null || entry.Component.IsDisposed);
                List<ComponentTextInfo> entries = SysPara.ComponentLangurageList[(int)LanType];
                foreach (ComponentTextInfo entry in entries.ToArray()) ApplyRegisteredControl(entry);
            }
            if (MainF != null)
            {
                MainF.NumC1.Image = LanType == LanguageType.Chinese ? MainF.NumC2.Image
                    : LanType == LanguageType.English ? MainF.NumC3.Image : MainF.NumC4.Image;
                MainF.NumC1.Text = LanType == LanguageType.Chinese ? "CN" : LanType == LanguageType.English ? "EN" : "ES";
            }
            foreach (Flow_BaseForm module in Flow_Module.FlowChart_ModuleList) module.Refresh();
            NotifyLanguageChanged();
        }

		public static void SwitchPermission(PermissionType Permission)
		{
			SysPara.UserPermission = Permission;
			MainF.SwitchPermission(Permission);

			MiddleLayer.DataF.UserLoginLog[0] = SysPara.UserName;
			MiddleLayer.DataF.UserLoginLog[1] = SysPara.UserPermission.ToString();
			MiddleLayer.DataF.SaveUserLoginLog(MiddleLayer.DataF.UserLoginLog);

			// 界面操作日志：记下本次登录 / 切换用户的工号与权限
			OperationLog.Write("UserLoginForm",
				(SysPara.UserName == "None" ? "注销" : "登录")
				+ "　工号：" + SysPara.UserName + "　权限：" + SysPara.UserPermission);
		}

#endregion
#region Load from xml file
#endregion
#region compare all component IOPort data and write to list
#endregion
#region Write to xml file
#endregion
		/// <summary>
		/// 动态创建对象
		/// </summary>
		public static dynamic CreateForm(dynamic FormAddress, string FormName)
		{
			dynamic dmic = (FormAddress == null) ? null : FormAddress;
			Assembly Assembly = Assembly.GetExecutingAssembly();
			foreach (Type ObjType in Assembly.GetTypes())
				if (ObjType.Name == FormName)
				{
					if (dmic == null)
						dmic = System.Activator.CreateInstance(ObjType, null);
					lstForm.Add(dmic);
					if (ObjType.IsSubclassOf(typeof(ModuleBaseForm)))
						dmic.ModuleInitialize(dmic.Text);
					break;
				}
			return dmic;
		}
		public static void show()
		{
		}

		public static void Initial()
		{
			if (NPSDK.Alarm.IsError)
			{
				OptionChoiceForm warning = new OptionChoiceForm();
				warning.fnChangeButtonsText(LangMsg("Common", "msg_OK", "确定", "OK", "Aceptar"), LangMsg("Common", "msg_OK", "确定", "OK", "Aceptar"));
				warning.fnSetMessageAndButtons(LangMsg("MiddleLayer", "msg_ResetAlarmFirst", "请先清除报警！", "Please Reset Alarm Firstly!", "¡Restablezca la alarma primero!"), false, true, false);
				warning.ShowDialog();

				return;
			}
			switch (SysPara.SystemMode)
			{
				case RunMode.IDLE:
					SysPara.UpConveyorInitialOk = false;
					SysPara.SystemMode = RunMode.INITIAL;
					NPSDK.Flow_Module.Module_StopRun();
					Thread.Sleep(500);
					NPSDK.Flow_Module.Module_InitialRun();
					break;
				case RunMode.INITIAL:
					SysPara.UpConveyorInitialOk = false;
					SysPara.SystemMode = RunMode.INITIAL;
					NPSDK.Flow_Module.Module_StopRun();
					Thread.Sleep(500);
					NPSDK.Flow_Module.Module_InitialRun();
					break;
				case RunMode.RUN:
					DialogResult result = MessageBox.Show(LangMsg("MiddleLayer", "msg_Reinitialize", "正在自动生产，确认退出自动生产并执行初始化吗？", "Exit automatic production and initialize?", "¿Salir de producción automática e inicializar?"), LangMsg("MiddleLayer", "msg_InitializeTitle", "初始化", "Initialize", "Inicializar"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
					if (result == System.Windows.Forms.DialogResult.Cancel)
						break;
					SysPara.UpConveyorInitialOk = false;
					SysPara.SystemMode = RunMode.INITIAL;
					NPSDK.Flow_Module.Module_StopRun();
					Thread.Sleep(500);
					NPSDK.Flow_Module.Module_InitialRun();
					break;
				case RunMode.PAUSE:
					NPSDK.Flow_Module.Module_InitialRun();
					NPSDK.Alarm.Clear();
					SysPara.SystemMode = RunMode.INITIAL;
					return;
			}
			SysPara.SystemMode = RunMode.INITIAL;
			NPSDK.Flow_Module.Module_InitialRun();
		}

		public static void StartRun()
		{
			string visionError;
			if (VPF != null && !VPF.CanStartProduction(out visionError))
			{
				MessageBox.Show(visionError, LangMsg("Vision", "msg_NotReady", "视觉尚未就绪", "Vision is not ready", "Visión no está lista"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			AlarmClear();
			Thread.Sleep(200);
			if (NPSDK.Alarm.IsError)
			{
				OptionChoiceForm warning = new OptionChoiceForm();
				warning.fnChangeButtonsText(LangMsg("Common", "msg_OK", "确定", "OK", "Aceptar"), LangMsg("Common", "msg_OK", "确定", "OK", "Aceptar"));
				warning.fnSetMessageAndButtons(LangMsg("MiddleLayer", "msg_ResetAlarmFirst", "请先清除报警！", "Please Reset Alarm Firstly!", "¡Restablezca la alarma primero!"), false, true, false);
				warning.ShowDialog();
				return;
			}

			if (SysPara.UpConveyorInitialOk)
			{
				if (!SysPara.SystemRun)
				{
					if (SysPara.SystemMode == RunMode.INITIAL)
					{
						SysPara.SystemMode = RunMode.RUN;

						AlarmClear();

						NPSDK.Flow_Module.Module_StopRun();
						Thread.Sleep(500);
						NPSDK.Flow_Module.Module_StartRun();

						SysPara.SystemRun = true;
					}
				}
				else
				{
					if (SysPara.SystemMode == RunMode.PAUSE)
					{
						AlarmClear();
						MiddleLayer.SetHightSpeed();
						SysPara.SystemMode = RunMode.RUN;

						NPSDK.Flow_Module.Module_StartRun();
						StartRunCV();
					}
				}
			}
			else
			{
				MessageBox.Show(LangMsg("MiddleLayer", "msg_InitializeFirst", "请先初始化。", "Initialize the machine first.", "Inicialice primero la máquina."), LangMsg("Common", "msg_WarningTitle", "警告", "Warning", "Aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
			}
		}

		public static void PauseRun()
		{
			bool stopRuncv = false;
			if (SysPara.SystemMode == RunMode.RUN || (SysPara.SystemMode == RunMode.INITIAL && !SysPara.UpConveyorInitialOk))
			{
				stopRuncv = true;
				SysPara.SystemMode = RunMode.PAUSE;
			}
			NPSDK.Flow_Module.Module_PauseRun();
			StopManualRun();
			StopAllMotor();
			if (stopRuncv)
			{
				StopRunCV();
			}
		}

		/// <summary>
		/// 自动化运行停止
		/// </summary>
		public static void StopRun()
		{
			if (SysPara.SystemMode != RunMode.IDLE)
			{
				SysPara.SystemRun = false;
				SysPara.UpConveyorInitialOk = false;
				SysPara.SystemMode = RunMode.IDLE;
				NPSDK.Flow_Module.Module_StopRun();
			}
			NPSDK.Flow_Module.Module_StopRun();
			StopManualRun();
			StopAllMotor();
		}

		public static void StopManualRun()
		{
			foreach (ModuleBaseForm Module in ModuleManager.ModuleList)
				Module.StopManualTask.Cancel();
			StopAllMotor();
		}

		public static void StopAllMotor()
		{
			foreach (ControlBaseInterface control in SDKPara.ControlList)
			{
				if (control is NP_Motor)
				{
					((NP_Motor)control).Stop();
				}
			}
		}

		public static void AlarmClear()
		{
			foreach (ControlBaseInterface control in SDKPara.ControlList)
				if (control is NP_Motor)
				{
					if (((NP_Motor)control).GetAxisIOState().ALM)
					{
						((NP_Motor)control).AlarmReset();
					}
				}

			try
			{
				if (SysPara.SystemRun)
				{
					for (int i = 0; i < NPSDK.Alarm.AlarmList.Count; i++)
					{
					}
				}
			}
			catch
			{
			}

			AlarmList.Clear();
			NPSDK.Alarm.Clear();
		}

		public static void SetHightSpeed()
		{
		}

		public static void SetLowSpeed()
		{
		}

		/// <summary>
		/// 加载物料数据
		/// </summary>
		public static bool OpenRecipe(string RecipePath)
		{
			if (SysPara.SystemMode != RunMode.IDLE)
			{
				MessageBox.Show(LangMsg("MiddleLayer", "msg_RunningRecipe", "设备运行中，不能切换配方。", "Cannot change recipes while the machine is running.", "No se puede cambiar de receta mientras la máquina está funcionando."), LangMsg("MiddleLayer", "msg_RecipeTitle", "切换配方", "Change recipe", "Cambiar receta"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}
			foreach (VisionproInterface station in VisionproInterface.VList)
			{
				if (!station.IsBusy) continue;
				MessageBox.Show(LangMsg("MiddleLayer", "msg_VisionBusy", "请先结束视觉运行或编辑：", "Finish the vision operation or editing first: ", "Finalice primero la operación o edición de visión: ") + station.StationName, LangMsg("MiddleLayer", "msg_RecipeTitle", "切换配方", "Change recipe", "Cambiar receta"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}
			bool bOpenSuccess = false;
			string oldRecipeName = SysPara.RecipeName;
			try
			{
				SysPara.RecipeDataDirectory = Path.GetDirectoryName(RecipePath);
				SysPara.RecipeName = Path.GetFileNameWithoutExtension(RecipePath);

				SysPara.FilePath = SysPara.RecipeDataDirectory + "\\" + SysPara.RecipeName + ".xml";
				for (int i = 0; i < ModuleManager.ModuleList.Count; i++)
					ModuleManager.ModuleList[i].ReadRecipeData(SysPara.FilePath);

				HardF.ReadRecipeData(SysPara.FilePath);

				// 配方换了：托盘卡是按 PSet 的列生成的，列可能多也可能少，重扫一遍
				try { if (PlatF != null) PlatF.RebuildTrayTable(); } catch { }

				bOpenSuccess = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, MiddleLayer.LangMsg("Common", "msg_SystemTitle", "系统提示", "System message", "Mensaje del sistema"), MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
				SysPara.RecipeDataDirectory = ".\\ModuleData\\RecipeData";
				SysPara.RecipeName = "Recipe";
				bOpenSuccess = false;
			}

			OpenVision();
			IniFile IniFile = new IniFile(".\\MachineSetup.ini");
			IniFile.WriteString("MachineSetup", "RecipeName", SysPara.RecipeName);
			IniFile.WriteString("PathSetup", "RecipeDirectory", SysPara.RecipeDataDirectory.Replace(System.IO.Directory.GetCurrentDirectory() + "\\", ".\\"));

			// 界面操作日志：只有真的换了配方才记（启动时也会调本函数）
			if (bOpenSuccess && oldRecipeName != SysPara.RecipeName)
				OperationLog.Write("ProductManagerForm", "切换配方：" + oldRecipeName + " → " + SysPara.RecipeName);

			return bOpenSuccess;
		}

		public static void OpenVision()
		{
			for (int i = 0; i < VisionproInterface.VList.Count; i++)
			{
				string path = VisionproInterface.VList[i].GetVppPath(SysPara.RecipeName);
				FileInfo f = new FileInfo(path);
				VisionproInterface.VList[i].LoadTB(f.FullName);
			}
			if (VPF != null) VPF.Getdgv_H1_VisionDataList();

			// 刷新视觉页面上显示的 vpp 路径（VPForm 尚未创建时为 null）
			if (VPF != null) VPF.OnRecipeChanged();
		}

		public static void AlwaysRun()
		{
			if (NPSDK.Alarm.DoStop)
			{
				if (SysPara.SystemRun)
					LogManagement.Instance.SaveLog(LogManagement.LogType.MachineStatus, "Alarm stop");
				NPSDK.Alarm.DoStop = false;
			}

			foreach (ModuleBaseForm Module in ModuleManager.ModuleList)
			{
				try
				{
					Module.AlwaysRun();
				}
				catch (Exception)
				{
					NPSDK.Alarm.Show("2010", "An unpredictable error occurred on AlwaysRun() ! ModuleName=\"" + Module.Name + "\"");
				}
			}
		}

		public static void CheckMotorProtected()
		{
			if (SysPara.Simulation)
				return;

			AxisIOState IOState;
			foreach (ControlBaseInterface control in SDKPara.ControlList)
			{
				if (control is NP_Motor)
				{
					control.Refresh();

					IOState = ((NP_Motor)control).GetAxisIOState();

					if (IOState.ALM)
						NPSDK.Alarm.Show("2030", $"Motor {control.Name} alarmado");

					if (IOState.EMG)
						NPSDK.Alarm.Show("2031", $"Motor {control.Name} con señal EMG activa");

					if ((IOState.SLN || IOState.SLP || IOState.MEL || IOState.PEL))
						NPSDK.Alarm.Show("2032", $"Motor {control.Name} llegó al sensor de límite");

					if (!IOState.SVON && SysPara.UpConveyorInitialOk && control.Name != "MTR_Z")
					{
					}
				}
			}
		}

		public static void InitialParameterReset()
		{
			foreach (ModuleBaseForm Module in ModuleManager.ModuleList)
			{
				try
				{
					Module.InitialParameterReset();
				}
				catch (Exception)
				{
					NPSDK.Alarm.Show("2013", LangMsg("MiddleLayer", "msg_ExecResetError", "未知错误在 ExecuteReset() ! 名称=\"", "An unpredictable error occurred on ExecuteReset() ! ModuleName=\"", "Error desconocido en ExecuteReset() ! Módulo=\"") + Module.Name + "\"");
					StopRun();
				}
			}
		}

		public static void InitialReset()
		{
			foreach (ModuleBaseForm Module in ModuleManager.ModuleList)
			{
				try
				{
					Module.InitialReset();
				}
				catch (Exception)
				{
					NPSDK.Alarm.Show("2014", LangMsg("MiddleLayer", "msg_InitialResetError", "未知错误在 InitialReset() ! 名称=\"", "An unpredictable error occurred on InitialReset() ! ModuleName=\"", "Error desconocido en InitialReset() ! Módulo=\"") + Module.Name + "\"");
					StopRun();
				}
			}
		}

		public static bool GetInitialOk()
		{
			bool bInitialOk = true;
			foreach (ModuleBaseForm Module in ModuleManager.ModuleList)
				bInitialOk &= Module.GetInitialOk();
			return bInitialOk;
		}

		public static void RunReset()
		{
			foreach (ModuleBaseForm Module in ModuleManager.ModuleList)
			{
				try
				{
					Module.RunReset();
				}
				catch (Exception)
				{
					NPSDK.Alarm.Show("2016", "An unpredictable error occurred on RunReset() ! ModuleName=\"" + Module.Name + "\"");
					StopRun();
				}
			}
		}

		public static void Run()
		{
			foreach (ModuleBaseForm Module in ModuleManager.ModuleList)
			{
				try
				{
					Module.Run();
				}
				catch (Exception)
				{
					NPSDK.Alarm.Show("2017", "An unpredictable error occurred on Run() ! ModuleName=\"" + Module.Name + "\"");
					StopRun();
				}
			}
		}
	}
}
