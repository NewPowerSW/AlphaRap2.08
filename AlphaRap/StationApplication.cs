using AlphaRap.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AlphaRap.MES;
using AlphaRap.TaskProcess;
using AlphaRapLibrary;
using NPSDK;
using NPSDK.Motion;

namespace AlphaRap
{
    /// <summary>组装本站的实际页面、工位和旧式输送线行为。</summary>
    internal sealed class StationApplication : MachineApplication
    {
        private readonly AlwaysRunTask background = new AlwaysRunTask();
        private AlwaysRunTasks applicationTasks;
        private readonly object loginAuditSync = new object();
        private Task loginAudit = Task.CompletedTask;
        private readonly object outputSync = new object();
        private readonly Dictionary<NP_Output, bool> pausedOutputs = new Dictionary<NP_Output, bool>();

        internal static StationVisionForm Vision { get; private set; }
        internal static StationIo Io { get; private set; }

        /// <summary>建立由应用持有的工站 IO，页面与工站共享同一个资源目录。</summary>
        internal void BindStationIo(HardwareResources resources)
        {
            if (Io != null) return;
            applicationTasks = new AlwaysRunTasks(typeof(StationApplication).Assembly);
            Io = new StationIo(resources, background.InputNames.Concat(applicationTasks.InputNames));
        }

        /// <summary>在机器配置和配方完成加载后启动应用实现的常驻任务。</summary>
        internal void StartAlwaysRunTasks() { applicationTasks?.Start(Io); }

        /// <summary>关闭应用时先通知自定义刷新取消，保留实际工作占用供关闭流程等待。</summary>
        internal void RequestAlwaysRunStop()
        {
            if (applicationTasks != null) ApplicationTasks.Global.Track("AlwaysRun.Stop", applicationTasks.RequestStop());
        }

        /// <summary>后台扫描停止后解除工站对硬件目录的订阅。</summary>
        internal void ReleaseStationIo()
        {
            applicationTasks?.Dispose();
            applicationTasks = null;
            Io?.Dispose();
            Io = null;
        }
        internal static LENS Mes { get; private set; }

        /// <summary>组装本机页面并启动设备和后台任务。</summary>
        public override void Initialize(Action<string> reportProgress)
        {
            MiddleLayer.Application = this;
            MiddleLayer.InitialProject(reportProgress);
        }

        /// <summary>关闭由本站启动的后台任务和伺服。</summary>
        public override void Shutdown() { MiddleLayer.DisposeProject(); }
        /// <summary>保持消息循环可用，等待实际任务清理完成后再释放界面。</summary>
        public override System.Threading.Tasks.Task ShutdownAsync() { return MiddleLayer.DisposeProjectAsync(); }

        /// <summary>准备设备期间保护当前已保存的 IO。</summary>
        public override bool IsMachineCommandPending => MiddleLayer.MachineCommandPending;

        /// <summary>按本站运行状态暂停流程和输出。</summary>
        public override void PauseMachine() { MiddleLayer.PauseRun(); }

        /// <summary>按本站运行规则停止流程、手动动作和电机。</summary>
        public override void StopMachine() { MiddleLayer.StopRun(); }

        /// <summary>检查本站配方草稿并切换模块和视觉配置。</summary>
        public override bool ChangeRecipe(string path) { return MiddleLayer.OpenRecipe(path); }

        /// <summary>按发生顺序在后台记录登录或注销，使用当时的工号和权限。</summary>
        public override void OnPermissionChanged(PermissionType permission)
        {
            DataForm data = ApplicationHost.Data;
            if (data == null) return;
            string userName = SysPara.UserName;
            string action = (userName == "None" ? "注销" : "登录") + "　工号：" + userName + "　权限：" + permission;
            lock (loginAuditSync)
            {
                loginAudit = loginAudit.ContinueWith(_ =>
                {
                    data.WriteLog("UserLogin", userName, permission.ToString());
                    data.AddOperationLog("UserLoginForm", action, userName, permission);
                }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
                ApplicationTasks.Global.Track("LoginAudit", loginAudit);
            }
        }

        /// <summary>提供本站应用名称。</summary>
        public override string DisplayName { get { return "AlphaRap-SRM"; } }

        /// <summary>创建本站主窗口。</summary>
        public override System.Windows.Forms.Form CreateWindow() { return new MainForm(); }

        /// <summary>用应用的确认弹窗显示框架警告。</summary>
        public override void ShowWarning(string message, string confirmText)
        {
            using (var warning = new Alpha.OptionChoiceForm())
            {
                warning.fnChangeButtonsText(confirmText, confirmText);
                warning.fnSetMessageAndButtons(message, false, true, false);
                warning.ShowDialog();
            }
        }

        /// <summary>创建本站首页及生产看板。</summary>
        public override System.Windows.Forms.Form CreateHomePage() { return new HomeForm(); }

        /// <summary>创建展示本站动态资源的手动页。</summary>
        public override ManualPage CreateManualPage()
        {
            return new ManualForm();
        }

        /// <summary>创建并登记本站的固定日志数据页。</summary>
        public override DataForm CreateDataPage() { return new StationDataForm(); }

        /// <summary>创建保留旧配方视觉表的项目页面。</summary>
        public override VPForm CreateVisionPage() { return Vision = new StationVisionForm(); }

        /// <summary>在通用轴管理页上登记旧配方里的 X1 点位表。</summary>
        public override HardForm CreateHardwarePage()
        {
            HardForm form = base.CreateHardwarePage();
            form.ResourceDefaults = StationIo.LoadDefaults();
            DataTable table = form.RecipeData.Tables.Add("tb_X1_Post");
            table.Columns.Add("X1", typeof(double));
            table.Columns.Add("Annotation", typeof(string));
            return form;
        }

        /// <summary>提供本站旧通信参数的列和默认值。</summary>
        public override Parameter CreateDevicePage()
        {
            Parameter page = base.CreateDevicePage();
            DataTable table = page.SettingData.Tables["MSet"];
            table.Columns.Add("PLCIP", typeof(string)).DefaultValue = "192.168.10.10";
            table.Columns.Add("PLCPort", typeof(int)).DefaultValue = 502;
            table.Columns.Add("ScannIP", typeof(string));
            table.Columns.Add("ScannPort", typeof(int)).DefaultValue = 1024;
            table.Columns.Add("Pressure_COM", typeof(string));
            return page;
        }

        /// <summary>提供本站默认开关，保留旧程序使用的两个兼容列。</summary>
        public override SystemSetting CreateSettingsPage()
        {
            return new SystemSetting(new[] {
                new BooleanSettingDefault("DisScann", "禁用扫码枪", true),
                new BooleanSettingDefault("UseTryRun", "使用空跑", true),
                new BooleanSettingDefault("DisVision", "禁用视觉"),
                new BooleanSettingDefault("DisMes", "禁用 MES"),
                new BooleanSettingDefault("DisDoor", "禁用门禁"),
                new BooleanSettingDefault("DisRecheck", "禁用复检"),
                new BooleanSettingDefault("DisBuzzer", "禁用蜂鸣器"),
                new BooleanSettingDefault("Dis3D", "禁用 3D")
            });
        }

        /// <summary>保留本站原有生产记录列。</summary>
        public override string[] ProductionColumns
        {
            get { return new[] { "Time", "PalletCode", "Driver", "Pressure", "PressureMax", "PressureMin", "Result" }; }
        }

        /// <summary>按原顺序提供三个物理工位。</summary>
        public override IEnumerable<Flow_BaseForm> CreateStations()
        {
            yield return new Gantry();
            yield return new UpConveryor();
            yield return new DownConveryor();
        }

        /// <summary>由项目选择客户接口页面。</summary>
        public override ModuleBaseForm CreateMesPage() { return Mes = new LENS(); }

        /// <summary>执行一次本站的后台状态更新。</summary>
        public override void Scan() { background.AlwaysRun(Io, MiddleLayer.StopForSafetyFault); }

        /// <summary>检查常驻任务登记的运行许可并报警。</summary>
        internal string CheckSafetyBeforeRun() { return background.CheckSafetyBeforeRun(Io); }

        /// <summary>旧流程以本站上料初始化标记决定是否允许生产。</summary>
        public override bool IsLegacyInitialized { get { return StationData.UpConveyorInitialOk; } }

        /// <summary>清除本站的旧式初始化标记。</summary>
        public override void ResetLegacyInitialization() { StationData.UpConveyorInitialOk = false; }

        /// <summary>暂停本站输送线输出并保留首次暂停前的状态。</summary>
        public override void PauseLegacyOutputs()
        {
            StationIo io = Io;
            if (io == null) return;
            lock (outputSync)
                foreach (string key in StationIo.PauseOutputs) PauseOutput(io.ReadyOutput(key));
        }

        /// <summary>暂停单个输出，单点异常不会跳过其他输出。</summary>
        private void PauseOutput(NP_Output output)
        {
            if (output == null) return;
            try
            {
                if (!pausedOutputs.ContainsKey(output)) pausedOutputs.Add(output, output.IsOn());
                output.Off();
            }
            catch (Exception error) { Debug.WriteLine(error); }
        }

        /// <summary>停止后丢弃本站暂停记录。</summary>
        public override void StopLegacyOutputs()
        {
            lock (outputSync) pausedOutputs.Clear();
        }

        /// <summary>恢复原来开启的输出，全部成功后释放快照。</summary>
        public override void ResumeLegacyOutputs()
        {
            lock (outputSync)
            {
                foreach (var output in pausedOutputs)
                    if (output.Value) output.Key.On();
                pausedOutputs.Clear();
            }
        }
    }
}
