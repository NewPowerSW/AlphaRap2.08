using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlphaRapLibrary;
using NPSDK;
using NPSDK.Design;
using NPSDK.Motion;

namespace AlphaRap
{
    partial class MiddleLayer
    {
        private static FlowDesignerForm flowDesigner;
        private static bool designerExitPending;
        internal static bool FlowDesignerRunning
        {
            get
            {
                return flowDesigner != null && !flowDesigner.IsDisposed && flowDesigner.Executing;
            }
        }

        /// <summary>打开真实 SDK 节点设计器，设备列表使用当前已实例化对象。</summary>
        public static void OpenFlowDesigner()
        {
            if (flowDesigner != null && !flowDesigner.IsDisposed)
            {
                flowDesigner.Activate();
                return;
            }

            foreach (var text in FlowDesignerTexts.Defaults)
                LangMsg("FlowDesigner", text.Key, text.Value[0], text.Value[1], text.Value[2]);
            var environment = FlowDesignerDemo.CreateEnvironment();
            foreach (DataRow alarm in AlarmCatalog.Load().Rows)
            {
                string code = Convert.ToString(alarm["AlarID"]).TrimStart('A');
                if (!environment.Registry.AlarmCodes.Contains(code))
                    environment.Registry.AlarmCodes.Add(code);
            }

            RegisterDesignerHardware(environment.Registry);
            environment.Locale = SysPara.LanguageShow.ToString();
            environment.Text = DesignerText;
            environment.RegisterLanguage = root => RegisterAndApplyLanguage(root, "FlowDesigner");
            environment.ChangeLanguage = language =>
            {
                var value = (LanguageType)Enum.Parse(typeof(LanguageType), language);
                if (MainF != null)
                    MainF.SwitchLanguageTo(value);
                else
                    SwitchLanguage(value);
            };
            environment.ExternalPause = () => Alarm.IsError;
            environment.OutputDirectory = Path.GetFullPath(Path.Combine("ProductionData", "FlowChart"));
            environment.DefaultPath = Path.GetFullPath(Path.Combine(string.IsNullOrWhiteSpace(SysPara.SettingDataDirectory) ? "ModuleData/SettingData" : SysPara.SettingDataDirectory, "FlowChartDesigner.xml"));
            environment.CanStart = DesignerStartReason;
            flowDesigner = new FlowDesignerForm(environment);
            LanguageChanged += RefreshDesignerLanguage;
            flowDesigner.FormClosed += (s, e) =>
            {
                LanguageChanged -= RefreshDesignerLanguage;
                flowDesigner = null;
            };
            if (MainF == null)
                flowDesigner.Show();
            else
                flowDesigner.Show(MainF);
        }

        /// <summary>退出主程序前停止设计流程，待设备回包和 worker 结束后重新关闭主窗口。</summary>
        internal static bool FinishDesignerBeforeExit()
        {
            if (!FlowDesignerRunning)
                return true;
            if (designerExitPending)
                return false;
            designerExitPending = true;
            flowDesigner.FormClosed += (s, e) =>
            {
                designerExitPending = false;
                if (MainF != null && !MainF.IsDisposed)
                    MainF.BeginInvoke(new Action(MainF.Close));
            };
            flowDesigner.RequestStop();
            flowDesigner.Close();
            return false;
        }

        /// <summary>读取现有语言包，底稿缺失时按框架机制补齐三语。</summary>
        private static string DesignerText(string key)
        {
            string[] values;
            return FlowDesignerTexts.Defaults.TryGetValue(key, out values) ? LangMsg("FlowDesigner", key, values[0], values[1], values[2]) : key;
        }

        /// <summary>语言刷新保留节点、参数编辑值与本轮结果。</summary>
        private static void RefreshDesignerLanguage(object sender, EventArgs e)
        {
            if (flowDesigner != null && !flowDesigner.IsDisposed)
                flowDesigner.SetLanguageSelection(SysPara.LanguageShow.ToString());
        }

        /// <summary>只允许维护人员在自动生产和初始化以外试运行设计流程。</summary>
        private static string DesignerStartReason()
        {
            if (SysPara.UserPermission != PermissionType.Maintenance && SysPara.UserPermission != PermissionType.Administrator)
                return LangMsg("FlowDesigner", "MaintenanceRequired", "请使用维护或管理员账号。", "A maintenance or administrator account is required.", "Se requiere una cuenta de mantenimiento o administrador.");
            if (SysPara.SystemRun || SysPara.SystemMode == RunMode.RUN || SysPara.SystemMode == RunMode.PAUSE || SysPara.SystemMode == RunMode.INITIAL && !SysPara.UpConveyorInitialOk || Alarm.IsError)
                return LangMsg("FlowDesigner", "MachineBusy", "请先结束自动生产或初始化并清除报警。", "Finish production or initialization and clear alarms first.", "Finalice la producción o inicialización y borre las alarmas primero.");
            return "";
        }

        /// <summary>设备回调持续检查权限及主框架状态，不绕过宿主停机入口。</summary>
        private static void EnsureDesignerReady(Control control = null)
        {
            string reason = DesignerStartReason();
            if (!string.IsNullOrEmpty(reason))
                throw new InvalidOperationException(reason);
            if (control != null && control.IsDisposed)
                throw new ObjectDisposedException(control.Name);
            var hardware = control as ControlBaseInterface;
            if (hardware != null && !hardware.IsInitialized)
                throw new InvalidOperationException(DesignerText("HardwareNotReady"));
        }

        /// <summary>登记运动、IO、扫码和视觉适配；不自动连接任何对象。</summary>
        private static void RegisterDesignerHardware(FlowActionRegistry registry)
        {
            foreach (ControlBaseInterface control in SDKPara.ControlList.ToArray())
            {
                if (control.IsDisposed)
                    continue;
                string id = ControlIdentity(control);
                var motor = control as NP_Motor;
                if (motor != null)
                    RegisterDesignerMotor(registry, id, motor);
                var input = control as NP_Input;
                if (input != null)
                    registry.Add(new FlowActionBinding { MethodName = "IO.Read", TargetId = id, Outputs = new[] { "IsOn" }, Poll = call =>
                    {
                        EnsureDesignerReady(input);
                        return FlowActionSample.Of("IsOn", input.IsOn());
                    } });
                var output = control as NP_Output;
                if (output != null)
                    registry.Add(new FlowActionBinding { MethodName = "IO.Write", TargetId = id, Parameters = new[] { FlowDesignerDemo.Parameter("State", "Boolean", "false") }, Outputs = new[] { "IsOn" }, Start = call =>
                    {
                        EnsureDesignerReady(output);
                        if ((bool)call.Inputs["State"])
                            output.On();
                        else
                            output.Off();
                    }, Poll = call => FlowActionSample.Of("IsOn", output.IsOn()) });
            }

            foreach (var device in DeviceControl.GetFlowDesignerDevices())
            {
                var scanner = device.Value as ScannerKeyenceTcp;
                if (scanner == null)
                    continue;
                registry.Add(new FlowActionBinding { MethodName = "Scanner.Trigger", TargetId = device.Key, Outputs = new[] { "Barcode" }, Start = call => StartDesignerScanner(call, scanner), Poll = PollDesignerScanner });
            }

            if (VPF == null)
                return;
            foreach (var station in VPF.GetFlowDesignerStations())
                RegisterDesignerVision(registry, station.Key, station.Value);
        }

        /// <summary>由窗体和父控件名称组成实例身份，重名绑定会被注册表拒绝。</summary>
        private static string ControlIdentity(Control control)
        {
            string id = control.Name;
            for (Control parent = control.Parent; parent != null; parent = parent.Parent)
                id = parent.Name + "/" + id;
            return id;
        }

        /// <summary>首次发出定位指令，后续只读取位置和忙状态，停止时调用该轴 Stop。</summary>
        private static void RegisterDesignerMotor(FlowActionRegistry registry, string id, NP_Motor motor)
        {
            registry.Add(new FlowActionBinding { MethodName = "Motion.Move", TargetId = id, Parameters = new[] { FlowDesignerDemo.Parameter("Position", "Double", "0"), FlowDesignerDemo.Parameter("Tolerance", "Double", "0.2") }, Outputs = new[] { "Completed", "Position" }, Start = call =>
            {
                EnsureDesignerReady(motor);
                var io = motor.GetAxisIOState();
                if (!io.SVON || io.ALM || io.EMG || io.PEL || io.MEL || motor.GetBusy())
                    throw new InvalidOperationException(DesignerText("AxisNotReady"));
                if ((double)call.Inputs["Tolerance"] <= 0)
                    throw new FlowDesignIssue("InvalidParameter");
                motor.Goto((double)call.Inputs["Position"]);
            }, Poll = call =>
            {
                EnsureDesignerReady(motor);
                var io = motor.GetAxisIOState();
                if (!io.SVON || io.ALM || io.EMG)
                    throw new InvalidOperationException(DesignerText("AxisNotReady"));
                double position = motor.GetEncPos();
                return FlowActionSample.Of("Completed", !motor.GetBusy() && Math.Abs(position - (double)call.Inputs["Position"]) <= (double)call.Inputs["Tolerance"], "Position", position);
            }, Cancel = call => motor.Stop() });
        }

        /// <summary>异步触发已连接扫码枪一次，读取超时仍由设备自身负责。</summary>
        private static void StartDesignerScanner(FlowInvocation call, ScannerKeyenceTcp scanner)
        {
            EnsureDesignerReady();
            if (!scanner.IsConnected || scanner.IsRunning || scanner.TimeOut <= 0 || scanner.TimeOut > 60000)
                throw new InvalidOperationException(DesignerText("ScannerNotReady"));
            call.Pending = Task.Run(() => call.State = scanner.ScanOnce());
        }

        /// <summary>等待该次扫码返回，迟到数据不会被停止后的会话采用。</summary>
        private static FlowActionSample PollDesignerScanner(FlowInvocation call)
        {
            if (!call.Pending.IsCompleted)
                return new FlowActionSample
                {
                    Waiting = true
                };
            call.Pending.GetAwaiter().GetResult();
            string barcode = Convert.ToString(call.State);
            if (barcode == "ERROR")
                throw new InvalidOperationException(DesignerText("ScannerNotReady"));
            return FlowActionSample.Of("Barcode", barcode);
        }

        /// <summary>使用视觉请求编号匹配结果，取消后等待底层任务释放工位。</summary>
        private static void RegisterDesignerVision(FlowActionRegistry registry, string id, VpStation station)
        {
            registry.Add(new FlowActionBinding { MethodName = "Vision.Run", TargetId = id, Outputs = new[] { "X", "Y", "Angle", "Passed" }, Start = call =>
            {
                EnsureDesignerReady();
                long runId;
                if (!station.TryRunTB(out runId))
                    throw new InvalidOperationException(station.LastError);
                call.State = runId;
                call.Pending = Task.Run(() =>
                {
                    while (station.IsBusy)
                        Thread.Sleep(20);
                });
            }, Poll = call =>
            {
                var result = station.LastRunResult;
                if (result == null || result.RunId != (long)call.State)
                    throw new InvalidOperationException(DesignerText("VisionResultChanged"));
                if (result.State == VisionRunState.Running)
                    return new FlowActionSample
                    {
                        Waiting = true
                    };
                if (result.State != VisionRunState.Succeeded && result.State != VisionRunState.Rejected)
                    throw new InvalidOperationException(result.Error);
                return FlowActionSample.Of("X", result.X, "Y", result.Y, "Angle", result.Angle, "Passed", result.State == VisionRunState.Succeeded);
            }, Cancel = call => station.CancelRun((long)call.State) });
        }
    }
}
