using AlphaRap.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
        private readonly object outputSync = new object();
        private readonly Dictionary<NP_Output, bool> pausedOutputs = new Dictionary<NP_Output, bool>();

        internal static ManualForm Manual { get; private set; }
        internal static StationVisionForm Vision { get; private set; }
        internal static LENS Mes { get; private set; }

        /// <summary>提供本站应用名称。</summary>
        public override string DisplayName { get { return "AlphaRap-SRM"; } }

        /// <summary>创建本站主窗口。</summary>
        public override System.Windows.Forms.Form CreateWindow() { return new MainForm(); }

        /// <summary>创建本站首页及生产看板。</summary>
        public override System.Windows.Forms.Form CreateHomePage() { return new HomeForm(); }

        /// <summary>创建保留本站固定 IO 的手动页。</summary>
        public override ManualPage CreateManualPage() { return Manual = new ManualForm(); }

        /// <summary>创建保留旧配方视觉表的项目页面。</summary>
        public override VPForm CreateVisionPage() { return Vision = new StationVisionForm(); }

        /// <summary>在通用轴管理页上登记旧配方里的 X1 点位表。</summary>
        public override HardForm CreateHardwarePage()
        {
            HardForm form = base.CreateHardwarePage();
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
                new BooleanSettingDefault("DisScann", "禁用扫码枪", "Disable scanner", "Desactivar escáner", true),
                new BooleanSettingDefault("UseTryRun", "使用空跑", "Dry run", "Ejecución en vacío", true),
                new BooleanSettingDefault("DisVision", "禁用视觉", "Disable vision", "Desactivar visión"),
                new BooleanSettingDefault("DisMes", "禁用 MES", "Disable MES", "Desactivar MES"),
                new BooleanSettingDefault("DisDoor", "禁用门禁", "Disable door check", "Desactivar control de puerta"),
                new BooleanSettingDefault("DisRecheck", "禁用复检", "Disable recheck", "Desactivar reinspección"),
                new BooleanSettingDefault("DisBuzzer", "禁用蜂鸣器", "Disable buzzer", "Desactivar zumbador"),
                new BooleanSettingDefault("Dis3D", "禁用 3D", "Disable 3D", "Desactivar 3D")
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
        public override void Scan() { background.AlwaysRun(); }

        /// <summary>旧流程以本站上料初始化标记决定是否允许生产。</summary>
        public override bool IsLegacyInitialized { get { return StationData.UpConveyorInitialOk; } }

        /// <summary>清除本站的旧式初始化标记。</summary>
        public override void ResetLegacyInitialization() { StationData.UpConveyorInitialOk = false; }

        /// <summary>暂停本站输送线输出并保留首次暂停前的状态。</summary>
        public override void PauseLegacyOutputs()
        {
            if (Manual == null) return;
            lock (outputSync)
                foreach (NP_Output output in Manual.Conveyor_List) PauseOutput(output);
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
