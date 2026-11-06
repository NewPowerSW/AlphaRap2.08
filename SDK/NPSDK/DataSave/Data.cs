using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataSave
{
    public partial class Data : UserControl
    {
        private int mark = -1;
        private int queryVersion;
        private DateTime lastQueryStart = DateTime.Today;
        private DateTime lastQueryEnd = DateTime.Today;

        private static readonly Color HeaderBg = Color.FromArgb(220, 232, 246);
        private static readonly Color HeaderFg = Color.FromArgb(45, 70, 110);
        private static readonly Color SelectBg = Color.FromArgb(210, 228, 250);
        private static readonly Color CardBorder = Color.FromArgb(210, 218, 230);

        public Data()
        {
            InitializeComponent();
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }
            ProcudtDataSlect.SelectedIndexChanged += (sender, args) => {
                queryVersion++; listView1.Items.Clear(); listView1.Columns.Clear(); UpdateStats(0, 0, 0); SetStatus("");
            };
        }

        private List<TypeCollection1> items = new List<TypeCollection1>();

        [TypeConverter(typeof(System.ComponentModel.CollectionConverter))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Category("#ADLIK"), Description("图像文件集")]
        public List<TypeCollection1> ParameterSet
        {
            get { return items; }
            set { items = value; }
        }

        private void Data_Load(object sender, EventArgs e)
        {
            if (GetService(typeof(System.ComponentModel.Design.IDesignerHost)) != null
                || LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            SelectType.SelectedIndex = 0;
            ProcudtDataChange();
            UpdateStats(0, 0, 0);
            SetStatus("Select a date range and click Query");
        }

        /// <summary>刷新可查询模块并保留当前选择，使过期查询结果失效。</summary>
        public void ReloadSources()
        {
            if (InvokeRequired) { Invoke(new Action(ReloadSources)); return; }
            queryVersion++;
            string selected = Convert.ToString(ProcudtDataSlect.SelectedItem);
            ProcudtDataSlect.Items.Clear();
            foreach (TypeCollection1 source in ParameterSet) ProcudtDataSlect.Items.Add(source.Text);
            int index = ProcudtDataSlect.Items.IndexOf(selected);
            if (ProcudtDataSlect.Items.Count > 0) ProcudtDataSlect.SelectedIndex = index < 0 ? 0 : index;
        }

        /// <summary>初次显示时绑定已有日志模块。</summary>
        private void ProcudtDataChange() { ReloadSources(); }

        private void button1_Click(object sender, EventArgs e)
        {
            QueryByRange(dateTimePicker1.Value.Date, dateTimePicker2.Value.Date);
        }

        private void buttonToday_Click(object sender, EventArgs e)
        {
            dateTimePicker1.Value = DateTime.Today;
            dateTimePicker2.Value = DateTime.Today;
            QueryByRange(dateTimePicker1.Value.Date, dateTimePicker2.Value.Date);
        }

        private void button7Days_Click(object sender, EventArgs e)
        {
            dateTimePicker2.Value = DateTime.Today;
            dateTimePicker1.Value = DateTime.Today.AddDays(-6);
            QueryByRange(dateTimePicker1.Value.Date, dateTimePicker2.Value.Date);
        }

        private void button30Days_Click(object sender, EventArgs e)
        {
            dateTimePicker2.Value = DateTime.Today;
            dateTimePicker1.Value = DateTime.Today.AddDays(-29);
            QueryByRange(dateTimePicker1.Value.Date, dateTimePicker2.Value.Date);
        }

        private void QueryByRange(DateTime start, DateTime end)
        {
            if (end < start)
            {
                DateTime swap = start;
                start = end;
                end = swap;
                dateTimePicker1.Value = start;
                dateTimePicker2.Value = end;
            }

            lastQueryStart = start;
            lastQueryEnd = end;
            SetStatus("Querying...");
            ReadData(start, end, 0);
        }

        /// <summary>捕获选定日志实例，在后台查询后仅发布当前请求的结果。</summary>
        public void ReadData(DateTime startdate, DateTime enddate, int ReadQuantity)
        {
            if (ParameterSet.Count == 0)
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show(" ParameterSet is null, please check!", "Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        UpdateStats(0, DaySpan(startdate, enddate), 0);
                        SetStatus("ParameterSet is empty");
                    }));
                }
                else
                {
                    MessageBox.Show(" ParameterSet is null, please check!", "Note", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    UpdateStats(0, DaySpan(startdate, enddate), 0);
                    SetStatus("ParameterSet is empty");
                }
                return;
            }

            int selectedIndex = -1;
            string filter = "ALL";
            string pathHint = "";
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    selectedIndex = ProcudtDataSlect.SelectedIndex;
                    filter = SelectType.SelectedItem != null ? SelectType.SelectedItem.ToString() : "ALL";
                    if (selectedIndex >= 0)
                        pathHint = ParameterSet[selectedIndex].SavePath ?? "";
                }));
            }
            else
            {
                selectedIndex = ProcudtDataSlect.SelectedIndex;
                filter = SelectType.SelectedItem != null ? SelectType.SelectedItem.ToString() : "ALL";
                if (selectedIndex >= 0)
                    pathHint = ParameterSet[selectedIndex].SavePath ?? "";
            }

            if (selectedIndex < 0)
            {
                UpdateStats(0, DaySpan(startdate, enddate), 0);
                SetStatus("Please select a data source");
                return;
            }

            TypeCollection1 source = ParameterSet[selectedIndex];
            int version = ++queryVersion;
            Action work = () =>
            {
                try
                {
                    List<string[]> returndata = source.ReadDataFromToFile(startdate, enddate);
                    PostQuery(() => {
                        if (version == queryVersion && !IsDisposed) ApplyQueryResult(returndata, startdate, enddate, ReadQuantity, filter, pathHint);
                    });
                }
                catch (Exception error)
                {
                    PostQuery(() => { if (version == queryVersion) SetStatus(error.Message); });
                }
            };

            if (InvokeRequired)
                work();
            else
                Task.Factory.StartNew(work);
        }

        /// <summary>查询完成后安全返回 UI，窗口关闭时不再投递回调。</summary>
        private void PostQuery(Action update)
        {
            if (IsDisposed || !IsHandleCreated) return;
            try { BeginInvoke(update); }
            catch (InvalidOperationException) { }
        }

        /// <summary>按实际文件表头呈现历史记录，不采用已经切换的模块字段。</summary>
        private void ApplyQueryResult(List<string[]> returndata, DateTime starTime1, DateTime endTime1, int ReadQuantity, string filter, string pathHint)
        {
            listView1.Columns.Clear();
            mark = -1;
            listView1.BeginUpdate();
            listView1.Items.Clear();

            if (returndata == null || returndata.Count == 0)
            {
                listView1.EndUpdate();
                UpdateStats(0, DaySpan(starTime1, endTime1), 0);
                SetStatus(string.IsNullOrEmpty(pathHint)
                    ? "No data in this range"
                    : "No data in this range (" + pathHint + ")");
                return;
            }

            int columnCount = returndata[0].Length;
            for (int i = 0; i < columnCount; i++)
                listView1.Columns.Add(returndata[0][i], i == 0 ? 180 : Math.Max(110, (listView1.Width - 180) / Math.Max(1, columnCount - 1)), HorizontalAlignment.Left);

            if (ReadQuantity < returndata.Count && ReadQuantity != 0)
            {
                for (int i = returndata.Count - ReadQuantity + 1; i < returndata.Count; i++)
                    TryAddRow(returndata[i], filter);
            }
            else
            {
                for (int i = 1; i < returndata.Count; i++)
                    TryAddRow(returndata[i], filter);
            }

            listView1.EndUpdate();
            UpdateStats(listView1.Items.Count, DaySpan(starTime1, endTime1), listView1.Columns.Count);
            SetStatus(listView1.Items.Count == 0
                ? "No matching records for filter " + filter
                : listView1.Items.Count + " record(s) loaded");
        }

        private void TryAddRow(string[] row, string filter)
        {
            if (filter == "ALL")
            {
                listView1.Items.Add(new ListViewItem(row));
            }
            else if (filter == "PASS")
            {
                if (row.Length > 2 && row[2] == "PASS")
                    listView1.Items.Add(new ListViewItem(row));
            }
            else if (filter == "FAIL")
            {
                if (row.Length > 2 && row[2] == "FAIL")
                    listView1.Items.Add(new ListViewItem(row));
            }
        }

        private static int DaySpan(DateTime start, DateTime end)
        {
            return Math.Max(1, (end.Date - start.Date).Days + 1);
        }

        private void UpdateStats(int records, int days, int columns)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateStats(records, days, columns)));
                return;
            }

            lblStatRecordsValue.Text = records.ToString();
            lblStatDaysValue.Text = days.ToString();
            lblStatColumnsValue.Text = columns.ToString();
        }

        private void SetStatus(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => SetStatus(text)));
                return;
            }

            lblStatus.Text = text;
        }

        private void CardPanel_Paint(object sender, PaintEventArgs e)
        {
            Panel panel = sender as Panel;
            if (panel == null)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
            using (Pen pen = new Pen(CardBorder))
            {
                using (GraphicsPath path = RoundRect(rect, 6))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }

        private static GraphicsPath RoundRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void listView1_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void listView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(HeaderBg))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }
            using (Pen pen = new Pen(CardBorder))
            {
                e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            }
            TextRenderer.DrawText(
                e.Graphics,
                e.Header.Text,
                e.Font,
                e.Bounds,
                HeaderFg,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            listView1.FullRowSelect = true;
            if (listView1.SelectedIndices.Count > 0)
            {
                if (mark == -1)
                {
                    listView1.SelectedItems[0].BackColor = SelectBg;
                    mark = listView1.SelectedIndices[0];
                }
                else if (listView1.SelectedIndices[0] != mark)
                {
                    listView1.Items[listView1.SelectedIndices[0]].BackColor = SelectBg;
                    listView1.Items[mark].BackColor = Color.White;
                    mark = listView1.SelectedIndices[0];
                }
            }
            else if (mark >= 0 && mark < listView1.Items.Count)
            {
                listView1.Items[mark].BackColor = Color.White;
                mark = -1;
            }
        }
    }
}
