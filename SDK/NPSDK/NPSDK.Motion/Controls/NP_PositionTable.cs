using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace NPSDK.Motion
{
	/// <summary>
	/// 配方点位表，提供点位编辑、删除、重排和定位操作。
	/// </summary>
	public partial class NP_PositionTable : UserControl
	{
		public delegate void GotoHandle();

		private string oldSettingValue = "0";
		private readonly List<PositionColumn> items = new List<PositionColumn>();
		private DataSet gDataSet;
		private DataTable gDataSet1;
		private bool runGoto;
		private readonly Timer tmrGoto;

		private TableLayoutPanel tableLayoutPanel1;
		private TableLayoutPanel tableLayoutPanel2;
		private Button btn_AddPost;
		private Button btn_DeletePost;
		private ContextMenuStrip pointMenu;
		private ToolStripMenuItem pointDelete;
		private DataRow contextRow;
		private Func<string, string, string, string, string> translate;
		[Browsable(false)] public Func<bool> CanEdit { get; set; }
		private Button btn_ReplacePost;
		private Button btn_UP;
		private Button btn_GotoPost;
		public DataGridView Position;

		[Browsable(true)]
		[Category("#NP")]
		[Description("单轴快捷绑定；ParameterSet 为空时 Add/Replace/GoTo 使用该轴")]
		public NP_Motor WorkMotor { get; set; }

		[TypeConverter(typeof(CollectionConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("#NP")]
		[Description("各列绑定的轴与软限位（同 DMC ParameterSet）")]
		public List<PositionColumn> ParameterSet
		{
			get { return items; }
			set
			{
				items.Clear();
				if (value != null)
				{
					items.AddRange(value);
				}
			}
		}

		[Category("#NP")]
		[Description("点位 DataSet")]
		public DataSet DataSet
		{
			get { return gDataSet; }
			set
			{
				gDataSet = value;
				if (IsDesignHost && Position != null)
				{
					Position.DataSource = value;
				}
			}
		}

		[Category("#NP")]
		[Description("点位 DataTable")]
		public DataTable DataSet1
		{
			get { return gDataSet1; }
			set
			{
				gDataSet1 = value;
				if (IsDesignHost && Position != null && value != null && gDataSet != null
					&& gDataSet.Tables.IndexOf(gDataSet1) != -1)
				{
					Position.DataMember = value.TableName;
				}
			}
		}

		public event GotoHandle Goto;

		public event GotoHandle ValueChange;

		/// <summary>刷新点位操作文案，表名、列键和编辑中的值保持原样。</summary>
		public void ApplyLanguage(Func<string, string, string, string, string> text)
		{
			translate = text;
			btn_AddPost.Text = TextFor("pointAdd", "新增", "Add", "Añadir");
			btn_DeletePost.Text = TextFor("pointDeleteButton", "删除", "Delete", "Eliminar");
			pointDelete.Text = TextFor("pointDelete", "删除点位", "Delete point", "Eliminar punto");
			btn_ReplacePost.Text = TextFor("pointReplace", "替换", "Replace", "Reemplazar");
			btn_UP.Text = TextFor("pointUp", "上移", "Up", "Subir");
			btn_GotoPost.Text = TextFor("pointGoto", "前往", "Go to", "Ir a");
			ApplyColumnHeaders();
		}

		/// <summary>只翻译标准点位字段标题，保留列键、自定义轴名和编辑中的值。</summary>
		private void ApplyColumnHeaders()
		{
			foreach (DataGridViewColumn column in Position.Columns)
			{
				if (column.DataPropertyName == "Annotation") column.HeaderText = TextFor("pointAnnotation", "说明", "Annotation", "Anotación");
				if (column.DataPropertyName == "Position" || column.DataPropertyName == "Pos") column.HeaderText = TextFor("pointPosition", "位置", "Position", "Posición");
			}
		}

		/// <summary>通过宿主语言桥取文案，独立使用时返回英文底稿。</summary>
		private string TextFor(string key, string zh, string en, string es)
		{
			return translate == null ? en : translate(key, zh, en, es);
		}

		/// <summary>检查宿主编辑权限及所绑定轴是否被流程占用。</summary>
		private bool EditingAllowed()
		{
			if (IsDesignHost || IsDisposed || (CanEdit != null && !CanEdit())) return false;
			foreach (NP_Motor axis in EnumerateAxes()) if (!FlowResourceGate.CanAccess(axis)) return false;
			return true;
		}

		/// <summary>右键选中实际命中的数据行，表头及空白处清空菜单目标。</summary>
		private void SelectContextRow(object sender, MouseEventArgs args)
		{
			if (args.Button != MouseButtons.Right) return;
			contextRow = null;
			int index = Position.HitTest(args.X, args.Y).RowIndex;
			if (index < 0) return;
			DataRowView view = Position.Rows[index].DataBoundItem as DataRowView;
			if (view == null) return;
			Position.CurrentCell = Position.Rows[index].Cells[0];
			Position.ClearSelection(); Position.Rows[index].Selected = true;
			contextRow = view.Row;
		}

		/// <summary>只有有效行才显示删除入口，运行中或轴占用时禁用。</summary>
		private void OpenPointMenu(object sender, CancelEventArgs args)
		{
			if (contextRow == null || contextRow.RowState == DataRowState.Detached || contextRow.RowState == DataRowState.Deleted) { args.Cancel = true; return; }
			pointDelete.Enabled = EditingAllowed();
		}

		/// <summary>拦截运行中直接编辑点位的入口。</summary>
		private void BeginEditPoint(object sender, DataGridViewCellCancelEventArgs args)
		{
			args.Cancel = !EditingAllowed();
		}

		private static bool IsDesignHost
		{
			get { return LicenseManager.UsageMode == LicenseUsageMode.Designtime; }
		}

		public NP_PositionTable()
		{
			InitializeComponent();
			tmrGoto = new Timer { Interval = 100 };
			tmrGoto.Tick += TmrGoto_Tick;
			Load += NP_PositionTable_Load;
		}

		private void NP_PositionTable_Load(object sender, EventArgs e)
		{
			if (IsDesignHost || (Site != null && Site.DesignMode))
			{
				try
				{
					if (Position != null && gDataSet != null && gDataSet1 != null
						&& gDataSet.Tables.IndexOf(gDataSet1) != -1)
					{
						Position.DataMember = gDataSet1.TableName;
					}
				}
				catch
				{
				}
				return;
			}

			EnsureDefaultTable();
			if (gDataSet != null && gDataSet1 != null && Position != null)
			{
				Position.DataSource = gDataSet;
				Position.DataMember = gDataSet1.TableName;
			}
		}

		/// <summary>未绑定外部表时，自动建单列 Pos 表，便于 WorkMotor 直接使用。</summary>
		private void EnsureDefaultTable()
		{
			if (gDataSet1 != null)
			{
				return;
			}
			gDataSet = new DataSet("PositionData");
			gDataSet1 = new DataTable("Points");
			gDataSet1.Columns.Add("Pos", typeof(double));
			gDataSet.Tables.Add(gDataSet1);
		}

		/// <summary>为新绑定的点位列应用标题及填充布局。</summary>
		private void Position_ColumnAdded(object sender, DataGridViewColumnEventArgs e)
		{
			ApplyColumnHeaders();
			FitColumns();
		}

		/// <summary>绑定完成后刷新标准表头，不改动表格数据。</summary>
		private void Position_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
		{
			ApplyColumnHeaders();
			FitColumns();
		}

		/// <summary>让表格原生填充可见宽度，禁止排序以保持点位索引的明确顺序。</summary>
		private void FitColumns()
		{
			foreach (DataGridViewColumn column in Position.Columns)
			{
				column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
				column.SortMode = DataGridViewColumnSortMode.NotSortable;
			}
		}

		/// <summary>绘制与可见行顺序一致的点位编号。</summary>
		private void Position_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
		{
			if (Position.DataSource == null)
			{
				return;
			}
			Rectangle bounds = new Rectangle(
				e.RowBounds.Location.X,
				e.RowBounds.Location.Y,
				Position.RowHeadersWidth - 4,
				e.RowBounds.Height);
			TextRenderer.DrawText(
				e.Graphics,
				(e.RowIndex + 1).ToString(),
				Position.RowHeadersDefaultCellStyle.Font,
				bounds,
				Position.RowHeadersDefaultCellStyle.ForeColor,
				TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
		}

		/// <summary>把各绑定轴当前位置追加为一个点位行。</summary>
		private void btn_AddPost_Click(object sender, EventArgs e)
		{
			if (!EditingAllowed()) return;
			EnsureDefaultTable();
			DataTable table = gDataSet1;
			DataRow row = table.NewRow();
			int cols = Math.Min(Position.ColumnCount > 0 ? Position.ColumnCount : table.Columns.Count, table.Columns.Count);
			for (int i = 0; i < cols; i++)
			{
				NP_Motor axis = GetAxisForColumn(i);
				row[i] = axis != null ? axis.GetEncPos() : 0.0;
			}
			table.Rows.Add(row);
			RaiseValueChange();
		}

		/// <summary>工具栏删除当前选中的绑定行，复用确认、权限及草稿变更通知。</summary>
		private void DeleteSelectedPoint(object sender, EventArgs e)
		{
			if (!EditingAllowed() || Position.CurrentRow == null) return;
			DataRowView view = Position.CurrentRow.DataBoundItem as DataRowView;
			if (view == null) return;
			contextRow = view.Row;
			DeletePoint(sender, e);
		}

		/// <summary>确认后删除右键命中的数据行，不依赖旧选中行或显示行号。</summary>
		private void DeletePoint(object sender, EventArgs e)
		{
			DataRow row = contextRow;
			if (!EditingAllowed() || row == null || row.RowState == DataRowState.Detached || row.RowState == DataRowState.Deleted || row.Table != gDataSet1) return;
			DialogResult result = MessageBox.Show(
				string.Format(TextFor("pointRemove", "删除点位 P{0}？后续点位索引会变化，请检查流程绑定。", "Delete point P{0}? Later point indices will change; review flow bindings.", "¿Eliminar punto P{0}? Cambiarán los índices posteriores; revise los vínculos del flujo."), Position.CurrentRow.Index + 1),
				TextFor("pointWarning", "点位操作", "Point operation", "Operación de puntos"),
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Exclamation,
				MessageBoxDefaultButton.Button1);
			if (result != DialogResult.Yes)
			{
				return;
			}
			if (!EditingAllowed()) return;
			row.Delete();
			contextRow = null;
			RaiseValueChange();
		}

		/// <summary>用各轴当前位置替换所选点位，保留说明列。</summary>
		private void btn_ReplacePost_Click(object sender, EventArgs e)
		{
			if (!EditingAllowed()) return;
			if (Position.CurrentRow == null || gDataSet1 == null)
			{
				return;
			}
			DialogResult result = MessageBox.Show(
				string.Format(TextFor("pointReplaceConfirm", "用当前位置替换点位 P{0}？", "Replace point P{0} with the current position?", "¿Reemplazar punto P{0} por la posición actual?"), Position.CurrentRow.Index + 1),
				TextFor("pointWarning", "点位操作", "Point operation", "Operación de puntos"),
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Exclamation,
				MessageBoxDefaultButton.Button1);
			if (result != DialogResult.Yes)
			{
				return;
			}
			DataRow row = ((DataRowView)Position.CurrentRow.DataBoundItem).Row;
			int cols = Math.Min(row.Table.Columns.Count, Math.Max(items.Count, 1));
			for (int i = 0; i < cols; i++)
			{
				NP_Motor axis = GetAxisForColumn(i);
				if (axis != null)
				{
					row[i] = axis.GetEncPos();
				}
			}
			RaiseValueChange();
		}

		/// <summary>将所选点位与上一个点位交换，显式改变点位索引。</summary>
		private void btn_UP_Click(object sender, EventArgs e)
		{
			if (!EditingAllowed()) return;
			if (Position.CurrentRow == null || gDataSet1 == null)
			{
				return;
			}
			int index = Position.CurrentRow.Index;
			if (index <= 0)
			{
				return;
			}
			DataRow upper = ((DataRowView)Position.Rows[index - 1].DataBoundItem).Row;
			DataRow current = ((DataRowView)Position.CurrentRow.DataBoundItem).Row;
			object[] upperValues = upper.ItemArray;
			object[] currentValues = current.ItemArray;
			upper.ItemArray = currentValues;
			current.ItemArray = upperValues;
			Position.ClearSelection();
			Position.Rows[index - 1].Selected = true;
			Position.CurrentCell = Position.Rows[index - 1].Cells[0];
			RaiseValueChange();
		}

		/// <summary>请求宿主移动到所选点位，无订阅时执行内置移动。</summary>
		private void btn_GotoPost_Click(object sender, EventArgs e)
		{
			if (!EditingAllowed()) return;
			if (Goto != null)
			{
				Goto();
				return;
			}
			StartBuiltinGoto();
		}

		/// <summary>无外部 Goto 订阅时，按当前行对各轴发定位。</summary>
		public void StartBuiltinGoto()
		{
			if (!EditingAllowed()) return;
			if (runGoto || Position.CurrentRow == null || gDataSet1 == null)
			{
				return;
			}
			int rowIndex = Position.CurrentRow.Index;
			if (rowIndex < 0 || rowIndex >= gDataSet1.Rows.Count)
			{
				return;
			}
			DataRow row = ((DataRowView)Position.CurrentRow.DataBoundItem).Row;
			bool any = false;
			int cols = row.Table.Columns.Count;
			for (int i = 0; i < cols; i++)
			{
				NP_Motor axis = GetAxisForColumn(i);
				if (axis == null || axis.GetBusy())
				{
					continue;
				}
				double target = Convert.ToDouble(row[i]);
				axis.WorkSpeed = axis.ManualSpeed;
				axis.Goto(target);
				any = true;
			}
			if (any)
			{
				runGoto = true;
				tmrGoto.Enabled = true;
			}
		}

		public void Stop()
		{
			runGoto = false;
			tmrGoto.Enabled = false;
			foreach (NP_Motor axis in EnumerateAxes())
			{
				axis.Stop();
			}
		}

		private void TmrGoto_Tick(object sender, EventArgs e)
		{
			if (!runGoto)
			{
				tmrGoto.Enabled = false;
				return;
			}
			bool busy = false;
			foreach (NP_Motor axis in EnumerateAxes())
			{
				if (axis.GetBusy())
				{
					busy = true;
					break;
				}
			}
			if (!busy)
			{
				runGoto = false;
				tmrGoto.Enabled = false;
			}
		}

		private void Position_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (Position.DataSource == null || gDataSet1 == null)
			{
				return;
			}
			RaiseValueChange();
			DataGridView grid = (DataGridView)sender;
			if (grid.CurrentCell == null || grid.CurrentCell.Value == null)
			{
				return;
			}
			int col = grid.CurrentCell.ColumnIndex;
			Type type = gDataSet1.Columns[col].DataType;
			if (type != typeof(int) && type != typeof(double))
			{
				return;
			}
			if (grid.CurrentCell.Value.ToString() == "")
			{
				grid.CurrentCell.Value = 0;
			}
			PositionColumn column = col < items.Count ? items[col] : null;
			if (column == null)
			{
				return;
			}
			double value = Convert.ToDouble(grid.CurrentCell.Value);
			if (value < column.Limit_Low)
			{
				MessageBox.Show(
					grid.Columns[col].HeaderText + " value can't low " + column.Limit_Low,
					"Warning",
					MessageBoxButtons.OK,
					MessageBoxIcon.Exclamation,
					MessageBoxDefaultButton.Button1,
					MessageBoxOptions.DefaultDesktopOnly);
				grid.CurrentCell.Value = oldSettingValue;
			}
			else if (value > column.Limit_High)
			{
				MessageBox.Show(
					grid.Columns[col].HeaderText + " value can't exceeds " + column.Limit_High,
					"Warning",
					MessageBoxButtons.OK,
					MessageBoxIcon.Exclamation,
					MessageBoxDefaultButton.Button1,
					MessageBoxOptions.DefaultDesktopOnly);
				grid.CurrentCell.Value = oldSettingValue;
			}
		}

		private void Position_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
		{
			e.Control.KeyPress -= Teach_Point_PointShow_KeyPress;
			e.Control.KeyPress += Teach_Point_PointShow_KeyPress;
		}

		private void Teach_Point_PointShow_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (Position.DataSource == null || gDataSet1 == null || Position.CurrentCell == null)
			{
				return;
			}
			Type type = gDataSet1.Columns[Position.CurrentCell.ColumnIndex].DataType;
			if (type != typeof(int) && type != typeof(double))
			{
				return;
			}
			if ((e.KeyChar <= '/' || e.KeyChar >= ':') && e.KeyChar != '\b' && e.KeyChar != '.' && e.KeyChar != '-')
			{
				e.Handled = true;
			}
			DataGridViewTextBoxEditingControl edit = sender as DataGridViewTextBoxEditingControl;
			if (edit == null)
			{
				return;
			}
			if (e.KeyChar == '-' && (edit.SelectionStart != 0 || edit.Text.IndexOf('-') >= 0))
			{
				e.Handled = true;
			}
			if (e.KeyChar == '.' && (edit.SelectionStart == 0 || edit.Text.IndexOf('.') >= 0))
			{
				e.Handled = true;
			}
			if (Position.CurrentCell.Value != null && Position.CurrentCell.Value.ToString() == "")
			{
				Position.CurrentCell.Value = "0";
				e.Handled = true;
			}
		}

		private void Position_Click(object sender, EventArgs e)
		{
			DataGridView grid = (DataGridView)sender;
			if (grid.CurrentCell == null)
			{
				return;
			}
			if (grid.CurrentCell.Value == null || grid.CurrentCell.Value.ToString() == "")
			{
				oldSettingValue = "0";
				grid.CurrentCell.Value = 0;
			}
			else
			{
				oldSettingValue = grid.CurrentCell.Value.ToString();
			}
		}

		private NP_Motor GetAxisForColumn(int columnIndex)
		{
			if (columnIndex >= 0 && columnIndex < items.Count && items[columnIndex].Axis != null)
			{
				return items[columnIndex].Axis;
			}
			if (columnIndex == 0)
			{
				return WorkMotor;
			}
			return null;
		}

		private IEnumerable<NP_Motor> EnumerateAxes()
		{
			bool any = false;
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i].Axis != null)
				{
					any = true;
					yield return items[i].Axis;
				}
			}
			if (!any && WorkMotor != null)
			{
				yield return WorkMotor;
			}
		}

		private void RaiseValueChange()
		{
			if (ValueChange != null)
			{
				ValueChange();
			}
		}

		/// <summary>释放点位菜单和运动轮询定时器。</summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (pointMenu != null) pointMenu.Dispose();
				runGoto = false;
				if (tmrGoto != null)
				{
					tmrGoto.Enabled = false;
					tmrGoto.Dispose();
				}
			}
			base.Dispose(disposing);
		}

	}
}
