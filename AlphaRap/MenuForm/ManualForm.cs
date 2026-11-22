using AlphaRapLibrary;
using AlphaRap.Configuration;
using AlphaRap.FlowProgramming;
using AlphaRap.TaskProcess;
using System;
using NPSDK.Motion;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AlphaRap
{
	public partial class ManualForm : ManualPage, IConfigurationParticipant
	{
		public List<NP_Output> Conveyor_List = new List<NP_Output>();

        internal DownConveyorIoBindings DownConveyorBindings { get; private set; }

        string IConfigurationParticipant.ConfigurationId => "Station.DownConveyorIo";
        int IConfigurationParticipant.ConfigurationOrder => 45;

        /// <summary>校验下料工位的 IO 草稿引用。</summary>
        void IConfigurationParticipant.ValidateConfiguration() { DownConveyorBindings?.Validate(); }

        /// <summary>把工位绑定加入主保存事务。</summary>
        ConfigurationChange IConfigurationParticipant.PrepareConfigurationSave()
        {
            return DownConveyorBindings?.PrepareSave() ?? new ConfigurationChange(Array.Empty<ConfigurationFile>());
        }

        /// <summary>主保存选择放弃时还原 IO 绑定草稿。</summary>
        Action IConfigurationParticipant.PrepareConfigurationDiscard() { return () => DownConveyorBindings?.Discard(); }

        /// <summary>输入输出资源使用独立页面，复用统一硬件目录和实例。</summary>
        public override void BindResources(Configuration.HardwareResources resources)
        {
            ioResourcePanel.BindResources(resources);
            if (resources != null) DownConveyorBindings = new DownConveyorIoBindings(resources, this);
        }
		public ManualForm()
		{
			CheckForIllegalCrossThreadCalls = false;
			InitializeComponent();
            if (!UiTheme.IsDesignTime) ApplicationHost.RegisterLanguage(this, Name);
            UiTheme.ApplyDpi(this);
			Conveyor_List.Add(OB_UpConveyor_Work_ConveyorForward);
			Conveyor_List.Add(OB_UpConveyor_Work_ConveyorReverse);
			Conveyor_List.Add(OB_UpConveyor_LocalMachineReady_SMEMA);
			Conveyor_List.Add(OB_UpConveyor_LocalMachineAvailable_SMEMA);

			ApplyPageStyle();
			if (UiTheme.IsDesignTime) return;
			WireIoLog();          // 输出点 / 气缸手动动作写操作日志
            ApplyDownConveyorBindingLanguage();
            EventHandler languageHandler = delegate { RefreshPageTexts(); ApplyDownConveyorBindingLanguage(); };
			ApplicationHost.LanguageChanged += languageHandler;
			Disposed += delegate { ApplicationHost.LanguageChanged -= languageHandler; };
		}

        /// <summary>更新绑定入口，保留已选 IO 的业务名称。</summary>
        private void ApplyDownConveyorBindingLanguage()
        {
            btnDownConveyorIo_dyn.Text = DownConveyorIoForm.Title;
        }

        /// <summary>空闲时编辑下料绑定，确定后保留草稿供主界面保存。</summary>
        private void EditDownConveyorIo(object sender, EventArgs args)
        {
            if (DownConveyorBindings == null) return;
            try
            {
                FlowPrograms.RequireIdle();
                DownConveyorBindings.RequireLoaded();
                using (var dialog = new DownConveyorIoForm(DownConveyorBindings)) dialog.ShowDialog(this);
            }
            catch (InvalidOperationException error) { MessageBox.Show(this, error.Message, DownConveyorIoForm.Title, MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

		/// <summary>给整页的输出点 / 气缸挂"动作即记录"（输入点只监视，不记）。</summary>
		private void WireIoLog()
		{
			WireIoLog(this);
		}

		private void WireIoLog(Control parent)
		{
			foreach (Control c in parent.Controls)
			{
				NP_Output ob = c as NP_Output;
				if (ob != null)
				{
					NP_Output target = ob;
					bool[] before = new bool[1];
					target.MouseDown += delegate { before[0] = target.IsOn(); };
					target.Click += delegate
					{
						bool now = target.IsOn();
						if (now == before[0]) return;      // 状态没变（例如不在手动模式）就不记
						OperationLog.Write("ManualForm", "手动输出：" + IoName(target) + " → " + (now ? "开" : "关"));
					};
				}
				else if (c is NP_Cylinder)
				{
					Control target = c;
					target.Click += delegate
					{
						OperationLog.Write("ManualForm", "手动气缸动作：" + IoName(target));
					};
				}

				if (c.Controls.Count > 0) WireIoLog(c);
			}
		}

		/// <summary>IO 点显示名：优先语言包（键 = 控件名），其次控件文字，最后控件名。</summary>
		private static string IoName(Control c)
		{
			string s = ApplicationHost.Text("ManualForm", c.Name, null);
			if (string.IsNullOrEmpty(s)) s = c.Text;
			if (string.IsNullOrEmpty(s)) s = c.Name;
			return s;
		}
	}
}
