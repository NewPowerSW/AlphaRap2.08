using System;
using System.IO;
using AlphaRap.Classes;
using AlphaRap.MenuForm;
using AlphaRapLibrary;
using static AlphaRap.FrameworkLanguage;
using NPSDK;

namespace AlphaRap
{
    partial class MiddleLayer
    {
        /// <summary>读取配置、创建页面、登记语言并启动设备内核、配方和后台扫描。</summary>
        public static void InitialProject(Action<string> reportProgress)
        {
            ReadLanguageSettings();
            ShowLoadingStage(reportProgress, "stage_Ini", "读取系统配置", "Loading system configuration", "Cargando configuración del sistema");
            LoadMachineSettings(Application.DisplayName);
            LoadAlarmTable();
            CreateProjectForms();
            ShowLoadingStage(reportProgress, "stage_Languages", "载入语言表", "Loading translations", "Cargando traducciones");
            InitialLanguageData();
            SwitchLanguage(SysPara.LanguageShow);
            ShowLoadingStage(reportProgress, "stage_Permissions", "载入权限", "Loading permissions", "Cargando permisos");
            ApplicationHost.SetPermission(SysPara.UserPermission);

            ShowLoadingStage(reportProgress, "stage_IO", "初始化输入输出", "Initializing IO", "Inicializando E/S");
            SDKKernal.SetSimulation(SysPara.Simulation);
            SDKKernal.InitializeComponent();
            OperationLog.Write("MainForm", "设备初始化完成（仿真=" + SysPara.Simulation + "）");
            alarmRunTask = new AlarmRunTask();
            ShowLoadingStage(reportProgress, "stage_Recipe", "载入配方", "Loading recipe", "Cargando receta");
            OpenRecipe(SysPara.FilePath);
            FlowProgramming.FlowPrograms.Install();
            FlowCtrl = new FlowControl(AlwaysRun, Application.Scan, Application.ResetLegacyInitialization);
            FlowCtrl.StartThread();
            ServoOn();
            ShowLoadingStage(reportProgress, "stage_Vision", "载入视觉算法", "Loading vision algorithms", "Cargando algoritmos de visión");
        }

        /// <summary>在创建任何页面之前读取界面语言和目录，数值区域格式保持不变。</summary>
        internal static void ReadLanguageSettings()
        {
            using (IniFile ini = new IniFile(".\\MachineSetup.ini"))
            {
                SysPara.LanguageDataDirectory = ini.ReadString("PathSetup", "LanguageDirectory", ".\\LanguageData");
                SysPara.LanguageName = ini.ReadString("MachineSetup", "LanguageName", "Chinese");
            }
            SysPara.LanguageShow = SysPara.LanguageName == "English" ? LanguageType.English
                : SysPara.LanguageName == "Español" || SysPara.LanguageName == "Spanish" ? LanguageType.Español : LanguageType.Chinese;
        }

        /// <summary>读取机器系统参数和目录，同步模块配置目录与当前配方路径。</summary>
        private static void LoadMachineSettings(string defaultProjectName)
        {
            using (IniFile ini = new IniFile(".\\MachineSetup.ini"))
            {
                SysPara.ProjectName = ini.ReadString("MachineSetup", "ProjectName", defaultProjectName);
                SysPara.RecipeName = ini.ReadString("MachineSetup", "RecipeName", "Recipe");
                SysPara.Simulation = ini.ReadBoolen("LogicSetup", "Simulation", false);
                SysPara.LogFilePath = ini.ReadString("PathSetup", "LogFileDirectory", ".\\LogFile");
                SysPara.VisionFileDirectory = ini.ReadString("PathSetup", "VisionDataDirectory", ".\\VisionData");
                SysPara.AlarmTableDirectory = ini.ReadString("PathSetup", "AlarmTableDirectory", ".\\AlarmTable");
                SysPara.SettingDataDirectory = ini.ReadString("PathSetup", "SettingDataDirectory", ".\\ModuleData\\SettingData");
                SysPara.RecipeDataDirectory = ini.ReadString("PathSetup", "RecipeDirectory", ".\\ModuleData\\RecipeData");
                SysPara.IOPortDirectory = ini.ReadString("PathSetup", "IOPortDirectory", ".\\ModuleData");
            }
            ModuleManager.SettingDataDirectory = SysPara.SettingDataDirectory;
            SysPara.FilePath = Path.Combine(SysPara.RecipeDataDirectory, SysPara.RecipeName + ".xml");
        }

        /// <summary>按模块依赖顺序创建流程页、设置页和业务页。</summary>
        private static void CreateProjectForms()
        {
            foreach (Flow_BaseForm station in Application.CreateStations()) ApplicationHost.RegisterModule(station);
            SpanLifeF = new LifeSpan.LifeSpanForm();
            ProductF = ApplicationHost.RegisterModule(ProductF ?? new ProductManagerForm());
            PlatF = ApplicationHost.RegisterModule(PlatF ?? new PlatFormSetting());
            SystemS = ApplicationHost.RegisterModule(SystemS ?? Application.CreateSettingsPage());
            ParF = ApplicationHost.RegisterModule(ParF ?? Application.CreateDevicePage());
            SystemF = ApplicationHost.RegisterModule(SystemF ?? new SystemForm());
            MainF = ApplicationHost.RegisterModule(MainF ?? Application.CreateWindow());
            ApplicationHost.MainWindow = MainF;
            HomeF = ApplicationHost.RegisterModule(HomeF ?? Application.CreateHomePage());
            ApplicationHost.Home = HomeF;
            ManualF = ApplicationHost.RegisterModule(ManualF ?? Application.CreateManualPage());
            HardF = ApplicationHost.RegisterModule(HardF ?? Application.CreateHardwarePage());
            CheckF = ApplicationHost.RegisterModule(CheckF ?? new CheckForm());
            AddF = ApplicationHost.RegisterModule(AddF ?? new AddUserForm());
            VPF = ApplicationHost.RegisterModule(VPF ?? Application.CreateVisionPage());
            FlowF = new Flow();
            DataF = ApplicationHost.RegisterModule(DataF ?? Application.CreateDataPage());
            LogDirectories.RestoreAlarmPath(DataF);
            LockForm1 = new LockForm1();
            MesF = ApplicationHost.RegisterModule(MesF ?? Application.CreateMesPage());
        }

        /// <summary>更新启动阶段，没有启动页时不创建界面。</summary>
        private static void ShowLoadingStage(Action<string> reportProgress, string key, string zh, string en, string es)
        {
            reportProgress?.Invoke(LangMsg("LoadingForm", key, zh, en, es));
        }

        /// <summary>登记本站页面，包括由应用单独持有的流程、锁屏和寿命页。</summary>
        private static void InitialLanguageData()
        {
            InitializeLanguageData();
            RegisterLanguage(LockForm1, "LockForm1");
            RegisterLanguage(FlowF, "Flow");
            RegisterLanguage(SpanLifeF, "SpanLife");
        }

        /// <summary>停止后台扫描后关闭伺服，避免扫描继续访问正在关闭的设备。</summary>
        public static void DisposeProject()
        {
            if (alarmRunTask != null) alarmRunTask.StopAlarmRunTask();
            if (FlowCtrl != null) FlowCtrl.StopThread();
            ServoOff();
        }
    }
}
