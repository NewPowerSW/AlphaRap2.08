using System;
using System.Windows.Forms;
using Cognex.VisionPro;

namespace AlphaRap
{
    internal partial class VpCalibrationSetupForm : Form
    {
        private readonly VpCalibrationSetup setup;

        /// <summary>在设计器中显示固定界面，不读取相机。</summary>
        public VpCalibrationSetupForm() { InitializeComponent(); }

        /// <summary>使用上次试拍照片与独立形状副本进行拖动设置。</summary>
        internal VpCalibrationSetupForm(VpCalibrationSetup setup) : this()
        {
            this.setup = setup;
            limit.Text = setup.MaxFitRms.ToString("R");
            Shown += ShowPhoto;
            confirm.Click += AcceptSettings;
            RefreshTexts();
            FrameworkLanguage.RegisterAndApplyLanguage(this, "VpCalibrationSetupForm");
            FrameworkLanguage.LanguageChanged += RefreshTexts;
        }

        /// <summary>显示控件就绪后载入照片，叠加可拖动的搜索形状。</summary>
        private void ShowPhoto(object sender, EventArgs e)
        {
            photo.Image = setup.Image;
            if (setup.Circle != null) AddShape(setup.Circle);
            else
            {
                AddShape(setup.Line1);
                AddShape(setup.Line2);
            }
            photo.Fit(true);
        }

        /// <summary>开启图形拖动，并在照片中显示对应颜色。</summary>
        private void AddShape(ICogGraphicInteractive shape)
        {
            shape.Interactive = true;
            photo.InteractiveGraphics.Add(shape, "calibration", false);
        }

        /// <summary>按特征类型说明应如何摆放图形与设置质量门限。</summary>
        private void RefreshTexts(object sender = null, EventArgs e = null)
        {
            Text = T("VpCalibrationSetupForm", "设置找边", "Set up edges", "Configurar bordes");
            instructions.Text = setup.Circle != null
                ? T("circleHelp", "拖动圆心和半径，让圆贴近要找的边。误差门限可先保留当前值。", "Drag the center and radius until the circle follows the edge. Start with the current fit limit.", "Arrastre el centro y el radio hasta colocar el círculo sobre el borde. Mantenga inicialmente el límite de ajuste.")
                : T("linesHelp", "拖动黄线和蓝线的两端，让每条线贴在一条边上。系统用两条边的交点采样。", "Drag the ends of the yellow and cyan lines onto the two edges. Their intersection is the sampling point.", "Arrastre los extremos de las líneas amarilla y cian sobre los dos bordes. Su intersección es el punto de muestreo.");
            limitLabel.Text = T("limitLabel", "允许的拟合误差 (px)", "Allowed edge fit error (px)", "Error permitido de ajuste (px)");
            confirm.Text = T("confirm", "使用此设置", "Use these settings", "Usar estos ajustes");
            cancel.Text = T("cancel", "取消", "Cancel", "Cancelar");
        }

        /// <summary>检查拖动后的形状与门限，关闭后由当前配方工位提交草稿。</summary>
        private void AcceptSettings(object sender, EventArgs e)
        {
            double value;
            if (!VpConfigStore.TryParseInputDouble(limit.Text, out value) || value <= 0)
            {
                MessageBox.Show(this, T("positiveLimit", "请输入大于 0 的像素误差门限。", "Enter a fit limit greater than zero pixels.", "Introduzca un límite de ajuste mayor que cero píxeles."), Text);
                limit.Focus();
                return;
            }
            setup.MaxFitRms = value;
            DialogResult = DialogResult.OK;
        }

        /// <summary>释放照片显示及语言订阅。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing) FrameworkLanguage.LanguageChanged -= RefreshTexts;
            base.Dispose(disposing);
        }

        /// <summary>读取找边设置窗口的三语文案。</summary>
        private static string T(string key, string zh, string en, string es) { return FrameworkLanguage.LangMsg("VpCalibrationSetupForm", key, zh, en, es); }
    }
}
