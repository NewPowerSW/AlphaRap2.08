using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlphaRap.FlowRuntime;

namespace AlphaRap.FlowWorkbench
{
    public partial class FlowWorkbenchForm : Form
    {
        private readonly ActionCatalog catalog = ActionCatalog.Simulation();
        private readonly FlowLanguage language = new FlowLanguage();
        private FlowDefinition flow;
        private FlowNode current;
        private bool binding;
        private CancellationTokenSource stop;
        private Task execution;
        private bool closing;
        private readonly List<Choice> actionChoices = new List<Choice>();
        private readonly List<Choice> sourceChoices = new List<Choice>();
        private string statusKey;
        private object[] statusArguments;

        public sealed class Choice
        {
            public string Id { get; set; }
            public string Text { get; set; }
        }

        /// <summary>创建仅使用仿真实例的流程工作台。</summary>
        public FlowWorkbenchForm() : this("Chinese") { }

        /// <summary>以指定三语界面创建离线工作台。</summary>
        public FlowWorkbenchForm(string chosenLanguage)
        {
            InitializeComponent();
            language.Switch(chosenLanguage);
            languageBox.Items.AddRange(new object[] { "Chinese", "English", "Español" });
            languageBox.SelectedItem = chosenLanguage;
            outputBox.Text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProductionData");
            WireEvents();
            foreach (var action in catalog.All) actionChoices.Add(new Choice { Id = action.Id, Text = action.Id });
            actionColumn.DataSource = actionChoices;
            actionColumn.ValueMember = "Id";
            actionColumn.DisplayMember = "Text";
            alarmColumn.Items.AddRange("", "SIM_ACTION", "SIM_TIMEOUT");
            language.RegisterAndApplyLanguage(this);
            ApplyLanguage();
            BindFlow(DemoFlow.Create(catalog));
        }

        /// <summary>连接编辑和运行事件，不在构造期间启动任何任务。</summary>
        private void WireEvents()
        {
            newButton.Click += NewFlow;
            openButton.Click += OpenFlow;
            saveButton.Click += SaveFlow;
            validateButton.Click += ValidateFlow;
            runButton.Click += RunFlow;
            stopButton.Click += StopFlow;
            addButton.Click += AddNode;
            deleteButton.Click += DeleteNode;
            deviceAddButton.Click += AddDevice;
            deviceDeleteButton.Click += DeleteDevice;
            addFieldButton.Click += AddField;
            folderButton.Click += ChooseFolder;
            showFolderButton.Click += ShowFolder;
            languageBox.SelectedIndexChanged += ChangeLanguage;
            nodeGrid.CurrentCellChanged += SelectNode;
            nodeGrid.CellValueChanged += EditNode;
            nodeGrid.CurrentCellDirtyStateChanged += CommitCombo;
            nodeGrid.CellBeginEdit += PrepareTargets;
            inputGrid.CurrentCellDirtyStateChanged += CommitCombo;
            inputGrid.CellValueChanged += EditBinding;
            inputGrid.UserDeletingRow += DeleteBinding;
            inputGrid.UserDeletedRow += BindingDeleted;
            deviceGrid.CellValueChanged += EditDevice;
            nodeGrid.DataError += GridError;
            inputGrid.DataError += GridError;
            deviceGrid.DataError += GridError;
            graph.NodeSelected += SelectGraphNode;
            FormClosing += CloseWorkbench;
        }

        /// <summary>替换整个编辑文档，清除前次结果但不重建固定控件。</summary>
        private void BindFlow(FlowDefinition definition)
        {
            binding = true;
            flow = definition;
            current = null;
            nodeGrid.Rows.Clear();
            foreach (var node in flow.Nodes) nodeGrid.Rows.Add(node.Id, node.Title, node.ActionId, node.TargetId, node.Next, node.FalseNext, node.TimeoutMs, node.AlarmCode);
            deviceGrid.Rows.Clear();
            foreach (var device in flow.Devices) deviceGrid.Rows.Add(device.Id, device.Kind.ToString(), device.State, device.X, device.Y, device.Angle, device.DelayMs);
            RefreshChoices();
            graph.Flow = flow;
            graph.States.Clear();
            graph.Invalidate();
            resultGrid.Rows.Clear();
            logGrid.Rows.Clear();
            binding = false;
            if (nodeGrid.Rows.Count > 0) nodeGrid.CurrentCell = nodeGrid.Rows[0].Cells[0];
            SelectNode(this, EventArgs.Empty);
        }

        /// <summary>刷新节点出口和设备选项，保留稳定标识。</summary>
        private void RefreshChoices()
        {
            var ids = new[] { "" }.Concat(flow.Nodes.Select(n => n.Id)).ToArray();
            nextColumn.Items.Clear(); nextColumn.Items.AddRange(ids);
            falseColumn.Items.Clear(); falseColumn.Items.AddRange(ids);
            targetColumn.Items.Clear(); targetColumn.Items.AddRange(new[] { "" }.Concat(flow.Devices.Select(d => d.Id)).ToArray());
        }

        /// <summary>切换当前节点，提交原节点输入后展示新节点的参数契约。</summary>
        private void SelectNode(object sender, EventArgs e)
        {
            if (binding || flow == null || nodeGrid.CurrentRow == null) return;
            SaveInputs();
            string id = Convert.ToString(nodeGrid.CurrentRow.Cells[0].Value);
            current = flow.Nodes.FirstOrDefault(n => n.Id == id);
            ShowInputs();
        }

        /// <summary>按照动作输入描述创建数据行，常量和来源引用分别编辑。</summary>
        private void ShowInputs()
        {
            binding = true;
            inputGrid.Rows.Clear();
            sourceChoices.Clear();
            sourceChoices.Add(new Choice { Id = "", Text = language.Text("Constant") });
            foreach (var node in flow.Nodes)
                foreach (var output in catalog.Find(node.ActionId).Outputs) sourceChoices.Add(new Choice { Id = node.Id + "." + output.Name, Text = node.Id + "." + output.Name });
            sourceColumn.DataSource = null;
            sourceColumn.DataSource = sourceChoices;
            sourceColumn.ValueMember = "Id";
            sourceColumn.DisplayMember = "Text";
            if (current != null)
            {
                var spec = catalog.Find(current.ActionId);
                foreach (var input in current.Inputs)
                {
                    if (!sourceChoices.Any(c => c.Id == (input.Source ?? ""))) sourceChoices.Add(new Choice { Id = input.Source, Text = input.Source });
                    int row = inputGrid.Rows.Add(input.Name, input.Source ?? "", input.Value);
                    inputGrid.Rows[row].Cells[0].ReadOnly = spec.Inputs.Any(f => f.Name == input.Name);
                    inputGrid.Rows[row].Cells[2].ReadOnly = !string.IsNullOrEmpty(input.Source);
                }
                selectedLabel.Text = current.Id + " · " + language.Text("Action_" + current.ActionId.Replace('.', '_'));
                graph.SelectedId = current.Id;
                graph.Invalidate();
                addFieldButton.Enabled = spec.ExtraInputs && stop == null;
            }
            binding = false;
        }

        /// <summary>把输入编辑行写回当前节点，解析与类型检查交给发布校验。</summary>
        private void SaveInputs()
        {
            if (current == null || binding) return;
            current.Inputs = inputGrid.Rows.Cast<DataGridViewRow>().Select(r => new InputBinding { Name = Cell(r, 0), Source = Cell(r, 1), Value = Cell(r, 2) }).ToList();
        }

        /// <summary>更新来源绑定；引用字段的常量输入框自动只读。</summary>
        private void EditBinding(object sender, DataGridViewCellEventArgs e)
        {
            if (binding || e.RowIndex < 0) return;
            inputGrid.Rows[e.RowIndex].Cells[2].ReadOnly = Cell(inputGrid.Rows[e.RowIndex], 1) != "";
            SaveInputs();
        }

        /// <summary>只允许删除保存动作的自定义字段，保护动作必需输入。</summary>
        private void DeleteBinding(object sender, DataGridViewRowCancelEventArgs e)
        {
            e.Cancel = stop != null || current == null || !catalog.Find(current.ActionId).ExtraInputs || catalog.Find(current.ActionId).Inputs.Any(f => f.Name == Cell(e.Row, 0));
        }

        /// <summary>字段删除后同步当前节点映射。</summary>
        private void BindingDeleted(object sender, DataGridViewRowEventArgs e) { SaveInputs(); }

        /// <summary>更新节点元数据；更换动作时采用新契约默认参数。</summary>
        private void EditNode(object sender, DataGridViewCellEventArgs e)
        {
            if (binding || e.RowIndex < 0) return;
            var row = nodeGrid.Rows[e.RowIndex];
            var node = flow.Nodes.First(n => n.Id == Cell(row, 0));
            string action = Cell(row, 2);
            bool changed = node.ActionId != action;
            node.Title = Cell(row, 1); node.TargetId = Cell(row, 3); node.Next = Cell(row, 4); node.FalseNext = Cell(row, 5); node.AlarmCode = Cell(row, 7);
            int timeout;
            node.TimeoutMs = int.TryParse(Cell(row, 6), out timeout) ? timeout : 0;
            if (changed)
            {
                node.ActionId = action;
                node.Inputs = DemoFlow.Node(catalog, node.Id, action, "", "").Inputs;
                var spec = catalog.Find(action);
                node.TargetId = spec.TargetKind.HasValue ? flow.Devices.Where(d => d.Kind == spec.TargetKind).Select(d => d.Id).FirstOrDefault() ?? "" : "";
                if (!spec.Branch) node.FalseNext = "";
                binding = true; row.Cells[3].Value = node.TargetId; row.Cells[5].Value = node.FalseNext; binding = false;
            }
            if (node == current) ShowInputs();
            graph.Invalidate();
        }

        /// <summary>更新仿真实例参数；非法数值交给表格和运行校验报告。</summary>
        private void EditDevice(object sender, DataGridViewCellEventArgs e)
        {
            if (binding || e.RowIndex < 0) return;
            var row = deviceGrid.Rows[e.RowIndex];
            var device = flow.Devices.First(d => d.Id == Cell(row, 0));
            try
            {
                device.State = Convert.ToBoolean(row.Cells[2].Value);
                device.Kind = (DeviceKind)Enum.Parse(typeof(DeviceKind), Cell(row, 1));
                device.X = Convert.ToDouble(row.Cells[3].Value); device.Y = Convert.ToDouble(row.Cells[4].Value); device.Angle = Convert.ToDouble(row.Cells[5].Value);
                device.DelayMs = Convert.ToInt32(row.Cells[6].Value);
            }
            catch (Exception error) { SetStatus(language.Text("InvalidDevice", device.Id) + " " + error.Message); }
        }

        /// <summary>为目标下拉框只提供类型匹配的实例。</summary>
        private void PrepareTargets(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.ColumnIndex != 3) return;
            var spec = catalog.Find(Cell(nodeGrid.Rows[e.RowIndex], 2));
            var cell = (DataGridViewComboBoxCell)nodeGrid.Rows[e.RowIndex].Cells[3];
            cell.Items.Clear(); cell.Items.Add("");
            cell.Items.AddRange(flow.Devices.Where(d => spec.TargetKind.HasValue && d.Kind == spec.TargetKind).Select(d => d.Id).ToArray());
        }

        /// <summary>下拉框选择后立即通知模型，不改变文本编辑状态。</summary>
        private void CommitCombo(object sender, EventArgs e)
        {
            var grid = (DataGridView)sender;
            if (grid.IsCurrentCellDirty && grid.CurrentCell is DataGridViewComboBoxCell) grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        /// <summary>记录表格错误，阻止非法数据静默写入。</summary>
        private void GridError(object sender, DataGridViewDataErrorEventArgs e) { e.ThrowException = false; SetMessage("GridError"); }

        /// <summary>读取表格业务文本。</summary>
        private static string Cell(DataGridViewRow row, int column) { return Convert.ToString(row.Cells[column].Value) ?? ""; }

        /// <summary>从画布选择对应节点的编辑行。</summary>
        private void SelectGraphNode(string id)
        {
            foreach (DataGridViewRow row in nodeGrid.Rows) if (Cell(row, 0) == id) { nodeGrid.CurrentCell = row.Cells[0]; return; }
        }

        /// <summary>提交当前控件编辑，保持运行与保存使用同一配置快照。</summary>
        private void CommitEdits()
        {
            if (!nodeGrid.EndEdit() || !inputGrid.EndEdit() || !deviceGrid.EndEdit()) throw new FlowIssue("GridError");
            SaveInputs();
        }

        /// <summary>加载默认可执行工艺。</summary>
        private void NewFlow(object sender, EventArgs e) { BindFlow(DemoFlow.Create(catalog)); SetMessage("DemoReady"); }

        /// <summary>读取 XML 并先验证再替换当前文档。</summary>
        private void OpenFlow(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog { Filter = "XML|*.xml" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                try { var loaded = FlowFiles.Load(dialog.FileName); FlowValidation.Validate(loaded, catalog); BindFlow(loaded); SetStatus(dialog.FileName); }
                catch (Exception error) { Report(error); }
            }
        }

        /// <summary>验证并原子保存当前流程，包含仿真实例参数。</summary>
        private void SaveFlow(object sender, EventArgs e)
        {
            try
            {
                CommitEdits(); FlowValidation.Validate(flow, catalog);
                using (var dialog = new SaveFileDialog { Filter = "XML|*.xml", FileName = "Flow.xml" })
                    if (dialog.ShowDialog(this) == DialogResult.OK) { FlowFiles.Save(dialog.FileName, flow); SetStatus(dialog.FileName); }
            }
            catch (Exception error) { Report(error); }
        }

        /// <summary>验证动作、目标、出口及数据绑定。</summary>
        private void ValidateFlow(object sender, EventArgs e)
        {
            try { CommitEdits(); FlowValidation.Validate(flow, catalog); SetMessage("Valid"); }
            catch (Exception error) { Report(error); }
        }

        /// <summary>添加采用标准契约的节点，连线由工程师明确选择。</summary>
        private void AddNode(object sender, EventArgs e)
        {
            try { CommitEdits(); }
            catch (Exception error) { Report(error); return; }
            int number = 1;
            while (flow.Nodes.Any(n => n.Id == "N" + number)) number++;
            flow.Nodes.Add(DemoFlow.Node(catalog, "N" + number, "flow.note", "", ""));
            BindFlow(flow);
            SelectGraphNode("N" + number);
        }

        /// <summary>删除节点并清除指向该节点的边；后续校验检查不可达节点。</summary>
        private void DeleteNode(object sender, EventArgs e)
        {
            if (current == null || flow.Nodes.Count == 1) return;
            var removed = current;
            flow.Nodes.Remove(removed);
            foreach (var node in flow.Nodes) { if (node.Next == removed.Id) node.Next = ""; if (node.FalseNext == removed.Id) node.FalseNext = ""; }
            if (flow.StartNode == removed.Id) flow.StartNode = flow.Nodes[0].Id;
            BindFlow(flow);
        }

        /// <summary>为保存动作增加用户自定义字段映射。</summary>
        private void AddField(object sender, EventArgs e)
        {
            if (current == null || !catalog.Find(current.ActionId).ExtraInputs) return;
            SaveInputs();
            int number = 1;
            while (current.Inputs.Any(i => i.Name == "Field" + number)) number++;
            current.Inputs.Add(new InputBinding { Name = "Field" + number });
            ShowInputs();
        }

        /// <summary>增加可配置的仿真实例，身份由工作台唯一分配。</summary>
        private void AddDevice(object sender, EventArgs e)
        {
            try { CommitEdits(); }
            catch (Exception error) { Report(error); return; }
            int number = 1;
            while (flow.Devices.Any(d => d.Id == "Device" + number)) number++;
            flow.Devices.Add(new SimDevice { Id = "Device" + number, Kind = DeviceKind.Vision, DelayMs = 200 });
            BindFlow(flow);
            tabs.SelectedTab = devicesTab;
        }

        /// <summary>只允许删除未被节点引用的仿真实例。</summary>
        private void DeleteDevice(object sender, EventArgs e)
        {
            if (deviceGrid.CurrentRow == null) return;
            string id = Cell(deviceGrid.CurrentRow, 0);
            if (flow.Nodes.Any(n => n.TargetId == id)) { SetMessage("DeviceInUse", id); return; }
            flow.Devices.RemoveAll(d => d.Id == id);
            BindFlow(flow);
            tabs.SelectedTab = devicesTab;
        }

        /// <summary>执行隔离的运行快照，所有观察更新通过 UI 消息队列处理。</summary>
        private async void RunFlow(object sender, EventArgs e)
        {
            if (stop != null) return;
            try
            {
                CommitEdits(); FlowValidation.Validate(flow, catalog);
                var snapshot = FlowFiles.Snapshot(flow);
                string directory = Path.GetFullPath(outputBox.Text);
                stop = new CancellationTokenSource();
                SetRunning(true);
                graph.States.Clear(); resultGrid.Rows.Clear(); logGrid.Rows.Clear();
                var runner = new FlowRunner();
                runner.Notice += ReceiveNotice;
                execution = CompleteRun(runner, snapshot, directory, stop.Token);
                await execution;
            }
            catch (Exception error) { Report(error); }
            finally { if (stop != null) stop.Dispose(); stop = null; execution = null; SetRunning(false); }
        }

        /// <summary>在后台执行流程，并保留保存路径供结果查看。</summary>
        private async Task CompleteRun(FlowRunner runner, FlowDefinition snapshot, string directory, CancellationToken token)
        {
            var result = await Task.Run(() => runner.RunAsync(snapshot, catalog, directory, token));
            SetMessage("RunSummary", result.State, result.RunId);
            if (result.Error != null) Report(result.Error);
        }

        /// <summary>将不可变运行通知投递到 UI 线程。</summary>
        private void ReceiveNotice(FlowNotice notice)
        {
            if (IsDisposed || !IsHandleCreated) return;
            BeginInvoke(new Action(() => ApplyNotice(notice)));
        }

        /// <summary>显示本次节点结果、分支轨迹和仿真报警。</summary>
        private void ApplyNotice(FlowNotice notice)
        {
            graph.States[notice.NodeId] = language.Text(notice.State);
            graph.Invalidate();
            int index = logGrid.Rows.Add(notice.NodeId, language.Text(notice.State), notice.AlarmCode, notice.Detail);
            logGrid.Rows[index].Tag = notice;
            if (notice.Outputs == null) return;
            foreach (var pair in notice.Outputs) resultGrid.Rows.Add(notice.NodeId, pair.Key, ActionCatalog.Format(pair.Value));
        }

        /// <summary>协作停止当前动作；已提交的本地记录不会被回滚删除。</summary>
        private void StopFlow(object sender, EventArgs e) { if (stop != null) stop.Cancel(); }

        /// <summary>运行期间锁定编辑，但保留停止和语言切换。</summary>
        private void SetRunning(bool value)
        {
            foreach (var button in new[] { newButton, openButton, saveButton, validateButton, runButton, addButton, deleteButton, deviceAddButton, deviceDeleteButton, addFieldButton, folderButton }) button.Enabled = !value;
            nodeGrid.ReadOnly = value; deviceGrid.ReadOnly = value; inputGrid.ReadOnly = value; outputBox.ReadOnly = value;
            stopButton.Enabled = value;
            if (!value && current != null) addFieldButton.Enabled = catalog.Find(current.ActionId).ExtraInputs;
        }

        /// <summary>选择生产数据保存根目录。</summary>
        private void ChooseFolder(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog { SelectedPath = outputBox.Text }) if (dialog.ShowDialog(this) == DialogResult.OK) outputBox.Text = dialog.SelectedPath;
        }

        /// <summary>打开当前本地结果目录。</summary>
        private void ShowFolder(object sender, EventArgs e)
        {
            try { string path = Path.GetFullPath(outputBox.Text); Directory.CreateDirectory(path); Process.Start("explorer.exe", "\"" + path + "\""); }
            catch (Exception error) { Report(error); }
        }

        /// <summary>按框架语言标识切换文本，不提交或重建参数行。</summary>
        private void ChangeLanguage(object sender, EventArgs e)
        {
            try { language.Switch(Convert.ToString(languageBox.SelectedItem)); ApplyLanguage(); }
            catch (Exception error) { Report(error); }
        }

        /// <summary>刷新控件、动作显示和缓存状态，保留业务字段原值。</summary>
        private void ApplyLanguage()
        {
            language.RegisterAndApplyLanguage(this);
            foreach (var choice in actionChoices) choice.Text = language.Text("Action_" + choice.Id.Replace('.', '_'));
            foreach (var choice in sourceChoices) if (choice.Id == "") choice.Text = language.Text("Constant");
            foreach (DataGridViewRow row in logGrid.Rows)
            {
                var notice = row.Tag as FlowNotice;
                if (notice != null) { row.Cells[1].Value = language.Text(notice.State); graph.States[notice.NodeId] = language.Text(notice.State); }
            }
            if (current != null) selectedLabel.Text = current.Id + " · " + language.Text("Action_" + current.ActionId.Replace('.', '_'));
            nodeGrid.Invalidate(); inputGrid.Invalidate(); graph.Invalidate(); RefreshStatus();
        }

        /// <summary>以当前语言解释已知校验错误，保留外部错误详情。</summary>
        private void Report(Exception error)
        {
            var issue = error as FlowIssue;
            if (issue == null) SetStatus(error.Message);
            else SetMessage(issue.Key, issue.Arguments);
        }

        /// <summary>更新底部状态和可复制的完整提示。</summary>
        private void SetStatus(string text) { statusKey = null; statusArguments = null; statusBox.Text = text; }

        /// <summary>缓存文案键与参数，切换语言时重新解释。</summary>
        private void SetMessage(string key, params object[] arguments) { statusKey = key; statusArguments = arguments; RefreshStatus(); }

        /// <summary>刷新当前状态，不改写业务参数和外部异常原文。</summary>
        private void RefreshStatus()
        {
            if (statusKey == null) return;
            statusBox.Text = statusKey == "RunSummary" ? language.Text(statusKey, language.Text((string)statusArguments[0]), statusArguments[1]) : language.Text(statusKey, statusArguments);
        }

        /// <summary>等待运行停止后再关闭，避免丢弃仍在写盘的任务。</summary>
        private async void CloseWorkbench(object sender, FormClosingEventArgs e)
        {
            if (stop == null || execution == null) return;
            e.Cancel = true;
            if (closing) return;
            closing = true;
            stop.Cancel();
            try { await execution; } catch (Exception error) { System.Diagnostics.Trace.WriteLine(error); }
            closing = false;
            BeginInvoke(new Action(Close));
        }
    }
}
