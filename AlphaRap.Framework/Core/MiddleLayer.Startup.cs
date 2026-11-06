using System;
using System.IO;
using System.Windows.Forms;
using AlphaRap.Classes;
using AlphaRap.MenuForm;
using AlphaRapLibrary;
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
            SwitchPermission(SysPara.UserPermission);

            ShowLoadingStage(reportProgress, "stage_IO", "初始化输入输出", "Initializing IO", "Inicializando E/S");
            SDKKernal.SetSimulation(SysPara.Simulation);
            SDKKernal.InitializeComponent();
            OperationLog.Write("MainForm", "设备初始化完成（仿真=" + SysPara.Simulation + "）");
            alarmRunTask = new AlarmRunTask();
            ShowLoadingStage(reportProgress, "stage_Recipe", "载入配方", "Loading recipe", "Cargando receta");
            OpenRecipe(SysPara.FilePath);
            FlowProgramming.FlowPrograms.Install();
            FlowCtrl = new FlowControl();
            FlowCtrl.StartThread();
            ServoOn();
            ShowLoadingStage(reportProgress, "stage_Vision", "载入视觉算法", "Loading vision algorithms", "Cargando algoritmos de visión");
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
            foreach (Flow_BaseForm station in Application.CreateStations()) RegisterForm(station);
            SpanLifeF = new LifeSpan.LifeSpanForm();
            ProductF = RegisterForm(ProductF ?? new ProductManagerForm());
            PlatF = RegisterForm(PlatF ?? new PlatFormSetting());
            SystemS = RegisterForm(SystemS ?? Application.CreateSettingsPage());
            ParF = RegisterForm(ParF ?? Application.CreateDevicePage());
            SystemF = RegisterForm(SystemF ?? new SystemForm());
            MainF = RegisterForm(MainF ?? Application.CreateWindow());
            HomeF = RegisterForm(HomeF ?? Application.CreateHomePage());
            ManualF = RegisterForm(ManualF ?? Application.CreateManualPage());
            HardF = RegisterForm(HardF ?? Application.CreateHardwarePage());
            CheckF = RegisterForm(CheckF ?? new CheckForm());
            AddF = RegisterForm(AddF ?? new AddUserForm());
            VPF = RegisterForm(VPF ?? Application.CreateVisionPage());
            FlowF = new Flow();
            DataF = RegisterForm(DataF ?? Application.CreateDataPage());
            LogDirectories.RestoreAlarmPath(DataF);
            LockForm1 = new LockForm1();
            MesF = RegisterForm(MesF ?? Application.CreateMesPage());
        }

        /// <summary>更新启动阶段，没有启动页时不创建界面。</summary>
        private static void ShowLoadingStage(Action<string> reportProgress, string key, string zh, string en, string es)
        {
            reportProgress?.Invoke(LangMsg("LoadingForm", key, zh, en, es));
        }

        /// <summary>登记实际窗体并初始化模块，重复调用继续使用原实例。</summary>
        private static T RegisterForm<T>(T form) where T : Form
        {
            if (form == null || lstForm.Contains(form)) return form;
            ModuleBaseForm module = form as ModuleBaseForm;
            if (module != null) module.ModuleInitialize(module.Text);
            lstForm.Add(form);
            return form;
        }

        /// <summary>更新页面权限并记录当前用户登录信息。</summary>
        public static void SwitchPermission(PermissionType Permission)
        {
            SysPara.UserPermission = Permission;
            WindowModules?.SwitchPermission(Permission);
            DataF?.WriteLog("UserLogin", SysPara.UserName, Permission.ToString());
            OperationLog.Write("UserLoginForm", (SysPara.UserName == "None" ? "注销" : "登录")
                + "　工号：" + SysPara.UserName + "　权限：" + Permission);
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
