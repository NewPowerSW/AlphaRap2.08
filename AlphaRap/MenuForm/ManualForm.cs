using AlphaRapLibrary;
using NPSDK.Motion;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AlphaRap
{
	public partial class ManualForm : ModuleBaseForm
	{
		public List<NP_Output> Conveyor_List = new List<NP_Output>();
		public ManualForm()
		{
			CheckForIllegalCrossThreadCalls = false;
			InitializeComponent();
            if (!UiTheme.IsDesignTime) MiddleLayer.RegisterAndApplyLanguage(this, Name);
            UiTheme.ApplyDpi(this);
			Conveyor_List.Add(OB_UpConveyor_Work_ConveyorForward);
			Conveyor_List.Add(OB_UpConveyor_Work_ConveyorReverse);
			Conveyor_List.Add(OB_UpConveyor_LocalMachineReady_SMEMA);
			Conveyor_List.Add(OB_UpConveyor_LocalMachineAvailable_SMEMA);

			ApplyPageStyle();
			if (UiTheme.IsDesignTime) return;
			WireIoLog();          // 输出点 / 气缸手动动作写操作日志
			System.EventHandler languageHandler = delegate { RefreshPageTexts(); };
			MiddleLayer.LanguageChanged += languageHandler;
			Disposed += delegate { MiddleLayer.LanguageChanged -= languageHandler; };
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
			string s = MiddleLayer.LangText("ManualForm", c.Name, null);
			if (string.IsNullOrEmpty(s)) s = c.Text;
			if (string.IsNullOrEmpty(s)) s = c.Name;
			return s;
		}
	}
}
