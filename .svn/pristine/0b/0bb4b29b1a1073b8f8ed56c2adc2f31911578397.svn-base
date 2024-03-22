using NPSDK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using NPSDK;

namespace Alpha
{
    public partial class MotorControlForm : Form
    {
        #region ENUMS
        public enum AxisType
        {
            X = 0,
            Y = 1,
            Z = 2,
            R1 = 3,
            R2 = 4,
            R3 = 5
        }
        public enum MoveType
        {
            LEFT = 0,
            RIGHT = 1,
            FRONT = 2,
            BACK = 3,
            TLEFT = 4,
            TRIGHT = 5,
            UP = 6,
            DOWN = 7
        }
        public enum ActuatorNumber
        {
            ONE = 0,
            TWO = 1,
            THREE = 2,
            FOUR = 3
        }
        public enum ActionType
        {
            NULL = 0,
            CYLINDER = 1,
            GRIPPER = 2,
            VACUUM = 3,
            BLOW = 4,
            BRAKE = 5
        }
		#endregion ENUMS
		
		#region VARIABLES
		//NPMotor
		private const int _MOTORSQTY = 6;
        private const int _ACTUATORSQTY = 4;
        private Adlink_Motor[] NPMotor = new Adlink_Motor[_MOTORSQTY]; //NPMotor
        private MoveType[,] YHMotorType = new MoveType[_MOTORSQTY, 2]; //NPMotor Type
        Button[] ActButtons = new Button[_ACTUATORSQTY]; //Actuator buttons
        private ActionType[,] ActuatorType = new ActionType[_MOTORSQTY, _ACTUATORSQTY]; //Actuator Type
        private Adlink_Output[,] ActuatorTypeOBA = new Adlink_Output[_MOTORSQTY, _ACTUATORSQTY]; //Actuator Type OBA
        private Adlink_Output[,] ActuatorTypeOBB = new Adlink_Output[_MOTORSQTY, _ACTUATORSQTY]; //Actuator Type OBB
        private bool[,] ActuatorTypeOBState = new bool[_MOTORSQTY, _ACTUATORSQTY]; //Actuator state
        private Button[] AxisButton = new Button[_MOTORSQTY];
        private Adlink_Output[] Brake = new Adlink_Output[_MOTORSQTY];
        private List<Adlink_Input>[] Safety = new List<Adlink_Input>[_MOTORSQTY];
        private int iChoseYHMotorIndex = 0; //YHMotorIndex
        private bool bRunGoto = false; //RunGoto
        private double iRunGotoPos = 0; //RunGoto

        //Brake
        private bool bDoBrake = false;
        private JTimer JT_Brake = new JTimer();

        //LOOP
        private bool bRunLoop = false; //run loop
        private double iRunPos1 = 0; //Loop 1
        private double iRunPos2 = 0; //Loop 2
        private int iLoopTask; //loop
        private JTimer RunLoopTM = new JTimer();
        private bool bRunHome = false;

        //GUI
        private bool nonNumberEntered = false;

        //Color
        private Color cToff = Color.DarkGreen;
        private Color cTon = Color.Red;
        private Color cUnselected = Color.White;
        private Color cAcuRABlue = Color.FromArgb(0, 180, 230);
        private Color cActuatorOff = Color.FromArgb(255, 192, 192);
        private Color cActuatorOn = Color.FromArgb(192, 255, 192);
        #endregion VARIABLES

        public MotorControlForm()
        {
            InitializeComponent();

            #region Declarations
            ActButtons[0] = btn_Act1;
            ActButtons[1] = btn_Act2;
            ActButtons[2] = btn_Act3;
            ActButtons[3] = btn_Act4;
            #endregion Declarations
        }

        #region INITIAL
        public void Initial()
        {
            AxisButton[0] = btnAxisX;
            AxisButton[1] = btnAxisY;
            AxisButton[2] = btnAxisZ;
            AxisButton[3] = btnAxisU1;
            AxisButton[4] = btnAxisU2;
            AxisButton[5] = btnAxisU3;

            btn_JOGN.Enabled = false;
            btn_STOP.Enabled = false;
            btn_JOGP.Enabled = false;
            btn_HOME.Enabled = false;
            lb_SelectedMotor.Text = "null";

            btnAxisX.Enabled = NPMotor[0] == null ? false : true;
            btnAxisY.Enabled = NPMotor[1] == null ? false : true;
            btnAxisZ.Enabled = NPMotor[2] == null ? false : true;
            btnAxisU1.Enabled = NPMotor[3] == null ? false : true;
            btnAxisU2.Enabled = NPMotor[4] == null ? false : true;
            btnAxisU3.Enabled = NPMotor[5] == null ? false : true;

            btn_JOGN.BackgroundImage = null;
            btn_JOGP.BackgroundImage = null;

            for (int i = 0; i < _MOTORSQTY; i++)
            {
                if (NPMotor[i] != null)
                {
                    iChoseYHMotorIndex = i;
                    ChangeButtonColor(AxisButton[iChoseYHMotorIndex]);
                    break;
                }
            }

            for (int i = 0; i < _ACTUATORSQTY; i++)
            {
                ActButtons[i].Enabled = false;
                ActButtons[i].BackgroundImage = null;
                ActButtons[i].BackColor = Color.White;
            }
            tmrScan.Enabled = true;
        }
        #endregion INITIAL

        #region GENERAL FUNCTIONS

        #region System
        private void YHMotorControlForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            tmrScan.Enabled = false;
            StopRun();
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
        }
        #endregion System

        #region GUI
        private void ChangeButtonColor(Button sender)
        {
            foreach (Control control in tableLayoutPanel3.Controls)
            {
                if (control is Button)
                    ((Button)control).BackColor = cUnselected;
            }
            ((Button)sender).BackColor = cAcuRABlue;
        }
        private void ChangeButtonYHMotorType(int iYHMotor)
        {
            //Image img = Alpha.Properties.Resources.left;
            switch (YHMotorType[iYHMotor, 0])
            {
                case MoveType.LEFT:
                    //img = Alpha.Properties.Resources.left;
                    break;
                case MoveType.RIGHT:
                    //img = Alpha.Properties.Resources.right;
                    break;
                case MoveType.FRONT:
                    //img = Alpha.Properties.Resources.up;
                    break;
                case MoveType.BACK:
                    //img = Alpha.Properties.Resources.down;
                    break;
                case MoveType.TLEFT:
                    //img = Alpha.Properties.Resources.rotate_left;
                    break;
                case MoveType.TRIGHT:
                    //img = Alpha.Properties.Resources.rotate_right;
                    break;
                case MoveType.UP:
                    //img = Alpha.Properties.Resources.Up1;
                    break;
                case MoveType.DOWN:
                    //img = Alpha.Properties.Resources.Down1;
                    break;
            }
            //btn_JOGN.BackgroundImage = img;
            //switch (YHMotorType[iYHMotor, 1])
            //{
            //    case MoveType.LEFT:
            //        img = Alpha.Properties.Resources.left;
            //        break;
            //    case MoveType.RIGHT:
            //        img = Alpha.Properties.Resources.right;
            //        break;
            //    case MoveType.FRONT:
            //        img = Alpha.Properties.Resources.up;
            //        break;
            //    case MoveType.BACK:
            //        img = Alpha.Properties.Resources.down;
            //        break;
            //    case MoveType.TLEFT:
            //        img = Alpha.Properties.Resources.rotate_left;
            //        break;
            //    case MoveType.TRIGHT:
            //        img = Alpha.Properties.Resources.rotate_right;
            //        break;
            //    case MoveType.UP:
            //        img = Alpha.Properties.Resources.Up1;
            //        break;
            //    case MoveType.DOWN:
            //        img = Alpha.Properties.Resources.Down1;
            //        break;
            //}
            //btn_JOGP.BackgroundImage = img;

            if (btn_JOGN.BackgroundImage != null)
                btn_JOGN.Enabled = true;

            if (btn_JOGP.BackgroundImage != null)
                btn_JOGP.Enabled = true;

            btn_STOP.Enabled = true;
            btn_HOME.Enabled = true;
        }
        private void ChangeButtonActuatorType(int iYHMotor)
        {
            for (int i = 0; i < _ACTUATORSQTY; i++)
            {
                Image img = null;
                switch (ActuatorType[iYHMotor, i])
                {
                    case ActionType.CYLINDER:
                       
                        break;
                    case ActionType.GRIPPER:
                      
                        break;
                    case ActionType.VACUUM:
                       
                        break;
                    case ActionType.BLOW:
                     
                        break;
                    case ActionType.BRAKE:
                       
                        break;
                }
                ActButtons[i].BackgroundImage = img;
                if (ActButtons[i].BackgroundImage != null)
                {
                    ActButtons[i].Enabled = true;
                    ActButtons[i].BackColor = ActuatorTypeOBState[iYHMotor, i] ? cActuatorOn : cActuatorOff;
                }
                else
                {
                    ActButtons[i].Enabled = false;
                    ActButtons[i].BackColor = Color.White;
                }
            }
        }
        private void doActuator(int iYHMotor, ActuatorNumber ActN)
        {
            Adlink_Output OBA = ActuatorTypeOBA[iYHMotor, (int)ActN];
			Adlink_Output OBB = ActuatorTypeOBB[iYHMotor, (int)ActN];
            if (OBA != null)
            {
                if (OBB != null)
                {
                    if (!ActuatorTypeOBState[iYHMotor, (int)ActN])
                    {
                        ActuatorTypeOBState[iYHMotor, (int)ActN] = true;
                        ActButtons[(int)ActN].BackColor = ActuatorTypeOBState[iYHMotor, (int)ActN] ? cActuatorOn : cActuatorOff;
                        OBA.On();
                        OBB.Off();
                    }
                    else
                    {
                        ActuatorTypeOBState[iYHMotor, (int)ActN] = false;
                        ActButtons[(int)ActN].BackColor = ActuatorTypeOBState[iYHMotor, (int)ActN] ? cActuatorOn : cActuatorOff;
                        OBA.Off();
                        OBB.On();
                    }

                }
                else
                {
                    if (!ActuatorTypeOBState[iYHMotor, (int)ActN])
                    {
                        ActuatorTypeOBState[iYHMotor, (int)ActN] = true;
                        ActButtons[(int)ActN].BackColor = ActuatorTypeOBState[iYHMotor, (int)ActN] ? cActuatorOn : cActuatorOff;
                        OBA.On();
                    }
                    else
                    {
                        ActuatorTypeOBState[iYHMotor, (int)ActN] = false;
                        ActButtons[(int)ActN].BackColor = ActuatorTypeOBState[iYHMotor, (int)ActN] ? cActuatorOn : cActuatorOff;
                        OBA.Off();
                    }
                }
            }
        }
        private void btnAxisX_Click(object sender, EventArgs e)
        {
            StopRun();
            iChoseYHMotorIndex = 0;
            ChangeButtonColor(AxisButton[iChoseYHMotorIndex]);
            ChangeButtonYHMotorType(iChoseYHMotorIndex);
            ChangeButtonActuatorType(iChoseYHMotorIndex);
            lb_SelectedMotor.Text = "Axis X";
        }
        private void btnAxisY_Click(object sender, EventArgs e)
        {
            StopRun();
            iChoseYHMotorIndex = 1;
            ChangeButtonColor(AxisButton[iChoseYHMotorIndex]);
            ChangeButtonYHMotorType(iChoseYHMotorIndex);
            ChangeButtonActuatorType(iChoseYHMotorIndex);
            lb_SelectedMotor.Text = "Axis Y";
        }
        private void btnAxisZ_Click(object sender, EventArgs e)
        {
            StopRun();
            iChoseYHMotorIndex = 2;
            ChangeButtonColor(AxisButton[iChoseYHMotorIndex]);
            ChangeButtonYHMotorType(iChoseYHMotorIndex);
            ChangeButtonActuatorType(iChoseYHMotorIndex);
            lb_SelectedMotor.Text = "Axis Z";
        }
        private void btnAxisU_Click(object sender, EventArgs e)
        {
            StopRun();
            iChoseYHMotorIndex = 3;
            ChangeButtonColor(AxisButton[iChoseYHMotorIndex]);
            ChangeButtonYHMotorType(iChoseYHMotorIndex);
            ChangeButtonActuatorType(iChoseYHMotorIndex);
            lb_SelectedMotor.Text = "Axis U1";
        }
        private void btnAxisU2_Click(object sender, EventArgs e)
        {
            StopRun();
            iChoseYHMotorIndex = 4;
            ChangeButtonColor(AxisButton[iChoseYHMotorIndex]);
            ChangeButtonYHMotorType(iChoseYHMotorIndex);
            ChangeButtonActuatorType(iChoseYHMotorIndex);
            lb_SelectedMotor.Text = "Axis U2";
        }
        private void btnAxisU3_Click(object sender, EventArgs e)
        {
            StopRun();
            iChoseYHMotorIndex = 5;
            ChangeButtonColor(AxisButton[iChoseYHMotorIndex]);
            ChangeButtonYHMotorType(iChoseYHMotorIndex);
            ChangeButtonActuatorType(iChoseYHMotorIndex);
            lb_SelectedMotor.Text = "Axis U3";
        }
        private void btn_Act1_Click(object sender, EventArgs e)
        {
            doActuator(iChoseYHMotorIndex, ActuatorNumber.ONE);
        }
        private void btn_Act2_Click(object sender, EventArgs e)
        {
            doActuator(iChoseYHMotorIndex, ActuatorNumber.TWO);
        }
        private void btn_Act3_Click(object sender, EventArgs e)
        {
            doActuator(iChoseYHMotorIndex, ActuatorNumber.THREE);
        }
        private void btn_Act4_Click(object sender, EventArgs e)
        {
            doActuator(iChoseYHMotorIndex, ActuatorNumber.FOUR);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (!GetYHMotorRun())
            {
                if (TextPosition1.Text != "")
                {
                    iRunGotoPos = Convert.ToDouble(TextPosition1.Text);
                    bRunGoto = true;
                }
                else
                {
                    TextPosition1.Focus();
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (!GetYHMotorRun())
            {
                if (TextPosition2.Text != "")
                {
                    iRunGotoPos = Convert.ToDouble(TextPosition2.Text);
                    bRunGoto = true;
                }
                else
                {
                    TextPosition1.Focus();
                }
            }
        }
        private void TextPosition1_DoubleClick(object sender, EventArgs e)
        {
            TextPosition1.Text = labPos.Text;
        }
        private void TextPosition2_DoubleClick(object sender, EventArgs e)
        {
            TextPosition2.Text = labPos.Text;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (TextGap.Text != "" && TextPosition1.Text != "")
                TextPosition1.Text = (Convert.ToDouble(TextPosition1.Text) + Convert.ToDouble(TextGap.Text)).ToString();
        }
        private void button4_Click(object sender, EventArgs e)
        {
            if (TextGap.Text != "" && TextPosition2.Text != "")
                TextPosition2.Text = (Convert.ToDouble(TextPosition2.Text) + Convert.ToDouble(TextGap.Text)).ToString();
        }
        private void button6_Click(object sender, EventArgs e)
        {
            StopRun();
        }
        private void button19_Click(object sender, EventArgs e)
        {
            bool r1 = getIsSafetyMovement();
            if (!r1)
            {
                  string sMsgBox="Safety Sensor is active, can't move";
                  MethodBase MB = MethodBase.GetCurrentMethod();
                string sTittle="Movement Exception "+MB.ReflectedType.Name+MB.Name;
                MessageBox.Show(new Form { TopMost = true }, sMsgBox,sTittle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0x40000);
                return;
            }
            if (!GetYHMotorRun())
            {
                NPMotor[iChoseYHMotorIndex].HomeReset();
                bRunHome = true;
            }
        }
        private void button9_Click(object sender, EventArgs e)
        {
            if (TextSpeed.Text != "")
                NPMotor[iChoseYHMotorIndex].WorkSpeed = Convert.ToDouble(TextSpeed.Text);
            else
                TextSpeed.Focus();
        }
        private void button11_Click(object sender, EventArgs e)
        {
            if (TextAcceleration.Text != "")
                NPMotor[iChoseYHMotorIndex].Tacc = Convert.ToDouble(TextAcceleration.Text);
            else
                TextAcceleration.Focus();
        }
        private void button15_Click(object sender, EventArgs e)
        {
            if (TextDeceleration.Text != "")
                NPMotor[iChoseYHMotorIndex].Tdec = Convert.ToDouble(TextDeceleration.Text);
            else
                TextDeceleration.Focus();
        }
        private void btn_JOGP_MouseDown(object sender, MouseEventArgs e)
        {
            bool r1 = getIsSafetyMovement();
            if (!r1)
            {
                NPMotor[iChoseYHMotorIndex].Stop();
                 string sMsgBox="Safety Sensor is active, can't move";
                  MethodBase MB = MethodBase.GetCurrentMethod();
                string sTittle=$"Movement Exception {MB.ReflectedType.Name} {MB.Name}";
                MessageBox.Show(new Form { TopMost = true }, sMsgBox,sTittle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0x40000);
                return;
            }
            else
            {
                if (!GetYHMotorRun())
                {
                    doReleaseBrake();
                    NPMotor[iChoseYHMotorIndex].JogN();
                    return;
                }
            }
        }
        private void btn_JOGN_MouseUp(object sender, MouseEventArgs e)
        {
            doBrakeWithDelay();
            NPMotor[iChoseYHMotorIndex].Stop();
            bRunGoto = false;
        }
        private void btn_JOGN_MouseDown(object sender, MouseEventArgs e)
        {
            bool r1 = getIsSafetyMovement();
            if (!r1)
            {
                NPMotor[iChoseYHMotorIndex].Stop();
                 string sMsgBox="Safety Sensor is active, can't move";
                  MethodBase MB = MethodBase.GetCurrentMethod();
                string sTittle=$"Movement Exception {MB.ReflectedType.Name} {MB.Name}";
                MessageBox.Show(new Form { TopMost = true }, sMsgBox,sTittle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0x40000);
                return;
            }
            else
            {
                if (!GetYHMotorRun())
                {
                    doReleaseBrake();
                    NPMotor[iChoseYHMotorIndex].JogP();
                    return;
                }
            }
        }
        private void btnChangeHomeDirection_Click(object sender, EventArgs e)
        {
            ReverseHomeDirection();
        }
        private void btnChangeMoveDirection_Click(object sender, EventArgs e)
        {
            ReverseMoveDirection();
        }
        private void button23_Click(object sender, EventArgs e)
        {
            if (textBox2.Text != "")
                NPMotor[iChoseYHMotorIndex].EncGearRatio = Convert.ToDouble(textBox2.Text);
            else
                textBox2.Focus();
        }
        private void button22_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
                NPMotor[iChoseYHMotorIndex].GearRatio = Convert.ToDouble(textBox1.Text);
            else
                textBox1.Focus();
        }
        private void button24_Click(object sender, EventArgs e)
        {
            NPMotor[iChoseYHMotorIndex].SetPos(0);
            NPMotor[iChoseYHMotorIndex].SetEncPos(0);
        }
        private void button_ResetAlarm_Click(object sender, EventArgs e)
        {
            //MiddleLayer.SliderF.Mt_SliderLeft.AlarmReset();
            //MiddleLayer.SliderF.Mt_SliderRight.AlarmReset();

            NPMotor[iChoseYHMotorIndex].AlarmReset();
            NPMotor[iChoseYHMotorIndex].AlarmReset();
            NPMotor[iChoseYHMotorIndex].ServoOn();

        }
        private void button5_Click(object sender, EventArgs e)
        {
            if (!GetYHMotorRun())
            {
                if (TextPosition1.Text != "" && TextPosition2.Text != "")
                {
                    iLoopTask = 1;
                    iRunPos1 = Convert.ToDouble(TextPosition1.Text);
                    iRunPos2 = Convert.ToDouble(TextPosition2.Text);
                    bRunLoop = true;
                }
            }
        }
        private void button7_Click(object sender, EventArgs e)
        {
            if (TextSpeedRatio.Text != "")
                NPMotor[iChoseYHMotorIndex].SpeedRatio = Convert.ToDouble(TextSpeedRatio.Text);
            else
                TextSpeedRatio.Focus();
        }
        private void TextPosition1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!nonNumberEntered)
            {
                if (e.KeyChar == 45 && (((TextBox)sender).SelectionStart != 0 || ((TextBox)sender).Text.IndexOf("-") >= 0))
                {
                    e.Handled = true;
                }
                else if (e.KeyChar != 45 && e.KeyChar != 8)
                {
                    if (e.KeyChar == 46 && ((TextBox)sender).Text.IndexOf(".") >= 0)
                        e.Handled = true;
                    if ((e.KeyChar < 48 || e.KeyChar > 57) && e.KeyChar != 46)
                        e.Handled = true;
                }
            }
        }
        private void TextPosition1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.C | Keys.Control) || e.KeyData == (Keys.A | Keys.Control) || e.KeyData == (Keys.V | Keys.Control) || e.KeyData == (Keys.X | Keys.Control))
                nonNumberEntered = true;
            else
                nonNumberEntered = false;
        }
        private void button10_Click(object sender, EventArgs e)
        {
            NPMotor[iChoseYHMotorIndex].ServoOn();
        }
        private void button12_Click(object sender, EventArgs e)
        {
            NPMotor[iChoseYHMotorIndex].ServoOff();
        }
        #endregion GUI

        #region NPMotor
        public void setMotor(AxisType YHMotorNumber, Adlink_Motor JYHMotor, MoveType _Negative, MoveType _Positive, Adlink_Output OB_Brake, List<Adlink_Input> IB_Safety)
        {
            NPMotor[(int)YHMotorNumber] = JYHMotor;
            YHMotorType[(int)YHMotorNumber, 1] = _Negative;
            YHMotorType[(int)YHMotorNumber, 0] = _Positive;
            NPMotor[(int)YHMotorNumber].WorkSpeed = 33;
            NPMotor[(int)YHMotorNumber].SpeedRatio = 100;
            Brake[(int)YHMotorNumber] = OB_Brake;
            Safety[(int)YHMotorNumber] = IB_Safety;
        }
        public void setActuator(AxisType YHMotorNumber, ActuatorNumber ActuatorN, ActionType ActuatorT, Adlink_Output OB_ActA, Adlink_Output OB_ActB)
        {
            ActuatorType[(int)YHMotorNumber, (int)ActuatorN] = ActuatorT;
            ActuatorTypeOBA[(int)YHMotorNumber, (int)ActuatorN] = OB_ActA;
            ActuatorTypeOBB[(int)YHMotorNumber, (int)ActuatorN] = OB_ActB;
        }
        private void ReverseHomeDirection()
        {
            NPMotor[iChoseYHMotorIndex].HomeDir = !NPMotor[iChoseYHMotorIndex].HomeDir;
        }
        private void ReverseMoveDirection()
        {
            NPMotor[iChoseYHMotorIndex].MoveDir = !NPMotor[iChoseYHMotorIndex].MoveDir;
        }
        bool GetYHMotorRun()
        {
            return bRunGoto || bRunHome || bRunLoop;
        }
        void StopRun()
        {
            bRunGoto = false;
            bRunHome = false;
            bRunLoop = false;
            NPMotor[iChoseYHMotorIndex].Stop();
        }
        private void doReleaseBrake()
        {
            if (Brake[iChoseYHMotorIndex] != null)
                Brake[iChoseYHMotorIndex].On();
            bDoBrake = false;
        }
        private bool getIsSafetyMovement()
        {
            bool r1 = true;
            if (Safety[iChoseYHMotorIndex] != null)
            {
                int iTotalItemsList = Safety[iChoseYHMotorIndex].Count;
                if (iTotalItemsList > 0)
                {
                    for (int i = 0; i < iTotalItemsList; i++)
                    {
                        if (Safety[iChoseYHMotorIndex][i].On())
                        {
                            r1 = false;
                        }
                    }
                }
            }
            return r1;
        }
        private void doBrakeWithDelay()
        {
            bDoBrake = true;
            JT_Brake.Restart();
        }
        #endregion NPMotor

        #endregion GENERAL FUNCTIONS

        #region ASYNC
        private void tmrScan_Tick(object sender, EventArgs e)
        {
            #region Brake
            if (JT_Brake.IsOn(2000) && bDoBrake)
            {
                bDoBrake = false;
                if (Brake[iChoseYHMotorIndex] != null)
                    Brake[iChoseYHMotorIndex].Off();
            }
            #endregion Brake

            tmrScan.Enabled = false;
            if (NPMotor[iChoseYHMotorIndex] != null)
            {
                labMoveSpeed.Text = NPMotor[iChoseYHMotorIndex].WorkSpeed.ToString();
                labMoveAcceleration.Text = NPMotor[iChoseYHMotorIndex].Tacc.ToString();
                labMoveDeceleration.Text = NPMotor[iChoseYHMotorIndex].Tdec.ToString();
                labGearRatio.Text = NPMotor[iChoseYHMotorIndex].GearRatio.ToString();
                labEncoderGearRatio.Text = NPMotor[iChoseYHMotorIndex].EncGearRatio.ToString();
                labHomeDirection.Text = NPMotor[iChoseYHMotorIndex].HomeDir.ToString();
                labMoveDirection.Text = NPMotor[iChoseYHMotorIndex].MoveDir.ToString();
                labSpdRate.Text = NPMotor[iChoseYHMotorIndex].SpeedRatio.ToString();
                labPos.Text = (NPMotor[iChoseYHMotorIndex].GetPos()).ToString("N4");
                labRealPos.Text = (NPMotor[iChoseYHMotorIndex].GetEncPos()).ToString("N4");
                NPSDK.MotionState MState = NPMotor[iChoseYHMotorIndex].GetMotionState();
                NPSDK.AxisIOState IOState = NPMotor[iChoseYHMotorIndex].GetAxisIOState();
                panel_HMV.BackColor = MState.HMV ? cToff : cTon;
                panel_SMV.BackColor = MState.SMV ? cToff : cTon;
                panel_ALM.BackColor = IOState.ALM ? cToff : cTon;
                panel_EMG.BackColor = IOState.EMG ? cToff : cTon;
                panel_MEL.BackColor = IOState.MEL ? cToff : cTon;
                panel_PEL.BackColor = IOState.PEL ? cToff : cTon;
                panel_SVON.BackColor = IOState.SVON ? cToff : cTon;
                panel_ORG.BackColor = IOState.ORG ? cToff : cTon;

                bool r01 = getIsSafetyMovement();

                bool r02 = MState.SMV || MState.HMV;
                if (!r01 && r02)
                {
                    bRunGoto = false;
                    bRunHome = false;
                    bRunLoop = false;
                    NPMotor[iChoseYHMotorIndex].Stop();
                    string sMsgBox="Safety Sensor is active, can't move";
                     MethodBase MB = MethodBase.GetCurrentMethod();
                    string sTittle=$"Movement Exception {MB.ReflectedType.Name} {MB.Name}";
                    MessageBox.Show(new Form { TopMost = true }, sMsgBox,sTittle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0x40000);
                }
                if (bRunGoto)
                {
                    if (NPMotor[iChoseYHMotorIndex].Goto(iRunGotoPos))
                    {
                        bRunGoto = false;
                        doBrakeWithDelay();
                    }
                }
                else if (bRunHome)
                {
                    doReleaseBrake();
                    if (NPMotor[iChoseYHMotorIndex].Home())
                    {
                        bRunHome = false;
                        doBrakeWithDelay();
                    }
                }
                else if (bRunLoop)
                {
                    doReleaseBrake();
                    switch (iLoopTask)
                    {
                        case 1:
                            if (NPMotor[iChoseYHMotorIndex].Goto(iRunPos1))
                            {
                                RunLoopTM.Restart();
                                iLoopTask = 2;
                            }
                            break;
                        case 2:
                            if (TextDelay.Text == "")
                                iLoopTask = 3;
                            else if (RunLoopTM.IsOn(Convert.ToInt16(TextDelay.Text)))
                                iLoopTask = 3;
                            break;
                        case 3:
                            if (NPMotor[iChoseYHMotorIndex].Goto(iRunPos2))
                            {
                                RunLoopTM.Restart();
                                iLoopTask = 4;
                            }
                            break;
                        case 4:
                            if (TextDelay.Text == "")
                                iLoopTask = 1;
                            else if (RunLoopTM.IsOn(Convert.ToInt16(TextDelay.Text)))
                                iLoopTask = 1;
                            break;
                    }
                }
            }
            tmrScan.Enabled = true;
        }
        #endregion ASYNC

        private void btn_JOGP_Click(object sender, EventArgs e)
        {

        }
    }
}
