using System;

namespace AlphaRap
{
    internal static class Program
    {
        /// <summary>以当前机器的应用实现启动通用框架。</summary>
        [STAThread]
        private static void Main()
        {
            ApplicationHost.Run(new StationApplication());
        }
    }
}
