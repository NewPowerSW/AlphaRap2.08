using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlphaRap;
using AlphaRap.FlowProgramming;
using AlphaRapLibrary;
using NPSDK;

public sealed class OfflineUiDevice : AlphaRap.AbstractDevice
{
    public readonly ManualResetEventSlim Entered = new ManualResetEventSlim();
    public readonly ManualResetEventSlim Release = new ManualResetEventSlim();
    public bool BlockOpen, BlockReceive;
    public int OpenCount, IoThread;
    private bool connected;
    public override bool IsConnected { get { return connected; } }
    public override bool SupportsRawIo { get { return true; } }

    /// <summary>只构造离线测试设备。</summary>
    public OfflineUiDevice(string name) : base(name) { }

    /// <summary>模拟阻塞打开，同时记录实际执行线程。</summary>
    public override void Open()
    {
        IoThread = Thread.CurrentThread.ManagedThreadId; OpenCount++; Entered.Set();
        if (BlockOpen) { Release.Wait(); Flow_Chart.CurrentExecutionToken.ThrowIfCancellationRequested(); }
        connected = true;
    }

    /// <summary>允许测试关闭打断等待，不接触任何硬件。</summary>
    public override void Close() { Release.Set(); connected = false; }

    /// <summary>模拟忽略取消直到实际数据返回的旧同步读取。</summary>
    public override string Receive() { Entered.Set(); if (BlockReceive) Release.Wait(); return "LATE"; }

    /// <summary>在内存接收原始发送输入。</summary>
    public override string Send(string text) { return ""; }
}

internal static class DeviceUiCases
{
    /// <summary>创建实际页面和配置实例，但不显示窗口、不打开硬件。</summary>
    private static DeviceControl Panel(ModuleBaseForm form)
    {
        int number = form.Controls.OfType<DeviceControl>().Count() + 1;
        var panel = new DeviceControl { Name = "OfflineControl" + number, SingleDevice = true };
        form.Controls.Add(panel); var formHandle = form.Handle; var panelHandle = panel.Handle;
        string error = panel.BindDevice(AlphaRap.Configuration.HardwareControlTypes.Identity(typeof(OfflineUiDevice)), form.Name + "_" + panel.Name);
        Assert(string.IsNullOrEmpty(error), "Cannot bind offline UI device: " + error); return panel;
    }

    /// <summary>阻塞打开期间 UI 心跳仍能运行，关闭可取消，重复打开不会派发第二次。</summary>
    internal static void Responsive()
    {
        SysPara.SystemMode = RunMode.IDLE;
        using(var form = new ModuleBaseForm { Name = "OfflineUiHost" })
        {
            DeviceControl panel = Panel(form); var device = (OfflineUiDevice)panel.Device; device.BlockOpen = true;
            int uiThread = Thread.CurrentThread.ManagedThreadId, ticks = 0;
            using(var timer = new System.Windows.Forms.Timer { Interval = 5 })
            {
                timer.Tick += (sender,args) => ticks++; timer.Start();
                Stopwatch time = Stopwatch.StartNew(); Task opening = panel.OpenDeviceAsync();
                Assert(time.ElapsedMilliseconds < 200, "Open blocked UI caller");
                Pump(() => device.Entered.IsSet && ticks >= 2);
                Assert(device.IoThread != uiThread && DeviceOperations.HasPending && form.IsManualRun(), "IO was not tracked in background");
                panel.OpenDeviceAsync().GetAwaiter().GetResult(); Assert(device.OpenCount == 1, "Duplicate UI open dispatched");
                Task closing = panel.CloseDeviceAsync(); Pump(() => opening.IsCompleted && closing.IsCompleted);
                opening.GetAwaiter().GetResult(); closing.GetAwaiter().GetResult();
                Assert(!DeviceOperations.HasPending && !device.IsConnected, "UI stop released early");
            }
            ModuleManager.ModuleList.Remove(form);
        }
    }

    /// <summary>面板或模块释放后，实际未返回的读取仍保留全局占用并禁止主保存。</summary>
    internal static void Disposal()
    {
        SysPara.SystemMode = RunMode.IDLE;
        var form = new ModuleBaseForm { Name = "DisposedUiHost" }; DeviceControl panel = Panel(form);
        var device = (OfflineUiDevice)panel.Device; int deliveries = 0;
        panel.Received += (sender,text) => deliveries++;
        Task open = panel.OpenDeviceAsync(); Pump(() => open.IsCompleted); open.GetAwaiter().GetResult();
        device.Entered.Reset(); device.Release.Reset(); device.BlockReceive = true;
        Task reading = panel.ReceiveDataAsync(); Pump(() => device.Entered.IsSet);
        panel.Dispose(); ModuleManager.ModuleList.Remove(form); form.Dispose();
        Assert(DeviceOperations.HasPending && !reading.IsCompleted, "Disposed UI lost physical IO");
        bool blocked = false; try { FlowPrograms.RequireIdle(); } catch(InvalidOperationException) { blocked = true; }
        Assert(blocked, "Save/recipe changes ignored detached IO");
        device.Release.Set(); Pump(() => reading.IsCompleted && !DeviceOperations.HasPending); reading.GetAwaiter().GetResult();
        Assert(deliveries == 0, "Late data delivered to disposed view");
    }

    /// <summary>取消回调阻塞时，停止请求快速返回任务，全局占用保留到回调退出。</summary>
    internal static void CancellationCleanup()
    {
        SysPara.SystemMode = RunMode.IDLE;
        using(var entered=new ManualResetEventSlim())
        using(var cancelEntered=new ManualResetEventSlim())
        using(var actionRelease=new ManualResetEventSlim())
        using(var cancelRelease=new ManualResetEventSlim())
        {
            Task work=DeviceOperations.RunAsync(null,()=>
            {
                using(Flow_Chart.CurrentExecutionToken.Register(()=>{cancelEntered.Set();cancelRelease.Wait();}))
                {entered.Set();actionRelease.Wait();}
            });
            Assert(entered.Wait(1500),"Background work not started");
            Stopwatch watch=Stopwatch.StartNew();Task stopping=DeviceOperations.RequestStopAllAsync();
            Assert(watch.ElapsedMilliseconds<200 && cancelEntered.Wait(1500),"Stop callback blocked caller");
            actionRelease.Set();Assert(DeviceOperations.HasPending && !stopping.Wait(20),"Blocked cancellation occupancy lost");
            cancelRelease.Set();Assert(stopping.Wait(1500),"Cancel did not finish");
            Pump(()=>work.IsCompleted && !DeviceOperations.HasPending);
            try{work.GetAwaiter().GetResult();}catch(OperationCanceledException) { }
        }
    }

    /// <summary>模块忙碌时异步入口明确拒绝，未接受动作不会遗留登记。</summary>
    internal static void BusyAndStopGuard()
    {
        SysPara.SystemMode=RunMode.IDLE;
        using(var form=new ModuleBaseForm())
        using(var entered=new ManualResetEventSlim())
        using(var release=new ManualResetEventSlim())
        {
            form.ExecuteManual(()=>{entered.Set();release.Wait();});Assert(entered.Wait(1500),"Manual owner not entered");
            int dispatched=0;Task rejected=DeviceOperations.RunAsync(form,()=>Interlocked.Increment(ref dispatched));
            Pump(()=>rejected.IsCompleted);bool failed=false;try{rejected.GetAwaiter().GetResult();}catch(InvalidOperationException){failed=true;}
            Assert(failed && dispatched==0 && !DeviceOperations.HasPending,"Busy module dropped a task silently");
            release.Set();Pump(()=>!form.IsManualRun());ModuleManager.ModuleList.Remove(form);
        }
        SysPara.SystemMode=RunMode.STOPPING;Task late=DeviceOperations.RunAsync(null,()=>{throw new Exception("Stop guard failed");});
        Pump(()=>late.IsCompleted);bool cancelled=false;try{late.GetAwaiter().GetResult();}catch(OperationCanceledException){cancelled=true;}
        Assert(cancelled && !DeviceOperations.HasPending,"Late UI command ran while stopping");SysPara.SystemMode=RunMode.IDLE;
    }

    /// <summary>同一参数页的多个自动连接各自登记，不因共享模块手动锁而丢掉第二台设备。</summary>
    internal static void ParallelAutoOpen()
    {
        SysPara.SystemMode=RunMode.IDLE;
        using(var form=new ModuleBaseForm { Name="AutoOpenHost" })
        {
            DeviceControl first=Panel(form);DeviceControl second=Panel(form);
            var one=(OfflineUiDevice)first.Device;var two=(OfflineUiDevice)second.Device;
            one.BlockOpen=two.BlockOpen=true;first.AutoOpen=second.AutoOpen=true;
            var load=typeof(DeviceControl).GetMethod("DeviceControl_Load",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            load.Invoke(first,new object[]{first,EventArgs.Empty});load.Invoke(second,new object[]{second,EventArgs.Empty});
            Pump(()=>one.Entered.IsSet && two.Entered.IsSet);
            Assert(DeviceOperations.HasPending && one.OpenCount==1 && two.OpenCount==1,"Auto-open dropped a device");
            one.Release.Set();two.Release.Set();Pump(()=>!DeviceOperations.HasPending);
            Pump(()=>!(bool)typeof(DeviceControl).GetField("_uiBusy",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(first)
                && !(bool)typeof(DeviceControl).GetField("_uiBusy",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(second));
            Assert(one.IsConnected && two.IsConnected,"Auto-open did not finish");ModuleManager.ModuleList.Remove(form);
        }
    }

    /// <summary>设备已返回但 UI 尚未交付时发生整机停止，旧接收事件也必须作废。</summary>
    internal static void LatePresentation()
    {
        SysPara.SystemMode=RunMode.IDLE;
        using(var form=new ModuleBaseForm {Name="LateUiHost"})
        {
            SynchronizationContext previous=SynchronizationContext.Current;
            using(var context=new WindowsFormsSynchronizationContext())
            {
                SynchronizationContext.SetSynchronizationContext(context);
                try { LatePresentationCore(form, context); }
                finally { SynchronizationContext.SetSynchronizationContext(previous); }
            }
            ModuleManager.ModuleList.Remove(form);
        }
    }

    /// <summary>在明确安装的 UI 同步上下文中制造后台已结束而消息尚未泵送的窗口。</summary>
    private static void LatePresentationCore(ModuleBaseForm form, WindowsFormsSynchronizationContext context)
    {
            DeviceControl panel=Panel(form);int deliveries=0,deliveryThread=0;panel.Received+=(sender,text)=>{deliveries++;deliveryThread=Thread.CurrentThread.ManagedThreadId;};
            Task open=panel.OpenDeviceAsync();Pump(()=>open.IsCompleted);open.GetAwaiter().GetResult();
            var device=(OfflineUiDevice)panel.Device;device.Entered.Reset();device.Release.Reset();device.BlockReceive=true;
            SynchronizationContext.SetSynchronizationContext(context);
            Task read=panel.ReceiveDataAsync();Assert(device.Entered.Wait(1500),"Read did not enter");device.Release.Set();
            Assert(SpinWait.SpinUntil(()=>!DeviceOperations.HasPending,1500),"Physical receive did not end");
            int beforeStop=deliveries;bool alreadyDelivered=read.IsCompleted;long versionBefore=DeviceOperations.StopVersion;
            DeviceOperations.RequestStopAllAsync().GetAwaiter().GetResult();
            Pump(()=>read.IsCompleted);read.GetAwaiter().GetResult();
            Assert(deliveries==0,"Late receive event survived machine stop; before="+beforeStop+", task="+alreadyDelivered+", thread="+deliveryThread+"/"+Thread.CurrentThread.ManagedThreadId+", version="+versionBefore+"/"+DeviceOperations.StopVersion);
    }

    /// <summary>整机停止继续等待清理任务，不把用于解锁旧 IO 的关闭任务再次取消。</summary>
    internal static void CleanupSurvivesStop()
    {
        SysPara.SystemMode=RunMode.STOPPING;
        using(var entered=new ManualResetEventSlim())
        using(var release=new ManualResetEventSlim())
        {
            Task cleanup=DeviceOperations.RunAsync(null,()=>{entered.Set();release.Wait();Flow_Chart.CurrentExecutionToken.ThrowIfCancellationRequested();},allowDuringStop:true);
            Assert(entered.Wait(1500),"Cleanup did not start during stop");
            DeviceOperations.RequestStopAllAsync().GetAwaiter().GetResult();
            Assert(DeviceOperations.HasPending && !cleanup.IsCompleted,"Cleanup occupancy was dropped");
            release.Set();Pump(()=>cleanup.IsCompleted && !DeviceOperations.HasPending);cleanup.GetAwaiter().GetResult();
        }
        SysPara.SystemMode=RunMode.IDLE;
    }

    /// <summary>泵送真实 WinForms 消息，等待异步 UI 操作或心跳。</summary>
    private static void Pump(Func<bool> condition)
    {
        Stopwatch timer=Stopwatch.StartNew();
        while(!condition() && timer.ElapsedMilliseconds<3000){Application.DoEvents();Thread.Sleep(2);}
        Assert(condition(),"UI operation did not complete");
    }

    /// <summary>失败终止离线验证。</summary>
    private static void Assert(bool value,string message){if(!value)throw new InvalidOperationException(message);}
}
