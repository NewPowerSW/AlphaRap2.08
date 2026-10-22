using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NPSDK.Design
{
    public partial class FlowDesignerForm : Form
    {
        private readonly FlowDesignerEnvironment environment;
        private FlowDesignDocument document;
        private string selectedId;
        private FlowChartSession session;
        private bool executing;
        private bool closing;
        private bool loading;
        private FlowNodeDialog currentDialog;
        private Func<string> statusText;
        private readonly Dictionary<string, Flow_Chart.FlowChartHandle> callbacks = new Dictionary<string, Flow_Chart.FlowChartHandle>();
        public FlowDesignDocument Document
        {
            get
            {
                return document;
            }
        }

        public FlowDesignerCanvas Canvas
        {
            get
            {
                return canvas;
            }
        }

        public bool Executing
        {
            get
            {
                return executing;
            }
        }

        public FlowChartSession Session
        {
            get
            {
                return session;
            }
        }

        /// <summary>以无硬件副作用的示例提供设计器预览。</summary>
        public FlowDesignerForm() : this(FlowDesignerDemo.CreateEnvironment())
        {
        }

        /// <summary>由宿主注入对象、报警和语言机制。</summary>
        public FlowDesignerForm(FlowDesignerEnvironment environment)
        {
            InitializeComponent();
            this.environment = environment;
            foreach (string method in environment.Registry.Bindings.Select(b => b.MethodName).Distinct())
                toolbox.Items.Add(method);
            outputBox.Text = environment.OutputDirectory;
            languageBox.Items.AddRange(new object[] { "Chinese", "English", "Español" });
            languageBox.SelectedItem = environment.Locale;
            toolbox.MouseDown += DragTool;
            canvas.NodeDropped += AddNode;
            canvas.EditRequested += EditNode;
            canvas.Selected += node =>
            {
                selectedId = node.Id;
                selectedLabel.Text = node.Id + " · " + node.Text;
            };
            editButton.Click += (s, e) =>
            {
                var node = SelectedNode();
                if (node != null)
                    EditNode(node);
            };
            deleteButton.Click += DeleteNode;
            exampleButton.Click += (s, e) => LoadDocument(FlowDesignFiles.Copy(environment.Example));
            openButton.Click += OpenDocument;
            saveButton.Click += SaveDocument;
            validateButton.Click += ValidateDocument;
            runButton.Click += (s, e) => StartConfiguredFlow();
            stopButton.Click += (s, e) => RequestStop();
            pauseButton.Click += (s, e) =>
            {
                if (session != null)
                    session.Pause(!session.PauseRequested);
            };
            instancesButton.Click += (s, e) =>
            {
                if (environment.ShowInstances != null)
                    environment.ShowInstances();
            };
            folderButton.Click += OpenOutput;
            entryBox.SelectedIndexChanged += (s, e) =>
            {
                if (!loading)
                    document.Entry = Convert.ToString(entryBox.SelectedItem);
            };
            languageBox.SelectedIndexChanged += (s, e) =>
            {
                if (loading)
                    return;
                environment.Locale = Convert.ToString(languageBox.SelectedItem);
                if (environment.ChangeLanguage != null)
                    environment.ChangeLanguage(environment.Locale);
                ApplyLanguage();
            };
            monitor.Tick += Monitor;
            FormClosing += ClosingDesigner;
            LoadDocument(FlowDesignFiles.Copy(environment.Example));
            ApplyLanguage();
            RestoreDefaultDocument();
            instancesButton.Enabled = environment.ShowInstances != null;
            monitor.Start();
        }

        /// <summary>更新固定标题、属性窗口及运行缓存文本，不重建节点。</summary>
        public void ApplyLanguage()
        {
            FlowDesignerUi.Apply(this, environment);
            if (statusText != null)
                statusBox.Text = statusText();
            foreach (DataGridViewRow row in traceGrid.Rows)
            {
                var notice = row.Tag as FlowChartNotice;
                if (notice != null)
                {
                    row.Cells[1].Value = environment.T(notice.State);
                    row.Cells[3].Value = NoticeDetail(notice);
                }
            }

            if (currentDialog != null)
                currentDialog.ApplyLanguage();
        }

        /// <summary>同步主框架语言选择，不再次触发语言切换或重建业务数据。</summary>
        public void SetLanguageSelection(string language)
        {
            loading = true;
            environment.Locale = language;
            languageBox.SelectedItem = language;
            loading = false;
            ApplyLanguage();
        }

        /// <summary>替换画布文档，只在未执行时进行。</summary>
        public void LoadDocument(FlowDesignDocument value)
        {
            if (executing)
                throw new FlowDesignIssue("Busy");
            environment.Registry.ValidateStructure(value);
            document = value;
            callbacks.Clear();
            if (!string.IsNullOrWhiteSpace(value.OutputDirectory))
                outputBox.Text = value.OutputDirectory;
            canvas.LoadDocument(document);
            loading = true;
            entryBox.Items.Clear();
            entryBox.Items.AddRange(document.Nodes.Select(n => n.Id).ToArray());
            entryBox.SelectedItem = document.Entry;
            loading = false;
            selectedId = document.Nodes.FirstOrDefault() == null ? null : document.Nodes[0].Id;
        }

        /// <summary>从工具箱拖出方法，画布会创建真实 Flow_Chart。</summary>
        private void DragTool(object sender, MouseEventArgs e)
        {
            if (executing || e.Button != MouseButtons.Left || toolbox.SelectedItem == null)
                return;
            toolbox.DoDragDrop(Convert.ToString(toolbox.SelectedItem), DragDropEffects.Copy);
        }

        /// <summary>增加带默认参数的节点，并立即打开其属性。</summary>
        public void AddNode(string method, Point point)
        {
            if (executing)
                return;
            if (document.Nodes.Count >= 300)
            {
                Report(new FlowDesignIssue("InvalidDocument"));
                return;
            }

            var binding = environment.Registry.Bindings.FirstOrDefault(b => b.MethodName == method);
            if (binding == null)
                return;
            int index = 1;
            while (document.Nodes.Any(n => n.Id == "Node" + index))
                index++;
            var node = new FlowDesignNode
            {
                Id = "Node" + index,
                Text = method,
                MethodName = method,
                Target = binding.TargetId,
                X = Math.Max(0, point.X),
                Y = Math.Max(0, point.Y),
                Parameters = binding.Parameters.Select(p => p.Copy()).ToList()
            };
            document.Nodes.Add(node);
            if (document.Nodes.Count == 1)
                document.Entry = node.Id;
            LoadDocument(document);
            selectedId = node.Id;
            EditNode(node);
        }

        /// <summary>双击编辑 SDK 节点；确定后重新绑定真实引用。</summary>
        private void EditNode(FlowDesignNode node)
        {
            if (executing)
                return;
            using (var dialog = new FlowNodeDialog(document, node, environment))
            {
                currentDialog = dialog;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    int index = document.Nodes.IndexOf(node);
                    document.Nodes[index] = dialog.EditedNode;
                    LoadDocument(document);
                    selectedId = dialog.EditedNode.Id;
                }

                currentDialog = null;
            }
        }

        /// <summary>返回当前选择的模型。</summary>
        private FlowDesignNode SelectedNode()
        {
            return document.Nodes.FirstOrDefault(n => n.Id == selectedId);
        }

        /// <summary>删除节点并清除对应跳转，实际 SDK 控件随文档释放。</summary>
        private void DeleteNode(object sender, EventArgs e)
        {
            if (executing || SelectedNode() == null)
                return;
            string id = selectedId;
            document.Nodes.Remove(SelectedNode());
            foreach (var node in document.Nodes)
            {
                if (node.Next == id)
                    node.Next = "";
                if (node.Case1 == id)
                    node.Case1 = "";
                if (node.Case2 == id)
                    node.Case2 = "";
                if (node.Case3 == id)
                    node.Case3 = "";
            }

            if (document.Entry == id)
                document.Entry = document.Nodes.Select(n => n.Id).FirstOrDefault() ?? "";
            LoadDocument(document);
        }

        /// <summary>启动入口 Flow_Chart，后继跳转完全交给 SDK。</summary>
        public void StartConfiguredFlow()
        {
            if (executing)
                return;
            try
            {
                environment.Registry.Validate(document);
                string reason = environment.CanStart == null ? "" : environment.CanStart();
                if (!string.IsNullOrEmpty(reason))
                {
                    SetStatus(() => environment.CanStart());
                    return;
                }

                var frozen = FlowDesignFiles.Copy(document);
                session = new FlowChartSession(environment.Registry, Path.GetFullPath(outputBox.Text));
                session.ExternalPause = environment.ExternalPause;
                session.Notice += ReceiveNotice;
                resultGrid.Rows.Clear();
                traceGrid.Rows.Clear();
                foreach (var definition in frozen.Nodes)
                {
                    var view = canvas.Nodes[definition.Id];
                    Flow_Chart.FlowChartHandle previous;
                    if (callbacks.TryGetValue(definition.Id, out previous))
                        view.FlowChartRun -= previous;
                    Flow_Chart.FlowChartHandle callback = () => session.Tick(definition);
                    callbacks[definition.Id] = callback;
                    view.FlowChartRun += callback;
                    view.ExecutionScope = session;
                    view.ConfiguredCanPause = definition.CanPause;
                    view.AlarmStaus = false;
                }

                SetExecuting(true);
                canvas.Nodes[frozen.Entry].FlowChart_Run();
            }
            catch (Exception error)
            {
                Report(error);
                SetExecuting(false);
            }
        }

        /// <summary>只停止本设计器范围，设备操作按绑定的 Cancel 处理。</summary>
        public void RequestStop()
        {
            if (session != null)
                session.Stop();
        }

        /// <summary>由框架暂停入口通知本设计器。</summary>
        public void RequestPause(bool value)
        {
            if (session != null)
                session.Pause(value);
        }

        /// <summary>等待 SDK worker 和异步设备回包真正结束后解锁编辑。</summary>
        private void Monitor(object sender, EventArgs e)
        {
            if (!executing || session == null)
                return;
            if (session.State == "Running" || canvas.Nodes.Values.Any(n => n.IsExecutionBusy) || session.PendingWork)
                return;
            SetExecuting(false);
            SetStatus(() => environment.T(session.State) + "  RunId=" + session.RunId);
            if (closing)
                BeginInvoke(new Action(Close));
        }

        /// <summary>派发运行观察，不从 worker 直接修改界面。</summary>
        private void ReceiveNotice(FlowChartNotice notice)
        {
            if (!IsHandleCreated || IsDisposed)
                return;
            BeginInvoke(new Action(() =>
            {
                if (IsDisposed)
                    return;
                if (traceGrid.Rows.Count >= 2000)
                    traceGrid.Rows.RemoveAt(0);
                int row = traceGrid.Rows.Add(notice.NodeId, environment.T(notice.State), notice.AlarmCode, NoticeDetail(notice));
                traceGrid.Rows[row].Tag = notice;
                SetStatus(() => notice.NodeId + " · " + environment.T(notice.State));
                if (notice.Values != null)
                    foreach (var pair in notice.Values)
                    {
                        string key = notice.NodeId + "." + pair.Key;
                        var existing = resultGrid.Rows.Cast<DataGridViewRow>().FirstOrDefault(r => (string)r.Tag == key);
                        if (existing == null)
                        {
                            int added = resultGrid.Rows.Add(notice.NodeId, pair.Key, "");
                            existing = resultGrid.Rows[added];
                            existing.Tag = key;
                        }

                        existing.Cells[2].Value = Convert.ToString(pair.Value, System.Globalization.CultureInfo.InvariantCulture);
                    }
            }));
        }

        /// <summary>锁定配置但保留实例观察、停止及语言刷新。</summary>
        private void SetExecuting(bool value)
        {
            executing = value;
            canvas.Editing = !value;
            foreach (var control in new Control[]
            {
                exampleButton,
                openButton,
                saveButton,
                validateButton,
                runButton,
                editButton,
                deleteButton,
                toolbox,
                entryBox,
                outputBox
            }

            )
                control.Enabled = !value;
            pauseButton.Enabled = value;
            stopButton.Enabled = value;
        }

        /// <summary>从 XML 读取配置并验证绑定身份。</summary>
        private void OpenDocument(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog
            {
                Filter = "XML|*.xml"
            }

            )
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;
                try
                {
                    var loaded = FlowDesignFiles.Load(dialog.FileName);
                    LoadDocument(loaded);
                    ShowRestoredStatus(dialog.FileName);
                }
                catch (Exception error)
                {
                    Report(error);
                }
            }
        }

        /// <summary>将画布、动作与判据原子保存，不修改已有 C# 流程。</summary>
        private void SaveDocument(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(environment.DefaultPath))
                {
                    SaveToPath(environment.DefaultPath);
                    return;
                }

                using (var dialog = new SaveFileDialog
                {
                    Filter = "XML|*.xml",
                    FileName = "FlowChart.xml"
                }

                )
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                        SaveToPath(dialog.FileName);
            }
            catch (Exception error)
            {
                Report(error);
            }
        }

        /// <summary>保存完整设计及输出目录，保存草稿不会启动设备。</summary>
        public void SaveToPath(string path)
        {
            if (executing)
                throw new FlowDesignIssue("Busy");
            environment.Registry.ValidateStructure(document);
            document.OutputDirectory = Path.GetFullPath(outputBox.Text);
            FlowDesignFiles.Save(path, document);
            ShowDesignStatus("SavedDesign", path);
        }

        /// <summary>启动后恢复默认文件；损坏时尝试上次备份，均失败则保留示例且不覆盖原文件。</summary>
        private void RestoreDefaultDocument()
        {
            string path = environment.DefaultPath;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return;
            try
            {
                LoadDocument(FlowDesignFiles.Load(path));
                ShowRestoredStatus(path);
            }
            catch (Exception error)
            {
                try
                {
                    LoadDocument(FlowDesignFiles.Load(path + ".bak"));
                    ShowDesignStatus("RestoredBackup", path + ".bak");
                }
                catch (Exception)
                {
                    SetStatus(() => string.Format(environment.T("RestoreFailed"), path, error.Message));
                }
            }
        }

        /// <summary>恢复离线画布并指出当前缺失或变更的绑定，保留用户配置。</summary>
        private void ShowRestoredStatus(string path)
        {
            ShowDesignStatus("RestoredDesign", path);
        }

        /// <summary>缓存可重新翻译的保存或恢复状态，并附上当前绑定校验结果。</summary>
        private void ShowDesignStatus(string key, string path)
        {
            string absolutePath = Path.GetFullPath(path);
            SetStatus(() => string.Format(environment.T(key), absolutePath));
            try
            {
                environment.Registry.Validate(document);
            }
            catch (FlowDesignIssue issue)
            {
                SetStatus(() => string.Format(environment.T(key), absolutePath) + "  " + string.Format(environment.T("BindingNeedsReview"), string.Format(environment.T(issue.Key), issue.Values)));
            }
        }

        /// <summary>检查方法、对象、参数和跳转，不触发动作。</summary>
        private void ValidateDocument(object sender, EventArgs e)
        {
            try
            {
                environment.Registry.Validate(document);
                SetStatus(() => environment.T("Validated"));
            }
            catch (Exception error)
            {
                Report(error);
            }
        }

        /// <summary>打开实际生产记录目录。</summary>
        private void OpenOutput(object sender, EventArgs e)
        {
            try
            {
                string path = Path.GetFullPath(outputBox.Text);
                Directory.CreateDirectory(path);
                System.Diagnostics.Process.Start("explorer.exe", "\"" + path + "\"");
            }
            catch (Exception error)
            {
                Report(error);
            }
        }

        /// <summary>按宿主语言显示错误，同时保留外部异常详情。</summary>
        private void Report(Exception error)
        {
            var issue = error as FlowDesignIssue;
            SetStatus(() => issue == null ? error.Message : string.Format(environment.T(issue.Key), issue.Values));
        }

        /// <summary>保存动态文案生成器，语言切换时只更新展示文字。</summary>
        private void SetStatus(Func<string> text)
        {
            statusText = text;
            statusBox.Text = text();
        }

        /// <summary>翻译框架运行错误，设备返回的原始详情保持原样。</summary>
        private string NoticeDetail(FlowChartNotice notice)
        {
            return notice.Issue == null ? notice.Detail : string.Format(environment.T(notice.Issue.Key), notice.Issue.Values);
        }

        /// <summary>关闭先等待实际 SDK 节点和异步操作停止。</summary>
        private void ClosingDesigner(object sender, FormClosingEventArgs e)
        {
            if (!executing)
                return;
            e.Cancel = true;
            closing = true;
            RequestStop();
        }
    }
}
