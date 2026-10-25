using System.Threading.Tasks;
using NPSDK;

namespace AlphaRap.FlowProgramming.Modules
{
    public sealed class InspectAtPointInputs
    {
        [FlowCodeField("检测点位", "Inspection point", "Punto de inspección", DefaultValue = "0", ResourceSlot = "Positioning")]
        public int Point { get; set; }
        [FlowCodeField("高度下限", "Minimum height", "Altura mínima", DefaultValue = "0")]
        public double MinimumHeight { get; set; }
        [FlowCodeField("高度上限", "Maximum height", "Altura máxima", DefaultValue = "10")]
        public double MaximumHeight { get; set; }
    }

    public sealed class InspectAtPointOutputs
    {
        [FlowCodeField("到位位置", "Arrival position", "Posición de llegada")]
        public double Position { get; set; }
        [FlowCodeField("检测高度", "Measured height", "Altura medida")]
        public double Height { get; set; }
        [FlowCodeField("视觉运行编号", "Vision run ID", "ID de ejecución de visión")]
        public long VisionRunId { get; set; }
        [FlowCodeField("合格", "Accepted", "Aceptado")]
        public bool Accepted { get; set; }
    }

    [FlowCodeModule("inspection.at-point", "code", "到位后视觉测高", "Inspect height at a point", "Medir altura en un punto", Exits = "NEXT,CASE1")]
    [FlowCodeResource("Positioning", "axis", "定位轴", "Positioning axis", "Eje de posición")]
    [FlowCodeResource("Present", "input", "产品到位输入", "Product present input", "Entrada de producto presente")]
    [FlowCodeResource("Inspection", "vision", "检测 VPP", "Inspection VPP", "VPP de inspección")]
    [FlowCodeResourceField("Inspection", "Height", typeof(double), "高度输出", "Height output", "Salida de altura")]
    public sealed class InspectAtPoint : FlowCodeNode<InspectAtPointInputs, InspectAtPointOutputs>
    {
        /// <summary>等待产品到位、移动到绑定点位后测高，输出独立结果供后续节点读取。</summary>
        protected override async Task<Flow_Chart.ResultType> ExecuteAsync(FlowCodeContext context, InspectAtPointInputs inputs, InspectAtPointOutputs outputs)
        {
            await context.Input("Present").WaitAsync(true);
            outputs.Position = await context.Axis("Positioning").MoveToAsync(inputs.Point);
            FlowCodeVisionResult inspection = await context.Vision("Inspection").RunAsync();
            outputs.VisionRunId = inspection.RunId;
            outputs.Height = inspection.Get<double>("Height");
            outputs.Accepted = inspection.Accepted && inputs.MinimumHeight <= inputs.MaximumHeight
                && outputs.Height >= inputs.MinimumHeight && outputs.Height <= inputs.MaximumHeight;
            return outputs.Accepted ? Flow_Chart.ResultType.NEXT : Flow_Chart.ResultType.CASE1;
        }
    }
}
