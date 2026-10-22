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
                new[] { new FlowField("Index", typeof(int), "0") }, new[] { new FlowField("Row", typeof(int)), new FlowField("Column", typeof(int)),
                    new FlowField("OffsetX", typeof(double)), new FlowField("OffsetY", typeof(double)) }, call => {
                    int index = call.Get<int>("Index");
                    if (Rows < 1 || Columns < 1 || index < 0 || index >= (long)Rows * Columns) throw new InvalidOperationException(FlowText.T("error.trayIndex"));
                    int row = index / Columns, column = index % Columns;
                    return new Dictionary<string, object> { { "Row", row }, { "Column", column }, { "OffsetX", column * ColumnPitch }, { "OffsetY", row * RowPitch } };
                }, false);
        }
    }
}
