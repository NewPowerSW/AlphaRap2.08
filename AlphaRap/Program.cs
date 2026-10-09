using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace AlphaRap
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 未处理异常写入 LogFile\crash_*.txt 并提示，避免程序无提示闪退
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => ReportCrash(e.Exception, false);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => ReportCrash(e.ExceptionObject as Exception, true);

            bool ISRuned;
            System.Threading.Mutex mutex = new System.Threading.Mutex(true, "OnlyRunOneInstance", out ISRuned);
            if (ISRuned)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

				#region LogShow
				MiddleLayer.LoadingF = new LoadingForm();
				MiddleLayer.LoadingF.Show();
				Thread LoadingMarqueeT = new Thread(MiddleLayer.LoadingF.RefreshUI);
				LoadingMarqueeT.Start();
				MiddleLayer.InitialProject();
				MiddleLayer.LoadingF.StopRefresh = true;
				LoadingMarqueeT.Join();
				MiddleLayer.LoadingF.Close();
				#endregion

				 //Initial Project

                Application.Run(MiddleLayer.MainF);
                MiddleLayer.DisposeProject();
            }
            else
            {
                MessageBox.Show("Program is Running!","OK",MessageBoxButtons.OK,MessageBoxIcon.Warning,MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        /// <summary>记录未处理异常：写入 LogFile\crash_时间.txt，并弹窗显示原因与文件位置。</summary>
        private static void ReportCrash(Exception ex, bool terminating)
        {
            string file = "";
            try
            {
                string dir = Path.Combine(Application.StartupPath, "LogFile");
                Directory.CreateDirectory(dir);
                file = Path.Combine(dir, "crash_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt");
                File.WriteAllText(file, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                    (terminating ? "  [程序终止]" : "") + Environment.NewLine + (ex != null ? ex.ToString() : "(无异常信息)"));
            }
            catch { }
            try
            {
                MessageBox.Show((ex != null ? ex.GetType().Name + ": " + ex.Message : "未知错误") +
                    Environment.NewLine + Environment.NewLine + "详细信息已保存到：" + Environment.NewLine + file,
                    "程序异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
        }
    }
}
