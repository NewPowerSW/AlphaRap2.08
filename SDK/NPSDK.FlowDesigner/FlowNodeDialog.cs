using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NPSDK.Design
{
    public partial class FlowNodeDialog : Form
    {
        private readonly FlowDesignerEnvironment environment;
        private readonly FlowDesignDocument document;
        private readonly List<SourceChoice> sources = new List<SourceChoice>();
        private bool loading;
        private bool propertyLanguagePending;
        private FlowDesignIssue lastIssue;
        public FlowDesignNode EditedNode { get; private set; }

        private readonly FlowNodeProperties properties;
        public sealed class SourceChoice
        {
            public string Id { get; set; }
            public string Text { get; set; }
        }

        /// <summary>提供无硬件访问的 WinForms 设计器预览。</summary>
        public FlowNodeDialog() : this(FlowDesignerDemo.CreateEnvironment())
        {
        }

        /// <summary>用同一示例环境连接预览属性和参数。</summary>
        private FlowNodeDialog(FlowDesignerEnvironment environment) : this(environment.Example, environment.Example.Nodes[1], environment)
        {
        }

        /// <summary>在副本上编辑节点，取消不会改动实际画布与 SDK 回调。</summary>
        public FlowNodeDialog(FlowDesignDocument document, FlowDesignNode node, FlowDesignerEnvironment environment)
        {
            InitializeComponent();
            this.environment = environment;
            this.document = FlowDesignFiles.Copy(document);
            EditedNode = this.document.Nodes.First(n => n.Id == node.Id);
            properties = new FlowNodeProperties(EditedNode, this.document, environment);
            propertyGrid.SelectedObject = properties;
            propertyGrid.PropertyValueChanged += (s, e) => BindParameters();
            propertyGrid.Leave += (s, e) =>
            {
                if (!propertyLanguagePending)
                    return;
                propertyLanguagePending = false;
                propertyGrid.Refresh();
            };
            parameterGrid.CellValueChanged += ParametersChanged;
            parameterGrid.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (parameterGrid.IsCurrentCellDirty && parameterGrid.CurrentCell is DataGridViewComboBoxCell)
                    parameterGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            parameterGrid.DataError += (s, e) =>
            {
                e.ThrowException = false;
                lastIssue = new FlowDesignIssue("InvalidParameter");
                errorBox.Text = environment.T(lastIssue.Key);
            };
            addParameter.Click += AddParameter;
            deleteParameter.Click += DeleteParameter;
            okButton.Click += Accept;
            ApplyLanguage();
            BindParameters();
        }

        /// <summary>刷新展示文字，保留属性对象和未提交参数。</summary>
        public void ApplyLanguage()
        {
            FlowDesignerUi.Apply(this, environment);
            foreach (var source in sources)
                if (source.Id == "")
                    source.Text = environment.T("Literal");
            if (propertyGrid.ContainsFocus)
                propertyLanguagePending = true;
            else
                propertyGrid.Refresh();
            if (lastIssue != null)
                errorBox.Text = string.Format(environment.T(lastIssue.Key), lastIssue.Values);
            parameterGrid.Invalidate();
        }

        /// <summary>根据所选方法显示有类型参数和前驱数据绑定。</summary>
        private void BindParameters()
        {
            loading = true;
            parameterGrid.Rows.Clear();
            sourceColumn.DataSource = null;
            sources.Clear();
            sources.Add(new SourceChoice { Id = "", Text = environment.T("Literal") });
            foreach (var node in document.Nodes)
            {
                var binding = environment.Registry.Find(node.MethodName, node.Target);
                if (binding == null)
                    continue;
                foreach (string output in binding.Outputs)
                    sources.Add(new SourceChoice { Id = node.Id + "." + output, Text = node.Id + "." + output });
            }

            sourceColumn.DataSource = sources;
            sourceColumn.ValueMember = "Id";
            sourceColumn.DisplayMember = "Text";
            foreach (var parameter in EditedNode.Parameters)
            {
                if (!sources.Any(s => s.Id == parameter.Source))
                    sources.Add(new SourceChoice { Id = parameter.Source, Text = parameter.Source });
                int row = parameterGrid.Rows.Add(parameter.Name, parameter.Type, parameter.Source, parameter.Value);
                var selected = environment.Registry.Find(EditedNode.MethodName, EditedNode.Target);
                bool extra = EditedNode.MethodName == "Data.Save" && selected != null && !selected.Parameters.Any(p => p.Name == parameter.Name);
                parameterGrid.Rows[row].Cells[0].ReadOnly = !extra;
                parameterGrid.Rows[row].Cells[1].ReadOnly = !extra;
                parameterGrid.Rows[row].Cells[3].ReadOnly = !string.IsNullOrEmpty(parameter.Source);
            }

            addParameter.Enabled = EditedNode.MethodName == "Data.Save";
            deleteParameter.Enabled = EditedNode.MethodName == "Data.Save";
            loading = false;
        }

        /// <summary>更新映射值，引用字段的常量格保持只读。</summary>
        private void ParametersChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (loading || e.RowIndex < 0)
                return;
            var row = parameterGrid.Rows[e.RowIndex];
            parameterGrid.Rows[e.RowIndex].Cells[3].ReadOnly = Convert.ToString(row.Cells[2].Value) != "";
            EditedNode.Parameters = parameterGrid.Rows.Cast<DataGridViewRow>().Select(r => new FlowDesignParameter { Name = Convert.ToString(r.Cells[0].Value), Type = Convert.ToString(r.Cells[1].Value), Source = Convert.ToString(r.Cells[2].Value), Value = Convert.ToString(r.Cells[3].Value) }).ToList();
        }

        /// <summary>增加本地记录的自定义字段。</summary>
        private void AddParameter(object sender, EventArgs e)
        {
            if (!parameterGrid.EndEdit())
                return;
            int number = 1;
            while (EditedNode.Parameters.Any(p => p.Name == "Field" + number))
                number++;
            EditedNode.Parameters.Add(new FlowDesignParameter { Name = "Field" + number });
            BindParameters();
        }

        /// <summary>删除所选自定义记录字段，动作必需参数保持完整。</summary>
        private void DeleteParameter(object sender, EventArgs e)
        {
            if (!parameterGrid.EndEdit() || parameterGrid.CurrentCell == null || EditedNode.MethodName != "Data.Save")
                return;
            int index = parameterGrid.CurrentCell.RowIndex;
            var binding = environment.Registry.Find(EditedNode.MethodName, EditedNode.Target);
            if (binding == null)
                return;
            if (binding.Parameters.Any(p => p.Name == EditedNode.Parameters[index].Name))
                return;
            EditedNode.Parameters.RemoveAt(index);
            BindParameters();
        }

        /// <summary>验证编辑副本后提交；不执行任何设备操作。</summary>
        private void Accept(object sender, EventArgs e)
        {
            if (!parameterGrid.EndEdit())
                return;
            try
            {
                environment.Registry.ValidateNode(document, EditedNode, new HashSet<string>(document.Nodes.Select(n => n.Id)));
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (FlowDesignIssue issue)
            {
                lastIssue = issue;
                errorBox.Text = string.Format(environment.T(issue.Key), issue.Values);
            }
            catch (Exception error)
            {
                errorBox.Text = error.Message;
            }
        }
    }
}
