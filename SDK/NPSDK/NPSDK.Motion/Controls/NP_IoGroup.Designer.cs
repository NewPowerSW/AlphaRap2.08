using System.Drawing;
using System.Windows.Forms;

namespace NPSDK.Motion
{
    partial class NP_IoGroup
    {
        /// <summary>组合只呈现实际 IO 卡片，属性和删除交由宿主右键菜单。</summary>
        private void InitializeComponent()
        {
            Name = "NP_IoGroup"; Size = new Size(310,40); MinimumSize = new Size(240,40); Margin = new Padding(5);
            Font = new Font("Microsoft YaHei",9F); BorderStyle = BorderStyle.None; AutoScaleMode = AutoScaleMode.None;
        }
    }
}
