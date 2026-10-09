using System;
using System.Drawing;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>
    /// 界面操作日志：谁（工号 / 权限）在什么时候、在哪个界面、做了什么。
    ///
    /// 落盘复用 DataForm 的 DataSave 机制（见 <see cref="DataForm.AddOperationLog"/>），
    /// 每个界面一个目录：D:\操作日志\{界面名}\{年}\{月}\{yyyy-MM-dd}.csv，
    /// 列：Time（库自动填） / UserID（工号） / UserPermission（权限） / Action（做了什么）。
    ///
    /// 记录来源两条：
    ///   ① <see cref="InstallFilter"/>：全局鼠标消息过滤器 —— 点任何按钮 / 勾选 / 单选 / 图标 / 页签都记一条。
    ///      用过滤器而不是逐个控件挂事件，是因为运行时才建出来的控件（VPForm 的相机页、VPP 页等）也自动覆盖。
    ///   ② <see cref="Write"/>：关键业务操作（登录、保存、换配方、增删相机 / VPP / 标定…）再补一条更清楚的记录。
    /// </summary>
    public static class OperationLog
    {
        /// <summary>日志根目录；其下按界面名分目录。</summary>
        public const string RootPath = @"D:\操作日志";

        private static ClickLogFilter _filter;

        /// <summary>写一条操作记录；<paramref name="formName"/> 同时用作分目录名。</summary>
        public static void Write(string formName, string action)
        {
            if (string.IsNullOrEmpty(action)) return;
            try
            {
                if (MiddleLayer.DataF != null) MiddleLayer.DataF.AddOperationLog(formName, action);
            }
            catch (Exception) { }
        }

        /// <summary>装上"点击即记录"的全局过滤器（幂等；在 UI 线程调用一次）。</summary>
        public static void InstallFilter()
        {
            if (_filter != null) return;
            try
            {
                _filter = new ClickLogFilter();
                Application.AddMessageFilter(_filter);
            }
            catch (Exception) { }
        }

        // ==================== 内部实现 ====================

        private const int WM_LBUTTONDOWN = 0x0201;

        /// <summary>鼠标左键按下时定位被点的控件并记一条；只做记录，不拦截消息。</summary>
        private sealed class ClickLogFilter : IMessageFilter
        {
            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg != WM_LBUTTONDOWN) return false;
                try
                {
                    Control hit = Control.FromChildHandle(m.HWnd);
                    if (hit == null) return false;

                    Control target = ResolveTarget(hit);
                    if (target == null) return false;

                    Form form = target.FindForm();
                    string formName = (form != null && !string.IsNullOrEmpty(form.Name))
                                    ? form.Name : target.Name;

                    if (target is TabControl)
                    {
                        string tabName = TabUnderMouse((TabControl)target);
                        if (!string.IsNullOrEmpty(tabName)) Write(formName, "切换页签「" + tabName + "」");
                        return false;
                    }

                    // 延后到本次消息处理完再取文字/勾选状态（勾选框此刻才翻转），顺便不拖慢点击响应
                    if (target.IsHandleCreated && !target.IsDisposed)
                        target.BeginInvoke((MethodInvoker)delegate { Write(formName, DescribeClick(target)); });
                }
                catch (Exception) { }
                return false;
            }
        }

        /// <summary>从被点中的控件往上找，返回真正该记录的控件（页签 / 按钮 / 勾选 / 图标）。</summary>
        private static Control ResolveTarget(Control hit)
        {
            for (Control c = hit; c != null; c = c.Parent)
            {
                if (c is TabControl) return c;
                if (IsClickable(c)) return c;
                if (c is Form) return null;      // 一路走到窗体说明点的是空白处
            }
            return null;
        }

        private static bool IsClickable(Control c)
        {
            if (c is ButtonBase) return true;    // Button / CheckBox / RadioButton（含 UiButton / UiCheckBox）
            if (c is FlatButton) return true;    // 自绘按钮，直接继承 Control
            if (c is AlarmChip) return true;
            if (c is LanguageSwitch) return true;
            if (c is PictureBox) return true;    // 导航 / 工具条图标
            if (c is LinkLabel) return true;
            return false;
        }

        private static string DescribeClick(Control c)
        {
            string name = DisplayName(c);
            CheckBox cb = c as CheckBox;
            if (cb != null) return "点击「" + name + "」→ " + (cb.Checked ? "选中" : "取消选中");
            RadioButton rb = c as RadioButton;
            if (rb != null) return "点击「" + name + "」→ " + (rb.Checked ? "选中" : "取消选中");
            return "点击「" + name + "」";
        }

        /// <summary>控件在界面上的名字：优先控件文字，其次语言包（图标类控件没有文字），最后控件名。</summary>
        private static string DisplayName(Control c)
        {
            string s = null;
            try { s = c.Text; } catch (Exception) { }
            if (string.IsNullOrEmpty(s))
            {
                try
                {
                    Form f = c.FindForm();
                    if (f != null) s = MiddleLayer.LangText(f.Name, c.Name, null);
                }
                catch (Exception) { }
            }
            if (string.IsNullOrEmpty(s)) s = c.Name;
            if (string.IsNullOrEmpty(s)) s = c.GetType().Name;
            return s;
        }

        /// <summary>鼠标落在哪个页签上（点击时选中页还没切，只能按坐标取）。</summary>
        private static string TabUnderMouse(TabControl tc)
        {
            try
            {
                Point p = tc.PointToClient(Control.MousePosition);
                for (int i = 0; i < tc.TabCount; i++)
                    if (tc.GetTabRect(i).Contains(p)) return tc.TabPages[i].Text;
            }
            catch (Exception) { }
            return null;
        }
    }
}
