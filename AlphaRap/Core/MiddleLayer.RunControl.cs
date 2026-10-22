using System.Threading;
using System.Windows.Forms;
using AlphaRapLibrary;
using NPSDK;
using Alpha;

namespace AlphaRap
{
    partial class MiddleLayer
    {
        public static bool MachineInitialReady { get { return FlowProgramming.FlowPrograms.HasPrograms ? FlowProgramming.FlowPrograms.IsInitialized : SysPara.UpConveyorInitialOk; } }
        /// <summary>确认运行中重新初始化的请求，按当前状态只发送一次初始化指令。</summary>
        public static void Initial()
        {
            if (!FlowProgramming.FlowPrograms.CanStartMachine(true)) return;
            if (FlowProgramming.FlowPrograms.HasPrograms) FlowProgramming.FlowPrograms.BeginBatch();
            if (Alarm.IsError) { ShowResetAlarmWarning(); return; }
            if (SysPara.SystemMode == RunMode.RUN)
            {
                DialogResult choice = MessageBox.Show(
                    LangMsg("MiddleLayer", "msg_Reinitialize", "正在自动生产，确认退出自动生产并执行初始化吗？", "Exit automatic production and initialize?", "¿Salir de producción automática e inicializar?"),
                    LangMsg("MiddleLayer", "msg_InitializeTitle", "初始化", "Initialize", "Inicializar"),
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (choice != DialogResult.OK) return;
            }

            bool paused = SysPara.SystemMode == RunMode.PAUSE;
            SysPara.SystemRun = false;
            SysPara.UpConveyorInitialOk = false;
            SysPara.SystemMode = RunMode.INITIAL;
            if (paused) Alarm.Clear();
            else
            {
                Flow_Module.Module_StopRun();
                Thread.Sleep(500);
            }
            Flow_Module.Module_InitialRun();
        }

        /// <summary>视觉就绪且完成初始化时进入生产；暂停后的恢复保留输送线状态。</summary>
        public static void StartRun()
        {
            if (!FlowProgramming.FlowPrograms.CanStartMachine(false)) return;
            string visionError;
            if (!FlowProgramming.FlowPrograms.HasPrograms && VPF != null && !VPF.CanStartProduction(out visionError))
            {
                MessageBox.Show(visionError, LangMsg("Vision", "msg_NotReady", "视觉尚未就绪", "Vision is not ready", "Visión no está lista"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            AlarmClear();
            Thread.Sleep(200);
            if (Alarm.IsError) { ShowResetAlarmWarning(); return; }
            if (!FlowProgramming.FlowPrograms.HasPrograms && !SysPara.UpConveyorInitialOk)
            {
                MessageBox.Show(LangMsg("MiddleLayer", "msg_InitializeFirst", "请先初始化。", "Initialize the machine first.", "Inicialice primero la máquina."),
                    LangMsg("Common", "msg_WarningTitle", "警告", "Warning", "Aviso"), MessageBoxButtons.OK, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            if (SysPara.SystemRun && SysPara.SystemMode == RunMode.PAUSE)
            {
                AlarmClear();
                SetHightSpeed();
                SysPara.SystemMode = RunMode.RUN;
                Flow_Module.Module_StartRun();
                StartRunCV();
                return;
            }
            if (SysPara.SystemRun || SysPara.SystemMode != RunMode.INITIAL) return;
            SysPara.SystemMode = RunMode.RUN;
            AlarmClear();
            if (!FlowProgramming.FlowPrograms.HasPrograms)
            {
                Flow_Module.Module_StopRun();
                Thread.Sleep(500);
            }
            Flow_Module.Module_StartRun();
            SysPara.SystemRun = true;
        }

        /// <summary>暂停自动或未完成的初始化流程，取消手动动作并记录输送线状态。</summary>
        public static void PauseRun()
        {
            bool pauseConveyor = SysPara.SystemMode == RunMode.RUN
                || (SysPara.SystemMode == RunMode.INITIAL && !MachineInitialReady);
            if (pauseConveyor) SysPara.SystemMode = RunMode.PAUSE;
            Flow_Module.Module_PauseRun();
            if (!FlowProgramming.FlowPrograms.HasPrograms) StopManualRun();
            if (pauseConveyor && !FlowProgramming.FlowPrograms.HasPrograms) StopRunCV();
        }

        /// <summary>退出自动和初始化状态，停止流程、手动动作及所有电机。</summary>
        public static void StopRun()
        {
            SysPara.SystemRun = false;
            SysPara.UpConveyorInitialOk = false;
            SysPara.SystemMode = RunMode.IDLE;
            Flow_Module.Module_StopRun();
            StopManualRun();
            lock (ConveyorSync) ConveyorRunning.Clear();
        }

        /// <summary>取消所有模块的手动任务并停止所有电机。</summary>
        public static void StopManualRun()
        {
            foreach (ModuleBaseForm module in ModuleManager.ModuleList)
                if (module.StopManualTask != null) module.StopManualTask.Cancel();
            StopAllMotor();
        }

        /// <summary>显示必须先清除报警的提示，关闭后释放对话框。</summary>
        private static void ShowResetAlarmWarning()
        {
            using (OptionChoiceForm warning = new OptionChoiceForm())
            {
                string ok = LangMsg("Common", "msg_OK", "确定", "OK", "Aceptar");
                warning.fnChangeButtonsText(ok, ok);
                warning.fnSetMessageAndButtons(LangMsg("MiddleLayer", "msg_ResetAlarmFirst", "请先清除报警！", "Please Reset Alarm Firstly!", "¡Restablezca la alarma primero!"), false, true, false);
                warning.ShowDialog();
            }
        }

        /// <summary>保留应用流程的高速切换入口，具体倍率由应用配置。</summary>
        public static void SetHightSpeed() { }

        /// <summary>保留应用流程的低速切换入口，具体倍率由应用配置。</summary>
        public static void SetLowSpeed() { }
    }
}
