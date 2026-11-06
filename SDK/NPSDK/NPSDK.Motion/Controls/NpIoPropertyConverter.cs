using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms.Design;

namespace NPSDK.Motion
{
	/// <summary>
	/// 设计器属性：板卡只显示 Port；基恩士显示 #Keyence；欧姆龙显示 #Omron。
	/// </summary>
	public class NpIoDesigner : ControlDesigner
	{
		protected override void PreFilterProperties(IDictionary properties)
		{
			base.PreFilterProperties(properties);
			ApplyVendorFilter(properties);
		}

		protected override void PostFilterProperties(IDictionary properties)
		{
			base.PostFilterProperties(properties);
			ApplyVendorFilter(properties);
		}

		private void ApplyVendorFilter(IDictionary properties)
		{
			NPVendor vendor = ReadVendor();
			if (vendor == NPVendor.Keyence)
			{
				Remove(properties, "Port", "OmronAddress", "OmronPort");
				Recategory(properties, "#Keyence", "PlcIp", "PlcPort", "Address", "DeviceType", "DataFormat");
				return;
			}
			if (vendor == NPVendor.Omron)
			{
				Remove(properties, "Port", "PlcPort", "Address", "DeviceType", "DataFormat");
				Recategory(properties, "#Omron", "PlcIp", "OmronPort", "OmronAddress");
				return;
			}
			Remove(properties, "PlcIp", "PlcPort", "Address", "DeviceType", "DataFormat", "OmronAddress", "OmronPort");
		}

		private NPVendor ReadVendor()
		{
			NP_Input input = Component as NP_Input;
			if (input != null)
			{
				return input.Vendor;
			}
			NP_Output output = Component as NP_Output;
			if (output != null)
			{
				return output.Vendor;
			}
			return NPVendor.Leadshine;
		}

		private static void Remove(IDictionary properties, params string[] names)
		{
			for (int i = 0; i < names.Length; i++)
			{
				properties.Remove(names[i]);
			}
		}

		private static void Recategory(IDictionary properties, string category, params string[] names)
		{
			for (int i = 0; i < names.Length; i++)
			{
				PropertyDescriptor descriptor = properties[names[i]] as PropertyDescriptor;
				if (descriptor != null)
				{
					properties[names[i]] = new NpCategoryDescriptor(descriptor, category);
				}
			}
		}
	}

	internal sealed class NpCategoryDescriptor : PropertyDescriptor
	{
		private readonly PropertyDescriptor inner;
		private readonly string category;

		public NpCategoryDescriptor(PropertyDescriptor inner, string category)
			: base(inner)
		{
			this.inner = inner;
			this.category = category;
		}

		public override string Category
		{
			get { return category; }
		}

		public override Type ComponentType
		{
			get { return inner.ComponentType; }
		}

		public override bool IsReadOnly
		{
			get { return inner.IsReadOnly; }
		}

		public override Type PropertyType
		{
			get { return inner.PropertyType; }
		}

		public override bool CanResetValue(object component)
		{
			return inner.CanResetValue(component);
		}

		public override object GetValue(object component)
		{
			return inner.GetValue(component);
		}

		public override void ResetValue(object component)
		{
			inner.ResetValue(component);
		}

		public override void SetValue(object component, object value)
		{
			inner.SetValue(component, value);
		}

		public override bool ShouldSerializeValue(object component)
		{
			return inner.ShouldSerializeValue(component);
		}
	}
}
