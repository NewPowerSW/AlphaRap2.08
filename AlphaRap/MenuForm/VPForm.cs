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
            UiTheme.ApplyDpi(this);
            if (UiTheme.IsDesignTime) return;

            // 移除设计器中的预览页；真实相机页按 VPForm.Cameras.xml 生成（见 BuildVpPages）
            RemoveDesignPreviewPage();

            // 固定视觉工位（供 TaskProcess/Gantry.cs 调用，如 MiddleLayer.VPF.H1_VFiducial.RunTB()）。
            // 画面只送主界面"视觉"页的相机格（见 BuildVpPages），老工位只出结果不出图。

            // 相机 / VPP 配置保存在独立的 VPForm.Cameras.xml（见 VpConfigStore），界面在 ModuleInitialize 后构建（见 BuildVpUi）
            // 语言切换时刷新含动态内容的文字（相机名 / 路径 / 点数 / 实时状态 / 工具条提示）
            MiddleLayer.LanguageChanged += OnLanguageChanged;
            Disposed += delegate { MiddleLayer.LanguageChanged -= OnLanguageChanged; };
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
        private string _visionDataError;

        /// <summary>把配方表 tb_H1_Visdata 刷进 <see cref="dgv_H1_VisionData_List"/>。</summary>
        public void Getdgv_H1_VisionDataList()
        {
            string error;
            TryLoadVisionCriteria(out error);
        }

        /// <summary>校验并整体替换配方视觉判据；失败时清空快照，防止沿用前一配方。</summary>
        public bool TryLoadVisionCriteria(out string error)
        {
            try
            {
                DataTable table = RecipeData.Tables["tb_H1_Visdata"];
                if (table == null || table.Rows.Count == 0) throw new InvalidOperationException(MiddleLayer.LangMsg("VPForm", "vpError_NoCriteria", "配方缺少视觉判据 tb_H1_Visdata。", "Recipe is missing vision criteria tb_H1_Visdata.", "La receta no contiene los criterios de visión tb_H1_Visdata."));
                List<VisionPostData> values = new List<VisionPostData>();
                for (int i = 0; i < table.Rows.Count; i++) values.Add(ReadVisionCriterion(table.Rows[i], i));
                dgv_H1_VisionData_List = values;
                _visionDataError = error = "";
                return true;
            }
            catch (Exception ex)
            {
                dgv_H1_VisionData_List = new List<VisionPostData>();
                _visionDataError = error = ex.Message;
                return false;
            }
        }

        /// <summary>按列名读取一行生产判据，启用时上下限必须有效。</summary>
        private static VisionPostData ReadVisionCriterion(DataRow row, int index)
        {
            VisionPostData data = new VisionPostData { Point = index, Enable = Convert.ToBoolean(row["Enable"]) };
            if (!data.Enable) return data;
            data.X = ReadVisionNumber(row, "X");
            data.Y = ReadVisionNumber(row, "Y");
            data.R = ReadVisionNumber(row, "U");
            data.X_Low = ReadVisionNumber(row, "X_Low");
            data.X_Hi = ReadVisionNumber(row, "X_Hi");
            data.Y_Low = ReadVisionNumber(row, "Y_Low");
            data.Y_Hi = ReadVisionNumber(row, "Y_Hi");
            data.R_Low = ReadVisionNumber(row, "U_Low");
            data.R_Hi = ReadVisionNumber(row, "U_Hi");
            if (data.X_Low >= data.X_Hi || data.Y_Low >= data.Y_Hi || data.R_Low >= data.R_Hi)
                throw new InvalidOperationException(MiddleLayer.LangMsg("VPForm", "vpError_RowA", "第 ", "Row ", "Fila ") + (index + 1) + MiddleLayer.LangMsg("VPForm", "vpError_RowB", " 行视觉判据下限必须小于上限。", ": vision lower limit must be below the upper limit.", ": el límite inferior de visión debe ser menor que el superior."));
            return data;
        }

        /// <summary>读取配方中的有限数值，空值和非数值禁止进入生产判定。</summary>
        private static double ReadVisionNumber(DataRow row, string column)
        {
            double value = Convert.ToDouble(row[column]);
            if (double.IsNaN(value) || double.IsInfinity(value)) throw new InvalidOperationException(MiddleLayer.LangMsg("VPForm", "vpError_CriteriaFinite", "视觉判据不是有限数值：", "Vision criterion is not finite: ", "El criterio de visión no es un número finito: ") + column);
            return value;
        }

        /// <summary>动态定位由本站补偿限制判定；内置兼容工位读取配方判据快照。</summary>
        public bool TryGetProductionVisionData(out VisionPostData data, out string error)
        {
            data = new VisionPostData();
            if (!string.IsNullOrEmpty(_config.ProductionVppId)) { error = ""; return true; }
            error = _visionDataError;
            List<VisionPostData> values = dgv_H1_VisionData_List;
            if (values == null || values.Count == 0)
            {
                if (string.IsNullOrEmpty(error)) error = MiddleLayer.LangMsg("VPForm", "vpError_CriteriaNotLoaded", "生产视觉判据尚未加载。", "Production vision criteria are not loaded.", "Los criterios de visión de producción no están cargados.");
                return false;
            }
            data = values[0];
            error = "";
            return true;
        }

        /// <summary>检查生产工位、判据及调试采集是否就绪，供启动自动流程前调用。</summary>
        public bool CanStartProduction(out string error)
        {
            VisionPostData data;
            if (!TryGetProductionVisionData(out data, out error)) return false;
            VpStation production;
            if (!TryGetProductionStation(out production, out error)) return false;
            foreach (VisionproInterface station in VisionproInterface.VList)
            {
                if (!station.IsBusy) continue;
                error = MiddleLayer.LangMsg("VPForm", "vpError_StationBusy", "视觉工位尚未结束运行或编辑：", "Vision station is still running or being edited: ", "La estación de visión sigue ejecutándose o en edición: ") + station.StationName;
                return false;
            }
            foreach (CameraEntry entry in _entries)
            {
                if (!IsLiveRunning(entry)) continue;
                error = MiddleLayer.LangMsg("VPForm", "vpError_StopLive", "请先停止相机实时显示：", "Stop camera live view first: ", "Detenga primero la vista en directo de la cámara: ") + entry.Key;
                return false;
            }
            error = "";
            return true;
        }

        /// <summary>按持久化绑定解析当前配方的生产工位，配置加载错误时禁止生产。</summary>
        public bool TryGetProductionStation(out VpStation station, out string error)
        {
            station = null;
            error = _vpLoadError;
            if (!string.IsNullOrEmpty(error)) return false;
            return VpProductionBinding.TryResolve(_config, H1_VFiducial, SysPara.RecipeName, out station, out error);
        }

        /// <summary>本 VPP 是否被明确选为生产定位算法。</summary>
        internal bool IsProductionVpp(VpVppConfig vpp)
        {
            return vpp != null && _config.ProductionVppId == vpp.Id;
        }

        /// <summary>保存生产绑定；校验或写盘失败时恢复原绑定与原工位类型。</summary>
        internal void SetProductionVpp(VpVppConfig vpp, bool enabled)
        {
            if (vpp == null || vpp.Station == null) return;
            if (!EnsureIdle(MiddleLayer.LangMsg(LangForm, "vpAct_Production", "修改生产定位绑定", "change the production positioning binding", "cambiar la vinculación de posicionamiento de producción"))) return;
            if (!enabled && !IsProductionVpp(vpp)) return;
            string previousId = _config.ProductionVppId;
            VpStationKind previousKind = vpp.Station.Kind;
            string error;
            if (!vpp.Station.TrySetKind(enabled ? VpStationKind.Fiducial : VpStationKind.Custom, out error))
            { MessageBox.Show(error, T("vpMsg_Title", "提示")); return; }
            _config.ProductionVppId = enabled ? vpp.Id : "";
            VpStation station;
            bool valid = !enabled || TryGetProductionStation(out station, out error);
            if (valid && TrySaveVpConfig(out error))
            {
                VpCameraConfig oldCamera;
                VpVppConfig oldVpp = _config.FindVpp(previousId, out oldCamera);
                if (oldVpp != null && oldVpp != vpp && oldVpp.Station != null) oldVpp.Station.TrySetKind(VpStationKind.Custom, out error);
                OperationLog.Write("VPForm", "生产定位绑定：" + (enabled ? LogTag(vpp) : "内置工位"));
                foreach (VpCameraConfig camera in _cameras) RefreshVppInfoTexts(camera);
                return;
            }
            _config.ProductionVppId = previousId;
            string ignored;
            vpp.Station.TrySetKind(previousKind, out ignored);
            MessageBox.Show(error, T("vpMsg_Title", "提示"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

		/// <summary>相机 SN 下拉框右侧箭头的点击区宽度（像素）：落在这里才算"展开列表"，落在文本区是手输 SN。</summary>
		private const int SnArrowClickWidth = 24;

		private Panel _vpBar;
		private bool _vpUiBuilt;
		/// <summary>建页失败的原因（非空时显示在工具条上）。</summary>
		private string _vpBuildError;
		/// <summary>读取 VPForm.Cameras.xml 失败的原因（非空时显示在工具条上，见 <see cref="UpdateVpHint"/>）。</summary>
		private string _vpLoadError;
		/// <summary>正在把配置回填到控件上：期间忽略 TextChanged / SelectedIndexChanged，避免把刚读到的值又写回去。</summary>
		private bool _fillingCameraUi;
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

		/// <summary>工具条右侧的提示标签：建页 / 读配置 / 存配置出错时以红字显示原因，平时为空。</summary>
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

			if (!string.IsNullOrEmpty(_vpLoadError))
			{
				lbl.ForeColor = Color.FromArgb(200, 60, 60);
				lbl.Text = T("vpHint_LoadError", "视觉配置未完全加载：") + _vpLoadError;
				return;
			}

			if (!string.IsNullOrEmpty(VpConfigStore.LastSaveError))
			{
				lbl.ForeColor = Color.FromArgb(200, 60, 60);
				lbl.Text = T("vpHint_SaveError", "视觉配置保存失败：") + VpConfigStore.LastSaveError;
				return;
			}

			// 扫描过但一台都没枚举到：告诉操作员下拉框为什么是空的
			List<CogCameraDevice> scanned = CogCameraDevices.Cached;
			if (scanned != null && scanned.Count == 0)
			{
				lbl.ForeColor = Color.FromArgb(176, 120, 32);
				lbl.Text = T("vpHint_NoCamera", "没有检测到相机：SN 下拉框为空。点一下 SN 下拉框右侧箭头可重新扫描；还不行请查相机电源和网线。");
				return;
			}

			// 枚举中途出错只拿到一部分设备时，把原因显示出来，别让人以为机器上就只有这么多相机
			string scanError = CogCameraDevices.LastError;
			if (!string.IsNullOrEmpty(scanError))
			{
				lbl.ForeColor = Color.FromArgb(176, 120, 32);
				lbl.Text = T("vpHint_ScanError", "相机枚举不完整：") + scanError;
				return;
			}

			// 配置里还有相机没选 SN：不选就没法取流
			VpCameraConfig unbound = FirstCameraWithoutSN();
			if (unbound != null)
			{
				lbl.ForeColor = Color.FromArgb(176, 120, 32);
				lbl.Text = T("vpHint_NeedSN", "请在 SN 下拉框里选择相机：") + unbound.Name;
				return;
			}

			lbl.ForeColor = Color.FromArgb(110, 120, 132);
			lbl.Text = "";
		}

		/// <summary>第一台还没选 SN 的相机；全都选好了返回 null。</summary>
		private VpCameraConfig FirstCameraWithoutSN()
		{
			for (int i = 0; i < _cameras.Count; i++)
				if (_cameras[i] != null && string.IsNullOrEmpty(_cameras[i].CameraSN)) return _cameras[i];
			return null;
		}

		private void BuildVpUi()
		{
			if (_vpUiBuilt) return;
			_vpUiBuilt = true;

			CreateVpToolBar();
			LoadConfigFromDisk();
			ScanCamerasAtStartup();
			BuildVpPages();
			UpdateVpHint();      // 读取失败 / 保存失败的原因立刻显示在工具条右侧
		}

		/// <summary>
		/// 启动时把整机相机枚举一次，之后各相机页的 SN 下拉框直接用这份结果，
		/// 不用等到点开下拉框才去扫驱动。枚举失败不影响界面，只记录原因。
		/// </summary>
		private void ScanCamerasAtStartup()
		{
			// 第一次枚举要初始化采集卡驱动（没有相机也要几秒），先把提示画出来，别让界面看着像卡死
			ShowScanningHint();

			List<CogCameraDevice> devices = CogCameraDevices.Enumerate(true);

			string error = CogCameraDevices.LastError;
			OperationLog.Write("VPForm", "启动扫描相机：" + (devices == null ? 0 : devices.Count) + " 台"
				+ (string.IsNullOrEmpty(error) ? "" : "（枚举失败：" + error + "）"));

			// 仍然没有 SN（一台都没枚举到）的相机先把名字记进日志，界面上也会提示
			for (int i = 0; i < _cameras.Count; i++)
			{
				VpCameraConfig cam = _cameras[i];
				if (cam == null || !string.IsNullOrEmpty(cam.CameraSN)) continue;
				OperationLog.Write("VPForm", "相机 " + cam.Name + " 还没有选择 SN");
			}
		}

		/// <summary>扫描相机前先把"正在扫描"画到工具条上（同步重绘，不依赖消息循环）。</summary>
		private void ShowScanningHint()
		{
			UiLabel hint = FindBarHint();
			if (hint == null) return;

			hint.ForeColor = Color.FromArgb(110, 120, 132);
			hint.Text = T("vpHint_ScanningCamera", "正在扫描相机…");
			hint.Refresh();
		}

		/// <summary>把 VPForm.Cameras.xml 读进 <see cref="_config"/>（读取失败时记录原因并在工具条上提示）。</summary>
		private void LoadConfigFromDisk()
		{
			string error;
			VpConfigFile loaded = VpConfigStore.Load(out error);
			_vpLoadError = error;

			_config.Cameras.Clear();
			_config.Cameras.AddRange(loaded.Cameras);
			_config.ProductionVppId = loaded.ProductionVppId;
			_config.Calibrations.Clear();
			foreach (KeyValuePair<string, VpCalibration> kv in loaded.Calibrations)
				_config.Calibrations[kv.Key] = kv.Value;
			// 隐藏页名单也要带过来，否则下一次保存会把磁盘上的 HiddenPages 清空
			_config.HiddenPages.Clear();
			_config.HiddenPages.AddRange(loaded.HiddenPages);

			OperationLog.Write("VPForm", "读取视觉配置：" + VpConfigStore.FilePath
				+ "，" + _config.Cameras.Count + " 台相机"
				+ (string.IsNullOrEmpty(error) ? "" : "（失败：" + error + "）"));

			System.Diagnostics.Debug.WriteLine("[VPForm] LoadConfigFromDisk → "
				+ _config.Cameras.Count + " 台相机" + (string.IsNullOrEmpty(error) ? "" : "；" + error));
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
			while (FindCamera(name) != null || _cameras.Exists(item => string.Equals(item.StorageFolder, name, StringComparison.OrdinalIgnoreCase)))
			{ n++; name = "Camera" + n; }

			VpCameraConfig cam = new VpCameraConfig();
			cam.Name = name;
			cam.FolderName = name;
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
			page.Tag = cam;
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

			// 属性条按钮。「实时显示」为相机级：取流使用本相机 SN 解析出的通道 + 当前选中 VPP 的曝光；
			// 文字随语言和实时状态变化，不进语言表（名字带 _dyn）。
			ctl.LiveButton.Click += delegate
			{
				LiveCamera(entry);
				RefreshLiveButton(ctl.LiveButton, entry);
			};
			entry.LiveButton = ctl.LiveButton;
			RefreshLiveButton(ctl.LiveButton, entry);
			ctl.DeleteCameraButton.Click += delegate { DeleteCamera(cam); };

			// 参数和显示名称在失焦或回车时提交，存储目录保持不变。
			FillCameraEditors(cam, ctl);
			AttachCameraEditors(cam, page, ctl);

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
			// （MainF 尚未建好时跳过，否则一台相机都建不出来）
			try { AddDisplay(st, MiddleLayer.MainF != null ? MiddleLayer.MainF.GetCameraDisplay(st != null ? st.CameraName : null) : null); }
			catch (Exception) { }
		}

		private static void AddDisplay(VpStation st, Cognex.VisionPro.CogRecordDisplay disp)
		{
			if (st == null || disp == null) return;
			st.BindDisplay(disp);
		}

		// ==================== 参数编辑 ====================
		// 曝光（默认曝光 / VPP 曝光 / 标定曝光）在失焦或回车时生效；
		// 显示名称在失焦或回车时生效；改名只更新归属与标定键。
		// 建页时必须先把配置回填进控件（FillCameraEditors / AttachVppEditors / VpCalibCard.Bind），
		// 否则界面上是一片空白，看起来就是"参数没加载"。

		/// <summary>把相机配置回填到属性条：名称 / 相机 SN / 默认曝光。</summary>
		private void FillCameraEditors(VpCameraConfig cam, VpCameraPage ctl)
		{
			if (cam == null || ctl == null) return;

			bool old = _fillingCameraUi;
			_fillingCameraUi = true;
			try
			{
				ctl.NameBox.Text = cam.Name ?? "";
				ctl.ExposureBox.Text = cam.Exposure.ToString("F1");
			}
			finally { _fillingCameraUi = old; }

			FillCameraSNList(cam, ctl, false);
		}

		/// <summary>
		/// 把当前可用的**全部相机 SN** 列进下拉框供选择，并选中配置里那台。
		/// <paramref name="refresh"/> = true 时重新枚举（带缓存节流），false 时直接用启动时扫到的结果。
		/// </summary>
		private void FillCameraSNList(VpCameraConfig cam, VpCameraPage ctl, bool refresh)
		{
			if (cam == null || ctl == null || ctl.SNBox == null) return;

			bool old = _fillingCameraUi;
			_fillingCameraUi = true;
			try
			{
				List<CogCameraDevice> devices = refresh ? CogCameraDevices.Enumerate() : CogCameraDevices.Cached;
				ctl.SetSNItems(devices);

				if (devices == null)
				{
					// 还没扫描过：只把配置里的 SN 显示出来
					ctl.ShowSNText(cam.CameraSN);
					return;
				}

				CogCameraDevice picked = CogCameraDevices.Find(devices, cam.CameraSN);
				if (picked != null)
				{
					ctl.SelectSNDevice(picked);
					return;
				}

				// 没选 SN，或选的这台当前不在机器上：照样把 SN 显示出来，方便发现"相机没接 / SN 变了"
				ctl.ShowSNText(cam.CameraSN);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("[VPForm] FillCameraSNList → " + ex);
			}
			finally
			{
				_fillingCameraUi = old;
			}
		}

		/// <summary>
		/// 展开下拉框**之前**调用（点箭头 / 按 F4）：重新枚举一次并刷新列表，插拔相机后不用重启。
		/// 不能放在 DropDown 事件里：下拉窗口正在弹出时改动列表项会让 ComboBox 崩，没相机时点几次就崩。
		/// </summary>
		private void RefreshCameraSNList(VpCameraConfig cam, VpCameraPage ctl)
		{
			if (cam == null || ctl == null) return;

			FillCameraSNList(cam, ctl, true);
			UpdateVpHint();
		}

		private void AttachCameraEditors(VpCameraConfig cam, TabPage page, VpCameraPage ctl)
		{
			if (cam == null || ctl == null) return;

			TextBox tbName = ctl.NameBox;
			TextBox tbExposure = ctl.ExposureBox;

			// 默认曝光改为失焦 / 回车时提交：避免每敲一个字符就写一次配置文件
			tbExposure.Leave += delegate { CommitCameraExposure(cam, ctl); };
			tbExposure.KeyDown += delegate (object s, KeyEventArgs e)
			{
				if (e.KeyCode != Keys.Enter) return;
				e.SuppressKeyPress = true;
				CommitCameraExposure(cam, ctl);
			};

			// 相机 SN：点箭头 / 按 F4 时先把列表准备好（这时下拉窗口还没弹出，可以安全地重建列表项），
			// 选中或手输后立即绑定。不做 DropDown 事件：弹出过程中改列表项会让 ComboBox 崩。
			ComboBox boxSN = ctl.SNBox;
			if (boxSN != null)
			{
				boxSN.MouseDown += delegate (object s, MouseEventArgs e)
				{
					if (e.Button != MouseButtons.Left) return;
					// 点右侧箭头才是"展开列表"；点文本区是手输 SN，不能把用户正在输入的内容冲掉
					if (e.X < boxSN.ClientSize.Width - SnArrowClickWidth) return;
					RefreshCameraSNList(cam, ctl);
				};
				boxSN.SelectedIndexChanged += delegate { ApplyCameraSN(cam, ctl); };
				boxSN.Leave += delegate { ApplyCameraSN(cam, ctl); };
				boxSN.KeyDown += delegate (object s, KeyEventArgs e)
				{
					// F4 / Alt+↓ 是展开下拉框，展开前先刷新列表
					if (e.KeyCode == Keys.F4 || (e.Alt && e.KeyCode == Keys.Down))
					{
						RefreshCameraSNList(cam, ctl);
						return;
					}
					if (e.KeyCode != Keys.Enter) return;
					e.SuppressKeyPress = true;
					ApplyCameraSN(cam, ctl);
				};
			}

			tbName.Leave += delegate { ApplyCameraRename(cam, tbName, page); };
			tbName.KeyDown += delegate (object s, KeyEventArgs e)
			{
				if (e.KeyCode != Keys.Enter) return;
				e.SuppressKeyPress = true;
				ApplyCameraRename(cam, tbName, page);
			};
		}

		/// <summary>
		/// 相机 SN 变更（下拉选中或手输 SN）：把 SN 写进配置，并同步给本相机下所有工位（含标定站）。
		/// 采集通道由 SN 现场解析，不再保存索引。
		/// </summary>
		private void ApplyCameraSN(VpCameraConfig cam, VpCameraPage ctl)
		{
			if (cam == null || ctl == null || ctl.SNBox == null) return;
			if (_fillingCameraUi) return;

			CogCameraDevice dev = ctl.SNBox.SelectedItem as CogCameraDevice;
			string typed = (ctl.SNBox.Text ?? "").Trim();
			// 文本框是准：选中项的文字与输入框一致时用它的绑定键（没有序列号的相机用 UniqueID），
			// 不一致（例如把输入框清空了）就按输入的文字走，避免残留的选中项挡住"解除绑定"
			string newSN = (dev != null && typed == dev.ToString()) ? dev.BindKey : typed;
			if (newSN == cam.CameraSN) return;

			// 换绑相机等于换采集源，设备运行中不允许改
			if (!EnsureIdle(T("vpAct_BindCamera", "修改相机绑定")))
			{
				FillCameraSNList(cam, ctl, false);
				return;
			}

			cam.CameraSN = newSN;
			ApplyCameraChannel(cam);
			RefreshVppInfoTexts(cam);
			RefreshCalibrationTexts(FindEntry(cam.Name));

			string error;
			int channel = cam.ResolveCcdIndex(out error);
			// 手输的 SN 在机器上找不到才弹警告；清空 SN 属于"解除绑定"，交给工具条提示
			if (!string.IsNullOrEmpty(newSN) && channel < 0 && !string.IsNullOrEmpty(error))
				ShowCameraBindWarning(error);

			OperationLog.Write("VPForm", "绑定相机：" + cam.Name + " → SN " + cam.CameraSN
				+ (channel >= 0 ? "（采集通道 " + channel + "）" : "（当前机器上没有这台相机）"));
			SaveVpConfig();
			UpdateVpHint();
		}

		/// <summary>提示"SN 已保存但当前机器上找不到这台相机"，相机接上后会按 SN 自动找到。</summary>
		private void ShowCameraBindWarning(string error)
		{
			MessageBox.Show(error + MiddleLayer.LangMsg("VPForm", "vpMsg_SNSaved", "\r\n（SN 已保存；相机接上后会自动按 SN 找到它）", "\r\n(SN saved; the camera will be found by SN when connected.)", "\r\n(SN guardado; la cámara se localizará por SN al conectarse.)"),
				T("vpMsg_Title", "提示"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		/// <summary>把相机的 SN 同步给本相机下所有视觉站（含标定站），采集通道由 SN 现场解析一次。</summary>
		private void ApplyCameraChannel(VpCameraConfig cam)
		{
			if (cam == null) return;

			string error;
			int channel = cam.ResolveCcdIndex(out error);   // 解析不到时为 -1，取流时会给出提示

			for (int i = 0; i < cam.Vpps.Count; i++)
			{
				VpStation st = (cam.Vpps[i] != null) ? cam.Vpps[i].Station : null;
				if (st == null) continue;
				st.RunLiveCCDSN = cam.CameraSN;
				st.RunLiveCCDIndex = channel;
			}

			VpCalibration cal = _config.GetCalibration(cam.Name);
			if (cal != null && cal.Station != null)
			{
				cal.Station.RunLiveCCDSN = cam.CameraSN;
				cal.Station.RunLiveCCDIndex = channel;
			}
		}

		/// <summary>失焦 / 回车时提交默认曝光，非法输入还原为配置里的值。</summary>
		private void CommitCameraExposure(VpCameraConfig cam, VpCameraPage ctl)
		{
			if (cam == null || ctl == null) return;
			if (_fillingCameraUi) return;

			ApplyCameraDefaultExposure(cam, ctl);
			SyncCameraExposure(cam, ctl);
		}

		/// <summary>应用默认曝光：只影响新建 VPP 与新建标定，不改变正在运行的工位。</summary>
		private void ApplyCameraDefaultExposure(VpCameraConfig cam, VpCameraPage ctl)
		{
			double exposure;
			if (!VpConfigStore.TryParseInputDouble(ctl.ExposureBox.Text, out exposure) || exposure <= 0) return;
			if (exposure == cam.Exposure) return;

			cam.Exposure = exposure;

			OperationLog.Write("VPForm", "修改默认曝光：" + cam.Name + " → " + exposure.ToString("F1"));
			SaveVpConfig();
		}

		/// <summary>把索引框与曝光框恢复成配置里的值（不触发编辑事件）。</summary>
		private void SyncCameraExposure(VpCameraConfig cam, VpCameraPage ctl)
		{
			bool old = _fillingCameraUi;
			_fillingCameraUi = true;
			try
			{
				ctl.ExposureBox.Text = cam.Exposure.ToString("F1");
			}
			finally { _fillingCameraUi = old; }
		}

		/// <summary>
		/// 校验相机名 / VPP 名能否用作目录名：空、过长、首尾空格、非法字符、系统保留名一律拒绝。
		/// 合法返回 true；不合法返回 false 并把提示文字写进 reason。
		/// </summary>
		private static bool IsValidNodeName(string name, out string reason)
		{
			reason = "";

			if (string.IsNullOrEmpty(name))
			{
				reason = T("vpMsg_NameEmpty", "名称不能为空！");
				return false;
			}
			if (name.Length > 64)
			{
				reason = T("vpMsg_NameTooLong", "名称太长，最多 64 个字符。");
				return false;
			}
			if (name != name.Trim())
			{
				reason = T("vpMsg_NameSpace", "名称首尾不能有空格。");
				return false;
			}
			if (name == "." || name == "..")
			{
				reason = T("vpMsg_NameDot", "名称不能是「.」或「..」。");
				return false;
			}

			char bad = FindInvalidNameChar(name);
			if (bad != '\0')
			{
				reason = T("vpMsg_NameBadCharA", "名称不能包含字符：") + bad;
				return false;
			}
			if (IsReservedName(name))
			{
				reason = T("vpMsg_NameReservedA", "「") + name
					+ T("vpMsg_NameReservedB", "」是系统保留名，请换一个。");
				return false;
			}
			return true;
		}

		/// <summary>返回名称里第一个不能出现在文件名中的字符；没有则返回 \0。</summary>
		private static char FindInvalidNameChar(string name)
		{
			char[] invalid = System.IO.Path.GetInvalidFileNameChars();
			for (int i = 0; i < name.Length; i++)
				if (Array.IndexOf(invalid, name[i]) >= 0) return name[i];
			return '\0';
		}

		/// <summary>判断是否为 DOS 设备保留名（CON / PRN / AUX / NUL / COM1~9 / LPT1~9），这些名字不能做目录名。</summary>
		private static bool IsReservedName(string name)
		{
			string upper = name.ToUpperInvariant();
			if (upper == "CON" || upper == "PRN" || upper == "AUX" || upper == "NUL") return true;

			if (upper.Length != 4) return false;
			if (upper.StartsWith("COM") || upper.StartsWith("LPT"))
			{
				char digit = upper[3];
				return digit >= '1' && digit <= '9';
			}
			return false;
		}

		/// <summary>修改相机显示名称并保留存储目录；名称不合法时还原输入框。</summary>
		private void ApplyCameraRename(VpCameraConfig cam, TextBox tbName, TabPage page)
		{
			if (cam == null || tbName == null) return;

			string newName = (tbName.Text ?? "").Trim();
			if (newName == cam.Name) return;
			if (!EnsureIdle(T("vpAct_RenameCamera", "修改相机名称"))) { tbName.Text = cam.Name; return; }

			string nameReason;
			if (!IsValidNodeName(newName, out nameReason))
			{
				MessageBox.Show(nameReason, T("vpMsg_Title", "提示"));
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

			// 只更新显示归属，存储目录在首次创建时确定。
			for (int i = 0; i < cam.Vpps.Count; i++)
				if (cam.Vpps[i].Station != null) cam.Vpps[i].Station.SetOwner(newName, cam.Vpps[i].Name,
					System.IO.Path.Combine(cam.StorageFolder, cam.Vpps[i].StorageFolder));

			cam.Name = newName;
			page.Name = DynCameraPrefix + newName;
			page.Text = newName;

			// 标定以相机名为键（VpConfigFile.Calibrations），改名时同步更新键和 entry.Key
			VpCalibration calOld = _config.GetCalibration(oldName);
			if (calOld != null)
			{
				_config.Calibrations.Remove(oldName);
				_config.Calibrations[newName] = calOld;
				if (calOld.Station != null) calOld.Station.SetOwner(newName, calOld.FolderName,
					System.IO.Path.Combine(cam.StorageFolder, calOld.FolderName));
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
		/// 刷新本相机下各 VPP 页顶部的归属行与路径行（相机名 / SN / VPP 名 / vpp 路径）。
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
					if (e.Page != null)
						foreach (Control control in e.Page.Controls)
							if (control is VpCameraPage) ((VpCameraPage)control).RefreshTexts();

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

		/// <summary>标定卡标题（随语言、已录点数和"是否已应用到本相机"变化）。</summary>
		private static string BuildCalibTitleText(CameraEntry entry, VpCalibration calib)
		{
			string head = T("vpCalib_Title", "标定");
			string tail = (calib == null)
				? T("vpCalib_NotAdded", "　　尚未添加标定")
				: T("vpCalib_PointsA", "　　已录 ") + calib.Points.Count
				  + T("vpCalib_PointsB", " 点（至少 9 点）");

			if (calib != null && !calib.IsAppliedTo(entry.Config, SysPara.RecipeName))
				tail += T("vpCalib_Dirty", "　　▲ 已修改，未应用");

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
			if (cam.Vpps.Exists(IsProductionVpp))
			{ MessageBox.Show(MiddleLayer.LangMsg(LangForm, "vpMsg_UnbindCamera", "请先解除或更换此相机的生产定位绑定。", "Remove or change this camera's production binding first.", "Primero quite o cambie la vinculación de producción de esta cámara."), T("vpMsg_Title", "提示")); return; }

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
			while (FindVpp(cam, name) != null || cam.Vpps.Exists(item => string.Equals(item.StorageFolder, name, StringComparison.OrdinalIgnoreCase)))
			{ n++; name = "VPP" + n; }

			VpVppConfig vpp = new VpVppConfig();
			vpp.Name = name;
			vpp.FolderName = name;
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
			ctl.CompGrid.Tag = vpp;     // RefreshDynamicTexts 靠这个 Tag 找回对应的 VPP
			_compGrids.Add(ctl.CompGrid);

			return page;
		}

		internal string BuildVppInfoText(VpCameraConfig cam, VpVppConfig vpp)
		{
			// 绑定信息：只显示 SN；没选 SN 时直接说明
			string bind = string.IsNullOrEmpty(cam.CameraSN)
				? T("vpInfo_NoSN", " (未选择 SN)")
				: " (SN " + cam.CameraSN + ")";

			return T("vpInfo_Camera", "相机：") + cam.Name
				 + bind
				 + T("vpInfo_Vpp", "　　VPP：") + (vpp != null ? vpp.Name : "")
				 + (IsProductionVpp(vpp) ? MiddleLayer.LangMsg(LangForm, "vpInfo_Production",
					"　　生产定位", "    Production positioning", "    Posicionamiento de producción") : "");
		}

		/// <summary>VPP 页与标定卡共用的 vpp 路径文字（按当前配方生成）。</summary>
		internal static string BuildVppPathText(VpVppConfig vpp)
		{
			if (vpp == null || vpp.Station == null)
				return T("vpPath_NotLoaded", "vpp：(未加载)");
			return T("vpPath_Prefix", "vpp：") + vpp.Station.GetVppPath(SysPara.RecipeName);
		}

		/// <summary>
		/// 填充补偿配置表：每行包含项目、补偿值和上下限，行对象存于 <see cref="DataGridViewRow.Tag"/>。
		/// 表头取自语言键 vpCompCol_*，切换语言时由 <see cref="RefreshDynamicTexts"/> 重填。
		/// </summary>
		internal void FillCompLimitGrid(DataGridView grid, VpVppConfig vpp)
		{
			if (grid == null || vpp == null) return;
			RefreshCompLimitHeaders(grid);

			grid.Rows.Clear();
			for (int i = 0; i < vpp.CompLimits.Count; i++)
			{
				VpCompLimitItem it = vpp.CompLimits[i];
				if (it == null) continue;
				int idx = grid.Rows.Add(it.Item, it.Compensation, it.Min, it.Max);
				grid.Rows[idx].Tag = it;    // 行与配置对象一一对应
			}
		}

		/// <summary>刷新补偿表头及项目显示，语言切换不重建行、不改写业务键和未提交的单元格。</summary>
		internal void RefreshCompLimitHeaders(DataGridView grid)
		{
			if (grid == null || grid.Columns.Count < 4) return;
			grid.Columns["cItem"].HeaderText = MiddleLayer.LangMsg(LangForm, "vpCompCol_Item", "项目", "Item", "Elemento");
			grid.Columns["cCompensation"].HeaderText = MiddleLayer.LangMsg(LangForm, "vpCompGrid_cCompensation", "补偿值", "Compensation", "Compensación");
			grid.Columns["cMin"].HeaderText = MiddleLayer.LangMsg(LangForm, "vpCompCol_Min", "补偿下限", "Lower limit", "Límite inferior");
			grid.Columns["cMax"].HeaderText = MiddleLayer.LangMsg(LangForm, "vpCompCol_Max", "补偿上限", "Upper limit", "Límite superior");
			grid.InvalidateColumn(grid.Columns["cItem"].Index);
		}

		/// <summary>
		/// 【添加行】：在表尾追加一行（默认名"补偿N"，跳过重名）并立即保存，然后进入名称单元格编辑。
		/// 名称为空的行不会保存（<see cref="VpConfigStore.Save"/> 会跳过）。
		/// </summary>
		internal void AddCompLimitRow(DataGridView grid, VpVppConfig vpp)
		{
			if (grid == null || vpp == null) return;
			if (!EnsureIdle(T("vpAct_Comp", "修改补偿限制"))) return;

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
			if (!EnsureIdle(T("vpAct_Comp", "修改补偿限制"))) return;

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
		/// 只提交当前编辑的单元格并立即保存，保留其他列的原始精度；非法输入还原，下限不得高于上限。
		/// </summary>
		internal void SyncCompLimitFromGrid(DataGridView grid, VpVppConfig vpp, int rowIndex, int columnIndex)
		{
			if (grid == null || vpp == null || rowIndex < 0 || rowIndex >= grid.Rows.Count) return;
			if (columnIndex < 0 || columnIndex >= grid.Columns.Count) return;
			if (!EnsureIdle(T("vpAct_Comp", "修改补偿限制"))) { FillCompLimitGrid(grid, vpp); return; }

			DataGridViewRow row = grid.Rows[rowIndex];
			VpCompLimitItem it = row.Tag as VpCompLimitItem;
			if (it == null) return;

			DataGridViewCell cell = row.Cells[columnIndex];
			string column = cell.OwningColumn.Name;
			string text = Convert.ToString(cell.Value).Trim();
			if (column == "cItem")
			{
				if (text.Length == 0) { cell.Value = it.Item; return; }
				if (text == it.Item) return;
				it.Item = text;
			}
			else
			{
				double previous;
				switch (column)
				{
					case "cCompensation": previous = it.Compensation; break;
					case "cMin": previous = it.Min; break;
					case "cMax": previous = it.Max; break;
					default: return;
				}

				double value;
				if (!VpConfigStore.TryParseInputDouble(text, out value)) { cell.Value = previous; return; }
				if (value == previous) return;
				switch (column)
				{
					case "cCompensation": it.Compensation = value; break;
					case "cMin": it.Min = value; break;
					case "cMax": it.Max = value; break;
				}
				cell.Value = value;
				if (it.Min > it.Max)
				{
					it.Min = it.Max;
					row.Cells["cMin"].Value = it.Min;
				}
			}

			OperationLog.Write("VPForm", "补偿配置：修改「" + it.Item + "」，补偿值=" + it.Compensation.ToString("R") + "，限制=["
				+ it.Min.ToString("F3") + ", " + it.Max.ToString("F3") + "]（" + LogTag(vpp) + "）");
			SaveVpConfig();
		}

		/// <summary>创建视觉站（构造时自动登记到 VisionproInterface.VList，OpenVision 时按配方加载 vpp）。</summary>
		private void CreateStation(VpCameraConfig cam, VpVppConfig vpp)
		{
			VpStation st = new VpStation(cam.Name, vpp.Name, IsProductionVpp(vpp) ? VpStationKind.Fiducial : VpStationKind.Custom);
			st.SetOwner(cam.Name, vpp.Name, System.IO.Path.Combine(cam.StorageFolder, vpp.StorageFolder));
			st.RunCompleted += OnStationRunCompleted;
			st.RunLiveCCDSN = cam.CameraSN;            // 相机绑定键（按 SN 绑定，见 CogCameraDevices）
			st.RunLiveCCDIndex = ResolveChannel(cam);  // 采集通道由 SN 现场解析，没选 SN 时为 -1
			st.RunExposure = vpp.Exposure;             // 本 VPP 的曝光
			st.Config = vpp;                           // 配置引用（补偿限制从这里读取）
			// 生产加载只读取已有文件；新算法由编辑入口显式创建。
			st.LoadTB(st.GetVppPath(SysPara.RecipeName));
			vpp.Station = st;
		}

		private void CreateCameraStations(VpCameraConfig cam)
		{
			for (int i = 0; i < cam.Vpps.Count; i++)
				if (cam.Vpps[i].Station == null) CreateStation(cam, cam.Vpps[i]);
		}

		/// <summary>在 UI 线程反馈手动调试失败，不让错误通知占用视觉运行线程。</summary>
		private void OnStationRunCompleted(VisionRunResult result)
		{
			if (result.State == VisionRunState.Succeeded || IsDisposed || !IsHandleCreated) return;
			if (InvokeRequired)
			{
				BeginInvoke(new Action(() => OnStationRunCompleted(result)));
				return;
			}
			MessageBox.Show(result.Error, T("vpMsg_Title", "提示"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		/// <summary>在空闲状态打开算法编辑，缺失的 VPP 由编辑操作显式新建。</summary>
		internal void EditStation(VpVppConfig vpp)
		{
			if (vpp == null || vpp.Station == null) return;
			if (!EnsureIdle(T("vpAct_Edit", "编辑视觉算法"))) return;
			string error;
			if (!vpp.Station.IsLoadTBOk && !vpp.Station.TryCreateTB(vpp.Station.GetVppPath(SysPara.RecipeName), out error))
			{ MessageBox.Show(error, T("vpMsg_Title", "提示")); return; }
			vpp.Station.EditTB();
		}

		/// <summary>拍照并运行 ToolBlock，结果自动显示到相机的显示控件上。</summary>
		internal void RunStation(VpVppConfig vpp)
		{
			if (vpp == null || vpp.Station == null) return;
			if (!EnsureIdle(T("vpAct_Capture", "手动拍照"))) return;
			long runId;
			if (!vpp.Station.TryRunTB(out runId)) MessageBox.Show(vpp.Station.LastError, T("vpMsg_Title", "提示"));
		}

		/// <summary>
		/// 相机级实时显示的开关：采集通道由相机 SN 现场解析，曝光用当前选中 VPP 的 Exposure。
		/// </summary>
		private void LiveCamera(CameraEntry entry)
		{
			if (entry == null || entry.Display == null) return;
			if (IsLiveRunning(entry)) { entry.Display.StopLiveDisplay(); return; }
			if (!EnsureIdle(T("vpAct_Live", "启动实时显示"))) return;

			VpVppConfig vpp = CurrentVpp(entry);
			VpStation driver = (vpp != null) ? vpp.Station : null;
			if (driver == null)
			{
				MessageBox.Show(T("vpMsg_NoVppForLive", "这台相机下还没有可用的 VPP。\r\n实时显示用的是「当前 VPP 的曝光」，请先添加一个 VPP。"),
					T("vpMsg_Title", "提示"));
				return;
			}

			driver.RunLiveCCDSN = (entry.Config != null) ? entry.Config.CameraSN : driver.RunLiveCCDSN;
			driver.RunLiveCCDIndex = (entry.Config != null) ? ResolveChannel(entry.Config) : driver.RunLiveCCDIndex;
			driver.RunExposure = vpp.Exposure;
			driver.RunLive(entry.Display);
		}

		/// <summary>按相机 SN 解析当前采集通道；没选 SN 或机器上找不到时返回 -1。</summary>
		private static int ResolveChannel(VpCameraConfig cam)
		{
			if (cam == null) return -1;
			string error;
			return cam.ResolveCcdIndex(out error);
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

		/// <summary>VPP 曝光与显示名称在失焦或回车时提交。</summary>
		private void AttachVppEditors(VpCameraConfig cam, VpVppConfig vpp, TabPage page, UiLabel info, TextBox tbExp, TextBox tbRename)
		{
			if (cam == null || vpp == null) return;

			// 先把配置回填进控件（否则页面上曝光框是空的，看起来就是"参数没加载"），再挂事件
			if (tbExp != null) tbExp.Text = vpp.Exposure.ToString("F1");
			if (tbRename != null) tbRename.Text = vpp.Name ?? "";

			// 曝光在失焦 / 回车时提交：避免每敲一个字符就写一次配置文件
			tbExp.Leave += delegate { CommitVppExposure(vpp, tbExp); };
			tbExp.KeyDown += delegate (object s, KeyEventArgs e)
			{
				if (e.KeyCode != Keys.Enter) return;
				e.SuppressKeyPress = true;
				CommitVppExposure(vpp, tbExp);
			};

			tbRename.Leave += delegate { ApplyVppRename(cam, vpp, tbRename, page, info); };
			tbRename.KeyDown += delegate (object s, KeyEventArgs e)
			{
				if (e.KeyCode != Keys.Enter) return;
				e.SuppressKeyPress = true;
				ApplyVppRename(cam, vpp, tbRename, page, info);
			};
		}

		/// <summary>应用 VPP 曝光（会推给正在运行的工位）；设备运行中拒绝修改并还原，非法输入也还原。</summary>
		private void CommitVppExposure(VpVppConfig vpp, TextBox box)
		{
			if (vpp == null || box == null) return;

			double exposure;
			if (!VpConfigStore.TryParseInputDouble(box.Text, out exposure) || exposure <= 0)
			{
				box.Text = vpp.Exposure.ToString("F1");
				return;
			}
			if (exposure == vpp.Exposure) return;

			if (!EnsureIdle(T("vpAct_ChangeExposure", "修改曝光")))
			{
				box.Text = vpp.Exposure.ToString("F1");
				return;
			}

			vpp.Exposure = exposure;
			if (vpp.Station != null) vpp.Station.RunExposure = exposure;
			SaveVpConfig();
		}

		/// <summary>保存本站 VPP，写盘失败时显示原因。</summary>
		internal void SaveStation(VpVppConfig vpp)
		{
			if (vpp == null || vpp.Station == null) return;
			if (!EnsureIdle(T("vpAct_Save", "保存视觉算法"))) return;
			string error;
			if (!vpp.Station.TrySaveTB(out error)) MessageBox.Show(error, T("vpMsg_Title", "提示"));
		}

		/// <summary>修改 VPP 显示名称并保留存储目录；名称不合法时还原输入框。</summary>
		private void ApplyVppRename(VpCameraConfig cam, VpVppConfig vpp, TextBox box, TabPage page, Label info)
		{
			if (cam == null || vpp == null || box == null) return;

			try
			{
				string newName = (box.Text ?? "").Trim();
				if (newName == vpp.Name) return;

				string nameReason;
				if (!IsValidNodeName(newName, out nameReason))
				{
					MessageBox.Show(nameReason, T("vpMsg_Title", "提示"));
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
					vpp.Station.SetOwner(cam.Name, newName, System.IO.Path.Combine(cam.StorageFolder, vpp.StorageFolder));
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
			if (IsProductionVpp(vpp))
			{ MessageBox.Show(MiddleLayer.LangMsg(LangForm, "vpMsg_UnbindVpp", "请先解除或更换此 VPP 的生产定位绑定。", "Remove or change this VPP's production binding first.", "Primero quite o cambie la vinculación de producción de este VPP."), T("vpMsg_Title", "提示")); return; }
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
					if (!ReferenceEquals(vppTabs.TabPages[i].Tag, vpp)) continue;
					TabPage p = vppTabs.TabPages[i];
					vppTabs.TabPages.RemoveAt(i);
					p.Dispose();
					break;
				}

			cam.Vpps.Remove(vpp);
		}

		// ---------------- 小工具 ----------------

		/// <summary>把视觉配置立即写入 VPForm.Cameras.xml；写失败时把原因显示到工具条。</summary>
		internal void SaveVpConfig()
		{
			string error;
			if (!TrySaveVpConfig(out error)) UpdateVpHint();
		}

		/// <summary>配置完整加载后才允许写盘，返回实际保存结果。</summary>
		private bool TrySaveVpConfig(out string error)
		{
			error = _vpLoadError;
			if (!string.IsNullOrEmpty(error)) return false;
			VpConfigStore.Save(_config);
			error = VpConfigStore.LastSaveError;
			return string.IsNullOrEmpty(error);
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
			if (!EnsureIdle(T("vpAct_Reload", "重载视觉配置"))) return;
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

		/// <summary>调试修改必须在设备空闲且所有视觉工位停止运行或编辑后执行。</summary>
		private bool EnsureIdle(string action)
		{
			if (SysPara.SystemMode != RunMode.IDLE)
			{
				MessageBox.Show(T("vpMsg_RunningCant", "设备运行中，不能") + action + "！", T("vpMsg_WarningTitle", "警告"));
				return false;
			}
			foreach (VisionproInterface station in VisionproInterface.VList)
			{
				if (!station.IsBusy) continue;
				MessageBox.Show(MiddleLayer.LangMsg("VPForm", "vpMsg_BusyAction", "视觉工位忙碌，不能", "Vision station is busy; cannot ", "La estación de visión está ocupada; no se puede ") + action + "：" + station.StationName, T("vpMsg_WarningTitle", "警告"));
				return false;
			}
			foreach (CameraEntry entry in _entries)
			{
				if (!IsLiveRunning(entry)) continue;
				MessageBox.Show(MiddleLayer.LangMsg("VPForm", "vpError_StopLive", "请先停止相机实时显示：", "Stop camera live view first: ", "Detenga primero la vista en directo de la cámara: ") + entry.Key, T("vpMsg_WarningTitle", "警告"));
				return false;
			}
			return true;
		}

		private VpCameraConfig FindCamera(string name)
		{
			if (string.IsNullOrEmpty(name)) return null;
			for (int i = 0; i < _cameras.Count; i++)
				if (string.Equals(_cameras[i].Name, name, StringComparison.OrdinalIgnoreCase)) return _cameras[i];
			return null;
		}

		/// <summary>按相机名取相机配置（名称 / SN / Vpps / 补偿限制）；不存在返回 null。</summary>
		public VpCameraConfig GetCameraConfig(string cameraName)
		{
			return FindCamera(cameraName);
		}

		/// <summary>
		/// 获取指定相机、VPP 已提交的补偿配置快照（Item、Compensation、Min、Max）；不存在返回 null。
		/// 不访问界面或运行视觉，不提交正在编辑的值；修改返回的列表不会更改配置，补偿由应用流程自行使用。
		/// </summary>
		public List<VpCompLimitItem> GetCompensationTable(string cameraName, string vppName)
		{
			VpCameraConfig camera = FindCamera(cameraName);
			VpVppConfig vpp = FindVpp(camera, vppName);
			if (vpp == null) return null;

			List<VpCompLimitItem> table = new List<VpCompLimitItem>();
			foreach (VpCompLimitItem item in vpp.CompLimits)
			{
				if (item == null) continue;
				table.Add(new VpCompLimitItem(item.Item, item.Min, item.Max) { Compensation = item.Compensation });
			}
			return table;
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

        /// <summary>列出动态 VPP 和兼容视觉站的实际对象，使用稳定 VPP 身份而非显示名定位。</summary>
        public Dictionary<string, VisionproInterface> GetFlowStations()
        {
            Dictionary<string, VisionproInterface> result = new Dictionary<string, VisionproInterface>();
            foreach (VpCameraConfig camera in _cameras)
                foreach (VpVppConfig vpp in camera.Vpps)
                    if (vpp.Station != null) result.Add("vpp/" + vpp.Id, vpp.Station);
            foreach (System.Reflection.FieldInfo field in GetType().GetFields())
            {
                if (!typeof(VisionproInterface).IsAssignableFrom(field.FieldType)) continue;
                VisionproInterface station = field.GetValue(this) as VisionproInterface;
                if (station != null) result.Add("legacy/" + field.Name, station);
            }
            return result;
        }

		private VpVppConfig FindVpp(VpCameraConfig cam, string name)
		{
			if (cam == null) return null;
			for (int i = 0; i < cam.Vpps.Count; i++)
				if (string.Equals(cam.Vpps[i].Name, name, StringComparison.OrdinalIgnoreCase)) return cam.Vpps[i];
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

		/// <summary>将语言切换后的动态文字刷新安排到窗体 UI 线程，释放后解除事件订阅。</summary>
		private void OnLanguageChanged(object sender, EventArgs e)
		{
			if (IsDisposed || Disposing) return;
			if (InvokeRequired) { BeginInvoke(new Action(RefreshDynamicTexts)); return; }
			RefreshDynamicTexts();
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
					if (e.Page != null)
						foreach (Control control in e.Page.Controls)
							if (control is VpCameraPage) ((VpCameraPage)control).RefreshTexts();

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
					if (vpp != null) RefreshCompLimitHeaders(g);
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
			if (entry == null || entry.Config == null || calib == null || calib.Station != null) return;

			VpStation st = new VpStation(entry.Key, calib.FolderName, VpStationKind.Calibration);
			st.SetOwner(entry.Key, calib.FolderName, System.IO.Path.Combine(entry.Config.StorageFolder, calib.FolderName));
			st.RunCompleted += OnStationRunCompleted;
			if (entry.Config != null)
			{
				st.RunLiveCCDSN = entry.Config.CameraSN;
				st.RunLiveCCDIndex = ResolveChannel(entry.Config);
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
			if (!EnsureIdle(T("vpAct_Point", "修改标定点"))) { FillCalibrationGrid(card.GridPoints, c); return; }

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
			RefreshCalibrationTexts(entry);      // 点位变了 → 标题显示"已修改，未应用"
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
			if (tbExp != null) VpConfigStore.TryParseInputDouble(tbExp.Text, out exp);
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
			if (entry == null || !EnsureIdle(T("vpAct_Edit", "编辑视觉算法"))) return;
			VpCalibration calib = _config.GetCalibration(entry.Key);
			if (calib == null || calib.Station == null) return;
			OperationLog.Write("VPForm", "打开 ToolBlock 编辑：相机 " + entry.Key);
			string error;
			if (!calib.Station.IsLoadTBOk && !calib.Station.TryCreateTB(calib.Station.GetVppPath(SysPara.RecipeName), out error))
			{ MessageBox.Show(error, T("vpMsg_Title", "提示")); return; }
			calib.Station.EditTB();
		}

		/// <summary>应用标定专用曝光（与各 VPP 的曝光独立）；标定尚未创建时，值在点【添加标定】时采用。</summary>
		private void ApplyCalibrationExposure(CameraEntry entry, TextBox box)
		{
			if (entry == null || box == null) return;
			if (!EnsureIdle(T("vpAct_ChangeExposure", "修改曝光"))) return;

			double exp;
			if (!VpConfigStore.TryParseInputDouble(box.Text, out exp) || exp <= 0) return;

			VpCalibration calib = _config.GetCalibration(entry.Key);
			if (calib == null) return;
			if (calib.Exposure == exp) return;

			calib.Exposure = exp;
			if (calib.Station != null)
			{
				calib.Station.RunExposure = exp;
				if (entry.Config != null)
				{
					calib.Station.RunLiveCCDSN = entry.Config.CameraSN;
					calib.Station.RunLiveCCDIndex = ResolveChannel(entry.Config);
				}
			}
			SaveVpConfig();
		}

		/// <summary>拍照：运行一次标定 ToolBlock，结果显示在本相机的共用显示控件上。</summary>
		private void SnapCalibration(CameraEntry entry)
		{
			if (entry == null || !EnsureIdle(T("vpAct_Capture", "手动拍照"))) return;
			VpCalibration calib = _config.GetCalibration(entry.Key);
			if (calib == null || calib.Station == null) return;
			long runId;
			if (!calib.Station.TryRunTB(out runId)) MessageBox.Show(calib.Station.LastError, T("vpMsg_Title", "提示"));
		}

		/// <summary>用当前像素坐标添加一个标定点（电机坐标初始为 0，在表格中填写）。</summary>
		private void AddCalibrationPoint(CameraEntry entry, DataGridView grid)
		{
			if (entry == null || !EnsureIdle(T("vpAct_Point", "修改标定点"))) return;
			VpCalibration calib = _config.GetCalibration(entry.Key);
			if (calib == null || calib.Station == null)
			{
				MessageBox.Show(T("vpMsg_NeedCalibration", "请先添加标定。"),
					T("vpMsg_Title", "提示"));
				return;
			}
			VisionRunResult result = calib.Station.LastRunResult;
			if (result == null || result.State != VisionRunState.Succeeded)
			{
				MessageBox.Show(T("vpMsg_NeedPixel", "请先点【拍照】取得像素坐标。"),
					T("vpMsg_Title", "提示"));
				return;
			}

			double px = result.X;
			double py = result.Y;
			if (MessageBox.Show(T("vpMsg_AddPointConfirm", "确认添加标定点？\r\n像素坐标 (")
					+ px.ToString("F3") + ", " + py.ToString("F3") + ")",
					T("vpMsg_AddPointTitle", "添加标定点"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK) return;

			SyncPointsFromGrid(grid, calib);
			calib.Points.Add(new VpCalibPoint(px, py, 0, 0));
			FillCalibrationGrid(grid, calib);
			SaveVpConfig();
			RefreshCalibrationTexts(entry);
		}

		private void RemoveCalibrationPoint(CameraEntry entry, DataGridView grid)
		{
			if (entry == null || !EnsureIdle(T("vpAct_Point", "修改标定点"))) return;
			VpCalibration calib = _config.GetCalibration(entry.Key);
			if (calib == null || grid == null || grid.CurrentRow == null) return;
			if (grid.CurrentRow.IsNewRow) return;

			SyncPointsFromGrid(grid, calib);
			int idx = grid.CurrentRow.Index;
			if (idx < 0 || idx >= calib.Points.Count) return;

			calib.Points.RemoveAt(idx);
			FillCalibrationGrid(grid, calib);
			SaveVpConfig();
			RefreshCalibrationTexts(entry);
		}

		/// <summary>标定残差的告警阈值（mm）；超过则要求操作员确认后才写入 VPP。</summary>
		private const double CalibrationRmseWarnMm = 0.05;

		/// <summary>
		/// 把标定点写入本相机下所有 VPP：先预检点位质量（残差 / 镜像 / 比例），
		/// 再逐个写入，全部成功才记「已应用」。
		/// </summary>
		private void ApplyCalibration(CameraEntry entry, DataGridView grid)
		{
			if (entry == null) return;
			if (!EnsureIdle(T("vpAct_ApplyCalibration", "应用标定"))) return;
			if (entry.Config == null || string.IsNullOrWhiteSpace(entry.Config.CameraSN))
			{ MessageBox.Show(MiddleLayer.LangMsg("VPForm", "vpMsg_SelectCalibSN", "请先选择标定相机 SN。", "Select the calibration camera SN first.", "Seleccione primero el SN de la cámara de calibración."), T("vpMsg_Title", "提示")); return; }

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

			List<CalibrationData> points = ToCalibrationData(calib);

			CalibApplyInfo precheck = VisionproInterface.CheckCalibration(points);
			if (!precheck.Calibrated)
			{
				MessageBox.Show(T("vpMsg_CalibInvalidA", "这批标定点算不出有效标定，未写入任何 VPP：\r\n")
					+ precheck.Error
					+ T("vpMsg_CalibInvalidB", "\r\n请检查点位是否共线、重复或坐标录错。"),
					T("vpMsg_CalibrationTitle", "标定"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			if (!ConfirmCalibrationQuality(precheck)) return;
			calib.AppliedSignature = "none";
			string saveError;
			if (!TrySaveVpConfig(out saveError))
			{ MessageBox.Show(saveError, T("vpMsg_Title", "提示")); return; }

			List<string> failures = new List<string>();
			int okCount = PushCalibrationToTargets(entry, points, calib.ToolName, failures);

			if (okCount == entry.Targets.Count)
			{
				try
				{
					calib.MarkApplied(entry.Config, SysPara.RecipeName);
					if (!TrySaveVpConfig(out saveError)) throw new InvalidOperationException(saveError);
				}
				catch (Exception ex) { calib.AppliedSignature = "none"; failures.Add(MiddleLayer.LangMsg("VPForm", "vpMsg_PublishFailed", "标定发布记录未保存：", "Calibration publication record was not saved: ", "No se guardó el registro de publicación de calibración: ") + ex.Message); }
			}
			RefreshCalibrationTexts(entry);

			ReportCalibrationResult(entry, precheck, okCount, failures);
		}

		/// <summary>把标定点转成视觉站写入 TB 用的结构。</summary>
		private static List<CalibrationData> ToCalibrationData(VpCalibration calib)
		{
			List<CalibrationData> list = new List<CalibrationData>();
			for (int i = 0; i < calib.Points.Count; i++)
			{
				CalibrationData data = new CalibrationData();
				data.PixelX = calib.Points[i].PixelX;
				data.PixelY = calib.Points[i].PixelY;
				data.MotorPosX = calib.Points[i].MotorPosX;
				data.MotorPosY = calib.Points[i].MotorPosY;
				list.Add(data);
			}
			return list;
		}

		/// <summary>残差超阈值、点位镜像或比例异常时弹窗确认；返回 false 表示操作员取消。</summary>
		private static bool ConfirmCalibrationQuality(CalibApplyInfo info)
		{
			string warnings = "";

			if (info.Rmse > CalibrationRmseWarnMm)
				warnings += "· " + T("vpMsg_CalibRmse", "标定残差偏大：") + info.Rmse.ToString("F3") + " mm"
					+ T("vpMsg_CalibRmseHint", "（建议不超过 ") + CalibrationRmseWarnMm.ToString("F3") + " mm）\r\n";

			if (info.Mirrored)
				warnings += "· " + T("vpMsg_CalibMirrored", "像素与电机的方向相反，X/Y 可能接反或相机镜像安装。\r\n");

			if (info.ScalePerPixel <= 0 || info.ScalePerPixel > 100)
				warnings += "· " + T("vpMsg_CalibScale", "比例异常：") + info.ScalePerPixel.ToString("F4") + " mm/px"
					+ T("vpMsg_CalibScaleHint", "（请检查坐标是否录错）") + "\r\n";

			if (warnings.Length == 0) return true;

			return MessageBox.Show(warnings + T("vpMsg_CalibContinue", "\r\n仍要写入本相机下所有 VPP 吗？"),
				T("vpMsg_CalibrationTitle", "标定"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK;
		}

		/// <summary>把标定点逐个写入各 VPP 的标定工具，返回成功个数；失败的工位与原因追加到 failures。</summary>
		private static int PushCalibrationToTargets(CameraEntry entry, List<CalibrationData> points,
			string toolName, List<string> failures)
		{
			int okCount = 0;
			for (int i = 0; i < entry.Targets.Count; i++)
			{
				VpStation station = entry.Targets[i];
				if (station == null) { failures.Add(MiddleLayer.LangMsg("VPForm", "vpMsg_EmptyStation", "?：工位为空", "?: station is missing", "?: falta la estación")); continue; }

				CalibApplyInfo info;
				if (station.SetCalibration(points, toolName, out info)) { okCount++; continue; }

				failures.Add(station.VppSubFolder + "："
					+ (string.IsNullOrEmpty(info.Error) ? T("vpMsg_UnknownReason", "未知原因") : info.Error));
			}
			return okCount;
		}

		/// <summary>显示应用结果（成功数 / 残差 / 比例 / 失败明细）并写操作日志。</summary>
		private static void ReportCalibrationResult(CameraEntry entry, CalibApplyInfo info,
			int okCount, List<string> failures)
		{
			string quality = T("vpMsg_CalibQuality", "\r\n残差 ") + info.Rmse.ToString("F3")
				+ T("vpMsg_CalibScaleShort", " mm，比例 ") + info.ScalePerPixel.ToString("F4") + " mm/px";

			OperationLog.Write("VPForm", "应用标定：相机 " + entry.Key + " → " + okCount + "/" + entry.Targets.Count
				+ " 个 VPP，残差 " + info.Rmse.ToString("F3") + " mm，比例 " + info.ScalePerPixel.ToString("F4")
				+ " mm/px" + (info.Mirrored ? "，方向镜像" : ""));

			if (failures.Count == 0)
			{
				MessageBox.Show(T("vpMsg_AppliedA", "标定已应用到本相机下 ") + okCount
					+ T("vpMsg_AppliedB", " 个 VPP：\r\n") + TargetNames(entry) + quality,
					T("vpMsg_CalibrationTitle", "标定"), MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			string detail = "";
			for (int i = 0; i < failures.Count; i++) detail += "\r\n· " + failures[i];

			MessageBox.Show(T("vpMsg_PartialA", "只有 ") + okCount + "/" + entry.Targets.Count
				+ T("vpMsg_PartialB", " 个 VPP 成功，以下失败：") + detail + quality,
				T("vpMsg_CalibrationTitle", "标定"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
			return VpConfigStore.TryParseInputDouble(Convert.ToString(v), out d) ? d : 0;
		}

		#endregion
	}
}
