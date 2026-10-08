using AlphaRapLibrary;

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SFCDLL_CommonEx.Interface;

namespace AlphaRap.MES
{
	public partial class DongJu : ModuleBaseForm
	{
		public DongJu()
		{
			InitializeComponent();
		}

		IniFile IniFile_Dongju = new IniFile(".\\Dongju.ini");
		#region SFC
		#region CheckStation
		/// <summary>
		/// 产品别
		/// </summary>
		public string Product
		{
			get
			{
				return IniFile_Dongju.ReadString("MES", "Product", "");
			}
			set
			{
				IniFile_Dongju.WriteString("MES", "Product", value);
				textBox_Product.Text = value;
			}
		}
		/// <summary>
		/// 站别
		/// </summary>
		public string Station
		{
			get
			{
				return IniFile_Dongju.ReadString("MES", "Station", "");
			}
			set
			{
				IniFile_Dongju.WriteString("MES", "Station", value);
				textBox_Station.Text = value;
			}
		}
		/// <summary>
		/// 条码
		/// </summary>
		public string Barcode
		{
			get
			{
				return IniFile_Dongju.ReadString("MES", "Barcode", "");
			}
			set
			{
				IniFile_Dongju.WriteString("MES", "Barcode", value);
				textBox_Barcode.Text = value;
			}
		}
		/// <summary>
		/// 条别
		/// </summary>
		public string LineNo
		{
			get
			{
				return IniFile_Dongju.ReadString("MES", "LineNo", "");
			}
			set
			{
				IniFile_Dongju.WriteString("MES", "LineNo", value);
				textBox_LineNo.Text = value;
			}
		}
		/// <summary>
		/// 条码
		/// </summary>
		public string Version
		{
			get
			{
				return IniFile_Dongju.ReadString("MES", "Version", "");
			}
			set
			{
				IniFile_Dongju.WriteString("MES", "Version", value);
				textBox_Version.Text = value;
			}
		}
		/// <summary>
		/// 返回值Y为通过，N是失败
		/// </summary>
		/// <param name="Product"></param>
		/// <param name="Station"></param>
		/// <param name="Barcode"></param>
		/// <param name="LineNo"></param>
		/// <param name="Version"></param>
		/// <returns></returns>
		public string CheckStationPass(string Product, string Station, string Barcode, string LineNo, string Version)
		{
			SFCDLL SFCDLL = new SFCDLL();
			return SFCDLL.CheckStationPass(Product, Station, Barcode, LineNo, Version);
		}

		#endregion
		#region GetSpecialvalue
		/// <summary>
		/// Type参数是给指定的固定字符串。比如GETDATE，获取日期
		/// </summary>
		public string Type
		{
			get
			{
				return IniFile_Dongju.ReadString("MES", "Type", "");
			}
			set
			{
				IniFile_Dongju.WriteString("MES", "Type", value);
				textBox_Type.Text = value;
			}
		}
		/// <summary>
		/// parameter要必须填写值。
		/// </summary>
		public string Parameters
		{
			get
			{
				return IniFile_Dongju.ReadString("MES", "Parameters", "");
			}
			set
			{
				IniFile_Dongju.WriteString("MES", "Parameters", value);
				textBox_Parameters.Text = value;
			}
		}
		/// <summary>
		/// 返回值Y为通过，N是失败
		/// </summary>
		/// <param name="Product"></param>
		/// <param name="Type"></param>
		/// <param name="Parameters"></param>
		/// <returns></returns>
		public string GetSpecialValue(string Product, string Type, string Parameters)
		{
			SFCDLL SFCDLL = new SFCDLL();
			return SFCDLL.GetSpecialValue(Product, Type, Parameters);
		}
		#endregion
		#region InsertInto Table
		public string TableName
		{
			get
			{
				return IniFile_Dongju.ReadString("MES", "TableName", "");
			}
			set
			{
				IniFile_Dongju.WriteString("MES", "TableName", value);
				textBox_TableName.Text = value;
			}
		}
		public string ColumnList
		{
			get
			{
				return IniFile_Dongju.ReadString("MES", "ColumnList", "");
			}
			set
			{
				IniFile_Dongju.WriteString("MES", "ColumnList", value);
				textBox_ColumnList.Text = value;
			}
		}
		/// <summary>
		/// ValueList：逗号隔开的点位名称对应值列表，例如val1, val2, val3, val4
		/// </summary>
		public string ValueList
		{
			get
			{
				return IniFile_Dongju.ReadString("MES", "ValueList", "");
			}
			set
			{
				IniFile_Dongju.WriteString("MES", "ValueList", value);
				textBox_ValueList.Text = value;
			}
		}
		/// <summary>
		/// 返回值Y为通过，N是失败
		/// </summary>
		/// <param name="Product"></param>
		/// <param name="TableName"></param>
		/// <param name="Key"></param>
		/// <param name="Value"></param>
		/// <returns></returns>
		public string InsertInTable(string Product, string TableName, string Key, string Value)
		{
			SFCDLL SFCDLL = new SFCDLL();

			return SFCDLL.InsertIntoTable(Product, TableName, Key, Value);
		}
		#endregion
		private void Save_Click(object sender, EventArgs e)
		{
			List<TextBox> List = new List<TextBox>();
			void EnumControls(Control container)
			{
				foreach (Control item in container.Controls)
				{
					//c is the child control here
					EnumControls(item);
					if (item is TextBox)
						List.Add((TextBox)item);
				}
			}

			//调用
			EnumControls(this);
			foreach (TextBox item in List)
				IniFile_Dongju.WriteString("MES", item.Name.Replace("textBox_", ""), item.Text);
		}

		private void CheckStation_Text_Click(object sender, EventArgs e)
		{
			try
			{
				textBox1.Text = CheckStationPass(Product, Station, Barcode, LineNo, Version);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			try
			{
				textBox3.Text = InsertInTable(Product, TableName, ColumnList, ValueList);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			try
			{
				textBox2.Text = GetSpecialValue(Product, Type, Parameters);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}
		#endregion
		#region SCADA
		/// <summary>
		/// 机种码，缺省为空，取配制文件设定
		/// </summary>
		public string Model_No
		{
			get
			{
				return IniFile_Dongju.ReadString("MES", "Model_No", "");
			}
			set
			{
				IniFile_Dongju.WriteString("MES", "Model_No", value);
				textBox_Model_No.Text = value;
			}
		}
		/// <summary>
		/// 项目名称，缺省为空，取配制文件设定
		/// </summary>
		public string ProjectName
		{
			get
			{
				return IniFile_Dongju.ReadString("MES", "ProjectName", "");
			}
			set
			{
				IniFile_Dongju.WriteString("MES", "ProjectName", value);
				textBox_ProjectName.Text = value;
			}
		}
		/// <summary>
		/// 逗号隔开的点位名称列表，点位名称规格：设备编号_参数名，例col1, col2, col3, col4
		/// </summary>

		public string TagNameList
		{
			get
			{
				return IniFile_Dongju.ReadString("MES", "TagNameList", "");
			}
			set
			{
				IniFile_Dongju.WriteString("MES", "TagNameList", value);
				textBox_TagNameList.Text = value;
			}
		}
		/// <summary>
		/// 逗号隔开的点位名称对应值列表
		/// </summary>
		public string SetValueList
		{
			get
			{
				return IniFile_Dongju.ReadString("MES", "SetValueList", "");
			}
			set
			{
				IniFile_Dongju.WriteString("MES", "SetValueList", value);
				textBox_SetValueList.Text = value;
			}
		}
		/// <summary>
		/// 设备编号
		/// </summary>
		public string EQP_ID
		{
			get
			{
				return IniFile_Dongju.ReadString("MES", "EQP_ID", "");
			}
			set
			{
				IniFile_Dongju.WriteString("MES", "EQP_ID", value);
				textBox_EQP_ID.Text = value;
			}
		}
		public string SCADA_SetData(string Model_No, string ProjectName, string TagNameList, string ValueList)
		{
			SFCDLL SFCDLL = new SFCDLL();
			return SFCDLL.SCADA_SetData(Model_No, ProjectName, TagNameList, ValueList);
		}

		#endregion
		private void DongJu_Load(object sender, EventArgs e)
		{
			List<TextBox> List = new List<TextBox>();
			void EnumControls(Control container)
			{
				//foreach (Control item in container.Controls)
				//{
				//	//c is the child control here
				//	EnumControls(item);
				//	if (item is TextBox)
				//		List.Add((TextBox)item);
				//}
				foreach (Control item in container.Controls)
				{
					//c is the child control here
					EnumControls(item);
					if (item is TextBox)
						List.Add((TextBox)item);
				}
			}
			//调用
			EnumControls(this);
			foreach (TextBox item in List)
				item.Text = IniFile_Dongju.ReadString("MES", item.Name.Replace("textBox_", ""), "");
		}

		private void button4_Click(object sender, EventArgs e)
		{
			textBox12.Text = SCADA_SetData(Model_No, ProjectName, TagNameList, ValueList);
		}
		public string SCADA_HandShake(string Model_No, string ProjectName, string EQP_ID)
		{
			SFCDLL SFCDLL = new SFCDLL();
			return SFCDLL.SCADA_HandShake(Model_No, ProjectName, EQP_ID);
		}
		private void button5_Click(object sender, EventArgs e)
		{
			textBox13.Text = SCADA_HandShake(Model_No, ProjectName, EQP_ID);
		}
	}
}
