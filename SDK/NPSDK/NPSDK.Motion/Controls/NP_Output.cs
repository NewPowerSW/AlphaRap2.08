using System.ComponentModel;
using System;
using System.Drawing;
using System.Windows.Forms;
using NPSDK;

namespace NPSDK.Motion
{
	/// <summary>
	/// 输出控件。板卡显示 Port；基恩士显示 IP 与软元件；欧姆龙显示 IP 与点地址。
	/// </summary>
	[Designer(typeof(NpIoDesigner))]
	public class NP_Output : ControlBaseInterface
	{
		private IOBaseInterface dOutput;
		private bool ready;
        /// <summary>Whether the IO has an initialized driver.</summary>
        public bool IsReady { get { return ready && dOutput != null; } }

        /// <summary>由宿主限制手动点击；不影响流程持有租约后的程序调用。</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<bool> ManualOperationAllowed { get; set; }
		private PictureBox lamp;

		[Browsable(true)]
		[Category("#NP")]
		[RefreshProperties(RefreshProperties.All)]
		[Description("Adlink/Leadshine=板卡；Keyence=PLC IO；Omron=OmronPLC")]
		public NPVendor Vendor { get; set; } = NPVendor.Leadshine;

		[Browsable(true)]
		[Category("#NP")]
		[Description("轴卡 / EtherCAT 点位。前 3 位卡号，其后为点位，如 00101")]
		public string Port { get; set; } = "00101";

		[Browsable(true)]
		[Category("#Omron")]
		[Description("欧姆龙读写地址，原样交给 OmronPLC，如 Q04[0]")]
		[DefaultValue("")]
		public string OmronAddress { get; set; } = "";

		[Browsable(true)]
		[Category("#Omron")]
		[Description("欧姆龙 CIP 端口，对应 OmronPLC.Port")]
		[DefaultValue(44818)]
		public int OmronPort { get; set; } = 44818;

		[Browsable(true)]
		[Category("#NP")]
		[Description("PLC IP。基恩士示例 192.168.3.100，欧姆龙示例 192.168.10.1")]
		[DefaultValue("192.168.3.100")]
		public string PlcIp { get; set; } = "192.168.3.100";

		[Browsable(true)]
		[Category("#Keyence")]
		[DefaultValue(8501)]
		public int PlcPort { get; set; } = 8501;

		[Browsable(true)]
		[Category("#Keyence")]
		[Description("软元件地址")]
		[DefaultValue(0)]
		public int Address { get; set; }

		[Browsable(true)]
		[Category("#Keyence")]
		[DefaultValue(KeyenceDeviceType.R)]
		public KeyenceDeviceType DeviceType { get; set; } = KeyenceDeviceType.R;

		[Browsable(true)]
		[Category("#Keyence")]
		[DefaultValue(KeyenceDataFormat.U)]
		public KeyenceDataFormat DataFormat { get; set; } = KeyenceDataFormat.U;

        /// <summary>创建输出卡片及指示图，不连接驱动或写入实际输出。</summary>
		public NP_Output()
		{
			lamp = new PictureBox
			{
				Dock = DockStyle.Right,
				Width = 70,
				BackgroundImage = Resource.off,
				BackgroundImageLayout = ImageLayout.Zoom,
				BackColor = Color.Transparent,
				Parent = this
			};
			Text = "Output";
			TextAlign = ContentAlignment.MiddleLeft;
			Size = new Size(220, 35);
			Click += (s, e) =>
			{
				if (IsOn())
				{
					Off();
				}
				else
				{
					On();
				}
			};
		}

        /// <summary>拒绝设计期、非左键、流程占用或宿主未授权的手动切换。</summary>
        protected override void OnClick(EventArgs args)
        {
            if (DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;
            MouseEventArgs mouse = args as MouseEventArgs;
            if (mouse != null && mouse.Button != MouseButtons.Left) return;
            if (!FlowResourceGate.CanAccess(this)) return;
            if (ManualOperationAllowed != null && !ManualOperationAllowed()) return;
            base.OnClick(args);
        }

        /// <summary>允许组合控件把指示图点击转发到相同的手动权限检查。</summary>
        internal void PerformManualClick(EventArgs args) { OnClick(args); }

        /// <summary>按厂商和配置初始化本输出实际驱动。</summary>
		public override void Initial()
		{
			NPHub.SelectVendor(Vendor);
			dOutput = NPControlBase.CreateOutput(Vendor);
			if (Vendor == NPVendor.Keyence)
			{
				string field = PlcPort + "|" + DeviceType + "|" + DataFormat;
				ready = dOutput != null && dOutput.Initial(Address.ToString(), PlcIp, field);
				SetToolTip("Keyence " + DeviceType + Address + " @" + PlcIp);
			}
			else if (Vendor == NPVendor.Omron)
			{
				ready = dOutput != null && dOutput.Initial(OmronAddress, PlcIp, OmronPort.ToString());
				SetToolTip("Omron " + OmronAddress + " @" + PlcIp + ":" + OmronPort);
			}
			else
			{
				ready = dOutput != null && dOutput.Initial(Port, string.Empty, string.Empty);
				SetToolTip("Port:" + Port);
			}
		}

        /// <summary>按实际输出状态刷新卡片指示图。</summary>
		public override void RefreshControl()
		{
			if (!ready || dOutput == null)
			{
				return;
			}
			lamp.BackgroundImage = dOutput.GetState() ? Resource.on : Resource.off;
		}

        /// <summary>资源租约允许时按配置的有效电平打开输出。</summary>
		public void On()
		{
            if (!NPSDK.FlowResourceGate.CanAccess(this)) return;
			if (ready && dOutput != null)
			{
				dOutput.On();
			}
		}

        /// <summary>资源租约允许时按配置的有效电平关闭输出。</summary>
		public void Off()
		{
            if (!NPSDK.FlowResourceGate.CanAccess(this)) return;
			if (ready && dOutput != null)
			{
				dOutput.Off();
			}
		}

        /// <summary>读取本输出是否处于配置的有效状态。</summary>
		public bool IsOn()
		{
			return ready && dOutput != null && dOutput.GetState();
		}

        /// <summary>释放本输出驱动并撤销就绪状态。</summary>
		public override void DisposeObj()
		{
			if (dOutput != null)
			{
				dOutput.Dispose();
			}
			ready = false;
		}
	}
}
