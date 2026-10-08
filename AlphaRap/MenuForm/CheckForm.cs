using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace AlphaRap
{
    public partial class CheckForm : Form
    {    
        private void CheckForm_Load(object sender, EventArgs e)
        {
            radioButton25.PerformClick();
        }
        private List<SignalTowerData> TowerData = new List<SignalTowerData>();
        private SignalTowerStatusType SelectStatus = SignalTowerStatusType.MachineIdle;

        private RadioButton[] rbGroup_GreenLight_RunMode;
        private RadioButton[] rbGroup_YellowLight_RunMode;
        private RadioButton[] rbGroup_RedLight_RunMode;
        private RadioButton[] rbGroup_Buzz_RunMode;
        private RadioButton[] rbGroup_GreenLight_MaintenanceMode;
        private RadioButton[] rbGroup_YellowLight_MaintenanceMode;
        private RadioButton[] rbGroup_RedLight_MaintenanceMode;
        private RadioButton[] rbGroup_Buzz_MaintenanceMode;

        public  SignalTowerStatusType PreviousStatus = SignalTowerStatusType.MachineIdle;
        public Status GreenLightStatus = 0;
        public Status YellowLightStatus = 0;
        public Status RedLightStatus = 0;
        public Status BuzzerStatus = 0;
        public bool bHaveChangeStatus = false;

        public enum SignalTowerStatusType
        {
            MachineIdle = 0,
            MachineInitialize,
            MachineRunning,
            MessageError,
            MessageInformation,
            MessageWarning,
        }

        public enum Status
        {
            Off = 0,
            On,
            Blink
        }

        public struct SignalTowerData
        {
            public SignalTowerStatusType SignalTowerStatus;
            public Status GreenLightStatus_R;
            public Status YellowLightStatus_R;
            public Status RedLightStatus_R;
            public Status BuzzStatus_R;
            public Status GreenLightStatus_M;
            public Status YellowLightStatus_M;
            public Status RedLightStatus_M;
            public Status BuzzerStatus_M;
        }

        public CheckForm()
        {
            InitializeComponent();

            rbGroup_GreenLight_RunMode = new RadioButton[] { rbGreenOff_R, rbGreenOn_R, rbGreenBlink_R };
            rbGroup_YellowLight_RunMode = new RadioButton[] { rbYellowOff_R, rbYellowOn_R, rbYellowBlink_R };
            rbGroup_RedLight_RunMode = new RadioButton[] { rbRedOff_R, rbRedOn_R, rbRedBlink_R };
            rbGroup_Buzz_RunMode = new RadioButton[] { rbBuzzOff_R, rbBuzzOn_R, rbBuzzBlink_R };
            rbGroup_GreenLight_MaintenanceMode = new RadioButton[] { rbGreenOff_M, rbGreenOn_M, rbGreenBlink_M };
            rbGroup_YellowLight_MaintenanceMode = new RadioButton[] { rbYellowOff_M, rbYellowOn_M, rbYellowBlink_M };
            rbGroup_RedLight_MaintenanceMode = new RadioButton[] { rbRedOff_M, rbRedOn_M, rbRedBlink_M };
            rbGroup_Buzz_MaintenanceMode = new RadioButton[] { rbBuzzOff_M, rbBuzzOn_M, rbBuzzBlink_M };

            // 左侧 6 个状态按钮裁成圆角，与应用其它按钮（芯片标签、底部工具栏）保持一致
            foreach (RadioButton rb in StateButtons)
                ApplyRoundedRegion(rb, 8);

            ReadAllSignalTowerData();
        }

        /// <summary>把控件裁剪为圆角矩形。尺寸固定，所以只需在构造时算一次。</summary>
        private static void ApplyRoundedRegion(Control c, int radius)
        {
            if (c == null || c.Width <= 0 || c.Height <= 0) return;
            try
            {
                int d = Math.Max(2, Math.Min(radius * 2, Math.Min(c.Width, c.Height)));
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddArc(0, 0, d, d, 180, 90);
                    path.AddArc(c.Width - d, 0, d, d, 270, 90);
                    path.AddArc(c.Width - d, c.Height - d, d, d, 0, 90);
                    path.AddArc(0, c.Height - d, d, d, 90, 90);
                    path.CloseFigure();
                    if (c.Region != null) c.Region.Dispose();
                    c.Region = new Region(path);
                }
            }
            catch { }
        }

        #region 跟随宿主尺寸 + 内容卡片居中

        /// <summary>内容卡片的宽度上限（超过就靠留白吸收，避免表格被拉得过宽）。</summary>
        private const int CardMaxWidth = 1520;

        /// <summary>内容卡片的高度上限（超过就靠留白吸收，避免 5 行表格被拉得过扁）。</summary>
        private const int CardMaxHeight = 680;

        /// <summary>卡片四周优先保留的留白（可用区够大时按这个值居中）。</summary>
        private const int CardOuterPadding = 144;

        /// <summary>
        /// 本窗体是运行时被 MainForm 动态挂到面板上的（TopLevel=false）。
        /// 实测：窗口在程序启动时是 1440×900，此时它按当时的宿主大小被"钉"住，
        /// 之后再把窗口最大化，窗体不会跟着放大 —— 右/下就会各留一大块空白。
        /// 这里显式贴合宿主客户区，保证任何窗口尺寸下都铺满。
        /// </summary>
        private void FitToHost()
        {
            try
            {
                if (TopLevel || Parent == null) return;

                // 先把 Dock 补回 Fill（防止被 WindowState 之类的设置覆盖掉）
                if (Dock != DockStyle.Fill) Dock = DockStyle.Fill;

                Size hostSize = Parent.ClientSize;
                if (Width != hostSize.Width || Height != hostSize.Height)
                    Size = new Size(hostSize.Width, hostSize.Height);

                LayoutContentCard(hostSize);
            }
            catch { }
        }

        /// <summary>
        /// 布局内容卡片。
        ///
        /// 背景：这一页原来是把内容按宿主尺寸"拉伸铺满"。窗口一最大化，两个矩阵表就被拉到
        /// 750×850 左右，5 行平均每行 170px —— 表格变形、四周又只剩十几像素的边距，
        /// 看起来既空又散。
        ///
        /// 做法：把卡片宽高限制在一个舒适区间内（约等于设计尺寸），居中的部分交给
        /// rootTable 两侧的百分比空列 / 上下空行去吸收，形成"内容居中 + 四周留白"的观感。
        /// 小窗口下则退化为"只留一圈小边距"，保证内容不被裁掉。
        /// </summary>
        private void LayoutContentCard(Size hostSize)
        {
            if (rootTable == null || rootTable.ColumnStyles.Count < 3 || rootTable.RowStyles.Count < 3) return;

            int cardW = Math.Min(hostSize.Width - CardOuterPadding, CardMaxWidth);
            int cardH = Math.Min(hostSize.Height - CardOuterPadding, CardMaxHeight);

            // 下限：窗口很小时也得有块能看的内容区
            cardW = Math.Max(360, cardW);
            cardH = Math.Max(280, cardH);

            // 上限：卡片绝不超出可用区（只剩 8px 边距的极端情况）
            cardW = Math.Min(cardW, Math.Max(160, hostSize.Width - 16));
            cardH = Math.Min(cardH, Math.Max(140, hostSize.Height - 16));

            if (Math.Abs(rootTable.ColumnStyles[1].Width - cardW) > 0.5f ||
                Math.Abs(rootTable.RowStyles[1].Height - cardH) > 0.5f)
            {
                rootTable.ColumnStyles[1].Width = cardW;
                rootTable.RowStyles[1].Height = cardH;
            }
        }

        private void Host_Resize(object sender, EventArgs e)
        {
            FitToHost();
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            if (Parent != null)
            {
                Parent.Resize -= Host_Resize;
                Parent.Resize += Host_Resize;
            }
            FitToHost();
        }

        #endregion

        #region 用户选择不同的信号灯      
        private void rbGroup_GreenLight_RunMode_Click(object sender, MouseEventArgs e)
        {
            ChangeSelectItems(rbGroup_GreenLight_RunMode, sender);
            RefreshSelectItemValue();
            WriteSignalTowerData(SelectStatus);
        }

        private void rbGroup_YellowLight_RunMode_Click(object sender, MouseEventArgs e)
        {
            ChangeSelectItems(rbGroup_YellowLight_RunMode, sender);
            RefreshSelectItemValue();
            WriteSignalTowerData(SelectStatus);
        }

        private void rbGroup_RedLight_RunMode_Click(object sender, MouseEventArgs e)
        {
            ChangeSelectItems(rbGroup_RedLight_RunMode, sender);
            RefreshSelectItemValue();
            WriteSignalTowerData(SelectStatus);
        }

        private void rbGroup_Buzzer_RunMode_Click(object sender, MouseEventArgs e)
        {
            ChangeSelectItems(rbGroup_Buzz_RunMode, sender);
            RefreshSelectItemValue();
            WriteSignalTowerData(SelectStatus);
        }

        private void rbGroup_GreenLight_MaintenanceMode_Click(object sender, MouseEventArgs e)
        {
            ChangeSelectItems(rbGroup_GreenLight_MaintenanceMode, sender);
            RefreshSelectItemValue();
            WriteSignalTowerData(SelectStatus);
        }

        private void rbGroup_YellowLight_MaintenanceMode_Click(object sender, MouseEventArgs e)
        {
            ChangeSelectItems(rbGroup_YellowLight_MaintenanceMode, sender);
            RefreshSelectItemValue();
            WriteSignalTowerData(SelectStatus);
        }

        private void rbGroup_RedLight_MaintenanceMode_Click(object sender, MouseEventArgs e)
        {
            ChangeSelectItems(rbGroup_RedLight_MaintenanceMode, sender);
            RefreshSelectItemValue();
            WriteSignalTowerData(SelectStatus);
        }

        private void rbGroup_Buzzer_MaintenanceMode_Click(object sender, MouseEventArgs e)
        {
            ChangeSelectItems(rbGroup_Buzz_MaintenanceMode, sender);
            RefreshSelectItemValue();
            WriteSignalTowerData(SelectStatus);
        }
        #endregion
        private void ChangeSelectItems(RadioButton[] Group, object SelectedItem)
        {
            RadioButton target = SelectedItem as RadioButton;
            foreach (RadioButton rb in Group)
                rb.Checked = false;
            target.Checked = true;
        }

        private void RefreshSelectItemValue()
        {
            try
            {
                SignalTowerData newSignalTowerData = new SignalTowerData();
                newSignalTowerData.SignalTowerStatus = SelectStatus;

                #region Get selected item value
                for (int i = 0; i < rbGroup_GreenLight_RunMode.Length; i++)
                    if (rbGroup_GreenLight_RunMode[i].Checked)
                    {
                        newSignalTowerData.GreenLightStatus_R = (Status)i;
                        break;
                    }
                for (int i = 0; i < rbGroup_YellowLight_RunMode.Length; i++)
                    if (rbGroup_YellowLight_RunMode[i].Checked)
                    {
                        newSignalTowerData.YellowLightStatus_R = (Status)i;
                        break;
                    }
                for (int i = 0; i < rbGroup_RedLight_RunMode.Length; i++)
                    if (rbGroup_RedLight_RunMode[i].Checked)
                    {
                        newSignalTowerData.RedLightStatus_R = (Status)i;
                        break;
                    }
                for (int i = 0; i < rbGroup_Buzz_RunMode.Length; i++)
                    if (rbGroup_Buzz_RunMode[i].Checked)
                    {
                        newSignalTowerData.BuzzStatus_R = (Status)i;
                        break;
                    }
                for (int i = 0; i < rbGroup_GreenLight_MaintenanceMode.Length; i++)
                    if (rbGroup_GreenLight_MaintenanceMode[i].Checked)
                    {
                        newSignalTowerData.GreenLightStatus_M = (Status)i;
                        break;
                    }
                for (int i = 0; i < rbGroup_YellowLight_MaintenanceMode.Length; i++)
                    if (rbGroup_YellowLight_MaintenanceMode[i].Checked)
                    {
                        newSignalTowerData.YellowLightStatus_M = (Status)i;
                        break;
                    }
                for (int i = 0; i < rbGroup_RedLight_MaintenanceMode.Length; i++)
                    if (rbGroup_RedLight_MaintenanceMode[i].Checked)
                    {
                        newSignalTowerData.RedLightStatus_M = (Status)i;
                        break;
                    }
                for (int i = 0; i < rbGroup_Buzz_MaintenanceMode.Length; i++)
                    if (rbGroup_Buzz_MaintenanceMode[i].Checked)
                    {
                        newSignalTowerData.BuzzerStatus_M = (Status)i;
                        break;
                    }
                #endregion

                TowerData[TowerData.FindIndex((SingalTowerData) => SingalTowerData.SignalTowerStatus == SelectStatus)] = newSignalTowerData;
            }
            catch (Exception) { }
        }
        /// <summary>
        /// 从数据源读取信号灯数据
        /// </summary>
        /// <returns></returns>
        private bool ReadAllSignalTowerData()
        {
            try
            {
                bool Success = false;
                DataTable ReadDate = DataBase.ReadAllData_Adapter("SignalTowerData", "SystemData.mdb", ref Success);
                if (Success)
                {
                    TowerData.Clear();
                    for (int i = 0; i < ReadDate.Rows.Count; i++)
                    {
                        SignalTowerData tmpSignalData = new SignalTowerData();
                        tmpSignalData.SignalTowerStatus = (SignalTowerStatusType)Enum.Parse(typeof(SignalTowerStatusType), ReadDate.Rows[i]["SignalTowerStatus"].ToString());
                        tmpSignalData.GreenLightStatus_R = (Status)Convert.ToInt32(ReadDate.Rows[i]["Green_R"]);
                        tmpSignalData.YellowLightStatus_R = (Status)Convert.ToInt32(ReadDate.Rows[i]["Yellow_R"]);
                        tmpSignalData.RedLightStatus_R = (Status)Convert.ToInt32(ReadDate.Rows[i]["Red_R"]);
                        tmpSignalData.BuzzStatus_R = (Status)Convert.ToInt32(ReadDate.Rows[i]["Buzzer_R"]);
                        tmpSignalData.GreenLightStatus_M = (Status)Convert.ToInt32(ReadDate.Rows[i]["Green_M"]);
                        tmpSignalData.YellowLightStatus_M = (Status)Convert.ToInt32(ReadDate.Rows[i]["Yellow_M"]);
                        tmpSignalData.RedLightStatus_M = (Status)Convert.ToInt32(ReadDate.Rows[i]["Red_M"]);
                        tmpSignalData.BuzzerStatus_M = (Status)Convert.ToInt32(ReadDate.Rows[i]["Buzzer_M"]);
                        TowerData.Add(tmpSignalData);
                    }
                }
                return Success;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }
        /// <summary>
        /// 将数据写入到数据源中
        /// </summary>
        /// <param name="Type"></param>
        /// <returns></returns>
        private bool WriteSignalTowerData(SignalTowerStatusType Type)
        {
            int ListIndex = TowerData.FindIndex((SingalTowerData) => SingalTowerData.SignalTowerStatus == Type);
            string strSQL = "update SignalTowerData set " +
                       "[Green_R]=" + (int)TowerData[ListIndex].GreenLightStatus_R +
                      ",[Yellow_R]=" + (int)TowerData[ListIndex].YellowLightStatus_R +
                      ",[Red_R]=" + (int)TowerData[ListIndex].RedLightStatus_R +
                      ",[Buzzer_R]=" + (int)TowerData[ListIndex].BuzzStatus_R +
                      ",[Green_M]=" + (int)TowerData[ListIndex].GreenLightStatus_M +
                      ",[Yellow_M]=" + (int)TowerData[ListIndex].YellowLightStatus_M +
                      ",[Red_M]=" + (int)TowerData[ListIndex].RedLightStatus_M +
                      ",[Buzzer_M]=" + (int)TowerData[ListIndex].BuzzerStatus_M +
                      " where [SignalTowerStatus]= \"" + TowerData[ListIndex].SignalTowerStatus.ToString() + "\"";

            int result = DataBase.DataBaseExecute(SysPara.MdbPath, strSQL);
            if (result != 0)
                return false;
            return true;
        }
        /// <summary>
        /// 模式选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <summary>左侧竖排的 6 个机台状态按钮（它们始终是一组单选）。</summary>
        private RadioButton[] StateButtons
        {
            get { return new RadioButton[] { radioButton25, radioButton1, radioButton2, radioButton3, radioButton4, radioButton5 }; }
        }

        private void radioButton_Click(object sender, EventArgs e)
        {
            // 保险：显式保证这 6 个按钮始终是一组单选。
            // WinForms 的 RadioButton 只在"同一个父容器内"自动互斥，一旦它们以后被挪进
            // 不同容器，就会静默变成多组单选 —— 这里兜住这个坑。
            RadioButton clicked = sender as RadioButton;
            foreach (RadioButton rb in StateButtons)
                if (rb != null && rb != clicked) rb.Checked = false;

            SelectStatus = (SignalTowerStatusType)Enum.Parse(typeof(SignalTowerStatusType), ((RadioButton)sender).Tag.ToString());
            int ListIndex = TowerData.FindIndex((SingalTowerData) => SingalTowerData.SignalTowerStatus == SelectStatus);

            ChangeSelectItems(rbGroup_GreenLight_RunMode, rbGroup_GreenLight_RunMode[(int)TowerData[ListIndex].GreenLightStatus_R]);
            ChangeSelectItems(rbGroup_YellowLight_RunMode, rbGroup_YellowLight_RunMode[(int)TowerData[ListIndex].YellowLightStatus_R]);
            ChangeSelectItems(rbGroup_RedLight_RunMode, rbGroup_RedLight_RunMode[(int)TowerData[ListIndex].RedLightStatus_R]);
            ChangeSelectItems(rbGroup_Buzz_RunMode, rbGroup_Buzz_RunMode[(int)TowerData[ListIndex].BuzzStatus_R]);
            ChangeSelectItems(rbGroup_GreenLight_MaintenanceMode, rbGroup_GreenLight_MaintenanceMode[(int)TowerData[ListIndex].GreenLightStatus_M]);
            ChangeSelectItems(rbGroup_YellowLight_MaintenanceMode, rbGroup_YellowLight_MaintenanceMode[(int)TowerData[ListIndex].YellowLightStatus_M]);
            ChangeSelectItems(rbGroup_RedLight_MaintenanceMode, rbGroup_RedLight_MaintenanceMode[(int)TowerData[ListIndex].RedLightStatus_M]);
            ChangeSelectItems(rbGroup_Buzz_MaintenanceMode, rbGroup_Buzz_MaintenanceMode[(int)TowerData[ListIndex].BuzzerStatus_M]);
        }

        public void SwitchSignalTowerStatus(SignalTowerStatusType SignalTowerStatus)
        {
            for (int i = 0; i < TowerData.Count; i++)
            {
                if (TowerData[i].SignalTowerStatus == SignalTowerStatus)
                {
                    if (SysPara.IsMaintenanceMode)
                    {
                        GreenLightStatus = TowerData[i].GreenLightStatus_M;
                        YellowLightStatus = TowerData[i].YellowLightStatus_M;
                        RedLightStatus = TowerData[i].RedLightStatus_M;
                        BuzzerStatus = TowerData[i].BuzzerStatus_M;
                    }
                    else
                    {
                        GreenLightStatus = TowerData[i].GreenLightStatus_R;
                        YellowLightStatus = TowerData[i].YellowLightStatus_R;
                        RedLightStatus = TowerData[i].RedLightStatus_R;
                        BuzzerStatus = TowerData[i].BuzzStatus_R;
                    }
                    break;
                }
            }
        }

		//private void rbRedOff_R_CheckedChanged(object sender, EventArgs e)
		//{
		//}
	}
}
