using System;
using System.Threading;
using System.Windows.Forms;

namespace AlphaRap
{
    internal static class Program
    {
        /// <summary>启动本机 WinForms 应用，并由应用管理窗口与进程生命周期。</summary>
        [STAThread]
        private static void Main()
        {
            var station = new StationApplication();
            bool firstInstance;
            using (var mutex = new Mutex(true, "OnlyRunOneInstance", out firstInstance))
            {
                if (!firstInstance)
                {
                    MiddleLayer.ReadLanguageSettings();
                    MessageBox.Show(ApplicationHost.Text("Program", "msg_AlreadyRunning", "程序已在运行。", "The program is already running.", "El programa ya está en ejecución."),
                        station.DisplayName, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }

                UiTheme.InitDpi();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                try { Application.Run(Initialize(station)); }
                finally { ApplicationHost.Shutdown(); }
            }
        }

        /// <summary>在加载界面显示模块进度，并在初始化结束后回收动画线程。</summary>
        private static Form Initialize(StationApplication station)
        {
            using (var loading = new LoadingForm(station.DisplayName))
            {
                loading.Show();
                var marquee = new Thread(loading.RefreshUI);
                marquee.Start();
                try { return ApplicationHost.Initialize(station, loading.SetCaption); }
                finally
                {
                    loading.StopRefresh = true;
                    marquee.Join();
                    loading.Close();
                }
            }
        }
    }
}
