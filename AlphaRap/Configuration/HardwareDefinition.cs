using System;
using System.Globalization;
using System.Linq;
using NPSDK;
using NPSDK.Motion;

namespace AlphaRap.Configuration
{
    public sealed class HardwareDefinition
    {
        public string Id = Guid.NewGuid().ToString("N");
        public string Kind = "Axis";
        public string Name = "Axis";
        public string ControlType = "";
        public NPVendor Vendor = NPVendor.Leadshine;
        public string Port = "0";
        public double WorkSpeed = 10, ManualSpeed = 10, MaxSpeed = 1000, SpeedRatio = 100, Tacc = 0.1, Tdec = 0.1;
        public double PulseEquivalent = 1000, EncoderPulseEquivalent = 1000;
        public int HomeMode = 28;
        public bool HomeDir;
        public double HomeHighSpeed = 20, HomeLowSpeed = 5, HomeOffset;
        public double LowLimit = -10000, HighLimit = 10000;
        public string PlcIp = "192.168.3.100", OmronAddress = "";
        public int PlcPort = 8501, Address, OmronPort = 44818;
        public bool ReverseState;
        public KeyenceDeviceType DeviceType = KeyenceDeviceType.R;
        public KeyenceDataFormat DataFormat = KeyenceDataFormat.U;
        public static readonly string[] CommonFields = { "Name", "ControlType", "Vendor", "Port" };
        public static readonly string[] AxisFields = { "WorkSpeed", "ManualSpeed", "MaxSpeed", "SpeedRatio", "Tacc", "Tdec", "PulseEquivalent", "EncoderPulseEquivalent", "HomeMode", "HomeDir", "HomeHighSpeed", "HomeLowSpeed", "HomeOffset", "LowLimit", "HighLimit" };
        public static readonly string[] IoFields = { "PlcIp", "PlcPort", "DeviceType", "DataFormat", "Address", "OmronAddress", "OmronPort", "ReverseState" };

        /// <summary>复制编辑草稿，不在编辑期间改变已配置实例。</summary>
        public HardwareDefinition Copy() { return (HardwareDefinition)MemberwiseClone(); }

        /// <summary>仅列出当前资源种类允许编辑的硬件字段。</summary>
        public string[] Fields()
        {
            if (Kind == "Axis") return CommonFields.Concat(AxisFields).ToArray();
            if (Vendor == NPVendor.Keyence) return new[] { "Name", "ControlType", "Vendor", "PlcIp", "PlcPort", "DeviceType", "DataFormat", "Address", "ReverseState" };
            if (Vendor == NPVendor.Omron) return new[] { "Name", "ControlType", "Vendor", "PlcIp", "OmronAddress", "OmronPort", "ReverseState" };
            return CommonFields.Concat(new[] { "ReverseState" }).ToArray();
        }

        /// <summary>轴仅列出已有运动驱动，IO 列出已有 IO 驱动；不连接硬件。</summary>
        public string[] Vendors()
        {
            return Enum.GetValues(typeof(NPVendor)).Cast<NPVendor>().Where(vendor => Kind != "Axis" || NPControlBase.SupportsMotor(vendor)).Select(vendor => vendor.ToString()).ToArray();
        }

        /// <summary>校验身份、厂商、地址、有限运动参数及限位。</summary>
        public void Validate()
        {
            Guid identity;
            if (!Guid.TryParseExact(Id,"N",out identity) || !new[] { "Axis","Input","Output" }.Contains(Kind) || string.IsNullOrWhiteSpace(Name) || Name.Trim() != Name) throw new InvalidOperationException(HardwareText.T("invalid"));
            HardwareControlTypes.Resolve(Kind,ControlType);
            if (!Enum.IsDefined(typeof(NPVendor),Vendor) || (Kind == "Axis" && !NPControlBase.SupportsMotor(Vendor))) throw new InvalidOperationException(HardwareText.T("vendor"));
            if ((Kind == "Axis" || Vendor == NPVendor.Leadshine || Vendor == NPVendor.Adlink) && string.IsNullOrWhiteSpace(Port)) throw new InvalidOperationException(HardwareText.T("invalid"));
            bool digits = (Port ?? "").All(character => character >= '0' && character <= '9');
            if (Kind == "Axis" && Vendor == NPVendor.Leadshine && (!digits || Port.Length != 1)) throw new InvalidOperationException(HardwareText.T("portFormat"));
            if (Kind == "Axis" && Vendor == NPVendor.Adlink && (!digits || Port.Length != 3 || Port[2] > '3')) throw new InvalidOperationException(HardwareText.T("portFormat"));
            if (Kind != "Axis" && (Vendor == NPVendor.Leadshine || Vendor == NPVendor.Adlink) && (!digits || Port.Length != 5)) throw new InvalidOperationException(HardwareText.T("portFormat"));
            foreach (string name in Kind == "Axis" ? AxisFields : new string[0])
            {
                object value = GetType().GetField(name).GetValue(this);
                if (!(value is double)) continue;
                double number = (double)value;
                if (double.IsNaN(number) || double.IsInfinity(number)) throw new InvalidOperationException(HardwareText.T("invalid"));
            }
            if (Kind == "Axis" && (LowLimit >= HighLimit || WorkSpeed <= 0 || ManualSpeed <= 0 || MaxSpeed < WorkSpeed || MaxSpeed < ManualSpeed || SpeedRatio <= 0 || SpeedRatio > 100 || Tacc <= 0 || Tdec <= 0 || PulseEquivalent <= 0 || EncoderPulseEquivalent <= 0 || HomeHighSpeed <= 0 || HomeLowSpeed <= 0)) throw new InvalidOperationException(HardwareText.T("invalid"));
            if (Kind != "Axis" && Vendor == NPVendor.Keyence && (PlcPort < 1 || PlcPort > 65535 || Address < 0 || !Enum.IsDefined(typeof(KeyenceDeviceType), DeviceType) || !Enum.IsDefined(typeof(KeyenceDataFormat), DataFormat))) throw new InvalidOperationException(HardwareText.T("invalid"));
            if (Kind != "Axis" && Vendor == NPVendor.Omron && (OmronPort < 1 || OmronPort > 65535)) throw new InvalidOperationException(HardwareText.T("invalid"));
            System.Net.IPAddress ip;
            if (Kind != "Axis" && (Vendor == NPVendor.Keyence || Vendor == NPVendor.Omron) && !System.Net.IPAddress.TryParse(PlcIp,out ip)) throw new InvalidOperationException(HardwareText.T("invalid"));
            if (Kind != "Axis" && Vendor == NPVendor.Omron && string.IsNullOrWhiteSpace(OmronAddress)) throw new InvalidOperationException(HardwareText.T("invalid"));
        }

        /// <summary>把明确字段的文本转换为对应类型，不执行脚本或任意对象方法。</summary>
        public void SetValue(string name, string value)
        {
            System.Reflection.FieldInfo field = GetType().GetField(name);
            if (!CommonFields.Concat(AxisFields).Concat(IoFields).Contains(name)) throw new ArgumentException(name);
            object parsed = field.FieldType.IsEnum ? Enum.Parse(field.FieldType,value) : Convert.ChangeType(value,field.FieldType,CultureInfo.InvariantCulture);
            field.SetValue(this,parsed);
        }
    }
}
