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

            // 设计器里放了一份"代表页"（VpCameraPage），只是为了**让 VS 设计视图能看到相机页长什么样**
            // （真实页面的数量由 VPForm.Cameras.xml 里的相机数决定，设计器里画不出来）。
            // 运行时立刻摘掉，真实页面完全按配置生成 —— 见 BuildVpPages / BuildCameraPage。
            RemoveDesignPreviewPage();

            // 设计器里原来的 3 个内置相机页已删除，本页**不再有自己的显示控件**
            // （原来是 cogRecDisp_H1_Recipe / H2 / H3，已随页面一起移除）。
            // 下面这 13 个 legacy 工位仍然必须存在：TaskProcess/Gantry.cs 的生产流程直接调用
            // MiddleLayer.VPF.H1_VFiducial.RunTB() 等，把它们从 VList 摘掉会让流程卡死。
            // 这些工位在界面上已无入口，结果显示统一挂到主界面那个显示上。
            H1_VFiducial.RecordDisplayList.Add(MiddleLayer.MainF.cogRecordDisplay1);
            H1_VFiducial2.RecordDisplayList.Add(MiddleLayer.MainF.cogRecordDisplay1);
            H1_VFiducial3.RecordDisplayList.Add(MiddleLayer.MainF.cogRecordDisplay1);
            H1_VFiducial4.RecordDisplayList.Add(MiddleLayer.MainF.cogRecordDisplay1);
            H1_VCalibration.RecordDisplayList.Add(MiddleLayer.MainF.cogRecordDisplay1);

            // 相机 / VPP 列表存在独立的 VPForm.Cameras.xml 里（不是 SettingData，
            // 免得跟着 ReadSettingData 的清表/读表流程一起丢），见 VpConfigStore。
            // 界面在 ModuleInitialize 之后才搭，见 BuildVpUi()。

            // 语言：静态文案由语言表负责（建完页登记一次，见 MiddleLayer.RegisterLanguage），
            // 这里只补"文字里带动态内容"的那几处（相机名 / 路径 / 点数 / 实时状态 / 工具条提示）。
            // 顺带解决"启动时 SysPara.LanguageShow 还没从 ini 赋好值"的问题 ——
            // InitialProject 里 SwitchLanguage() 在建完表单之后才跑，那一刻会把文案纠正过来。
            MiddleLayer.LanguageChanged += delegate { RefreshDynamicTexts(); };
		}
		// 视觉工位引用
		//
		// 原来这里是 17 个各写一份的类（H1_Vision_Calibration / H1_Vision_Fiducial..Fiducial4 / H2_* / H3_* / H4_*），
		// 除了类名和默认 CCD 号以外**代码完全一样**。现在全部换成同一个 VpStation：
		//   · 字段名保持原样（H1_VFiducial 等）→ 外部 `MiddleLayer.VPF.XXX`（TaskProcess/Gantry.cs）不用改；
		//   · 第一个参数就是原来那个类名 → **vpp 目录名不变，现场已有 vpp 文件继续可用**；
		//   · 第三个参数是原来构造函数里写死的默认 RunLiveCCDIndex（运行时仍会被配方里的 Pset 覆盖）；
		//   · 类型（Calibration / Fiducial）决定取哪些输出、十字线画多大。
		//
		// 注意：这些工位在界面上已经**没有任何入口**（内置相机页已删除），只为生产流程而存在。

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
        /// 生产流程用的视觉判据缓存 —— <c>TaskProcess/Gantry.cs</c> 会读 <c>VPF.dgv_H1_VisionData_List[0]</c>。
        /// 数据源是配方表 <c>tb_H1_Visdata</c>，由 <see cref="Getdgv_H1_VisionDataList"/> 刷进来。
        /// <b>这两个成员是对外契约，不要改名、不要删除。</b>
        /// （原来在界面上编辑这张表的是内置相机页，页面删除后已无编辑入口。）
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

		// 结构约定：
		//   tabControl1 的一页        = 一台相机（每台相机底下有自己的 VPP 子 TabControl）
		//   相机页里的子 TabControl 一页 = 一个 VPP
		//   一个 VPP 只能挂在创建它的那台相机下，不提供跨相机共用
		//
		// 视觉层这边不再为每个相机写类：VpStation 用"相机名\VPP名"决定 vpp 存哪，
		// 算法完全由 vpp 里的 ToolBlock 决定。老的 H1..H4 硬编码类一个都没动，
		// 它们的 vpp 路径仍按类名走，现场已有文件不受影响。
		//
		// 配置持久化：ModuleData\SettingData\VPForm.Cameras.xml（机器级，不随配方变）
		// vpp 文件位置：VisionData\{相机名}\{VPP名}\{配方名}.vpp

		// 运行时构建的相机页 / VPP 页的控件名前缀（方便按名字找回来）
		private const string DynCameraPrefix = "dynCam_";
		private const string DynVppPrefix = "dynVppPage_";
		/// <summary>设计器里那份"代表页"的页名（仅为让 VS 设计视图看到相机页骨架，运行时移除）。</summary>
		private const string PreviewPageName = "tabPageDesignPreview";

		private Panel _vpBar;
		private bool _vpUiBuilt;
		/// <summary>建页失败时的原因（非空就会显示在工具条上）。</summary>
		private string _vpBuildError;
		/// <summary>
		/// 有"改了但还没落盘"的参数改动。
		/// VPForm 里的**参数**（相机名/CameraIndex/默认曝光/各 VPP 曝光与名称/标定曝光）
		/// 一律先改内存、标脏，统一由**主界面的【保存】按钮**提交；
		/// 点"否"（取消）会重新读盘并重建界面 ⇒ 参数回到上一次保存。
		/// </summary>

		/// <summary>VPForm.Cameras.xml 的全部内容：相机 / VPP / 各自的标定。</summary>
		private readonly VpConfigFile _config = new VpConfigFile();

		/// <summary>运行时配置的相机（设计器里原来的 3 个内置相机页已删除，现在全部来自这里）。</summary>
		private List<VpCameraConfig> _cameras { get { return _config.Cameras; } }

		/// <summary>
		/// 相机页的统一定义 —— 每一页 = 一台相机，全部由运行时构建。
		/// 标定模块（<see cref="VpCalibration"/>）就挂在这里：
		/// 一台相机一份标定，应用到该相机下的所有 VPP 工位（Targets）。
		/// </summary>
		private class CameraEntry
		{
         /// <summary>配置里的相机键（就是相机名）。</summary>
            public string Key;
			public TabPage Page;
			/// <summary>产生这一页的配置。</summary>
			public VpCameraConfig Config;
			/// <summary>本相机下要应用标定的 VPP 工位。</summary>
			public List<VpStation> Targets = new List<VpStation>();
			/// <summary>标定卡片落在哪个容器里。</summary>
			public Control CalibHost;
			/// <summary>
			/// **整台相机共用的显示控件（一个相机只有一个）**。
			/// 标定和本相机下每个 VPP 都挂它进自己的 RecordDisplayList，画面都出在这里。
			/// </summary>
			public Cognex.VisionPro.CogRecordDisplay Display;
			/// <summary>标定卡片是否铺满宿主（动态相机的 calibHost 是专用容器，铺满）。</summary>
			public bool HostFill;
			/// <summary>相机属性条上的"实时显示"按钮（文字随语言 + 实时状态变，切语言时要重刷）。</summary>
			public Button LiveButton;
		}

		private readonly List<CameraEntry> _entries = new List<CameraEntry>();
		/// <summary>各 VPP 页上的补偿限制表（表头/行名是动态文案，切语言要重算；页面重建时清空）。</summary>
		private readonly List<DataGridView> _compGrids = new List<DataGridView>();

		private CameraEntry FindEntry(string key)
		{
			for (int i = 0; i < _entries.Count; i++)
				if (_entries[i].Key == key) return _entries[i];
			return null;
		}

		/// <summary>
		/// 基类建好模块名之后再搭界面。
		/// 相机/VPP 列表读的是独立配置文件，跟 SettingData 无关，
		/// 放在这里只是为了保证"界面已初始化完再往里加页"。
		/// </summary>
		public override void ModuleInitialize(string ModuleName)
		{
			base.ModuleInitialize(ModuleName);
			try
			{
				BuildVpUi();
			}
			catch (Exception ex)
			{
				// 以前这里是**静默** catch —— 结果是"页面只搭了一半，却没有任何提示"，
				// 排查起来只能靠挂调试器。现在至少把原因留在输出窗口和工具条上。
				_vpBuildError = ex.GetType().Name + ": " + ex.Message;
				System.Diagnostics.Debug.WriteLine("[VPForm] BuildVpUi 失败 → " + ex);
				UpdateVpHint();
			}
		}

		/// <summary>
		/// 工具条右侧：**平时留空**，只在建页出错时红字报原因。
		/// （"参数改了但还没保存"的橙字提醒已按需求删除 —— 未保存状态只由主界面保存流程处理。）
		/// </summary>
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

		/// <summary>
		/// 按当前 <see cref="_config"/> 把相机页搭出来。
		/// 设计器里原来那 3 个相机页（`tabPage1` Pick PCB / `tabPage2` Screw1 / `tabPage8` Screw2）
		/// **已经在设计器里彻底删除**，`tabControl1` 里一页都没有。
		/// 但 legacy 的 13 个视觉工位（`H1_VFiducial` 等）**必须保留** —— 生产流程还在用它们，
		/// 只是界面上已经没有入口了。
		/// </summary>
		private void BuildVpPages()
		{
			for (int i = 0; i < _cameras.Count; i++)
			{
				CreateCameraStations(_cameras[i]);
				tabControl1.TabPages.Add(BuildCameraPage(_cameras[i]));
			}
		}

		/// <summary>
		/// 摘掉设计器里那份"代表页"（<c>tabPageDesignPreview</c>）。
		///
		/// 它存在的唯一理由：相机页数量由配置决定，设计器里画不出来；在 VPForm.Designer.cs 里放一份
		/// **VpCameraPage 实例**，VS 设计视图就能看到相机页的真实骨架（属性条 / 标定卡区 / VPP 子页 / 右侧显示）。
		/// 运行时它没有任何用途（还会多出一个空页），构造时立刻移除。
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

			// 平时是空的，只在建页出错时红字报原因（见 UpdateVpHint）
			// 文字是按当前语言现算的，所以不进语言表（_dyn）
			UiLabel hint = new UiLabel();
			hint.Name = DynTextName("vpBarHint");
			hint.Dock = DockStyle.Fill;
			hint.AutoSize = false;
			hint.TextAlign = ContentAlignment.MiddleRight;
			hint.Font = new Font("宋体", 10.5F);
			hint.ForeColor = Color.FromArgb(110, 120, 132);
			hint.Padding = new Padding(0, 0, 12, 0);

			// 停靠顺序：后加的先生效 ⇒ 先 Left（按钮条）再 Fill（提示）
			bar.Controls.Add(hint);
			bar.Controls.Add(btns);

			// tabControl1 是 Dock=Fill，工具条最后 Add 到窗体上才能占住顶部一条
			Controls.Add(bar);

			// 按名字登记进语言表 → 以后切语言由 SwitchLanguage 自动换字（见 MiddleLayer.RegisterLanguage）
			MiddleLayer.RegisterLanguage(bar, LangForm);
		}

		// ---------------- 工具条按钮 / 属性条标签：见上面带语言登记的 AddBarButton / MakeCaptionL ----------------

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

			TabPage page = BuildCameraPage(cam);
			tabControl1.TabPages.Add(page);
			tabControl1.SelectedTab = page;

			SaveVpConfig();
		}

		/// <summary>
		/// 一台相机的整页：**骨架由设计器定义**（<see cref="VpCameraPage"/>：属性条 / 标定卡区 /
		/// VPP 子页容器 / 右侧显示区），这里只负责把 ActiveX 显示控件、标定卡、各 VPP 页
		/// 按配置填进它留出来的容器里。
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
			ctl.RefreshTexts();          // 属性条文案：新建的页面当场就该是当前语言
			page.Controls.Add(ctl);

			// ---- 整台相机共用的显示控件（一个相机只有一个）----
			// 标定、以及本相机下每一个 VPP 都把结果显示到这里（都挂进各自的 RecordDisplayList）。
			// ★ 必须先塞 OcxState，再设下面的属性 ★
			// `CogRecordDisplay` 是 **ActiveX（AxHost）控件**：代码 new 出来的没有设计器那行 OcxState
			// ⇒ 实例根本不会创建，只画一块深蓝占位色，读属性还会抛 AxHost.InvalidActiveXStateException。
			// 这里用的就是旧设计器里那份状态（同一个控件，通用），所以显示控件仍留在代码里建、不搬进设计器。
			Cognex.VisionPro.CogRecordDisplay sharedDisp = new Cognex.VisionPro.CogRecordDisplay();
			sharedDisp.Name = "vpCamDisp_" + cam.Name;
			try
			{
				object ocx = new System.ComponentModel.ComponentResourceManager(typeof(VPForm))
					.GetObject("vpDynDisp.OcxState");
				if (ocx is AxHost.State) sharedDisp.OcxState = (AxHost.State)ocx;
			}
			catch (Exception) { }

			// OcxState 之后再设，保证这里的排版不被状态里的旧值盖掉
			sharedDisp.Dock = DockStyle.Fill;
			sharedDisp.ColorMapPredefined = Cognex.VisionPro.Display.CogDisplayColorMapPredefinedConstants.None;
			sharedDisp.ColorMapLowerClipColor = Color.Black;
			sharedDisp.ColorMapLowerRoiLimit = 0D;
			sharedDisp.ColorMapUpperClipColor = Color.Black;
			sharedDisp.ColorMapUpperRoiLimit = 1D;
			ctl.DispHost.Controls.Add(sharedDisp);

			// entry 必须先登记：下面建 VPP 页时要用到 entry.Display
			CameraEntry entry = new CameraEntry();
			entry.Key = cam.Name;
			entry.Page = page;
			entry.Config = cam;
			entry.Display = sharedDisp;
			_entries.Add(entry);

			// ---- 属性条上的两个按钮 ----
			// 「实时显示」是**相机级**的（一台相机只有一个显示控件）：取流参数 = 本相机 CameraIndex
			// + 当前选中那个 VPP 的曝光。文字跟"语言 + 实时状态"走 → 不进语言表（名字带 _dyn）。
			ctl.LiveButton.Click += delegate
			{
				LiveCamera(entry);
				RefreshLiveButton(ctl.LiveButton, entry);
			};
			entry.LiveButton = ctl.LiveButton;
			RefreshLiveButton(ctl.LiveButton, entry);
			ctl.DeleteCameraButton.Click += delegate { DeleteCamera(cam); };

			// 参数编辑：没有"应用"按钮 —— 数字改完就地生效，名称要换 vpp 目录所以留到失焦/回车
			AttachCameraEditors(cam, page, ctl.NameBox, ctl.IndexBox, ctl.ExposureBox);

			// ---- 该相机自己的 VPP 容器：先把 TabControl 建出来再建页面 ----
			TabControl vppTabs = ctl.VppTabs;
			vppTabs.Tag = cam;
			for (int i = 0; i < cam.Vpps.Count; i++)
				vppTabs.TabPages.Add(BuildVppPage(cam, cam.Vpps[i], vppTabs));

			for (int i = 0; i < cam.Vpps.Count; i++)
				if (cam.Vpps[i].Station != null) entry.Targets.Add(cam.Vpps[i].Station);

			// ---- 标定卡片（相机级：一台相机一份，应用到本相机下所有 VPP）----
			entry.CalibHost = ctl.CalibHostPanel;
			entry.HostFill = true;      // 这个宿主是专为本卡片造的容器，铺满
			RefreshCalibrationCard(entry);

			// 建完了：把这一页（含标定卡、每个 VPP 页）的静态文案按名字登记进语言表。
			// 文件名/键 = 控件名，之后切语言由 SwitchLanguage 自动换字（跟 Gantry 那些页面同一套）。
			MiddleLayer.RegisterLanguage(page, LangForm);
			return page;
		}

		private static void RegisterDisplay(VpStation st, Cognex.VisionPro.CogRecordDisplay disp)
		{
			if (st == null || disp == null) return;
			for (int i = 0; i < st.RecordDisplayList.Count; i++)
				if (ReferenceEquals(st.RecordDisplayList[i], disp)) return;
			st.RecordDisplayList.Add(disp);
		}

		// ==================== 参数编辑（没有"应用"按钮）====================
		//
		// 原来相机属性条 / VPP 页 / 标定卡上各有一个"应用 xxx"按钮，现在全去掉：
		// 参数一改就直接落到内存对象上，统一由主界面【保存】写盘，点"否"回到上一次保存。
		//
		// 两类字段的时机不一样：
		//   · 数字（CameraIndex / 默认曝光 / VPP 曝光 / 标定曝光）—— TextChanged 就地生效，很轻；
		//   · 名称（相机名 / VPP 名）—— 改名会连带换 vpp 目录、挪标定的键，不能边打字边改，
		//     所以打字只标脏，真正的改名留到失焦（或回车）那一刻。
		//     标脏这一步不能省：用户打完字直接点主界面【保存】时，保存流程得先知道"有改动"。

		private void AttachCameraEditors(VpCameraConfig cam, TabPage page, TextBox tbName, TextBox tbIndex, TextBox tbExposure)
		{
			if (cam == null) return;

			tbIndex.TextChanged += delegate { ApplyCameraNumbers(cam, tbIndex, tbExposure); };
			tbExposure.TextChanged += delegate { ApplyCameraNumbers(cam, tbIndex, tbExposure); };
			// 失焦时把解析不了的中间状态还原成当前值，别让框里留着落不了地的数字
			tbIndex.Leave += delegate { NormalizeCameraNumbers(cam, tbIndex, tbExposure); };
			tbExposure.Leave += delegate { NormalizeCameraNumbers(cam, tbIndex, tbExposure); };

			tbName.TextChanged += delegate { };   // 改名不边打字边处理 —— 失焦/回车时统一应用（见 ApplyCameraRename）
			tbName.Leave += delegate { ApplyCameraRename(cam, tbName, page); };
			tbName.KeyDown += delegate (object s, KeyEventArgs e)
			{
				if (e.KeyCode != Keys.Enter) return;
				e.SuppressKeyPress = true;
				ApplyCameraRename(cam, tbName, page);
			};
		}

		/// <summary>CameraIndex / 默认曝光 改一下就生效（只改内存，等主界面【保存】落盘）。</summary>
		private void ApplyCameraNumbers(VpCameraConfig cam, TextBox tbIndex, TextBox tbExposure)
		{
			if (cam == null) return;
			bool changed = false;

			int idx;
			if (int.TryParse((tbIndex.Text ?? "").Trim(), out idx) && idx >= 0 && idx != cam.CameraIndex)
			{
				cam.CameraIndex = idx;

				// 通道号是相机级的 → 同步给本相机下所有工位；
				// **曝光不动**：每个 VPP 的曝光在它自己页上改，标定另有自己的，三者互不影响。
				for (int i = 0; i < cam.Vpps.Count; i++)
					if (cam.Vpps[i].Station != null) cam.Vpps[i].Station.RunLiveCCDIndex = idx;
				VpCalibration cal = _config.GetCalibration(cam.Name);
				if (cal != null && cal.Station != null) cal.Station.RunLiveCCDIndex = idx;

				// 每个 VPP 页顶部那行"相机：X（Index N）"会过期，跟着刷
				RefreshVppInfoTexts(cam);
				changed = true;
			}

			double exp;
			if (double.TryParse((tbExposure.Text ?? "").Trim(), out exp) && exp > 0 && exp != cam.Exposure)
			{
				cam.Exposure = exp;          // 只是默认值，供以后新建 VPP / 添加标定时继承
				changed = true;
			}

			if (changed) SaveVpConfig();
		}

		/// <summary>失焦时把解析不了的数字还原成当前值。</summary>
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

		/// <summary>相机改名：失焦/回车时才做（改的是 vpp 目录名，不能边打字边改）；值不合法就把框还原。</summary>
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

			// 相机改名 → 它下面所有 vpp 的目录名跟着改（老目录里的 vpp 文件不会自动搬过来）
			for (int i = 0; i < cam.Vpps.Count; i++)
				if (cam.Vpps[i].Station != null) cam.Vpps[i].Station.SetOwner(newName, cam.Vpps[i].Name);

			cam.Name = newName;
			page.Name = DynCameraPrefix + newName;
			page.Text = newName;

			// ★ 标定是以**相机名**做键的（VpConfigFile.Calibrations / GetCalibration）。
			//   改名必须把键和 entry.Key 一起挪过去，否则这台相机的标定就"找不到了"。
			VpCalibration calOld = _config.GetCalibration(oldName);
			if (calOld != null)
			{
				_config.Calibrations.Remove(oldName);
				_config.Calibrations[newName] = calOld;
				if (calOld.Station != null) calOld.Station.SetOwner(newName, calOld.FolderName);
			}
			if (entry != null) entry.Key = newName;

			RefreshVppInfoTexts(cam);
			// 标定卡片标题里也写着相机名 → 重建一次
			if (entry != null) RefreshCalibrationCard(entry);

			SaveVpConfig();
		}

		/// <summary>
		/// 刷新本相机下每个 VPP 页顶部的归属行与路径行
		/// （相机名 / CameraIndex / VPP 名 / vpp 路径 —— 相机改名、改 Index、换配方都要刷）。
		/// </summary>
		private void RefreshVppInfoTexts(VpCameraConfig cam)
		{
			TabControl tabs = FindVppTabControl(cam);
			if (tabs == null) return;

			for (int i = 0; i < tabs.TabPages.Count; i++)
			{
				TabPage p = tabs.TabPages[i];

				// 归属行与路径行由 VpVppPage 自己现算（文字带相机名 / 通道号 / 配方路径，属 _dyn 动态文案）
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
		/// 配方切换后由 <see cref="MiddleLayer.OpenVision"/> 调一次。
		///
		/// vpp 的重新载入是在 OpenVision 里做的（动态相机/VPP 与相机级标定站都在
		/// <c>VisionproInterface.VList</c> 里，所以**会**跟着配方换），
		/// 这里只负责把界面上那些"建页时算好的路径字符串"刷成新的 ——
		/// 否则换完配方，实际用的 vpp 已经换了，界面上还写着老配方的路径。
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

		/// <summary>标定卡上那行 vpp 路径（**按当前配方现算**，不要读 Station.TBPath）。</summary>
		private static string BuildCalibTipText(VpCalibration calib)
		{
			if (calib == null) return "";
			if (calib.Station == null) return T("vpPath_NotLoaded", "vpp：(未加载)");
			return T("vpPath_Prefix", "vpp：") + calib.Station.GetVppPath(SysPara.RecipeName);
		}

		/// <summary>标定卡标题（跟着**语言 + 已录点数**变）。</summary>
		private static string BuildCalibTitleText(CameraEntry entry, VpCalibration calib)
		{
			string head = T("vpCalib_Title", "标定");
			string tail = (calib == null)
				? T("vpCalib_NotAdded", "　　尚未添加标定")
				: T("vpCalib_PointsA", "　　已录 ") + calib.Points.Count
				  + T("vpCalib_PointsB", " 点（至少 9 点）");
			return head + "　" + entry.Key + tail;
		}

		/// <summary>
		/// 只重写标定卡上"带动态内容"的两行文字：标题（相机名 + 点数）与 vpp 路径（配方相关）。
		/// 不重建整张卡，免得把标定点表格也刷一遍。
		/// </summary>
		private void RefreshCalibrationTexts(CameraEntry entry)
		{
			if (entry == null || entry.CalibHost == null) return;

			// 卡片是设计器定义的控件，标题/提示由它自己现算（带相机名与点数，属 _dyn 动态文案）
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

			// 摘掉每个 VPP 的视觉站与显示控件
			for (int i = cam.Vpps.Count - 1; i >= 0; i--)
				RemoveVpp(cam, cam.Vpps[i], null);

			// 相机级标定也跟着一起没了（连同它的视觉站）
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

			SaveVpConfig();
		}

		private VpCameraConfig CurrentCameraConfig()
		{
			TabPage p = tabControl1.SelectedTab;
			if (p == null) return null;
			return p.Name.StartsWith(DynCameraPrefix) ? FindCamera(p.Name.Substring(DynCameraPrefix.Length)) : null;
		}

		/// <summary>工具条上的"删除当前相机"：只对运行时新增的相机有效（设计器里那 3 页已不再保留）。</summary>
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
			vpp.Exposure = cam.Exposure;      // 曝光各管各的，但新 VPP 从相机的默认曝光继承一个起点
			cam.Vpps.Add(vpp);

			TabPage page = BuildVppPage(cam, vpp, vppTabs);
			vppTabs.TabPages.Add(page);
			vppTabs.SelectedTab = page;

			// 新 VPP 也是本相机标定要覆盖的对象
			CameraEntry entry = FindEntry(cam.Name);
			if (entry != null && vpp.Station != null) entry.Targets.Add(vpp.Station);

			MiddleLayer.RegisterLanguage(page, LangForm);   // 静态文案按名字登记进语言表
			SaveVpConfig();
		}

		/// <summary>
		/// 一个 VPP 页：**布局已移到 <see cref="VpVppPage"/> 的设计器里**（VS 设计视图看到的就是 EXE 里的样子），
		/// 这里只负责造 TabPage、实例化那一页控件、挂上数据、接上编辑逻辑。
		/// </summary>
		private TabPage BuildVppPage(VpCameraConfig cam, VpVppConfig vpp, TabControl vppTabs)
		{
			if (vpp.Station == null) CreateStation(cam, vpp);

			TabPage page = new TabPage();
			page.Name = DynVppPrefix + cam.Name + "_" + vpp.Name;
			page.Text = vpp.Name;
			page.Padding = new Padding(3);
			page.UseVisualStyleBackColor = true;
			// 相机级「实时显示」要靠它拿到"当前 VPP 的曝光"（见 LiveCamera / CurrentVpp）
			page.Tag = vpp;

			// ---- 结果画到"整台相机共用"的那个显示控件上，本页不再自己建显示 ----
			CameraEntry entry = FindEntry(cam.Name);
			Cognex.VisionPro.CogRecordDisplay display = (entry != null) ? entry.Display : null;
			if (vpp.Station != null) RegisterDisplay(vpp.Station, display);

			// ---- 页面本体是设计器定义的控件 ----
			VpVppPage ctl = new VpVppPage();
			ctl.Owner = this;
			ctl.Cam = cam;
			ctl.Vpp = vpp;
			ctl.Dock = DockStyle.Fill;
			ctl.Bind();
			page.Controls.Add(ctl);

			// 曝光 / VPP 名 的编辑逻辑与页面布局无关，仍留在这边
			AttachVppEditors(cam, vpp, page, ctl.InfoLabel, ctl.ExposureBox, ctl.RenameBox);

			// 切语言时要重填补偿限制表的表头（DataGridView 不进语言表）
			_compGrids.Add(ctl.CompGrid);

			return page;
		}

		internal string BuildVppInfoText(VpCameraConfig cam, VpVppConfig vpp)
		{
			return T("vpInfo_Camera", "相机：") + cam.Name
				 + " (Index " + cam.CameraIndex + ")"
				 + T("vpInfo_Vpp", "　　VPP：") + (vpp != null ? vpp.Name : "");
		}

		/// <summary>
		/// VPP 页与标定卡共用的路径行文字。
		/// 路径**按当前配方现算**，不要用 <c>Station.TBPath</c> —— 那是上次 LoadTB 时存下来的字符串，
		/// 配方一换（物料管理→使用）就过期了，界面上会一直写着老配方的路径。
		/// </summary>
		internal static string BuildVppPathText(VpVppConfig vpp)
		{
			if (vpp == null || vpp.Station == null)
				return T("vpPath_NotLoaded", "vpp：(未加载)");
			return T("vpPath_Prefix", "vpp：") + vpp.Station.GetVppPath(SysPara.RecipeName);
		}

		/// <summary>
		/// 补偿限制表：行**由用户增删**（工具条【添加行】/【删除行】），每行 = 项目名（可编辑）+ 下限 + 上限。
		/// 表头是语言键（vpCompCol_*），切语言时经 <see cref="RefreshDynamicTexts"/> 重填
		/// （DataGridView 不进语言表 —— 它的文字是数据）。行对象引用就存在 <see cref="DataGridViewRow.Tag"/> 上，
		/// 编辑写回 / 删行都靠它，不受改名影响。
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
				grid.Rows[idx].Tag = it;    // 行 ↔ 配置对象 一一对应
			}
		}

		/// <summary>
		/// 【添加行】：表尾追加一行限制并**即时落盘**，然后直接进入名称单元格编辑。
		/// 新行**自动带默认名"补偿N"**（跳过重名）—— 空名字的行不会被写进配置
		/// （<see cref="VpConfigStore.Save"/> 跳过空 Item），不加默认名的话"加一行不填名字"
		/// 看起来就是"添加没生效"（重启/切页后行消失）。
		/// </summary>
		internal void AddCompLimitRow(DataGridView grid, VpVppConfig vpp)
		{
			if (grid == null || vpp == null) return;

			string name;
			int n = vpp.CompLimits.Count + 1;
			while (vpp.GetCompLimit(name = "补偿" + n) != null) n++;

			VpCompLimitItem it = new VpCompLimitItem(name, 0, 0);
			vpp.CompLimits.Add(it);
			FillCompLimitGrid(grid, vpp);
			SaveVpConfig();

			int idx = grid.Rows.Count - 1;
			if (idx >= 0)
			{
				grid.CurrentCell = grid.Rows[idx].Cells[0];
				grid.BeginEdit(true);
			}
		}

		/// <summary>【删除行】：删掉当前选中的那行（按 <see cref="DataGridViewRow.Tag"/> 里的对象引用删，改名不影响）。**即时写盘**。</summary>
		internal void RemoveCompLimitRow(DataGridView grid, VpVppConfig vpp)
		{
			if (grid == null || vpp == null) return;

			DataGridViewRow row = grid.CurrentRow;
			if (row == null || row.Index < 0) return;

			VpCompLimitItem it = row.Tag as VpCompLimitItem;
			if (it == null) return;

			vpp.CompLimits.Remove(it);
			FillCompLimitGrid(grid, vpp);
			SaveVpConfig();
		}

		/// <summary>
		/// 编辑结束把单元格写回 <see cref="VpCompLimitItem"/>：名称留空还原、限位解析失败还原、下限不许高过上限。
		/// 补偿限制表**不跟主页【保存】**—— 有改动立即写盘（<see cref="SaveVpConfig"/>），免得"改了限制还要记得去主页点保存"。
		/// </summary>
		internal void SyncCompLimitFromGrid(DataGridView grid, VpVppConfig vpp, int rowIndex)
		{
			if (grid == null || vpp == null || rowIndex < 0 || rowIndex >= grid.Rows.Count) return;

			DataGridViewRow row = grid.Rows[rowIndex];
			VpCompLimitItem it = row.Tag as VpCompLimitItem;
			if (it == null) return;

			bool changed = false;

			// 项目名：留空还原；改了就收下
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

			// 下限不能高过上限：越界就把下限拉回来（只动界，不动值）
			if (it.Min > it.Max)
			{
				it.Min = it.Max;
				row.Cells[1].Value = it.Min.ToString("F3");
				changed = true;
			}

			if (changed) SaveVpConfig();
		}

		/// <summary>建视觉站。构造函数里会自动登记进 VisionproInterface.VList，OpenVision 之后就按配方加载 vpp。</summary>
		private void CreateStation(VpCameraConfig cam, VpVppConfig vpp)
		{
			VpStation st = new VpStation(cam.Name, vpp.Name);
			st.RunLiveCCDIndex = cam.CameraIndex;      // 通道号是相机的
			st.RunExposure = vpp.Exposure;             // 曝光是**每个 VPP 各管各的**
			st.Config = vpp;                           // 回填配置引用：补偿限制的方法直接读它，调用时零查找
			// 立刻按当前配方读一次；文件不存在时 LoadTB 会新建一个空 ToolBlock 并存盘
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

		/// <summary>拍照。AutoVisionRunDisplay=true，跑完结果会自动画到这个 VPP 自己的显示控件上。</summary>
		internal void RunStation(VpVppConfig vpp)
		{
			if (vpp == null || vpp.Station == null) return;
			vpp.Station.RunTB();
		}

		/// <summary>
		/// **相机级实时显示** —— 一台相机只有一个显示控件，所以"实时"也挂在相机上（原来挂在每个 VPP 页上）。
		/// 取流参数：通道号用相机的 <c>CameraIndex</c>，曝光用**当前选中的那个 VPP** 的 Exposure。
		/// （曝光是每个 VPP 各管各的；标定的曝光不参与这里。）
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

		/// <summary>
		/// 实时按钮的文字/底色跟着状态走，不然看不出到底开没开。
		/// <b>必须走 <see cref="IsLiveRunning"/></b>，不能直接读 <c>Display.LiveDisplayRunning</c>（原因见那个方法）。
		/// </summary>
		private static void RefreshLiveButton(Button btn, CameraEntry entry)
		{
			if (btn == null) return;
			bool live = IsLiveRunning(entry);
			btn.Text = LiveButtonText(entry);
			btn.BackColor = live ? Color.FromArgb(255, 232, 204) : Color.FromArgb(232, 244, 255);
		}

		/// <summary>实时按钮的文字：跟着**当前语言 + 实时状态**变（Tag 里登记的那个工厂也调它）。</summary>
		private static string LiveButtonText(CameraEntry entry)
		{
			return IsLiveRunning(entry)
				? T("vpBtn_StopLive", "停止实时")
				: T("vpBtn_Live", "实时显示");
		}

		/// <summary>
		/// 安全地问"这台相机的显示控件现在是不是在实时"。
		///
		/// `CogRecordDisplay` 是 **ActiveX（AxHost）包出来的控件**：句柄/COM 对象还没建出来时，
		/// 读它的属性会抛 `System.Windows.Forms.AxHost+InvalidActiveXStateException` ——
		/// 而建相机页的时候这个显示控件刚 new 出来、还没挂到父容器上，正好就是这个状态
		/// （`ColorMap*` 那几个属性是本地缓存所以不报，`LiveDisplayRunning` 要问 COM，就炸了）。
		/// 所以：没建句柄 / 已释放 / COM 没就绪，一律当成"没在实时"。
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

		/// <summary>保存某个 VPP 自己的曝光（只影响这个 VPP 的实时取流）。</summary>
		/// <summary>VPP 页上的两个编辑框：曝光边改边生效，名字留到失焦/回车（改名要换 vpp 目录）。</summary>
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

			tbRename.TextChanged += delegate { };   // 改名不边打字边处理 —— 失焦/回车时统一应用（见 ApplyVppRename）
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

		/// <summary>VPP 改名：失焦/回车时才做（会换 vpp 目录）；值不合法就把框还原。</summary>
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

				vpp.Name = newName;
				if (vpp.Station != null)
				{
					vpp.Station.SetOwner(cam.Name, newName);
					// 目录换了，按当前配方重新读一次（老目录里的 vpp 还在，需要手工搬）
					vpp.Station.LoadTB(vpp.Station.GetVppPath(SysPara.RecipeName));
				}

				page.Text = newName;
				page.Name = DynVppPrefix + cam.Name + "_" + newName;
				info.Text = BuildVppInfoText(cam, vpp);
				SaveVpConfig();
			}
			catch (Exception) { }
		}

		/// <summary>删 VPP 前的确认框（文案同一份）。</summary>
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

			RemoveVpp(cam, vpp, vppTabs);
			SaveVpConfig();
		}

		/// <summary>彻底摘掉一个 VPP：停掉实时显示、注销视觉站、移除界面页与配置。</summary>
		private void RemoveVpp(VpCameraConfig cam, VpVppConfig vpp, TabControl vppTabs)
		{
			if (cam == null || vpp == null) return;

			string vppName = vpp.Name;

			// 0) 不再作为标定的应用对象
			CameraEntry entry = FindEntry(cam.Name);
			if (entry != null && vpp.Station != null) entry.Targets.Remove(vpp.Station);

			// 1) 视觉站
			if (vpp.Station != null)
			{
				try
				{
					for (int i = 0; i < vpp.Station.RecordDisplayList.Count; i++)
					{
						Cognex.VisionPro.CogRecordDisplay d = vpp.Station.RecordDisplayList[i];
						if (d == null) continue;
						// 相机共用的显示控件不能停：它可能正被同相机别的 VPP / 标定用着
						if (entry != null && ReferenceEquals(d, entry.Display)) continue;
						try { d.StopLiveDisplay(); } catch (Exception) { }
					}
				}
				catch (Exception) { }

				VisionproInterface.VList.Remove(vpp.Station);
				vpp.Station = null;
			}

			// 2) 界面
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

		/// <summary>
		/// 立即写盘。**只给"结构类"改动用**（增删相机 / 增删 VPP / 添加标定 / 增删标定点）——
		/// 这些会创建或销毁视觉工位、换 vpp 目录，做成"可取消"代价太大，所以即时生效。
		/// 参数类改动（名称 / CameraIndex / 各种曝光）请用 <see cref="SaveVpConfig"/>（即时落盘）。
		/// </summary>
		internal void SaveVpConfig()
		{
			try
			{
				VpConfigStore.Save(_config);
			}
			catch (Exception) { }
		}

		/// <summary>
		/// 主界面【保存】按"是"时调用：把相机 / VPP / 标定参数写进 VPForm.Cameras.xml。
		/// 这份配置不在 `SettingData` 里，`ModuleList` 那圈写盘覆盖不到，所以 <c>MainForm.SaveData()</c> 要单独叫一次。
		/// 参数现在都是**即时落盘**的（改动当场 <see cref="SaveVpConfig"/>），这里只是兜底再存一次，保持主保存链路兼容。
		/// </summary>
		public void CommitVpConfig()
		{
			SaveVpConfig();
		}

		/// <summary>
		/// 主界面【保存】按"否"（取消）时调用：整体拆掉 → 重新读盘 → 重建界面，
		/// 把"已经作用到运行中视觉工位上的内存改动"（RunExposure / RunLiveCCDIndex / vpp 路径）拉回盘上的值。
		/// （参数改动本身都已即时落盘，"否"只是把界面和工位状态拉回与磁盘一致。）
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

		/// <summary>
		/// 进页面：配方可能在别的地方被换过（物料管理 → 使用），把界面上那些路径按当前配方重写一遍。
		/// （离开页面不再弹保存确认 —— 参数都是即时落盘的，没有"未保存"状态。）
		/// </summary>
		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			if (!Visible) return;

			// 进页面：配方可能在别的地方被换过（物料管理 → 使用），把界面上那些路径按当前配方重写一遍
			OnRecipeChanged();
		}

		/// <summary>
		/// 把界面上所有相机页和它们底下的工位全部拆掉（**不碰磁盘、不碰 <see cref="_config"/>**）。
		/// 供 <see cref="RevertVpConfig"/> 重建用。
		/// </summary>
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
					// 不回写 TB：取消时它的路径可能正是要被丢掉的那个
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

		/// <summary>外部取**相机配置实例**（名字 / CameraIndex / Vpps / 补偿限制都在里面）：按相机名找，没有返回 null。</summary>
		public VpCameraConfig GetCameraConfig(string cameraName)
		{
			return FindCamera(cameraName);
		}

		/// <summary>外部取某个 VPP 的**视觉站**（触发拍照 / 读结果 / 读 TB 输出都用它）；没有返回 null。</summary>
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

		/// <summary>
		/// 取这台相机的 VPP 容器（那个 TabControl）。
		///
		/// ★ 相机页的布局已经搬进 <see cref="VpCameraPage"/> 设计器，VPP TabControl 是它的**内部子控件**，
		/// 不再是 TabPage 的直接子控件 —— 原来按"直接子控件里找 TabControl"会永远找不到，
		/// 于是 AddVpp/DeleteVpp 都拿不到容器、静默 return（表现为「点添加 VPP 没反应」）。
		/// 正确做法：TabPage → 里面的 VpCameraPage → 它的 VppTabs。
		/// </summary>
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

					// 兜底：万一以后又有人把 TabControl 直接挂在页上
					if (p.Controls[j] is TabControl) return (TabControl)p.Controls[j];
				}
			}
			return null;
		}

		// ==================== 语言：跟 Gantry 那类页面走同一套 ====================
		//
		// Gantry 页面里**一行语言代码都没有**：`InitialLanguageData()` 启动时按 `control.Name`
		// 把控件登记进语言表（文案从 LanguageData\{语言}.xml 的 /{语言}/{窗体}/{控件名} 取），
		// `SwitchLanguage()` 再把表里的 ComponentText 灌回控件。
		//
		// 本页的界面是 `ModuleInitialize` 之后才搭的，赶不上启动那一次扫描，
		// 所以只需补一步：建完页面调 `MiddleLayer.RegisterLanguage(page, LangForm)`，
		// 之后**切语言同样由 SwitchLanguage 自动完成**，这里不用再写换字逻辑。
		//
		// 代码里出现的 `T("键", "底稿")` 有两个用途：
		//   · 建控件/弹消息时取当前语言的文字（XML 是唯一来源）；
		//   · "底稿"是这个键第一次被写进 XML 时用的初值（作用等同 Designer 里写的 Text）。
		// ⇒ 要改文案或做翻译，直接编辑那三份 XML；底稿只在键还不存在时兜底。

		/// <summary>LanguageData 里属于本页的节点名（= 窗体名）。</summary>
		private const string LangForm = "VPForm";

		/// <summary>
		/// 把本页全部控件按名字补登记进语言表（见 <see cref="MiddleLayer.RegisterLanguage"/>）。
		/// 本页建页早于 <see cref="MiddleLayer.InitialLanguageData"/>（CreateForm → ModuleInitialize → BuildVpUi），
		/// 且控件是 UiLabel/UiButton（白名单精确类型扫不到的子类），所以列表建好后由
		/// InitialLanguageData 末尾回调这里补一次；之后切语言完全由 SwitchLanguage 自动完成。
		/// </summary>
		public void RegisterVpLanguage()
		{
			MiddleLayer.RegisterLanguage(this, LangForm);
		}

		/// <summary>按**键**取当前语言的文案（LanguageData\{当前语言}.xml）；XML 里没这个键就用底稿。</summary>
		private static string T(string key, string baseText)
		{
			return MiddleLayer.LangText(LangForm, key, baseText);
		}

		/// <summary>
		/// 文字里带**动态内容**（相机名 / VPP 名 / 配方路径 / 点数 / 实时状态）的控件用这个名字：
		/// 加后缀 `_dyn` 是给 <see cref="MiddleLayer.RegisterLanguage"/> 看的 —— 这类控件不能进语言表，
		/// 否则切语言时会被换成一条"写死的旧文字"，得由 <see cref="RefreshDynamicTexts"/> 现算。
		/// </summary>
		private static string DynTextName(string key)
		{
			return key + "_dyn";
		}

		/// <summary>
		/// 语言变化后补"文字里带动态内容"的那几处（静态文案已经被 SwitchLanguage 换掉了，见 RegisterLanguage）：
		/// 每个 VPP 页的归属行与 vpp 路径、标定卡的标题与路径、相机的实时按钮、工具条那句状态提示。
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
				// 补偿限制表的表头 / 行名也是动态文案（DataGridView 不进语言表）
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

		// ---- 下面这组工厂出来的控件都要"按名字进语言表"，所以名字就是语言键 ----
		//   · 名字 = key（稳定语义名，例如 vpBar_AddCamera），文案 = XML 里这个键的值；
		//   · baseText 只是 XML 里还没有这个键时补写用的底稿（和 Designer 里写的 Text 一个性质）；
		//   · 建好页之后调一次 MiddleLayer.RegisterLanguage(page, LangForm)，
		//     以后切语言由 SwitchLanguage 自动换（见 MiddleLayer.RegisterLanguage）。

		/// <summary>工具条按钮（名字 = 语言键）。</summary>
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
		//
		// 一台相机一份标定，**绑定整台相机**（不属于任何单个 VPP）：
		//   标定 ToolBlock = VisionData\{相机}\{Calibration.FolderName}\{配方}.vpp
		//   标定点        = 机器配置 VPForm.Cameras.xml（不随配方变）
		//   SetCalibration = 灌进本相机下所有 VPP 的 TB（工具名 Calibration.ToolName）
		//   显示           = 整台相机只有 entry.Display 一个，标定和所有 VPP 共用

		/// <summary>重建标定卡片。先把上一次建的那块（按控件类型认）清掉，不动宿主里原有的控件。</summary>
		private void RefreshCalibrationCard(CameraEntry entry)
		{
			if (entry == null || entry.CalibHost == null) return;

			// 旧卡片按**类型**认：Tag 现在存的是相机入口（VpCalibCard 要从 Tag 取它），不能再拿 Tag 当标记
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

			MiddleLayer.RegisterLanguage(card, LangForm);   // 标定卡是单独重建的，静态文案要就地补登记
		}

		/// <summary>
		/// 确保这台相机的**标定站**是活的。
		///
		/// 标定站不是常驻对象：配置文件里只存标定点与 vpp 目录名，站要按需建。
		/// 原来只有"添加标定"那一刻建过一次，所以**重启之后 `calib.Station` 一直是 null** ——
		/// 「编辑 TB / 拍照 / 添加标定点 / 应用到本相机」会全部静默失效
		/// （它们开头都有 `if (calib.Station == null) return;`）。
		/// 现在每次刷新标定卡片时补建一次（已经有了就什么都不做）。
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

			// 按当前配方读一次；文件不存在才会新建空 ToolBlock，已有标定不会被覆盖
			st.LoadTB(st.GetVppPath(SysPara.RecipeName));

			RegisterDisplay(st, entry.Display);
			calib.Station = st;
		}

		/// <summary>
		/// 标定卡片：**布局已移到 <see cref="VpCalibCard"/> 的设计器里**（VS 设计视图看到的就是 EXE 里的样子），
		/// 这里只负责按相机实例化、把数据挂上去。
		/// 相机入口（<c>CameraEntry</c> 是本类的私有嵌套类型）放进卡片的 Tag，标定配置给 Calib；
		/// 没建标定时卡片只显示"添加标定"（由 <see cref="VpCalibCard.Bind"/> 按 Calib==null 决定）。
		/// </summary>
		private VpCalibCard BuildCalibrationCard(CameraEntry entry)
		{
			VpCalibration calib = _config.GetCalibration(entry.Key);
			EnsureCalibrationStation(entry, calib);      // 重启 / 重建之后把标定站补回来

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
				card.Height = 296;      // 宿主里还挂着别的控件时（不用 Dock=Fill）退回固定高度
			}
			card.Bind();

			// 整台相机只有 entry.Display 一个显示：标定站与本相机所有 VPP 都画到它上面
			if (calib != null && calib.Station != null)
				RegisterDisplay(calib.Station, entry.Display);

			return card;
		}

		// ============ VpCalibCard（设计器定义的标定卡）↔ 本页 的接口 ============
		// 卡片只管布局、取数、把控件事件转过来；业务仍在本页，一行没动。
		// 相机入口统一从 card.Tag 取（CameraEntry 是本类的私有嵌套类型，卡片认不得它）。

		private CameraEntry EntryOf(VpCalibCard card)
		{
			return (card == null) ? null : (card.Tag as CameraEntry);
		}

		/// <summary>卡片表格改完一个单元格：写回标定点并立刻落盘（解析不了就把模型里的旧值写回格子）。</summary>
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
				// 解析不了就把模型里的旧值写回格子，别让 0 / 乱码进配置
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

		/// <summary>卡片曝光框的兜底值：该相机的默认曝光（没有就用 10）。</summary>
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

			// 标定曝光：**优先用卡片上"标定曝光"框里填的值**（标定还没建时那个值不落地，
			// 就等这一刻被采用）；框里读不到才退回相机的默认曝光。之后它与各 VPP 的曝光各管各的。
			double exp = 0;
			if (tbExp != null) double.TryParse((tbExp.Text ?? "").Trim(), out exp);
			if (exp <= 0 && entry.Config != null) exp = entry.Config.Exposure;
			calib.Exposure = exp;

			_config.Calibrations[entry.Key] = calib;

			// 建站 + 按当前配方读 vpp + 挂到"整台相机共用"的那个显示上。
			// 建站逻辑**只有 EnsureCalibrationStation 一份**，别再复制一遍 ——
			// 之前这里复制出来的那份漏了 Kind=Calibration，结果拍标定点永远取到 (0,0)。
			EnsureCalibrationStation(entry, calib);

			SaveVpConfig();
			RefreshCalibrationCard(entry);
		}

		private void EditCalibrationTB(CameraEntry entry)
		{
			VpCalibration calib = _config.GetCalibration(entry.Key);
			if (calib == null || calib.Station == null) return;
			if (!calib.Station.IsLoadTBOk) calib.Station.LoadTB(calib.Station.GetVppPath(SysPara.RecipeName));
			calib.Station.EditTB();
		}

		/// <summary>保存**标定专用曝光**（与各 VPP 的曝光互不影响）。</summary>
		/// <summary>标定曝光改一下就生效（只改内存）。标定还没建时不落地：值留在框里，点【添加标定】时才被采用。</summary>
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

		/// <summary>拍照：跑一次标定 ToolBlock，结果会画到本相机右侧那个共用显示控件上。</summary>
		private void SnapCalibration(CameraEntry entry)
		{
			VpCalibration calib = _config.GetCalibration(entry.Key);
			if (calib == null || calib.Station == null) return;
			calib.Station.RunTB();
		}

		/// <summary>
		/// 取当前像素坐标加一行（等价 legacy 的 Add_MPoint()）。
		/// 电机坐标 legacy 也是手工填的，这里给 0，让用户在表格里改。
		/// </summary>
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

		/// <summary>
		/// 把标定点灌进本相机下**所有 VPP**（等价 legacy 的 SetUpTheData()）。
		/// legacy 要求 ≥9 点，这里保持一致。
		/// </summary>
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

		// ---- 表格 ↔ 标定点 ----

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
