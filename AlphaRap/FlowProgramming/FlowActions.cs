using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NPSDK;
using NPSDK.Motion;

namespace AlphaRap.FlowProgramming
{
    /// <summary>校验和调用对象已声明的方法，不维护具体设备动作或协议分支。</summary>
    public sealed class FlowActions
    {
        public readonly FlowResources Resources;
        public readonly Flow_BaseForm Owner;
        private readonly List<FlowMethod> utility;
        public bool Simulate;
        internal bool UsePublishedOutputs;
        public Func<string, CancellationToken, Task<string>> RequestInput;

        /// <summary>使用已捕获的实际对象与配方快照。</summary>
        public FlowActions(FlowResources resources, Flow_BaseForm owner = null)
        {
            Resources = resources;
            Owner = owner;
            utility = new FlowUtilityObject().GetFlowMethods().ToList();
            foreach (FlowMethod method in utility) method.ValidateDefinition();
        }

        /// <summary>读取选中对象自己的方法；空目标只提供流程上下文方法。</summary>
        public List<FlowMethod> MethodsFor(FlowResource resource)
        {
            if (resource == null) return utility;
            if (resource.Kind == "native" && !ReferenceEquals(resource.Instance, Owner)) return new List<FlowMethod>();
            return resource.Methods;
        }

        /// <summary>代码流程只提供给所属实际窗体，其余对象继续使用原资源目录。</summary>
        public bool CanSelectResource(FlowResource resource)
        {
            return resource.Selectable && (resource.Kind != "native" || ReferenceEquals(resource.Instance, Owner));
        }

        /// <summary>按对象身份和方法身份联合解析，禁止用其他对象的同名方法补位。</summary>
        public FlowMethod FindMethod(FlowStep step)
        {
            if (!IsSupported(step)) return null;
            if (step.ExecutionMode == "None") return ConditionMethod();
            FlowResource resource = FindResource(step.Target);
            if (!string.IsNullOrEmpty(step.Target) && resource == null) return null;
            return MethodsFor(resource).SingleOrDefault(method => method.Id == step.Action);
        }

        /// <summary>只允许对象动作和条件节点，旧脚本及代码模块须明确重新配置。</summary>
        public static bool IsSupported(FlowStep step)
        {
            return step != null && string.IsNullOrEmpty(step.CodeModuleId)
                && (step.ExecutionMode == "Action" || step.ExecutionMode == "None");
        }

        /// <summary>条件节点只判定出口，不执行对象方法或产生动作输出。</summary>
        private static FlowMethod ConditionMethod()
        {
            FlowMethod method = new FlowMethod("flow.none", "仅判断条件", "Conditions only", "Solo condiciones") { UsesHardware = false };
            method.Invoke = call => { call.Token.ThrowIfCancellationRequested(); return Task.FromResult<IDictionary<string, object>>(new Dictionary<string, object>()); };
            return method;
        }

        /// <summary>校验方法、输入类型及数据来源，失效的对象或端口必须先修复。</summary>
        public void Validate(FlowStep step)
        {
            Lazy<List<FlowChoice>> outputs = new Lazy<List<FlowChoice>>(() => FlowPrograms.OutputChoices(this));
            if (!IsSupported(step)) throw new InvalidOperationException(string.Format(FlowText.T("retiredNode"), step == null ? "" : step.Caption));
            if (step.ControlOrigin == FlowControlSource.MissingOrigin)
                throw new InvalidOperationException(string.Format(FlowText.T("sourceControlMissing"), step.Caption, step.ControlName));
            FlowNativeMethods.ValidateLegacy(Owner, step);
            if (FlowProgramFile.RequiresBranchReview(step)) throw new InvalidOperationException(FlowText.T("reviewBranches") + " " + step.Caption);
            List<string> errors = FlowProgramFile.ValidateBranches(step);
            if (errors.Count > 0) throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
            FlowMethod method = FindMethod(step);
            if (method == null) throw new InvalidOperationException(FlowText.T("methodMissing") + " " + step.Caption + " / " + step.Target + " / " + step.Action);
            if (step.UsesFlowResult != method.UsesFlowResult)
                throw new InvalidOperationException(string.Format(FlowNativeText.T("contract"), step.Caption));
            if (step.Inputs.Count != method.Inputs.Count || method.Inputs.Any(field => step.Inputs.Count(input => input.Name == field.Name) != 1))
                throw new InvalidOperationException(FlowText.T("fieldsChanged") + " " + step.Caption);
            foreach (FlowInput input in step.Inputs)
            {
                ValidateSource(input, false, method, outputs);
                FlowField field = method.Inputs.Single(item => item.Name == input.Name);
                ValidateProducer(input, field, step, outputs);
                if (input.Source == "Constant") ValidateValue(field, input.Reference ?? "");
                if (input.Source == "Parameter") ValidateValue(field, Resources.Parameters[input.Reference]);
            }
            Dictionary<string, object> known = step.Inputs.Where(input => input.Source == "Constant" || input.Source == "Parameter")
                .ToDictionary(input => input.Name, input => input.Source == "Constant" ? (object)input.Reference : Resources.Parameters[input.Reference]);
            if (method.ValidateInputs != null) method.ValidateInputs(known);
            foreach (FlowBranch branch in FlowProgramFile.BranchesOf(step).Where(branch => !branch.Unconditional))
                ValidateConditions(branch, method, outputs);
        }

        /// <summary>校验每条出口的可见操作数；为空比较只使用左侧，不读取隐藏的右侧旧绑定。</summary>
        private void ValidateConditions(FlowBranch branch, FlowMethod method, Lazy<List<FlowChoice>> outputs)
        {
            foreach (FlowCondition condition in branch.Conditions)
            {
                ValidateSource(condition.Left, true, method, outputs);
                if (condition.Operator == "Empty") continue;
                ValidateSource(condition.Right, true, method, outputs);
                RequireComparisonValue(branch.Code, method, condition.Left, condition.Right);
                RequireComparisonValue(branch.Code, method, condition.Right, condition.Left);
            }
        }

        /// <summary>当前动作的布尔或数值结果必须填写比较值，文本字段仍允许显式比较空字符串。</summary>
        private static void RequireComparisonValue(string branch, FlowMethod method, FlowInput result, FlowInput value)
        {
            if (result.Source != "Result" || value.Source != "Constant" || !string.IsNullOrWhiteSpace(value.Reference)) return;
            FlowField field = method.Outputs.FirstOrDefault(output => output.Name == result.Reference);
            if (field == null || field.Type.IsEnum) return;
            TypeCode type = Type.GetTypeCode(field.Type);
            if (field.Type != typeof(bool) && (type < TypeCode.SByte || type > TypeCode.Decimal)) return;
            throw new InvalidOperationException(string.Format(FlowText.T("conditionValueRequired"), branch, FlowMethods.FieldTitle(method.Id, field, true)));
        }

        /// <summary>具有来源约束的结果必须来自同一实际对象的指定动作，拒绝跨 VPP 冒用相同运行编号。</summary>
        private void ValidateProducer(FlowInput input, FlowField field, FlowStep step, Lazy<List<FlowChoice>> outputs)
        {
            if (string.IsNullOrEmpty(field.OutputSourceMethod) && string.IsNullOrEmpty(field.OutputSourceField)) return;
            if (input.Source == "Constant") return;
            if (input.Source != "Output")
                throw new InvalidOperationException(string.Format(FlowText.T("producerMismatch"), step.Caption, FlowMethods.FieldTitle(step.Action, field)));
            FlowChoice source = outputs.Value.Single(item => item.Id == input.Reference);
            bool methodMatches = string.IsNullOrEmpty(field.OutputSourceMethod) || source.ActionId == field.OutputSourceMethod;
            bool fieldMatches = string.IsNullOrEmpty(field.OutputSourceField) || source.FieldName == field.OutputSourceField;
            if (source.TargetId == step.Target && methodMatches && fieldMatches) return;
            throw new InvalidOperationException(string.Format(FlowText.T("producerMismatch"), step.Caption, FlowMethods.FieldTitle(step.Action, field)));
        }

        /// <summary>固定值和可静态读取的参数在保存时就按真实方法类型校验。</summary>
        private static object ValidateValue(FlowField field, object value)
        {
            object typed;
            try { typed = field.ConvertValue(value); }
            catch (Exception error) when (error is FormatException || error is OverflowException || error is InvalidCastException || error is InvalidOperationException || error is ArgumentException)
            {
                throw new InvalidOperationException(string.Format(FlowText.T("invalidType"), field.Name, field.Type.Name), error);
            }
            if (field.Options.Count > 0 && !field.Options.Any(option => option.Value == Convert.ToString(typed, CultureInfo.InvariantCulture)))
                throw new InvalidOperationException(FlowText.T("invalidChoice") + " " + field.Name);
            return typed;
        }

        /// <summary>结果来源只允许用于动作后的条件；其他节点输出必须存在于当前工程。</summary>
        private void ValidateSource(FlowInput input, bool condition, FlowMethod method, Lazy<List<FlowChoice>> outputs)
        {
            if (input.Source == "Constant") return;
            if (input.Source == "Parameter" && Resources.Parameters.ContainsKey(input.Reference ?? "")) return;
            if (input.Source == "Context" && new[] { "CycleId", "RunId", "RecipeName", "ExecutionId" }.Contains(input.Reference)) return;
            if (input.Source == "Result" && condition && method.Outputs.Any(field => field.Name == input.Reference)) return;
            if (input.Source == "Output" && outputs.Value.Any(choice => choice.Id == input.Reference)) return;
            throw new InvalidOperationException(FlowText.T("sourceMissing") + " " + input.Name + " / " + input.Reference);
        }

        /// <summary>按稳定身份取得实际对象；删除后不按显示名猜测替代对象。</summary>
        public FlowResource FindResource(string id)
        {
            FlowResource resource;
            return id != null && Resources.Items.TryGetValue(id, out resource) ? resource : null;
        }

        /// <summary>解析类型化输入后持有实际资源租约，调用该对象自己的实现。</summary>
        public async Task<IDictionary<string, object>> Execute(FlowStep step, FlowExecutionContext context, CancellationToken token)
        {
            Validate(step);
            FlowMethod method = FindMethod(step);
            Dictionary<string, object> input = method.Inputs.ToDictionary(field => field.Name,
                field => ValidateValue(field, context.Resolve(step.Inputs.Single(item => item.Name == field.Name))));
            FlowCall call = new FlowCall { Step = step, Context = context, InvocationId = context.ExecutionId, Inputs = input, Token = token, Simulation = Simulate, RequestInput = RequestInput };
            FlowResource resource = step.ExecutionMode == "Action" ? FindResource(step.Target) : null;
            if (method.ValidateInputs != null) method.ValidateInputs(input);
            using (FlowResourceGate.Lease lease = await FlowResourceGate.Acquire(ResourceLocks(resource == null ? null : resource.Instance), token).ConfigureAwait(false))
                return await lease.Run(() => Invoke(method, call)).ConfigureAwait(false);
        }

        /// <summary>普通动作沿用条件出口；原生事件把自身返回值交给同一运行器处理。</summary>
        public async Task<FlowActionOutcome> ExecuteOutcome(FlowStep step, FlowExecutionContext context, CancellationToken token)
        {
            IDictionary<string, object> output = await Execute(step, context, token).ConfigureAwait(false);
            return new FlowActionOutcome { Outputs = output, BranchCode = step.UsesFlowResult ? ((Flow_Chart.ResultType)output["Result"]).ToString() : null };
        }

        /// <summary>模拟硬件必须使用明确的模拟实现；实际方法返回后检查输出契约。</summary>
        private async Task<IDictionary<string, object>> Invoke(FlowMethod method, FlowCall call)
        {
            call.Token.ThrowIfCancellationRequested();
            Func<FlowCall, Task<IDictionary<string, object>>> invoke = call.Simulation && method.UsesHardware ? method.Simulate : method.Invoke;
            if (invoke == null) throw new InvalidOperationException(FlowText.T("noSimulation") + " " + method.Id);
            IDictionary<string, object> output = await invoke(call).ConfigureAwait(false);
            call.Token.ThrowIfCancellationRequested();
            return ValidateOutputs(method, output);
        }

        /// <summary>按对象方法契约检查输出，拒绝缺失字段和无效数值。</summary>
        private static IDictionary<string, object> ValidateOutputs(FlowMethod method, IDictionary<string, object> output)
        {
            if (output == null || method.Outputs.Any(field => !output.ContainsKey(field.Name)))
                throw new InvalidOperationException(FlowText.T("outputMissing") + " " + method.Id);
            foreach (FlowField field in method.Outputs)
                if (output[field.Name] != null) output[field.Name] = ValidateValue(field, output[field.Name]);
            return output;
        }

        /// <summary>绑定表与轴共用资源，气缸占用实际输出，相机按采集通道互斥。</summary>
        private List<object> ResourceLocks(object instance)
        {
            Flow_BaseForm form = instance as Flow_BaseForm;
            if (form != null)
            {
                List<object> local = new List<object> { form };
                foreach (FlowResource resource in Resources.Items.Values.Where(item => !ReferenceEquals(item.Instance, form) && item.Id.StartsWith(form.Name + "/", StringComparison.Ordinal)))
                    local.AddRange(ResourceLocks(resource.Instance));
                return local.Distinct().ToList();
            }
            FlowPointAxis point = instance as FlowPointAxis;
            if (point != null) return new List<object> { point.Motor };
            NP_Cylinder cylinder = instance as NP_Cylinder;
            if (cylinder != null) return new object[] { cylinder, cylinder.CyOn_OB1, cylinder.CyOn_OB2, cylinder.CyOff_OB1, cylinder.CyOff_OB2 }.Where(item => item != null).ToList();
            VpStation station = instance as VpStation;
            if (station != null) return new List<object> { station, string.Intern("camera:" + station.CameraName) };
            return instance == null ? new List<object>() : new List<object> { instance };
        }
        /// <summary>读取有限数值，所有区域设置使用相同的持久化数值格式。</summary>
        internal static double Number(object value)
        {
            double number = Convert.ToDouble(value, CultureInfo.InvariantCulture);
            if (double.IsNaN(number) || double.IsInfinity(number)) throw new InvalidOperationException("A finite number is required.");
            return number;
        }

        /// <summary>读取整数，禁止将小数或越界值当作点位索引。</summary>
        internal static int Integer(object value)
        {
            double number = Number(value);
            if (number != Math.Truncate(number) || number < int.MinValue || number > int.MaxValue) throw new InvalidOperationException("An integer is required.");
            return (int)number;
        }

        /// <summary>解析明确的布尔值。</summary>
        internal static bool Boolean(object value)
        {
            string text = Convert.ToString(value, CultureInfo.InvariantCulture);
            if (text == "1") return true;
            if (text == "0") return false;
            return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
        }


    }
}
