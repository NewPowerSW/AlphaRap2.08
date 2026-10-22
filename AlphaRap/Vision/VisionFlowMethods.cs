using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Cognex.VisionPro.ToolBlock;
using NPSDK;
using AlphaRap.FlowProgramming;

namespace AlphaRap
{
    public abstract partial class VisionproInterface : IFlowObject
    {
        private long flowRunId;
        /// <summary>按当前 VPP 的真实端口声明输入、输出和运行能力，不运行算法。</summary>
        public virtual IEnumerable<FlowMethod> GetFlowMethods(bool includeDraft = false)
        {
            FlowMethod run = FlowMethods.Sync("vision.run", "拍照并运行当前 VPP", "Capture and run this VPP", "Capturar y ejecutar este VPP", new FlowField[0],
                new[] { new FlowField("RunId", typeof(long)), new FlowField("Accepted", typeof(bool)), new FlowField("X", typeof(double)),
                    new FlowField("Y", typeof(double)), new FlowField("Angle", typeof(double)) }, call => null);
            lock (_operationLock)
            {
                if (TB != null)
                {
                    foreach (CogToolBlockTerminal terminal in TB.Inputs)
                        run.Inputs.Add(new FlowField("TB:" + terminal.Name, terminal.ValueType, Convert.ToString(terminal.Value, CultureInfo.InvariantCulture)));
                    foreach (CogToolBlockTerminal terminal in TB.Outputs) run.Outputs.Add(new FlowField("TB:" + terminal.Name, terminal.ValueType));
                }
            }
            run.Invoke = RunFlowVision;
            yield return run;
            FlowField output = new FlowField("Output", typeof(string));
            output.Options.AddRange(run.Outputs.Where(field => field.Name.StartsWith("TB:")).Select(field => new FlowOption { Value = field.Name.Substring(3), Label = field.Name.Substring(3) }));
            if (output.Options.Count == 0) yield break;
            output.DefaultValue = output.Options[0].Value;
            yield return FlowMethods.Sync("vision.output", "读取指定运行的 VPP 输出", "Read a VPP run output", "Leer salida de una ejecución VPP",
                new[] { new FlowField("RunId", typeof(long)), output }, new[] { new FlowField("Value", typeof(object)) }, call => {
                    lock (_operationLock) { RequireFlowResult(call.Get<long>("RunId")); return FlowCall.Result("Value", GetOutput(call.Get<string>("Output"))); }
                });
        }

        /// <summary>一次设置输入并启动算法；返回本次结果，NG 留给节点条件判定，执行错误抛出异常。</summary>
        private async Task<IDictionary<string, object>> RunFlowVision(FlowCall call)
        {
            long id;
            lock (_operationLock)
            {
                if (IsBusy || !IsLoadTBOk || TB == null) throw new InvalidOperationException(FlowText.T("error.visionBusy") + StationName);
                foreach (KeyValuePair<string, object> input in call.Inputs) TB.Inputs[input.Key.Substring(3)].Value = input.Value;
                if (!TryRunTB(out id)) throw new InvalidOperationException(LastError);
                flowRunId = id;
            }
            try
            {
                while (IsBusy) await Task.Delay(20, call.Token).ConfigureAwait(false);
                call.Token.ThrowIfCancellationRequested();
                lock (_operationLock)
                {
                    VisionRunResult result = RequireFlowResult(id);
                    Dictionary<string, object> output = new Dictionary<string, object> { { "RunId", id }, { "Accepted", result.State == VisionRunState.Succeeded },
                        { "X", result.X }, { "Y", result.Y }, { "Angle", result.Angle } };
                    foreach (CogToolBlockTerminal terminal in TB.Outputs) output.Add("TB:" + terminal.Name, terminal.Value);
                    return output;
                }
            }
            catch
            {
                CancelRun(id);
                while (IsBusy) await Task.Delay(20).ConfigureAwait(false);
                throw;
            }
        }

        /// <summary>只允许读取已完成的同一请求，防止重拍后保存或读取其他产品的结果。</summary>
        protected VisionRunResult RequireFlowResult(long id)
        {
            VisionRunResult result = LastRunResult;
            if (IsBusy || result == null || result.RunId != id || (result.State != VisionRunState.Succeeded && result.State != VisionRunState.Rejected))
                throw new InvalidOperationException(result == null ? FlowText.T("error.visionResult") : FlowText.T("error.visionResultDetail") + result.Error);
            return result;
        }
    }

    public partial class VpStation
    {
        private VpVppConfig flowSavedConfig;
        private string flowSavedCamera;
        private double flowSavedExposure;

        /// <summary>磁盘加载或主保存成功后记录生产配置，后续编辑保持为草稿。</summary>
        internal void AcceptFlowConfiguration()
        {
            flowSavedConfig = CopyFlowConfiguration(Config);
            flowSavedCamera = RunLiveCCDSN; flowSavedExposure = RunExposure;
        }

        /// <summary>复制本 VPP 的补偿与存图参数，不复制运行对象。</summary>
        private static VpVppConfig CopyFlowConfiguration(VpVppConfig config)
        {
            if (config == null) return null;
            return new VpVppConfig { Id = config.Id, Name = config.Name, FolderName = config.FolderName, Exposure = config.Exposure,
                SaveImageEnabled = config.SaveImageEnabled, SaveImagePath = config.SaveImagePath, SaveImageOriginal = config.SaveImageOriginal,
                SaveImageSnapshot = config.SaveImageSnapshot, SaveImageAutoDelete = config.SaveImageAutoDelete, SaveImageKeepDays = config.SaveImageKeepDays,
                CompLimits = config.CompLimits.Where(item => item != null).Select(item => new VpCompLimitItem(item.Item, item.Min, item.Max) { Compensation = item.Compensation }).ToList() };
        }

        /// <summary>生产调用前拒绝未保存的相机绑定和 VPP 参数，避免编辑值直接进入运行。</summary>
        private void RequireSavedFlowConfiguration()
        {
            if (Config == null) return;
            VpVppConfig saved = flowSavedConfig;
            if (saved == null || flowSavedCamera != RunLiveCCDSN || flowSavedExposure != RunExposure || saved.Exposure != Config.Exposure ||
                saved.SaveImageEnabled != Config.SaveImageEnabled || saved.SaveImagePath != Config.SaveImagePath || saved.SaveImageOriginal != Config.SaveImageOriginal ||
                saved.SaveImageSnapshot != Config.SaveImageSnapshot || saved.SaveImageAutoDelete != Config.SaveImageAutoDelete || saved.SaveImageKeepDays != Config.SaveImageKeepDays ||
                saved.CompLimits.Count != Config.CompLimits.Count || saved.CompLimits.Where((item, index) => item.Item != Config.CompLimits[index].Item ||
                    item.Compensation != Config.CompLimits[index].Compensation || item.Min != Config.CompLimits[index].Min || item.Max != Config.CompLimits[index].Max).Any())
                throw new InvalidOperationException(FlowText.T("visionDraft"));
        }

        /// <summary>声明本相机下本 VPP 的存图与补偿表读取方法，补偿不会自动写入运动指令。</summary>
        public override IEnumerable<FlowMethod> GetFlowMethods(bool includeDraft = false)
        {
            foreach (FlowMethod method in base.GetFlowMethods(includeDraft))
            {
                if (!includeDraft) method.ValidateInputs = inputs => RequireSavedFlowConfiguration();
                yield return method;
            }
            VpVppConfig settings = CopyFlowConfiguration(includeDraft ? Config : flowSavedConfig);
            if (settings == null) yield break;
            yield return FlowMethods.Sync("vision.image.save", "按本站配置保存指定运行照片", "Save a run image using this VPP configuration", "Guardar imagen con la configuración de este VPP",
                new[] { new FlowField("RunId", typeof(long)) }, new[] { new FlowField("Path", typeof(string)) }, call => {
                    lock (_operationLock)
                    {
                        RequireFlowResult(call.Get<long>("RunId"));
                        string path = SaveLastImage(settings);
                        if (string.IsNullOrEmpty(path)) throw new InvalidOperationException(FlowText.T("error.imageSave"));
                        return FlowCall.Result("Path", path);
                    }
                });
            List<VpCompLimitItem> limits = settings.CompLimits.Where(item => item != null).Select(item =>
                new VpCompLimitItem(item.Item, item.Min, item.Max) { Compensation = item.Compensation }).ToList();
            if (limits.Count == 0) yield break;
            FlowField itemField = new FlowField("Item", typeof(string), limits[0].Item);
            itemField.Options.AddRange(limits.Select(item => new FlowOption { Value = item.Item, Label = item.Item }));
            yield return FlowMethods.Sync("vision.compensation", "读取本站补偿表项目", "Read this VPP compensation item", "Leer compensación de este VPP",
                new[] { itemField }, new[] { new FlowField("Compensation", typeof(double)), new FlowField("Min", typeof(double)), new FlowField("Max", typeof(double)) }, call => {
                    VpCompLimitItem item = limits.SingleOrDefault(row => row.Item == call.Get<string>("Item"));
                    if (item == null) throw new InvalidOperationException(FlowText.T("error.compensationMissing"));
                    return new Dictionary<string, object> { { "Compensation", FlowActions.Number(item.Compensation) }, { "Min", FlowActions.Number(item.Min) }, { "Max", FlowActions.Number(item.Max) } };
                }, false);
        }
    }
}
