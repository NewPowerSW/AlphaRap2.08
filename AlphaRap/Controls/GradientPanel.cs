using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>
    /// 垂直渐变背景的 TableLayoutPanel。
    /// 用于左侧导航栏：浅暖色自上而下渐变（现代 IM 侧边栏风格），
    /// 子控件设 BackColor = Transparent 即可透出渐变。
    /// </summary>
    public class GradientTableLayoutPanel : TableLayoutPanel
    {
        /// <summary>渐变顶部颜色（浅暖白）</summary>
        public Color GradientTop { get; set; } = Color.FromArgb(250, 252, 255);

        /// <summary>渐变底部颜色（暖米色）</summary>
        public Color GradientBottom { get; set; } = Color.FromArgb(228, 238, 249);

        public GradientTableLayoutPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint
                     | ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.ResizeRedraw
                     | ControlStyles.UserPaint, true);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (ClientRectangle.Width <= 0 || ClientRectangle.Height <= 0)
            {
                base.OnPaintBackground(e);
                return;
            }
            using (var brush = new LinearGradientBrush(
                ClientRectangle, GradientTop, GradientBottom, LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }
        }
    }
}
