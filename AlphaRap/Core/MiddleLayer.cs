
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

namespace AlphaRap
{
	class MiddleLayer
	{
		//===================Module Form===================

		public static LifeSpan.LifeSpanForm SpanLifeF;
		public static Flow FlowF;
		public static Epson.Form1 EpsonF;
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

		public static Robot RobotF;
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
		public static bool RobotConnectStatus = false;
		public static bool gEXIT = false;
		public static AtlasLibrary.MTF6000 Atlas_MTF6000 = new AtlasLibrary.MTF6000();

		public static List<string> AlarmList = new List<string>();

		public static void InitialProject3()
		{
			#region Load Ini File
			LoadingF.SetCaption("Load Ini File");

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
			SysPara.LanguageDataDirectory = iniFile.ReadString("PathSetup", "LanguageDirectory", ".\\LanguageData");

			ModuleManager.SettingDataDirectory = SysPara.SettingDataDirectory;
			EnsureDataDirectories();
			#endregion

			#region Load Alarm Table
			LoadingF.SetCaption("Load Alarm Table");
			LoadAlarmTable();
			#endregion

			#region Creat Module
			LoadingF.SetCaption("Load Form");
			//=======================Module Create===========================
			SystemF = CreateForm(SystemF, "SystemForm");

			MainF = CreateForm(MainF, "MainForm");

			#endregion

			InitialLanguageData();
			if (SysPara.LanguageName == "English")
				SysPara.LanguageShow = LanguageType.English;
			else if (SysPara.LanguageName == "Chinese")
				SysPara.LanguageShow = LanguageType.Chinese;
			else
				SysPara.LanguageShow = LanguageType.Español;

			SwitchLanguage(SysPara.LanguageShow);

			SwitchPermission(SysPara.UserPermission);

			OpenRecipe(string.Format("{0}\\{1}.xml", SysPara.RecipeDataDirectory, SysPara.RecipeName));
			ServoOn();
		}
		/// <summary>
		/// 创建运行所需的数据目录（新克隆的工程没有这些目录，缺失时各页面读取会抛 DirectoryNotFoundException）。
		/// 只建空目录，不生成任何配方或设置文件。
		/// </summary>
		private static void EnsureDataDirectories()
		{
			string[] dirs =
			{
				SysPara.LogFilePath, SysPara.VisionFileDirectory, SysPara.AlarmTableDirectory,
				SysPara.SettingDataDirectory, SysPara.RecipeDataDirectory, SysPara.MESDirectory,
				SysPara.IOPortDirectory, SysPara.LanguageDataDirectory
			};
			foreach (string d in dirs)
			{
				if (string.IsNullOrEmpty(d)) continue;
				try { Directory.CreateDirectory(ResolveAppPath(d)); }
				catch { }
			}
		}

		/// <summary>相对路径（如 .\\ModuleData）按程序所在目录解析；绝对路径原样返回。</summary>
		public static string ResolveAppPath(string path)
		{
			if (string.IsNullOrEmpty(path) || Path.IsPathRooted(path)) return path;
			string rel = path.StartsWith(".\\") || path.StartsWith("./") ? path.Substring(2) : path;
			return Path.Combine(Application.StartupPath, rel);
		}

		public static FlowControl FlowCtrl;
		/// <summary>
		/// 初始化项目
		/// </summary>
		public static void InitialProject()
		{
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
			SysPara.LanguageDataDirectory = iniFile.ReadString("PathSetup", "LanguageDirectory", ".\\LanguageData");

			SysPara.LanguageDataDirectory = iniFile.ReadString("PathSetup", "LanguageDirectory", ".\\LanguageData");
			SysPara.LanguageName = iniFile.ReadString("MachineSetup", "LanguageName", "");

			ModuleManager.SettingDataDirectory = SysPara.SettingDataDirectory;
			SysPara.FilePath = SysPara.RecipeDataDirectory + "\\" + SysPara.RecipeName + ".xml";
			EnsureDataDirectories();
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
			LoadingF.SetCaption("Load ProductManagerForm");
			ProductF = CreateForm(ProductF, "ProductManagerForm");
			PlatF = CreateForm(PlatF, "PlatFormSetting");
			SystemS = CreateForm(SystemS, "SystemSetting");
			ParF = CreateForm(ParF, "Parameter");
			SystemF = CreateForm(SystemF, "SystemForm");
			LoadingF.SetCaption("Load MainForm");
			MainF = CreateForm(MainF, "MainForm");
			LoadingF.SetCaption("Load HomeForm");
			HomeF = CreateForm(HomeF, "HomeForm");
			LoadingF.SetCaption("Load ManualForm");
			ManualF = CreateForm(ManualF, "ManualForm");
			HardF = CreateForm(HardF, "HardForm");
			CheckF = CreateForm(CheckF, "CheckForm");
			LogF = CreateForm(LogF, "LogForm");

			AddF = CreateForm(AddF, "AddUserForm");
			// 未安装 VisionPro 时不创建视觉页（VPF 为 null，视觉菜单给出提示）
			if (VisionRuntime.Installed) VPF = CreateForm(VPF, "VPForm");
			FlowF = new Flow();

			RobotF = CreateForm(RobotF, "Robot");
			DataF = CreateForm(DataF, "DataForm");
			LockForm1 = new LockForm1();
			MesF = CreateForm(MesF, "LENS");
			//===============================================================

			LoadingF.SetCaption("Load InitialLanguageData");
			InitialLanguageData();
			if (SysPara.LanguageName == "English")
				SysPara.LanguageShow = LanguageType.English;
			else if (SysPara.LanguageName == "Chinese")
				SysPara.LanguageShow = LanguageType.Chinese;
			else
				SysPara.LanguageShow = LanguageType.Español;

			SwitchLanguage(SysPara.LanguageShow);

			LoadingF.SetCaption("SwitchPermission");
			SwitchPermission(SysPara.UserPermission);
			LoadingF.SetCaption("Load IO");

			SDKKernal.SetSimulation(SysPara.Simulation);
			SDKKernal.InitializeComponent();

			alTask = new AlwaysRunTask();
			alarmRunTask = new AlarmRunTask();

			LoadingF.SetCaption("OpenRecipe");
			OpenRecipe(string.Format("{0}\\{1}.xml", SysPara.RecipeDataDirectory, SysPara.RecipeName));

			FlowCtrl = new FlowControl();
			FlowCtrl.StartThread();

			ServoOn();
			LoadingF.SetCaption("Load Vision");
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
			foreach (KeyValuePair<Adlink_Output, int> item in FlowLineStatus)
			{
				if (item.Value == 0)
				{
					item.Key.On();
				}
			}
		}
		static Dictionary<Adlink_Output, int> FlowLineStatus = new Dictionary<Adlink_Output, int>();
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

		static int ReturnOutPutStaus(Adlink_Output output)
		{
			if (output.OutputStaus)
			{
				return 0;
			}
			return -1;
		}
		#endregion
		private static void InitialLanguageData()
		{
			string[] LanguageArray = Enum.GetNames(typeof(LanguageType));
			SysPara.ComponentLangurageList = new List<ComponentTextInfo>[LanguageArray.Length];

			SysPara.TStripLangurageList = new List<TStripItemTextInfo>[LanguageArray.Length]; //for ToolStrip
			SysPara.CyCtrlLangList = new List<CylinderCtrlTextInfo>[LanguageArray.Length];//for CylinderControl
			for (int i = 0; i < LanguageArray.Length; i++)
			{
				SysPara.ComponentLangurageList[i] = new List<ComponentTextInfo>();
				List<ComponentTextInfo> XmlDataList = new List<ComponentTextInfo>();
				SysPara.TStripLangurageList[i] = new List<TStripItemTextInfo>();  //for MainForm ToolStrip
				List<TStripItemTextInfo> XmlDataList2 = new List<TStripItemTextInfo>(); // for MainForm ToolStrip
				SysPara.CyCtrlLangList[i] = new List<CylinderCtrlTextInfo>();// CylinderCotrol
				#region Load from xml file
				string sFilePath = string.Format("{0}\\{1}.xml", SysPara.LanguageDataDirectory, LanguageArray[i]);
				if (File.Exists(sFilePath))
				{
					XmlDocument ReadDoc = new XmlDocument();
					ReadDoc.Load(sFilePath);
					XmlElement Element = (XmlElement)ReadDoc.SelectSingleNode(LanguageArray[i]);
					if (Element != null)
					{
						XmlNodeList FormData = Element.ChildNodes;
						for (int j = 0; j < FormData.Count; j++)
						{
							#region MainForm ToolStrip，提取到XmlDataList2
							if (FormData[j].Name is "MainForm")     //MainForm ToolStrip，提取到XmlDataList2 
							{
								XmlNodeList ComponentData2 = FormData[j].ChildNodes;
								for (int k = 0; k < ComponentData2.Count; k++)
								{
									if (ComponentData2[k].Name.Contains("ToolStrip"))
									{
										TStripItemTextInfo CmpTextInfo2 = new TStripItemTextInfo();
										CmpTextInfo2.FormName = FormData[j].Name;
										CmpTextInfo2.ComponentName = ComponentData2[k].Name;
										CmpTextInfo2.ComponentText = ((XmlElement)ComponentData2[k]).GetAttribute("ComponentText");
										XmlDataList2.Add(CmpTextInfo2);
									}
								}
							}

							#endregion
							XmlNodeList ComponentData = FormData[j].ChildNodes;
							for (int k = 0; k < ComponentData.Count; k++)
							{
								ComponentTextInfo CmpTextInfo = new ComponentTextInfo();
								CmpTextInfo.FormName = FormData[j].Name;
								CmpTextInfo.ComponentName = ComponentData[k].Name;
								CmpTextInfo.ComponentText = ((XmlElement)ComponentData[k]).GetAttribute("ComponentText");
								XmlDataList.Add(CmpTextInfo);
							}
						}
					}
				}
				#endregion

				#region compare all component text data and write to list
				//AllForm
				foreach (Control Form in lstForm)
				{
					InitialLanguageCallback(Form, Form.Name, ref SysPara.ComponentLangurageList[i], ref XmlDataList);
				}
				//MainForm

				InitialLanguageCallbackMForm(MainF, MainF.Name, ref SysPara.TStripLangurageList[i], ref XmlDataList2); //MainForm ToolStrip
				#endregion

				#region Write to xml file
				// 在已有语言文件的基础上更新，保留运行时按键登记的条目（见 LangText）
				XmlDocument WriteDoc = new XmlDocument();
				try
				{
					if (File.Exists(sFilePath)) WriteDoc.Load(sFilePath);
				}
				catch (Exception) { WriteDoc = new XmlDocument(); }

				// 摘掉旧声明：WriteUnicodeXML 会重新插一条，留着就会写出两条 <?xml ?>
				for (int k = WriteDoc.ChildNodes.Count - 1; k >= 0; k--)
					if (WriteDoc.ChildNodes[k] is XmlDeclaration) WriteDoc.RemoveChild(WriteDoc.ChildNodes[k]);

				XMLExpand.GetElement(WriteDoc, LanguageArray[i]);
				for (int j = 0; j < SysPara.ComponentLangurageList[i].Count; j++)
				{
					XmlElement eSetting = XMLExpand.GetElement(WriteDoc, LanguageArray[i] + "/" + SysPara.ComponentLangurageList[i][j].FormName + "/" + SysPara.ComponentLangurageList[i][j].ComponentName);
					eSetting.SetAttribute("ComponentText", SysPara.ComponentLangurageList[i][j].ComponentText);
				}
				for (int j = 0; j < SysPara.TStripLangurageList[i].Count; j++)   //MainForm ToolStrip
				{
					XmlElement eSetting = XMLExpand.GetElement(WriteDoc, LanguageArray[i] + "/" + SysPara.TStripLangurageList[i][j].FormName + "/" + SysPara.TStripLangurageList[i][j].ComponentName);
					eSetting.SetAttribute("ComponentText", SysPara.TStripLangurageList[i][j].ComponentText);
				}

				if (!Directory.Exists(SysPara.LanguageDataDirectory))
					Directory.CreateDirectory(SysPara.LanguageDataDirectory);
				XMLExpand.WriteUnicodeXML(WriteDoc, sFilePath);
				#endregion

				#region write initial data for Cylinder control
				//write the initial data to CyCtrlLangList[i] from cyCtrlLangIniDataList;

				OtherItemsForLang.GetCyCtrlLanIntitialData(ManualF, ref SysPara.cyCtrlLangIniDataList);
				LanguageType LanTypeTemp = (LanguageType)i;
				InitialLangCallbackCyldCtrl(ManualF, LanTypeTemp, ref SysPara.cyCtrlLangIniDataList, ref SysPara.CyCtrlLangList[i]);
				#endregion
			}

			// VPForm 在语言表建立之前已构建，且控件为 UiLabel/UiButton 子类（白名单扫描不到），在此登记
			if (VPF != null) VPF.RegisterVpLanguage();
			// LockForm1 同样是启动期直接 new 的（不经 CreateForm，不在 lstForm，白名单扫不到），一并补登记。
			if (LockForm1 != null) RegisterLanguage(LockForm1, "LockForm1");
			// ProductManagerForm 的控件为 UiLabel / FlatButton 等自绘子类（白名单扫描不到），在此登记
			if (ProductF != null) RegisterLanguage(ProductF, "ProductManagerForm");
			// AddUserForm 同理（自绘控件 + 建得比语言表早）。
			if (AddF != null) RegisterLanguage(AddF, "AddUserForm");
		}

		private static void InitialLanguageCallback(Control cl, string FormName, ref List<ComponentTextInfo> ComponentLangurageList, ref List<ComponentTextInfo> XmlDataList)
		{
			foreach (Control control in cl.Controls)
			{
				Type ControlType = control.GetType();
				bool bNeedAdded = false;
				bNeedAdded |= (ControlType == typeof(Form));
				bNeedAdded |= (ControlType == typeof(Label));
				bNeedAdded |= (ControlType == typeof(GroupBox));
				bNeedAdded |= (ControlType == typeof(CheckBox));
				bNeedAdded |= (ControlType == typeof(CheckedListBox));
				bNeedAdded |= (ControlType == typeof(Button));
				bNeedAdded |= (ControlType == typeof(TabPage));
				bNeedAdded |= (ControlType == typeof(RadioButton));
				bNeedAdded |= (ControlType == typeof(Adlink_Motor));
				bNeedAdded |= (ControlType == typeof(Adlink_Input));
				bNeedAdded |= (ControlType == typeof(Adlink_Output));
				bNeedAdded |= (ControlType == typeof(Flow_Chart));

				bNeedAdded |= (ControlType == typeof(NPSDK.Flow_Chart));
				bool bCylinderControl = false;
				bCylinderControl |= (control.Name == "btnOn");
				bCylinderControl |= (control.Name == "btnOff");
				if (bNeedAdded && !bCylinderControl)
				{
					ComponentTextInfo AddComLan = new ComponentTextInfo();
					if (ControlType == typeof(NPSDK.Flow_Chart))
					{
						int Index = XmlDataList.FindIndex(ComLan => (ComLan.FormName == FormName && ComLan.ComponentName == ((NPSDK.Flow_Chart)control).Name));
						if (Index >= 0)
						{
							AddComLan.FormName = FormName;
							AddComLan.ComponentName = XmlDataList[Index].ComponentName;
							AddComLan.ComponentText = XmlDataList[Index].ComponentText;
							AddComLan.Component = ((NPSDK.Flow_Chart)control);
						}
						else
						{
							AddComLan.FormName = FormName;
							AddComLan.ComponentName = ((NPSDK.Flow_Chart)control).Name;
							AddComLan.ComponentText = ((NPSDK.Flow_Chart)control).text;
							AddComLan.Component = ((NPSDK.Flow_Chart)control);
						}
					}
					else
					{
						int Index = XmlDataList.FindIndex(ComLan => (ComLan.FormName == FormName && ComLan.ComponentName == control.Name));
						if (Index >= 0)
						{
							AddComLan.FormName = FormName;
							AddComLan.ComponentName = XmlDataList[Index].ComponentName;
							AddComLan.ComponentText = XmlDataList[Index].ComponentText;
							AddComLan.Component = control;
						}
						else
						{
							AddComLan.FormName = FormName;
							AddComLan.ComponentName = control.Name;
							AddComLan.ComponentText = control.Text;
							AddComLan.Component = control;
						}
					}
					ComponentLangurageList.Add(AddComLan);
				}
				if (control.HasChildren && ControlType != typeof(NPSDK.Flow_Chart))
					InitialLanguageCallback(control, FormName, ref ComponentLangurageList, ref XmlDataList);
			}
		}

		private static void InitialLanguageCallbackMForm(Form Form, string FormName, ref List<TStripItemTextInfo> TStripComponentLangurageList, ref List<TStripItemTextInfo> XmlDataList2)
		{
			List<TStripItemTextInfo> TStripItemList = new List<TStripItemTextInfo>();
			if (FormName is "MainForm")
			{
				OtherItemsForLang.GetToolStripItems(Form, ref TStripItemList);     //取Form的ToolStrip
			}
			foreach (TStripItemTextInfo TStripItemTxtInfo in TStripItemList)
			{
				TStripItemTextInfo AddComLan = new TStripItemTextInfo();
				int Index = XmlDataList2.FindIndex(ComLan => (ComLan.FormName == FormName && ComLan.ComponentName == TStripItemTxtInfo.ComponentName));
				if (Index >= 0)
				{
					AddComLan.FormName = FormName;
					AddComLan.ComponentName = XmlDataList2[Index].ComponentName;
					AddComLan.ComponentText = XmlDataList2[Index].ComponentText;
					AddComLan.Component = TStripItemTxtInfo.Component;
				}
				else
				{
					AddComLan.FormName = FormName;
					AddComLan.ComponentName = TStripItemTxtInfo.ComponentName;
					AddComLan.ComponentText = TStripItemTxtInfo.ComponentText;
					AddComLan.Component = TStripItemTxtInfo.Component;
				}
				TStripComponentLangurageList.Add(AddComLan);
			}
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
			try
			{
				string dir = SysPara.AlarmTableDirectory;
				if (string.IsNullOrEmpty(dir)) dir = ".\\AlarmTable";

				string langName;
				switch (lanType)
				{
					case LanguageType.Chinese: langName = "Chinese"; break;
					case LanguageType.English: langName = "English"; break;
					case LanguageType.Español: langName = "Spanish"; break;   // 枚举名 Español，文件名却是 Spanish
					default: langName = lanType.ToString(); break;
				}

				string file = Path.Combine(dir, langName + ".xml");
				if (!File.Exists(file))                                  // 兼容另一种命名
					file = Path.Combine(dir, lanType.ToString() + ".xml");
				if (!File.Exists(file))                                  // 兜底：宁可显示英文报警，也不能没有报警
					file = Path.Combine(dir, "English.xml");

				NPSDK.Alarm.Initial(file);
			}
			catch { }
		}

		/// <summary>
		/// 界面语言切换完成后触发。SwitchLanguage 只更新语言表中登记的控件，
		/// 自行管理文字的页面（FieldBox / FlatButton / UiLabel 等）通过此事件刷新文字。
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
		private static string LangFilePath(LanguageType lang)
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
			string file = LangFilePath(lang);
			try
			{
				if (File.Exists(file)) doc.Load(file);
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
			if (c is TextBoxBase) return false;
			if (c is DataGridView) return false;
			if (c is ComboBox) return false;
			if (c is ListControl) return false;

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
				|| c.GetType() == typeof(NPSDK.Flow_Chart);
		}

		private static void RegisterLanguageControl(Control c, string formName)
		{
			try
			{
				// 语言表（ComponentLangurageList）可能尚未创建：此时只写入 XML 条目，
				// 内存登记由 InitialLanguageData 末尾的 VPF.RegisterVpLanguage() 完成
				if (c == null) return;

				string key = c.Name;
				if (string.IsNullOrEmpty(key)) return;              // 没名字的控件不登记（XPath 会拼出空段）
				if (key.EndsWith(DynTextSuffix)) return;            // 文字带动态内容的控件由页面自己重算（见 VPForm）
				if (!IsLanguageControl(c)) return;                  // 输入框之类的"文字是数据"，不能进语言表
				string text = c.Text;
				if (string.IsNullOrEmpty(text)) return;             // 没文字的容器不登记

				string[] langs = Enum.GetNames(typeof(LanguageType));
				int listCount = (SysPara.ComponentLangurageList == null) ? 0 : SysPara.ComponentLangurageList.Length;
				for (int i = 0; i < langs.Length && i < listCount; i++)
				{
					List<ComponentTextInfo> list = SysPara.ComponentLangurageList[i];
					if (list == null) continue;

					// 同一个控件只留一条（拆页重建 / 重复登记时不至于越积越多）
					list.RemoveAll(delegate (ComponentTextInfo x) { return ReferenceEquals(x.Component, c); });

					// XML 中没有该键时，用控件当前 Text 写入
					string t = LangTextOf((LanguageType)i, formName, key);
					if (t == null) { SetLangText((LanguageType)i, formName, key, text); t = text; }

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

		public static void SwitchLanguage(LanguageType LanType)
		{
			// 先切换报警表：新报警在 Show(编号) 时按当前表解析（已显示的报警由 AlarmRunTask 经 HomeF.ResolveAlarmContent 重新解析）
			LoadAlarmTable(LanType);

			// 移除已释放控件的登记项（运行时创建的控件会随页面重建而释放）
			if (SysPara.ComponentLangurageList != null)
			{
				for (int i = 0; i < SysPara.ComponentLangurageList.Length; i++)
				{
					List<ComponentTextInfo> lst = SysPara.ComponentLangurageList[i];
					if (lst == null) continue;
					lst.RemoveAll(delegate (ComponentTextInfo x) { return x.Component == null || x.Component.IsDisposed; });
				}
			}

			// 单个控件换字失败不该连累后面（尤其是报警表/事件通知），逐条兜住
			try
			{
				for (int i = 0; i < SysPara.ComponentLangurageList[(int)LanType].Count; i++)
				{
					if (SysPara.ComponentLangurageList[(int)LanType][i].Component.GetType() == typeof(NPSDK.Flow_Chart))
					{
						((NPSDK.Flow_Chart)SysPara.ComponentLangurageList[(int)LanType][i].Component).text = SysPara.ComponentLangurageList[(int)LanType][i].ComponentText;
					}
					else
					{
						SysPara.ComponentLangurageList[(int)LanType][i].Component.Text = SysPara.ComponentLangurageList[(int)LanType][i].ComponentText;
					}
				}
			}
			catch (Exception ex) { System.Diagnostics.Debug.WriteLine("[SwitchLanguage] 控件换字异常: " + ex); }

			try
			{
				for (int i = 0; i < SysPara.TStripLangurageList[(int)LanType].Count; i++)
				{
					SysPara.TStripLangurageList[(int)LanType][i].Component.Text = SysPara.TStripLangurageList[(int)LanType][i].ComponentText;
				}
			}
			catch (Exception ex) { System.Diagnostics.Debug.WriteLine("[SwitchLanguage] ToolStrip换字异常: " + ex); }

			try
			{
				string[] a = SysPara.FilePath.Split('\\');
				string[] b = a[a.Length - 1].Split('.');
				ProductF.CurrentModel.Text = b[0];
			}
			catch (Exception ex) { System.Diagnostics.Debug.WriteLine("[SwitchLanguage] 型号名异常: " + ex); }

			if (SysPara.LanguageShow == LanguageType.Chinese)
			{
				MainF.NumC1.Image = MainF.NumC2.Image;
				MainF.NumC1.Text = "CN";
			}
			else if (SysPara.LanguageShow == LanguageType.English)
			{
				MainF.NumC1.Image = MainF.NumC3.Image;
				MainF.NumC1.Text = "EN";
			}
			else
			{
				MainF.NumC1.Image = MainF.NumC4.Image;
				MainF.NumC1.Text = "ES";
			}

			#region
			//解决切换语言Flow_chat 断线问题
			foreach (Flow_BaseForm Module in Flow_Module.FlowChart_ModuleList)
			{
				Module.Refresh();
			}
			#endregion

			// 通知自管文案的页面重新套一次文字（自定义控件不在语言表白名单里）。
			// 必须放在最后：前面的语言表循环会先把控件的 Text 改掉，这里再纠正回来。
			try
			{
				EventHandler handler = LanguageChanged;
				if (handler != null) handler(null, EventArgs.Empty);
			}
			catch { }
		}
		public static void SwitchPermission(PermissionType Permission)
		{
			SysPara.UserPermission = Permission;
			MainF.SwitchPermission(Permission);

			MiddleLayer.DataF.UserLoginLog[0] = SysPara.UserName;
			MiddleLayer.DataF.UserLoginLog[1] = SysPara.UserPermission.ToString();
			MiddleLayer.DataF.SaveUserLoginLog(MiddleLayer.DataF.UserLoginLog);
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
		public static void RobotConnect()
		{
		}
		public static void show()
		{
		}

		public static void Initial()
		{
			if (NPSDK.Alarm.IsError)
			{
				OptionChoiceForm warning = new OptionChoiceForm();
				warning.fnChangeButtonsText("OK", "OK");
				warning.fnSetMessageAndButtons(LangMsg("MiddleLayer", "msg_ResetAlarmFirst", "请先清除报警！", "Please Reset Alarm Firstly!", "¡Restablezca la alarma primero!"), false, true, false);
				warning.ShowDialog();

				return;
			}
			MiddleLayer.MainF.dataBControl1.StartWaitingTime();
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
					DialogResult result = MessageBox.Show("Automatic production now, Are you sure to exit automatic production and execute initialize?", "Initialize", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
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
			AlarmClear();
			Thread.Sleep(200);
			if (NPSDK.Alarm.IsError)
			{
				OptionChoiceForm warning = new OptionChoiceForm();
				warning.fnChangeButtonsText("OK", "OK");
				warning.fnSetMessageAndButtons(LangMsg("MiddleLayer", "msg_ResetAlarmFirst", "请先清除报警！", "Please Reset Alarm Firstly!", "¡Restablezca la alarma primero!"), false, true, false);
				warning.ShowDialog();
				return;
			}

			if (SysPara.UpConveyorInitialOk)
			{
				MiddleLayer.MainF.dataBControl1.StopWaitingTime();
				MiddleLayer.MainF.dataBControl1.StartRunTime();
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
				MessageBox.Show("Please Initialize Firstly!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
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
			MiddleLayer.MainF.dataBControl1.StopRunTime();
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
				if (control is Adlink_Motor)
				{
					((Adlink_Motor)control).Stop();
				}
			}
		}

		public static void AlarmClear()
		{
			foreach (ControlBaseInterface control in SDKPara.ControlList)
				if (control is Adlink_Motor)
				{
					if (((Adlink_Motor)control).GetAxisIOState().ALM)
					{
						((Adlink_Motor)control).AlarmReset();
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
			MiddleLayer.MainF.dataBControl1.StopAlarmTime();
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
			bool bOpenSuccess = false;
			try
			{
				SysPara.RecipeDataDirectory = Path.GetDirectoryName(RecipePath);
				SysPara.RecipeName = Path.GetFileNameWithoutExtension(RecipePath);

				SysPara.FilePath = SysPara.RecipeDataDirectory + "\\" + SysPara.RecipeName + ".xml";
				for (int i = 0; i < ModuleManager.ModuleList.Count; i++)
					ModuleManager.ModuleList[i].ReadRecipeData(SysPara.FilePath);

				HardF.ReadRecipeData(SysPara.FilePath);

				bOpenSuccess = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
				SysPara.RecipeDataDirectory = ".\\ModuleData\\RecipeData";
				SysPara.RecipeName = "Recipe";
				bOpenSuccess = false;
			}

			MiddleLayer.HardF.dgv_CalibPos.LoadXmlFileFromPath(string.Format(@"{0}\{1}\{2}\{3}\{4}", Application.StartupPath, "ModuleData", "HardForm", SysPara.RecipeName, "CalibPosition.xml"));
			MiddleLayer.HardF.dgv_MotorPos.LoadXmlFileFromPath(string.Format(@"{0}\{1}\{2}\{3}\{4}", Application.StartupPath, "ModuleData", "HardForm", SysPara.RecipeName, "MotorPosition.xml"));
			OpenVision();
			IniFile IniFile = new IniFile(".\\MachineSetup.ini");
			IniFile.WriteString("MachineSetup", "RecipeName", SysPara.RecipeName);
			IniFile.WriteString("PathSetup", "RecipeDirectory", SysPara.RecipeDataDirectory.Replace(System.IO.Directory.GetCurrentDirectory() + "\\", ".\\"));
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
				if (control is Adlink_Motor)
				{
					control.Refresh();

					IOState = ((Adlink_Motor)control).GetAxisIOState();

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
