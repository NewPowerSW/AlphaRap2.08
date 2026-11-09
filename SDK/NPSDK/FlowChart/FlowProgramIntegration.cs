using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace NPSDK
{
    public enum FlowProgramCommand { Initialize, Start, Pause, Stop, Resume }

    public interface IFlowProgramController
    {
        bool IsEnabled { get; }
        /// <summary>处理已启用的工位工程命令；未启用时返回 false，保留原流程。</summary>
        bool Handle(FlowProgramCommand command);
    }

    /// <summary>提供实际执行任务状态，供 SDK 汇总运行状态；原控制器接口保持兼容。</summary>
    public interface IFlowProgramStatus
    {
        bool IsRunning { get; }
    }

    public partial class Flow_BaseForm
    {
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IFlowProgramController ProgramController { get; set; }

        /// <summary>将生命周期命令交给当前工位的可配置流程。</summary>
        public bool TryProgramCommand(FlowProgramCommand command)
        {
            EnsureProgramController();
            return ProgramController != null && ProgramController.Handle(command);
        }

        /// <summary>通过全局工厂接入所有派生窗体，不依赖具体工位类型。</summary>
        public void EnsureProgramController()
        {
            if (ProgramController != null || LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;
            if (Flow_Module.ProgramControllerFactory == null) return;
            ProgramController = Flow_Module.ProgramControllerFactory(this);
        }

        /// <summary>派生窗体完成初始化后自动安装统一流程控制器。</summary>
        protected override void OnLoad(EventArgs e)
        {
            EnsureProgramController();
            base.OnLoad(e);
        }
    }

    public static partial class Flow_Module
    {
        public static Func<Flow_BaseForm, IFlowProgramController> ProgramControllerFactory { get; set; }

        /// <summary>已接管的工位以实际任务为准，其他工位保留原运行标记。</summary>
        public static bool IsExecutionActive(Flow_BaseForm module)
        {
            if (module == null) return false;
            IFlowProgramStatus status = module.ProgramController as IFlowProgramStatus;
            if (status != null && module.ProgramController.IsEnabled) return status.IsRunning;
            return !module.IsDisposed && module.RunningStatus;
        }

        /// <summary>真实旧线程始终保持忙碌；托管节点忽略过期标记，跟随所属工位任务。</summary>
        public static bool IsExecutionActive(Flow_Chart node)
        {
            if (node == null) return false;
            if (node.IsLegacyRunning) return true;
            Flow_BaseForm owner = null;
            for (Control parent = node.Parent; parent != null; parent = parent.Parent)
                if (parent is Flow_BaseForm) { owner = (Flow_BaseForm)parent; break; }
            if (node.ProgramManaged && owner != null && owner.ProgramController is IFlowProgramStatus && owner.ProgramController.IsEnabled)
                return ((IFlowProgramStatus)owner.ProgramController).IsRunning;
            return !node.IsDisposed && node.RunningStatus;
        }

    }

    public partial class Flow_Chart
    {
        private bool programHooksAttached;
        private bool programDragging;
        private bool programPointerDown;
        private Point programMouseOrigin;
        private Point programNodeOrigin;
        private int programLastClick;
        private bool programBreakpoint;
        private bool programBreakpointPaintAttached;
        private string programBindingLabel = "";
        private string programStatusMessage = "";
        private string programElapsedText = "";
        private bool programTimingStarted;
        private int programStateRevision;
        public event EventHandler ConfigurationRequested;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ProgramEditing { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ProgramManaged { get; set; }

        /// <summary>原生后台工作器或旧异步任务尚未退出时返回 true，不依赖历史运行标记。</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsLegacyRunning { get { return work.IsBusy || task != null && !task.IsCompleted; } }


        /// <summary>在原有状态标签显示执行方式，不改变业务标题或节点绑定。</summary>
        [Category("Appearance"), DefaultValue("")]
        public string ProgramBindingLabel
        {
            get { return programBindingLabel; }
            set
            {
                programBindingLabel = value ?? "";
                UpdateProgramLabel();
            }
        }

        /// <summary>业务执行状态与固定执行方式同时显示，避免状态刷新擦除类型标记。</summary>
        private void SetProgramMessage(string message)
        {
            programStatusMessage = message ?? "";
            UpdateProgramLabel();
        }

        /// <summary>按当前状态和耗时预留标题空间，空闲节点保持标题居中。</summary>
        private void UpdateProgramLabel()
        {
            if (IsDisposed) return;
            if (IsHandleCreated && InvokeRequired) { BeginInvoke(new Action(UpdateProgramLabel)); return; }
            label1.Text = programBindingLabel.Length == 0 ? programStatusMessage
                : programStatusMessage.Length == 0 ? programBindingLabel : programBindingLabel + " · " + programStatusMessage;
            bool hasHeader = label1.Text.Length > 0 || programElapsedText.Length > 0;
            button1.Padding = new Padding(0, hasHeader ? label1.Font.Height + 3 : 0, 0, 0);
            button1.Invalidate();
        }

        /// <summary>在按钮表面动态绘制状态和右上角耗时，按文字宽度排列并避开断点。</summary>
        private void DrawProgramBinding(object sender, PaintEventArgs args)
        {
            if (label1 == null) return;
            const TextFormatFlags flags = TextFormatFlags.Top | TextFormatFlags.SingleLine
                | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;
            int right = button1.ClientSize.Width - (programBreakpoint ? 16 : 4);
            int available = Math.Max(0, right - 3);
            if (available == 0) return;
            int timeWidth = programElapsedText.Length == 0 ? 0 : Math.Min(available,
                TextRenderer.MeasureText(args.Graphics, programElapsedText, label1.Font, Size.Empty, flags).Width);
            int statusWidth = Math.Max(0, available - timeWidth - (timeWidth > 0 ? 5 : 0));
            Rectangle area = new Rectangle(3, 3, statusWidth, label1.PreferredHeight);
            TextRenderer.DrawText(args.Graphics, label1.Text, label1.Font, area, label1.ForeColor,
                flags | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
            Rectangle timeArea = new Rectangle(right - timeWidth, 3, timeWidth, label1.PreferredHeight);
            TextRenderer.DrawText(args.Graphics, programElapsedText, label1.Font, timeArea, label1.ForeColor,
                flags | TextFormatFlags.Right | TextFormatFlags.EndEllipsis);
        }

        /// <summary>在 UI 线程更新耗时文字，原生流程和托管流程使用相同的右上角布局。</summary>
        private void SetProgramElapsedTime(string text)
        {
            if (IsDisposed) return;
            if (IsHandleCreated && InvokeRequired) { BeginInvoke(new Action(() => SetProgramElapsedTime(text))); return; }
            if (programElapsedText == text) return;
            programElapsedText = text;
            UpdateProgramLabel();
        }

        /// <summary>开始一次节点执行，IDLE 轮询继续读取同一计时器。</summary>
        public void BeginProgramTiming() { lock (Elapsedtimer) { programTimingStarted = true; Elapsedtimer.Restart(); } RefreshProgramTiming(); }

        /// <summary>暂停或结束节点计时，保留当前尝试的耗时供界面查看。</summary>
        public void PauseProgramTiming() { PauseProgramTiming(false); }

        /// <summary>冻结节点计时；暂停归零时保留时间字段，继续后从零累计。</summary>
        public void PauseProgramTiming(bool reset)
        {
            lock (Elapsedtimer) { Elapsedtimer.Stop(); if (reset) Elapsedtimer.Reset(); }
            RefreshProgramTiming();
        }

        /// <summary>普通暂停恢复后继续累计本次节点的执行时间。</summary>
        public void ResumeProgramTiming() { lock (Elapsedtimer) if (programTimingStarted) Elapsedtimer.Start(); }

        /// <summary>清除上次执行显示，不把运行耗时写入流程配置。</summary>
        public void ResetProgramTiming() { lock (Elapsedtimer) { programTimingStarted = false; Elapsedtimer.Reset(); } RefreshProgramTiming(); }

        /// <summary>在 UI 线程刷新右上角秒数，共用原生事件读取的节点计时器。</summary>
        public void RefreshProgramTiming()
        {
            if (IsDisposed) return;
            if (IsHandleCreated && InvokeRequired) { BeginInvoke(new Action(RefreshProgramTiming)); return; }
            string text;
            lock (Elapsedtimer) text = programTimingStarted ? (Elapsedtimer.ElapsedMilliseconds / 1000.0).ToString("F2", CultureInfo.InvariantCulture) : "";
            SetProgramElapsedTime(text);
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ProgramBreakpoint
        {
            get { return programBreakpoint; }
            set
            {
                if (!programBreakpointPaintAttached)
                {
                    Paint += DrawProgramBreakpoint;
                    button1.Paint += DrawProgramBreakpoint;
                    programBreakpointPaintAttached = true;
                }
                if (programBreakpoint == value) return;
                programBreakpoint = value;
                Invalidate(true);
            }
        }

        /// <summary>在节点原有表面绘制断点标记，不改变标题、子控件和设计器序列化。</summary>
        private void DrawProgramBreakpoint(object sender, PaintEventArgs args)
        {
            if (!programBreakpoint) return;
            Control surface = (Control)sender;
            Rectangle marker = new Rectangle(Math.Max(1, surface.ClientSize.Width - 12), 3, 8, 8);
            args.Graphics.FillEllipse(Brushes.Red, marker);
            args.Graphics.DrawEllipse(Pens.White, marker);
        }

        /// <summary>给节点及其内部表面安装拖动和双击配置交互。</summary>
        public void EnableProgramEditor(bool enabled)
        {
            ProgramEditing = enabled;
            if (!enabled) { programPointerDown = false; Capture = false; programLastClick = 0; }
            if (programHooksAttached) return;
            programHooksAttached = true;
            MouseCaptureChanged += (sender, args) => { if (!Capture) programPointerDown = false; };
            HookProgramSurface(this);
        }

        /// <summary>递归绑定鼠标表面；不创建线程或设备对象。</summary>
        private void HookProgramSurface(Control surface)
        {
            surface.MouseDown += ProgramMouseDown;
            surface.MouseMove += ProgramMouseMove;
            surface.MouseUp += ProgramMouseUp;
            foreach (Control child in surface.Controls) HookProgramSurface(child);
        }

        /// <summary>记录鼠标和节点坐标，仅允许编辑模式下左键操作。</summary>
        private void ProgramMouseDown(object sender, MouseEventArgs e)
        {
            if (!ProgramEditing || e.Button != MouseButtons.Left) return;
            programDragging = false;
            programPointerDown = true;
            programMouseOrigin = ((Control)sender).PointToScreen(e.Location);
            programNodeOrigin = Location;
            Capture = true;
        }

        /// <summary>超过系统拖动阈值后移动节点。</summary>
        private void ProgramMouseMove(object sender, MouseEventArgs e)
        {
            if (!ProgramEditing || !programPointerDown || Parent == null) return;
            Point position = ((Control)sender).PointToScreen(e.Location);
            Point delta = new Point(position.X - programMouseOrigin.X, position.Y - programMouseOrigin.Y);
            if (!programDragging && Math.Abs(delta.X) < SystemInformation.DragSize.Width && Math.Abs(delta.Y) < SystemInformation.DragSize.Height) return;
            programDragging = true;
            Location = new Point(Math.Max(0, programNodeOrigin.X + delta.X), Math.Max(0, programNodeOrigin.Y + delta.Y));
            Parent.Invalidate(true);
        }

        /// <summary>识别内部 Button 连续单击，避免 Button 不发 DoubleClick。</summary>
        private void ProgramMouseUp(object sender, MouseEventArgs e)
        {
            if (!ProgramEditing || e.Button != MouseButtons.Left) return;
            programPointerDown = false;
            Capture = false;
            if (programDragging) { programLastClick = 0; return; }
            int now = Environment.TickCount;
            int interval = unchecked(now - programLastClick);
            if (programLastClick != 0 && interval >= 0 && interval <= SystemInformation.DoubleClickTime)
            {
                programLastClick = 0;
                if (ConfigurationRequested != null) ConfigurationRequested(this, EventArgs.Empty);
                return;
            }
            programLastClick = now;
        }

        /// <summary>在 UI 线程显示执行状态，不拥有执行线程。</summary>
        public void SetProgramState(string message, Color color)
        {
            if (IsDisposed || !IsHandleCreated) return;
            int revision = Interlocked.Increment(ref programStateRevision);
            if (InvokeRequired) { BeginInvoke(new Action(() => ApplyProgramState(message, color, revision))); return; }
            ApplyProgramState(message, color, revision);
        }

        /// <summary>只应用最新状态，停止清空后丢弃已排队的旧运行或报警刷新。</summary>
        private void ApplyProgramState(string message, Color color, int revision)
        {
            if (IsDisposed || revision != Volatile.Read(ref programStateRevision)) return;
            button1.BackColor = color;
            SetProgramMessage(message);
        }
    }
}
