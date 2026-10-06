using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NPSDK;
namespace AlphaRap
{
    public partial class Flow : Form
    {
        
       
        public Flow()
        {


            CheckForIllegalCrossThreadCalls = false;


            InitializeComponent();
        
           



        }



        public void AddFlowChart()
        {

            for (int i = 0; i < NPSDK.Flow_Module.FlowChart_ModuleList.Count; i++)
            {
                TabPage p = new TabPage();
                // 必须起名：TabPage 会被语言扫描按名字登记，名字为空时
                // XMLExpand.GetElement 会拼出 "中文/窗体名/" 这种空段路径并抛 XPathException
                p.Name = "flowChartTab_" + i;
                p.Text = Flow_Module.FlowChart_ModuleList[i].Name;
                p.AutoScroll = true;
                tabControl1.TabPages.Add(p);
               
                Flow_Module.FlowChart_ModuleList[i].TopLevel = false;
                Flow_Module.FlowChart_ModuleList[i].FormBorderStyle = FormBorderStyle.None;
                //Flow_Module.FlowChart_ModuleList[i].WindowState = FormWindowState.Maximized;
                Flow_Module.FlowChart_ModuleList[i].Dock = DockStyle.Fill;
                Panel _pane = new Panel();
                
                _pane.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
                _pane.Controls.Add(Flow_Module.FlowChart_ModuleList[i]);
                _pane.Show();
                //_pane.AutoSize = true;
                _pane.Dock = DockStyle.Fill;

                Flow_Module.FlowChart_ModuleList[i].Show();

                tabControl1.TabPages[i].Controls.Add(_pane);
            }

        }

        private void Flow_Load(object sender, EventArgs e)
        {
            AddFlowChart();
        }
    }
}
