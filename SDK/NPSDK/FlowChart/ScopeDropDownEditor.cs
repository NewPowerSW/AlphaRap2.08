using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace NPSDK
{

    public class ScopeDropDownEditor : UITypeEditor
    {

       public override UITypeEditorEditStyle GetEditStyle
        (ITypeDescriptorContext context)
       {  
            if (context != null && context.Instance != null)  
            {  
                return UITypeEditorEditStyle.DropDown;  
            }  
            return base.GetEditStyle(context);  
        }  

        public override object EditValue(ITypeDescriptorContext context,  IServiceProvider provider, object value)
        {  
            IWindowsFormsEditorService editorService = null;  
            if (context !=null && context.Instance != null && provider != null)  
            {  
                editorService = (IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService));  
                if (editorService != null)  
                {
                    Flow_Chart control =(Flow_Chart)context.Instance;
                 
                    ScopeEditorControl editorControl =new ScopeEditorControl(control,(Control) value);
                  
                    editorService.DropDownControl(editorControl);  
                  
                    value = editorControl.Casse;  
                  return value;  
                }  
            }  
           return value;  
        }  
   }  

}
