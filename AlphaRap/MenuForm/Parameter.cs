using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AlphaRapLibrary;

namespace AlphaRap
{
	/// <summary>
	/// 设备管理页：<b>一个 DeviceControl 面板 = 一台设备</b>。
	/// 点【添加设备】选一个 AbstractDevice 子类，页面就多出一个完整面板
	/// （参数表 / 打开关闭 / 发送接收 / 记录框）；面板右上角【移除】把这一台整个拿掉。
	///
	/// 参数保存在 {SettingDataDirectory}\DeviceControl.xml，**跟随主界面【保存】**：
	/// 选"是"写入（面板集合的增删一并落盘、已删面板的旧节点会被清掉）；
	/// 选"否"回滚到上次保存的样子（面板集合也一起回滚）。
	/// </summary>
	public partial class Parameter : ModuleBaseForm
	{
		/// <summary>本页当前显示的设备面板（顺序 = 界面顺序 = 写入 XML 的顺序）。</summary>
		private readonly List<DeviceControl> _panels = new List<DeviceControl>();

		/// <summary>Load 只处理一次（句柄重建会再次触发）。</summary>
		private bool _loaded;

		/// <summary>防止"改宽度 → 容器 Resize → 再改宽度"的回环。</summary>
		private bool _laying;

		/// <summary>
		/// 构造函数：必须调用 InitializeComponent()，否则 Parameter.Designer.cs 里定义的控件
		/// （panelHost / flowDevices 等）一个都不会被创建 —— 现象是整页空白。
		/// </summary>
		public Parameter()
		{
			InitializeComponent();

			this.Load += Parameter_Load;
			this.Disposed += Parameter_Disposed;

			// 主界面【保存】选"否"时，DeviceControl.ReloadAll() 会先发这个通知 —— 面板集合本身也要回滚
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

		#region 面板增删

		/// <summary>【添加设备】：选设备类 → 建一个单设备面板 → 绑定该类的一台设备。</summary>
		private void btnAddDevice_Click(object sender, EventArgs e)
		{
			Type t = DeviceControl.PickDeviceTypeDialog(this, this.Font, this.Name, null);
			if (t == null) return;

			DeviceControl panel = NewPanel(NextPanelName());      // 先建面板并挂进容器
			string err = panel.BindDevice(t.Name, null);          // 再绑定设备（这时 FindForm 才取得到窗体名做键）
			if (!string.IsNullOrEmpty(err))
			{
				RemovePanel(panel);
				MessageBox.Show(err, "添加设备", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		/// <summary>面板上的【移除】：确认后把整个面板拿掉（参数会在下次【保存】时从 XML 里消失）。</summary>
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

		/// <summary>建一个"单设备"面板并挂进容器；名字由调用方给（必须与 XML 里的键一致）。</summary>
		private DeviceControl NewPanel(string panelName)
		{
			DeviceControl panel = new DeviceControl();
			panel.SingleDevice = true;      // 必须在挂进容器之前设好：面板 Load 时按它决定要不要自载 XML
			panel.Name = panelName;
			panel.Height = 360;
			panel.Width = PanelWidth();
			panel.Margin = new Padding(6);
			panel.RemoveRequested += Panel_RemoveRequested;

			flowDevices.Controls.Add(panel);    // 这一步会触发面板的 Load
			_panels.Add(panel);

			LayoutPanels();
			return panel;
		}

		private void RemovePanel(DeviceControl panel)
		{
			if (panel == null || !_panels.Contains(panel)) return;

			try { panel.RemoveRequested -= Panel_RemoveRequested; }
			catch (Exception) { }

			_panels.Remove(panel);
			flowDevices.Controls.Remove(panel);
			panel.Dispose();

			LayoutPanels();
		}

		/// <summary>按 XML 重建面板集合（启动时、【保存】选"否"回滚时都走这里）。</summary>
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
			}
		}

		/// <summary>
		/// 给下一个面板起名：dc&lt;最大序号 + 1&gt;。
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

		#region 布局

		private void flowDevices_Resize(object sender, EventArgs e)
		{
			if (_laying) return;
			_laying = true;
			try { LayoutPanels(); }
			finally { _laying = false; }
		}

		/// <summary>让每个面板占满容器宽度（竖向单列），否则容器一缩就会出横向滚动条。</summary>
		private void LayoutPanels()
		{
			int w = PanelWidth();
			for (int i = 0; i < _panels.Count; i++) _panels[i].Width = w;
		}

		/// <summary>面板宽度 = 容器可视宽度 − 内边距 − 纵向滚动条余量。</summary>
		private int PanelWidth()
		{
			int w = flowDevices.ClientSize.Width - flowDevices.Padding.Horizontal - 24;
			return (w < 360) ? 360 : w;
		}

		#endregion
	}
}
