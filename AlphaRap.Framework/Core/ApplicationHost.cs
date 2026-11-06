using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AlphaRap.FlowProgramming;
using AlphaRapLibrary;
using NPSDK;
using Cognex.VisionPro;

namespace AlphaRap
{
    /// <summary>由应用窗口提供模块需要的保存、登录及视觉预览操作。</summary>
    public interface IModuleWindow
    {
        /// <summary>打开应用的统一保存入口。</summary>
        void SaveData();
        /// <summary>刷新应用窗口的权限显示。</summary>
        void SwitchPermission(PermissionType permission);
        /// <summary>登录流程退出后刷新应用界面。</summary>
        void CompleteLogin();
        /// <summary>按当前相机配置更新应用预览区。</summary>
        void SetCameraDisplays(IList<string> cameraNames);
        /// <summary>取得指定相机的可选应用预览控件。</summary>
        CogRecordDisplay GetCameraDisplay(string cameraName);
    }

    /// <summary>项目提供实际页面、工位和与机器有关的生命周期行为。</summary>
    public class MachineApplication
    {
        /// <summary>按本机需要组装页面、加载配置并启动模块。</summary>
        public virtual void Initialize(Action<string> reportProgress)
        {
            throw new NotSupportedException("The application must provide its startup sequence.");
        }

        /// <summary>停止由本机应用启动的任务与设备。</summary>
        public virtual void Shutdown() { }

        /// <summary>处理报警模块发出的整机暂停请求。</summary>
        public virtual void PauseMachine() { }

        /// <summary>处理流程编辑页发出的整机停止请求。</summary>
        public virtual void StopMachine() { }

        /// <summary>按本机规则切换配方，完成时返回 true。</summary>
        public virtual bool ChangeRecipe(string path) { return false; }

        /// <summary>在通用权限状态和应用窗口刷新后处理本机登录记录。</summary>
        public virtual void OnPermissionChanged(PermissionType permission) { }

        /// <summary>机器未提供配置时使用的项目显示名称。</summary>
        public virtual string DisplayName { get { return "AlphaRap"; } }

        /// <summary>提供应用主窗口，用于可选的模块交互和 UI 线程调度。</summary>
        public virtual Form CreateWindow() { return null; }

        /// <summary>显示模块发出的警告，应用可以提供自己的窗口样式。</summary>
        public virtual void ShowWarning(string message, string confirmText)
        {
            MessageBox.Show(message, DisplayName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>提供应用自己的首页。</summary>
        public virtual Form CreateHomePage() { return null; }

        /// <summary>创建通用 IO 页面，项目可返回带固定 IO 的派生页面。</summary>
        public virtual ManualPage CreateManualPage() { return new DefaultManualPage(); }

        /// <summary>创建通用数据页，应用可登记自己的固定日志。</summary>
        public virtual DataForm CreateDataPage() { return new DataForm(); }

        /// <summary>创建视觉管理页面，项目可扩展配方和默认视觉站。</summary>
        public virtual VPForm CreateVisionPage() { return new VPForm(); }

        /// <summary>创建轴管理页面，项目可扩展历史点位表。</summary>
        public virtual HardForm CreateHardwarePage() { return new HardForm(); }

        /// <summary>创建设备管理页，应用可补充自己的通信参数。</summary>
        public virtual Parameter CreateDevicePage() { return new Parameter(); }

        /// <summary>创建常量管理页，应用可提供初始设置。</summary>
        public virtual SystemSetting CreateSettingsPage() { return new SystemSetting(); }

        /// <summary>应用可指定无历史列定义时的生产记录表头。</summary>
        public virtual string[] ProductionColumns { get { return new[] { "Time", "Result" }; } }

        /// <summary>提供本机的站点实例，框架负责登记原生流程事件。</summary>
        public virtual IEnumerable<Flow_BaseForm> CreateStations() { return new Flow_BaseForm[0]; }

        /// <summary>提供可选的 MES 页面。</summary>
        public virtual ModuleBaseForm CreateMesPage() { return null; }

        /// <summary>执行一次项目后台扫描。</summary>
        public virtual void Scan() { }

        /// <summary>读取项目旧式流程的初始化结果。</summary>
        public virtual bool IsLegacyInitialized { get { return false; } }

        /// <summary>清除项目旧式流程的初始化状态。</summary>
        public virtual void ResetLegacyInitialization() { }

        /// <summary>暂停项目旧式流程的外部输出。</summary>
        public virtual void PauseLegacyOutputs() { }

        /// <summary>恢复项目旧式流程的外部输出。</summary>
        public virtual void ResumeLegacyOutputs() { }

        /// <summary>清理旧式输出的暂停记录。</summary>
        public virtual void StopLegacyOutputs() { }
    }

    /// <summary>应用启动和通用页面、真实资源的访问入口。</summary>
    public static class ApplicationHost
    {
        private static readonly List<Form> RegisteredPages = new List<Form>();
        private static MachineApplication application;

        /// <summary>读取应用 EXE 的版本。</summary>
        public static string ApplicationVersion
        {
            get { return (Assembly.GetEntryAssembly() ?? typeof(ApplicationHost).Assembly).GetName().Version.ToString(3); }
        }

        /// <summary>读取当前加载的框架包版本。</summary>
        public static string FrameworkVersion
        {
            get { return typeof(ApplicationHost).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion; }
        }

        /// <summary>由应用指定模块使用的窗口及 UI 调度入口。</summary>
        public static Form MainWindow { get; set; }
        /// <summary>由应用提供的首页。</summary>
        public static Form Home { get; set; }
        public static DataForm Data { get { return FindModule<DataForm>(); } }
        public static Parameter Parameters { get { return FindModule<Parameter>(); } }
        public static SystemSetting Settings { get { return FindModule<SystemSetting>(); } }

        internal static IModuleWindow ModuleWindow { get { return MainWindow as IModuleWindow; } }
        internal static IReadOnlyList<Form> ModulePages { get { return RegisteredPages; } }

        /// <summary>登记实际页面并初始化模块，重复登记同一实例时不重复初始化。</summary>
        public static T RegisterModule<T>(T page) where T : Form
        {
            if (page == null || RegisteredPages.Contains(page)) return page;
            ModuleBaseForm module = page as ModuleBaseForm;
            if (module != null) module.ModuleInitialize(module.Text);
            RegisteredPages.Add(page);
            return page;
        }

        /// <summary>获取已登记且仍可用的指定类型页面，包括应用派生页面。</summary>
        public static T FindModule<T>() where T : Form
        {
            return RegisteredPages.OfType<T>().FirstOrDefault(page => !page.IsDisposed);
        }

        /// <summary>转交应用处理报警暂停。</summary>
        internal static void PauseMachine() { application?.PauseMachine(); }

        /// <summary>转交应用处理流程页的整机停止。</summary>
        internal static void StopMachine() { application?.StopMachine(); }

        /// <summary>由应用确认并执行当前配方的切换。</summary>
        internal static bool ChangeRecipe(string path) { return application?.ChangeRecipe(path) ?? false; }

        /// <summary>更新通用权限和窗口显示，再通知应用记录本机登录信息。</summary>
        internal static void SetPermission(PermissionType permission)
        {
            SysPara.UserPermission = permission;
            ModuleWindow?.SwitchPermission(permission);
            application?.OnPermissionChanged(permission);
        }

        /// <summary>读取流程工具箱使用的同一个真实对象，资源键由配置或项目定义。</summary>
        public static T GetResource<T>(string id) where T : class
        {
            FlowResource resource;
            if (string.IsNullOrWhiteSpace(id) || !FlowResources.Capture().Items.TryGetValue(id, out resource))
                throw new KeyNotFoundException(id);
            T instance = resource.Instance as T;
            if (instance == null) throw new InvalidCastException(id + " / " + typeof(T).FullName);
            return instance;
        }

        /// <summary>使用框架现有语言表注册项目页面。</summary>
        public static void RegisterLanguage(Control control, string formName)
        {
            FrameworkLanguage.RegisterAndApplyLanguage(control, formName);
        }

        /// <summary>登记当前模块页面的文案并应用所选语言。</summary>
        public static void InitializeLanguage(LanguageType language)
        {
            FrameworkLanguage.InitializeLanguageData();
            FrameworkLanguage.SwitchLanguage(language);
        }

        public static event EventHandler LanguageChanged
        {
            add { FrameworkLanguage.LanguageChanged += value; }
            remove { FrameworkLanguage.LanguageChanged -= value; }
        }

        /// <summary>读取已有语言键或回退到调用方提供的原文。</summary>
        public static string Text(string formName, string key, string fallback)
        {
            return FrameworkLanguage.LangText(formName, key, fallback);
        }

        /// <summary>读取项目文案的当前语言，保留现场语言包覆盖。</summary>
        public static string Text(string formName, string key, string zh, string en, string es = null)
        {
            return FrameworkLanguage.LangMsg(formName, key, zh, en, es);
        }

        /// <summary>启动应用提供的初始化流程并返回已登记的主窗口。</summary>
        public static Form Initialize(MachineApplication application, Action<string> reportProgress = null)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));
            if (ApplicationHost.application != null) throw new InvalidOperationException("An application is already registered.");
            ApplicationHost.application = application;
            application.Initialize(reportProgress);
            return ApplicationHost.MainWindow;
        }

        /// <summary>关闭应用拥有的任务并释放框架持有的窗口引用。</summary>
        public static void Shutdown()
        {
            try { application?.Shutdown(); }
            finally
            {
                application = null;
                RegisteredPages.Clear();
                MainWindow = null;
                Home = null;
            }
        }

        /// <summary>使用当前语言读取报警编号对应的文本。</summary>
        public static string GetAlarmText(string code) { return AlarmCatalog.GetAlarmText(SysPara.LanguageShow, code); }

        /// <summary>收到最新报警快照时通知应用窗口。</summary>
        public static event Action<IReadOnlyList<AlarmDisplayRow>> AlarmRowsChanged;

        /// <summary>向应用发布报警显示数据。</summary>
        internal static void PublishAlarms(IReadOnlyList<AlarmDisplayRow> rows)
        {
            var handler = AlarmRowsChanged;
            if (handler != null) handler(rows);
        }
    }
}
