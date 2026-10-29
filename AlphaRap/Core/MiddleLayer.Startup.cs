using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using AlphaRap.Classes;
using AlphaRap.MenuForm;
using AlphaRapLibrary;
using NPSDK;

namespace AlphaRap
{
    partial class MiddleLayer
    {
        private static readonly Lazy<Dictionary<string, Type>> FormTypes = new Lazy<Dictionary<string, Type>>(FindFormTypes);

        /// <summary>读取配置、创建页面、登记语言并启动设备内核、配方和后台扫描。</summary>
        public static void InitialProject()
        {
            ReadLanguageSettings();
            ShowLoadingStage("stage_Ini", "读取系统配置", "Loading system configuration", "Cargando configuración del sistema");
            LoadMachineSettings("AlphaRap2.0");
            LoadAlarmTable();
            CreateProjectForms();
            ShowLoadingStage("stage_Languages", "载入语言表", "Loading translations", "Cargando traducciones");
            InitialLanguageData();
            SwitchLanguage(SysPara.LanguageShow);
            ShowLoadingStage("stage_Permissions", "载入权限", "Loading permissions", "Cargando permisos");
            SwitchPermission(SysPara.UserPermission);

            ShowLoadingStage("stage_IO", "初始化输入输出", "Initializing IO", "Inicializando E/S");
            SDKKernal.SetSimulation(SysPara.Simulation);
            SDKKernal.InitializeComponent();
            OperationLog.Write("MainForm", "设备初始化完成（仿真=" + SysPara.Simulation + "）");
            alTask = new AlwaysRunTask();
            alarmRunTask = new AlarmRunTask();
            ShowLoadingStage("stage_Recipe", "载入配方", "Loading recipe", "Cargando receta");
            OpenRecipe(SysPara.FilePath);
            FlowProgramming.FlowPrograms.Install();
            gEXIT = false;
            FlowCtrl = new FlowControl();
            FlowCtrl.StartThread();
            ServoOn();
            ShowLoadingStage("stage_Vision", "载入视觉算法", "Loading vision algorithms", "Cargando algoritmos de visión");
        }

        /// <summary>初始化仅包含系统页和主窗口的兼容项目入口。</summary>
        public static void InitialProject3()
        {
            ReadLanguageSettings();
            ShowLoadingStage("stage_Ini", "读取系统配置", "Loading system configuration", "Cargando configuración del sistema");
            LoadMachineSettings("AlphaRap3.0");
            ShowLoadingStage("stage_Alarms", "载入报警表", "Loading alarms", "Cargando alarmas");
            LoadAlarmTable();
            ShowLoadingStage("stage_Forms", "载入页面", "Loading pages", "Cargando páginas");
            SystemF = CreateForm(SystemF, "SystemForm");
            MainF = CreateForm(MainF, "MainForm");
            InitialLanguageData();
            SwitchLanguage(SysPara.LanguageShow);
            SwitchPermission(SysPara.UserPermission);
            OpenRecipe(SysPara.FilePath);
            ServoOn();
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
                SysPara.MESDirectory = ini.ReadString("PathSetup", "MESDirectory", ".\\ModuleData\\MESData");
                SysPara.IOPortDirectory = ini.ReadString("PathSetup", "IOPortDirectory", ".\\ModuleData");
            }
            ModuleManager.SettingDataDirectory = SysPara.SettingDataDirectory;
            SysPara.FilePath = Path.Combine(SysPara.RecipeDataDirectory, SysPara.RecipeName + ".xml");
        }

        /// <summary>按模块依赖顺序创建流程页、设置页和业务页。</summary>
        private static void CreateProjectForms()
        {
            GantryF = CreateForm(GantryF, "Gantry");
            UpConveyorF = CreateForm(UpConveyorF, "UpConveryor");
            DownConveyorF = CreateForm(DownConveyorF, "DownConveryor");
            SpanLifeF = new LifeSpan.LifeSpanForm();
            ShowLoadingStage("stage_Products", "载入型号管理", "Loading model management", "Cargando gestión de modelos");
            ProductF = CreateForm(ProductF, "ProductManagerForm");
            PlatF = CreateForm(PlatF, "PlatFormSetting");
            SystemS = CreateForm(SystemS, "SystemSetting");
            ParF = CreateForm(ParF, "Parameter");
            SystemF = CreateForm(SystemF, "SystemForm");
            ShowLoadingStage("stage_Main", "载入主界面", "Loading main window", "Cargando ventana principal");
            MainF = CreateForm(MainF, "MainForm");
            ShowLoadingStage("stage_Home", "载入主页", "Loading home page", "Cargando página de inicio");
            HomeF = CreateForm(HomeF, "HomeForm");
            ShowLoadingStage("stage_Manual", "载入手动页面", "Loading manual controls", "Cargando controles manuales");
            ManualF = CreateForm(ManualF, "ManualForm");
            HardF = CreateForm(HardF, "HardForm");
            CheckF = CreateForm(CheckF, "CheckForm");
            AddF = CreateForm(AddF, "AddUserForm");
            VPF = CreateForm(VPF, "VPForm");
            FlowF = new Flow();
            DataF = CreateForm(DataF, "DataForm");
            LogDirectories.RestoreAlarmPath(DataF);
            LockForm1 = new LockForm1();
            MesF = CreateForm(MesF, "LENS");
        }

        /// <summary>更新启动阶段，没有启动页时不创建界面。</summary>
        private static void ShowLoadingStage(string key, string zh, string en, string es)
        {
            if (LoadingF == null || LoadingF.IsDisposed) return;
            LoadingF.SetCaption(LangMsg("LoadingForm", key, zh, en, es));
        }

        /// <summary>缓存当前程序集的具体窗体类型，保持按简单类型名查找的顺序。</summary>
        private static Dictionary<string, Type> FindFormTypes()
        {
            Dictionary<string, Type> types = new Dictionary<string, Type>(StringComparer.Ordinal);
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsAbstract || !typeof(Form).IsAssignableFrom(type) || types.ContainsKey(type.Name)) continue;
                types.Add(type.Name, type);
            }
            return types;
        }

        /// <summary>创建或复用窗体，同一实例只登记、初始化一次；未知类型返回原对象。</summary>
        public static dynamic CreateForm(dynamic FormAddress, string FormName)
        {
            Type type;
            if (string.IsNullOrEmpty(FormName) || !FormTypes.Value.TryGetValue(FormName, out type)) return FormAddress;
            Form form = (Form)FormAddress;
            if (form == null || form.IsDisposed) form = (Form)Activator.CreateInstance(type);
            if (lstForm.Contains(form)) return form;
            ModuleBaseForm module = form as ModuleBaseForm;
            if (module != null) module.ModuleInitialize(module.Text);
            lstForm.Add(form);
            return form;
        }

        /// <summary>更新页面权限并记录当前用户登录信息。</summary>
        public static void SwitchPermission(PermissionType Permission)
        {
            SysPara.UserPermission = Permission;
            if (MainF != null) MainF.SwitchPermission(Permission);
            if (DataF != null)
            {
                DataF.UserLoginLog[0] = SysPara.UserName;
                DataF.UserLoginLog[1] = Permission.ToString();
                DataF.SaveUserLoginLog(DataF.UserLoginLog);
            }
            OperationLog.Write("UserLoginForm", (SysPara.UserName == "None" ? "注销" : "登录")
                + "　工号：" + SysPara.UserName + "　权限：" + Permission);
        }

        /// <summary>停止后台扫描后关闭伺服，避免扫描继续访问正在关闭的设备。</summary>
        public static void DisposeProject()
        {
            gEXIT = true;
            if (alarmRunTask != null) alarmRunTask.AlarmTaskIsRun = false;
            if (FlowCtrl != null) FlowCtrl.StopThread();
            ServoOff();
        }

        /// <summary>保留外部调用约定的显示入口，页面显示由 MainForm 管理。</summary>
        public static void show() { }
    }
}
