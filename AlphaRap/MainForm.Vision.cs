using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Cognex.VisionPro;

namespace AlphaRap
{
    /// <summary>
    /// 主界面"视觉"页（tabPage10）：按相机动态生成显示区 —— 一台相机一格，
    /// 格子顶部是相机名标签、下面是该相机的 CogRecordDisplay。
    ///
    /// 与 VPForm 相机页上的显示收的是同一张图：VPForm 建工位时会把"本相机在主界面的显示"
    /// 一起挂进该工位的 RecordDisplayList（见 VPForm.RegisterDisplay），
    /// 于是 VisionproInterface 推记录时两个显示同时更新。
    ///
    /// 相机增删（VPForm.AddCamera / DeleteCamera）与改名都会调用 SetCameraDisplays 同步；
    /// 重建时**复用已有相机格里的显示控件实例**（不销毁重建），因为该控件已被工位的
    /// RecordDisplayList 引用，销毁它会让老相机的画面失效。
    ///
    /// 显示控件在运行时创建（设计器里不再放 cogRecordDisplay1），
    /// OcxState 复用 VPForm.resx 的 vpDynDisp.OcxState。
    /// </summary>
    public partial class MainForm
    {
        /// <summary>相机名 → 该相机在主界面上的显示控件。</summary>
        private readonly Dictionary<string, CogRecordDisplay> _camDisplays =
            new Dictionary<string, CogRecordDisplay>(StringComparer.Ordinal);

        /// <summary>当前已生成的相机顺序；与相机列表一致时跳过重建，避免反复重画 ActiveX。</summary>
        private readonly List<string> _camDisplayKeys = new List<string>();

        /// <summary>当前挂在 tabPage10 上的根控件（等分网格，或"未配置相机"提示）。</summary>
        private Control _camDisplayRoot;

        private const string CamCellPrefix = "camCell_dyn_";
        private const string CamCaptionPrefix = "camDispCap_dyn_";
        private const string CamDisplayPrefix = "camDisp_dyn_";

        /// <summary>
        /// 按相机列表重建"视觉"页的显示格子；列表没变时不重建。
        /// 由 VPForm 在相机增删/改名以及建页前调用（见 VPForm.SyncMainDisplays）。
        /// </summary>
        public void SetCameraDisplays(IList<string> cameraNames)
        {
            try
            {
                List<string> keys = new List<string>();
                if (cameraNames != null)
                {
                    for (int i = 0; i < cameraNames.Count; i++)
                    {
                        string k = cameraNames[i];
                        if (!string.IsNullOrEmpty(k) && !keys.Contains(k)) keys.Add(k);
                    }
                }

                if (SameCameraKeys(keys)) return;

                RebuildCameraDisplays(keys);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[SetCameraDisplays] " + ex.Message);
            }
        }

        /// <summary>取某台相机在主界面上的显示控件；没有这台相机时返回 null。</summary>
        public CogRecordDisplay GetCameraDisplay(string cameraName)
        {
            if (string.IsNullOrEmpty(cameraName)) return null;
            CogRecordDisplay d;
            return _camDisplays.TryGetValue(cameraName, out d) ? d : null;
        }

        private bool SameCameraKeys(List<string> keys)
        {
            if (keys.Count != _camDisplayKeys.Count) return false;
            for (int i = 0; i < keys.Count; i++)
                if (keys[i] != _camDisplayKeys[i]) return false;
            return true;
        }

        /// <summary>相机列表变化时重建界面；**保留仍在用的显示控件实例**，只重建外层容器。</summary>
        private void RebuildCameraDisplays(List<string> keys)
        {
            // 先把已有显示控件从旧容器里摘下来：它们可能已被工位的 RecordDisplayList 引用，
            // 不能跟着旧容器一起 Dispose，否则老相机的画面会失效。
            for (int i = 0; i < _camDisplayKeys.Count; i++)
            {
                CogRecordDisplay d;
                if (!_camDisplays.TryGetValue(_camDisplayKeys[i], out d)) continue;

                if (d.Parent != null) d.Parent.Controls.Remove(d);
                if (!keys.Contains(_camDisplayKeys[i]))
                {
                    _camDisplays.Remove(_camDisplayKeys[i]);   // 相机已删除，显示控件也一并销毁
                    d.Dispose();
                }
            }

            // 拆掉上一次的根控件（网格或提示）
            if (_camDisplayRoot != null)
            {
                tabPage10.Controls.Remove(_camDisplayRoot);
                _camDisplayRoot.Dispose();
                _camDisplayRoot = null;
            }
            _camDisplayKeys.Clear();

            if (keys.Count == 0)
            {
                _camDisplayRoot = BuildVisionHint();
                tabPage10.Controls.Add(_camDisplayRoot);
                return;
            }

            // 自动等分网格：列数 = ceil(sqrt(n))，行数 = ceil(n / 列数)
            int cols = (int)Math.Ceiling(Math.Sqrt(keys.Count));
            int rows = (int)Math.Ceiling(keys.Count / (double)cols);

            TableLayoutPanel grid = new TableLayoutPanel();
            grid.Name = "camDisplayGrid";
            grid.Dock = DockStyle.Fill;
            grid.BackColor = UiKit.PageBg;
            grid.Padding = new Padding(6);
            grid.ColumnCount = cols;
            grid.RowCount = rows;
            for (int c = 0; c < cols; c++)
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / cols));
            for (int r = 0; r < rows; r++)
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / rows));

            for (int i = 0; i < keys.Count; i++)
            {
                CogRecordDisplay disp;
                if (!_camDisplays.TryGetValue(keys[i], out disp))
                {
                    disp = CreateCameraDisplay(keys[i]);
                    _camDisplays[keys[i]] = disp;
                }
                grid.Controls.Add(BuildCameraCell(keys[i], disp), i % cols, i / cols);
            }

            _camDisplayKeys.AddRange(keys);
            _camDisplayRoot = grid;
            tabPage10.Controls.Add(grid);
        }

        /// <summary>新建一个该相机的显示控件（ActiveX，必须先设 OcxState）。</summary>
        private CogRecordDisplay CreateCameraDisplay(string cameraName)
        {
            CogRecordDisplay disp = new CogRecordDisplay();
            disp.Name = CamDisplayPrefix + cameraName;

            // CogRecordDisplay 是 ActiveX：代码创建时必须先给 OcxState，否则实例不创建、只画一块深蓝占位色。
            // 复用 VPForm 那份 blob（同一个控件类型），它由 VPForm 运行期读取，不依赖设计器控件。
            try
            {
                object ocx = new ComponentResourceManager(typeof(VPForm)).GetObject("vpDynDisp.OcxState");
                if (ocx is AxHost.State) disp.OcxState = (AxHost.State)ocx;
            }
            catch (Exception) { }

            disp.Dock = DockStyle.Fill;
            disp.ColorMapPredefined = Cognex.VisionPro.Display.CogDisplayColorMapPredefinedConstants.None;
            disp.ColorMapLowerClipColor = Color.Black;
            disp.ColorMapLowerRoiLimit = 0D;
            disp.ColorMapUpperClipColor = Color.Black;
            disp.ColorMapUpperRoiLimit = 1D;
            return disp;
        }

        /// <summary>一格：顶部相机名标签 + 下方该相机的显示控件（显示控件由调用方传入并复用）。</summary>
        private Control BuildCameraCell(string cameraName, CogRecordDisplay disp)
        {
            Panel cell = new Panel();
            cell.Name = CamCellPrefix + cameraName;
            cell.Dock = DockStyle.Fill;
            cell.Margin = new Padding(4);
            cell.BackColor = UiKit.Surface;

            UiLabel caption = new UiLabel();
            caption.Name = CamCaptionPrefix + cameraName;
            caption.Dock = DockStyle.Top;
            caption.Height = 26;
            caption.AutoSize = false;
            caption.TextAlign = ContentAlignment.MiddleLeft;
            caption.Font = UiKit.Bold(10.5f);
            caption.ForeColor = UiKit.TextPrimary;
            caption.Padding = new Padding(8, 0, 0, 0);
            caption.Text = cameraName;      // 标签写相机名，与 VPForm 的相机页签一致

            // 停靠顺序是"后加入的先停靠"：先加显示（Fill）、后加标签（Top），标签才会占到顶部
            cell.Controls.Add(disp);
            cell.Controls.Add(caption);
            return cell;
        }

        /// <summary>没有相机时的一句提示。</summary>
        private Control BuildVisionHint()
        {
            UiLabel hint = new UiLabel();
            hint.Name = "camDispHint_dyn";
            hint.Dock = DockStyle.Fill;
            hint.AutoSize = false;
            hint.TextAlign = ContentAlignment.MiddleCenter;
            hint.Font = UiKit.Regular(12f);
            hint.ForeColor = UiKit.TextMuted;
            hint.Text = MiddleLayer.LangMsg("MainForm", "msg_NoCamera",
                "尚未配置相机", "No camera configured", "No hay cámara configurada");
            return hint;
        }
    }
}
