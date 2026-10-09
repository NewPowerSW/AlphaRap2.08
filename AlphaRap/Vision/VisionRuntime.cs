using System;
using System.IO;
using Microsoft.Win32;

namespace AlphaRap
{
    /// <summary>
    /// 检测本机是否安装了 Cognex VisionPro 运行库。
    /// 未安装时创建 Cognex 的 ActiveX 控件会在原生代码中崩溃（.NET 无法捕获，程序直接退出），
    /// 因此视觉页面、主界面图像显示和相机断开都先判断这里。
    /// </summary>
    public static class VisionRuntime
    {
        private static bool? _installed;

        /// <summary>本机是否安装了 VisionPro（结果缓存）。</summary>
        public static bool Installed
        {
            get
            {
                if (!_installed.HasValue) _installed = Detect();
                return _installed.Value;
            }
        }

        private static bool Detect()
        {
            try
            {
                // VisionPro 安装程序会设置系统环境变量 VPRO_ROOT 指向安装目录
                string root = Environment.GetEnvironmentVariable("VPRO_ROOT", EnvironmentVariableTarget.Machine)
                              ?? Environment.GetEnvironmentVariable("VPRO_ROOT");
                if (!string.IsNullOrEmpty(root) && Directory.Exists(root)) return true;
            }
            catch { }

            try
            {
                foreach (RegistryView view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
                {
                    using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                    using (RegistryKey key = hklm.OpenSubKey(@"SOFTWARE\Cognex\VisionPro"))
                    {
                        if (key != null) return true;
                    }
                }
            }
            catch { }

            return false;
        }
    }
}
