using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
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
        private string programBindingDetails = "";
        private string programStatusMessage = "";
        private string programElapsedText = "";
        private bool programTimingStarted;
        private volatile ProgramAppearance programAppearance;
        public event EventHandler ConfigurationRequested;

        private sealed class ProgramAppearance
        {
            internal readonly string Message;
            internal readonly Color Color;
            internal readonly bool ShowStatus;

            /// <summary>将文字与颜色作为同一次节点状态保存。</summary>
            internal ProgramAppearance(string message, Color color, bool showStatus)
            {
                Message = message;
                Color = color;
                ShowStatus = showStatus;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ProgramEditing { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ProgramManaged { get; set; }

        /// <summary>原生流程会话尚未退出时返回 true，不依赖历史运行标记或停止按钮状态。</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsLegacyRunning { get { return task != null && !task.IsCompleted; } }


        /// <summary>为节点提示提供执行方式，不占用节点标题空间。</summary>
        [Category("Appearance"), DefaultValue("")]
        public string ProgramBindingLabel
        {
            get { return programBindingLabel; }
            set
            {
                programBindingLabel = value ?? "";
                UpdateProgramToolTip();
            }
        }

        /// <summary>设置悬停时显示的绑定说明，并同步辅助功能描述。</summary>
        public void SetProgramBindingDetails(string details)
        {
            programBindingDetails = details ?? "";
            AccessibleDescription = programBindingDetails;
            UpdateProgramToolTip();
        }

        /// <summary>在节点和按钮表面显示完整标题、执行状态与绑定说明。</summary>
        private void UpdateProgramToolTip()
        {
            if (IsDisposed || Disposing) return;
            string details = programBindingDetails.Length == 0 ? programBindingLabel : programBindingDetails;
            string hint = string.Join(Environment.NewLine, new[] { button1.Text, programStatusMessage, details }
                .Where(line => !string.IsNullOrWhiteSpace(line)));
            if (programToolTip.GetToolTip(button1) == hint) return;
            programToolTip.SetToolTip(this, hint);
            programToolTip.SetToolTip(button1, hint);
        }

        /// <summary>有状态时保留独立状态行，仅显示耗时时让标题垂直居中。</summary>
        private void UpdateProgramLabel()
        {
            if (IsDisposed) return;
            if (IsHandleCreated && InvokeRequired) { PostProgramUpdate(UpdateProgramLabel); return; }
            bool hasStatus = label1.Text.Length > 0;
            // Flat 按钮自身已有内边距，额外留白只用于错开状态行和标题。
            int top = hasStatus ? Math.Max(0, label1.Font.Height - 2) : 0;
            int right = 0;
            if (!hasStatus && programElapsedText.Length > 0)
                right = TextRenderer.MeasureText(programElapsedText, label1.Font, Size.Empty, TextFormatFlags.NoPadding).Width + (programBreakpoint ? 18 : 6);
            button1.Padding = new Padding(0, top, right, 0);
            button1.Invalidate();
        }

        /// <summary>在按钮表面动态绘制状态和右上角耗时，按文字宽度排列并避开断点。</summary>
        private void DrawProgramBinding(object sender, PaintEventArgs args)
        {
            if (label1 == null) return;
            const TextFormatFlags flags = TextFormatFlags.Top | TextFormatFlags.SingleLine
                | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;
            int right = button1.ClientSize.Width - (programBreakpoint ? 17 : 6);
            int available = Math.Max(0, right - 6);
            if (available == 0) return;
            int timeWidth = programElapsedText.Length == 0 ? 0 : Math.Min(available,
                TextRenderer.MeasureText(args.Graphics, programElapsedText, label1.Font, Size.Empty, flags).Width);
            int statusWidth = Math.Max(0, available - timeWidth - (timeWidth > 0 ? 5 : 0));
            Rectangle area = new Rectangle(6, 3, statusWidth, label1.PreferredHeight);
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
            if (IsHandleCreated && InvokeRequired) { PostProgramUpdate(() => SetProgramElapsedTime(text)); return; }
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
            if (IsHandleCreated && InvokeRequired) { PostProgramUpdate(RefreshProgramTiming); return; }
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
                UpdateProgramLabel();
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

        /// <summary>保存最新执行状态，隐藏页创建句柄后也能显示。</summary>
        public void SetProgramState(string message, Color color)
        {
            SetProgramState(message, color, true);
        }

        /// <summary>更新节点状态，允许已完成等状态仅保留在悬停提示中。</summary>
        public void SetProgramState(string message, Color color, bool showStatus)
        {
            if (IsDisposed || Disposing) return;
            ProgramAppearance appearance = new ProgramAppearance(message, color, showStatus);
            programAppearance = appearance;
            if (!IsHandleCreated) return;
            if (InvokeRequired) { PostProgramUpdate(() => ApplyProgramState(appearance)); return; }
            ApplyProgramState(appearance);
        }

        /// <summary>首次显示或重建句柄时补上后台已报告的状态。</summary>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ProgramAppearance appearance = programAppearance;
            if (appearance != null) ApplyProgramState(appearance);
        }

        /// <summary>只应用当前状态，过期的界面回调不会覆盖新状态。</summary>
        private void ApplyProgramState(ProgramAppearance appearance)
        {
            if (IsDisposed || !ReferenceEquals(appearance, programAppearance)) return;
            button1.BackColor = appearance.Color;
            bool runningGreen = appearance.Color == Color.Green;
            button1.ForeColor = runningGreen ? Color.White : Color.Empty;
            label1.ForeColor = runningGreen ? Color.White : Color.Black;
            programStatusMessage = appearance.Message ?? "";
            label1.Text = appearance.ShowStatus ? programStatusMessage : "";
            UpdateProgramLabel();
            UpdateProgramToolTip();
        }

        /// <summary>将界面刷新交给 UI 线程；关闭窗体或重建句柄时丢弃失效的刷新。</summary>
        private void PostProgramUpdate(Action update)
        {
            if (IsDisposed || Disposing || !IsHandleCreated) return;
            try { BeginInvoke(new Action(() => { if (!IsDisposed && !Disposing) update(); })); }
            catch (InvalidOperationException) { }
        }
    }
}
