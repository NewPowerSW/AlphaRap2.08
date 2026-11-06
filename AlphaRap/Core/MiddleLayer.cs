using System.Windows.Forms;
using System.Collections.Generic;
using AlphaRap.MenuForm;
using LifeSpan;

namespace AlphaRap
{
    /// <summary>应用的共享入口与状态；组装本机页面、配置与整机命令。</summary>
    partial class MiddleLayer
    {
        public static Form MainF;
        public static SystemForm SystemF;
        public static Form HomeF;
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

        public static MachineApplication Application = new MachineApplication();
        public static FlowControl FlowCtrl;
        public static AlarmRunTask alarmRunTask;

        public static List<string> AlarmList = new List<string>();

    }
}
