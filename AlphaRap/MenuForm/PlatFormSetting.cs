using System;
using AlphaRapLibrary;

namespace AlphaRap
{
    public partial class PlatFormSetting : ModuleBaseForm
    {
        public PlatFormSetting()
        {
            InitializeComponent();
            ReadSettingData();          
        }
        public void TxH1ScaleFactor()
        {           
        }
        public void TxtH2ScaleFactor()
        {
        }
        /// <summary>
        /// 判断间隔几个点清洗一次的数据的合法性
        /// </summary>

        private void PlatFormSetting_Leave(object sender, EventArgs e)
        {
            if (SysPara.items > 1)
            {                
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
	}
}
