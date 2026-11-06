using System;
using System.ComponentModel;

namespace NPSDK.Motion
{
	/// <summary>轴控件 Vendor 下拉仅显示支持 Motor 的厂商（不含 Keyence、Omron）。</summary>
	public class MotionVendorConverter : EnumConverter
	{
		public MotionVendorConverter()
			: base(typeof(NPVendor))
		{
		}

		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			return true;
		}

		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
		{
			return true;
		}

		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			return new StandardValuesCollection(new object[]
			{
				NPVendor.Adlink,
				NPVendor.Leadshine
			});
		}
	}
}
