using System.Reflection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using AlphaRap.FlowProgramming;
using AlphaRapLibrary;
using NPSDK;

namespace AlphaRap
{
    /// <summary>项目提供实际页面、工位和与机器有关的生命周期行为。</summary>
    public class MachineApplication
    {
        /// <summary>机器未提供配置时使用的项目显示名称。</summary>
        public virtual string DisplayName { get { return "AlphaRap"; } }

        /// <summary>创建通用 IO 页面，项目可返回带固定 IO 的派生页面。</summary>
        public virtual ManualPage CreateManualPage() { return new DefaultManualPage(); }

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

        public static MainForm MainWindow { get { return MiddleLayer.MainF; } }
        public static HomeForm Home { get { return MiddleLayer.HomeF; } }
        public static DataForm Data { get { return MiddleLayer.DataF; } }
        public static Parameter Parameters { get { return MiddleLayer.ParF; } }
        public static SystemSetting Settings { get { return MiddleLayer.SystemS; } }

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
            MiddleLayer.RegisterAndApplyLanguage(control, formName);
        }

        public static event EventHandler LanguageChanged
        {
            add { MiddleLayer.LanguageChanged += value; }
            remove { MiddleLayer.LanguageChanged -= value; }
        }

        /// <summary>读取已有语言键或回退到调用方提供的原文。</summary>
        public static string Text(string formName, string key, string fallback)
        {
            return MiddleLayer.LangText(formName, key, fallback);
        }

        /// <summary>读取项目文案的当前语言，保留现场语言包覆盖。</summary>
        public static string Text(string formName, string key, string zh, string en, string es = null)
        {
            return MiddleLayer.LangMsg(formName, key, zh, en, es);
        }

        /// <summary>在应用的 STA 入口初始化并运行框架主窗体。</summary>
        public static void Run(MachineApplication application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));
            bool firstInstance;
            using (var mutex = new Mutex(true, "OnlyRunOneInstance", out firstInstance))
            {
                if (!firstInstance)
                {
                    MiddleLayer.ReadLanguageSettings();
                    MessageBox.Show(Text("Program", "msg_AlreadyRunning", "程序已在运行。", "The program is already running.", "El programa ya está en ejecución."),
                        application.DisplayName, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
                RunWindow(application);
            }
        }

        /// <summary>在创建任何窗口前初始化 WinForms 与 DPI，退出时关闭后台扫描。</summary>
        private static void RunWindow(MachineApplication application)
        {
            MiddleLayer.Application = application;
            UiTheme.InitDpi();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MiddleLayer.ReadLanguageSettings();
            try
            {
                LoadProject();
                Application.Run(MiddleLayer.MainF);
            }
            finally { MiddleLayer.DisposeProject(); }
        }

        /// <summary>显示启动进度，并在初始化成功或失败时释放进度线程。</summary>
        private static void LoadProject()
        {
            using (var loading = new LoadingForm())
            {
                MiddleLayer.LoadingF = loading;
                loading.Show();
                var marquee = new Thread(loading.RefreshUI);
                marquee.Start();
                try { MiddleLayer.InitialProject(); }
                finally
                {
                    loading.StopRefresh = true;
                    marquee.Join();
                    loading.Close();
                    MiddleLayer.LoadingF = null;
                }
            }
        }
    }
}
