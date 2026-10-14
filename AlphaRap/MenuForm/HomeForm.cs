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
            UiTheme.ApplyDpi(this);

			// 「生产信息」表格：字段名由操作员增删，值由外部代码写入（见 SetInfoValue）
			SetupInfoTable();

			// 底部设备状态条：按 Parameter 里的设备清单动态生成（增 / 删 / 改名 / 连接状态都会跟着变）
			_devicesChangedHandler = delegate { RefreshDeviceStatusSafe(); };
			DeviceControl.DevicesChanged += _devicesChangedHandler;
			this.Disposed += delegate { DeviceControl.DevicesChanged -= _devicesChangedHandler; };
			RebuildDeviceStatus();

			ApplyPageStyle();
			MiddleLayer.LanguageChanged += (s, e) => RefreshPageTexts();

			bool bPlat = Convert.ToBoolean(SysPara.bPlat);

			MiddleLayer.MainF.tabPage10.Parent = bPlat ? null : MiddleLayer.MainF.uiTabControl1;
		}

        // ---------------- 「生产信息」表格 ----------------
        // 自动设备：**表头（列）与数据行全部由代码添加**，界面不提供手工编辑（值只读）。
        // 同时**本地落盘**：{ProducPath}\ProductionData\{yyyy}\{MM}\{yyyy-MM-dd}.csv（一天一个文件）
        //
        //   SetInfoHeaders(new[] { "Time", "PalletCode", "Result" });   // 设列（同时写列定义）
        //   AddInfoRow("10:01:02", "PLT-0001", "OK");                   // 加一行（同时落盘，带时间戳）
        //
        // 列名是数据（与设备名 / 配方名同理），**不走语言包**。

        /// <summary>建表：套用扁平表格样式、载入列定义（没有就用默认列并落盘）。</summary>
        private void SetupInfoTable()
        {
            if (grdInfo == null) return;
            try
            {
                UiKit.StyleGrid(grdInfo);
                grdInfo.ReadOnly = true;
                grdInfo.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                grdInfo.MultiSelect = false;
                grdInfo.AllowUserToAddRows = false;
                grdInfo.AllowUserToDeleteRows = false;
                grdInfo.AllowUserToResizeRows = false;
                grdInfo.RowHeadersVisible = false;
                // 表头内边距收窄，长列名才放得下
                grdInfo.ColumnHeadersDefaultCellStyle.Padding = UiTheme.Dpi(new Padding(4, 0, 4, 0));
                // 列宽等分卡片宽度：永远不出横向滚动条；列多时表头靠 tooltip 看全
                grdInfo.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                if (btnInfoQuery != null)
                {
                    btnInfoQuery.Icon = AppIcons.Get(AppIcon.Search, UiTheme.Dpi(14), UiKit.TextPrimary);
                    btnInfoQuery.Click += delegate { MainForm.OpenProductionQuery(); };
                }

                string[] cols = ProductionDataStore.GetColumns();
                if (cols == null || cols.Length == 0)
                {
                    cols = DefaultInfoHeaders();
                    ProductionDataStore.SetColumns(cols);
                }
                BuildInfoColumns(cols);
                grdInfo.Rows.Clear();
            }
            catch { }
        }

        /// <summary>默认表头（占位，实际列名由业务代码 SetInfoHeaders 覆盖）。</summary>
        private static string[] DefaultInfoHeaders()
        {
            // ⚠ 用户给的参考表里写的是 Drver / Preasure，这里按正确拼写 Driver / Pressure
            return new string[] { "Time", "PalletCode", "Driver", "Pressure",
                                  "PressureMax", "PressureMin", "Result" };
        }

        /// <summary>设置表头（列）—— 同时写入本地列定义，之后 AddInfoRow 按这套列落盘。</summary>
        public void SetInfoHeaders(IList<string> headers)
        {
            if (headers == null || headers.Count == 0) return;
            try
            {
                string[] cols = new string[headers.Count];
                for (int i = 0; i < cols.Length; i++) cols[i] = headers[i] ?? "";
                ProductionDataStore.SetColumns(cols);
                BuildInfoColumns(cols);
                grdInfo.Rows.Clear();
            }
            catch { }
        }

        /// <summary>
        /// 追加一行生产数据：**既显示在首页表格里，也写入本地文件**（时间戳取当前时间）。
        /// 列数不足补空、超出忽略。返回错误信息，空串表示成功。
        /// </summary>
        public string AddInfoRow(params string[] values)
        {
            return AddInfoRowAt(DateTime.Now, values);
        }

        /// <summary>上一次落盘失败的提示（用来避免每次失败都写日志）。</summary>
        private static string _infoLastError = "";

        /// <summary>追加一行并指定时间（补录 / 导入用）。既显示也落盘。</summary>
        public string AddInfoRowAt(DateTime time, string[] values)
        {
            string err = "";
            try
            {
                AddInfoRowCore(values);
                err = ProductionDataStore.Append(time, values);
            }
            catch (Exception ex) { err = ex.Message; }

            // 落盘失败不能让车间静默丢数据：错误一变就记一条运行日志
            if (!string.Equals(err, _infoLastError, StringComparison.Ordinal))
            {
                _infoLastError = err;
                if (err.Length > 0)
                    OperationLog.Write("HomeForm", "生产数据落盘失败：" + err);
            }
            return err;
        }

        /// <summary>只在界面上显示若干行（**不落盘**，用于回填 / 预览）。</summary>
        public void ShowInfoRows(IList<string[]> rows)
        {
            if (grdInfo == null) return;
            try
            {
                grdInfo.Rows.Clear();
                if (rows == null) return;
                for (int i = 0; i < rows.Count; i++) AddInfoRowCore(rows[i]);
                grdInfo.ClearSelection();
            }
            catch { }
        }

        private void AddInfoRowCore(string[] values)
        {
            if (grdInfo == null || grdInfo.Columns.Count == 0) return;
            object[] cells = new object[grdInfo.Columns.Count];
            for (int i = 0; i < cells.Length; i++)
                cells[i] = (values != null && i < values.Length) ? (object)(values[i] ?? "") : "";
            grdInfo.Rows.Add(cells);
        }

        /// <summary>清空界面上的数据行（保留表头，**不动已落盘的文件**）。</summary>
        public void ClearInfoRows()
        {
            if (grdInfo == null) return;
            try
            {
                grdInfo.Rows.Clear();
                grdInfo.ClearSelection();
            }
            catch { }
        }

        /// <summary>当前列数。</summary>
        public int InfoColumnCount { get { return grdInfo == null ? 0 : grdInfo.Columns.Count; } }

        /// <summary>界面当前显示的数据行数（不是文件里的条数）。</summary>
        public int InfoRowCount { get { return grdInfo == null ? 0 : grdInfo.Rows.Count; } }

        /// <summary>重建列（全部只读）。</summary>
        private void BuildInfoColumns(IList<string> headers)
        {
            if (grdInfo == null || headers == null || headers.Count == 0) return;
            try
            {
                grdInfo.Columns.Clear();
                for (int i = 0; i < headers.Count; i++)
                {
                    DataGridViewTextBoxColumn c = new DataGridViewTextBoxColumn();
                    c.Name = "infoCol" + i;
                    c.HeaderText = headers[i] ?? "";
                    c.HeaderCell.ToolTipText = c.HeaderText;   // 列多时表头可能被截断，悬停可看全
                    c.ReadOnly = true;
                    c.SortMode = DataGridViewColumnSortMode.NotSortable;
                    c.MinimumWidth = UiTheme.Dpi(52);
                    grdInfo.Columns.Add(c);
                }
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
			// 生产数据：累计良品 / 不良、节拍、当天每小时数量
			string ct = !string.IsNullOrEmpty(SysPara.CircleTime) ? SysPara.CircleTime
					  : (MiddleLayer.MainF != null ? MiddleLayer.MainF.txtCyCT.Text : "");
			ShowValues(SysPara.iProductOK, SysPara.iProductNG, ct,
					   SysPara.iProductHourlyOutput, SysPara.iProductHourlyReject, DateTime.Now.Hour);

			// 设备状态圆点：只在颜色真的变了才改（名称与颜色都来自 Parameter 的设备清单）
			RefreshDeviceStatusColors();
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

		#region 设备状态条（按 Parameter 的设备清单动态生成）

		/// <summary>设备清单 / 连接状态变化时由 DeviceControl 广播过来。</summary>
		private EventHandler _devicesChangedHandler;

		/// <summary>状态条上的圆点（下标与设备顺序一一对应，用来只刷颜色不重建）。</summary>
		private readonly List<Label> _deviceDots = new List<Label>();

		/// <summary>防重入：EnsureParameterBuilt() 会同步广播一次，嵌套进来时直接返回，外面那层会建好。</summary>
		private bool _rebuildingDevStatus;

		/// <summary>可能在非 UI 线程被通知，统一转到 UI 线程再重建。</summary>
		private void RefreshDeviceStatusSafe()
		{
			if (IsDisposed) return;
			if (InvokeRequired)
			{
				try { BeginInvoke((MethodInvoker)RebuildDeviceStatus); } catch (Exception) { }
				return;
			}
			RebuildDeviceStatus();
		}

		/// <summary>
		/// 重建底部设备状态条：**一台设备一组**「状态圆点 + 设备名」，顺序与 Parameter 的页签一致。
		/// 内容全部来自 Parameter 的设备清单（DeviceControl.GetDeviceStatuses），
		/// 所以设备**改名 / 删除 / 新增**后这里自动跟着变；圆点颜色 = 该设备的连接状态。
		/// </summary>
		private void RebuildDeviceStatus()
		{
			if (connLayout == null || IsDisposed || _rebuildingDevStatus) return;
			_rebuildingDevStatus = true;
			try
			{
				EnsureParameterBuilt();      // 没进过 Parameter 页时先把设备清单按 XML 读出来（否则这里是空的）

				List<DeviceControl.DeviceStatus> list = DeviceControl.GetDeviceStatuses();

				connLayout.SuspendLayout();
				connLayout.Controls.Clear();
				_deviceDots.Clear();

				for (int i = 0; i < list.Count; i++)
				{
					// 状态圆点（12x12 胶囊：已连接绿 / 未连接红）
					UiLabel dot = new UiLabel();
					dot.Name = "devDot" + (i + 1);
					dot.AutoSize = false;
					dot.Size = new Size(12, 12);            // 96 DPI 设计尺寸，添加时由 ApplyDpi 的钩子放大
					dot.Margin = new Padding(0, 9, 6, 0);
					dot.BackColor = list[i].IsConnected ? DeviceOnline : DeviceOffline;
					dot.Resize += (s, ev) => ApplyPillRegion((Control)s, 6);
					connLayout.Controls.Add(dot);
					ApplyPillRegion(dot, 6);
					_deviceDots.Add(dot);

					// 设备名：直接显示 Parameter 里的"设备名"（数据、不接语言包，改名后这里跟着改）
					UiLabel name = new UiLabel();
					name.Name = "devName" + (i + 1);
					name.AutoSize = true;
					name.Font = UiTheme.Body;
					name.ForeColor = UiKit.TextPrimary;
					name.Margin = new Padding(0, 5, 22, 0);
					name.Text = string.IsNullOrEmpty(list[i].Name) ? list[i].TypeName : list[i].Name;
					connLayout.Controls.Add(name);
				}
			}
			catch (Exception) { }
			finally
			{
				try { connLayout.ResumeLayout(true); } catch (Exception) { }
				_rebuildingDevStatus = false;
			}
		}

		/// <summary>定时刷新圆点颜色（每秒一次；数量对不上说明清单变了，直接整体重建）。</summary>
		private void RefreshDeviceStatusColors()
		{
			try
			{
				List<DeviceControl.DeviceStatus> list = DeviceControl.GetDeviceStatuses();
				if (list.Count != _deviceDots.Count) { RebuildDeviceStatus(); return; }

				for (int i = 0; i < list.Count; i++)
				{
					Color c = list[i].IsConnected ? DeviceOnline : DeviceOffline;
					if (_deviceDots[i].BackColor != c) _deviceDots[i].BackColor = c;
				}
			}
			catch (Exception) { }
		}

		/// <summary>确保 Parameter 的设备清单已经按 XML 建好（不依赖用户有没有打开过那一页）。</summary>
		private static void EnsureParameterBuilt()
		{
			try { if (MiddleLayer.ParF != null) MiddleLayer.ParF.EnsurePanelsBuilt(); }
			catch (Exception) { }
		}

		#endregion

		#region 1.2 初始化加载点位事件
		private void HomeForm_Load(object sender, EventArgs e)
		{
			timer1.Enabled = true;

			
			MiddleLayer.HardF.StopAllMotor();
		}

		#endregion

		public void DataINITIAL(DataGridView Position, int RowIndex, int Columns)

		{
			Position.Rows.Clear();

			Position.RowCount = RowIndex;
			Position.ColumnCount = Columns;

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
			// 按当前语言取对应的报警表文件（AlarmTable\{Chinese|English|Español}.xml）
			return GetAlarmConentOf(SysPara.LanguageShow, Index);
		}

		/// <summary>
		/// 报警列表显示用的文案：按编号在三种语言的报警表中做前缀匹配，匹配到则把前缀换成当前语言的内容并保留后面的细节
		/// （如 NPSDK 初始化失败报警中的 Name= / Port=），匹配不到则原样返回。
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
