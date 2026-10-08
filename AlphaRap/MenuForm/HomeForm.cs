using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;
using System.Threading;

namespace AlphaRap
{
	public partial class HomeForm : Form
	{
		public Classes.RFIDOperation RFIDOper = new Classes.RFIDOperation();

		public HomeForm()
		{
			InitializeComponent();

			// 状态块裁成圆角胶囊（工业 HMI 风格），并随尺寸变化重算
			label_ScannStaus.Resize += (s, ev) => ApplyPillRegion(label_ScannStaus, 6);
			label_PLCStaus.Resize += (s, ev) => ApplyPillRegion(label_PLCStaus, 6);
			ApplyPillRegion(label_ScannStaus, 6);
			ApplyPillRegion(label_PLCStaus, 6);
			// 图表只用于显示：正常已由 NoFocusChart 关掉焦点，
			// 这里再加一道保险，防止外部代码把焦点设到图表上后残留虚线焦点框
			chart1.Enter += ClearChartFocus;
			chart2.Enter += ClearChartFocus;
			bool bPlat = Convert.ToBoolean(SysPara.bPlat);

			MiddleLayer.MainF.tabPage10.Parent = bPlat ? null : MiddleLayer.MainF.uiTabControl1;
		}

		/// <summary>图表意外获得焦点时立刻把焦点交还出去，避免残留虚线焦点框。</summary>
		private void ClearChartFocus(object sender, EventArgs e)
		{
			try
			{
				Control c = sender as Control;
				if (c == null || !c.Focused) return;
				Form f = FindForm();
				if (f != null)
					BeginInvoke(new Action(delegate { try { f.ActiveControl = null; } catch { } }));
			}
			catch { }
		}

		/// <summary>把控件裁剪为圆角胶囊（状态指示块用）。</summary>
		private static void ApplyPillRegion(Control c, int radius)
		{
			if (c == null || c.Width <= 0 || c.Height <= 0) return;
			try
			{
				int d = Math.Max(2, Math.Min(radius * 2, Math.Min(c.Width, c.Height)));
				using (var path = new GraphicsPath())
				{
					path.AddArc(0, 0, d, d, 180, 90);
					path.AddArc(c.Width - d, 0, d, d, 270, 90);
					path.AddArc(c.Width - d, c.Height - d, d, d, 0, 90);
					path.AddArc(0, c.Height - d, d, d, 90, 90);
					path.CloseFigure();
					if (c.Region != null) c.Region.Dispose();
					c.Region = new Region(path);
				}
			}
			catch { }
		}

		private static readonly Color DeviceOnline = Color.FromArgb(34, 150, 83);
		private static readonly Color DeviceOffline = Color.FromArgb(214, 69, 69);

		private void timer1_Tick(object sender, EventArgs e)
		{
			// 虚拟键盘的**自动弹出已禁用**（原来焦点落在输入框就拉起 osk，弹窗抢焦点
			// 会把表格单元格的编辑直接取消掉，表现为"改了保存不上"）。
			// 需要软键盘时用主页顶栏的键盘图标手动调出（见 MainForm.ToggleVirtualKeyboard）。

			#region 外部应用状态
			// 只在状态变化时改色，避免每秒重绘
			Color scanColor = B_Scan1connect ? DeviceOnline : DeviceOffline;
			Color plcColor = B_PLCStaus ? DeviceOnline : DeviceOffline;
			if (label_ScannStaus.BackColor != scanColor) label_ScannStaus.BackColor = scanColor;
			if (label_PLCStaus.BackColor != plcColor) label_PLCStaus.BackColor = plcColor;
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

		/// <summary>设备在线检测周期（毫秒）。</summary>
		private const int DeviceCheckIntervalMs = 1500;

		void BgWork_Demo(object sender, DoWorkEventArgs e)
		{
			// 原来是 Thread.Sleep(10) 的死循环：每秒约 100 轮 × 2 次 Ping，
			// 还要每轮读两次配置。状态灯本身 1 秒才刷新一次，这么高的频率只是在白白占用 CPU 和网络。
			// 现在每 1.5 秒检测一次，程序退出（gEXIT）时结束循环。
			while (!MiddleLayer.gEXIT)
			{
				try
				{
					string plcIp = Convert.ToString(MiddleLayer.ParF.GetSettingValue("MSet", "PLCIP"));
					string scanIp = Convert.ToString(MiddleLayer.ParF.GetSettingValue("MSet", "ScannIP"));

					B_PLCStaus = MiddleLayer.ParF.PingTCP(plcIp);
					B_Scan1connect = MiddleLayer.ParF.PingTCP(scanIp);
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine("Device check failed: " + ex.Message);
				}
				Thread.Sleep(DeviceCheckIntervalMs);
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

		// 报警表缓存：语言 -> (编号 -> Content)。文件运行期不变，每种语言只解析一次
		// （AlarmRun 在后台线程刷新，UI 线程也会查，读写都走锁）。
		private static readonly Dictionary<string, Dictionary<string, string>> AlarmTableCache = new Dictionary<string, Dictionary<string, string>>();
		private static readonly object AlarmTableCacheLock = new object();

		private static Dictionary<string, string> LoadAlarmTableMap(LanguageType lang)
		{
			lock (AlarmTableCacheLock)
			{
				Dictionary<string, string> map;
				if (AlarmTableCache.TryGetValue(lang.ToString(), out map)) return map;

				map = new Dictionary<string, string>();
				try
				{
					// 枚举名即文件名：Chinese/English/Español；缺文件时回落英文，宁可显示英文报警也不能哑掉
					string file = System.Windows.Forms.Application.StartupPath + "\\AlarmTable\\" + lang + ".xml";
					if (!System.IO.File.Exists(file))
						file = System.Windows.Forms.Application.StartupPath + "\\AlarmTable\\English.xml";

					AlphaRap.srvConfigReadWriteXML srv = new AlphaRap.srvConfigReadWriteXML();
					System.Xml.XmlDocument doc = srv.XmlDocumentLoad(file);
					string inner = doc.FirstChild.InnerXml.Trim();
					if (inner.Length > 0)
					{
						string[] rows = inner.Replace("/><", "/>\n<").Split('\n');
						foreach (string forRow in rows)
						{
							string line = forRow.Trim();
							int sp = line.IndexOf(' ');
							if (sp < 3 || !line.StartsWith("<A")) continue;
							string code = line.Substring(2, sp - 2);
							int ci = line.IndexOf("Content=\"");
							if (ci < 0) continue;
							ci += "Content=\"".Length;
							int ce = line.IndexOf('"', ci);
							if (ce < 0) continue;
							map[code] = line.Substring(ci, ce - ci);
						}
					}
				}
				catch (Exception) { }
				AlarmTableCache[lang.ToString()] = map;
				return map;
			}
		}

		/// <summary>按指定语言查报警表内容（找不到返回 "Error"，与旧口径一致）。</summary>
		public string GetAlarmConentOf(LanguageType lang, string Index)
		{
			try
			{
				Dictionary<string, string> map = LoadAlarmTableMap(lang);
				string v;
				if (map.TryGetValue((Index ?? "").Trim(), out v)) return v;
			}
			catch (Exception) { }
			return "Error";
		}

		public string GetAlarmConent(string Index)
		{
			// 按当前语言取对应的报警表文件（枚举名即文件名：Chinese/English/Español），
			// 不再写死只有中英两个分支 —— AlarmTable 目录下三份文件齐全。
			return GetAlarmConentOf(SysPara.LanguageShow, Index);
		}

		/// <summary>
		/// 报警列表显示用的文案。NPSDK 驱动内部有些报警是两参数 Show(编号, 写死英文) 弹出的
		/// （IO/电机组件初始化失败那几条，还会带上 Name=/Port= 细节），这些文字不走报警表。
		/// 这里按编号在三种语言的表里做**前缀匹配**：能对上就把前缀换成当前语言的表内容、
		/// 保留后面的细节；完全对不上（纯自定义文本）就原样保留，避免丢信息。
		/// </summary>
		public string ResolveAlarmContent(string code, string stored)
		{
			try
			{
				if (string.IsNullOrEmpty(stored)) return stored;
				string cur = GetAlarmConentOf(SysPara.LanguageShow, code);
				if (cur == "Error" || string.IsNullOrEmpty(cur) || cur == stored) return stored;

				LanguageType[] all = (LanguageType[])Enum.GetValues(typeof(LanguageType));
				foreach (LanguageType lang in all)
				{
					string t = GetAlarmConentOf(lang, code);
					if (t != "Error" && !string.IsNullOrEmpty(t) && stored.StartsWith(t, StringComparison.Ordinal))
						return cur + stored.Substring(t.Length);
				}
			}
			catch (Exception) { }
			return stored;
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
	}
}
