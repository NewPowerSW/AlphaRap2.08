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
                // Application.Run(new ProductManagerForm());

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
