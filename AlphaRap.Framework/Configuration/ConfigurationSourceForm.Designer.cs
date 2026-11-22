using System.Drawing;
using System.Windows.Forms;

namespace AlphaRap.Configuration
{
    partial class ConfigurationSourceForm
    {
        private Label sourcePathLabel, sourceSearchLabel;
        private TextBox sourcePath, sourceSearch, sourceDetails;
        private ListBox sourceList;
        private Button sourceCopy, sourceClose;

        /// <summary>布置文件路径、资源查找、详情和关闭按钮。</summary>
        private void InitializeComponent()
        {
            sourcePathLabel = new Label();
            sourceSearchLabel = new Label();
            sourcePath = new TextBox();
            sourceSearch = new TextBox();
            sourceDetails = new TextBox();
            sourceList = new ListBox();
            sourceCopy = new Button();
            sourceClose = new Button();
            SuspendLayout();
            sourcePathLabel.AutoSize = true;
            sourcePathLabel.Location = new Point(12, 12);
            sourcePathLabel.Name = "sourcePathLabel";
            sourcePathLabel.Text = "配置文件";
            sourcePath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            sourcePath.Location = new Point(12, 35);
            sourcePath.Name = "sourcePath";
            sourcePath.ReadOnly = true;
            sourcePath.Size = new Size(626, 23);
            sourcePath.TabIndex = 1;
            sourceCopy.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            sourceCopy.Location = new Point(646, 33);
            sourceCopy.Name = "sourceCopy";
            sourceCopy.Size = new Size(102, 27);
            sourceCopy.TabIndex = 2;
            sourceCopy.Text = "复制路径";
            sourceSearchLabel.AutoSize = true;
            sourceSearchLabel.Location = new Point(12, 74);
            sourceSearchLabel.Name = "sourceSearchLabel";
            sourceSearchLabel.Text = "查找名称或 ID";
            sourceSearch.Location = new Point(12, 96);
            sourceSearch.Name = "sourceSearch";
            sourceSearch.Size = new Size(222, 23);
            sourceSearch.TabIndex = 0;
            sourceList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            sourceList.HorizontalScrollbar = true;
            sourceList.IntegralHeight = false;
            sourceList.Location = new Point(12, 126);
            sourceList.Name = "sourceList";
            sourceList.Size = new Size(222, 234);
            sourceList.TabIndex = 3;
            sourceDetails.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            sourceDetails.Location = new Point(244, 96);
            sourceDetails.Multiline = true;
            sourceDetails.Name = "sourceDetails";
            sourceDetails.ReadOnly = true;
            sourceDetails.ScrollBars = ScrollBars.Vertical;
            sourceDetails.Size = new Size(504, 264);
            sourceDetails.TabIndex = 4;
            sourceClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            sourceClose.DialogResult = DialogResult.Cancel;
            sourceClose.Location = new Point(646, 370);
            sourceClose.Name = "sourceClose";
            sourceClose.Size = new Size(102, 30);
            sourceClose.TabIndex = 5;
            sourceClose.Text = "关闭";
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = sourceClose;
            ClientSize = new Size(760, 412);
            Controls.AddRange(new Control[] { sourcePathLabel, sourcePath, sourceCopy, sourceSearchLabel, sourceSearch, sourceList, sourceDetails, sourceClose });
            Font = new Font("Microsoft YaHei UI", 9F);
            MinimumSize = new Size(660, 390);
            Name = "ConfigurationSourceForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "配置来源";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
