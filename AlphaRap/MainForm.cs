using AlphaRap.Classes;
using AlphaRapLibrary;
using Cognex.VisionPro;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace AlphaRap
{
	public partial class MainForm : ModuleBaseForm
	{
		[System.Runtime.InteropServices.DllImport("User32.dll")]
		private static extern IntPtr WindowFromPoint(Point p);
		public Point pt;
		public Form forkeybord = MiddleLayer.HomeF;

		[System.Runtime.InteropServices.DllImport("user32.dll ")]
		public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int wndproc);
		[System.Runtime.InteropServices.DllImport("user32.dll ")]
		public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
		public const int GWL_STYLE = -16;
		public const int WS_DISABLED = 0x8000000;

		private MENU_PageType MENU_SelectPage = MENU_PageType.Home;
		private MENU_PageType1 MENU_SelectPage1 = MENU_PageType1.Home;
		private PictureBox[] MENU_Picture;
		public enum MENU_PageType
		{
			Home,
			Product,
			Save,
			Hard,
			Manual,
			Check,
			System,
			Robot,
			AddUser,
			Mes,
			Exit,
			Rapid,
			Log,
			Data,
			Vision,
			User,
			LifeSpan,
			Lock,
			Init,
			Start,
			Pause,
			Stop

		}
		public enum MENU_PageType1
		{
			Save,
			Login,
			Reset,
			Run,
			Pause,
			Stop,
			Home,
			Product,
			Hard,
			Manual,
			Check,
			System,
			AddUser,
			Mes,
			Rapid,
			Data,
			Vision,
			Exit,
			Robot,
			LifeSpan,
		}

		HomeForm hf = MiddleLayer.HomeF;
		ManualForm mf = MiddleLayer.ManualF;
		SystemForm sf = MiddleLayer.SystemF;
		HardForm hardf = MiddleLayer.HardF;
		LogForm mesf = MiddleLayer.LogF;
		CheckForm cf = MiddleLayer.CheckF;
		DataForm df = MiddleLayer.DataF;
		AddUserForm addf = MiddleLayer.AddF;
		LoadForm LoadFrm;


		int HourInputShift = 0;
		int HourOutputShift = 0;
		int HourRejectShift = 0;
		double HourYeild = 0;

		int AllInputShift = 0;
		int AllOutputShift = 0;
		int AllRejectShift = 0;
		double AllYeild = 0;
		private Thread startThread;
		public MainForm()
		{
			InitializeComponent();
			MENU_Picture = new PictureBox[] { MENU_Home, MENU_Product, MENU_Save, MENU_Hard, MENU_Manual, MENU_Check, MENU_System, MENU_Robot, MENU_AddUser, MENU_Mes, MENU_Exit, MENU_Rapid, MENU_Log, MENU_Data, MENU_Vision, MENU_Login, MENU_LifeSpan, MENU_Lock, MENU_Reset, MENU_Run, MENU_Pause, MENU_Stop };
			startThread = new Thread
			(() =>
			{
				LoadFrm = new LoadForm();
				LoadFrm.ShowDialog();
			}
			);
			startThread.Start();
		}
		public MouseHook mh;
		public KeyboardHook k_hook;

		private void MainForm_Load(object sender, EventArgs e)
		{


			SysPara.UserName = MiddleLayer.AddF.ReadAllUserData();
			SysPara.UserPermission = PermissionType.Operator;
			SwitchPermission(SysPara.UserPermission);
			RefreshMenuBackcolor();
			LoginOutTime.Enabled = false;


			SwitchMainPage(MENU_PageType.Manual);
			SwitchMainPage(MENU_PageType.Rapid);
			SwitchMainPage(MENU_PageType.Home);

			MiddleLayer.OpenRecipe(SysPara.FilePath);

			//鼠标监听
			//mh = new MouseHook();
			//mh.SetHook();
			//mh.MouseDownEvent += mh_MouseDownEvent;
			//mh.MouseUpEvent += mh_MouseUpEvent;
			//mh.MouseMoveEvent += mh_MouseMoveEvent;

			//键盘监听
			k_hook = new KeyboardHook();
			k_hook.KeyDownEvent += new KeyEventHandler(hook_KeyDown);//钩住键按下
			k_hook.Start();//安装键盘钩子

			MiddleLayer.LoadProcessRate = 100;
			MiddleLayer.alarmRunTask.AlarmTaskIsRun = true;
			GetProductDataINI();

		}

		#region show Form      
		public void SwitchMainPage(MENU_PageType PageType)
		{
			MENU_SelectPage = PageType;
			//WZF 修改
			Panel ShowPanl = new Panel();
			ShowPanl = panel2;
			tableLayoutPanel.Visible = false;

			RefreshMenuBackcolor();
			switch (MENU_SelectPage)
			{
				case MENU_PageType.Home:
					forkeybord = MiddleLayer.HomeF;
					ShowPanl = plMainShow;
					ShowhMainPage(MiddleLayer.HomeF, ShowPanl);
					tableLayoutPanel.Parent = panel2;
					tableLayoutPanel.Visible = true;
					break;
				case MENU_PageType.Vision:
					forkeybord = MiddleLayer.VPF;
					ShowhMainPage(MiddleLayer.VPF, ShowPanl);
					break;
				case MENU_PageType.Product:
					forkeybord = MiddleLayer.ProductF;
					ShowhMainPage(MiddleLayer.ProductF, ShowPanl);
					break;
				case MENU_PageType.Hard:
					MiddleLayer.PauseRun();
					forkeybord = MiddleLayer.HardF;
					ShowhMainPage(MiddleLayer.HardF, ShowPanl);
					break;
				case MENU_PageType.Manual:
					MiddleLayer.PauseRun();
					forkeybord = MiddleLayer.ManualF;
					ShowhMainPage(MiddleLayer.ManualF, ShowPanl);
					break;
				case MENU_PageType.Check:
					forkeybord = MiddleLayer.CheckF;
					ShowhMainPage(MiddleLayer.CheckF, ShowPanl);
					break;
				case MENU_PageType.System:
					forkeybord = MiddleLayer.SystemF;
					ShowhMainPage(MiddleLayer.SystemF, ShowPanl);
					break;
				case MENU_PageType.AddUser:
					forkeybord = MiddleLayer.AddF;
					ShowhMainPage(MiddleLayer.AddF, ShowPanl);
					break;
				case MENU_PageType.Log:
					forkeybord = MiddleLayer.LogF;
					ShowhMainPage(MiddleLayer.LogF, ShowPanl);
					break;
				case MENU_PageType.Data:
					forkeybord = MiddleLayer.DataF;
					ShowhMainPage(MiddleLayer.DataF, ShowPanl);
					break;
				case MENU_PageType.Robot:
					MiddleLayer.StopRun();
					forkeybord = MiddleLayer.RobotF;
					ShowhMainPage(MiddleLayer.RobotF, ShowPanl);
					break;
				case MENU_PageType.Rapid:
					forkeybord = MiddleLayer.FlowF;
					ShowhMainPage(MiddleLayer.FlowF, ShowPanl);
					break;
				case MENU_PageType.LifeSpan:
					forkeybord = MiddleLayer.SpanLifeF;
					ShowhMainPage(MiddleLayer.SpanLifeF, ShowPanl);
					break;
				case MENU_PageType.Mes:
					forkeybord = MiddleLayer.MesF;
					ShowhMainPage(MiddleLayer.MesF, ShowPanl);
					break;
				case MENU_PageType.Exit:
					Close();
					break;
			}
		}
		/// <summary>
		/// 更新菜单栏按钮的颜色
		/// </summary>
		public void RefreshMenuBackcolor()
		{
			for (int i = 0; i < MENU_Picture.Length; i++)
				if (i == (int)MENU_SelectPage)
					MENU_Picture[i].BackColor = Color.LimeGreen;
				else
				{
					MENU_Picture[i].BackColor = Color.White;

				}
		}

		/// <summary>
		/// 显示当前点击窗体
		/// </summary>
		/// <param name="ShowPage"></param>
		public void ShowhMainPage(dynamic ShowPage, Panel ShowPanl)
		{


			ShowPanl.Focus();
			foreach (Control Fcontrol in panel2.Controls)
			{
				Fcontrol.Parent = null;
				Fcontrol.Visible = false;
			}

			if (ShowPage.GetType().IsSubclassOf(typeof(Form)))
			{
				ShowPage.TopLevel = false;
				ShowPage.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
				ShowPage.WindowState = FormWindowState.Maximized;
				ShowPage.Dock = DockStyle.Fill;
			}
			else
				ShowPage.Dock = DockStyle.Fill;

			ShowPage.Parent = ShowPanl;
			ShowPage.Show();
		}

		private void MENU_Click(object sender, EventArgs e)
		{

			string ItemName = Convert.ToString(((Control)sender).Tag);
			MENU_PageType MENU_Page_Type = (MENU_PageType)Enum.Parse(typeof(MENU_PageType), ItemName);
			SwitchMainPage(MENU_Page_Type);
		}
		//UserLoginForm UserLoginF = new UserLoginForm();
		//用户登录按钮
		private void pictureBox13_Click(object sender, EventArgs e)
		{
			UserLoginForm UserLoginF = new UserLoginForm();
			forkeybord = UserLoginF;

			string OrgUser = SysPara.UserName;
			UserLoginF.ShowDialog();
			if (OrgUser != SysPara.UserName)
			{
				try
				{
					if (!MENU_Picture[(int)MENU_SelectPage].Enabled)
						SwitchMainPage(MENU_PageType.Home);
					else
						RefreshMenuBackcolor();
				}
				catch (Exception)
				{
					SwitchMainPage(MENU_PageType.Home);
				}
			}
		}
		/// <summary>
		/// 选择用户权限
		/// </summary>
		/// <param name="Permission"></param>
		public void SwitchPermission(PermissionType Permission)
		{
			string strSQL = "select * from PermissionSetup where Permission ='" + Permission.ToString() + "'";
			bool Successful = false;

			DataTable readData = DataBase.ReadData_Adapter(SysPara.MdbPath, strSQL, ref Successful);
			if (Successful)
			{
				if (readData.Rows.Count > 0)
				{
					MENU_Product.Enabled = Convert.ToBoolean(readData.Rows[0]["Product"]);
					MENU_Hard.Enabled = Convert.ToBoolean(readData.Rows[0]["Hard"]);
					MENU_Manual.Enabled = Convert.ToBoolean(readData.Rows[0]["Manual"]);
					MENU_Check.Enabled = Convert.ToBoolean(readData.Rows[0]["Check"]);
					MENU_System.Enabled = Convert.ToBoolean(readData.Rows[0]["System"]);
					MENU_AddUser.Enabled = (Permission == PermissionType.Administrator);
					MENU_Mes.Enabled = Convert.ToBoolean(readData.Rows[0]["Mes"]);
					MENU_Rapid.Enabled = Convert.ToBoolean(readData.Rows[0]["Rapid"]);
					MENU_Data.Enabled = Convert.ToBoolean(readData.Rows[0]["Data"]);
					MENU_Vision.Enabled = Convert.ToBoolean(readData.Rows[0]["Vision"]);
					MENU_Exit.Enabled = Convert.ToBoolean(readData.Rows[0]["Exit"]);
					MENU_Robot.Enabled = Convert.ToBoolean(readData.Rows[0]["Power"]);
				}
			}
		}
		#endregion

		#region Machine Status 
		private static Dictionary<RunMode, string> GetStatusTextMap(LanguageType language)
		{
			switch (language)
			{
				case LanguageType.Chinese:
					return new Dictionary<RunMode, string>
			{
				{ RunMode.IDLE, "待机状态" },
				{ RunMode.INITIAL, SysPara.UpConveyorInitialOk ? "初始化完成" : "正在初始化状态" },
				{ RunMode.RUN, "运行状态" },
				{ RunMode.PAUSE, "暂停状态" }
			};
				case LanguageType.English:
					return new Dictionary<RunMode, string>
			{
				{ RunMode.IDLE, "IDLE" },
				{ RunMode.INITIAL, SysPara.UpConveyorInitialOk ? "INITIAL Completed" : "INITIAL" },
				{ RunMode.RUN, "RUNNING" },
				{ RunMode.PAUSE, "PAUSE" }
			};
				case LanguageType.Español:
					return new Dictionary<RunMode, string>
			{
				{ RunMode.IDLE, "modo  espera" },
				{ RunMode.INITIAL, SysPara.UpConveyorInitialOk ? "cargando terminado" : "Estado  inicialización" },
				{ RunMode.RUN, "CORRER" },
				{ RunMode.PAUSE, "Estado suspendido" }
                // Note: Additional translations needed for RUN and PAUSE in Español  
            };
				default:
					throw new ArgumentException("Unsupported language type.");
			}
		}
		private void UpdateMachineStatus()
		{
			var statusTextMap = GetStatusTextMap(SysPara.LanguageShow);
			if (statusTextMap.TryGetValue(SysPara.SystemMode, out string statusText))
			{
				MachineStatus.BackColor = SysPara.SystemMode == RunMode.RUN ? Color.LimeGreen : Color.Yellow;
				if (SysPara.SystemMode == RunMode.PAUSE)
				{
					MachineStatus.BackColor = Color.Red;
				}
				MachineStatus.Text = statusText;
			}
			else
			{
				// Handle unsupported system mode if needed  
				MachineStatus.BackColor = Color.Black;
				MachineStatus.Text = "Unknown status";
			}
		}
		#endregion

		private void timer1_Tick(object sender, EventArgs e)
		{

			#region Machine Status 
			//F2024/03/10修改
			UpdateMachineStatus();
			#endregion

			#region ProductData
			//AddProductData();
			#endregion

			#region Language
			//add language selection
			string warnMessageCh1 = "用户名:  ";
			string warnMessageCh2 = "  权限:  ";
			string warnMessageCh3 = "  登录时间:  ";
			string warnMessageCh4 = "配方:  ";
			string warnMessageCh5 = "所有产品型号";

			string warnMessageEn1 = "UserName :  ";
			string warnMessageEn2 = "    UserPermission :  ";
			string warnMessageEn3 = "    LoginTime :  ";
			string warnMessageEn4 = "RecipeName :  ";
			string warnMessageEn5 = "All Product Models";


			string warnMessageESP1 = "Usuario :  ";
			string warnMessageESP2 = "    Permiso :  ";
			string warnMessageESP3 = "    LoginTime :  ";
			string warnMessageESP4 = "Fórmula :  ";
			string warnMessageESP5 = "Todos los modelos de productos";

			string message1 = "", message2 = "", message3 = "", message4 = "";
			switch (SysPara.LanguageShow)
			{
				case LanguageType.Chinese:
					message1 = warnMessageCh1;
					message2 = warnMessageCh2;
					message3 = warnMessageCh3;
					message4 = warnMessageCh4;
					MiddleLayer.ProductF.listView1.Columns[0].Text = warnMessageCh5;

					break;
				case LanguageType.English:
					message1 = warnMessageEn1;
					message2 = warnMessageEn2;
					message3 = warnMessageEn3;
					message4 = warnMessageEn4;
					MiddleLayer.ProductF.listView1.Columns[0].Text = warnMessageEn5;
					break;
				default:
					message1 = warnMessageESP1;
					message2 = warnMessageESP2;
					message3 = warnMessageESP3;
					message4 = warnMessageESP4;
					MiddleLayer.ProductF.listView1.Columns[0].Text = warnMessageESP5;
					break;

			}
			#endregion

			#region  RecipeName
			toolStripStatusLabel5.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			LoginText.Text = message1 + SysPara.UserName + message2 + SysPara.UserPermission + message3 + SysPara.UserLoginTime;
			lbRecipeName.Text = message4 + MiddleLayer.ProductF.CurrentModel.Text;
			#endregion

			#region  PictureBox
			MENU_Product.Image = MENU_Product.Enabled ? imageList1.Images[2] : imageList1.Images[3];
			MENU_Save.Image = MENU_Save.Enabled ? imageList1.Images[4] : imageList1.Images[5];
			MENU_Hard.Image = MENU_Product.Enabled ? imageList1.Images[6] : imageList1.Images[7];
			MENU_Manual.Image = MENU_Manual.Enabled ? imageList1.Images[8] : imageList1.Images[9];
			MENU_Check.Image = MENU_Check.Enabled ? imageList1.Images[10] : imageList1.Images[11];
			MENU_Robot.Image = MENU_Robot.Enabled ? imageList1.Images[12] : imageList1.Images[13];
			MENU_System.Image = MENU_System.Enabled ? imageList1.Images[14] : imageList1.Images[15];
			MENU_AddUser.Image = MENU_AddUser.Enabled ? imageList1.Images[16] : imageList1.Images[17];
			MENU_Mes.Image = MENU_Mes.Enabled ? imageList1.Images[18] : imageList1.Images[19];
			MENU_Rapid.Image = MENU_Rapid.Enabled ? imageList1.Images[20] : imageList1.Images[21];
			MENU_Data.Image = MENU_Data.Enabled ? imageList1.Images[22] : imageList1.Images[23];
			MENU_Vision.Image = MENU_Vision.Enabled ? imageList1.Images[24] : imageList1.Images[25];
			MENU_Exit.Image = MENU_Exit.Enabled ? imageList1.Images[26] : imageList1.Images[27];

			MENU_Run.Enabled = SysPara.UpConveyorInitialOk && (SysPara.SystemMode == RunMode.INITIAL || SysPara.SystemMode == RunMode.PAUSE);
			MENU_Run.Image = MENU_Run.Enabled ? imageList1.Images[30] : imageList1.Images[33];
			MENU_Pause.Enabled = SysPara.SystemMode == RunMode.RUN;
			MENU_Pause.Image = MENU_Pause.Enabled ? imageList1.Images[31] : imageList1.Images[34];
			MENU_Log.Image = MENU_Log.Enabled ? imageList1.Images[37] : imageList1.Images[38];

			if (SysPara.UserPermission == PermissionType.Administrator || SysPara.UserPermission == PermissionType.Maintenance)
			{
				MENU_LifeSpan.Enabled = true;
				MENU_LifeSpan.Image = imageList1.Images[36];
			}
			else
			{
				MENU_LifeSpan.Enabled = false;
				MENU_LifeSpan.Image = imageList1.Images[35];
			}
			MiddleLayer.MainF.MENU_Reset.Enabled = SysPara.SystemMode == RunMode.IDLE ? true : false;
			#endregion
		}

		#region Run Message

		public void AddErrorLog(string strMessage)
		{

			MiddleLayer.DataF.AddLogError(strMessage);

		}
		public void WriteRUNMessageText(string strMessage)
		{
			//SysPara.RunMessageTime = DateTime.Now.ToString("HH:mm:ss");
			SysPara.RunMessageTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
			if (textBox_RUNMessage == null)
				return;
			Action action = () =>
			{
				try
				{
					int iTotal = 0;
					int iLenght = textBox_RUNMessage.Lines.Length;
					textBox_RUNMessage.AppendText(SysPara.RunMessageTime + ": " + strMessage + "\r\n");
					if (textBox_RUNMessage.Lines.Length > 200)
					{
						for (int i = 0; i < 100; i++)
						{
							iTotal = iTotal + textBox_RUNMessage.Lines[i].Length + 2;
						}
						textBox_RUNMessage.Text = textBox_RUNMessage.Text.Substring(iTotal);
					}
				}
				catch
				{

				}
			};
			try
			{
				textBox_RUNMessage.Invoke(action);
			}
			catch
			{

			}
		}
		public void WriteErrorMessageText(string strMessage)
		{

			SysPara.RunMessageTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
			if (textBox_ERRORMessage == null)
				return;
			Action action = () =>
			{
				try
				{
					int iTotal = 0;
					int iLenght = textBox_ERRORMessage.Lines.Length;

					textBox_ERRORMessage.AppendText(SysPara.RunMessageTime + ": " + strMessage + "\r\n");
					if (textBox_ERRORMessage.Lines.Length > 200)
					{
						for (int i = 0; i < 100; i++)
						{
							iTotal = iTotal + textBox_ERRORMessage.Lines[i].Length + 2;
						}
						textBox_ERRORMessage.Text = textBox_ERRORMessage.Text.Substring(iTotal);
					}
				}
				catch
				{

				}
			};
			try
			{
				textBox_ERRORMessage.Invoke(action);
			}
			catch
			{

			}
		}
		public void WriteRunMessageResult(string RunTime, string strMessage)
		{
			ListViewItem lvi = new ListViewItem(RunTime);
			ListView listView1 = new ListView();
			lvi.SubItems.Add(strMessage);
			lvi.SubItems.Add(SysPara.UserName);
			listView1.Items.Add(lvi);
			string Year = DateTime.Now.Year.ToString();
			string month = DateTime.Now.Month.ToString();
			string day = DateTime.Now.Day.ToString();
			SysPara.RunMessagePath = MiddleLayer.LogF.GetSettingValue("Path", "RunPath") + "\\RunMessageData\\" + "\\" + Year + "\\" + month + "\\" + day + "\\";
			ListViewWrite.WriteExcelData(SysPara.RunMessagePath, listView1);
		}
		#endregion

		private readonly object ProductObjLock = new object();

		//private void AddProductData()
		//{
		//	lock (ProductObjLock)
		//	{
		//		DateTime datanow = DateTime.Now;

		//		txtCyCT.Text = SysPara.CircleTime + "/s";

		//		if ((SysPara.iProductOK + SysPara.iProductNG).ToString() != MiddleLayer.MainF.txtPTotal.Text)
		//		{
		//			MiddleLayer.SpanLifeF.TimeAdd();
		//			dataBControl1.AddProductQuantity(1);
		//			txtPTotal.Text = (SysPara.iProductOK + SysPara.iProductNG).ToString();

		//			SysPara.iProductHourlyInput[datanow.Hour] += 1;
		//		}

		//		if (txtPOK.Text != SysPara.iProductOK.ToString())
		//		{
		//			hoursProductShow1.kPointAdd(DateTime.Now, (int)(SysPara.iProductOK - Convert.ToInt32(txtPOK.Text)), true);
		//			hoursProductShow1.AllTimeDataShow(DateTime.Now);

		//			hoursProductShow1.GetAllShift(DateTime.Now, ref AllInputShift, ref AllOutputShift, ref AllRejectShift, ref AllYeild);
		//			if (AllOutputShift.ToString() == txtPOK.Text)
		//			{
		//				hoursProductShow1.kPointAdd(DateTime.Now, (int)(SysPara.iProductOK - Convert.ToInt32(txtPOK.Text)), true);
		//				hoursProductShow1.AllTimeDataShow(DateTime.Now);
		//			}
		//			SysPara.iProductHourlyOutput[datanow.Hour] += SysPara.iProductOK - Convert.ToInt32(txtPOK.Text);

		//			txtPOK.Text = SysPara.iProductOK.ToString();
		//		}

		//		if (txtPNG.Text != SysPara.iProductNG.ToString())
		//		{
		//			hoursProductShow1.kPointAdd(DateTime.Now, (int)(SysPara.iProductNG - Convert.ToInt32(txtPNG.Text)), false);
		//			hoursProductShow1.AllTimeDataShow(DateTime.Now);
		//			dataBControl1.AddProductQuantity(Ngnumber: 1);
		//			hoursProductShow1.GetAllShift(DateTime.Now, ref AllInputShift, ref AllOutputShift, ref AllRejectShift, ref AllYeild);
		//			if (AllRejectShift.ToString() == txtPNG.Text)
		//			{
		//				hoursProductShow1.kPointAdd(DateTime.Now, (int)(SysPara.iProductNG - Convert.ToInt32(txtPNG.Text)), false);
		//				hoursProductShow1.AllTimeDataShow(DateTime.Now);
		//			}

		//			SysPara.iProductHourlyReject[datanow.Hour] += SysPara.iProductNG - Convert.ToInt32(txtPNG.Text);
		//			txtPNG.Text = SysPara.iProductNG.ToString();
		//		}

		//		GetProductData();
		//	}
		//}
		public void iProductOKAdd()
		{
			lock (ProductObjLock)
			{
				SysPara.iProductOK++;
			}

		}
		public void iProductNGAdd()
		{
			lock (ProductObjLock)
			{
				SysPara.iProductNG++;
			}

		}

		#region GetProductData
		//private void GetProductData()
		//{

		//	DateTime datanow = DateTime.Now;


		//	hoursProductShow1.GetHourShift(DateTime.Now, ref HourInputShift, ref HourOutputShift, ref HourRejectShift, ref HourYeild);
		//	hoursProductShow1.GetAllShift(DateTime.Now, ref AllInputShift, ref AllOutputShift, ref AllRejectShift, ref AllYeild);

		//	txtPTotal.Text = AllInputShift.ToString();
		//	txtPOK.Text = AllOutputShift.ToString();
		//	txtPNG.Text = AllRejectShift.ToString();
		//	txtPRatio.Text = AllYeild.ToString("F2"); ;
		//	SysPara.iProductOK = AllOutputShift;
		//	SysPara.iProductNG = AllRejectShift;

		//}
		private void GetProductDataINI()
		{

			DateTime datanow = DateTime.Now;


			hoursProductShow1.GetHourShift(DateTime.Now, ref HourInputShift, ref HourOutputShift, ref HourRejectShift, ref HourYeild);
			hoursProductShow1.GetAllShift(DateTime.Now, ref AllInputShift, ref AllOutputShift, ref AllRejectShift, ref AllYeild);

			//txtPTotal.Text = AllInputShift.ToString();
			//txtPOK.Text = AllOutputShift.ToString();
			//txtPNG.Text = AllRejectShift.ToString();
			//txtPRatio.Text = AllYeild.ToString("F2"); ;
			SysPara.iProductOK = AllOutputShift;
			SysPara.iProductNG = AllRejectShift;

		}
		#endregion


		private void btExit_Click(object sender, EventArgs e)
		{
			//add language selection
			string warnMessageCh1 = "确定要退出调试吗？";
			string warnMessageCh2 = "提示";
			string warnMessageEn1 = "Are you sure to Exit?";
			string warnMessageEn2 = "Note";

			string message1 = "", message2 = "";
			switch (SysPara.LanguageShow)
			{
				case LanguageType.Chinese:
					message1 = warnMessageCh1;
					message2 = warnMessageCh2;
					break;
				case LanguageType.English:
					message1 = warnMessageEn1;
					message2 = warnMessageEn2;
					break;
			}
			DialogResult dr;
			dr = MessageBox.Show(message1, message2, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if
			 (dr == DialogResult.Yes)
			{
				aaaa:
				MiddleLayer.gEXIT = true;
				SwitchMainPage(MENU_PageType.Home);
				MiddleLayer.FlowCtrl.bStopWork = true;
				Close();
				System.Environment.Exit(0);
				GC.Collect(); //GC回收
				goto aaaa;
			}
		}

		#region reminder
		public void ShowWord(Control con, string word)
		{
			ToolTip p = new ToolTip();
			p.ShowAlways = true;
			p.SetToolTip(con, word);
		}
		private void MouseEnter1(object sender, EventArgs e)
		{
			string ItemName = Convert.ToString(((Control)sender).Tag);
			SwitchRemind((MENU_PageType1)Enum.Parse(typeof(MENU_PageType1), ItemName));
		}
		private void SwitchRemind(MENU_PageType1 PageType)
		{
			MENU_SelectPage1 = PageType;
			switch (MENU_SelectPage1)
			{
				case MENU_PageType1.Home:
					if (SysPara.LanguageShow == LanguageType.Chinese)
						ShowWord(this.MENU_Home, "主界面");
					else
						ShowWord(this.MENU_Home, "Home");

					break;
				case MENU_PageType1.Product:
					if (SysPara.LanguageShow == LanguageType.Chinese)
						ShowWord(this.MENU_Product, "物料管理");
					else
						ShowWord(this.MENU_Product, "ProductManager");
					break;
				case MENU_PageType1.Save:
					if (SysPara.LanguageShow == LanguageType.Chinese)
						ShowWord(this.MENU_Save, "保存");
					else
						ShowWord(this.MENU_Save, "Save");
					break;
				case MENU_PageType1.Hard:
					if (SysPara.LanguageShow == LanguageType.Chinese)
						ShowWord(this.MENU_Hard, "硬件调试");
					else
						ShowWord(this.MENU_Hard, "HardForm");
					break;
				case MENU_PageType1.Manual:
					if (SysPara.LanguageShow == LanguageType.Chinese)
						ShowWord(this.MENU_Manual, "手动界面");
					else
						ShowWord(this.MENU_Manual, "ManagerForm");
					break;
				case MENU_PageType1.Check:
					if (SysPara.LanguageShow == LanguageType.Chinese)
						ShowWord(this.MENU_Check, "信号监视");
					else
						ShowWord(this.MENU_Check, "CheckForm");
					break;
				case MENU_PageType1.System:
					if (SysPara.LanguageShow == LanguageType.Chinese)
						ShowWord(this.MENU_System, "系统设置");
					else
						ShowWord(this.MENU_System, "SystemForm");
					break;
				case MENU_PageType1.AddUser:
					if (SysPara.LanguageShow == LanguageType.Chinese)
						ShowWord(this.MENU_AddUser, "用户设置");
					else
						ShowWord(this.MENU_AddUser, "UserManager");
					break;
				case MENU_PageType1.Mes:
					if (SysPara.LanguageShow == LanguageType.Chinese)
						ShowWord(this.MENU_Mes, "数据上传");
					else
						ShowWord(this.MENU_Mes, "MES");
					break;
				case MENU_PageType1.Rapid:
					if (SysPara.LanguageShow == LanguageType.Chinese)
						ShowWord(this.MENU_Rapid, "快捷键");
					else
						ShowWord(this.MENU_Rapid, "RapidButton");
					break;
				case MENU_PageType1.Data:
					if (SysPara.LanguageShow == LanguageType.Chinese)
						ShowWord(this.MENU_Data, "产品数据");
					else
						ShowWord(this.MENU_Data, "ProductData");
					break;
				case MENU_PageType1.Vision:
					if (SysPara.LanguageShow == LanguageType.Chinese)
						ShowWord(this.MENU_Vision, "视觉界面");
					else
						ShowWord(this.MENU_Vision, "VisionForm");
					break;
				case MENU_PageType1.Login:
					if (SysPara.LanguageShow == LanguageType.Chinese)
						ShowWord(this.MENU_Login, "用户登录");
					else
						ShowWord(this.MENU_Login, "UserLogin");
					break;
				case MENU_PageType1.Reset:
					if (SysPara.LanguageShow == LanguageType.Chinese)
						ShowWord(this.MENU_Reset, "复位");
					else
						ShowWord(this.MENU_Reset, "ResetButton");
					break;
				case MENU_PageType1.Run:
					if (SysPara.LanguageShow == LanguageType.Chinese)
						ShowWord(this.MENU_Run, "运行");
					else
						ShowWord(this.MENU_Run, "RunButton");
					break;
				case MENU_PageType1.Pause:
					if (SysPara.LanguageShow == LanguageType.Chinese)
						ShowWord(this.MENU_Pause, "暂停");
					else
						ShowWord(this.MENU_Pause, "PauseButton");
					break;
				case MENU_PageType1.Stop:
					if (SysPara.LanguageShow == LanguageType.Chinese)
						ShowWord(this.MENU_Stop, "停止");
					else
						ShowWord(this.MENU_Stop, "StopButton");
					break;
				case MENU_PageType1.Exit:
					if (SysPara.LanguageShow == LanguageType.Chinese)
						ShowWord(this.MENU_Exit, "退出");
					else
						ShowWord(this.MENU_Exit, "Exit");
					break;
				case MENU_PageType1.Robot:
					if (SysPara.LanguageShow == LanguageType.Chinese)
						ShowWord(this.MENU_Robot, "机械手");
					else
						ShowWord(this.MENU_Robot, "Robot");
					break;
			}
		}
		#endregion

		private void pictureBox10_Click(object sender, EventArgs e)
		{
			string ItemName = Convert.ToString(((Control)sender).Tag);
			SwitchMainPage((MENU_PageType)Enum.Parse(typeof(MENU_PageType), ItemName));
		}
		/// <summary>
		/// 初始化按钮
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MENU_Reset_Click(object sender, EventArgs e)
		{
			MiddleLayer.Initial();
		}
		//获取H1表格点位行集合并初始化生成点位

		//获取H2表格点位行集合并初始化生成点位
		private void GetH2PosCount()
		{
			DataTable dtH2 = MiddleLayer.HardF.RecipeData.Tables["tb_H2_SolderPost"];
			int H2PosCount = dtH2.Rows.Count;

		}
		//保存数据按钮
		private void pictureBox3_Click(object sender, EventArgs e)
		{
			SaveData();
		}
		public void SaveData()
		{

			SysPara.items = 1;
			SysPara.items2 = 1;
			DialogResult dr;

			dr = MessageBox.Show((SysPara.LanguageShow == LanguageType.Chinese) ? "确认要保存吗？" : "Are you sure to save it？", (SysPara.LanguageShow == LanguageType.Chinese) ? "提示" : "Notes", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
			if (dr == DialogResult.Yes)
			{
				plMainShow.Focus();
				MiddleLayer.AddF.WritePermission();
				for (int i = 0; i < ModuleManager.ModuleList.Count; i++)
				{


					ModuleManager.ModuleList[i].WriteRecipeData(SysPara.FilePath);
					ModuleManager.ModuleList[i].WriteSettingData();
				}

			}
			else
			{
				for (int i = 0; i < ModuleManager.ModuleList.Count; i++)
				{
					ModuleManager.ModuleList[i].ReadRecipeData(SysPara.FilePath);
					ModuleManager.ModuleList[i].ReadSettingData();
				}
			}
			MiddleLayer.HardF.SaveHardData();

		}

		/// <summary>
		/// 菜单栏选择配方
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string OrgRecipeName = SysPara.RecipeName;
			OpenFileDialog OpenFileDir = new OpenFileDialog();
			OpenFileDir.Filter = "XML Files|*.xml";

			try
			{
				//SysPara.FilePath = System.IO.Directory.GetCurrentDirectory();
				OpenFileDir.InitialDirectory = SysPara.RecipeDataDirectory.Replace(".\\", System.IO.Directory.GetCurrentDirectory() + "\\");
			}
			catch (Exception)
			{
				SysPara.RecipeDataDirectory = string.Format("{0}\\ModuleData\\RecipeData\\Recipe.xml", System.IO.Directory.GetCurrentDirectory());
				string directory = Path.GetDirectoryName(SysPara.RecipeDataDirectory);
				System.IO.Directory.CreateDirectory(directory);
				OpenFileDir.InitialDirectory = directory;
			}

			if (OpenFileDir.ShowDialog() == DialogResult.OK)
				if (MiddleLayer.OpenRecipe(OpenFileDir.FileName))
				{
					string[] a = OpenFileDir.FileName.Split('\\');
					string[] b = a[a.Length - 1].Split('.');
					MiddleLayer.ProductF.CurrentModel.Text = b[0];
					//MiddleLayer.LogF.AddLog(LogType.Operation, string.Format("User change the recipe \"{0}\"->\"{1}\" . UserType:{2} UserName:{3}", OrgRecipeName, SysPara.RecipeName, SysPara.LoginLevel.ToString(), SysPara.LoginUserName));
				}
			MiddleLayer.OpenRecipe(SysPara.FilePath);
		}
		/// <summary>
		/// 菜单栏新建配方
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void NewToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveFileDialog SaveFileDir = new SaveFileDialog();
			SaveFileDir.Filter = "XML Files|*.xml";

			string Directory = SysPara.RecipeDataDirectory.Replace(".\\", System.IO.Directory.GetCurrentDirectory() + "\\");
			SaveFileDir.InitialDirectory = Directory;
			if (SaveFileDir.ShowDialog() == DialogResult.OK)
			{

				MiddleLayer.HardF.WriteRecipeData(SaveFileDir.FileName);

			}
		}


		private void btStart_Click(object sender, EventArgs e)
		{

			MiddleLayer.StartRun();
		}

		private void btStop_Click(object sender, EventArgs e)
		{
			MiddleLayer.StopRun();
		}

		private void btPause_Click(object sender, EventArgs e)
		{
			MiddleLayer.PauseRun();
		}
		/// <summary>
		/// 把用户登录数据保存到文件
		/// </summary>
		/// <param name="UserName"></param>
		/// <param name="UserPermission"></param>
		/// <param name="LoginTime"></param>
		public void AddUserResult(string UserName, string UserPermission, string LoginTime)
		{
			ListViewItem lvi = new ListViewItem(LoginTime);
			ListView listView1 = new ListView();
			lvi.SubItems.Add(UserName);
			lvi.SubItems.Add(UserPermission);
			listView1.Items.Add(lvi);
			string Year = DateTime.Now.Year.ToString();
			string month = DateTime.Now.Month.ToString();
			string day = DateTime.Now.Day.ToString();
			SysPara.UserMessagePath = MiddleLayer.LogF.GetSettingValue("Path", "UserPath") + "\\UserLoginData\\" + Year + "\\" + month + "\\" + day + "\\";
			ListViewWrite.WriteExcelData(SysPara.UserMessagePath, listView1);
		}

		//鼠标监听事件
		#region MouseMonitor
		// bool bMonitor = false;
		private void mh_MouseDownEvent(object sender, MouseEventArgs e)
		{
			if (SysPara.UserPermission != PermissionType.Operator)
			{
				LoginOutTime.Stop();
				LoginOutTime.Interval = MiddleLayer.PlatF.GetSettingValue("MSet", "Interval") * 1000;
				LoginOutTime.Start();
			}

			pt = Cursor.Position;
			pt = forkeybord.PointToClient(pt);

			if (forkeybord.Name != "HomeForm")
			{
				MiddleLayer.HomeF.ppt = pt;
			}
			else
			{
				MiddleLayer.HomeF.ppt = new Point(0, 0);
			}
		}

		Control GetControl(Control C)
		{
			Control Temp = C.GetChildAtPoint(C.PointToClient(Cursor.Position));
			if (Temp == null)
				return C;
			else
				return GetControl(Temp);
		}

		private void mh_MouseUpEvent(object sender, MouseEventArgs e)
		{
			if (SysPara.UserPermission != PermissionType.Operator)
			{
				LoginOutTime.Stop();
				LoginOutTime.Interval = MiddleLayer.PlatF.GetSettingValue("MSet", "Interval") * 1000;
				LoginOutTime.Start();
			}
		}
		private void mh_MouseMoveEvent(object sender, MouseEventArgs e)
		{
			if (SysPara.UserPermission != PermissionType.Operator)
			{
				LoginOutTime.Stop();
				LoginOutTime.Interval = MiddleLayer.PlatF.GetSettingValue("MSet", "Interval") * 1000;
				LoginOutTime.Start();
			}
		}
		#endregion
		#region KeyMonitor
		private void hook_KeyDown(object sender, KeyEventArgs e)
		{
			if (SysPara.UserPermission != PermissionType.Operator)
			{
				LoginOutTime.Stop();
				LoginOutTime.Interval = MiddleLayer.PlatF.GetSettingValue("MSet", "Interval") * 1000;
				LoginOutTime.Start();
			}
		}
		#endregion
		private void LoginOutTime_Tick(object sender, EventArgs e)
		{
			SysPara.UserName = "None";
			SysPara.UserPermission = PermissionType.Operator;
			SwitchPermission(SysPara.UserPermission);
			SwitchMainPage(MENU_PageType.Home);
			RefreshMenuBackcolor();
			LoginOutTime.Enabled = false;

			//if (SysPara.UserPermission != PermissionType.Operator)
			//{
			//	SysPara.UserName = MiddleLayer.AddF.ReadAllUserData();
			//	SysPara.UserPermission = PermissionType.Operator;
			//	SwitchPermission(SysPara.UserPermission);
			//	SwitchMainPage(MENU_PageType.Home);
			//	RefreshMenuBackcolor();
			//	LoginOutTime.Enabled = false;
			//}

		}
		/// <summary>
		/// 切换中文状态
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void NumC2_Click(object sender, EventArgs e)
		{

			SysPara.LanguageShow = LanguageType.Chinese;
			MiddleLayer.SwitchLanguage(SysPara.LanguageShow);
			SysPara.LanguageName = "Chinese";
			IniFile IniFile = new IniFile(".\\MachineSetup.ini");
			IniFile.WriteString("MachineSetup", "LanguageName", SysPara.LanguageName);

		}
		/// <summary>
		/// 切换英文状态
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void NumC3_Click(object sender, EventArgs e)
		{

			SysPara.LanguageShow = LanguageType.English;
			MiddleLayer.SwitchLanguage(SysPara.LanguageShow);

			SysPara.LanguageName = "English";
			IniFile IniFile = new IniFile(".\\MachineSetup.ini");
			IniFile.WriteString("MachineSetup", "LanguageName", SysPara.LanguageName);



		}

		private void MENU_Robot_Click(object sender, EventArgs e)
		{
			string ItemName = Convert.ToString(((Control)sender).Tag);
			SwitchMainPage((MENU_PageType)Enum.Parse(typeof(MENU_PageType), ItemName));
		}



		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			try
			{
				CogFrameGrabbers CCD_Graber = new Cognex.VisionPro.CogFrameGrabbers();
				for (int i = 0; i < CCD_Graber.Count; i++)
				{
					CCD_Graber[i].Disconnect(true);

				}
				MiddleLayer.alarmRunTask.AlarmTaskIsRun = false;

				System.Environment.Exit(0);
				GC.Collect(); //GC回收

			}
			catch
			{ }
		}

		private void MENU_Vision_Click(object sender, EventArgs e)
		{
			string ItemName = Convert.ToString(((Control)sender).Tag);
			SwitchMainPage((MENU_PageType)Enum.Parse(typeof(MENU_PageType), ItemName));
		}

		private void MENU_Manual_DoubleClick(object sender, EventArgs e)
		{

		}

		private void LOTO_Click(object sender, EventArgs e)
		{
			if (SysPara.SystemMode == RunMode.IDLE)
			{
				try
				{

					MiddleLayer.LockForm1.groupBox2.Visible = false;
					MiddleLayer.LockForm1.groupBox_Login.Visible = true;
					MiddleLayer.LockForm1.ShowDialog();

				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.ToString());
				}

			}
			else
			{
				MessageBox.Show("The device must be in the stop mode", "notice", MessageBoxButtons.OK);
			}
		}


		private void pictureBox1_Click(object sender, EventArgs e)
		{
			string ItemName = Convert.ToString(((Control)sender).Tag);
			MENU_PageType MENU_Page_Type = (MENU_PageType)Enum.Parse(typeof(MENU_PageType), ItemName);
			if (SysPara.SystemRun)
			{

			}
			SwitchMainPage(MENU_Page_Type);
		}
		int a = 1;
		//照明灯
		private void btLight_Click(object sender, EventArgs e)
		{
			if (a == 1)
			{
				this.btLight.BackColor = Color.Green;
				MiddleLayer.ManualF.OB_LEDLight.On();

				a++;
			}
			else
			{
				btLight.BackColor = Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));
				MiddleLayer.ManualF.OB_LEDLight.Off();

				a = 1;
			}

		}
		private void Buzzer_Click(object sender, EventArgs e)
		{
			Console.WriteLine(SysPara.bByPass);
			MiddleLayer.alTask.BuzzOff();
		}

		private void btAlarmReset_Click(object sender, EventArgs e)
		{
			MiddleLayer.AlarmClear();
			Thread.Sleep(100);
		}
		int btDoorIndex = 1;
		private void btDoor_Click(object sender, EventArgs e)
		{
			if (btDoorIndex == 1)
			{
				this.btDoor.BackColor = Color.Green;
				btDoorIndex++;
			}
			else
			{
				btDoor.BackColor = Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));
				btDoorIndex = 1;
			}

		}
		private void btClearCount_Click(object sender, EventArgs e)
		{

			DialogResult reult = MessageBox.Show(" Do you want to Clear  Count?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);

			if ((reult == DialogResult.Yes))
			{
				SysPara.iProductOK = 0;
				SysPara.iProductNG = 0;

			}
		}
		int b = 0;
		//直通
		private void btByPass_Click(object sender, EventArgs e)
		{

			if (b == 1)
			{
				btByPass.BackColor = Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));

				SysPara.bByPass = false;
				b = 0;
			}
			else
			{

				DialogResult reult = MessageBox.Show("是否直通模式?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);

				if ((reult == DialogResult.Yes))
				{
					SysPara.bByPass = true;
					b = 1;
					btByPass.BackColor = Color.Green;
				}
			}
		}

		private void plMainShow_Paint(object sender, PaintEventArgs e)
		{

		}

		private void españolToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SysPara.LanguageShow = LanguageType.Español;
			MiddleLayer.SwitchLanguage(SysPara.LanguageShow);
			SysPara.LanguageName = "Español";
			IniFile IniFile = new IniFile(".\\MachineSetup.ini");
			IniFile.WriteString("MachineSetup", "LanguageName", SysPara.LanguageName);
		}


	}
}
