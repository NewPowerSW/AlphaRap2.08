using System;
using System.Collections.Generic;
using System.Linq;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    public partial class FlowNodeForm
    {
        private bool codeEditing;
        private string codeId = "";
        private readonly Dictionary<string, FlowStep> codeDrafts = new Dictionary<string, FlowStep>();
        private string InputModeKey { get { return codeEditing ? "Code/" + codeId : executionMode; } }

        /// <summary>显示已注册模块并保留失效身份，不自动选取其他模块。</summary>
        private void FillCodeChoices()
        {
            List<FlowChoice> choices = new List<FlowChoice> { new FlowChoice { Id = "", Label = FlowCodeUiText.T("choose") } };
            choices.AddRange(FlowToolboxCatalog.Definitions.Select(definition => new FlowChoice { Id = definition.Id, Label = FlowCodeUiText.DefinitionTitle(definition) }));
            if (!string.IsNullOrEmpty(codeId) && choices.All(choice => choice.Id != codeId))
                choices.Add(new FlowChoice { Id = codeId, Label = string.Format(FlowCodeUiText.T("missing"), codeId) });
            codeChoice.DisplayMember = "Label"; codeChoice.ValueMember = "Id"; codeChoice.DataSource = choices; codeChoice.SelectedValue = codeId;
            codeMode.Text = FlowCodeUiText.T("codeMode"); codeChoiceLabel.Text = FlowCodeUiText.T("codeChoice");
            if (codeEditing) parameterPage.Text = FlowCodeUiText.T("codeMode");
        }

        /// <summary>取得当前模块的局部草稿，切换模块不会共用可变绑定。</summary>
        private FlowStep CodeDraft()
        {
            FlowStep draft;
            if (codeDrafts.TryGetValue(codeId, out draft)) return draft;
            FlowCodeDefinition definition = FlowToolboxCatalog.Find(codeId);
            draft = new FlowStep { ExecutionMode = "Action", CodeModuleId = codeId, CodeModuleVersion = definition == null ? 0 : definition.Version,
                Target = "code/" + codeId, Action = "code.execute" };
            codeDrafts[codeId] = draft;
            return draft;
        }

        /// <summary>在同级参数和出口页面绑定实际资源，出口仅配置模块声明的去向。</summary>
        private void BindCodeDraft()
        {
            FlowCodeDefinition definition = FlowToolboxCatalog.Find(codeId);
            FlowStep draft = CodeDraft();
            codeBindings.Bind(definition, draft, catalog);
            codeSettings.Height = 38 + codeBindings.PreferredContentHeight;
            List<FlowChoice> targets = new List<FlowChoice> { new FlowChoice { Id = "", Label = FlowText.T("end") } };
            targets.AddRange(module.Program.Steps.Where(step => step.Id != original.Id).Select(step => new FlowChoice { Id = step.Id, Label = step.Caption }));
            codeRoutes.Bind(definition, draft, targets);
        }

        /// <summary>记录当前模块的资源和出口草稿，取消窗口不改变原节点。</summary>
        private void CaptureCodeDraft()
        {
            if (!codeEditing) return;
            FlowStep draft = CodeDraft();
            codeBindings.ReadTo(draft); codeRoutes.ReadTo(draft);
        }

        /// <summary>更换模块前缓存输入及资源，重新选回时继续原来的编辑。</summary>
        private void CodeChanged(object sender, EventArgs args)
        {
            if (filling) return;
            CaptureModeInputs();
            codeId = Convert.ToString(codeChoice.SelectedValue);
            filling = true;
            try { BindCodeDraft(); RefreshExecutionMethod(); RestoreModeInputs(); }
            finally { filling = false; }
        }

        /// <summary>轴绑定改变时刷新其点位候选，保留旧索引和来源引用。</summary>
        private void CodeBindingsChanged(object sender, EventArgs args)
        {
            if (filling || !codeEditing) return;
            CaptureModeInputs();
            filling = true;
            try { RefreshExecutionMethod(); RestoreModeInputs(); }
            finally { filling = false; }
        }

        /// <summary>恢复当前模式的输入草稿，没有草稿时由元数据提供初值。</summary>
        private void RestoreModeInputs()
        {
            List<FlowInput> previous;
            FillInputs(modeInputs.TryGetValue(InputModeKey, out previous) ? previous : new List<FlowInput>());
        }

        /// <summary>确认节点时写入模块身份及资源映射，其他执行方式清除旧模块绑定。</summary>
        private void ReadCodeTo(FlowStep candidate)
        {
            candidate.CodeModuleId = codeEditing ? codeId : "";
            candidate.CodeModuleVersion = 0;
            candidate.ResourceBindings = new List<FlowResourceBinding>();
            if (!codeEditing) return;
            FlowCodeDefinition definition = FlowToolboxCatalog.Find(codeId);
            candidate.CodeModuleVersion = definition == null ? CodeDraft().CodeModuleVersion : definition.Version;
            candidate.Target = "code/" + codeId; candidate.Action = "code.execute";
            codeBindings.ReadTo(candidate, true);
        }
    }
}
