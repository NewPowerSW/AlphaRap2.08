using System.Collections.Generic;
using AlphaRap.MES;
using AlphaRap.MenuForm;
using AlphaRap.TaskProcess;
using LifeSpan;

namespace AlphaRap
{
    /// <summary>应用的共享入口与状态；行为按启动、运行、模块、运动、配方、报警和语言分文件维护。</summary>
    partial class MiddleLayer
    {
        public static MainForm MainF;
        public static SystemForm SystemF;
        public static HomeForm HomeF;
        public static ManualForm ManualF;
        public static HardForm HardF;
        public static CheckForm CheckF;
        public static LogForm LogF;
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
        public static LENS MesF;
        public static LoadingForm LoadingF;

        public static UpConveryor UpConveyorF;
        public static DownConveryor DownConveyorF;
        public static Gantry GantryF;
        public static FlowControl FlowCtrl;
        public static AlwaysRunTask alTask;
        public static AlarmRunTask alarmRunTask;

        public static List<dynamic> lstForm = new List<dynamic>();
        public static List<string> AlarmList = new List<string>();
        public static AtlasLibrary.MTF6000 Atlas_MTF6000 = new AtlasLibrary.MTF6000();

        public static int LoadProcessRate = 0;
        public static int iTaskStep = 0;
        public static bool bTask = true;
        public static bool bTaskFlowOK = false;
        public static bool bTaskFlowOK2 = false;
        public static bool bConveyorTaskOK = false;
        public static volatile bool gEXIT = false;
    }
}
