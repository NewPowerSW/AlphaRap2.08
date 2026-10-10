using System;
using System.Windows.Forms;
using AlphaRapLibrary;
using System.Drawing;
using System.Data;
using System.Collections.Generic;

namespace AlphaRap
{
    public partial class VPForm : ModuleBaseForm
	{
		public VPForm()
        {
            InitializeComponent();

            // 移除设计器中的预览页；真实相机页按 VPForm.Cameras.xml 生成（见 BuildVpPages）
            RemoveDesignPreviewPage();

            // 固定视觉工位（供 TaskProcess/Gantry.cs 调用，如 MiddleLayer.VPF.H1_VFiducial.RunTB()）。
            // 画面不再送主界面：主界面"视觉"页只显示配置里的相机（见 BuildVpPages），老工位只出结果不出图。

            // 相机 / VPP 配置保存在独立的 VPForm.Cameras.xml（见 VpConfigStore），界面在 ModuleInitialize 后构建（见 BuildVpUi）
            // 语言切换时刷新含动态内容的文字（相机名 / 路径 / 点数 / 实时状态 / 工具条提示）
            MiddleLayer.LanguageChanged += delegate { RefreshDynamicTexts(); };
		}
		// 视觉工位：VpStation(vpp 目录名, 工位类型, 默认 CCD 号)
		//   · 字段名供生产流程调用（TaskProcess/Gantry.cs 中的 MiddleLayer.VPF.XXX）
		//   · 默认 CCD 号运行时会被配方中的 Pset 覆盖
		//   · 工位类型（Calibration / Fiducial）决定取哪些输出、十字线大小

		public VpStation H1_VCalibration = new VpStation("Camera1", VpStationKind.Calibration, 3);
		public VpStation H1_VFiducial = new VpStation("Camera1", VpStationKind.Fiducial, 3);
		public VpStation H1_VFiducial2 = new VpStation("Camera1", VpStationKind.Fiducial, 3);
		public VpStation H1_VFiducial3 = new VpStation("Camera1", VpStationKind.Fiducial, 3);
		public VpStation H1_VFiducial4 = new VpStation("Camera1", VpStationKind.Fiducial, 3);

		public struct VisionPostData
        {
            public int Point;
            public double X;
            public double Y;
            public double R;

            public double X_Low;
            public double X_Hi;
            public double Y_Low;
            public double Y_Hi;
            public double R_Low;
            public double R_Hi;

            public bool Enable;
        }

        /// <summary>
        /// 生产流程用的视觉判据（TaskProcess/Gantry.cs 读取 dgv_H1_VisionData_List[0]），
        /// 数据来自配方表 tb_H1_Visdata，由 <see cref="Getdgv_H1_VisionDataList"/> 加载。
        /// </summary>
        public List<VisionPostData> dgv_H1_VisionData_List = new List<VisionPostData>();

        /// <summary>把配方表 tb_H1_Visdata 刷进 <see cref="dgv_H1_VisionData_List"/>。</summary>
        public void Getdgv_H1_VisionDataList()
        {
            dgv_H1_VisionData_List.Clear();
            DataTable H1dt = MiddleLayer.VPF.RecipeData.Tables["tb_H1_Visdata"];
            for (int i = 0; i < H1dt.Rows.Count; i++)
            {
                DataRow dr = H1dt.Rows[i];
                VisionPostData data = new VisionPostData { Point = i, X = Convert.ToDouble(dr[0]), Y = Convert.ToDouble(dr[1]), R = Convert.ToDouble(dr[2]), X_Low = Convert.ToDouble(dr[3]), X_Hi = Convert.ToDouble(dr[4]), Y_Low = Convert.ToDouble(dr[5]), Y_Hi = Convert.ToDouble(dr[6]),R_Low = Convert.ToDouble(dr[7]), R_Hi = Convert.ToDouble(dr[8]), Enable = Convert.ToBoolean(dr[9]) };
				dgv_H1_VisionData_List.Add(data);
            }
        }

		#region 动态相机 / VPP

		// 页面结构：
		//   tabControl1 的一页 = 一台相机，相机页里的子 TabControl 一页 = 该相机下的一个 VPP
		//   VpStation 按"相机名\VPP名"存放 vpp，算法由 vpp 中的 ToolBlock 决定
		// 配置文件：ModuleData\SettingData\VPForm.Cameras.xml（机器级，不随配方变）
		// vpp 文件：VisionData\{相机名}\{VPP名}\{配方名}.vpp

		// 运行时构建的相机页 / VPP 页的控件名前缀
		private const string DynCameraPrefix = "dynCam_";
		private const string DynVppPrefix = "dynVppPage_";
		/// <summary>设计器预览页的页名（运行时移除）。</summary>
		private const string PreviewPageName = "tabPageDesignPreview";

		private Panel _vpBar;
		private bool _vpUiBuilt;
		/// <summary>建页失败的原因（非空时显示在工具条上）。</summary>
		private string _vpBuildError;
		/// <summary>VPForm.Cameras.xml 的全部内容：相机 / VPP / 各自的标定。</summary>
		private readonly VpConfigFile _config = new VpConfigFile();

		/// <summary>配置中的相机列表。</summary>
		private List<VpCameraConfig> _cameras { get { return _config.Cameras; } }

		/// <summary>
		/// 一个相机页的运行时数据。每台相机一份标定（<see cref="VpCalibration"/>），应用到该相机下的所有 VPP 工位（Targets）。
		/// </summary>
		private class CameraEntry
		{
         /// <summary>相机键（即相机名）。</summary>
            public string Key;
			public TabPage Page;
			/// <summary>产生这一页的配置。</summary>
			public VpCameraConfig Config;
			/// <summary>本相机下要应用标定的 VPP 工位。</summary>
			public List<VpStation> Targets = new List<VpStation>();
			/// <summary>标定卡片落在哪个容器里。</summary>
			public Control CalibHost;
			/// <summary>整台相机共用的显示控件：标定和本相机下每个 VPP 的结果都显示在这里。</summary>
			public Cognex.VisionPro.CogRecordDisplay Display;
			/// <summary>标定卡片是否铺满宿主（动态相机的 calibHost 是专用容器，铺满）。</summary>
			public bool HostFill;
			/// <summary>相机属性条上的"实时显示"按钮（文字随语言和实时状态变化）。</summary>
			public Button LiveButton;
		}

		private readonly List<CameraEntry> _entries = new List<CameraEntry>();
		/// <summary>各 VPP 页上的补偿限制表（切换语言时重填表头；页面重建时清空）。</summary>
		private readonly List<DataGridView> _compGrids = new List<DataGridView>();

		private CameraEntry FindEntry(string key)
		{
			for (int i = 0; i < _entries.Count; i++)
				if (_entries[i].Key == key) return _entries[i];
			return null;
		}

		/// <summary>基类初始化完成后构建视觉界面。</summary>
		public override void ModuleInitialize(string ModuleName)
		{
			base.ModuleInitialize(ModuleName);
			try
			{
				BuildVpUi();
			}
			catch (Exception ex)
			{
				// 建页失败的原因写入调试输出并显示在工具条上
				_vpBuildError = ex.GetType().Name + ": " + ex.Message;
				System.Diagnostics.Debug.WriteLine("[VPForm] BuildVpUi 失败 → " + ex);
				UpdateVpHint();
			}
		}

		/// <summary>工具条右侧的提示标签：建页出错时以红字显示原因，平时为空。</summary>
		private UiLabel FindBarHint()
		{
			if (_vpBar == null) return null;
			for (int i = 0; i < _vpBar.Controls.Count; i++)
			{
				UiLabel l = _vpBar.Controls[i] as UiLabel;
				if (l != null && l.Name == DynTextName("vpBarHint")) return l;
			}
			return null;
		}

		private void UpdateVpHint()
		{
			UiLabel lbl = FindBarHint();
			if (lbl == null) return;

			if (!string.IsNullOrEmpty(_vpBuildError))
			{
				lbl.ForeColor = Color.FromArgb(200, 60, 60);
				lbl.Text = T("vpHint_BuildError", "界面搭建出错（其余部分仍可用）：") + _vpBuildError;
				return;
			}

			lbl.ForeColor = Color.FromArgb(110, 120, 132);
			lbl.Text = "";
		}

		private void BuildVpUi()
		{
			if (_vpUiBuilt) return;
			_vpUiBuilt = true;

			CreateVpToolBar();
			LoadConfigFromDisk();
			BuildVpPages();
		}

		/// <summary>把 VPForm.Cameras.xml 读进 <see cref="_config"/>。</summary>
		private void LoadConfigFromDisk()
		{
			VpConfigFile loaded = VpConfigStore.Load();
			_config.Cameras.Clear();
			_config.Cameras.AddRange(loaded.Cameras);
			_config.Calibrations.Clear();
			foreach (KeyValuePair<string, VpCalibration> kv in loaded.Calibrations)
				_config.Calibrations[kv.Key] = kv.Value;
		}

		/// <summary>按当前 <see cref="_config"/> 构建全部相机页。</summary>
		private void BuildVpPages()
		{
			// 先让主界面"视觉"页按相机把显示格子建好，下面建页时 RegisterDisplay 才取得到对应显示
			SyncMainDisplays();

			for (int i = 0; i < _cameras.Count; i++)
			{
				CreateCameraStations(_cameras[i]);
				tabControl1.TabPages.Add(BuildCameraPage(_cameras[i]));
			}
		}

		/// <summary>
		/// 把主界面"视觉"页的显示格子按当前相机列表同步，并把"本相机在主界面的显示"
		/// 重新挂进本相机各工位（含标定站），使两边显示同一张图。
		/// 相机增删、改名后必须调用，否则主界面不会出现 / 移除对应的显示格子。
		/// </summary>
		private void SyncMainDisplays()
		{
			List<string> camKeys = new List<string>();
			for (int i = 0; i < _cameras.Count; i++) camKeys.Add(_cameras[i].Name);
			try { MiddleLayer.MainF.SetCameraDisplays(camKeys); } catch (Exception) { }

			for (int i = 0; i < _cameras.Count; i++)
			{
				VpCameraConfig cam = _cameras[i];
				CameraEntry e = FindEntry(cam.Name);
				if (e == null) continue;

				for (int j = 0; j < cam.Vpps.Count; j++)
					if (cam.Vpps[j].Station != null) RegisterDisplay(cam.Vpps[j].Station, e.Display);

				VpCalibration calib = _config.GetCalibration(cam.Name);
				if (calib != null && calib.Station != null) RegisterDisplay(calib.Station, e.Display);
			}
		}

		/// <summary>操作日志里用的"相机 / VPP"标识（拿不到相机名时退化为 VPP 名）。</summary>
		private static string LogTag(VpVppConfig vpp)
		{
			if (vpp == null) return "?";
			string cam = (vpp.Station != null) ? vpp.Station.CameraName : null;
			return (string.IsNullOrEmpty(cam) ? "" : cam + " / ") + vpp.Name;
		}

		/// <summary>
		/// 移除设计器预览页（tabPageDesignPreview）。该页只用于在设计视图中显示相机页骨架。
		/// </summary>
		private void RemoveDesignPreviewPage()
		{
			try
			{
				if (tabControl1 == null) return;
				for (int i = tabControl1.TabPages.Count - 1; i >= 0; i--)
				{
					TabPage p = tabControl1.TabPages[i];
					if (p == null || p.Name != PreviewPageName) continue;
					tabControl1.TabPages.RemoveAt(i);
					p.Dispose();
				}
			}
			catch (Exception) { }
		}

		// ---------------- 顶部工具条 ----------------

		private void CreateVpToolBar()
		{
			if (_vpBar != null) return;

			Panel bar = new Panel();
			_vpBar = bar;
			bar.Name = "panelVpBar";
			bar.Dock = DockStyle.Top;
			bar.Height = 46;
			bar.BackColor = Color.FromArgb(238, 243, 249);

			FlowLayoutPanel btns = new FlowLayoutPanel();
			btns.Name = "vpBarButtons";
			btns.Dock = DockStyle.Left;
			btns.FlowDirection = FlowDirection.LeftToRight;
			btns.WrapContents = false;
			btns.AutoSize = false;
			btns.Width = 720;
			btns.Padding = new Padding(8, 7, 8, 7);
			btns.BackColor = bar.BackColor;

			AddBarButton(btns,  "vpBar_AddCamera", "添加相机", delegate { AddCamera(); });
			AddBarButton(btns,  "vpBar_RemoveCamera", "删除当前相机", delegate { DeleteCurrentCameraPage(); });
			AddBarButton(btns,  "vpBar_AddVpp", "添加 VPP", delegate { AddVppToCurrentCamera(); });
			AddBarButton(btns,  "vpBar_RemoveVpp", "删除当前 VPP", delegate { DeleteCurrentVpp(); });

			// 建页出错时显示原因（见 UpdateVpHint）；文字动态生成，不进语言表（_dyn）
			UiLabel hint = new UiLabel();
			hint.Name = DynTextName("vpBarHint");
			hint.Dock = DockStyle.Fill;
			hint.AutoSize = false;
			hint.TextAlign = ContentAlignment.MiddleRight;
			hint.Font = new Font("宋体", 10.5F);
			hint.ForeColor = Color.FromArgb(110, 120, 132);
			hint.Padding = new Padding(0, 0, 12, 0);

			// 停靠顺序：后加的先生效，先 Left（按钮条）再 Fill（提示）
			bar.Controls.Add(hint);
			bar.Controls.Add(btns);

			// tabControl1 为 Dock=Fill，工具条最后加入才能占据顶部
			Controls.Add(bar);

			// 按控件名登记进语言表，切换语言时由 SwitchLanguage 更新文字
			MiddleLayer.RegisterLanguage(bar, LangForm);
		}


		// ---------------- 相机 ----------------

		private void AddCamera()
		{
			if (!EnsureIdle(T("vpAct_AddCamera", "添加相机"))) return;

			int n = _cameras.Count + 1;
			string name = "Camera" + n;
			while (FindCamera(name) != null) { n++; name = "Camera" + n; }

			VpCameraConfig cam = new VpCameraConfig();
			cam.Name = name;
			cam.CameraIndex = 0;
			cam.Exposure = 10;
			_cameras.Add(cam);

			// 先在主界面"视觉"页补上这台相机的显示格子，下面建页时 RegisterDisplay 才取得到它
			SyncMainDisplays();

			TabPage page = BuildCameraPage(cam);
			tabControl1.TabPages.Add(page);
			tabControl1.SelectedTab = page;

			OperationLog.Write("VPForm", "添加相机：" + cam.Name);
			SaveVpConfig();
		}

		/// <summary>
		/// 构建一台相机的页面：页面骨架为 <see cref="VpCameraPage"/>（属性条 / 标定卡区 / VPP 子页 / 右侧显示区），
		/// 此处按配置填入显示控件、标定卡和各 VPP 页。
		/// </summary>
		private TabPage BuildCameraPage(VpCameraConfig cam)
		{
			TabPage page = new TabPage();
			page.Name = DynCameraPrefix + cam.Name;
			page.Text = cam.Name;
			page.Padding = new Padding(4);
			page.UseVisualStyleBackColor = true;

			VpCameraPage ctl = new VpCameraPage();
			ctl.Owner = this;
			ctl.Cam = cam;
			ctl.Dock = DockStyle.Fill;
			ctl.RefreshTexts();          // 属性条文案按当前语言显示
			page.Controls.Add(ctl);

			// 整台相机共用的显示控件：标定和本相机下每个 VPP 的结果都显示在这里。
			// CogRecordDisplay 是 ActiveX 控件，代码创建时必须先设置 OcxState 再设置其它属性，否则实例不会创建。
			Cognex.VisionPro.CogRecordDisplay sharedDisp = new Cognex.VisionPro.CogRecordDisplay();
			sharedDisp.Name = "vpCamDisp_" + cam.Name;
			try
			{
				object ocx = new System.ComponentModel.ComponentResourceManager(typeof(VPForm))
					.GetObject("vpDynDisp.OcxState");
				if (ocx is AxHost.State) sharedDisp.OcxState = (AxHost.State)ocx;
			}
			catch (Exception) { }

			// 在 OcxState 之后设置，避免被状态中的值覆盖
			sharedDisp.Dock = DockStyle.Fill;
			sharedDisp.ColorMapPredefined = Cognex.VisionPro.Display.CogDisplayColorMapPredefinedConstants.None;
			sharedDisp.ColorMapLowerClipColor = Color.Black;
			sharedDisp.ColorMapLowerRoiLimit = 0D;
			sharedDisp.ColorMapUpperClipColor = Color.Black;
			sharedDisp.ColorMapUpperRoiLimit = 1D;
			ctl.DispHost.Controls.Add(sharedDisp);

			// 先登记 entry：下面构建 VPP 页时要用到 entry.Display
			CameraEntry entry = new CameraEntry();
			entry.Key = cam.Name;
			entry.Page = page;
			entry.Config = cam;
			entry.Display = sharedDisp;
			_entries.Add(entry);

			// 属性条按钮。「实时显示」为相机级：取流使用本相机 CameraIndex + 当前选中 VPP 的曝光；
			// 文字随语言和实时状态变化，不进语言表（名字带 _dyn）。
			ctl.LiveButton.Click += delegate
			{
				LiveCamera(entry);
				RefreshLiveButton(ctl.LiveButton, entry);
			};
			entry.LiveButton = ctl.LiveButton;
			RefreshLiveButton(ctl.LiveButton, entry);
			ctl.DeleteCameraButton.Click += delegate { DeleteCamera(cam); };

			// 参数编辑：数字修改即生效，名称在失焦或回车时生效（会更换 vpp 目录）
			AttachCameraEditors(cam, page, ctl.NameBox, ctl.IndexBox, ctl.ExposureBox);

			// 该相机的 VPP 容器
			TabControl vppTabs = ctl.VppTabs;
			vppTabs.Tag = cam;
			for (int i = 0; i < cam.Vpps.Count; i++)
				vppTabs.TabPages.Add(BuildVppPage(cam, cam.Vpps[i], vppTabs));

			for (int i = 0; i < cam.Vpps.Count; i++)
				if (cam.Vpps[i].Station != null) entry.Targets.Add(cam.Vpps[i].Station);

			// 标定卡片（相机级：一台相机一份，应用到本相机下所有 VPP）
			entry.CalibHost = ctl.CalibHostPanel;
			entry.HostFill = true;      // 专用容器，标定卡铺满
			RefreshCalibrationCard(entry);

			// 把整页（含标定卡、各 VPP 页）的静态文案按控件名登记进语言表
			MiddleLayer.RegisterLanguage(page, LangForm);
			return page;
		}

		private static void RegisterDisplay(VpStation st, Cognex.VisionPro.CogRecordDisplay disp)
		{
			AddDisplay(st, disp);
			// 主界面"视觉"页上同一台相机的显示也收这张图，两边看到的是同一张
			AddDisplay(st, MiddleLayer.MainF.GetCameraDisplay(st != null ? st.CameraName : null));
		}

		private static void AddDisplay(VpStation st, Cognex.VisionPro.CogRecordDisplay disp)
		{
			if (st == null || disp == null) return;
			for (int i = 0; i < st.RecordDisplayList.Count; i++)
				if (ReferenceEquals(st.RecordDisplayList[i], disp)) return;
			st.RecordDisplayList.Add(disp);
		}

		// ==================== 参数编辑 ====================
		// 数字（CameraIndex / 默认曝光 / VPP 曝光 / 标定曝光）在 TextChanged 时生效；
		// 名称（相机名 / VPP 名）在失焦或回车时生效，因为改名会更换 vpp 目录和标定键。

		private void AttachCameraEditors(VpCameraConfig cam, TabPage page, TextBox tbName, TextBox tbIndex, TextBox tbExposure)
		{
			if (cam == null) return;

			tbIndex.TextChanged += delegate { ApplyCameraNumbers(cam, tbIndex, tbExposure); };
			tbExposure.TextChanged += delegate { ApplyCameraNumbers(cam, tbIndex, tbExposure); };
			// 失焦时把无法解析的输入还原为当前值
			tbIndex.Leave += delegate { NormalizeCameraNumbers(cam, tbIndex, tbExposure); };
			tbExposure.Leave += delegate { NormalizeCameraNumbers(cam, tbIndex, tbExposure); };

			tbName.Leave += delegate { ApplyCameraRename(cam, tbName, page); };
			tbName.KeyDown += delegate (object s, KeyEventArgs e)
			{
				if (e.KeyCode != Keys.Enter) return;
				e.SuppressKeyPress = true;
				ApplyCameraRename(cam, tbName, page);
			};
		}

		/// <summary>应用 CameraIndex 和默认曝光的修改。</summary>
		private void ApplyCameraNumbers(VpCameraConfig cam, TextBox tbIndex, TextBox tbExposure)
		{
			if (cam == null) return;
			bool changed = false;

			int idx;
			if (int.TryParse((tbIndex.Text ?? "").Trim(), out idx) && idx >= 0 && idx != cam.CameraIndex)
			{
				cam.CameraIndex = idx;

				// 通道号同步给本相机下所有工位；曝光不同步（各 VPP 和标定各自设置）
				for (int i = 0; i < cam.Vpps.Count; i++)
					if (cam.Vpps[i].Station != null) cam.Vpps[i].Station.RunLiveCCDIndex = idx;
				VpCalibration cal = _config.GetCalibration(cam.Name);
				if (cal != null && cal.Station != null) cal.Station.RunLiveCCDIndex = idx;

				// 刷新各 VPP 页顶部的"相机：X（Index N）"
				RefreshVppInfoTexts(cam);
				changed = true;
			}

			double exp;
			if (double.TryParse((tbExposure.Text ?? "").Trim(), out exp) && exp > 0 && exp != cam.Exposure)
			{
				cam.Exposure = exp;          // 默认曝光，新建 VPP / 添加标定时继承
				changed = true;
			}

			if (changed)
			{
				OperationLog.Write("VPForm", "修改相机参数：" + cam.Name
					+ "（Index " + cam.CameraIndex + "，默认曝光 " + cam.Exposure.ToString("F1") + "）");
				SaveVpConfig();
			}
		}

		/// <summary>失焦时把无法解析的数字还原为当前值。</summary>
		private void NormalizeCameraNumbers(VpCameraConfig cam, TextBox tbIndex, TextBox tbExposure)
		{
			if (cam == null) return;

			int idx;
			if (!int.TryParse((tbIndex.Text ?? "").Trim(), out idx) || idx < 0)
				tbIndex.Text = cam.CameraIndex.ToString();

			double exp;
			if (!double.TryParse((tbExposure.Text ?? "").Trim(), out exp) || exp <= 0)
				tbExposure.Text = cam.Exposure.ToString("F1");
		}

		/// <summary>相机改名（失焦或回车时执行，会更换 vpp 目录名）；名称不合法时还原输入框。</summary>
		private void ApplyCameraRename(VpCameraConfig cam, TextBox tbName, TabPage page)
		{
			if (cam == null || tbName == null) return;

			string newName = (tbName.Text ?? "").Trim();
			if (newName == cam.Name) return;

			if (newName == "")
			{
				MessageBox.Show(T("vpMsg_CameraNameEmpty", "相机名称不能为空！"),
					T("vpMsg_Title", "提示"));
				tbName.Text = cam.Name;
				return;
			}

			VpCameraConfig other = FindCamera(newName);
			if (other != null && other != cam)
			{
				MessageBox.Show(T("vpMsg_CameraNameExistsA", "相机名「") + newName
					+ T("vpMsg_CameraNameExistsB", "」已存在！"),
					T("vpMsg_Title", "提示"));
				tbName.Text = cam.Name;
				return;
			}

			CameraEntry entry = FindEntry(cam.Name);
			string oldName = cam.Name;

			// 本相机下所有 vpp 的目录名随之更改（旧目录中的 vpp 文件不会自动移动）
			for (int i = 0; i < cam.Vpps.Count; i++)
				if (cam.Vpps[i].Station != null) cam.Vpps[i].Station.SetOwner(newName, cam.Vpps[i].Name);

			cam.Name = newName;
			page.Name = DynCameraPrefix + newName;
			page.Text = newName;

			// 标定以相机名为键（VpConfigFile.Calibrations），改名时同步更新键和 entry.Key
			VpCalibration calOld = _config.GetCalibration(oldName);
			if (calOld != null)
			{
				_config.Calibrations.Remove(oldName);
				_config.Calibrations[newName] = calOld;
				if (calOld.Station != null) calOld.Station.SetOwner(newName, calOld.FolderName);
			}
			if (entry != null) entry.Key = newName;

			RefreshVppInfoTexts(cam);
			// 标定卡片标题含相机名，重建卡片
			if (entry != null) RefreshCalibrationCard(entry);

			// 主界面"视觉"页的格子标签也跟着改名
			SyncMainDisplays();

			OperationLog.Write("VPForm", "相机改名：" + oldName + " → " + newName);
			SaveVpConfig();
		}

		/// <summary>
		/// 刷新本相机下各 VPP 页顶部的归属行与路径行（相机名 / CameraIndex / VPP 名 / vpp 路径）。
		/// </summary>
		private void RefreshVppInfoTexts(VpCameraConfig cam)
		{
			TabControl tabs = FindVppTabControl(cam);
			if (tabs == null) return;

			for (int i = 0; i < tabs.TabPages.Count; i++)
			{
				TabPage p = tabs.TabPages[i];

				// 归属行与路径行由 VpVppPage 生成（动态文案 _dyn）
				for (int k = 0; k < p.Controls.Count; k++)
				{
					VpVppPage ctl = p.Controls[k] as VpVppPage;
					if (ctl == null) continue;
					ctl.RefreshTexts();
					break;
				}
			}
		}

		/// <summary>
		/// 配方切换后由 <see cref="MiddleLayer.OpenVision"/> 调用：刷新界面上的 vpp 路径文字
		/// （vpp 本身由 OpenVision 重新载入）。
		/// </summary>
		public void OnRecipeChanged()
		{
			if (IsDisposed) return;
			try
			{
				for (int i = 0; i < _entries.Count; i++)
				{
					CameraEntry e = _entries[i];
					if (e == null) continue;

					if (e.Config != null) RefreshVppInfoTexts(e.Config);
					RefreshCalibrationTexts(e);
				}
			}
			catch (Exception) { }
		}

		/// <summary>标定卡上的 vpp 路径文字（按当前配方生成）。</summary>
		private static string BuildCalibTipText(VpCalibration calib)
		{
			if (calib == null) return "";
			if (calib.Station == null) return T("vpPath_NotLoaded", "vpp：(未加载)");
			return T("vpPath_Prefix", "vpp：") + calib.Station.GetVppPath(SysPara.RecipeName);
		}

		/// <summary>标定卡标题（随语言和已录点数变化）。</summary>
		private static string BuildCalibTitleText(CameraEntry entry, VpCalibration calib)
		{
			string head = T("vpCalib_Title", "标定");
			string tail = (calib == null)
				? T("vpCalib_NotAdded", "　　尚未添加标定")
				: T("vpCalib_PointsA", "　　已录 ") + calib.Points.Count
				  + T("vpCalib_PointsB", " 点（至少 9 点）");
			return head + "　" + entry.Key + tail;
		}

		/// <summary>刷新标定卡上的标题（相机名 + 点数）与 vpp 路径，不重建整张卡。</summary>
		private void RefreshCalibrationTexts(CameraEntry entry)
		{
			if (entry == null || entry.CalibHost == null) return;

			// 标题与提示由 VpCalibCard 生成（动态文案 _dyn）
			for (int i = 0; i < entry.CalibHost.Controls.Count; i++)
			{
				VpCalibCard card = entry.CalibHost.Controls[i] as VpCalibCard;
				if (card == null) continue;
				card.Calib = _config.GetCalibration(entry.Key);
				card.RefreshTexts();
				return;
			}
		}

		private void DeleteCamera(VpCameraConfig cam)
		{
			if (cam == null) return;
			if (!EnsureIdle(T("vpAct_DeleteCamera", "删除相机"))) return;

			if (MessageBox.Show(T("vpMsg_DeleteCameraA", "确认删除相机「") + cam.Name
					+ T("vpMsg_DeleteCameraB", "」及其下 ") + cam.Vpps.Count + T("vpMsg_DeleteCameraC", " 个 VPP 页？\r\n")
					+ T("vpMsg_KeepFiles", "（只删界面与配置，磁盘上的 vpp 文件保留）"),
				T("vpMsg_DeleteCameraTitle", "删除相机"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK) return;

			// 移除每个 VPP 的视觉站与显示控件
			for (int i = cam.Vpps.Count - 1; i >= 0; i--)
				RemoveVpp(cam, cam.Vpps[i], null);

			// 同时移除相机级标定及其视觉站
			VpCalibration calib = _config.GetCalibration(cam.Name);
			if (calib != null)
			{
				if (calib.Station != null)
				{
					try { calib.Station.SaveTB(); } catch (Exception) { }
					VisionproInterface.VList.Remove(calib.Station);
				}
				_config.Calibrations.Remove(cam.Name);
			}

			_entries.RemoveAll(delegate (CameraEntry e) { return e.Key == cam.Name; });

			_cameras.Remove(cam);

			for (int i = tabControl1.TabPages.Count - 1; i >= 0; i--)
			{
				TabPage p = tabControl1.TabPages[i];
				if (p.Name != DynCameraPrefix + cam.Name) continue;
				tabControl1.TabPages.RemoveAt(i);
				p.Dispose();
			}

			// 主界面"视觉"页去掉这台相机的显示格子
			SyncMainDisplays();

			OperationLog.Write("VPForm", "删除相机：" + cam.Name);
			SaveVpConfig();
		}

		private VpCameraConfig CurrentCameraConfig()
		{
			TabPage p = tabControl1.SelectedTab;
			if (p == null) return null;
			return p.Name.StartsWith(DynCameraPrefix) ? FindCamera(p.Name.Substring(DynCameraPrefix.Length)) : null;
		}

		/// <summary>工具条上的"删除当前相机"。</summary>
		private void DeleteCurrentCameraPage()
		{
			TabPage p = tabControl1.SelectedTab;
			if (p == null) return;
			if (!p.Name.StartsWith(DynCameraPrefix)) return;

			VpCameraConfig cam = FindCamera(p.Name.Substring(DynCameraPrefix.Length));
			if (cam != null) DeleteCamera(cam);
		}

		// ---------------- VPP ----------------

		private void AddVppToCurrentCamera()
		{
			VpCameraConfig cam = CurrentCameraConfig();
			if (cam == null)
			{
				MessageBox.Show(T("vpMsg_NeedCameraPage", "请先切到一个相机页（或点工具条上的「添加相机」新建一台）。"),
					T("vpMsg_Title", "提示"));
				return;
			}
			AddVpp(cam, FindVppTabControl(cam));
		}

		private void AddVpp(VpCameraConfig cam, TabControl vppTabs)
		{
			if (cam == null || vppTabs == null) return;
			if (!EnsureIdle(T("vpAct_AddVpp", "添加 VPP"))) return;

			int n = cam.Vpps.Count + 1;
			string name = "VPP" + n;
			while (FindVpp(cam, name) != null) { n++; name = "VPP" + n; }

			VpVppConfig vpp = new VpVppConfig();
			vpp.Name = name;
			vpp.Exposure = cam.Exposure;      // 新 VPP 继承相机的默认曝光
			cam.Vpps.Add(vpp);

			TabPage page = BuildVppPage(cam, vpp, vppTabs);
			vppTabs.TabPages.Add(page);
			vppTabs.SelectedTab = page;

			// 新 VPP 加入本相机标定的应用对象
			CameraEntry entry = FindEntry(cam.Name);
			if (entry != null && vpp.Station != null) entry.Targets.Add(vpp.Station);

			MiddleLayer.RegisterLanguage(page, LangForm);   // 静态文案按控件名登记进语言表
			OperationLog.Write("VPForm", "添加 VPP：" + cam.Name + " / " + vpp.Name);
			SaveVpConfig();
		}

		/// <summary>构建一个 VPP 页（布局见 <see cref="VpVppPage"/>），并挂接数据与编辑逻辑。</summary>
		private TabPage BuildVppPage(VpCameraConfig cam, VpVppConfig vpp, TabControl vppTabs)
		{
			if (vpp.Station == null) CreateStation(cam, vpp);

			TabPage page = new TabPage();
			page.Name = DynVppPrefix + cam.Name + "_" + vpp.Name;
			page.Text = vpp.Name;
			page.Padding = new Padding(3);
			page.UseVisualStyleBackColor = true;
			// 相机级「实时显示」通过它取当前 VPP 的曝光（见 LiveCamera / CurrentVpp）
			page.Tag = vpp;

			// 结果显示在整台相机共用的显示控件上
			CameraEntry entry = FindEntry(cam.Name);
			Cognex.VisionPro.CogRecordDisplay display = (entry != null) ? entry.Display : null;
			if (vpp.Station != null) RegisterDisplay(vpp.Station, display);

			VpVppPage ctl = new VpVppPage();
			ctl.Owner = this;
			ctl.Cam = cam;
			ctl.Vpp = vpp;
			ctl.Dock = DockStyle.Fill;
			ctl.Bind();
			page.Controls.Add(ctl);

			// 曝光 / VPP 名的编辑逻辑
			AttachVppEditors(cam, vpp, page, ctl.InfoLabel, ctl.ExposureBox, ctl.RenameBox);

			// 切换语言时重填补偿限制表的表头（DataGridView 不进语言表）
			_compGrids.Add(ctl.CompGrid);

			return page;
		}

		internal string BuildVppInfoText(VpCameraConfig cam, VpVppConfig vpp)
		{
			return T("vpInfo_Camera", "相机：") + cam.Name
				 + " (Index " + cam.CameraIndex + ")"
				 + T("vpInfo_Vpp", "　　VPP：") + (vpp != null ? vpp.Name : "");
		}

		/// <summary>VPP 页与标定卡共用的 vpp 路径文字（按当前配方生成）。</summary>
		internal static string BuildVppPathText(VpVppConfig vpp)
		{
			if (vpp == null || vpp.Station == null)
				return T("vpPath_NotLoaded", "vpp：(未加载)");
			return T("vpPath_Prefix", "vpp：") + vpp.Station.GetVppPath(SysPara.RecipeName);
		}

		/// <summary>
		/// 填充补偿限制表：每行 = 项目名 + 下限 + 上限，行对象存于 <see cref="DataGridViewRow.Tag"/>。
		/// 表头取自语言键 vpCompCol_*，切换语言时由 <see cref="RefreshDynamicTexts"/> 重填。
		/// </summary>
		internal void FillCompLimitGrid(DataGridView grid, VpVppConfig vpp)
		{
			if (grid == null || vpp == null) return;

			grid.Columns[0].HeaderText = T("vpCompCol_Item", "项目");
			grid.Columns[1].HeaderText = T("vpCompCol_Min", "补偿下限");
			grid.Columns[2].HeaderText = T("vpCompCol_Max", "补偿上限");

			grid.Rows.Clear();
			for (int i = 0; i < vpp.CompLimits.Count; i++)
			{
				VpCompLimitItem it = vpp.CompLimits[i];
				if (it == null) continue;
				int idx = grid.Rows.Add(it.Item, it.Min.ToString("F3"), it.Max.ToString("F3"));
				grid.Rows[idx].Tag = it;    // 行与配置对象一一对应
			}
		}

		/// <summary>
		/// 【添加行】：在表尾追加一行（默认名"补偿N"，跳过重名）并立即保存，然后进入名称单元格编辑。
		/// 名称为空的行不会保存（<see cref="VpConfigStore.Save"/> 会跳过）。
		/// </summary>
		internal void AddCompLimitRow(DataGridView grid, VpVppConfig vpp)
		{
			if (grid == null || vpp == null) return;

			string name;
			int n = vpp.CompLimits.Count + 1;
			while (vpp.GetCompLimit(name = "补偿" + n) != null) n++;

			VpCompLimitItem it = new VpCompLimitItem(name, 0, 0);
			vpp.CompLimits.Add(it);
			OperationLog.Write("VPForm", "补偿限制：添加行「" + name + "」（" + LogTag(vpp) + "）");
			FillCompLimitGrid(grid, vpp);
			SaveVpConfig();

			int idx = grid.Rows.Count - 1;
			if (idx >= 0)
			{
				grid.CurrentCell = grid.Rows[idx].Cells[0];
				grid.BeginEdit(true);
			}
		}

		/// <summary>【删除行】：删除当前选中行（按 <see cref="DataGridViewRow.Tag"/> 中的对象删除）并立即保存。</summary>
		internal void RemoveCompLimitRow(DataGridView grid, VpVppConfig vpp)
		{
			if (grid == null || vpp == null) return;

			DataGridViewRow row = grid.CurrentRow;
			if (row == null || row.Index < 0) return;

			VpCompLimitItem it = row.Tag as VpCompLimitItem;
			if (it == null) return;

			vpp.CompLimits.Remove(it);
			OperationLog.Write("VPForm", "补偿限制：删除行「" + it.Item + "」（" + LogTag(vpp) + "）");
			FillCompLimitGrid(grid, vpp);
			SaveVpConfig();
		}

		/// <summary>
		/// 编辑结束时把单元格写回 <see cref="VpCompLimitItem"/> 并立即保存：名称为空或数值无法解析时还原，下限不得高于上限。
		/// </summary>
		internal void SyncCompLimitFromGrid(DataGridView grid, VpVppConfig vpp, int rowIndex)
		{
			if (grid == null || vpp == null || rowIndex < 0 || rowIndex >= grid.Rows.Count) return;

			DataGridViewRow row = grid.Rows[rowIndex];
			VpCompLimitItem it = row.Tag as VpCompLimitItem;
			if (it == null) return;

			bool changed = false;

			// 项目名：为空时还原
			string nameText = (row.Cells[0].Value ?? "").ToString().Trim();
			if (nameText.Length == 0) row.Cells[0].Value = it.Item;
			else if (nameText != it.Item) { it.Item = nameText; changed = true; }

			// 下限 / 上限
			double v;
			string minText = (row.Cells[1].Value ?? "").ToString().Trim();
			string maxText = (row.Cells[2].Value ?? "").ToString().Trim();

			if (!double.TryParse(minText, out v)) row.Cells[1].Value = it.Min.ToString("F3");
			else if (v != it.Min) { it.Min = v; changed = true; }

			if (!double.TryParse(maxText, out v)) row.Cells[2].Value = it.Max.ToString("F3");
			else if (v != it.Max) { it.Max = v; changed = true; }

			// 下限高于上限时把下限设为上限
			if (it.Min > it.Max)
			{
				it.Min = it.Max;
				row.Cells[1].Value = it.Min.ToString("F3");
				changed = true;
			}

			if (changed)
			{
				OperationLog.Write("VPForm", "补偿限制：修改「" + it.Item + "」为 ["
					+ it.Min.ToString("F3") + ", " + it.Max.ToString("F3") + "]（" + LogTag(vpp) + "）");
				SaveVpConfig();
			}
		}

		/// <summary>创建视觉站（构造时自动登记到 VisionproInterface.VList，OpenVision 时按配方加载 vpp）。</summary>
		private void CreateStation(VpCameraConfig cam, VpVppConfig vpp)
		{
			VpStation st = new VpStation(cam.Name, vpp.Name);
			st.RunLiveCCDIndex = cam.CameraIndex;      // 相机通道号
			st.RunExposure = vpp.Exposure;             // 本 VPP 的曝光
			st.Config = vpp;                           // 配置引用（补偿限制从这里读取）
			// 按当前配方加载；文件不存在时 LoadTB 新建空 ToolBlock 并保存
			st.LoadTB(st.GetVppPath(SysPara.RecipeName));
			vpp.Station = st;
		}

		private void CreateCameraStations(VpCameraConfig cam)
		{
			for (int i = 0; i < cam.Vpps.Count; i++)
				if (cam.Vpps[i].Station == null) CreateStation(cam, cam.Vpps[i]);
		}

		internal void EditStation(VpVppConfig vpp)
		{
			if (vpp == null || vpp.Station == null) return;
			if (!vpp.Station.IsLoadTBOk) vpp.Station.LoadTB(vpp.Station.GetVppPath(SysPara.RecipeName));
			vpp.Station.EditTB();
		}

		/// <summary>拍照并运行 ToolBlock，结果自动显示到相机的显示控件上。</summary>
		internal void RunStation(VpVppConfig vpp)
		{
			if (vpp == null || vpp.Station == null) return;
			vpp.Station.RunTB();
		}

		/// <summary>
		/// 相机级实时显示的开关：通道号用相机的 CameraIndex，曝光用当前选中 VPP 的 Exposure。
		/// </summary>
		private void LiveCamera(CameraEntry entry)
		{
			if (entry == null || entry.Display == null) return;

			VpVppConfig vpp = CurrentVpp(entry);
			VpStation driver = (vpp != null) ? vpp.Station : null;
			if (driver == null)
			{
				MessageBox.Show(T("vpMsg_NoVppForLive", "这台相机下还没有可用的 VPP。\r\n实时显示用的是「当前 VPP 的曝光」，请先添加一个 VPP。"),
					T("vpMsg_Title", "提示"));
				return;
			}

			driver.RunLiveCCDIndex = (entry.Config != null) ? entry.Config.CameraIndex : driver.RunLiveCCDIndex;
			driver.RunExposure = vpp.Exposure;
			driver.RunLive(entry.Display);
		}

		/// <summary>按实时状态（<see cref="IsLiveRunning"/>）刷新实时按钮的文字和底色。</summary>
		private static void RefreshLiveButton(Button btn, CameraEntry entry)
		{
			if (btn == null) return;
			bool live = IsLiveRunning(entry);
			btn.Text = LiveButtonText(entry);
			btn.BackColor = live ? Color.FromArgb(255, 232, 204) : Color.FromArgb(232, 244, 255);
		}

		/// <summary>实时按钮的文字（随语言和实时状态变化）。</summary>
		private static string LiveButtonText(CameraEntry entry)
		{
			return IsLiveRunning(entry)
				? T("vpBtn_StopLive", "停止实时")
				: T("vpBtn_Live", "实时显示");
		}

		/// <summary>
		/// 相机显示控件是否处于实时显示。CogRecordDisplay 是 ActiveX 控件，句柄或 COM 对象未就绪时读取
		/// LiveDisplayRunning 会抛异常，此时按"未实时"处理。
		/// </summary>
		private static bool IsLiveRunning(CameraEntry entry)
		{
			try
			{
				Cognex.VisionPro.CogRecordDisplay disp = (entry != null) ? entry.Display : null;
				if (disp == null || disp.IsDisposed || !disp.IsHandleCreated) return false;
				return disp.LiveDisplayRunning;
			}
			catch (Exception) { return false; }
		}

		/// <summary>当前相机页里 VPP 子 TabControl 选中的那一个 VPP。</summary>
		private VpVppConfig CurrentVpp(CameraEntry entry)
		{
			if (entry == null || entry.Config == null) return null;
			TabControl tabs = FindVppTabControl(entry.Config);
			if (tabs == null || tabs.SelectedTab == null) return null;
			return tabs.SelectedTab.Tag as VpVppConfig;
		}

		/// <summary>VPP 页上的两个编辑框：曝光修改即生效，名称在失焦或回车时生效（会更换 vpp 目录）。</summary>
		private void AttachVppEditors(VpCameraConfig cam, VpVppConfig vpp, TabPage page, UiLabel info, TextBox tbExp, TextBox tbRename)
		{
			if (cam == null || vpp == null) return;

			tbExp.TextChanged += delegate
			{
				double exp;
				if (!double.TryParse((tbExp.Text ?? "").Trim(), out exp) || exp <= 0) return;
				if (exp == vpp.Exposure) return;
				vpp.Exposure = exp;
				if (vpp.Station != null) vpp.Station.RunExposure = exp;
				SaveVpConfig();
			};
			tbExp.Leave += delegate
			{
				double exp;
				if (!double.TryParse((tbExp.Text ?? "").Trim(), out exp) || exp <= 0)
					tbExp.Text = vpp.Exposure.ToString("F1");
			};

			tbRename.Leave += delegate { ApplyVppRename(cam, vpp, tbRename, page, info); };
			tbRename.KeyDown += delegate (object s, KeyEventArgs e)
			{
				if (e.KeyCode != Keys.Enter) return;
				e.SuppressKeyPress = true;
				ApplyVppRename(cam, vpp, tbRename, page, info);
			};
		}

		internal void SaveStation(VpVppConfig vpp)
		{
			if (vpp == null || vpp.Station == null) return;
			vpp.Station.SaveTB();
		}

		/// <summary>VPP 改名（失焦或回车时执行，会更换 vpp 目录）；名称不合法时还原输入框。</summary>
		private void ApplyVppRename(VpCameraConfig cam, VpVppConfig vpp, TextBox box, TabPage page, Label info)
		{
			if (cam == null || vpp == null || box == null) return;

			try
			{
				string newName = (box.Text ?? "").Trim();
				if (newName == vpp.Name) return;

				if (newName == "")
				{
					MessageBox.Show(T("vpMsg_VppNameEmpty", "VPP 名称不能为空！"),
						T("vpMsg_Title", "提示"));
					box.Text = vpp.Name;
					return;
				}
				if (FindVpp(cam, newName) != null)
				{
					MessageBox.Show(T("vpMsg_VppExistsA", "本相机下已有同名 VPP：「")
						+ newName + T("vpMsg_VppExistsB", "」"), T("vpMsg_Title", "提示"));
					box.Text = vpp.Name;
					return;
				}
				if (!EnsureIdle(T("vpAct_RenameVpp", "改名 VPP"))) { box.Text = vpp.Name; return; }

				OperationLog.Write("VPForm", "VPP 改名：" + cam.Name + " / " + vpp.Name + " → " + newName);
				vpp.Name = newName;
				if (vpp.Station != null)
				{
					vpp.Station.SetOwner(cam.Name, newName);
					// 按当前配方从新目录重新加载（旧目录中的 vpp 文件需要手动移动）
					vpp.Station.LoadTB(vpp.Station.GetVppPath(SysPara.RecipeName));
				}

				page.Text = newName;
				page.Name = DynVppPrefix + cam.Name + "_" + newName;
				info.Text = BuildVppInfoText(cam, vpp);
				SaveVpConfig();
			}
			catch (Exception) { }
		}

		/// <summary>删除 VPP 前的确认框。</summary>
		private static bool ConfirmDeleteVpp(VpVppConfig vpp)
		{
			return MessageBox.Show(
				T("vpMsg_DeleteVppA", "确认删除 VPP「") + vpp.Name
				+ T("vpMsg_DeleteVppB", "」？\r\n（只删界面与配置，磁盘上的 vpp 文件保留）"),
				T("vpMsg_DeleteVppTitle", "删除 VPP"),
				MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK;
		}

		private void DeleteCurrentVpp()
		{
			VpCameraConfig cam = CurrentCameraConfig();
			if (cam == null)
			{
				MessageBox.Show(T("vpMsg_NeedDynamicCameraPage", "请先切到一台动态相机页。"),
					T("vpMsg_Title", "提示"));
				return;
			}

			TabControl vppTabs = FindVppTabControl(cam);
			if (vppTabs == null || vppTabs.SelectedTab == null) return;

			TabPage vp = vppTabs.SelectedTab;
			VpVppConfig vpp = FindVpp(cam, vp.Text);
			if (vpp == null) return;

			if (!EnsureIdle(T("vpAct_DeleteVpp", "删除 VPP"))) return;
			if (!ConfirmDeleteVpp(vpp)) return;

			OperationLog.Write("VPForm", "删除 VPP：" + LogTag(vpp));
			RemoveVpp(cam, vpp, vppTabs);
			SaveVpConfig();
		}

		/// <summary>删除一个 VPP：停止实时显示、注销视觉站、移除界面页与配置。</summary>
		private void RemoveVpp(VpCameraConfig cam, VpVppConfig vpp, TabControl vppTabs)
		{
			if (cam == null || vpp == null) return;

			string vppName = vpp.Name;

			// 从标定的应用对象中移除
			CameraEntry entry = FindEntry(cam.Name);
			if (entry != null && vpp.Station != null) entry.Targets.Remove(vpp.Station);

			// 视觉站
			if (vpp.Station != null)
			{
				try
				{
					for (int i = 0; i < vpp.Station.RecordDisplayList.Count; i++)
					{
						Cognex.VisionPro.CogRecordDisplay d = vpp.Station.RecordDisplayList[i];
						if (d == null) continue;
						// 相机共用的显示控件可能正被同相机的其它 VPP 或标定使用，不停止
						if (entry != null && ReferenceEquals(d, entry.Display)) continue;
						try { d.StopLiveDisplay(); } catch (Exception) { }
					}
				}
				catch (Exception) { }

				VisionproInterface.VList.Remove(vpp.Station);
				vpp.Station = null;
			}

			// 界面
			if (vppTabs != null)
				for (int i = vppTabs.TabPages.Count - 1; i >= 0; i--)
				{
					if (vppTabs.TabPages[i].Name != DynVppPrefix + cam.Name + "_" + vppName) continue;
					TabPage p = vppTabs.TabPages[i];
					vppTabs.TabPages.RemoveAt(i);
					p.Dispose();
					break;
				}

			cam.Vpps.Remove(vpp);
		}

		// ---------------- 小工具 ----------------

		/// <summary>把视觉配置立即写入 VPForm.Cameras.xml。</summary>
		internal void SaveVpConfig()
		{
			try
			{
				VpConfigStore.Save(_config);
			}
			catch (Exception) { }
		}

		/// <summary>主界面【保存】选"是"时由 MainForm.SaveData() 调用：把相机 / VPP / 标定参数写入 VPForm.Cameras.xml。</summary>
		public void CommitVpConfig()
		{
			SaveVpConfig();
		}

		/// <summary>
		/// 主界面【保存】选"否"时调用：拆除界面、重新读取配置文件并重建，使界面与视觉工位状态和磁盘一致。
		/// </summary>
		public void RevertVpConfig()
		{
			try
			{
				TeardownVpUi();
				LoadConfigFromDisk();
				BuildVpPages();
				UpdateVpHint();
			}
			catch (Exception) { }
		}

		/// <summary>进入页面时按当前配方刷新界面上的 vpp 路径。</summary>
		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			if (!Visible) return;

			OnRecipeChanged();
		}

		/// <summary>拆除所有相机页及其工位（不修改磁盘和 <see cref="_config"/>），供 <see cref="RevertVpConfig"/> 重建使用。</summary>
		private void TeardownVpUi()
		{
			for (int i = _entries.Count - 1; i >= 0; i--)
			{
				CameraEntry e = _entries[i];

				try { if (IsLiveRunning(e)) e.Display.StopLiveDisplay(); }
				catch (Exception) { }

				if (e.Config != null)
					for (int j = e.Config.Vpps.Count - 1; j >= 0; j--)
						RemoveVpp(e.Config, e.Config.Vpps[j], null);

				VpCalibration calib = _config.GetCalibration(e.Key);
				if (calib != null && calib.Station != null)
				{
					// 不回写 ToolBlock（取消时其路径可能正要被丢弃）
					VisionproInterface.VList.Remove(calib.Station);
					calib.Station = null;
				}

				if (e.Page != null)
				{
					tabControl1.TabPages.Remove(e.Page);
					e.Page.Dispose();
				}
			}
			_entries.Clear();
			_compGrids.Clear();
		}

		private bool EnsureIdle(string action)
		{
			if (SysPara.SystemMode == RunMode.IDLE) return true;
			MessageBox.Show(T("vpMsg_RunningCant", "设备运行中，不能")
							+ action + "！",
				T("vpMsg_WarningTitle", "警告"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
			return false;
		}

		private VpCameraConfig FindCamera(string name)
		{
			if (string.IsNullOrEmpty(name)) return null;
			for (int i = 0; i < _cameras.Count; i++)
				if (_cameras[i].Name == name) return _cameras[i];
			return null;
		}

		/// <summary>按相机名取相机配置（名称 / CameraIndex / Vpps / 补偿限制）；不存在返回 null。</summary>
		public VpCameraConfig GetCameraConfig(string cameraName)
		{
			return FindCamera(cameraName);
		}

		/// <summary>取某个 VPP 的视觉站（用于拍照、读结果和 ToolBlock 输出）；不存在返回 null。</summary>
		public VpStation GetVppStation(string cameraName, string vppName)
		{
			VpCameraConfig cam = FindCamera(cameraName);
			if (cam == null) return null;
			for (int i = 0; i < cam.Vpps.Count; i++)
				if (cam.Vpps[i] != null && cam.Vpps[i].Name == vppName) return cam.Vpps[i].Station;
			return null;
		}

		private VpVppConfig FindVpp(VpCameraConfig cam, string name)
		{
			if (cam == null) return null;
			for (int i = 0; i < cam.Vpps.Count; i++)
				if (cam.Vpps[i].Name == name) return cam.Vpps[i];
			return null;
		}

		/// <summary>取相机的 VPP 容器：TabPage 中的 <see cref="VpCameraPage"/>.VppTabs。</summary>
		private TabControl FindVppTabControl(VpCameraConfig cam)
		{
			if (cam == null) return null;
			for (int i = 0; i < tabControl1.TabPages.Count; i++)
			{
				TabPage p = tabControl1.TabPages[i];
				if (p.Name != DynCameraPrefix + cam.Name) continue;

				for (int j = 0; j < p.Controls.Count; j++)
				{
					VpCameraPage ctl = p.Controls[j] as VpCameraPage;
					if (ctl != null) return ctl.VppTabs;

					// TabControl 直接挂在页上的情况
					if (p.Controls[j] is TabControl) return (TabControl)p.Controls[j];
				}
			}
			return null;
		}

		// ==================== 语言 ====================
		// 控件按控件名登记进语言表（LanguageData\{语言}.xml 的 /{语言}/VPForm/{控件名}），
		// 切换语言时由 SwitchLanguage 更新文字。本页在 ModuleInitialize 之后构建，建页后需调用
		// MiddleLayer.RegisterLanguage(page, LangForm) 登记。
		// T("键", "底稿")：取当前语言的文字；XML 中没有该键时以底稿写入并使用。

		/// <summary>LanguageData 里属于本页的节点名（= 窗体名）。</summary>
		private const string LangForm = "VPForm";

		/// <summary>
		/// 把本页全部控件按控件名登记进语言表（由 <see cref="MiddleLayer.InitialLanguageData"/> 在语言表建好后调用）。
		/// </summary>
		public void RegisterVpLanguage()
		{
			MiddleLayer.RegisterLanguage(this, LangForm);
		}

		/// <summary>按键取当前语言的文案（LanguageData\{当前语言}.xml）；XML 中没有该键时使用底稿。</summary>
		private static string T(string key, string baseText)
		{
			return MiddleLayer.LangText(LangForm, key, baseText);
		}

		/// <summary>
		/// 含动态内容（相机名 / VPP 名 / 配方路径 / 点数 / 实时状态）的控件名：后缀 _dyn 使
		/// <see cref="MiddleLayer.RegisterLanguage"/> 跳过该控件，文字由 <see cref="RefreshDynamicTexts"/> 生成。
		/// </summary>
		private static string DynTextName(string key)
		{
			return key + "_dyn";
		}

		/// <summary>
		/// 语言变化后刷新动态文字：各 VPP 页的归属行与 vpp 路径、标定卡的标题与路径、相机实时按钮、工具条提示。
		/// </summary>
		private void RefreshDynamicTexts()
		{
			if (IsDisposed) return;
			try
			{
				for (int i = 0; i < _entries.Count; i++)
				{
					CameraEntry e = _entries[i];
					if (e == null) continue;

					if (e.Config != null) RefreshVppInfoTexts(e.Config);
					RefreshCalibrationTexts(e);
					if (e.LiveButton != null) RefreshLiveButton(e.LiveButton, e);
				}
				// 补偿限制表的表头（DataGridView 不进语言表）
				for (int i = _compGrids.Count - 1; i >= 0; i--)
				{
					DataGridView g = _compGrids[i];
					if (g == null || g.IsDisposed) { _compGrids.RemoveAt(i); continue; }
					VpVppConfig vpp = g.Tag as VpVppConfig;
					if (vpp != null) FillCompLimitGrid(g, vpp);
				}
				UpdateVpHint();
			}
			catch (Exception) { }
		}

		// 工具条按钮：控件名即语言键（如 vpBar_AddCamera），baseText 为 XML 中没有该键时的底稿

		/// <summary>创建工具条按钮（控件名 = 语言键）。</summary>
		private void AddBarButton(Control parent, string key, string baseText, EventHandler onClick)
		{
			UiButton b = new UiButton();
			b.Name = key;
			b.Text = T(key, baseText);
			b.Font = new Font("宋体", 11.25F);
			b.FlatStyle = FlatStyle.Flat;
			b.BackColor = Color.White;
			b.ForeColor = Color.FromArgb(38, 50, 64);
			b.FlatAppearance.BorderColor = Color.FromArgb(203, 216, 230);
			b.Size = new Size(126, 32);
			b.Margin = new Padding(2, 0, 6, 0);
			b.Cursor = Cursors.Hand;
			if (onClick != null) b.Click += onClick;
			parent.Controls.Add(b);
		}

		// ==================== 相机级标定 ====================
		// 每台相机一份标定：
		//   标定 ToolBlock = VisionData\{相机}\{Calibration.FolderName}\{配方}.vpp
		//   标定点         = VPForm.Cameras.xml（机器级，不随配方变）
		//   SetCalibration = 写入本相机下所有 VPP 的 ToolBlock（工具名 Calibration.ToolName）
		//   显示           = 与本相机所有 VPP 共用 entry.Display

		/// <summary>重建标定卡片：移除旧的 VpCalibCard（宿主中的其它控件保留）后重新创建。</summary>
		private void RefreshCalibrationCard(CameraEntry entry)
		{
			if (entry == null || entry.CalibHost == null) return;

			// 按类型识别旧卡片（Tag 存放的是相机入口）
			for (int i = entry.CalibHost.Controls.Count - 1; i >= 0; i--)
			{
				VpCalibCard old = entry.CalibHost.Controls[i] as VpCalibCard;
				if (old == null) continue;
				entry.CalibHost.Controls.RemoveAt(i);
				old.Dispose();
			}

			VpCalibCard card = BuildCalibrationCard(entry);
			entry.CalibHost.Controls.Add(card);
			card.BringToFront();

			MiddleLayer.RegisterLanguage(card, LangForm);   // 重建的标定卡登记进语言表
		}

		/// <summary>
		/// 确保相机的标定站已创建（配置文件只保存标定点与 vpp 目录名，标定站按需创建；已存在时不做任何事）。
		/// </summary>
		private void EnsureCalibrationStation(CameraEntry entry, VpCalibration calib)
		{
			if (entry == null || calib == null || calib.Station != null) return;

			VpStation st = new VpStation(entry.Key, calib.FolderName, VpStationKind.Calibration);
			if (entry.Config != null)
			{
				st.RunLiveCCDIndex = entry.Config.CameraIndex;
				st.RunExposure = calib.Exposure;
			}

			// 按当前配方加载；文件不存在时新建空 ToolBlock，已有标定不会被覆盖
			st.LoadTB(st.GetVppPath(SysPara.RecipeName));

			RegisterDisplay(st, entry.Display);
			calib.Station = st;
		}

		/// <summary>
		/// 创建标定卡片（布局见 <see cref="VpCalibCard"/>）：相机入口放在 Tag，标定配置给 Calib；
		/// 未建标定时只显示"添加标定"（见 <see cref="VpCalibCard.Bind"/>）。
		/// </summary>
		private VpCalibCard BuildCalibrationCard(CameraEntry entry)
		{
			VpCalibration calib = _config.GetCalibration(entry.Key);
			EnsureCalibrationStation(entry, calib);      // 确保标定站已创建

			VpCalibCard card = new VpCalibCard();
			card.Name = "vpCalibCard_" + entry.Key;
			card.Owner = this;
			card.Calib = calib;
			card.Tag = entry;
			if (entry.HostFill)
			{
				card.Dock = DockStyle.Fill;
			}
			else
			{
				card.Dock = DockStyle.Top;
				card.Height = 296;      // 宿主中还有其它控件时使用固定高度
			}
			card.Bind();

			// 标定站与本相机所有 VPP 共用 entry.Display
			if (calib != null && calib.Station != null)
				RegisterDisplay(calib.Station, entry.Display);

			return card;
		}

		// ============ VpCalibCard 与本页的接口 ============
		// 卡片负责布局并转发控件事件，标定业务在本页处理；相机入口从 card.Tag 取。

		private CameraEntry EntryOf(VpCalibCard card)
		{
			return (card == null) ? null : (card.Tag as CameraEntry);
		}

		/// <summary>标定点表格单元格编辑完成：写回标定点并立即保存（无法解析时恢复原值）。</summary>
		internal void OnCalibGridEdited(VpCalibCard card, DataGridViewCellEventArgs e)
		{
			if (card == null) return;
			CameraEntry entry = EntryOf(card);
			if (entry == null) return;

			VpCalibration c = _config.GetCalibration(entry.Key);
			if (c == null) return;

			DataGridView grid = card.GridPoints;
			DataGridViewRow row = (e.RowIndex >= 0 && e.RowIndex < grid.Rows.Count) ? grid.Rows[e.RowIndex] : null;
			if (row != null && e.RowIndex < c.Points.Count)
			{
				// 无法解析时把原值写回单元格
				VpCalibPoint p = c.Points[e.RowIndex];
				double v;
				if (e.ColumnIndex == 2 && !double.TryParse((row.Cells[2].Value ?? "").ToString().Trim(), out v))
					row.Cells[2].Value = p.MotorPosX.ToString("F3");
				if (e.ColumnIndex == 3 && !double.TryParse((row.Cells[3].Value ?? "").ToString().Trim(), out v))
					row.Cells[3].Value = p.MotorPosY.ToString("F3");
			}

			SyncPointsFromGrid(grid, c);
			SaveVpConfig();
		}

		internal void FillCalibrationGrid(VpCalibCard card)
		{
			if (card == null) return;
			FillCalibrationGrid(card.GridPoints, card.Calib);
		}

		internal string BuildCalibTitleText(VpCalibCard card)
		{
			return BuildCalibTitleText(EntryOf(card), (card == null) ? null : card.Calib);
		}

		internal string BuildCalibTipText(VpCalibCard card)
		{
			return BuildCalibTipText((card == null) ? null : card.Calib);
		}

		/// <summary>标定曝光框的默认值：相机的默认曝光（未设置时为 10）。</summary>
		internal double CameraDefaultExposure(VpCalibCard card)
		{
			CameraEntry entry = EntryOf(card);
			return (entry != null && entry.Config != null) ? entry.Config.Exposure : 10;
		}

		internal void ApplyCalibrationExposure(VpCalibCard card)
		{
			if (card == null) return;
			ApplyCalibrationExposure(EntryOf(card), card.ExposureBox);
		}

		internal void AddCalibration(VpCalibCard card)
		{
			if (card == null) return;
			AddCalibration(EntryOf(card), card.ExposureBox);
		}

		internal void EditCalibrationTB(VpCalibCard card)
		{
			if (card == null) return;
			EditCalibrationTB(EntryOf(card));
		}

		internal void SnapCalibration(VpCalibCard card)
		{
			if (card == null) return;
			SnapCalibration(EntryOf(card));
		}

		internal void AddCalibrationPoint(VpCalibCard card)
		{
			if (card == null) return;
			AddCalibrationPoint(EntryOf(card), card.GridPoints);
		}

		internal void RemoveCalibrationPoint(VpCalibCard card)
		{
			if (card == null) return;
			RemoveCalibrationPoint(EntryOf(card), card.GridPoints);
		}

		internal void ApplyCalibration(VpCalibCard card)
		{
			if (card == null) return;
			ApplyCalibration(EntryOf(card), card.GridPoints);
		}

		private void AddCalibration(CameraEntry entry, TextBox tbExp)
		{
			if (entry == null || _config.GetCalibration(entry.Key) != null) return;
			if (!EnsureIdle(T("vpAct_AddCalibration", "添加标定"))) return;

			VpCalibration calib = new VpCalibration();

			// 标定曝光：优先用卡片上"标定曝光"框的值，无法读取时用相机的默认曝光
			double exp = 0;
			if (tbExp != null) double.TryParse((tbExp.Text ?? "").Trim(), out exp);
			if (exp <= 0 && entry.Config != null) exp = entry.Config.Exposure;
			calib.Exposure = exp;

			_config.Calibrations[entry.Key] = calib;

			// 创建标定站、按当前配方加载 vpp，并挂到相机共用的显示控件上
			EnsureCalibrationStation(entry, calib);

			SaveVpConfig();
			RefreshCalibrationCard(entry);
			OperationLog.Write("VPForm", "添加标定：相机 " + entry.Key);
		}

		private void EditCalibrationTB(CameraEntry entry)
		{
			VpCalibration calib = _config.GetCalibration(entry.Key);
			if (calib == null || calib.Station == null) return;
			OperationLog.Write("VPForm", "打开 ToolBlock 编辑：相机 " + entry.Key);
			if (!calib.Station.IsLoadTBOk) calib.Station.LoadTB(calib.Station.GetVppPath(SysPara.RecipeName));
			calib.Station.EditTB();
		}

		/// <summary>应用标定专用曝光（与各 VPP 的曝光独立）；标定尚未创建时，值在点【添加标定】时采用。</summary>
		private void ApplyCalibrationExposure(CameraEntry entry, TextBox box)
		{
			if (entry == null || box == null) return;

			double exp;
			if (!double.TryParse((box.Text ?? "").Trim(), out exp) || exp <= 0) return;

			VpCalibration calib = _config.GetCalibration(entry.Key);
			if (calib == null) return;
			if (calib.Exposure == exp) return;

			calib.Exposure = exp;
			if (calib.Station != null)
			{
				calib.Station.RunExposure = exp;
				if (entry.Config != null) calib.Station.RunLiveCCDIndex = entry.Config.CameraIndex;
			}
			SaveVpConfig();
		}

		/// <summary>拍照：运行一次标定 ToolBlock，结果显示在本相机的共用显示控件上。</summary>
		private void SnapCalibration(CameraEntry entry)
		{
			VpCalibration calib = _config.GetCalibration(entry.Key);
			if (calib == null || calib.Station == null) return;
			calib.Station.RunTB();
		}

		/// <summary>用当前像素坐标添加一个标定点（电机坐标初始为 0，在表格中填写）。</summary>
		private void AddCalibrationPoint(CameraEntry entry, DataGridView grid)
		{
			VpCalibration calib = _config.GetCalibration(entry.Key);
			if (calib == null || calib.Station == null)
			{
				MessageBox.Show(T("vpMsg_NeedCalibration", "请先添加标定。"),
					T("vpMsg_Title", "提示"));
				return;
			}
			if (!calib.Station.IsAccept)
			{
				MessageBox.Show(T("vpMsg_NeedPixel", "请先点【拍照】取得像素坐标。"),
					T("vpMsg_Title", "提示"));
				return;
			}

			double px = calib.Station.Calibration.x;
			double py = calib.Station.Calibration.y;
			if (MessageBox.Show(T("vpMsg_AddPointConfirm", "确认添加标定点？\r\n像素坐标 (")
					+ px.ToString("F3") + ", " + py.ToString("F3") + ")",
					T("vpMsg_AddPointTitle", "添加标定点"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK) return;

			SyncPointsFromGrid(grid, calib);
			calib.Points.Add(new VpCalibPoint(px, py, 0, 0));
			FillCalibrationGrid(grid, calib);
			SaveVpConfig();
		}

		private void RemoveCalibrationPoint(CameraEntry entry, DataGridView grid)
		{
			VpCalibration calib = _config.GetCalibration(entry.Key);
			if (calib == null || grid == null || grid.CurrentRow == null) return;
			if (grid.CurrentRow.IsNewRow) return;

			SyncPointsFromGrid(grid, calib);
			int idx = grid.CurrentRow.Index;
			if (idx < 0 || idx >= calib.Points.Count) return;

			calib.Points.RemoveAt(idx);
			FillCalibrationGrid(grid, calib);
			SaveVpConfig();
		}

		/// <summary>把标定点写入本相机下所有 VPP（至少需要 9 个点）。</summary>
		private void ApplyCalibration(CameraEntry entry, DataGridView grid)
		{
			VpCalibration calib = _config.GetCalibration(entry.Key);
			if (calib == null)
			{
				MessageBox.Show(T("vpMsg_NeedCalibration", "请先添加标定。"),
					T("vpMsg_Title", "提示"));
				return;
			}

			SyncPointsFromGrid(grid, calib);
			if (calib.Points.Count < 9)
			{
				MessageBox.Show(T("vpMsg_Need9PointsA", "标定点至少 9 个（当前 ")
					+ calib.Points.Count + T("vpMsg_Need9PointsB", " 个）。"), T("vpMsg_Title", "提示"));
				return;
			}
			if (entry.Targets.Count == 0)
			{
				MessageBox.Show(T("vpMsg_NoStationToApply", "这台相机下还没有可应用标定的 VPP 工位。"),
					T("vpMsg_Title", "提示"));
				return;
			}

			List<CalibrationData> list = new List<CalibrationData>();
			for (int i = 0; i < calib.Points.Count; i++)
			{
				CalibrationData d = new CalibrationData();
				d.PixelX = calib.Points[i].PixelX;
				d.PixelY = calib.Points[i].PixelY;
				d.MotorPosX = calib.Points[i].MotorPosX;
				d.MotorPosY = calib.Points[i].MotorPosY;
				list.Add(d);
			}

			bool allOk = true;
			int okCount = 0;
			for (int i = 0; i < entry.Targets.Count; i++)
			{
				if (entry.Targets[i].SetCalibration(list, calib.ToolName)) okCount++;
				else allOk = false;
			}

			OperationLog.Write("VPForm", "应用标定：相机 " + entry.Key + " → "
				+ entry.Targets.Count + " 个 VPP");
			SaveVpConfig();

			if (allOk)
				MessageBox.Show(T("vpMsg_AppliedA", "标定已应用到本相机下 ") + okCount
					+ T("vpMsg_AppliedB", " 个 VPP：\r\n") + TargetNames(entry),
					T("vpMsg_CalibrationTitle", "标定"));
			else
				MessageBox.Show(T("vpMsg_PartialA", "只有 ") + okCount + "/" + entry.Targets.Count
					+ T("vpMsg_PartialB", " 个 VPP 成功。\r\n")
					+ T("vpMsg_PartialC", "失败的一般是它的 TB 里没有名为 \"")
					+ calib.ToolName + T("vpMsg_PartialD", "\" 的标定工具。\r\n")
					+ TargetNames(entry), T("vpMsg_CalibrationTitle", "标定"),
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		private static string TargetNames(CameraEntry entry)
		{
			string s = "";
			for (int i = 0; i < entry.Targets.Count; i++)
				s += (i > 0 ? "、" : "") + entry.Targets[i].VppSubFolder;
			return s;
		}

		// ---- 表格与标定点 ----

		private static void FillCalibrationGrid(DataGridView grid, VpCalibration calib)
		{
			if (grid == null) return;
			grid.Rows.Clear();
			if (calib == null) return;
			for (int i = 0; i < calib.Points.Count; i++)
			{
				VpCalibPoint p = calib.Points[i];
				grid.Rows.Add(p.PixelX.ToString("F3"), p.PixelY.ToString("F3"),
							  p.MotorPosX.ToString("F3"), p.MotorPosY.ToString("F3"));
			}
		}

		/// <summary>把表格里的电机坐标写回标定点（像素坐标只读，不改）。</summary>
		private static void SyncPointsFromGrid(DataGridView grid, VpCalibration calib)
		{
			if (grid == null || calib == null) return;

			List<VpCalibPoint> list = new List<VpCalibPoint>();
			for (int i = 0; i < grid.Rows.Count; i++)
			{
				DataGridViewRow r = grid.Rows[i];
				if (r == null || r.IsNewRow) continue;
				list.Add(new VpCalibPoint(
					ToDouble(r.Cells[0].Value), ToDouble(r.Cells[1].Value),
					ToDouble(r.Cells[2].Value), ToDouble(r.Cells[3].Value)));
			}
			calib.Points = list;
		}

		private static double ToDouble(object v)
		{
			if (v == null) return 0;
			double d;
			return double.TryParse(Convert.ToString(v), out d) ? d : 0;
		}

		#endregion
	}
}
