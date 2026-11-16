using System;
using System.Threading;
using System.Threading.Tasks;
using NPSDK;

namespace AlphaRap
{
    public class FlowControl
    {
        private readonly Action runModules;
        private readonly Action scanApplication;
        private readonly Action resetInitialization;

        public volatile bool bStopWork = false;
        private Task work = Task.FromResult(0);

        /// <summary>创建通用 IO 与电机保护扫描。</summary>
        public FlowControl() : this(null, null, null) { }

        /// <summary>创建扫描器并接入应用的周期任务和初始化回调。</summary>
        public FlowControl(Action runModules, Action scanApplication, Action resetInitialization)
        {
            this.runModules = runModules;
            this.scanApplication = scanApplication;
            this.resetInitialization = resetInitialization;
        }

        /// <summary>启动 IO、模块周期和电机保护的后台扫描。</summary>
        public void StartThread()
        {
            if (!work.IsCompleted) return;
            bStopWork = false;
            work = Task.Factory.StartNew(DoWork, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            ApplicationTasks.Global.Track("Scan", work, () => { StopThread(); return Task.FromResult(0); });
        }

        /// <summary>通知扫描任务退出，不阻塞调用方消息循环。</summary>
        public void StopThread()
        {
            bStopWork = true;
        }

        /// <summary>异步等待实际扫描退出，退出前仍保留任务占用。</summary>
        public async Task StopAsync() { StopThread(); await work.ConfigureAwait(false); }

        /// <summary>按既有扫描间隔处理设备任务，累计运行时间和每秒扫描次数。</summary>
        public void DoWork()
        {
            int ScanTick = 0; 
            Int64 LastSecond = 0;
            Int64 TempSecond = 0;
            GetTickCountEx tick = new GetTickCountEx();
            while (!bStopWork)
            {
                Execute();

                Thread.Sleep(5);
                ScanTick++;
                TempSecond = tick.Value;
                if ((TempSecond - LastSecond) >= 1000)
                {
                    LastSecond = TempSecond;
                    if (SysPara.SystemMode == RunMode.RUN)
                    {
                        SysPara.OperationSecond++;
                        if (SysPara.SystemRun)
                            SysPara.RunSecond++;
                        else
                            SysPara.StopSecond++;
                    }
                    SysPara.ScanTime = ScanTick;
                    ScanTick = 0;
                }
            }
        }

        /// <summary>依次刷新 IO、模块、常驻任务和电机保护，隔离各阶段异常。</summary>
        private void Execute()
        {
            try { SDKKernal.RefreshIO(); }
            catch (Exception ex) { NPSDK.Alarm.Show("2018", ex.Message); }

            try { runModules?.Invoke(); }
            catch (Exception) { NPSDK.Alarm.Show("2019");}

            try { scanApplication?.Invoke(); }
            catch (Exception) { NPSDK.Alarm.Show("2019"); }

            try { MotorProtection.CheckMotorProtected(); }
            catch (Exception) { NPSDK.Alarm.Show("2020"); }
        }

        /// <summary>进入初始化状态并通知应用清除旧式初始化状态。</summary>
        public void InitialReset()
        {
            MachineState.Global.Initialize();
            resetInitialization?.Invoke();
        }

        /// <summary>将扫描统计的运行模式切换为自动运行。</summary>
        public void RunReset()
        {
            MachineState.Global.Start(true);
        }
    }
}
