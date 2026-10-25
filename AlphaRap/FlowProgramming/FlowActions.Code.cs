using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    public sealed partial class FlowActions
    {
        /// <summary>运行代码模块的类型化入口；普通动作仍由原参数和条件管线执行。</summary>
        public async Task<FlowActionOutcome> ExecuteStep(FlowStep step, FlowExecutionContext context, CancellationToken token)
        {
            if (string.IsNullOrEmpty(step.CodeModuleId))
                return new FlowActionOutcome { Outputs = await Execute(step, context, token).ConfigureAwait(false) };
            Validate(step);
            FlowCodeDefinition definition = FlowToolboxCatalog.Find(step.CodeModuleId);
            FlowMethod method = FlowResourceAccess.CreateMethod(definition, step, Resources);
            Dictionary<string, object> input = method.Inputs.ToDictionary(field => field.Name,
                field => ValidateValue(field, context.Resolve(step.Inputs.Single(item => item.Name == field.Name))));
            if (method.ValidateInputs != null) method.ValidateInputs(input);
            FlowCall call = new FlowCall { Step = step, Context = context, InvocationId = Guid.NewGuid().ToString("N"),
                Inputs = input, Token = token, Simulation = Simulate, RequestInput = RequestInput };
            call.InvokeResourceAsync = (slot, action, values, cancellation) => ExecuteCodeResource(call, definition, slot, action, values, cancellation);
            FlowActionOutcome result = await definition.InvokeAsync(call).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            result.Outputs = ValidateOutputs(method, result.Outputs);
            return result;
        }

        /// <summary>在保存和每次执行前校验模块版本、实际角色和全部出口去向。</summary>
        private void ValidateCode(FlowStep step)
        {
            FlowCodeDefinition definition = FlowToolboxCatalog.Find(step.CodeModuleId);
            if (definition == null || definition.Version != step.CodeModuleVersion)
                throw new InvalidOperationException(string.Format(FlowCodeText.T("moduleMissing"), step.CodeModuleId));
            FlowResourceAccess.Validate(definition, step, Resources);
            foreach (string exit in definition.Exits)
                if (step.TargetOf(exit) == null) throw new InvalidOperationException(string.Format(FlowCodeText.T("exitMissing"), exit));
        }

        /// <summary>只解析当前节点明确声明的角色，实际调用沿用原对象方法和同一资源锁。</summary>
        private Task<IDictionary<string, object>> ExecuteCodeResource(FlowCall parent, FlowCodeDefinition definition, string slot,
            string action, IDictionary<string, object> values, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            FlowCodeResourceSlot role = definition.ResourceSlots.SingleOrDefault(item => item.Name == slot);
            FlowResourceBinding binding = parent.Step.ResourceBindings.SingleOrDefault(item => item.Slot == slot);
            FlowResource resource = binding == null ? null : FindResource(binding.Target);
            if (role == null || !FlowResourceAccess.Matches(role.Capability, resource))
                throw new InvalidOperationException(string.Format(FlowCodeText.T("bindingMissing"), slot));
            return ExecuteResource(parent, resource, action, values, token, true);
        }
    }
}
