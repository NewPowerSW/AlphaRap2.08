using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace AlphaRap
{
	public partial class HomeForm : Form
	{
		public Classes.RFIDOperation RFIDOper = new Classes.RFIDOperation();
        private string[] infoHeaders = new string[0];

		public HomeForm()
		{
			InitializeComponent();
            UiTheme.ApplyDpi(this);

			// 「生产信息」表格：表头与数据行全部由代码添加（见 SetInfoHeaders / AddInfoRow）
			SetupInfoTable();

            // VS 设计器：只把界面画出来，**不连设备、不读配置、不订阅事件**。
            // MiddleLayer 的静态初始化会构造硬件对象（Atlas_MTF6000），设计器进程里会抛异常，
            // 而构造函数末尾还要用 MiddleLayer.MainF —— 不提前返回的话设计器根本打不开这个窗体。
            if (UiTheme.IsDesignTime)
            {
                ApplyPageStyle();
                return;
            }

			// 设备连接列表跟随现有设备配置更新。
			_devicesChangedHandler = delegate { RefreshDeviceStatusSafe(); };
			DeviceControl.DevicesChanged += _devicesChangedHandler;
			this.Disposed += delegate { DeviceControl.DevicesChanged -= _devicesChangedHandler; };
			RebuildDeviceStatus();

			ApplyPageStyle();
			EventHandler languageHandler = delegate { RefreshPageTexts(); };
			MiddleLayer.LanguageChanged += languageHandler;
			Disposed += delegate { MiddleLayer.LanguageChanged -= languageHandler; };

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
                grdInfo.CellBorderStyle = DataGridViewCellBorderStyle.None;
                grdInfo.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
                grdInfo.GridColor = Color.FromArgb(226, 235, 244);
                grdInfo.DefaultCellStyle.Font = UiKit.Regular(9f);
                grdInfo.DefaultCellStyle.ForeColor = Color.FromArgb(40, 59, 83);
                grdInfo.DefaultCellStyle.SelectionBackColor = Color.FromArgb(225, 241, 252);
                grdInfo.DefaultCellStyle.SelectionForeColor = Color.FromArgb(20, 36, 59);
                grdInfo.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(247, 250, 254);
                grdInfo.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(246, 249, 252);
                grdInfo.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(99, 119, 143);
                grdInfo.ColumnHeadersDefaultCellStyle.Font = UiKit.Regular(9f);
                grdInfo.ColumnHeadersHeight = UiTheme.Dpi(32);
                grdInfo.RowTemplate.Height = UiTheme.Dpi(30);
                grdInfo.CellFormatting += FormatProductionCell;
                // 表头内边距收窄，长列名才放得下
                grdInfo.ColumnHeadersDefaultCellStyle.Padding = UiTheme.Dpi(new Padding(4, 0, 4, 0));
                // 列宽等分卡片宽度：永远不出横向滚动条；列多时表头靠 tooltip 看全
                grdInfo.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                if (btnInfoQuery != null)
                {
                    btnInfoQuery.Icon = AppIcons.Get(AppIcon.Search, UiTheme.Dpi(14), UiKit.TextPrimary);
                    btnInfoQuery.Click += delegate { MainForm.OpenProductionQuery(); };
                }

                // VS 设计器：画得和运行期一样（默认列 + 示例行），但不读配置、不落盘
                if (UiTheme.IsDesignTime)
                {
                    BuildInfoColumns(DefaultInfoHeaders());
                    ShowInfoRows(DesignSampleRows());
                    return;
                }

                string[] cols = ProductionDataStore.GetColumns();
                if (cols == null || cols.Length == 0)
                {
                    cols = DefaultInfoHeaders();
                    ProductionDataStore.SetColumns(cols);
                }
                BuildInfoColumns(cols);
                grdInfo.Rows.Clear();
                RefreshRecordCount();
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

        /// <summary>
        /// 设计期示例行 —— **只为让 VS 设计器里看到的和运行起来的 EXE 一致**，
        /// 运行期不会用到（设计器里没有真实数据）。
        /// </summary>
        private static IList<string[]> DesignSampleRows()
        {
            return new List<string[]>
            {
                new[] { "10:01:02", "PLT-0001", "A01", "0.42", "0.55", "0.31", "OK" },
                new[] { "10:01:36", "PLT-0002", "A01", "0.44", "0.56", "0.33", "OK" },
                new[] { "10:02:10", "PLT-0003", "A02", "0.39", "0.51", "0.28", "NG" },
                new[] { "10:02:47", "PLT-0004", "A02", "0.45", "0.57", "0.34", "OK" },
                new[] { "10:03:21", "PLT-0005", "A01", "0.41", "0.53", "0.30", "OK" }
            };
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
                RefreshRecordCount();
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
                RefreshRecordCount();
                if (rows == null) return;
                for (int i = 0; i < rows.Count; i++) AddInfoRowCore(rows[i]);
                grdInfo.ClearSelection();
            }
            catch { }
        }

        /// <summary>向只读生产记录追加一行，并刷新显示计数。</summary>
        private void AddInfoRowCore(string[] values)
        {
            if (grdInfo == null || grdInfo.Columns.Count == 0) return;
            object[] cells = new object[grdInfo.Columns.Count];
            for (int i = 0; i < cells.Length; i++)
                cells[i] = (values != null && i < values.Length) ? (object)(values[i] ?? "") : "";
            grdInfo.Rows.Add(cells);
            RefreshRecordCount();
        }

        /// <summary>清空界面上的数据行（保留表头，**不动已落盘的文件**）。</summary>
        public void ClearInfoRows()
        {
            if (grdInfo == null) return;
            try
            {
                grdInfo.Rows.Clear();
                grdInfo.ClearSelection();
                RefreshRecordCount();
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
                infoHeaders = new string[headers.Count];
                grdInfo.Columns.Clear();
                for (int i = 0; i < headers.Count; i++)
                {
                    infoHeaders[i] = headers[i] ?? "";
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

        /// <summary>生产字段是业务定义；切换语言时恢复原始表头而不重建列和数据行。</summary>
        private void RestoreInfoHeaders()
        {
            if (grdInfo == null) return;
            for (int i = 0; i < infoHeaders.Length && i < grdInfo.Columns.Count; i++)
                grdInfo.Columns[i].HeaderText = infoHeaders[i];
        }

        /// <summary>按结果字段的原始值强调合格与不合格，其他生产字段保持原值和默认样式。</summary>
        private void FormatProductionCell(object sender, DataGridViewCellFormattingEventArgs args)
        {
            if (args.ColumnIndex < 0 || args.ColumnIndex >= infoHeaders.Length) return;
            string header = infoHeaders[args.ColumnIndex];
            if (!string.Equals(header, "Result", StringComparison.OrdinalIgnoreCase) && header != "结果") return;
            string result = Convert.ToString(args.Value).Trim();
            if (string.Equals(result, "OK", StringComparison.OrdinalIgnoreCase) || string.Equals(result, "PASS", StringComparison.OrdinalIgnoreCase) || result == "合格")
                args.CellStyle.ForeColor = Color.FromArgb(0, 151, 102);
            if (string.Equals(result, "NG", StringComparison.OrdinalIgnoreCase) || string.Equals(result, "FAIL", StringComparison.OrdinalIgnoreCase) || result == "不合格")
                args.CellStyle.ForeColor = Color.FromArgb(239, 68, 78);
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

        #region 设备连接状态

        private EventHandler _devicesChangedHandler;
        private readonly List<Label> _deviceDots = new List<Label>();
        private readonly List<Label> _deviceNames = new List<Label>();
        private readonly List<Label> _deviceStates = new List<Label>();
        private bool _rebuildingDevStatus;

        /// <summary>把设备清单变更转到页面线程；设计期不加载设备配置。</summary>
        private void RefreshDeviceStatusSafe()
        {
            if (IsDisposed || UiTheme.IsDesignTime) return;
            if (!InvokeRequired) { RebuildDeviceStatus(); return; }
            if (!IsHandleCreated) return;
            try { BeginInvoke((MethodInvoker)RebuildDeviceStatus); }
            catch (InvalidOperationException) { }
        }

        /// <summary>从已有设备清单重建连接状态列表，不连接设备或执行生产动作。</summary>
        private void RebuildDeviceStatus()
        {
            if (connLayout == null || IsDisposed || UiTheme.IsDesignTime || _rebuildingDevStatus) return;
            _rebuildingDevStatus = true;
            try
            {
                EnsureParameterBuilt();
                BindDeviceStatuses(DeviceControl.GetDeviceStatuses());
            }
            finally { _rebuildingDevStatus = false; }
        }

        /// <summary>将设备名称和连接状态绑定到可滚动列表，并释放旧列表控件。</summary>
        private void BindDeviceStatuses(IList<DeviceControl.DeviceStatus> devices)
        {
            connLayout.SuspendLayout();
            try
            {
                while (connLayout.Controls.Count > 0) connLayout.Controls[0].Dispose();
                connLayout.RowStyles.Clear();
                _deviceDots.Clear();
                _deviceNames.Clear();
                _deviceStates.Clear();
                connLayout.RowCount = Math.Max(1, devices.Count);
                for (int i = 0; i < devices.Count; i++) AddDeviceStatusRow(devices[i], i);
                lblDeviceEmpty.Visible = devices.Count == 0;
                connLayout.Visible = devices.Count > 0;
                RefreshDeviceTexts();
            }
            finally { connLayout.ResumeLayout(true); }
        }

        /// <summary>添加一行真实设备状态，名称按设备配置显示，连接文案由语言入口提供。</summary>
        private void AddDeviceStatusRow(DeviceControl.DeviceStatus device, int index)
        {
            connLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, UiTheme.Dpi(40)));
            UiLabel dot = new UiLabel();
            dot.Name = "deviceDot" + index + "_dyn";
            dot.Text = "●";
            dot.Dock = DockStyle.Fill;
            dot.Margin = new Padding(0);
            dot.TextAlign = ContentAlignment.MiddleLeft;
            dot.Font = UiTheme.Body;
            UiLabel name = new UiLabel();
            name.Name = "deviceName" + index + "_dyn";
            name.Text = DeviceName(device);
            name.Dock = DockStyle.Fill;
            name.Margin = new Padding(0, 0, 4, 0);
            name.TextAlign = ContentAlignment.MiddleLeft;
            name.AutoEllipsis = true;
            name.Font = UiTheme.Body;
            name.ForeColor = Color.FromArgb(40, 59, 83);
            UiLabel state = new UiLabel();
            state.Name = "deviceState" + index + "_dyn";
            state.Dock = DockStyle.Fill;
            state.Margin = new Padding(0);
            state.AutoEllipsis = true;
            state.TextAlign = ContentAlignment.MiddleRight;
            state.Font = UiTheme.Body;
            state.Tag = device.IsConnected;
            connLayout.Controls.Add(dot, 0, index);
            connLayout.Controls.Add(name, 1, index);
            connLayout.Controls.Add(state, 2, index);
            _deviceDots.Add(dot);
            _deviceNames.Add(name);
            _deviceStates.Add(state);
        }

        /// <summary>显示设备配置名称；未命名时显示其真实类型名。</summary>
        private static string DeviceName(DeviceControl.DeviceStatus device)
        {
            return string.IsNullOrWhiteSpace(device.Name) ? device.TypeName : device.Name;
        }

        /// <summary>只刷新连接颜色和文案；设备数量、名称变更时重新绑定列表。</summary>
        private void RefreshDeviceStatusColors()
        {
            if (UiTheme.IsDesignTime || IsDisposed) return;
            List<DeviceControl.DeviceStatus> devices = DeviceControl.GetDeviceStatuses();
            if (devices.Count != _deviceStates.Count) { RebuildDeviceStatus(); return; }
            for (int i = 0; i < devices.Count; i++)
            {
                if (_deviceNames[i].Text != DeviceName(devices[i])) { RebuildDeviceStatus(); return; }
                _deviceStates[i].Tag = devices[i].IsConnected;
            }
            RefreshDeviceTexts();
        }

        /// <summary>按当前语言更新设备总数、在线及离线文案，不更改设备状态。</summary>
        private void RefreshDeviceTexts()
        {
            if (lblDeviceCount == null) return;
            string online = UiTheme.T("HomeForm", "devices_Online", "在线", "Online", "En línea");
            string offline = UiTheme.T("HomeForm", "devices_Offline", "离线", "Offline", "Sin conexión");
            SetText(lblDeviceCount, string.Format(UiTheme.T("HomeForm", "devices_Count", "{0} 台", "{0} devices", "{0} equipos"), _deviceStates.Count));
            for (int i = 0; i < _deviceStates.Count; i++)
            {
                bool connected = _deviceStates[i].Tag is bool && (bool)_deviceStates[i].Tag;
                Color color = connected ? DeviceOnline : DeviceOffline;
                _deviceDots[i].ForeColor = color;
                _deviceStates[i].ForeColor = color;
                SetText(_deviceStates[i], connected ? online : offline);
            }
        }

        /// <summary>确保现有参数页面已加载设备清单。</summary>
        private static void EnsureParameterBuilt()
        {
            if (MiddleLayer.ParF != null) MiddleLayer.ParF.EnsurePanelsBuilt();
        }

        #endregion

		#region 1.2 初始化加载点位事件
		private void HomeForm_Load(object sender, EventArgs e)
		{
			if (UiTheme.IsDesignTime) return;
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

        private static readonly Dictionary<string, Dictionary<string, string>> AlarmTableCache = new Dictionary<string, Dictionary<string, string>>();
        private static readonly object AlarmTableCacheLock = new object();

        /// <summary>按文件路径和修改时间缓存报警文本，兼容现场修改和配置目录。</summary>
        private static Dictionary<string, string> LoadAlarmTableMap(LanguageType lang)
        {
            string file = AlarmCatalog.FilePath(lang, true);
            string key = System.IO.Path.GetFullPath(file) + "|" + System.IO.File.GetLastWriteTimeUtc(file).Ticks;
            lock (AlarmTableCacheLock)
            {
                Dictionary<string, string> map;
                if (AlarmTableCache.TryGetValue(key, out map)) return map;
                map = new Dictionary<string, string>();
                if (!System.IO.File.Exists(file)) return map;
                foreach (System.Xml.Linq.XElement entry in System.Xml.Linq.XElement.Load(file).Elements())
                    map[entry.Name.LocalName.TrimStart('A')] = (string)entry.Attribute("Content") ?? "";
                if (AlarmTableCache.Count > 12) AlarmTableCache.Clear();
                AlarmTableCache[key] = map;
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
