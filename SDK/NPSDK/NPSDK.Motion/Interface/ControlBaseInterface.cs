using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace NPSDK
{
	public class ControlBaseInterface : Label
	{
		public bool Simulation;

        /// <summary>是否随 SDK 启动初始化；由宿主延迟连接的资源可关闭。</summary>
        [Browsable(false)] public bool InitializeWithSdk { get; set; } = true;

		private ToolTip IOTip = new ToolTip();
        private readonly WindowsFormsSynchronizationContext uiContext;

		private IContainer components = null;

		public override bool AutoSize => false;

		/// <summary>创建并登记硬件控件，不连接物理设备。</summary>
		public ControlBaseInterface()
		{
            uiContext = SynchronizationContext.Current as WindowsFormsSynchronizationContext;
			InitializeComponent();
			SDKPara.RegisterControl(this);
			base.Margin = new Padding(1);
			BorderStyle = BorderStyle.FixedSingle;
			BackColor = Color.FromArgb(240, 240, 240);
			AutoSize = false;
		}

		public virtual void Initial()
		{
		}

		public virtual void DisposeObj()
		{
		}

		/// <summary>刷新 IO 或轴限位/原点。由 NSDK.RefreshControls 调用。</summary>
		public virtual void RefreshControl()
		{
		}

		public void SetSimulation(bool IsSimu)
		{
			Simulation = IsSimu;
		}

        /// <summary>设备连接可在后台执行，地址提示交给创建控件的界面线程。</summary>
		protected void SetToolTip(string sMsg)
		{
            if (uiContext != null && !ReferenceEquals(SynchronizationContext.Current, uiContext))
            {
                uiContext.Post(_ => { if (!IsDisposed) IOTip.SetToolTip(this, sMsg); }, null);
                return;
            }
			IOTip.SetToolTip(this, sMsg);
		}

		/// <summary>与扫描互斥释放驱动及控件，并移除扫描登记。</summary>
		protected override void Dispose(bool disposing)
		{
            lock (this)
            {
                if (disposing && !IsDisposed)
                {
                    SDKPara.UnregisterControl(this);
                    DisposeObj();
                    IOTip.Dispose();
                    if (components != null) components.Dispose();
                }
                base.Dispose(disposing);
            }
		}

		private void InitializeComponent()
		{
			base.SuspendLayout();
			this.BackColor = System.Drawing.Color.White;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			this.MaximumSize = new System.Drawing.Size(500, 40);
			base.Name = "ControlBaseInterface";
			base.Size = new System.Drawing.Size(180, 30);
			base.ResumeLayout(false);
		}
	}
}
