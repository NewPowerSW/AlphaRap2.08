using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AlphaRapLibrary;

namespace AlphaRap
{
	/// <summary>
	/// 设备管理页：**一个页签 = 一台设备**（TabControl，横向页签）。
	/// 点【添加设备】选一个 AbstractDevice 子类，就多出一个页签；页签里是完整面板
	/// （参数表 / 打开关闭 / 发送接收 / 记录框），面板右上角【移除】把这一台整个拿掉。
	///
	/// 参数保存在 {SettingDataDirectory}\DeviceControl.xml，**跟随主界面【保存】**：
	/// 选"是"写入（页签的增删一并落盘、已删页签的旧节点会被清掉）；
	/// 选"否"回滚到上次保存的样子（页签集合也一起回滚）。
	/// </summary>
	public partial class Parameter : ModuleBaseForm
	{
		/// <summary>本页当前显示的设备面板（顺序 = 页签顺序 = 写入 XML 的顺序）。</summary>
		private readonly List<DeviceControl> _panels = new List<DeviceControl>();

		/// <summary>Load 只处理一次（句柄重建会再次触发）。</summary>
		private bool _loaded;

		/// <summary>页签标题的兜底文字（设备名还没定下来时用）。</summary>
		private const string DefaultTabTitle = "设备";

		/// <summary>
		/// 构造函数：必须调用 InitializeComponent()，否则 Parameter.Designer.cs 里定义的控件
		/// （panelHost / tabDevices 等）一个都不会被创建 —— 现象是整页空白。
		/// </summary>
		public Parameter()
		{
			InitializeComponent();

			this.Load += Parameter_Load;
			this.Disposed += Parameter_Disposed;

			// 主界面【保存】选"否"时，DeviceControl.ReloadAll() 会先发这个通知 —— 页签集合本身也要回滚
			DeviceControl.ReloadPanelsRequested += Parameter_ReloadPanelsRequested;
		}

		#region 生命周期

		private void Parameter_Load(object sender, EventArgs e)
		{
			if (_loaded) return;
			_loaded = true;

			RebuildPanelsFromXml();      // 按 XML 决定"上次存了几台设备、各是什么类"
		}

		private void Parameter_Disposed(object sender, EventArgs e)
		{
			DeviceControl.UnregisterPanelHost(this.Name);
			DeviceControl.ReloadPanelsRequested -= Parameter_ReloadPanelsRequested;
		}

		private void Parameter_ReloadPanelsRequested(object sender, EventArgs e)
		{
			RebuildPanelsFromXml();
		}

		#endregion

		#region 页签增删

		/// <summary>【添加设备】：选设备类 → 建一个页签 + 单设备面板 → 绑定该类的一台设备。</summary>
		private void btnAddDevice_Click(object sender, EventArgs e)
		{
			Type t = DeviceControl.PickDeviceTypeDialog(this, this.Font, this.Name, null);
			if (t == null) return;

			DeviceControl panel = NewPanel(NextPanelName());      // 先建页签并挂进 TabControl
			string err = panel.BindDevice(t.Name, null);          // 再绑定设备（这时 FindForm 才取得到窗体名做键）
			if (!string.IsNullOrEmpty(err))
			{
				RemovePanel(panel);
				MessageBox.Show(err, "添加设备", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		/// <summary>面板上的【移除】：确认后把整个页签拿掉（参数会在下次【保存】时从 XML 里消失）。</summary>
		private void Panel_RemoveRequested(object sender, EventArgs e)
		{
			DeviceControl panel = sender as DeviceControl;
			if (panel == null) return;

			string what = string.IsNullOrEmpty(panel.DeviceName) ? "该设备" : panel.DeviceName;
			DialogResult r = MessageBox.Show(
				"确定移除设备 " + what + " 吗？" + Environment.NewLine
				+ "它的参数会在下次保存时从配置文件里移除。",
				"移除设备", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
			if (r != DialogResult.Yes) return;

			RemovePanel(panel);
		}

		/// <summary>设备改名后同步页签标题。</summary>
		private void Panel_DeviceNameChanged(object sender, EventArgs e)
		{
			UpdateTabTitle(sender as DeviceControl);
		}

		/// <summary>建一个页签 + 里面的单设备面板；名字由调用方给（必须与 XML 里的键一致）。</summary>
		private DeviceControl NewPanel(string panelName)
		{
			DeviceControl panel = new DeviceControl();
			panel.SingleDevice = true;      // 必须在挂进页签之前设好：面板 Load 时按它决定要不要自载 XML
			panel.Name = panelName;
			panel.Dock = DockStyle.Fill;
			panel.RemoveRequested += Panel_RemoveRequested;
			panel.DeviceNameChanged += Panel_DeviceNameChanged;

			TabPage page = new TabPage();
			page.Name = panelName;          // 与面板同名，方便用 tabDevices.TabPages[name] 反查
			page.Text = DefaultTabTitle;
			page.UseVisualStyleBackColor = true;
			page.Controls.Add(panel);       // 这一步会触发面板的 Load

			tabDevices.TabPages.Add(page);
			tabDevices.SelectedTab = page;
			_panels.Add(panel);

			UpdateTabTitle(panel);
			return panel;
		}

		private void RemovePanel(DeviceControl panel)
		{
			if (panel == null || !_panels.Contains(panel)) return;

			try
			{
				panel.RemoveRequested -= Panel_RemoveRequested;
				panel.DeviceNameChanged -= Panel_DeviceNameChanged;
			}
			catch (Exception) { }

			TabPage page = PageOf(panel);
			if (page != null)
			{
				tabDevices.TabPages.Remove(page);
				page.Dispose();             // 连带 Dispose 掉里面的 DeviceControl
			}
			else panel.Dispose();

			_panels.Remove(panel);
		}

		/// <summary>按 XML 重建页签集合（启动时、【保存】选"否"回滚时都走这里）。</summary>
		private void RebuildPanelsFromXml()
		{
			// 到这里说明本页真的显示过了（面板已物化）——此时才登记为"面板宿主"，
			// 让【保存】敢清掉本窗体名下的旧节点。
			// ⚠ 不能放在构造函数里：否则"开机后没进过本页就直接保存"会把旧参数误清空。
			DeviceControl.RegisterPanelHost(this.Name);

			for (int i = _panels.Count - 1; i >= 0; i--) RemovePanel(_panels[i]);

			List<string> names = DeviceControl.ReadFormPanelNames(this.Name);
			for (int i = 0; i < names.Count; i++)
			{
				DeviceControl panel = NewPanel(names[i]);
				panel.LoadFromXml();     // 按"窗体名_控件名"把自己那条读回来（顺带建出设备实例）
				UpdateTabTitle(panel);   // 载入后设备名才定下来，这里补一次标题
			}
		}

		/// <summary>
		/// 给下一个页签起名：dc&lt;最大序号 + 1&gt;。
		/// 序号**不复用** —— 名字同时是 XML 里的键，复用会跟旧节点串味。
		/// </summary>
		private string NextPanelName()
		{
			int max = 0;
			for (int i = 0; i < _panels.Count; i++)
			{
				string n = _panels[i].Name;
				if (string.IsNullOrEmpty(n) || !n.StartsWith("dc", StringComparison.OrdinalIgnoreCase)) continue;

				int v;
				if (int.TryParse(n.Substring(2), out v) && v > max) max = v;
			}
			return "dc" + (max + 1);
		}

		#endregion

		#region 页签标题

		/// <summary>按面板名反查它所在的页签（TabControl 的字符串索引器就是按 TabPage.Name 找）。</summary>
		private TabPage PageOf(DeviceControl panel)
		{
			if (panel == null || string.IsNullOrEmpty(panel.Name)) return null;
			return tabDevices.TabPages[panel.Name];
		}

		/// <summary>页签标题 = 设备名；悬停提示里补上设备类名。</summary>
		private void UpdateTabTitle(DeviceControl panel)
		{
			TabPage page = PageOf(panel);
			if (page == null) return;

			string title = panel.DeviceName;
			if (string.IsNullOrEmpty(title)) title = DefaultTabTitle;

			page.Text = title;
			page.ToolTipText = string.IsNullOrEmpty(panel.BoundTypeName)
				? title
				: title + "（" + panel.BoundTypeName + "）";
		}

		#endregion
	}
}
