using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace NPSDK.Motion
{
    [ToolboxItem(false)]
    public partial class NP_IoGroup : UserControl
    {
        private readonly bool outputDirection;
        public event EventHandler ParametersRequested;
        [Browsable(false)] public Func<bool> CanOperate { get; set; }
        [Category("#NP"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ControlBaseInterface IO { get; private set; }
        [Category("#NP")] public string SignalCaption { get { return IO.Text; } set { IO.Text = value; } }

        /// <summary>为设计器提供输入组合控件预览。</summary>
        public NP_IoGroup() : this(false) { }

        /// <summary>只创建一种实际 IO 对象，隐藏的方向不会注册或连接。</summary>
        protected NP_IoGroup(bool output)
        {
            InitializeComponent();
            outputDirection = output;
            BindIO(output ? (ControlBaseInterface)new NP_Output() : new NP_Input());
        }

        /// <summary>接管同方向的实际 IO 控件，保留卡片布局并释放被替换的控件。</summary>
        public void BindIO(ControlBaseInterface existing)
        {
            if (existing == null) throw new ArgumentNullException("existing");
            if (IsDisposed || Disposing) throw new ObjectDisposedException(GetType().Name);
            if (existing.IsDisposed || existing.Disposing) throw new ObjectDisposedException(existing.GetType().Name);
            if (outputDirection ? !(existing is NP_Output) : !(existing is NP_Input))
                throw new ArgumentException("The IO direction does not match this group.", "existing");
            if (ReferenceEquals(existing, IO)) return;
            if (!FlowResourceGate.CanAccess(existing) || IO != null && !FlowResourceGate.CanAccess(IO))
                throw new InvalidOperationException("The IO is in use.");
            if (existing.Parent is NP_IoGroup && !ReferenceEquals(existing.Parent, this))
                throw new ArgumentException("The IO is already owned by another group.", "existing");

            ControlBaseInterface previous = IO;
            existing.Name = previous == null ? "Signal" : previous.Name;
            existing.Dock = previous == null ? DockStyle.Fill : previous.Dock;
            existing.MaximumSize = previous == null ? new System.Drawing.Size(500, 60) : previous.MaximumSize;
            NP_Output output = existing as NP_Output;
            if (output != null) output.ManualOperationAllowed = Allowed;
            foreach (Control indicator in existing.Controls)
                indicator.Click += IndicatorClicked;
            Controls.Add(existing);
            IO = existing;
            if (previous == null) return;
            Controls.Remove(previous);
            previous.Dispose();
        }

        /// <summary>状态图的单次点击通过实际输出控件的统一权限入口转发。</summary>
        private void IndicatorClicked(object sender, EventArgs args)
        {
            NP_Output output = IO as NP_Output;
            if (output != null) output.PerformManualClick(args);
        }

        /// <summary>请求宿主打开参数编辑，权限和配置保存由宿主管理。</summary>
        public void ShowParameters() { if (Allowed() && ParametersRequested != null) ParametersRequested(this,EventArgs.Empty); }

        /// <summary>阻止设计环境、流程占用或宿主未授权的 IO 操作。</summary>
        private bool Allowed() { return !IsDisposed && !Disposing && !DesignMode && LicenseManager.UsageMode != LicenseUsageMode.Designtime && FlowResourceGate.CanAccess(IO) && (CanOperate == null || CanOperate()); }
    }

    [ToolboxItem(true)]
    public sealed class NP_InputGroup : NP_IoGroup
    {
        /// <summary>创建可在设计器拖入的输入组合控件。</summary>
        public NP_InputGroup() : base(false) { Name = "NP_InputGroup"; }
    }

    [ToolboxItem(true)]
    public sealed class NP_OutputGroup : NP_IoGroup
    {
        /// <summary>创建可在设计器拖入的输出组合控件。</summary>
        public NP_OutputGroup() : base(true) { Name = "NP_OutputGroup"; }
    }
}
