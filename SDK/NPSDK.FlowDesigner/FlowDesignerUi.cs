using System.Windows.Forms;

namespace NPSDK.Design
{
    public static class FlowDesignerUi
    {
        /// <summary>按宿主语言机制刷新静态控件和表头，保留业务值。</summary>
        public static void Apply(Control root, FlowDesignerEnvironment environment)
        {
            if (environment.RegisterLanguage != null)
                environment.RegisterLanguage(root);
            ApplyCore(root, environment);
        }

        /// <summary>只更新可翻译展示层，不遍历 PropertyGrid 内部编辑控件。</summary>
        private static void ApplyCore(Control root, FlowDesignerEnvironment environment)
        {
            if (root is Flow_Chart)
                return;
            if (root is Form || root is Button || root is Label || root is TabPage)
                if (!string.IsNullOrEmpty(root.Name) && !root.Name.EndsWith("_dyn"))
                    root.Text = environment.T(root.Name);
            var grid = root as DataGridView;
            if (grid != null)
            {
                foreach (DataGridViewColumn column in grid.Columns)
                    column.HeaderText = environment.T(grid.Name + "_" + column.Name);
                return;
            }

            if (root is PropertyGrid || root is TextBoxBase || root is ComboBox || root is ListBox)
                return;
            foreach (Control child in root.Controls)
                ApplyCore(child, environment);
        }
    }
}
