using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlphaRapLibrary;
using AlphaRap.Classes;

namespace AlphaRap
{
    public partial class PlatFormSetting : ModuleBaseForm
    {
        public PlatFormSetting()
        {
            InitializeComponent();
            ReadSettingData();          
        }
        /// <summary>
        /// 点击radioH1执行的操作显示单头
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioH1_Click(object sender, EventArgs e)
        {
            //MiddleLayer.HardF.tabPage1.Parent = null;
            //MiddleLayer.HardF.MotorControl2.Parent = null;
            //MiddleLayer.HomeF.tabPage2.Parent = null;
            SysPara.bPlat = "true";
            IniFile IniFile = new IniFile(".\\MachineSetup.ini");
            IniFile.WriteString("MachineSetup", "bPlat", SysPara.bPlat);
            WriteSettingData();
        }
        /// <summary>
        /// 点击radioH2执行的操作显示双头
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioH1H2_Click(object sender, EventArgs e)
        {
        
            //MiddleLayer.HomeF.tabPage2.Parent = MiddleLayer.HomeF.tabHome;
            SysPara.bPlat = "false";
            MiddleLayer.bTaskFlowOK2 = false;
            IniFile IniFile = new IniFile(".\\MachineSetup.ini");
            IniFile.WriteString("MachineSetup", "bPlat", SysPara.bPlat);
            WriteSettingData();
        }
        public void TxH1ScaleFactor()
        {           

        }
        public void TxtH2ScaleFactor()
        {
            
        }
        private void Txt_H1ScaleFactor_TextChanged(object sender, EventArgs e)
        {
            TxH1ScaleFactor();
        }

        private void Txt_H2ScaleFactor_TextChanged(object sender, EventArgs e)
        {
            TxtH2ScaleFactor();
        }    
        /// <summary>
        /// 判断间隔几个点清洗一次的数据的合法性
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
    

        private void PlatFormSetting_Leave(object sender, EventArgs e)
        {
            if (SysPara.items > 1)
            {                
                // MiddleLayer.SystemF.SystemGroup.Focus();
                MiddleLayer.MainF.SaveData();
                SysPara.items = 1;
            }
        }

        private void DataChange_Click(object sender, EventArgs e)
        {
            SysPara.items++;
        }

        /// <summary>
        /// 判断自动登出时间的合法性
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

    }
}
