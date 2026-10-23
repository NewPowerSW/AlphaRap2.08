using System.Collections.Generic;
using System.Windows.Forms;

namespace AlphaRap.Configuration
{
    public static class HardwareText
    {
        public static readonly Dictionary<string,string[]> Words = new Dictionary<string,string[]>
        {
            { "AxisPort",new[]{"轴号","Axis number","Número de eje"} },
            { "IoPort",new[]{"IO 点号","IO port","Puerto de E/S"} },
            { "properties",new[]{"属性编辑","Edit properties","Editar propiedades"} },
            { "title",new[]{"硬件资源组合","Hardware resource groups","Grupos de recursos de hardware"} },
            { "addAxis",new[]{"新增轴","Add axis","Añadir eje"} }, { "addInput",new[]{"新增输入","Add input","Añadir entrada"} }, { "addOutput",new[]{"新增输出","Add output","Añadir salida"} },
            { "edit",new[]{"参数","Parameters","Parámetros"} }, { "delete",new[]{"删除资源","Delete resource","Eliminar recurso"} }, { "save",new[]{"保存资源定义","Save resource definitions","Guardar definiciones"} },
            { "hint", new[] { "双击标题或右键属性编辑，右键删除；配置统一使用主界面【保存】。轴点位随当前配方切换。新资源使用主界面【初始化】连接；已连接资源修改驱动参数后需保存并重启。", "Double-click titles or right-click to edit properties; right-click to delete. Use the main Save button. Axis points follow the recipe. Main Initialize connects new resources; restart after saving driver changes to connected hardware.", "Doble clic en el título o clic derecho para editar propiedades; clic derecho para eliminar. Use Guardar principal. Los puntos siguen la receta. Inicializar conecta recursos nuevos; reinicie tras guardar cambios de controlador." } },
            { "invalid",new[]{"请检查名称、地址、有限参数、速度及软限位。","Check name, address, finite parameters, speeds and soft limits.","Revise nombre, dirección, parámetros finitos, velocidades y límites."} },
            { "vendor",new[]{"轴仅支持当前 SDK 提供的运动板卡厂商。","Axes require a motion-card vendor supported by this SDK.","Los ejes requieren un fabricante de tarjeta de movimiento admitido."} },
            { "duplicate",new[]{"资源名称重复。","Duplicate resource name.","Nombre de recurso duplicado."} },
            { "portDuplicate",new[]{"物理通道已被其他控件使用：","The physical channel is already used by:","El canal físico ya está usado por:"} },
            { "portFormat",new[]{"雷赛轴号使用一位数字 0–9；凌华轴号使用三位数字（末位 0–3）；板卡 IO 使用五位数字（前三位卡号，后两位点号）。","Leadshine axis: one digit 0–9; Adlink axis: three digits (last 0–3); card IO: five digits (three for card, two for point).","Eje Leadshine: un dígito 0–9; eje Adlink: tres dígitos (último 0–3); E/S de tarjeta: cinco dígitos (tres de tarjeta, dos de punto)."} },
            { "DeviceType",new[]{"基恩士软元件类型","Keyence device type","Tipo de dispositivo Keyence"} }, { "DataFormat",new[]{"基恩士数据格式","Keyence data format","Formato de datos Keyence"} },
            { "missing",new[]{"请先选择资源。","Select a resource first.","Seleccione primero un recurso."} },
            { "pendingRecipe",new[]{"当前轴点位有未保存修改，请先使用主界面【保存】；选择否可放弃修改。","Axis points have unsaved edits. Use the main Save button first; choose No to discard.","Hay puntos sin guardar. Use Guardar principal; elija No para descartar."} },
            { "duplicatePath",new[]{"配置文件路径重复，请检查模块身份。","Duplicate configuration file path. Check module identities.","Ruta de configuración duplicada. Revise la identidad de los módulos."} },
            { "noFreePort",new[]{"当前驱动的通道已用满，请先释放通道或配置其他板卡。","No free channel is available. Release a channel or configure another card.","No hay un canal libre. Libere un canal o configure otra tarjeta."} },
            { "loadError",new[]{"硬件资源文件加载失败，请恢复备份后重启，当前文件不会被覆盖。","Hardware definitions failed to load. Restore the backup and restart; the file will not be overwritten.","No se cargaron las definiciones. Restaure la copia y reinicie; no se sobrescribirá el archivo."} },
            { "saveFirst", new[] { "硬件资源有未保存修改，请先点击主界面【保存】。", "Hardware resources have unsaved edits. Click the main Save button first.", "Hay recursos sin guardar. Pulse Guardar principal primero." } },
            { "restart", new[] { "已连接硬件的参数有变更，请点击主界面【保存】并重启后运行。", "Connected hardware parameters changed. Use the main Save button and restart before running.", "Cambió el hardware conectado. Use Guardar principal y reinicie antes de ejecutar." } },
            { "notReady",new[]{"硬件初始化未就绪。","Hardware initialization is not ready.","La inicialización del hardware no está lista."} },
            { "referenced",new[]{"以下节点仍绑定该资源或点位参数，不能删除：{0}","These nodes still bind this resource or its point parameters; deletion is blocked: {0}","Estos nodos aún usan el recurso o sus puntos; no se permite eliminar: {0}"} },
            { "remove", new[] { "删除资源“{0}”？点击主界面【保存】后下次启动不再创建该组合，历史配方点位保留。", "Delete resource '{0}'? Use the main Save button to keep it removed after restart. Historical recipe points remain.", "¿Eliminar «{0}»? Use Guardar principal para mantenerlo eliminado tras reiniciar. Se conservan los puntos históricos." } },
            { "saved",new[]{"资源定义已保存。点位请使用主界面【保存】。","Definitions saved. Use the main Save button to save points.","Definiciones guardadas. Use Guardar principal para guardar puntos."} },
            { "property",new[]{"参数","Parameter","Parámetro"} }, { "value",new[]{"值","Value","Valor"} },
            { "Name",new[]{"资源名称","Resource name","Nombre del recurso"} }, { "Vendor",new[]{"厂商","Vendor","Fabricante"} }, { "Port",new[]{"轴号 / IO 点号","Axis / IO port","Puerto de eje / E/S"} },
            { "WorkSpeed",new[]{"运行速度","Work speed","Velocidad de trabajo"} }, { "ManualSpeed",new[]{"手动速度","Manual speed","Velocidad manual"} }, { "MaxSpeed",new[]{"最大速度","Maximum speed","Velocidad máxima"} },
            { "SpeedRatio",new[]{"速度比例 (%)","Speed ratio (%)","Porcentaje de velocidad (%)"} }, { "Tacc",new[]{"加速时间 (s)","Acceleration (s)","Aceleración (s)"} }, { "Tdec",new[]{"减速时间 (s)","Deceleration (s)","Deceleración (s)"} },
            { "PulseEquivalent",new[]{"指令脉冲当量","Command pulse equivalent","Equivalente de pulsos de mando"} }, { "EncoderPulseEquivalent",new[]{"编码器脉冲当量","Encoder pulse equivalent","Equivalente de pulsos del codificador"} },
            { "HomeMode",new[]{"回零模式代码","Home mode code","Código de modo de origen"} }, { "HomeDir",new[]{"回零方向","Home direction","Dirección de origen"} },
            { "HomeHighSpeed",new[]{"回零高速","Home high speed","Velocidad alta de origen"} }, { "HomeLowSpeed",new[]{"回零低速","Home low speed","Velocidad baja de origen"} }, { "HomeOffset",new[]{"回零偏移","Home offset","Desplazamiento de origen"} },
            { "LowLimit",new[]{"软限位下限","Soft lower limit","Límite inferior"} }, { "HighLimit",new[]{"软限位上限","Soft upper limit","Límite superior"} },
            { "PlcIp",new[]{"PLC IP","PLC IP","IP del PLC"} }, { "PlcPort",new[]{"PLC TCP 端口","PLC TCP port","Puerto TCP del PLC"} },
            { "Address",new[]{"基恩士地址","Keyence address","Dirección Keyence"} }, { "OmronAddress",new[]{"欧姆龙地址","Omron address","Dirección Omron"} }, { "OmronPort",new[]{"欧姆龙 CIP 端口","Omron CIP port","Puerto CIP Omron"} },
            { "ReverseState",new[]{"反转有效电平","Reverse active state","Invertir estado activo"} }
        };

        /// <summary>读取统一三语底稿并尊重现场语言包。</summary>
        public static string T(string key)
        {
            string[] words;
            return Words.TryGetValue(key,out words) ? Translate(key,words[0],words[1],words[2]) : key;
        }

        /// <summary>给 NPSDK 组合控件提供宿主语言桥，不改 CurrentCulture。</summary>
        public static string Translate(string key,string zh,string en,string es) { return UiTheme.IsDesignTime ? zh : MiddleLayer.LangMsg("HardwareResources",key,zh,en,es); }

        /// <summary>按固定 Name 更新标签与表头，不重建参数行。</summary>
        public static void Apply(Control parent)
        {
            string[] words;
            string key = parent.Tag as string;
            if (key != null && Words.TryGetValue(key,out words)) parent.Text = Translate(parent.Name,words[0],words[1],words[2]);
            DataGridView grid = parent as DataGridView;
            if (grid != null)
                foreach (DataGridViewColumn column in grid.Columns)
                    if (Words.TryGetValue(column.Tag as string ?? "",out words)) column.HeaderText = Translate(grid.Name+"_"+column.Name,words[0],words[1],words[2]);
            foreach (Control child in parent.Controls) Apply(child);
        }
    }
}
