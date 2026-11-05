using System.Collections.Generic;
using AlphaRap.MenuForm;
using LifeSpan;

namespace AlphaRap
{
    /// <summary>应用的共享入口与状态；行为按启动、运行、模块、运动、配方、报警和语言分文件维护。</summary>
    partial class MiddleLayer
    {
        public static MainForm MainF;
        public static SystemForm SystemF;
        public static HomeForm HomeF;
        public static ManualPage ManualF;
        public static HardForm HardF;
        public static CheckForm CheckF;
        public static DataForm DataF;
        public static Parameter ParF;
        public static SystemSetting SystemS;
        public static PlatFormSetting PlatF;
        public static AddUserForm AddF;
        public static VPForm VPF;
        public static ProductManagerForm ProductF;
        public static LifeSpanForm SpanLifeF;
        public static Flow FlowF;
        public static LockForm1 LockForm1;
        public static AlphaRapLibrary.ModuleBaseForm MesF;
        public static LoadingForm LoadingF;

        public static MachineApplication Application = new MachineApplication();
        public static FlowControl FlowCtrl;
        public static AlarmRunTask alarmRunTask;

        public static List<dynamic> lstForm = new List<dynamic>();
        public static List<string> AlarmList = new List<string>();

    }
}
