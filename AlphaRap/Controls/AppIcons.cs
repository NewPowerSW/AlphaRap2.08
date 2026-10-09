using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace AlphaRap
{
    /// <summary>MainForm 用到的矢量图标。</summary>
    public enum AppIcon
    {
        Home, Save, Rapid, Vision, Hard, Manual, Data, Log, Check, AddUser, Mes, LifeSpan,
        Run, Pause, Stop, Reset, System, Lock, AlarmReset, Exit,
        Product, Login,

        // 登录 / 用户管理两个窗体用到的补充图标
        Fingerprint,        // 指纹登录
        Close,              // 关闭
        Shield,             // 权限
        Plus,               // 新增
        Trash,              // 删除
        Edit,               // 修改
        Keyboard,           // 虚拟键盘
        Logo                // 品牌标识（登录页用）
    }

    /// <summary>
    /// 图标配色：默认为深石板灰，机台控制键使用语义色（运行绿 / 暂停琥珀 / 停止红 / 复位蓝 / 退出红）。
    /// </summary>
    public static class AppIconColor
    {
        /// <summary>导航与功能图标：深石板灰（浅底上清晰，且比纯黑柔和）。</summary>
        public static readonly Color Nav = Color.FromArgb(58, 74, 92);

        /// <summary>禁用态：中性浅灰蓝。</summary>
        public static readonly Color Disabled = Color.FromArgb(180, 192, 204);

        /// <summary>禁用态（深蓝顶栏上）。</summary>
        public static readonly Color DisabledOnDark = Color.FromArgb(126, 168, 205);

        public static readonly Color Run = Color.FromArgb(46, 150, 67);
        public static readonly Color Pause = Color.FromArgb(214, 158, 32);
        public static readonly Color Stop = Color.FromArgb(206, 62, 62);
        public static readonly Color Reset = Color.FromArgb(4, 108, 182);
        public static readonly Color Danger = Color.FromArgb(206, 62, 62);

        /// <summary>深蓝顶栏上的图标：纯白，与顶栏文字同色。</summary>
        public static readonly Color OnDarkBar = Color.White;
    }

    /// <summary>
    /// 矢量图标工厂：图标在 24×24 逻辑网格上用 GDI+ 绘制（2px 描边、圆头圆角），可按任意尺寸和颜色输出位图。
    /// 同一 (图标, 尺寸, 颜色) 只绘制一次并缓存，可在定时器中反复调用。
    /// </summary>
    public static class AppIcons
    {
        private const float Grid = 24f;                 // 逻辑网格边长
        private const float StrokeWidth = 2f;           // 标准描边宽度（逻辑单位）

        private static readonly Dictionary<string, Image> Cache = new Dictionary<string, Image>();

        public static Image Get(AppIcon icon, int size, Color color)
        {
            return Get(icon, size, color, color);
        }

        /// <param name="accent">次要颜色，仅个别图标用（如"报警复位"的红色斜线）。</param>
        public static Image Get(AppIcon icon, int size, Color color, Color accent)
        {
            if (size < 8) size = 8;
            if (size > 256) size = 256;

            string key = ((int)icon).ToString() + "|" + size + "|" +
                         color.ToArgb().ToString() + "|" + accent.ToArgb().ToString();

            Image cached;
            if (Cache.TryGetValue(key, out cached)) return cached;

            Image image = null;
            try { image = Render(icon, size, color, accent); }
            catch { image = null; }

            if (image != null) Cache[key] = image;      // 失败不缓存，下次还能重试
            return image;
        }

        private static Image Render(AppIcon icon, int size, Color color, Color accent)
        {
            Bitmap bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.ScaleTransform(size / Grid, size / Grid);
                Draw(g, icon, color, accent);
            }
            return bmp;
        }

        // ------------------------------------------------------------------ 基元

        private static Pen Stroke(Color c)
        {
            return Stroke(c, StrokeWidth);
        }

        private static Pen Stroke(Color c, float width)
        {
            Pen p = new Pen(c, width);
            p.StartCap = LineCap.Round;
            p.EndCap = LineCap.Round;
            p.LineJoin = LineJoin.Round;
            return p;
        }

        /// <summary>逻辑网格上的圆角矩形（半径自动收敛，不会超过短边一半）。</summary>
        private static GraphicsPath RR(float x, float y, float w, float h, float r)
        {
            float d = Math.Min(r * 2f, Math.Min(w, h));
            GraphicsPath path = new GraphicsPath();
            path.AddArc(x, y, d, d, 180, 90);
            path.AddArc(x + w - d, y, d, d, 270, 90);
            path.AddArc(x + w - d, y + h - d, d, d, 0, 90);
            path.AddArc(x, y + h - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static PointF P(float x, float y)
        {
            return new PointF(x, y);
        }

        // ------------------------------------------------------------------ 图标

        private static void Draw(Graphics g, AppIcon icon, Color c, Color accent)
        {
            switch (icon)
            {
                case AppIcon.Home: DrawHome(g, c); break;
                case AppIcon.Save: DrawSave(g, c); break;
                case AppIcon.Rapid: DrawRapid(g, c); break;
                case AppIcon.Vision: DrawVision(g, c); break;
                case AppIcon.Hard: DrawHard(g, c); break;
                case AppIcon.Manual: DrawManual(g, c); break;
                case AppIcon.Data: DrawData(g, c); break;
                case AppIcon.Log: DrawLog(g, c); break;
                case AppIcon.Check: DrawCheck(g, c); break;
                case AppIcon.AddUser: DrawAddUser(g, c); break;
                case AppIcon.Mes: DrawMes(g, c); break;
                case AppIcon.LifeSpan: DrawLifeSpan(g, c); break;

                case AppIcon.Run: DrawRun(g, c); break;
                case AppIcon.Pause: DrawPause(g, c); break;
                case AppIcon.Stop: DrawStop(g, c); break;
                case AppIcon.Reset: DrawReset(g, c); break;
                case AppIcon.System: DrawSystem(g, c); break;
                case AppIcon.Lock: DrawLock(g, c); break;
                case AppIcon.AlarmReset: DrawAlarmReset(g, c, accent); break;
                case AppIcon.Exit: DrawExit(g, c); break;

                case AppIcon.Product: DrawProduct(g, c); break;
                case AppIcon.Login: DrawLogin(g, c); break;

                case AppIcon.Fingerprint: DrawFingerprint(g, c); break;
                case AppIcon.Close: DrawClose(g, c); break;
                case AppIcon.Shield: DrawShield(g, c); break;
                case AppIcon.Plus: DrawPlus(g, c); break;
                case AppIcon.Trash: DrawTrash(g, c); break;
                case AppIcon.Edit: DrawEdit(g, c); break;
                case AppIcon.Keyboard: DrawKeyboard(g, c); break;
                case AppIcon.Logo: DrawLogo(g, c); break;
            }
        }

        // ---------- 左侧导航（描边线稿） ----------

        /// <summary>主界面：屋顶 + 屋身 + 门</summary>
        private static void DrawHome(Graphics g, Color c)
        {
            using (Pen p = Stroke(c))
            {
                g.DrawLines(p, new[] { P(3f, 11.4f), P(12f, 3.6f), P(21f, 11.4f) });
                using (GraphicsPath body = RR(6.2f, 10.8f, 11.6f, 9.6f, 2.2f)) g.DrawPath(p, body);
                using (GraphicsPath door = RR(10.2f, 15.4f, 3.6f, 5f, 1.2f)) g.DrawPath(p, door);
            }
        }

        /// <summary>保存：磁盘（工业软件里仍是最没有歧义的"保存"符号）</summary>
        private static void DrawSave(Graphics g, Color c)
        {
            using (Pen p = Stroke(c))
            {
                using (GraphicsPath body = RR(3.8f, 3.8f, 16.4f, 16.4f, 2.6f)) g.DrawPath(p, body);
                g.DrawLines(p, new[] { P(8.4f, 3.8f), P(8.4f, 9.6f), P(15.6f, 9.6f), P(15.6f, 3.8f) });
                using (GraphicsPath label = RR(7.4f, 13.8f, 9.2f, 6.4f, 1.2f)) g.DrawPath(p, label);
            }
        }

        /// <summary>快捷键：闪电</summary>
        private static void DrawRapid(Graphics g, Color c)
        {
            using (Pen p = Stroke(c))
            {
                p.LineJoin = LineJoin.Round;
                g.DrawPolygon(p, new[]
                {
                    P(13.8f, 2.8f), P(5.6f, 13.4f), P(10.8f, 13.4f),
                    P(9.2f, 21.2f), P(17.8f, 10.4f), P(12.6f, 10.4f)
                });
            }
        }

        /// <summary>视觉界面：相机</summary>
        private static void DrawVision(Graphics g, Color c)
        {
            using (Pen p = Stroke(c))
            {
                using (GraphicsPath body = RR(3.2f, 7f, 17.6f, 12.8f, 3f)) g.DrawPath(p, body);
                g.DrawLines(p, new[] { P(8.4f, 7f), P(9.5f, 4.4f), P(14.5f, 4.4f), P(15.6f, 7f) });
                g.DrawEllipse(p, 9f, 10.1f, 6f, 6f);
            }
        }

        /// <summary>硬件调试：芯片</summary>
        private static void DrawHard(Graphics g, Color c)
        {
            using (Pen p = Stroke(c))
            {
                using (GraphicsPath chip = RR(6.2f, 6.2f, 11.6f, 11.6f, 2.2f)) g.DrawPath(p, chip);
                using (GraphicsPath core = RR(9.8f, 9.8f, 4.4f, 4.4f, 1.2f)) g.DrawPath(p, core);
            }
            using (Pen p = Stroke(c, 1.8f))
            {
                float[] at = { 9.4f, 12f, 14.6f };
                foreach (float v in at)
                {
                    g.DrawLine(p, v, 3.4f, v, 6.2f);
                    g.DrawLine(p, v, 17.8f, v, 20.6f);
                    g.DrawLine(p, 3.4f, v, 6.2f, v);
                    g.DrawLine(p, 17.8f, v, 20.6f, v);
                }
            }
        }

        /// <summary>手动界面：两条带旋钮的调节滑轨</summary>
        private static void DrawManual(Graphics g, Color c)
        {
            using (Pen p = Stroke(c))
            {
                const float cy1 = 8.4f, cy2 = 15.6f, kx1 = 10.6f, kx2 = 13.4f, kr = 3.2f;
                g.DrawLine(p, 3.6f, cy1, kx1 - kr, cy1);
                g.DrawLine(p, kx1 + kr, cy1, 20.4f, cy1);
                g.DrawEllipse(p, kx1 - kr, cy1 - kr, kr * 2f, kr * 2f);

                g.DrawLine(p, 3.6f, cy2, kx2 - kr, cy2);
                g.DrawLine(p, kx2 + kr, cy2, 20.4f, cy2);
                g.DrawEllipse(p, kx2 - kr, cy2 - kr, kr * 2f, kr * 2f);
            }
        }

        /// <summary>产品数据：柱状图</summary>
        private static void DrawData(Graphics g, Color c)
        {
            using (Pen p = Stroke(c))
            {
                g.DrawLine(p, 3.6f, 20.4f, 20.4f, 20.4f);
                using (GraphicsPath b1 = RR(4.8f, 12.6f, 3.8f, 7.8f, 1.4f)) g.DrawPath(p, b1);
                using (GraphicsPath b2 = RR(10.1f, 8.6f, 3.8f, 11.8f, 1.4f)) g.DrawPath(p, b2);
                using (GraphicsPath b3 = RR(15.4f, 4.6f, 3.8f, 15.8f, 1.4f)) g.DrawPath(p, b3);
            }
        }

        /// <summary>日志：文档 + 文字行</summary>
        private static void DrawLog(Graphics g, Color c)
        {
            using (Pen p = Stroke(c))
            {
                using (GraphicsPath body = RR(4.8f, 3.2f, 14.4f, 17.6f, 2.4f)) g.DrawPath(p, body);
            }
            using (Pen p = Stroke(c, 1.8f))
            {
                g.DrawLine(p, 8.2f, 8.2f, 15.8f, 8.2f);
                g.DrawLine(p, 8.2f, 12f, 15.8f, 12f);
                g.DrawLine(p, 8.2f, 15.8f, 12.6f, 15.8f);
            }
        }

        /// <summary>信号监视：示波器方波</summary>
        private static void DrawCheck(Graphics g, Color c)
        {
            using (Pen p = Stroke(c))
            {
                using (GraphicsPath frame = RR(3f, 5.2f, 18f, 13.6f, 2.4f)) g.DrawPath(p, frame);
            }
            using (Pen p = Stroke(c, 1.8f))
            {
                g.DrawLines(p, new[]
                {
                    P(5.2f, 12f), P(7.6f, 12f), P(7.6f, 8.6f), P(11.2f, 8.6f),
                    P(11.2f, 15.4f), P(14.8f, 15.4f), P(14.8f, 12f), P(18.8f, 12f)
                });
            }
        }

        /// <summary>用户设置：人 + 齿轮（与顶栏"用户登录"的纯人像区分开）</summary>
        private static void DrawAddUser(Graphics g, Color c)
        {
            using (Pen p = Stroke(c))
            {
                g.DrawEllipse(p, 5.6f, 4.4f, 6.4f, 6.4f);
                g.DrawArc(p, 3.2f, 11.4f, 11.2f, 10.4f, 180f, 180f);
            }
            using (Pen p = Stroke(c, 1.8f))
            {
                g.DrawEllipse(p, 15f, 15f, 6.4f, 6.4f);
                g.DrawEllipse(p, 17.4f, 17.4f, 1.6f, 1.6f);
                for (int i = 0; i < 6; i++)
                {
                    double a = i * Math.PI / 3.0;
                    float x1 = 18.2f + (float)(Math.Cos(a) * 3.2f);
                    float y1 = 18.2f + (float)(Math.Sin(a) * 3.2f);
                    float x2 = 18.2f + (float)(Math.Cos(a) * 4.7f);
                    float y2 = 18.2f + (float)(Math.Sin(a) * 4.7f);
                    g.DrawLine(p, x1, y1, x2, y2);
                }
            }
        }

        /// <summary>数据上传（MES）：向上箭头 + 托盘</summary>
        private static void DrawMes(Graphics g, Color c)
        {
            using (Pen p = Stroke(c))
            {
                g.DrawLine(p, 12f, 3.6f, 12f, 14.8f);
                g.DrawLines(p, new[] { P(7.6f, 8.2f), P(12f, 3.6f), P(16.4f, 8.2f) });
                g.DrawLines(p, new[] { P(4.4f, 14.4f), P(4.4f, 20.6f), P(19.6f, 20.6f), P(19.6f, 14.4f) });
            }
        }

        /// <summary>寿命：沙漏</summary>
        private static void DrawLifeSpan(Graphics g, Color c)
        {
            using (Pen p = Stroke(c))
            {
                g.DrawLine(p, 5.6f, 3.8f, 18.4f, 3.8f);
                g.DrawLine(p, 5.6f, 20.2f, 18.4f, 20.2f);
                using (GraphicsPath left = new GraphicsPath())
                {
                    left.AddBezier(7.2f, 3.8f, 7.2f, 9.6f, 12f, 9.8f, 12f, 12f);
                    left.AddBezier(12f, 12f, 12f, 14.2f, 7.2f, 14.4f, 7.2f, 20.2f);
                    g.DrawPath(p, left);
                }
                using (GraphicsPath right = new GraphicsPath())
                {
                    right.AddBezier(16.8f, 3.8f, 16.8f, 9.6f, 12f, 9.8f, 12f, 12f);
                    right.AddBezier(12f, 12f, 12f, 14.2f, 16.8f, 14.4f, 16.8f, 20.2f);
                    g.DrawPath(p, right);
                }
            }
        }

        // ---------- 机台操作（几何实心，移动端媒体键的通行做法） ----------

        /// <summary>运行：实心三角（描边 + 填充 = 圆角三角）</summary>
        private static void DrawRun(Graphics g, Color c)
        {
            PointF[] pts = { P(8.4f, 5.4f), P(19f, 12f), P(8.4f, 18.6f) };
            using (Pen p = new Pen(c, 4f) { LineJoin = LineJoin.Round })
                g.DrawPolygon(p, pts);
            using (SolidBrush b = new SolidBrush(c))
                g.FillPolygon(b, pts);
        }

        /// <summary>暂停：两条实心圆角竖条</summary>
        private static void DrawPause(Graphics g, Color c)
        {
            using (SolidBrush b = new SolidBrush(c))
            {
                using (GraphicsPath l = RR(7.2f, 5.2f, 3.6f, 13.6f, 1.8f)) g.FillPath(b, l);
                using (GraphicsPath r = RR(13.2f, 5.2f, 3.6f, 13.6f, 1.8f)) g.FillPath(b, r);
            }
        }

        /// <summary>停止：实心圆角方块</summary>
        private static void DrawStop(Graphics g, Color c)
        {
            using (SolidBrush b = new SolidBrush(c))
            using (GraphicsPath sq = RR(6.2f, 6.2f, 11.6f, 11.6f, 2.6f))
                g.FillPath(b, sq);
        }

        /// <summary>复位：顺时针环形箭头</summary>
        private static void DrawReset(Graphics g, Color c)
        {
            using (Pen p = Stroke(c))
            using (AdjustableArrowCap cap = new AdjustableArrowCap(1.7f, 1.7f, true))
            {
                p.CustomEndCap = cap;
                using (GraphicsPath arc = new GraphicsPath())
                {
                    arc.AddArc(4.8f, 4.8f, 14.4f, 14.4f, 62f, 284f);
                    g.DrawPath(p, arc);
                }
            }
        }

        /// <summary>系统设置：齿轮</summary>
        private static void DrawSystem(Graphics g, Color c)
        {
            using (Pen p = Stroke(c))
            {
                g.DrawEllipse(p, 5.4f, 5.4f, 13.2f, 13.2f);
                g.DrawEllipse(p, 9.6f, 9.6f, 4.8f, 4.8f);
            }
            using (Pen p = Stroke(c, 2.4f))
            {
                for (int i = 0; i < 8; i++)
                {
                    double a = i * Math.PI / 4.0;
                    float cos = (float)Math.Cos(a), sin = (float)Math.Sin(a);
                    g.DrawLine(p, 12f + cos * 6.5f, 12f + sin * 6.5f,
                                  12f + cos * 9.6f, 12f + sin * 9.6f);
                }
            }
        }

        /// <summary>锁定：挂锁</summary>
        private static void DrawLock(Graphics g, Color c)
        {
            using (Pen p = Stroke(c))
            {
                g.DrawArc(p, 8.4f, 5.2f, 7.2f, 7.2f, 180f, 180f);
                g.DrawLine(p, 8.4f, 8.8f, 8.4f, 11.4f);
                g.DrawLine(p, 15.6f, 8.8f, 15.6f, 11.4f);
                using (GraphicsPath body = RR(5f, 11.4f, 14f, 9f, 2.6f)) g.DrawPath(p, body);
            }
            using (SolidBrush b = new SolidBrush(c))
                g.FillEllipse(b, 10.8f, 14.4f, 2.4f, 2.4f);
        }

        /// <summary>报警复位：铃铛 + 红色斜线（表示"解除"）</summary>
        private static void DrawAlarmReset(Graphics g, Color c, Color accent)
        {
            using (Pen p = Stroke(c))
            {
                using (GraphicsPath bell = new GraphicsPath())
                {
                    bell.AddBezier(6.6f, 16.2f, 6.6f, 11.2f, 8.4f, 6.4f, 12f, 6.4f);
                    bell.AddBezier(12f, 6.4f, 15.6f, 6.4f, 17.4f, 11.2f, 17.4f, 16.2f);
                    bell.CloseFigure();
                    g.DrawPath(p, bell);
                }
                g.DrawLine(p, 5.2f, 16.2f, 18.8f, 16.2f);
                g.DrawArc(p, 10.2f, 16.2f, 3.6f, 3.6f, 0f, 180f);
            }
            using (Pen p = Stroke(accent, 2.2f))
                g.DrawLine(p, 5.6f, 20.2f, 18.4f, 3.8f);
        }

        /// <summary>退出：门框 + 向外箭头</summary>
        private static void DrawExit(Graphics g, Color c)
        {
            using (Pen p = Stroke(c))
            {
                g.DrawLines(p, new[] { P(10.6f, 3.8f), P(4.4f, 3.8f), P(4.4f, 20.2f), P(10.6f, 20.2f) });
                g.DrawLine(p, 8.8f, 12f, 20f, 12f);
                g.DrawLines(p, new[] { P(16f, 7.8f), P(20.2f, 12f), P(16f, 16.2f) });
            }
        }

        // ---------- 顶栏（白线，压在深蓝底上） ----------

        /// <summary>物料管理：等轴测包装盒</summary>
        private static void DrawProduct(Graphics g, Color c)
        {
            PointF top = P(12f, 3.2f);
            PointF upperRight = P(20.6f, 7.4f);
            PointF upperLeft = P(3.4f, 7.4f);
            PointF lowerRight = P(20.6f, 16.6f);
            PointF lowerLeft = P(3.4f, 16.6f);
            PointF bottom = P(12f, 21f);
            PointF center = P(12f, 12.2f);

            using (Pen p = Stroke(c))
            {
                p.LineJoin = LineJoin.Round;
                g.DrawPolygon(p, new[] { top, upperRight, lowerRight, bottom, lowerLeft, upperLeft });
                g.DrawLines(p, new[] { upperLeft, center, upperRight });
                g.DrawLine(p, center, bottom);
            }
        }

        /// <summary>用户登录：人像</summary>
        private static void DrawLogin(Graphics g, Color c)
        {
            using (Pen p = Stroke(c))
            {
                g.DrawEllipse(p, 8.1f, 3.6f, 7.8f, 7.8f);
                g.DrawArc(p, 4.4f, 13.4f, 15.2f, 14f, 180f, 180f);
            }
        }

        // ---------- 登录 / 用户管理（补充图标） ----------

        /// <summary>指纹：三条开口向下的同心弧 + 中央脊线（小尺寸下仍能读出"指纹"）</summary>
        private static void DrawFingerprint(Graphics g, Color c)
        {
            using (Pen p = Stroke(c))
            {
                g.DrawArc(p, 3.4f, 3.0f, 17.2f, 17.2f, 200f, 140f);
                g.DrawArc(p, 6.0f, 5.6f, 12f, 12f, 200f, 140f);
                g.DrawArc(p, 8.6f, 8.2f, 6.8f, 6.8f, 200f, 140f);
                g.DrawLine(p, 12f, 9.4f, 12f, 14.6f);
            }
            using (Pen p = Stroke(c, 1.8f))
            {
                g.DrawArc(p, 5.2f, 12.6f, 13.6f, 8.2f, 34f, 50f);
                g.DrawArc(p, 7.8f, 14.0f, 8.4f, 6.0f, 30f, 42f);
            }
        }

        /// <summary>关闭：一个叉</summary>
        private static void DrawClose(Graphics g, Color c)
        {
            using (Pen p = Stroke(c))
            {
                g.DrawLine(p, 6.6f, 6.6f, 17.4f, 17.4f);
                g.DrawLine(p, 17.4f, 6.6f, 6.6f, 17.4f);
            }
        }

        /// <summary>权限：盾牌 + 对勾</summary>
        private static void DrawShield(Graphics g, Color c)
        {
            using (Pen p = Stroke(c))
            {
                using (GraphicsPath shield = new GraphicsPath())
                {
                    shield.AddLines(new[] { P(12f, 3.2f), P(19.4f, 6.2f), P(19.4f, 12.2f) });
                    shield.AddBezier(19.4f, 12.2f, 19.4f, 16.4f, 16.6f, 19.2f, 12f, 20.8f);
                    shield.AddBezier(12f, 20.8f, 7.4f, 19.2f, 4.6f, 16.4f, 4.6f, 12.2f);
                    shield.CloseFigure();
                    g.DrawPath(p, shield);
                }
                g.DrawLines(p, new[] { P(8.9f, 12.1f), P(11.3f, 14.5f), P(15.6f, 9.3f) });
            }
        }

        /// <summary>新增：加号</summary>
        private static void DrawPlus(Graphics g, Color c)
        {
            using (Pen p = Stroke(c))
            {
                g.DrawLine(p, 12f, 5.4f, 12f, 18.6f);
                g.DrawLine(p, 5.4f, 12f, 18.6f, 12f);
            }
        }

        /// <summary>删除：垃圾桶</summary>
        private static void DrawTrash(Graphics g, Color c)
        {
            using (Pen p = Stroke(c))
            {
                g.DrawLine(p, 4.6f, 6.6f, 19.4f, 6.6f);
                g.DrawLines(p, new[] { P(9.4f, 6.6f), P(9.4f, 4.4f), P(14.6f, 4.4f), P(14.6f, 6.6f) });
                using (GraphicsPath body = RR(6.4f, 6.6f, 11.2f, 13.8f, 2.2f)) g.DrawPath(p, body);
            }
            using (Pen p = Stroke(c, 1.8f))
            {
                g.DrawLine(p, 10.2f, 10.4f, 10.2f, 16.8f);
                g.DrawLine(p, 13.8f, 10.4f, 13.8f, 16.8f);
            }
        }

        /// <summary>修改：铅笔</summary>
        private static void DrawEdit(Graphics g, Color c)
        {
            using (Pen p = Stroke(c))
            {
                p.LineJoin = LineJoin.Round;
                g.DrawPolygon(p, new[] { P(6.8f, 14.0f), P(15.6f, 5.2f), P(18.8f, 8.4f), P(10.0f, 17.2f) });
                g.DrawPolygon(p, new[] { P(6.8f, 14.0f), P(10.0f, 17.2f), P(4.4f, 19.6f) });
                g.DrawLine(p, 14.4f, 6.4f, 17.6f, 9.6f);
            }
        }

        /// <summary>虚拟键盘：圆角面板 + 两排按键点 + 空格条（手动调出 osk 的入口图标）。</summary>
        private static void DrawKeyboard(Graphics g, Color c)
        {
            using (Pen p = Stroke(c))
            {
                g.DrawPath(p, RR(2.5f, 6.5f, 19f, 11.5f, 2f));
            }
            using (SolidBrush b = new SolidBrush(c))
            {
                // 两排按键点（每排 4 个小圆点）
                for (int row = 0; row < 2; row++)
                {
                    float y = 9.2f + row * 3.2f;
                    for (int i = 0; i < 4; i++)
                    {
                        float x = 5.4f + i * 3.7f;
                        g.FillEllipse(b, x, y, 1.9f, 1.9f);
                    }
                }
                // 空格条
                g.FillRectangle(b, 8.6f, 14.9f, 6.8f, 1.6f);
            }
        }

        /// <summary>品牌标识：六边形 + 镜头（视觉检测的意象，登录页大尺寸使用）</summary>
        private static void DrawLogo(Graphics g, Color c)
        {
            using (Pen p = Stroke(c))
            {
                p.LineJoin = LineJoin.Round;
                g.DrawPolygon(p, new[]
                {
                    P(12f, 2.6f), P(20.4f, 7.4f), P(20.4f, 16.6f),
                    P(12f, 21.4f), P(3.6f, 16.6f), P(3.6f, 7.4f)
                });
                g.DrawEllipse(p, 8.0f, 8.0f, 8.0f, 8.0f);
            }
            using (SolidBrush b = new SolidBrush(c))
                g.FillEllipse(b, 10.6f, 10.6f, 2.8f, 2.8f);
        }
    }
}
