using System;
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
            bool ISRuned;
            System.Threading.Mutex mutex = new System.Threading.Mutex(true, "OnlyRunOneInstance", out ISRuned);
            if (ISRuned)
            {
                // 高 DPI：必须最先执行——要在创建任何窗口之前声明 DPI 感知，
                // 也要在创建任何窗体之前定好缩放系数（窗体构造函数里会按它把设计尺寸整体放大）
                UiTheme.InitDpi();

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
    }   
}
