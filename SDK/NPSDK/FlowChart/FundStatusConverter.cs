using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NPSDK
{
    class FundStatusConverter : TypeConverter
    {

        ///
                /// 是否能用string转换到fundstatus类型。
                ///     
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(String)) return true;
            //这里也可以是其它类型的，可以参考后面的链接里面有介绍
            return base.CanConvertFrom(context, sourceType);
        }
        ///
               /// 能否从fundstatus转换到string
                ///  
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(Scope)) return true;

            // if (destinationType == typeof(InstanceDescriptor)) return true;

            return base.CanConvertTo(context, destinationType);
        }

        ///
        /// 从string转到Fundstatus类型。在Property窗口中显示为string类型。
        ///      
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            String result = "";
            if (value == null) return result;
            if (destinationType == typeof(Scope))
            {
                //这里处理选择后在属性框里面显示的值
                Scope scope = (Scope)value;

              
              
              result = (string)value;
                return result;

            }

            if (destinationType == typeof(InstanceDescriptor))
            {
                ConstructorInfo ci = typeof(Scope).GetConstructor(new Type[] { typeof(bool), typeof(bool), typeof(string) });
                Scope scope = (Scope)value;
                return new InstanceDescriptor(ci, new object[] { scope.Max, scope.Min });
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        ///
        /// 从fundstatus转换为string
        ///       
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value == null || value.ToString().Length == 0) return new Scope();
            if (value is string)
            {
                //String[] v = ((String)value).Split(',');
                string tempvalue = (string)value;
                Scope csf = new Scope();
                csf.AA =tempvalue;
                
              //  csf.Normal = tempvalue.Contains("normal");
                return tempvalue;
            }
            return base.ConvertFrom(context, culture, value);
        }


    }
}
