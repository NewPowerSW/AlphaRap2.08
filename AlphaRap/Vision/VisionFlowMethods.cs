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
                new[] { new FlowField("RunId", typeof(long)) { DisplayName = new[] { "拍照结果编号", "Capture result identifier", "Identificador del resultado de captura" }, Advanced = true,
                        Help = new[] { "关联本次拍照结果；保存照片或读取输出时选择本拍照节点即可。", "Identifies this capture result; select this capture node when saving its image or reading its output.", "Identifica este resultado; seleccione este nodo de captura para guardar su imagen o leer su salida." } },
                    new FlowField("Accepted", typeof(bool)) { DisplayName = new[] { "视觉判定通过", "Vision accepted", "Visión aceptada" } },
                    new FlowField("X", typeof(double)) { DisplayName = new[] { "视觉 X 结果", "Vision X result", "Resultado X de visión" } },
                    new FlowField("Y", typeof(double)) { DisplayName = new[] { "视觉 Y 结果", "Vision Y result", "Resultado Y de visión" } },
                    new FlowField("Angle", typeof(double)) { DisplayName = new[] { "视觉角度结果", "Vision angle result", "Resultado angular de visión" } } }, call => null);
            lock (_operationLock)
            {
                if (TB != null)
                {
                    foreach (CogToolBlockTerminal terminal in TB.Inputs)
                        run.Inputs.Add(new FlowField("TB:" + terminal.Name, terminal.ValueType, Convert.ToString(terminal.Value, CultureInfo.InvariantCulture)) {
                            DisplayName = new[] { terminal.Name, terminal.Name, terminal.Name }
                        });
                    foreach (CogToolBlockTerminal terminal in TB.Outputs) run.Outputs.Add(new FlowField("TB:" + terminal.Name, terminal.ValueType) {
                        DisplayName = new[] { terminal.Name, terminal.Name, terminal.Name }
                    });
                }
            }
            run.Invoke = RunFlowVision;
            yield return run;
            FlowField output = new FlowField("Output", typeof(string)) { DisplayName = new[] { "VPP 输出项目", "VPP output item", "Salida de VPP" } };
            output.Options.AddRange(run.Outputs.Where(field => field.Name.StartsWith("TB:")).Select(field => new FlowOption { Value = field.Name.Substring(3), Label = field.Name.Substring(3) }));
            if (output.Options.Count == 0) yield break;
            output.DefaultValue = output.Options[0].Value;
            yield return FlowMethods.Sync("vision.output", "读取指定运行的 VPP 输出", "Read a VPP run output", "Leer salida de una ejecución VPP",
                new[] { new FlowField("RunId", typeof(long)) { DisplayName = new[] { "来源拍照节点", "Source capture node", "Nodo de captura de origen" },
                    Help = new[] { "选择本 VPP 的拍照节点，自动使用该节点本次拍照的结果编号。", "Choose a capture node for this VPP; its capture result identifier is bound automatically.", "Seleccione un nodo de captura de este VPP; se vincula automáticamente el identificador de su resultado." },
                    OutputSourceMethod = "vision.run", OutputSourceField = "RunId" }, output },
                new[] { new FlowField("Value", typeof(object)) { DisplayName = new[] { "VPP 输出值", "VPP output value", "Valor de salida de VPP" } } }, call => {
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
                new[] { new FlowField("RunId", typeof(long)) { DisplayName = new[] { "来源拍照节点", "Source capture node", "Nodo de captura de origen" },
                    Help = new[] { "选择本 VPP 的拍照节点，保存该次拍照的照片；不重新拍照。", "Choose a capture node for this VPP to save that capture's image without taking another photo.", "Seleccione un nodo de captura de este VPP para guardar su imagen sin tomar otra foto." },
                    OutputSourceMethod = "vision.run", OutputSourceField = "RunId" } },
                new[] { new FlowField("Path", typeof(string)) { DisplayName = new[] { "照片文件路径", "Image file path", "Ruta del archivo de imagen" } } }, call => {
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
            FlowField itemField = new FlowField("Item", typeof(string), limits[0].Item) { DisplayName = new[] { "补偿表项目", "Compensation table item", "Elemento de la tabla de compensación" },
                Help = new[] { "选择本 VPP 已有的补偿项目；读取后由后续节点显式使用，不自动移动轴。", "Choose an existing compensation item in this VPP; later nodes must explicitly use it. Reading does not move an axis.", "Seleccione un elemento de compensación existente de este VPP; los nodos posteriores deben usarlo explícitamente. La lectura no mueve ningún eje." } };
            itemField.Options.AddRange(limits.Select(item => new FlowOption { Value = item.Item, Label = item.Item }));
            yield return FlowMethods.Sync("vision.compensation", "读取本站补偿表项目", "Read this VPP compensation item", "Leer compensación de este VPP",
                new[] { itemField }, new[] { new FlowField("Compensation", typeof(double)) { DisplayName = new[] { "补偿值", "Compensation value", "Valor de compensación" } },
                    new FlowField("Min", typeof(double)) { DisplayName = new[] { "下限", "Lower limit", "Límite inferior" } },
                    new FlowField("Max", typeof(double)) { DisplayName = new[] { "上限", "Upper limit", "Límite superior" } } }, call => {
                    VpCompLimitItem item = limits.SingleOrDefault(row => row.Item == call.Get<string>("Item"));
                    if (item == null) throw new InvalidOperationException(FlowText.T("error.compensationMissing"));
                    return new Dictionary<string, object> { { "Compensation", FlowActions.Number(item.Compensation) }, { "Min", FlowActions.Number(item.Min) }, { "Max", FlowActions.Number(item.Max) } };
                }, false);
        }
    }
}
