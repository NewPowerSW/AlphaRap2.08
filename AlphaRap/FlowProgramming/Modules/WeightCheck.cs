using NPSDK;

namespace AlphaRap.FlowProgramming.Modules
{
    public sealed class WeightCheckInputs
    {
        [FlowCodeField("实测重量", "Measured weight", "Peso medido", Unit = "g")]
        public double Weight { get; set; }
        [FlowCodeField("重量下限", "Minimum weight", "Peso mínimo", DefaultValue = "0", Unit = "g")]
        public double Minimum { get; set; }
        [FlowCodeField("重量上限", "Maximum weight", "Peso máximo", DefaultValue = "10", Unit = "g")]
        public double Maximum { get; set; }
    }

    public sealed class WeightCheckOutputs
    {
        [FlowCodeField("重量", "Weight", "Peso", Unit = "g")]
        public double Weight { get; set; }
        [FlowCodeField("合格", "Accepted", "Aceptado")]
        public bool Accepted { get; set; }
    }

    [FlowCodeModule("quality.weight-check", "code", "重量判定", "Weight check", "Evaluar peso", Exits = "NEXT,CASE1")]
    public sealed class WeightCheck : FlowCodeNode<WeightCheckInputs, WeightCheckOutputs>
    {
        /// <summary>判断输入重量是否处于闭区间；合格走 NEXT，不合格走 CASE1。</summary>
        protected override Flow_Chart.ResultType Execute(FlowCodeContext context, WeightCheckInputs inputs, WeightCheckOutputs outputs)
        {
            outputs.Weight = inputs.Weight;
            outputs.Accepted = inputs.Minimum <= inputs.Maximum && inputs.Weight >= inputs.Minimum && inputs.Weight <= inputs.Maximum;
            return outputs.Accepted ? Flow_Chart.ResultType.NEXT : Flow_Chart.ResultType.CASE1;
        }
    }
}
