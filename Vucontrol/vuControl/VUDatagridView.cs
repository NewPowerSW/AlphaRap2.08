using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using LogSv;
using Newtonsoft.Json;

namespace VUControl
{
	public class VUDatagridView : UserControl
	{
		public delegate void delegateSlectedRow(bool bComplete);

		private string[] dgvHeader;

		private string[] columnArr;

		private int currentSelectedIndex;

		private DataGridViewRow selectRow;

		private IContainer components = null;

		public DataGridView myDgv;

		private ContextMenuStrip cMS_RightKeyMenu;

		private ToolStripMenuItem tsl_Add;

		private ToolStripMenuItem tsm_Delete;

		private ToolStripMenuItem tsm_Clear;

		private ToolStripMenuItem tsm_ImportFromFile;

		private ToolStripMenuItem tsm_ImprotFromCSV;

		private ToolStripMenuItem tsm_ImprotFromJson;

		private ToolStripMenuItem tsm_ImprotFromXML;

		private ToolStripMenuItem tsm_ExportToFile;

		private ToolStripMenuItem tsm_ExportToCSV;

		private ToolStripMenuItem tsm_ExportToJson;

		private ToolStripMenuItem tsm_ExportToXml;

		private ToolStripMenuItem tsm_MoveUp;

		private ToolStripMenuItem tsm_MoveDown;

		public string[] DgvHeader
		{
			get
			{
				return dgvHeader;
			}
			set
			{
				if (dgvHeader != null || dgvHeader != value)
				{
					dgvHeader = value;
					InitDatagridView(ref myDgv, dgvHeader);
				}
			}
		}

		public string[] ColumnArr
		{
			get
			{
				return columnArr;
			}
			set
			{
				if (columnArr != null || columnArr != value)
				{
					columnArr = value;
				}
			}
		}

		public int CurrentSelectedIndex
		{
			get
			{
				return currentSelectedIndex;
			}
			set
			{
				if (currentSelectedIndex != value)
				{
					currentSelectedIndex = value;
				}
			}
		}

		public DataGridViewRow SelectRow
		{
			get
			{
				return selectRow;
			}
			set
			{
				if (selectRow != null || selectRow != value)
				{
					selectRow = value;
				}
			}
		}

		public event delegateSlectedRow RaiseSelectedEvent;

		public VUDatagridView()
		{
			InitializeComponent();
		}

		public void LoadXmlFileFromPath(string filePath)
		{
			clsFileOperator.ReadXmlToDataGridView(ref myDgv, filePath);
		}

		public void SaveXmlFileFromPath(string filePath)
		{
			clsFileOperator.SaveXmlPara(myDgv, filePath);
		}

		protected void InitDatagridView(ref DataGridView mydgv, params string[] columnHeader)
		{
			if (mydgv.Columns.Count > 0)
			{
				mydgv.Columns.Clear();
			}
			DataGridViewColumn dataGridViewColumn = new DataGridViewColumn();
			Font font = new Font("Arial", 12f);
			myDgv.Font = font;
			for (int i = 0; i < columnHeader.Length; i++)
			{
				dataGridViewColumn = new DataGridViewTextBoxColumn();
				dataGridViewColumn.HeaderText = columnHeader[i];
				dataGridViewColumn.Name = "col" + i;
				dataGridViewColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
				dataGridViewColumn.ValueType = typeof(string);
				mydgv.Columns.Add(dataGridViewColumn);
			}
		}

		public void tsl_Add_Click(object sender, EventArgs e)
		{
			DataGridViewRow dataGridViewRow = new DataGridViewRow();
			foreach (DataGridViewColumn column in myDgv.Columns)
			{
				dataGridViewRow.Cells.Add(column.CellTemplate.Clone() as DataGridViewCell);
			}
			dataGridViewRow.Cells[0].Value = (myDgv.Rows.Count + 1).ToString();
			myDgv.Rows.Add(dataGridViewRow);
		}

		public void tsm_Delete_Click(object sender, EventArgs e)
		{
			if (myDgv.Rows.Count > 0)
			{
				int index = myDgv.CurrentRow.Index;
				myDgv.Rows.RemoveAt(index);
				Log.log.Write($"删除表格{myDgv.Name.ToString()}的第{index}行.", Color.Black);
			}
		}

		public void tsm_Clear_Click(object sender, EventArgs e)
		{
			try
			{
				myDgv.Rows.Clear();
				Log.log.Write("清空表格" + myDgv.Name.ToString() + "完成!", Color.Black);
			}
			catch (Exception ex)
			{
				Log.log.Write(ex.Message, Color.Red);
			}
		}

		private void tsm_ImprotFromCSV_Click(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			DataTable dataTable = new DataTable();
			openFileDialog.Filter = "CSV文件|*.csv;*.CSV";
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				myDgv.Rows.Clear();
				Log.log.Write("您选择的文件路径为" + openFileDialog.FileName, Color.Black);
				clsFileOperator.ReadCsv(ref myDgv, openFileDialog.FileName, ";");
			}
		}

		private void tsm_ImprotFromJson_Click(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Json file|*.Json;*.json";
			string empty = string.Empty;
			DataTable dt = new DataTable();
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				myDgv.Rows.Clear();
				using (StreamReader streamReader = new StreamReader(openFileDialog.FileName, Encoding.UTF8))
				{
					empty = streamReader.ReadToEnd();
					empty = empty.Replace(Environment.NewLine, ",").TrimEnd(',');
					dt = JsonConvert.DeserializeObject<DataTable>(empty);
					streamReader.Close();
				}
				dataTableToDgv(dt, ref myDgv);
			}
		}

		private void dataTableToDgv(DataTable dt, ref DataGridView dgv)
		{
			for (int i = 0; i < dt.Rows.Count; i++)
			{
				string[] array = new string[dt.Columns.Count];
				for (int j = 0; j < dt.Columns.Count; j++)
				{
					array[j] = dt.Rows[i][j].ToString();
				}
				DataGridViewRowCollection rows = dgv.Rows;
				object[] values = array;
				rows.Add(values);
			}
			dgv.Refresh();
		}

		private void tsm_ImprotFromXML_Click(object sender, EventArgs e)
		{
			string empty = string.Empty;
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "xml files|*.xml";
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				empty = openFileDialog.FileName.ToString();
				clsFileOperator.ReadXmlToDataGridView(ref myDgv, empty);
				Log.log.Write("从路径" + empty + "加载xml文件到" + myDgv.Name + "中....", Color.Black);
			}
		}

		private void tsm_ExportToCSV_Click(object sender, EventArgs e)
		{
			clsFileOperator.DataGridViewExportToCSV(myDgv);
		}

		private void tsm_ExportToJson_Click(object sender, EventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "Json File|*.Json";
			DataTable value = GetdgvToTable(myDgv);
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				using (StreamWriter streamWriter = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
				{
					string text = JsonConvert.SerializeObject(value);
					string newLine = Environment.NewLine;
					text = text.Replace(",", newLine);
					streamWriter.WriteLine(text);
					streamWriter.Flush();
					streamWriter.Close();
				}
			}
		}

		private DataTable GetdgvToTable(DataGridView dgv)
		{
			DataTable dataTable = new DataTable();
			for (int i = 0; i < dgv.Columns.Count; i++)
			{
				DataColumn column = new DataColumn(dgv.Columns[i].Name.ToString());
				dataTable.Columns.Add(column);
			}
			for (int j = 0; j < dgv.Rows.Count; j++)
			{
				DataRow dataRow = dataTable.NewRow();
				for (int k = 0; k < dgv.Columns.Count; k++)
				{
					dataRow[k] = Convert.ToString(dgv.Rows[j].Cells[k].Value);
				}
				dataTable.Rows.Add(dataRow);
			}
			return dataTable;
		}

		private void tsm_ExportToXml_Click(object sender, EventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			string empty = string.Empty;
			saveFileDialog.Filter = "xml files|*.xml;*.XML";
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				empty = saveFileDialog.FileName;
				clsFileOperator.SaveXmlPara(myDgv, empty);
				Log.log.Write("将表格名为:" + myDgv.Name.ToString() + "中的内存到处保存到路径" + empty + "中完成!", Color.Black);
			}
		}

		public void tsm_MoveUp_Click(object sender, EventArgs e)
		{
			if (myDgv.Rows.Count == 0)
			{
				return;
			}
			try
			{
				int index = myDgv.CurrentRow.Index;
				if (index <= 0)
				{
					MessageBox.Show("当前行已经是第一行，不能再向上移动....");
					Log.log.Write("当前行已经是第一行，不能再向上移动....", Color.Black);
				}
				else
				{
					DataGridViewRow currentRow = myDgv.CurrentRow;
					myDgv.Rows.Remove(currentRow);
					myDgv.Rows.Insert(index - 1, currentRow);
				}
			}
			catch (Exception ex)
			{
				Log.log.Write(ex.Message, Color.Black);
			}
		}

		public void tsm_MoveDown_Click(object sender, EventArgs e)
		{
			int num = -1;
			if (myDgv.Rows.Count == 0)
			{
				return;
			}
			try
			{
				num = myDgv.CurrentRow.Index;
				if (num >= myDgv.Rows.Count - 1)
				{
					MessageBox.Show("当前行已经是最后一行，不能再向下移动....");
					Log.log.Write("当前行已经是最后一行，不能再向下移动....", Color.Black);
				}
				else
				{
					DataGridViewRow currentRow = myDgv.CurrentRow;
					myDgv.Rows.Remove(currentRow);
					myDgv.Rows.Insert(num + 1, currentRow);
				}
			}
			catch (Exception ex)
			{
				Log.log.Write(ex.Message, Color.Black);
			}
		}

		private void myDgv_SelectionChanged(object sender, EventArgs e)
		{
			if (myDgv.Rows.Count != 0)
			{
				currentSelectedIndex = myDgv.CurrentRow.Index;
				GetSlectRowData(myDgv, ref selectRow);
				if (this.RaiseSelectedEvent != null)
				{
					this.RaiseSelectedEvent(true);
				}
			}
		}

		public void GetSlectRowData(DataGridView dgv, ref DataGridViewRow selectRow)
		{
			ColumnArr = new string[dgv.ColumnCount];
			selectRow = dgv.CurrentRow;
			if (selectRow.Index != -1 && selectRow.Cells[0].Value != null && selectRow.Cells[dgv.ColumnCount - 1].Value != null)
			{
				for (int i = 0; i < selectRow.Cells.Count && selectRow.Cells[i].Value != null; i++)
				{
					ColumnArr[i] = selectRow.Cells[i].Value.ToString();
				}
			}
		}

		public string[] GetDataArrayByIndex(int rowIndex)
		{
			string[] array = new string[myDgv.ColumnCount];
			if (myDgv.Rows.Count < rowIndex)
			{
				Log.log.Write("表格中的行数小于要查找的行数.....", Color.Red);
				return null;
			}
			DataGridViewRow dataGridViewRow = myDgv.Rows[rowIndex];
			for (int i = 0; i < array.Length && dataGridViewRow.Cells[i].Value != null; i++)
			{
				array[i] = dataGridViewRow.Cells[i].Value.ToString();
			}
			return array;
		}

		public DataGridViewRow GetDataGridRow(int rowIndex)
		{
			if (myDgv.Rows.Count < rowIndex)
			{
				Log.log.Write("表格中的行数小于要查找的行数.....", Color.Red);
				return null;
			}
			return myDgv.Rows[rowIndex];
		}

		public string[] GetDataArrayByFlagName(int columnIndex, string flagStr)
		{
			int num = -999;
			for (int i = 0; i < myDgv.Rows.Count; i++)
			{
				if (myDgv.Rows[i].Cells[columnIndex].Value.ToString() == flagStr)
				{
					num = i;
					Log.log.Write($"在第{num}行的第{columnIndex}列找到了标志字符{flagStr}。", Color.Black);
					break;
				}
			}
			if (num == -999)
			{
				Log.log.Write("在表格中未找到列名为" + flagStr + "的列,请检查列名是否设置正确", Color.Black);
				return null;
			}
			string[] array = new string[myDgv.ColumnCount];
			for (int j = 0; j < array.Length; j++)
			{
				array[j] = myDgv.Rows[num].Cells[j].Value.ToString();
			}
			return array;
		}

		public DataGridViewRow GetDataGridRowByFlagName(int columnIndex, string flagStr)
		{
			int num = -999;
			for (int i = 0; i < myDgv.Rows.Count; i++)
			{
				if (myDgv.Rows[i].Cells[columnIndex].Value.ToString() == flagStr)
				{
					num = i;
					Log.log.Write($"在第{num}行的第{columnIndex}列找到了标志字符{flagStr}。", Color.Black);
					break;
				}
			}
			if (num == -999)
			{
				Log.log.Write("在表格中未找到列名为" + flagStr + "的列,请检查列名是否设置正确.....", Color.Black);
				return null;
			}
			return myDgv.Rows[num];
		}

		private void myDgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
		}

		public void WriteRowToDataGrid(params string[] writeData)
		{
			if (writeData.Length > myDgv.ColumnCount - 1)
			{
				Log.log.Write("向表格中添加的数据的列数大于表格允许存储的最大列数", Color.Red);
				return;
			}
			DataGridViewRow dataGridViewRow = new DataGridViewRow();
			foreach (DataGridViewColumn column in myDgv.Columns)
			{
				dataGridViewRow.Cells.Add(column.CellTemplate.Clone() as DataGridViewCell);
			}
			dataGridViewRow.Cells[0].Value = (myDgv.Rows.Count + 1).ToString();
			for (int i = 0; i < writeData.Length; i++)
			{
				dataGridViewRow.Cells[i + 1].Value = writeData[i];
			}
			myDgv.Rows.Add(dataGridViewRow);
		}

		public void UpdateGridViewRow(int index, int startIndex, int endIndex, params string[] updateStr)
		{
			string[] array = new string[startIndex - 1];
			string[] array2 = new string[myDgv.ColumnCount - endIndex];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = myDgv.Rows[index].Cells[i].Value.ToString();
			}
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j] = myDgv.Rows[index].Cells[j].Value.ToString();
			}
			DataGridViewRow dataGridViewRow = new DataGridViewRow();
			dataGridViewRow = myDgv.Rows[index];
			for (int k = 0; k < array.Length - 1; k++)
			{
				dataGridViewRow.Cells[k].Value = array[k];
			}
			for (int l = startIndex; l < endIndex + 1; l++)
			{
				dataGridViewRow.Cells[l].Value = updateStr[l - startIndex];
			}
			for (int m = 0; m < array2.Length; m++)
			{
				dataGridViewRow.Cells[m].Value = array2[m];
			}
			myDgv.Refresh();
		}

		public void SetBackGroundColor(DataGridView mydgv, int index, Color color)
		{
			mydgv.Rows[index].DefaultCellStyle.BackColor = color;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
			this.myDgv = new System.Windows.Forms.DataGridView();
			this.cMS_RightKeyMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.tsl_Add = new System.Windows.Forms.ToolStripMenuItem();
			this.tsm_Delete = new System.Windows.Forms.ToolStripMenuItem();
			this.tsm_Clear = new System.Windows.Forms.ToolStripMenuItem();
			this.tsm_ImportFromFile = new System.Windows.Forms.ToolStripMenuItem();
			this.tsm_ImprotFromCSV = new System.Windows.Forms.ToolStripMenuItem();
			this.tsm_ImprotFromJson = new System.Windows.Forms.ToolStripMenuItem();
			this.tsm_ImprotFromXML = new System.Windows.Forms.ToolStripMenuItem();
			this.tsm_ExportToFile = new System.Windows.Forms.ToolStripMenuItem();
			this.tsm_ExportToCSV = new System.Windows.Forms.ToolStripMenuItem();
			this.tsm_ExportToJson = new System.Windows.Forms.ToolStripMenuItem();
			this.tsm_ExportToXml = new System.Windows.Forms.ToolStripMenuItem();
			this.tsm_MoveUp = new System.Windows.Forms.ToolStripMenuItem();
			this.tsm_MoveDown = new System.Windows.Forms.ToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)this.myDgv).BeginInit();
			this.cMS_RightKeyMenu.SuspendLayout();
			base.SuspendLayout();
			this.myDgv.AllowUserToAddRows = false;
			this.myDgv.AllowUserToResizeRows = false;
			dataGridViewCellStyle.BackColor = System.Drawing.Color.FromArgb(235, 243, 255);
			this.myDgv.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle;
			this.myDgv.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
			this.myDgv.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
			this.myDgv.BackgroundColor = System.Drawing.Color.White;
			this.myDgv.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(43, 87, 154);
			dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft YaHei UI", 9f);
			dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(43, 87, 154);
			dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.myDgv.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
			this.myDgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.myDgv.ContextMenuStrip = this.cMS_RightKeyMenu;
			dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft YaHei UI", 9f);
			dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(48, 48, 48);
			dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(65, 165, 238);
			dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.FromArgb(48, 48, 48);
			dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.myDgv.DefaultCellStyle = dataGridViewCellStyle3;
			this.myDgv.Dock = System.Windows.Forms.DockStyle.Fill;
			this.myDgv.EnableHeadersVisualStyles = false;
			this.myDgv.Font = new System.Drawing.Font("Microsoft YaHei UI", 9f);
			this.myDgv.GridColor = System.Drawing.Color.FromArgb(43, 87, 154);
			this.myDgv.Location = new System.Drawing.Point(0, 0);
			this.myDgv.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
			this.myDgv.Name = "myDgv";
			dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(235, 243, 255);
			dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft YaHei UI", 13.8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
			dataGridViewCellStyle4.ForeColor = System.Drawing.Color.FromArgb(48, 48, 48);
			dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(43, 87, 154);
			dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.White;
			dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.myDgv.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
			this.myDgv.RowHeadersWidth = 51;
			dataGridViewCellStyle5.BackColor = System.Drawing.Color.White;
			this.myDgv.RowsDefaultCellStyle = dataGridViewCellStyle5;
			this.myDgv.RowTemplate.Height = 23;
			this.myDgv.Size = new System.Drawing.Size(685, 455);
			this.myDgv.TabIndex = 1;
			this.myDgv.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(myDgv_CellFormatting);
			this.myDgv.SelectionChanged += new System.EventHandler(myDgv_SelectionChanged);
			this.cMS_RightKeyMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.cMS_RightKeyMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[7] { this.tsl_Add, this.tsm_Delete, this.tsm_Clear, this.tsm_ImportFromFile, this.tsm_ExportToFile, this.tsm_MoveUp, this.tsm_MoveDown });
			this.cMS_RightKeyMenu.Name = "cMS_RightKeyMenu";
			this.cMS_RightKeyMenu.Size = new System.Drawing.Size(154, 172);
			this.tsl_Add.Name = "tsl_Add";
			this.tsl_Add.Size = new System.Drawing.Size(153, 24);
			this.tsl_Add.Text = "增加";
			this.tsl_Add.Click += new System.EventHandler(tsl_Add_Click);
			this.tsm_Delete.Name = "tsm_Delete";
			this.tsm_Delete.Size = new System.Drawing.Size(153, 24);
			this.tsm_Delete.Text = "删除";
			this.tsm_Delete.Click += new System.EventHandler(tsm_Delete_Click);
			this.tsm_Clear.Name = "tsm_Clear";
			this.tsm_Clear.Size = new System.Drawing.Size(153, 24);
			this.tsm_Clear.Text = "清空";
			this.tsm_Clear.Click += new System.EventHandler(tsm_Clear_Click);
			this.tsm_ImportFromFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[3] { this.tsm_ImprotFromCSV, this.tsm_ImprotFromJson, this.tsm_ImprotFromXML });
			this.tsm_ImportFromFile.Name = "tsm_ImportFromFile";
			this.tsm_ImportFromFile.Size = new System.Drawing.Size(153, 24);
			this.tsm_ImportFromFile.Text = "从文件导入";
			this.tsm_ImprotFromCSV.Name = "tsm_ImprotFromCSV";
			this.tsm_ImprotFromCSV.Size = new System.Drawing.Size(200, 26);
			this.tsm_ImprotFromCSV.Text = "从CSV导入";
			this.tsm_ImprotFromCSV.Click += new System.EventHandler(tsm_ImprotFromCSV_Click);
			this.tsm_ImprotFromJson.Name = "tsm_ImprotFromJson";
			this.tsm_ImprotFromJson.Size = new System.Drawing.Size(200, 26);
			this.tsm_ImprotFromJson.Text = "从Json文件导入";
			this.tsm_ImprotFromJson.Click += new System.EventHandler(tsm_ImprotFromJson_Click);
			this.tsm_ImprotFromXML.Name = "tsm_ImprotFromXML";
			this.tsm_ImprotFromXML.Size = new System.Drawing.Size(200, 26);
			this.tsm_ImprotFromXML.Text = "从XML文件导入";
			this.tsm_ImprotFromXML.Click += new System.EventHandler(tsm_ImprotFromXML_Click);
			this.tsm_ExportToFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[3] { this.tsm_ExportToCSV, this.tsm_ExportToJson, this.tsm_ExportToXml });
			this.tsm_ExportToFile.Name = "tsm_ExportToFile";
			this.tsm_ExportToFile.Size = new System.Drawing.Size(153, 24);
			this.tsm_ExportToFile.Text = "导出到文件";
			this.tsm_ExportToCSV.Name = "tsm_ExportToCSV";
			this.tsm_ExportToCSV.Size = new System.Drawing.Size(200, 26);
			this.tsm_ExportToCSV.Text = "导出到CSV文件";
			this.tsm_ExportToCSV.Click += new System.EventHandler(tsm_ExportToCSV_Click);
			this.tsm_ExportToJson.Name = "tsm_ExportToJson";
			this.tsm_ExportToJson.Size = new System.Drawing.Size(200, 26);
			this.tsm_ExportToJson.Text = "导出到Json文件";
			this.tsm_ExportToJson.Click += new System.EventHandler(tsm_ExportToJson_Click);
			this.tsm_ExportToXml.Name = "tsm_ExportToXml";
			this.tsm_ExportToXml.Size = new System.Drawing.Size(200, 26);
			this.tsm_ExportToXml.Text = "导出到XML文件";
			this.tsm_ExportToXml.Click += new System.EventHandler(tsm_ExportToXml_Click);
			this.tsm_MoveUp.Name = "tsm_MoveUp";
			this.tsm_MoveUp.Size = new System.Drawing.Size(153, 24);
			this.tsm_MoveUp.Text = "上移";
			this.tsm_MoveUp.Click += new System.EventHandler(tsm_MoveUp_Click);
			this.tsm_MoveDown.Name = "tsm_MoveDown";
			this.tsm_MoveDown.Size = new System.Drawing.Size(153, 24);
			this.tsm_MoveDown.Text = "下移";
			this.tsm_MoveDown.Click += new System.EventHandler(tsm_MoveDown_Click);
			base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 15f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.Controls.Add(this.myDgv);
			base.Margin = new System.Windows.Forms.Padding(4);
			base.Name = "VUDatagridView";
			base.Size = new System.Drawing.Size(685, 455);
			((System.ComponentModel.ISupportInitialize)this.myDgv).EndInit();
			this.cMS_RightKeyMenu.ResumeLayout(false);
			base.ResumeLayout(false);
		}
	}
}
