using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AlphaRap.FlowWorkbench
{
    internal static class Program
    {
        /// <summary>启动独立仿真工作台，截图模式仅用于离线界面验证。</summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                string language = Option(args, "--language") ?? "Chinese";
                using (var form = new FlowWorkbenchForm(language))
                {
                    string screenshot = Option(args, "--screenshot");
                    if (screenshot == null) { Application.Run(form); return; }
                    form.Show();
                    Application.DoEvents();
                    using (var bitmap = new Bitmap(form.Width, form.Height))
                    {
                        form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(screenshot)));
                        bitmap.Save(screenshot);
                    }
                    form.Close();
                }
            }
            catch (Exception error) { MessageBox.Show(error.ToString(), "FlowWorkbench", MessageBoxButtons.OK, MessageBoxIcon.Error); Environment.ExitCode = 1; }
        }

        /// <summary>读取具有独立参数值的命令行选项。</summary>
        private static string Option(string[] args, string key)
        {
            int index = Array.IndexOf(args, key);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
        }
    }
}
