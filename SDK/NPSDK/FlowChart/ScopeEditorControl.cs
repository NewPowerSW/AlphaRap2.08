using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NPSDK
{

    public partial class ScopeEditorControl : UserControl
    {

      
        private Boolean canceling;
        private Control _Casse = new Control();
        private Control _Parent = new Control();
        ListBox gListBox =new ListBox();
        public ScopeEditorControl(Control Parent, Control Casse)
        {
           
            _Casse = Casse;
            _Parent = Parent;
            InitializeComponent();
        }

       


        public Control Casse
        {
            get
            {
                return _Casse;
            }
        }
        private void textBox1_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                
             
            }
            catch (FormatException)
            {
                e.Cancel = true;
                MessageBox.Show("无效的值", "验证错误",
               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void textBox2_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                
            }
            catch (FormatException)
            {
                e.Cancel = true;
                MessageBox.Show("无效的值", "验证错误",
              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
               
           
                canceling = true;
            }
            return base.ProcessDialogKey(keyData);
        }
        private void ScopeEditorControl_Leave(object sender, EventArgs e)
        {
            if (!canceling)
            {
                
           
            }
        }
        private void ScopeEditorControl_Load(object sender, EventArgs e)
        {
           

            this.listBox1.SelectedIndexChanged -= new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            listBox1.Items.Add("无");
         
            foreach (Control control in (_Parent.Parent).Controls)
            {
				
					if (control.Name != _Parent.Name && control.GetType().ToString().Contains("Flow_Chart"))
					{
						listBox1.Items.Add(control.Name);
						gListBox.Items.Add(control);
					}
			
					
               
            }
           
            listBox1.SelectedItem= _Casse==null? "无": _Casse.Name;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (listBox1.SelectedIndex>0)
            {

                _Casse = (Control)gListBox.Items[listBox1.SelectedIndex - 1];
            }
           else
            {
                _Casse =null;

            }
            SendKeys.Send("{enter}");
            
        }
    }

}
