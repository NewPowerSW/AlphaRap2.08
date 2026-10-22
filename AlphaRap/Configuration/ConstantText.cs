using System.Collections.Generic;
using System.Windows.Forms;

namespace AlphaRap.Configuration
{
    public static class ConstantText
    {
        public static readonly Dictionary<string, string[]> Words = new Dictionary<string, string[]>
        {
            { "title", new[] { "设计器常量", "Designer constants", "Constantes del diseñador" } },
            { "Setting", new[] { "Setting · 机器常量", "Setting · Machine constants", "Setting · Constantes de máquina" } },
            { "Recipe", new[] { "Recipe · 当前配方常量", "Recipe · Current recipe constants", "Recipe · Constantes de receta actual" } },
            { "add", new[] { "新增", "Add", "Añadir" } }, { "edit", new[] { "修改", "Edit", "Editar" } },
            { "delete", new[] { "删除", "Delete", "Eliminar" } }, { "save", new[] { "保存当前表", "Save current table", "Guardar tabla actual" } },
            { "search", new[] { "查询键或说明", "Search key or description", "Buscar clave o descripción" } },
            { "key", new[] { "常量键", "Constant key", "Clave de constante" } }, { "type", new[] { "类型", "Type", "Tipo" } },
            { "value", new[] { "值", "Value", "Valor" } }, { "description", new[] { "说明", "Description", "Descripción" } },
            { "Boolean", new[] { "布尔", "Boolean", "Booleano" } }, { "Integer", new[] { "整数", "Integer", "Entero" } },
            { "Number", new[] { "数值", "Number", "Número" } }, { "Text", new[] { "文本", "Text", "Texto" } },
            { "hint", new[] { "Setting 不随配方切换；Recipe 跟随物料管理的当前配方。修改后使用主界面【保存】。流程节点的“型号 / 机器参数”可绑定 Constants。", "Setting is independent of recipes; Recipe follows product management. Use the main Save button after editing. Bind Constants under Recipe / Machine parameter in flow nodes.", "Setting no cambia con la receta; Recipe sigue la gestión de productos. Use Guardar principal tras editar. Vincule Constants en Parámetro de receta / máquina del nodo." } },
            { "context", new[] { "当前配方：{0}", "Current recipe: {0}", "Receta actual: {0}" } },
            { "duplicate", new[] { "该作用域已有同名常量。", "A constant with this key already exists in this scope.", "Ya existe una constante con esta clave en este ámbito." } },
            { "missing", new[] { "常量已删除或不存在，请重新选择。", "The constant is unavailable. Select it again.", "La constante no está disponible. Vuelva a seleccionarla." } },
            { "legacy", new[] { "历史参数 DisScann 和 UseTryRun 保留键和布尔类型，可修改值。", "Legacy keys DisScann and UseTryRun must keep their names and Boolean type; their values can be edited.", "Las claves heredadas DisScann y UseTryRun conservan su nombre y tipo Boolean; sus valores se pueden editar." } },
            { "keyRequired", new[] { "请输入无首尾空格和控制字符的常量键。", "Enter a constant key without leading/trailing spaces or control characters.", "Introduzca una clave sin espacios al inicio o final ni caracteres de control." } },
            { "invalidValue", new[] { "值与类型不符。布尔使用 True/False，数值小数点使用句点且必须有限。", "Value does not match the type. Use True/False for Boolean and a decimal point for finite numbers.", "El valor no coincide con el tipo. Use True/False para Boolean y punto decimal para números finitos." } },
            { "recipeRequired", new[] { "请先通过物料管理创建并使用一个有效配方。", "Create and select a valid recipe in product management first.", "Cree y seleccione primero una receta válida en gestión de productos." } },
            { "remove", new[] { "删除常量“{0}”？保存后生效。", "Delete constant '{0}'? Save to apply.", "¿Eliminar la constante «{0}»? Guarde para aplicar." } },
            { "saved", new[] { "{0} 常量已保存。", "{0} constants saved.", "Constantes de {0} guardadas." } },
            { "pendingRecipe", new[] { "当前配方常量有未保存修改，是否保存后再切换？", "Save changes to current recipe constants before switching?", "¿Guardar cambios de las constantes de la receta antes de cambiar?" } },
            { "pendingMainSave", new[] { "当前配方常量有未保存修改，请先使用主界面【保存】；选择否可放弃修改。", "Current recipe constants have unsaved edits. Use the main Save button first; choose No to discard.", "Hay constantes sin guardar. Use Guardar principal; elija No para descartar." } },
            { "referenced", new[] { "以下流程节点仍引用该参数，不能删除、改键或改变类型：{0}", "These flow nodes still reference this parameter; it cannot be deleted, renamed or retyped: {0}", "Estos nodos aún usan el parámetro; no puede eliminarse ni cambiar su clave o tipo: {0}" } },
            { "DisScann", new[] { "禁用扫码枪", "Disable scanner", "Desactivar escáner" } },
            { "UseTryRun", new[] { "使用空跑", "Dry run", "Ejecución en vacío" } },
            { "DisVision", new[] { "禁用视觉", "Disable vision", "Desactivar visión" } },
            { "DisMes", new[] { "禁用 MES", "Disable MES", "Desactivar MES" } },
            { "DisDoor", new[] { "禁用门禁", "Disable door check", "Desactivar control de puerta" } },
            { "DisRecheck", new[] { "禁用复检", "Disable recheck", "Desactivar reinspección" } },
            { "DisBuzzer", new[] { "禁用蜂鸣器", "Disable buzzer", "Desactivar zumbador" } },
            { "Dis3D", new[] { "禁用 3D", "Disable 3D", "Desactivar 3D" } }
        };

        /// <summary>使用框架统一语言入口及现场覆盖，设计期只读内置底稿。</summary>
        public static string T(string key)
        {
            string[] value;
            if (!Words.TryGetValue(key, out value)) return key;
            return UiTheme.IsDesignTime ? value[0] : MiddleLayer.LangMsg("DesignerConstants", key, value[0], value[1], value[2]);
        }

        /// <summary>按固定控件 Name 和表头登记文字，不重建绑定数据。</summary>
        public static void Apply(Control control)
        {
            string[] words;
            Form form = control as Form ?? control.FindForm();
            string scope = form is SystemSetting ? "SystemSetting" : "DesignerConstants";
            string key = control.Tag as string;
            if (key != null && Words.TryGetValue(key, out words)) control.Text = UiTheme.IsDesignTime ? words[0] : MiddleLayer.LangMsg(scope, control.Name, words[0], words[1], words[2]);
            DataGridView grid = control as DataGridView;
            if (grid != null)
                foreach (DataGridViewColumn column in grid.Columns)
                    if (Words.TryGetValue(column.Tag as string ?? "", out words)) column.HeaderText = UiTheme.IsDesignTime ? words[0] : MiddleLayer.LangMsg(scope, grid.Name + "_" + column.Name, words[0], words[1], words[2]);
            foreach (Control child in control.Controls) Apply(child);
        }
    }
}
