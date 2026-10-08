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
        /// <summary>离开页面时，若参数有修改则保存。</summary>
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

	}
}
