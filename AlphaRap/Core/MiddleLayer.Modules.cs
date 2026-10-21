using System;
using AlphaRapLibrary;
using NPSDK;

namespace AlphaRap
{
    partial class MiddleLayer
    {
        /// <summary>调用全部模块的后台周期任务，模块失败时报警并继续扫描其余模块。</summary>
        public static void AlwaysRun()
        {
            if (Alarm.DoStop)
            {
                if (SysPara.SystemRun) LogManagement.Instance.SaveLog(LogManagement.LogType.MachineStatus, "Alarm stop");
                Alarm.DoStop = false;
            }
            ExecuteModules(module => module.AlwaysRun(), "AlwaysRun", "2010", false);
        }

        /// <summary>复位全部模块的初始化参数，失败时停止生产。</summary>
        public static void InitialParameterReset()
        {
            ExecuteModules(module => module.InitialParameterReset(), "InitialParameterReset", "2013", true);
        }

        /// <summary>复位全部模块的初始化步骤，失败时停止生产。</summary>
        public static void InitialReset()
        {
            ExecuteModules(module => module.InitialReset(), "InitialReset", "2014", true);
        }

        /// <summary>读取所有模块的初始化状态，全部完成时返回 true。</summary>
        public static bool GetInitialOk()
        {
            bool ready = true;
            foreach (ModuleBaseForm module in ModuleManager.ModuleList) ready &= module.GetInitialOk();
            return ready;
        }

        /// <summary>复位全部模块的自动运行步骤，失败时停止生产。</summary>
        public static void RunReset()
        {
            ExecuteModules(module => module.RunReset(), "RunReset", "2016", true);
        }

        /// <summary>调用全部模块的自动运行任务，失败时停止生产。</summary>
        public static void Run()
        {
            ExecuteModules(module => module.Run(), "Run", "2017", true);
        }

        /// <summary>遍历模块并隔离回调异常，按调用方指定策略报警和停止生产。</summary>
        private static void ExecuteModules(Action<ModuleBaseForm> action, string operation, string alarmCode, bool stopOnFailure)
        {
            foreach (ModuleBaseForm module in ModuleManager.ModuleList)
            {
                try { action(module); }
                catch (Exception error)
                {
                    LogModuleFailure(error, stopOnFailure);
                    string message = string.Format(LangMsg("MiddleLayer", "msg_ModuleActionFailed",
                        "模块「{0}」执行 {1} 失败。", "Module \"{0}\" failed during {1}.", "El módulo «{0}» falló durante {1}."), module.Name, operation);
                    Alarm.Show(alarmCode, message);
                    if (stopOnFailure) StopRun();
                }
            }
        }

        /// <summary>输出完整异常；需要停止生产的错误同时写入运行日志，日志失败不影响报警和停止。</summary>
        private static void LogModuleFailure(Exception error, bool persist)
        {
            System.Diagnostics.Debug.WriteLine(error);
            if (!persist || DataF == null || DataF.IsDisposed) return;
            try { DataF.AddLogError(error.ToString()); }
            catch (Exception logError) { System.Diagnostics.Debug.WriteLine(logError); }
        }
    }
}
