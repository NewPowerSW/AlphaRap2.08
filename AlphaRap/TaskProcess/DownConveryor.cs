using System;
using System.Collections.Generic;
using AlphaRap.FlowProgramming;
using NPSDK;
using Result = NPSDK.Flow_Chart.ResultType;

namespace AlphaRap.TaskProcess
{
    public partial class DownConveryor : Flow_BaseForm, ICompiledFlowEntries
    {
        /// <summary>登记工站的初始化首节点。</summary>
        IReadOnlyList<Flow_Chart> ICompiledFlowEntries.InitialNodes => new[] { DownConveyInit_Flow1_1 };

        /// <summary>登记生产启动使用的下料入口。</summary>
        IReadOnlyList<Flow_Chart> ICompiledFlowEntries.AutoNodes => new[] { DownConveyInit_Flow1_1 };

        /// <summary>创建下料流程监控页并加载设计器连线。</summary>
        public DownConveryor()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
            UiTheme.ApplyDpi(this);
        }

        /// <summary>由初始化节点执行下料准备。</summary>
        public override void Initial() { }

        /// <summary>由运行器暂停下料流程。</summary>
        public override void PauseRun() { }

        /// <summary>由自动节点执行下料动作。</summary>
        public override void StartRun() { }

        /// <summary>逐项关闭下料输送及握手输出，汇报关闭失败的点位。</summary>
        public override void StopRun()
        {
            StationIo io = StationApplication.Io;
            if (io == null) return;

            string[] outputs =
            {
                "OB_DownConveyor_SpeedSwitch",
                "OB_DownConveyor_Forward",
                "OB_DownConveyor_LocalMachineAvailable_SMEMA",
                "OB_DownConveyor_LocalMachineReady_SMEMA"
            };
            var errors = new List<Exception>();
            foreach (string output in outputs)
            {
                try { io.Output(output).Off(); }
                catch (Exception error) { errors.Add(error); }
            }

            if (errors.Count > 0) throw new AggregateException(errors);
        }

        /// <summary>等待超时后沿用节点报警编号和工位位置。</summary>
        private static Result WaitWithAlarm(Flow_Chart node, int timeoutMs, string code)
        {
            if (node.FlowChart_ElapsedMilliseconds <= timeoutMs) return Result.IDLE;
            ApplicationHost.Data.AddLogError("Alarm:" + ApplicationHost.GetAlarmText(code) + "- Position:" + node.text);
            Alarm.Show(code);
            return Result.IDLE;
        }

        /// <summary>清除下料初始化完成标志。</summary>
        private Result DownConveyInit_Flow1_1_FlowChartRun()
        {
            StationData.DownConveyorInitialOk = false;
            return Result.NEXT;
        }

        /// <summary>初始化前关闭流道正反转。</summary>
        private Result DownConveyInit_Flow1_2_FlowChartRun()
        {
            StationApplication.Io.Output("OB_DownConveyor_Reverse").Off();
            StationApplication.Io.Output("OB_DownConveyor_Forward").Off();
            return Result.NEXT;
        }

        /// <summary>升起挡停并等待到位。</summary>
        private Result DownConveyInit_Flow1_3_FlowChartRun()
        {
            StationApplication.Io.Cylinder("OB_DownConveyor_Stop_Cylinder").On();
            return StationApplication.Io.Read("IB_DownConveyor_StopUp_Sensor") == true ? Result.NEXT : Result.IDLE;
        }

        /// <summary>启动流道完成初始化排料。</summary>
        private Result DownConveyInit_Flow1_4_FlowChartRun()
        {
            StationApplication.Io.Output("OB_DownConveyor_Forward").On();
            return Result.NEXT;
        }

        /// <summary>排料时间到后停止流道。</summary>
        private Result DownConveyInit_Flow1_5_FlowChartRun()
        {
            if (DownConveyInit_Flow1_5.FlowChart_ElapsedMilliseconds <= 3000) return Result.IDLE;
            StationApplication.Io.Output("OB_DownConveyor_Forward").Off();
            return Result.NEXT;
        }

        /// <summary>登记下料初始化成功。</summary>
        private Result DownConveyInit_Flow1_6_FlowChartRun()
        {
            StationData.DownConveyorInitialOk = true;
            return Result.NEXT;
        }

        /// <summary>进入新一轮自动下料。</summary>
        private Result DownConveyorAuto_Flow1_1_FlowChartRun() { return Result.NEXT; }

        /// <summary>按挡停位置是否已有板件选择接板或出板路径。</summary>
        private Result DownConveyorAuto_Flow1_2_FlowChartRun()
        {
            bool? board = StationApplication.Io.Read("IB_DownConveyor_BoardStop_Sensor");
            if (!board.HasValue) return Result.IDLE;
            return board.Value ? Result.CASE1 : Result.NEXT;
        }

        /// <summary>升起挡停并沿用原到位报警。</summary>
        private Result DownConveyorAuto_Flow1_4_FlowChartRun()
        {
            StationApplication.Io.Cylinder("OB_DownConveyor_Stop_Cylinder").On();
            if (StationApplication.Io.Read("IB_DownConveyor_StopUp_Sensor") == true) return Result.NEXT;
            return WaitWithAlarm(DownConveyorAuto_Flow1_4, 5000, "4");
        }

        /// <summary>向上游发出本机可接板信号。</summary>
        private Result DownConveyorAuto_Flow1_5_FlowChartRun()
        {
            StationApplication.Io.Output("OB_DownConveyor_LocalMachineReady_SMEMA").On();
            return Result.NEXT;
        }

        /// <summary>等待上游有板。</summary>
        private Result DownConveyorAuto_Flow1_6_FlowChartRun()
        {
            return StationApplication.Io.Read("IB_DownConveyor_UpMachineAvailable_SMEMA") == true ? Result.NEXT : Result.IDLE;
        }

        /// <summary>开始接板输送。</summary>
        private Result DownConveyorAuto_Flow1_7_FlowChartRun()
        {
            StationApplication.Io.Output("OB_DownConveyor_Forward").On();
            return Result.NEXT;
        }

        /// <summary>等待板件触发入料感应器。</summary>
        private Result DownConveyorAuto_Flow1_8_FlowChartRun()
        {
            return StationApplication.Io.Read("IB_DownConveyor_BoardIn_Sensor") == true ? Result.NEXT : Result.IDLE;
        }

        /// <summary>等待板件离开入料感应器。</summary>
        private Result DownConveyorAuto_Flow1_9_FlowChartRun()
        {
            return StationApplication.Io.Read("IB_DownConveyor_BoardIn_Sensor") == false ? Result.NEXT : Result.IDLE;
        }

        /// <summary>撤销本机可接板信号。</summary>
        private Result DownConveyorAuto_Flow2_0_FlowChartRun()
        {
            StationApplication.Io.Output("OB_DownConveyor_LocalMachineReady_SMEMA").Off();
            return Result.NEXT;
        }

        /// <summary>采样减速感应器并按现有工艺进入换速。</summary>
        private Result DownConveyorAuto_Flow2_1_FlowChartRun()
        {
            StationApplication.Io.Read("IB_DownConveyor_BoardSpeedSwitch_Sensor");
            return Result.NEXT;
        }

        /// <summary>切换流道速度。</summary>
        private Result DownConveyorAuto_Flow2_2_FlowChartRun()
        {
            StationApplication.Io.Output("OB_DownConveyor_SpeedSwitch").On();
            return Result.NEXT;
        }

        /// <summary>等待板件到达挡停位置。</summary>
        private Result DownConveyorAuto_Flow2_3_FlowChartRun()
        {
            return StationApplication.Io.Read("IB_DownConveyor_BoardStop_Sensor") == true ? Result.NEXT : Result.IDLE;
        }

        /// <summary>板件到达后停止输送。</summary>
        private Result DownConveyorAuto_Flow2_4_FlowChartRun()
        {
            StationApplication.Io.Output("OB_DownConveyor_SpeedSwitch").Off();
            StationApplication.Io.Output("OB_DownConveyor_Forward").Off();
            return Result.NEXT;
        }

        /// <summary>已有板件时进入出板分支。</summary>
        private Result DownConveyorAuto_Flow1_3_FlowChartRun() { return Result.NEXT; }

        /// <summary>向下游发出本机有板信号。</summary>
        private Result DownConveyorAuto_Flow2_5_FlowChartRun()
        {
            StationApplication.Io.Output("OB_DownConveyor_LocalMachineAvailable_SMEMA").On();
            return Result.NEXT;
        }

        /// <summary>等待下游准备接板。</summary>
        private Result DownConveyorAuto_Flow2_6_FlowChartRun()
        {
            return StationApplication.Io.Read("IB_DownConveyor_DownMachineReady_SMEMA") == true ? Result.NEXT : Result.IDLE;
        }

        /// <summary>降下挡停并沿用原到位报警。</summary>
        private Result DownConveyorAuto_Flow2_7_FlowChartRun()
        {
            StationApplication.Io.Cylinder("OB_DownConveyor_Stop_Cylinder").Off();
            if (StationApplication.Io.Read("IB_DownConveyor_StopDown_Sensor") == true) return Result.NEXT;
            return WaitWithAlarm(DownConveyorAuto_Flow2_7, 5000, "11");
        }

        /// <summary>开始出板输送。</summary>
        private Result DownConveyorAuto_Flow2_8_FlowChartRun()
        {
            StationApplication.Io.Output("OB_DownConveyor_Forward").On();
            return Result.NEXT;
        }

        /// <summary>等待板件触发出料感应器。</summary>
        private Result DownConveyorAuto_Flow2_9_FlowChartRun()
        {
            return StationApplication.Io.Read("IB_DownConveyor_BoardOut_Sensor") == true ? Result.NEXT : Result.IDLE;
        }

        /// <summary>等待板件离开出料感应器，超过原时限时报错。</summary>
        private Result DownConveyorAuto_Flow3_0_FlowChartRun()
        {
            if (StationApplication.Io.Read("IB_DownConveyor_BoardOut_Sensor") == false) return Result.NEXT;
            return WaitWithAlarm(DownConveyorAuto_Flow3_0, 8000, "12");
        }

        /// <summary>等待下游撤销接板信号。</summary>
        private Result DownConveyorAuto_Flow3_1_FlowChartRun()
        {
            return StationApplication.Io.Read("IB_DownConveyor_DownMachineReady_SMEMA") == false ? Result.NEXT : Result.IDLE;
        }

        /// <summary>交接后停止流道。</summary>
        private Result DownConveyorAuto_Flow3_2_FlowChartRun()
        {
            StationApplication.Io.Output("OB_DownConveyor_Forward").Off();
            return Result.NEXT;
        }

        /// <summary>交接结束后继续自动循环。</summary>
        private Result DownConveyorAuto_Flow3_3_FlowChartRun() { return Result.NEXT; }

        /// <summary>进入自动循环尾部。</summary>
        private Result DownConveyorAuto_Flow3_4_FlowChartRun() { return Result.NEXT; }

        /// <summary>返回自动循环入口。</summary>
        private Result DownConveyorAuto_Flow3_5_FlowChartRun() { return Result.NEXT; }

        /// <summary>继续执行现有流程节点。</summary>
        private Result flow_Chart1_FlowChartRun() { return Result.NEXT; }
    }
}
