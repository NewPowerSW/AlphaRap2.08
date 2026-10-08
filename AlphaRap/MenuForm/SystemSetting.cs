using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AlphaRapLibrary;

namespace AlphaRap
{
	public partial class SystemSetting : ModuleBaseForm
	{
		public SystemSetting()
		{
			InitializeComponent();
			SysPara.items = 1;
		}

		private void SystemSetting_Leave(object sender, EventArgs e)
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
		private void Raid_Click(object sender, EventArgs e)
		{
			RadioButton a = sender as RadioButton;
			List<RadioButton> buttonList = new List<RadioButton>();
			foreach (Control c in tableLayoutPanel1.Controls )
			{
				if (c is RadioButton)
				{
					buttonList.Add((RadioButton)c);
				}
			}
			foreach(RadioButton item in buttonList)
			{
				if(a.Name==item.Name)
				{
					item.Checked = !item.Checked;
				}
			}
			SysPara.items++;
			label3.Focus();
		}
	}
}