using System;
using NPSDK;
using Input = AlphaRap.TaskProcess.DownConveyorInput;
using Output = AlphaRap.TaskProcess.DownConveyorOutput;
using Result = NPSDK.Flow_Chart.ResultType;

namespace AlphaRap.TaskProcess
{
    public partial class DownConveryor : Flow_BaseForm
    {
        private readonly Func<DownConveyorProcess> getProcess;
        private DownConveyorProcess Process { get { return getProcess(); } }

        /// <summary>创建下料流程监控页，保留设计器入口。</summary>
        public DownConveryor()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
            UiTheme.ApplyDpi(this);
        }

        /// <summary>将现有流程节点接入应用提供的下料进程。</summary>
        internal DownConveryor(Func<DownConveyorProcess> getProcess) : this()
        {
            this.getProcess = getProcess;
        }

        /// <summary>由初始化具体节点处理动作。</summary>
        public override void Initial() { }

        /// <summary>由运行器管理流程暂停。</summary>
        public override void PauseRun() { }

        /// <summary>由自动流程具体节点处理动作。</summary>
        public override void StartRun() { }

        /// <summary>停止下料输出并撤销未交接的记录。</summary>
        public override void StopRun()
        {
            getProcess?.Invoke()?.Stop();
        }

        /// <summary>把工站输出动作转换为流程继续。</summary>
        private Result SetOutput(Output output, bool active)
        {
            Process.Set(output, active);
            return Result.NEXT;
        }

        /// <summary>把工站信号等待转换为节点继续或停留。</summary>
        private Result WaitFor(Input input, bool expected = true)
        {
            return Process.WaitFor(input, expected) ? Result.NEXT : Result.IDLE;
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
            Process.Set(Output.Reverse, false);
            Process.Set(Output.Forward, false);
            return Result.NEXT;
        }

        /// <summary>升起挡停并等待到位。</summary>
        private Result DownConveyInit_Flow1_3_FlowChartRun() { return Process.MoveStopper(true) ? Result.NEXT : Result.IDLE; }

        /// <summary>启动流道完成初始化排料。</summary>
        private Result DownConveyInit_Flow1_4_FlowChartRun() { return SetOutput(Output.Forward, true); }

        /// <summary>排料时间到后停止流道。</summary>
        private Result DownConveyInit_Flow1_5_FlowChartRun()
        {
            if (DownConveyInit_Flow1_5.FlowChart_ElapsedMilliseconds <= 3000) return Result.IDLE;
            return SetOutput(Output.Forward, false);
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
            bool? board = Process.BoardAtStop();
            if (!board.HasValue) return Result.IDLE;
            Process.BeginTransfer(SysPara.RecipeName);
            return board.Value ? Result.CASE1 : Result.NEXT;
        }

        /// <summary>升起挡停并沿用原到位报警。</summary>
        private Result DownConveyorAuto_Flow1_4_FlowChartRun()
        {
            if (Process.MoveStopper(true)) return Result.NEXT;
            return WaitWithAlarm(DownConveyorAuto_Flow1_4, 5000, "4");
        }

        /// <summary>向上游发出本机可接板信号。</summary>
        private Result DownConveyorAuto_Flow1_5_FlowChartRun() { return SetOutput(Output.ReadyForBoard, true); }

        /// <summary>等待上游有板。</summary>
        private Result DownConveyorAuto_Flow1_6_FlowChartRun() { return WaitFor(Input.UpstreamAvailable); }

        /// <summary>开始接板输送。</summary>
        private Result DownConveyorAuto_Flow1_7_FlowChartRun() { return SetOutput(Output.Forward, true); }

        /// <summary>等待板件触发入料感应器。</summary>
        private Result DownConveyorAuto_Flow1_8_FlowChartRun() { return WaitFor(Input.BoardIn); }

        /// <summary>等待板件离开入料感应器。</summary>
        private Result DownConveyorAuto_Flow1_9_FlowChartRun() { return WaitFor(Input.BoardIn, false); }

        /// <summary>撤销本机可接板信号。</summary>
        private Result DownConveyorAuto_Flow2_0_FlowChartRun() { return SetOutput(Output.ReadyForBoard, false); }

        /// <summary>采样减速感应器并按现有工艺进入换速。</summary>
        private Result DownConveyorAuto_Flow2_1_FlowChartRun()
        {
            Process.SampleSlowdown();
            return Result.NEXT;
        }

        /// <summary>切换流道速度。</summary>
        private Result DownConveyorAuto_Flow2_2_FlowChartRun() { return SetOutput(Output.SlowSpeed, true); }

        /// <summary>等待板件到达挡停位置。</summary>
        private Result DownConveyorAuto_Flow2_3_FlowChartRun() { return WaitFor(Input.BoardAtStop); }

        /// <summary>板件到达后停止输送。</summary>
        private Result DownConveyorAuto_Flow2_4_FlowChartRun()
        {
            Process.StopTransport();
            return Result.NEXT;
        }

        /// <summary>已有板件时进入出板分支。</summary>
        private Result DownConveyorAuto_Flow1_3_FlowChartRun() { return Result.NEXT; }

        /// <summary>向下游发出本机有板信号。</summary>
        private Result DownConveyorAuto_Flow2_5_FlowChartRun() { return SetOutput(Output.BoardAvailable, true); }

        /// <summary>等待下游准备接板。</summary>
        private Result DownConveyorAuto_Flow2_6_FlowChartRun() { return WaitFor(Input.DownstreamReady); }

        /// <summary>降下挡停并沿用原到位报警。</summary>
        private Result DownConveyorAuto_Flow2_7_FlowChartRun()
        {
            if (Process.MoveStopper(false)) return Result.NEXT;
            return WaitWithAlarm(DownConveyorAuto_Flow2_7, 5000, "11");
        }

        /// <summary>开始出板输送。</summary>
        private Result DownConveyorAuto_Flow2_8_FlowChartRun() { return SetOutput(Output.Forward, true); }

        /// <summary>等待板件触发出料感应器。</summary>
        private Result DownConveyorAuto_Flow2_9_FlowChartRun() { return WaitFor(Input.BoardOut); }

        /// <summary>等待板件离开出料感应器，超过原时限时报错。</summary>
        private Result DownConveyorAuto_Flow3_0_FlowChartRun()
        {
            if (Process.WaitFor(Input.BoardOut, false)) return Result.NEXT;
            return WaitWithAlarm(DownConveyorAuto_Flow3_0, 8000, "12");
        }

        /// <summary>等待下游撤销接板信号。</summary>
        private Result DownConveyorAuto_Flow3_1_FlowChartRun() { return WaitFor(Input.DownstreamReady, false); }

        /// <summary>交接后停止流道。</summary>
        private Result DownConveyorAuto_Flow3_2_FlowChartRun() { return SetOutput(Output.Forward, false); }

        /// <summary>结束本次板件交接。</summary>
        private Result DownConveyorAuto_Flow3_3_FlowChartRun()
        {
            Process.CompleteTransfer(DateTime.Now);
            return Result.NEXT;
        }

        /// <summary>进入自动循环尾部。</summary>
        private Result DownConveyorAuto_Flow3_4_FlowChartRun() { return Result.NEXT; }

        /// <summary>返回自动循环入口。</summary>
        private Result DownConveyorAuto_Flow3_5_FlowChartRun() { return Result.NEXT; }

        /// <summary>继续执行现有流程节点。</summary>
        private Result flow_Chart1_FlowChartRun() { return Result.NEXT; }
    }
}
