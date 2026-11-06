using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NPSDK;

namespace NPSDK.Motion
{
	/// <summary>
	/// 输入控件。板卡显示 Port；基恩士显示 IP 与软元件；欧姆龙显示 IP 与点地址。
	/// </summary>
	[Designer(typeof(NpIoDesigner))]
	public class NP_Input : ControlBaseInterface
	{
		private IOBaseInterface dInput;
		private bool ready;
        /// <summary>Whether the IO has an initialized driver.</summary>
        public bool IsReady { get { return ready && dInput != null; } }
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
		[Description("欧姆龙读取地址，原样交给 OmronPLC.OmronReadBoolean，如 I04[0]")]
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
		[Description("PLC TCP 端口")]
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

		public NP_Input()
		{
			lamp = new PictureBox
			{
				Dock = DockStyle.Right,
				Width = 30,
				BackgroundImage = Resource.ledoff,
				BackgroundImageLayout = ImageLayout.Stretch,
				Parent = this
			};
			Text = "Input";
			TextAlign = ContentAlignment.MiddleLeft;
			Size = new Size(180, 30);
		}

		public override void Initial()
		{
			NPHub.SelectVendor(Vendor);
			dInput = NPControlBase.CreateInput(Vendor);
			if (Vendor == NPVendor.Keyence)
			{
				string field = PlcPort + "|" + DeviceType + "|" + DataFormat;
				ready = dInput != null && dInput.Initial(Address.ToString(), PlcIp, field);
				SetToolTip("Keyence " + DeviceType + Address + " @" + PlcIp);
			}
			else if (Vendor == NPVendor.Omron)
			{
				ready = dInput != null && dInput.Initial(OmronAddress, PlcIp, OmronPort.ToString());
				SetToolTip("Omron " + OmronAddress + " @" + PlcIp + ":" + OmronPort);
			}
			else
			{
				ready = dInput != null && dInput.Initial(Port, string.Empty, string.Empty);
				SetToolTip("Port:" + Port);
			}
		}

		public override void RefreshControl()
		{
			if (!ready || dInput == null)
			{
				return;
			}
			lamp.BackgroundImage = dInput.GetState() ? Resource.ledon : Resource.ledoff;
		}

		public bool IsOn()
		{
			return ready && dInput != null && dInput.GetState();
		}

		public override void DisposeObj()
		{
			if (dInput != null)
			{
				dInput.Dispose();
			}
			ready = false;
		}
	}

	public enum KeyenceDeviceType
	{
		R,
		MR,
		DM
	}

	public enum KeyenceDataFormat
	{
		U,
		S,
		D,
		L,
		H
	}
}
