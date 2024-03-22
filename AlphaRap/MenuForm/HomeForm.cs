using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using NPSDK;
using AlphaRap.FunctionForms;
using Alpha._0;

namespace AlphaRap
{
	public partial class HomeForm : Form
	{
		[DllImport("user32.dll")]
		public static extern IntPtr GetFocus();

		public Point ppt;

		public Classes.RFIDOperation RFIDOper = new Classes.RFIDOperation();



		public HomeForm()
		{
			InitializeComponent();
			bool bPlat = Convert.ToBoolean(SysPara.bPlat);

			MiddleLayer.MainF.tabPage10.Parent = bPlat ? null : MiddleLayer.MainF.uiTabControl1;


		}

		private void timer1_Tick(object sender, EventArgs e)
		{

			#region KEYBORD
			IntPtr _ControlIntPtr = GetFocus();
			if (_ControlIntPtr != IntPtr.Zero)
			{
				Control _Control = Control.FromChildHandle(_ControlIntPtr);
				if (_Control != null)
				{
					Control _gControl = _Control;
					var retval = new Point(0, 0);
					for (; _gControl.Parent != null; _gControl = _gControl.Parent)
					{
						retval.Offset(_gControl.Location);
						if (_gControl.Name == MiddleLayer.MainF.forkeybord.Name)
						{
							break;
						}
					}

					bool MOUSEXY = ppt.X > retval.X && ppt.X < (retval.X + _Control.Width) && ppt.Y > retval.Y && ppt.Y < (retval.Y + _Control.Height);
					if ((_Control.GetType() == typeof(TextBox) || _Control.GetType() == typeof(DataGridViewTextBoxEditingControl) || _Control.GetType() == typeof(NPSDK.Keyence.Keyence_Text)) && MOUSEXY)
					{
						if (System.Diagnostics.Process.GetProcessesByName("osk").Length == 0 && System.IO.File.Exists(@"C:\Windows\system32\osk.exe"))
						{
							try
							{
								System.Diagnostics.Process.Start(@"C:\Windows\system32\osk.exe");
							}
							catch { }
						}
					}
					else
					{
						if (System.Diagnostics.Process.GetProcessesByName("osk").Length > 0)
						{
							Process[] a = System.Diagnostics.Process.GetProcessesByName("osk");
							foreach (Process b in a)
							{
								try
								{
									//  b.Kill();
								}
								catch
								{ }
							}
						}
					}
				}
			}
			#endregion
			#region 外部应用状态
			label_ScannStaus.BackColor = B_Scan1connect ? Color.Green:Color.Red;
			label_PLCStaus.BackColor = B_PLCStaus ? Color.Green : Color.Red;
			#endregion
		}

		#region Alarm Data
		public void WriteExcelData()
		{
			string[] strArrange = new string[22];
			SysPara.AlarmMessagePath = MiddleLayer.LogF.GetSettingValue("Path", "AlarmPath") + "\\AlarmMessageData\\" + DateTime.Now.Year + "\\" + DateTime.Now.Month + "\\" + DateTime.Now.Day + "\\";

			string strFileName = SysPara.AlarmMessagePath + DateTime.Now.ToString("yyyy-MM-dd") + ".csv";
			FileInfo fileInfo = new FileInfo(strFileName);
			if (fileInfo.Exists)
			{
				for (int i = 0; i < MiddleLayer.MainF.WarnningMessage.Items.Count; i++)
				{
					ListViewItem lvi = MiddleLayer.MainF.WarnningMessage.Items[i];
					for (int j = 0; j < lvi.SubItems.Count; j++)
					{
						strArrange[j] = lvi.SubItems[j].Text;
					}
					WriteExcelData(strFileName, strArrange);
				}
			}
			else
			{
				for (int i = 0; i < MiddleLayer.MainF.WarnningMessage.Columns.Count; i++)
				{
					strArrange[i] = MiddleLayer.MainF.WarnningMessage.Columns[i].Text;
				}

				DirectoryInfo dirInfo = new DirectoryInfo(SysPara.AlarmMessagePath);
				if (!dirInfo.Exists)
				{
					try
					{
						dirInfo.Create();
					}
					catch (Exception)
					{
						MiddleLayer.LogF.SettingData.Tables["Path"].Rows[0]["AlarmPath"] = "C:\\Log";
					}
				}

				for (int i = 0; i < MiddleLayer.MainF.WarnningMessage.Items.Count; i++)
				{
					ListViewItem lvi = MiddleLayer.MainF.WarnningMessage.Items[i];
					for (int j = 0; j < lvi.SubItems.Count; j++)
					{
						strArrange[j] = lvi.SubItems[j].Text;
					}
					WriteExcelData(strFileName, strArrange);
				}
			}
		}
		public void WriteExcelData(string strFileName, string[] strArange)
		{
			ListViewWrite.WriteCSV(strFileName, strArange);
		}
		#endregion

		//BacodeScanner
	
		public static void RefreshDifferentThreadUI(Control control, Action action)
		{
			if (control.InvokeRequired)
			{
				Action refreshUI = new Action(action);
				control.Invoke(refreshUI);
			}
			else
			{
				action.Invoke();
			}
		}

		#region 状态

		BackgroundWorker B_BgWork = new BackgroundWorker();
		/// <summary>
		/// 扫码枪连接状态
		/// </summary>
		public bool B_Scan1connect = false;
		/// <summary>
		/// PLC连接状态
		/// </summary>
		public bool B_PLCStaus = false;
		#region //后台

		void BgWork_Demo(object sender, DoWorkEventArgs e)
		{
			while (true)
			{
				Thread.Sleep(10);
				try
				{
				
					//#region //PING 各个设备IP

					B_PLCStaus = MiddleLayer.ParF.PingTCP(MiddleLayer.ParF.GetSettingValue("MSet", "PLCIP"));
					B_Scan1connect = MiddleLayer.ParF.PingTCP(MiddleLayer.ParF.GetSettingValue("MSet", "ScannIP"));

					//#endregion

				}
				catch (Exception ex)
				{
					//MessageBox.Show("程序出来点小问题..." + ex.Message, "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
				}
			}
		}

		#endregion
		#region 1.2 初始化加载点位事件
		private void HomeForm_Load(object sender, EventArgs e)
		{
			
			timer1.Enabled = true;

			B_BgWork.DoWork += BgWork_Demo;
			B_BgWork.RunWorkerAsync();

			MiddleLayer.HardF.StopAllMotor();
		}

#endregion
		#endregion

		public void DataINITIAL(DataGridView Position, int RowIndex, int Columns)

		{


			Position.Rows.Clear();

			Position.RowCount = RowIndex;
			Position.ColumnCount = Columns;

			// Position.Columns[0].Width = (Position.Width- Position.RowHeadersWidth)/ 2;




			for (int i = 0; i < Columns; i++)
			{
				Position.Columns[i].Width = (Position.Width) / Columns;
				Position.Columns[i].HeaderCell.Value = (i + 1).ToString();


			}

			for (int i = 0; i < RowIndex; i++)
			{
				Position.Rows[i].Height = (Position.Height) / RowIndex;
				Position.Rows[i].HeaderCell.Value = (i + 1).ToString();
				Position.Rows[i].Cells[0].Value = "";


			}

			Position.ClearSelection();


		}


		public void SetCellColor(DataGridView Position, int RowIndex, int Columns, Color CellColor)
		{
			Position[Columns, RowIndex].Style.BackColor = CellColor;

		}

		public void SetCellValue(DataGridView Position, int RowIndex, int Columns, string CellValue)
		{
			Position[Columns, RowIndex].Value = CellValue;
		}

		public string GetAlarmConent(string Index)
		{
			DataTable dtTable = new DataTable();
			List<string> mlist;
			dtTable.Rows.Clear();
			AlphaRap.srvConfigReadWriteXML srv = new AlphaRap.srvConfigReadWriteXML();

			System.Xml.XmlDocument _xmlDoc = SysPara.LanguageShow == LanguageType.English ? srv.XmlDocumentLoad(System.Windows.Forms.Application.StartupPath + "\\AlarmTable\\English.xml") : srv.XmlDocumentLoad(System.Windows.Forms.Application.StartupPath + "\\AlarmTable\\Chinese.xml");
			string strInnerXml = _xmlDoc.FirstChild.InnerXml;
			if (strInnerXml.Trim().Length > 0)
			{
				mlist = strInnerXml.Trim().Replace("/><", "/>\n<").Split('\n').ToList();
				foreach (string forRor in mlist)
				{
					string strValue = forRor.Substring(2, forRor.IndexOf(' ') - 1);
					string strIndexOf = "Content=\"";
					int iIndex = forRor.IndexOf(strIndexOf) + strIndexOf.Trim().Length;
					string strContent = forRor.Substring(iIndex, forRor.Trim().Length - iIndex);
					strContent = strContent.Substring(0, strContent.IndexOf('\"'));
					if (string.Equals(strValue.Trim(), Index.Trim()))
					{
						return strContent;
					}
				}
			}
			return "Error";
		}

		public bool TestJIG_Clear = false;
		public void refleshDatagriviewColor(DataGridView Position, int Row, int Columns, Color CellColor)
		{
			for (int i = 0; i < Row; i++)
			{
				for (int j = 0; j < Columns; j++)
				{
					SetCellColor(Position, i, j, CellColor);
				}
			}
		}

		private void button1_Click_1(object sender, EventArgs e)
		{
			NPSDK.Flow_Module.Module_AddAlarmLog("Alarm>>Code:");
		}

		
	}
}
