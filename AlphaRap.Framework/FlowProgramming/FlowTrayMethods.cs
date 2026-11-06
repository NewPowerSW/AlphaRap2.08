using System;
using System.Collections.Generic;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    public sealed partial class FlowTray : IFlowObject
    {
        /// <summary>托盘对象按自己的行列与间距计算穴位，不访问硬件。</summary>
        public IEnumerable<FlowMethod> GetFlowMethods(bool includeDraft = false)
        {
            yield return FlowMethods.Sync("tray.position", "计算托盘穴位偏移", "Calculate tray pocket offset", "Calcular desplazamiento de bandeja",
                new[] { new FlowField("Index", typeof(int), "0") { DisplayName = new[] { "穴位序号", "Pocket index", "Índice de cavidad" },
                    Help = new[] { "从 0 开始，先按列递增，再进入下一行；只计算相对托盘起点的偏移。", "Starts at 0, advancing through columns before the next row; calculates only the offset from the tray origin.", "Empieza en 0 y avanza por columnas antes de pasar a la siguiente fila; solo calcula el desplazamiento desde el origen de la bandeja." } } },
                new[] { new FlowField("Row", typeof(int)) { DisplayName = new[] { "行序号", "Row index", "Índice de fila" } },
                    new FlowField("Column", typeof(int)) { DisplayName = new[] { "列序号", "Column index", "Índice de columna" } },
                    new FlowField("OffsetX", typeof(double)) { DisplayName = new[] { "X 方向偏移", "X offset", "Desplazamiento X" } },
                    new FlowField("OffsetY", typeof(double)) { DisplayName = new[] { "Y 方向偏移", "Y offset", "Desplazamiento Y" } } }, call => {
                    int index = call.Get<int>("Index");
                    if (Rows < 1 || Columns < 1 || index < 0 || index >= (long)Rows * Columns) throw new InvalidOperationException(FlowText.T("error.trayIndex"));
                    int row = index / Columns, column = index % Columns;
                    return new Dictionary<string, object> { { "Row", row }, { "Column", column }, { "OffsetX", column * ColumnPitch }, { "OffsetY", row * RowPitch } };
                }, false);
        }
    }
}
