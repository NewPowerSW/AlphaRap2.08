using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NPSDK;
using System.Windows.Forms;
using System.Data;
using System.Threading;

namespace Alpha._0
{
    public class Servo
    {
        public ManualResetEvent manualReset = new ManualResetEvent(true);
        private Adlink_Motor Servo_X;
        private bool _xAxisHomeIsOK = false;
        private bool _yAxisHomeIsOK = false;
        private bool _zAxisHomeIsOK = false;
        private bool _r1AxisHomeIsOK = false;
        private bool _r2AxisHomeIsOK = false;
        private bool _r3AxisHomeIsOK = false;
        private bool _uAxisHomeIsOK = false;
        public Thread _homeWorkTh;
        public Adlink_Motor ServoX
        {
            get
            {
                return Servo_X;
            }
            set
            {
                if (Servo_X == null && Servo_X == value)
                    return;
                Servo_X = value;
            }
        }

        private Adlink_Motor Servo_Y;
        public Adlink_Motor ServoY
        {
            get
            {
                return Servo_Y;
            }
            set
            {
                if (Servo_Y == null && Servo_Y == value)
                    return;
                Servo_Y = value;
            }
        }

        private Adlink_Motor Servo_Z;
        public Adlink_Motor ServoZ
        {
            get
            {
                return Servo_Z;
            }
            set
            {
                if (Servo_Z == null && Servo_Z == value)
                    return;
                Servo_Z = value;
            }
        }

        private Adlink_Motor Servo_U;
        public Adlink_Motor ServoU
        {
            get
            {
                return Servo_U;
            }
            set
            {
                if (Servo_U == null && Servo_U == value)
                    return;
                Servo_U = value;
            }
        }

        private Adlink_Motor Servo_R1;
        public Adlink_Motor ServoR1
        {
            get
            {
                return Servo_R1;
            }
            set
            {
                if (Servo_R1 == null && Servo_R1 == value)
                    return;
                Servo_R1 = value;
            }
        }

        private Adlink_Motor Servo_R2;
        public Adlink_Motor ServoR2
        {
            get
            {
                return Servo_R2;
            }
            set
            {
                if (Servo_R2 == null && Servo_R2 == value)
                    return;
                Servo_R2 = value;
            }
        }

        private Adlink_Motor Servo_R3;
        public Adlink_Motor ServoR3
        {
            get
            {
                return Servo_R3;
            }
            set
            {
                if (Servo_R3 == null && Servo_R3 == value)
                    return;
                Servo_R3 = value;
            }
        }
        List<Adlink_Motor> motors = new List<Adlink_Motor>();
        //Axis Homing is OK

        public bool Servo_X_IsHomeOK
        {
            get
            {
                if (Servo_X != null) { return _xAxisHomeIsOK; }
                else { return false; }
            }
        }
        public bool Servo_Y_IsHomeOK
        {
            get
            {
                if (Servo_Y != null) { return _yAxisHomeIsOK; }
                else { return false; }
            }
        }
        public bool Servo_Z_IsHomeOK
        {
            get
            {
                if (Servo_Z != null) { return _zAxisHomeIsOK; }
                else { return false; }
            }
        }
        public bool Servo_R1_IsHomeOK
        {
            get
            {
                if (Servo_R1 != null) { return _r1AxisHomeIsOK; }
                else { return false; }
            }
        }
        public bool Servo_R2_IsHomeOK
        {
            get
            {
                if (Servo_R2 != null) { return _r2AxisHomeIsOK; }
                else { return false; }
            }
        }

        public bool Servo_R3_IsHomeOK
        {
            get
            {
                if (Servo_R3 != null) { return _r3AxisHomeIsOK; }
                else { return false; }
            }
        }
        public bool Servo_U_IsHomeOK
        {
            get
            {
                if (Servo_U != null) { return _uAxisHomeIsOK; }
                else { return false; }
            }
        }
        private JTimer HomeJtime = new JTimer();

        public Servo(Adlink_Motor X = null, Adlink_Motor Y = null, Adlink_Motor Z = null, Adlink_Motor R1 = null, Adlink_Motor R2 = null, Adlink_Motor R3 = null, Adlink_Motor U = null)
        {
            if (X != null) { Servo_X = X; motors.Add(Servo_X); }
            if (Y != null) { Servo_Y = Y; motors.Add(Servo_Y); }
            if (Z != null) { Servo_Z = Z; motors.Add(Servo_Z); }
            if (R1 != null) { Servo_R1 = R1; motors.Add(Servo_R1); }
            if (R2 != null) { Servo_R2 = R2; motors.Add(Servo_R2); }
            if (R3 != null) { Servo_R3 = R3; motors.Add(Servo_R3); }
            if (U != null) { Servo_U = U; motors.Add(Servo_U); }
        }
        /// <summary>
        /// Enable all servo
        /// </summary>
        /// <param name="motors"></param>
        /// <returns></returns>
        public bool ServoAllOn()
        {
            if (motors == null)
            {
                return false;
            }
            for (int i = 0; i < motors.Count; i++)
            {
                motors[i].ServoOn();
            }
            return true;
        }

        /// <summary>
        /// Disnable all servo
        /// </summary>
        /// <param name="motors"></param>
        /// <returns></returns>
        public bool ServoAllOff()
        {
            if (motors == null)
            {
                return false;
            }
            for (int i = 0; i < motors.Count; i++)
            {
                motors[i].ServoOff();
            }
            return true;
        }

        /// <summary>
        /// Single Axis enable
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="isOn"></param>
        public void ServoOnOrOff(ServoAixsName axis, bool isOn)
        {
            switch (axis)
            {
                case ServoAixsName.X:
                    if (isOn) { Servo_X.ServoOn(); }
                    else { Servo_X.ServoOff(); }
                    break;
                case ServoAixsName.Y:
                    if (isOn) { Servo_Y.ServoOn(); }
                    else { Servo_Y.ServoOff(); }
                    break;
                case ServoAixsName.Z:
                    if (isOn) { Servo_Z.ServoOn(); }
                    else { Servo_Z.ServoOff(); }
                    break;
                case ServoAixsName.R1:
                    if (isOn) { Servo_R1.ServoOn(); }
                    else { Servo_R1.ServoOff(); }
                    break;
                case ServoAixsName.R2:
                    if (isOn) { Servo_R2.ServoOn(); }
                    else { Servo_R2.ServoOff(); }
                    break;
                case ServoAixsName.R3:
                    if (isOn) { Servo_R3.ServoOn(); }
                    else { Servo_R3.ServoOff(); }
                    break;
                case ServoAixsName.U:
                    if (isOn) { Servo_U.ServoOn(); }
                    else { Servo_U.ServoOff(); }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Stop all axis
        /// </summary>
        /// <param name="motors"></param>
        /// <returns></returns>
        public bool StopAllMortos()
        {
            if (motors == null)
            {
                return false;
            }
            for (int i = 0; i < motors.Count; i++)
            {
                motors[i].Stop();
            }
            return true;
        }

        /// <summary>
        /// Stop single axis
        /// </summary>
        /// <param name="axis"></param>
        public void StopMotor(ServoAixsName axis)
        {
            switch (axis)
            {
                case ServoAixsName.X:
                    Servo_X.Stop();
                    break;
                case ServoAixsName.Y:
                    Servo_Y.Stop();
                    break;
                case ServoAixsName.Z:
                    Servo_Z.Stop();
                    break;
                case ServoAixsName.R1:
                    Servo_R1.Stop();
                    break;
                case ServoAixsName.R2:
                    Servo_R2.Stop();
                    break;
                case ServoAixsName.R3:
                    Servo_R3.Stop();
                    break;
                case ServoAixsName.U:
                    Servo_U.Stop();
                    break;
                default:
                    break;
            }
        }

        #region Home
        /// <summary>
        /// single axis home
        /// </summary>
        /// <param name="axis">axis</param>
        /// <param name="homeHighSpeed">Max home speed</param>
        /// <param name="homeLowSpeed">min home speed</param>
        /// <param name="timeout">home timeout</param>
        public void HomeAxis(ServoAixsName axis, double homeHighSpeed, double homeLowSpeed, int timeout)
        {
            AxisIOState XIOState;
            AxisIOState YIOState;
            AxisIOState ZIOState;
            AxisIOState UIOState;
            AxisIOState R1IOState;
            AxisIOState R2IOState;
            AxisIOState R3IOState;
            HomeJtime.Restart();
            if (_homeWorkTh != null)
            {
                if (_homeWorkTh.IsAlive)
                {
                    _homeWorkTh.Abort();
                }
            }
            switch (axis)
            {
                #region ?X Axis home?
                case ServoAixsName.X:
                    Servo_X.AlarmReset();
                    Servo_X.Stop();
                    Servo_X.HomeHighSpd = homeHighSpeed;
                    Servo_X.HomeLowSpd = homeLowSpeed;
                    XIOState = Servo_X.GetAxisIOState();
                    _xAxisHomeIsOK = false;
                    _homeWorkTh = new Thread(new ThreadStart(() =>
                    {
                        Servo_X.Stop();
                        Servo_X.HomeReset();
                        do
                        {
                            manualReset.WaitOne();
                            _xAxisHomeIsOK = Servo_X.Home();
                            XIOState = Servo_X.GetAxisIOState();
                            YIOState = Servo_Y.GetAxisIOState();
                            ZIOState = Servo_Z.GetAxisIOState();
                            if (XIOState.SLN || XIOState.SLP || XIOState.ALM
                            || YIOState.SLN || YIOState.SLP || YIOState.ALM
                            || ZIOState.SLN || ZIOState.SLP || ZIOState.ALM)
                            {
                                break;
                            }
                        } while (!_xAxisHomeIsOK);
                    }));
                    break;
                #endregion

                #region ?Y Axis home?
                case ServoAixsName.Y:
                    Servo_Y.AlarmReset();
                    Servo_Y.Stop();
                    Servo_Y.HomeHighSpd = homeHighSpeed;
                    Servo_Y.HomeLowSpd = homeLowSpeed;
                    YIOState = Servo_Y.GetAxisIOState();
                    _yAxisHomeIsOK = false;
                    _homeWorkTh = new Thread(new ThreadStart(() =>
                    {
                        Servo_Y.Stop();
                        Servo_Y.HomeReset();
                        do
                        {
                            manualReset.WaitOne();
                            _yAxisHomeIsOK = Servo_Y.Home();
                            XIOState = Servo_X.GetAxisIOState();
                            YIOState = Servo_Y.GetAxisIOState();
                            ZIOState = Servo_Z.GetAxisIOState();
                            if (XIOState.SLN || XIOState.SLP || XIOState.ALM
                            || YIOState.SLN || YIOState.SLP || YIOState.ALM
                            || ZIOState.SLN || ZIOState.SLP || ZIOState.ALM)

                            {
                                break;
                            }
                        } while (!_yAxisHomeIsOK);
                    }));
                    break;
                #endregion

                #region ?Z Axis home?
                case ServoAixsName.Z:
                    Servo_Z.AlarmReset();
                    Servo_Z.Stop();
                    Servo_Z.HomeHighSpd = homeHighSpeed;
                    Servo_Z.HomeLowSpd = homeLowSpeed;
                    ZIOState = Servo_Z.GetAxisIOState();
                    _zAxisHomeIsOK = false;
                    _homeWorkTh = new Thread(new ThreadStart(() =>
                    {
                        Servo_Z.Stop();
                        Servo_Z.HomeReset();
                        do
                        {
                            manualReset.WaitOne();
                            _zAxisHomeIsOK = Servo_Z.Home();
                            XIOState = Servo_X.GetAxisIOState();
                            YIOState = Servo_Y.GetAxisIOState();
                            ZIOState = Servo_Z.GetAxisIOState();
                            if (XIOState.SLN || XIOState.SLP || XIOState.ALM
                                || YIOState.SLN || YIOState.SLP || YIOState.ALM
                                || ZIOState.SLN || ZIOState.SLP || ZIOState.ALM)
                            {
                                break;
                            }
                        } while (!_zAxisHomeIsOK);
                    }));
                    break;
                #endregion

                #region ?U Axis home?
                case ServoAixsName.U:
                    Servo_U.AlarmReset();
                    Servo_U.Stop();
                    Servo_U.HomeHighSpd = homeHighSpeed;
                    Servo_U.HomeLowSpd = homeLowSpeed;
                    UIOState = Servo_U.GetAxisIOState();
                    _uAxisHomeIsOK = false;
                    Task.Factory.StartNew(() =>
                    {
                        Servo_U.HomeReset();
                        do
                        {
                            manualReset.WaitOne();
                            _uAxisHomeIsOK = Servo_U.Home();
                            XIOState = Servo_X.GetAxisIOState();
                            YIOState = Servo_Y.GetAxisIOState();
                            ZIOState = Servo_Z.GetAxisIOState();
                            UIOState = Servo_U.GetAxisIOState();
                            if (XIOState.SLN || XIOState.SLP || XIOState.ALM
                                || YIOState.SLN || YIOState.SLP || YIOState.ALM
                                || ZIOState.SLN || ZIOState.SLP || ZIOState.ALM
                                || UIOState.SLN || UIOState.SLP || UIOState.ALM)
                            {
                                break;
                            }
                        } while (!_uAxisHomeIsOK);
                    });
                    break;
                #endregion

                #region ?R1 Axis home?
                case ServoAixsName.R1:
                    Servo_R1.AlarmReset();
                    Servo_R1.Stop();
                    Servo_R1.HomeHighSpd = homeHighSpeed;
                    Servo_R1.HomeLowSpd = homeLowSpeed;
                    UIOState = Servo_R1.GetAxisIOState();
                    _r1AxisHomeIsOK = false;
                    if (_homeWorkTh != null)
                    {
                        if (_homeWorkTh.IsAlive)
                        {
                            _homeWorkTh.Abort();
                        }
                    }
                    _homeWorkTh = new Thread(new ThreadStart(() =>
                    {
                        Servo_R1.HomeReset();
                        do
                        {
                            manualReset.WaitOne();
                            _r1AxisHomeIsOK = Servo_R1.Home();
                            XIOState = Servo_X.GetAxisIOState();
                            YIOState = Servo_Y.GetAxisIOState();
                            ZIOState = Servo_Z.GetAxisIOState();
                            R1IOState = Servo_R1.GetAxisIOState();
                            R2IOState = Servo_R2.GetAxisIOState();
                            R3IOState = Servo_R3.GetAxisIOState();
                            if (XIOState.SLN || XIOState.SLP || XIOState.ALM
                                || YIOState.SLN || YIOState.SLP || YIOState.ALM
                                || ZIOState.SLN || ZIOState.SLP || ZIOState.ALM
                                || UIOState.SLN || UIOState.SLP || UIOState.ALM
                                || R1IOState.SLN || R1IOState.SLP || R1IOState.ALM
                                || R2IOState.SLN || R2IOState.SLP || R2IOState.ALM
                                || R3IOState.SLN || R3IOState.SLP || R3IOState.ALM)
                            {
                                break;
                            }
                        } while (!_r1AxisHomeIsOK);
                    }));
                    break;
                #endregion

                #region ?R2 Axis home?
                case ServoAixsName.R2:
                    Servo_R2.AlarmReset();
                    Servo_R2.Stop();
                    Servo_R2.HomeHighSpd = homeHighSpeed;
                    Servo_R2.HomeLowSpd = homeLowSpeed;
                    UIOState = Servo_R2.GetAxisIOState();
                    _r2AxisHomeIsOK = false;
                    if (_homeWorkTh != null)
                    {
                        if (_homeWorkTh.IsAlive)
                        {
                            _homeWorkTh.Abort();
                        }
                    }
                    _homeWorkTh = new Thread(new ThreadStart(() =>
                    {
                        Servo_R2.HomeReset();
                        do
                        {
                            manualReset.WaitOne();
                            _r2AxisHomeIsOK = Servo_R2.Home();
                            XIOState = Servo_X.GetAxisIOState();
                            YIOState = Servo_Y.GetAxisIOState();
                            ZIOState = Servo_Z.GetAxisIOState();
                            R1IOState = Servo_R1.GetAxisIOState();
                            R2IOState = Servo_R2.GetAxisIOState();
                            R3IOState = Servo_R3.GetAxisIOState();
                            if (XIOState.SLN || XIOState.SLP || XIOState.ALM
                                || YIOState.SLN || YIOState.SLP || YIOState.ALM
                                || ZIOState.SLN || ZIOState.SLP || ZIOState.ALM
                                || UIOState.SLN || UIOState.SLP || UIOState.ALM
                                || R1IOState.SLN || R1IOState.SLP || R1IOState.ALM
                                || R2IOState.SLN || R2IOState.SLP || R2IOState.ALM
                                || R3IOState.SLN || R3IOState.SLP || R3IOState.ALM)
                            {
                                break;
                            }
                        } while (!_r2AxisHomeIsOK);
                    }));
                    break;
                #endregion

                #region ?R3 Axis home?
                case ServoAixsName.R3:
                    Servo_R3.AlarmReset();
                    Servo_R3.Stop();
                    Servo_R3.HomeHighSpd = homeHighSpeed;
                    Servo_R3.HomeLowSpd = homeLowSpeed;
                    UIOState = Servo_R3.GetAxisIOState();
                    _r3AxisHomeIsOK = false;
                    if (_homeWorkTh != null)
                    {
                        if (_homeWorkTh.IsAlive)
                        {
                            _homeWorkTh.Abort();
                        }
                    }
                    _homeWorkTh = new Thread(new ThreadStart(() =>
                    {
                        Servo_R3.HomeReset();
                        do
                        {
                            manualReset.WaitOne();
                            _r3AxisHomeIsOK = Servo_R3.Home();
                            XIOState = Servo_X.GetAxisIOState();
                            YIOState = Servo_Y.GetAxisIOState();
                            ZIOState = Servo_Z.GetAxisIOState();
                            R1IOState = Servo_R1.GetAxisIOState();
                            R2IOState = Servo_R2.GetAxisIOState();
                            R3IOState = Servo_R3.GetAxisIOState();
                            if (XIOState.SLN || XIOState.SLP || XIOState.ALM
                                || YIOState.SLN || YIOState.SLP || YIOState.ALM
                                || ZIOState.SLN || ZIOState.SLP || ZIOState.ALM
                                || UIOState.SLN || UIOState.SLP || UIOState.ALM
                                || R1IOState.SLN || R1IOState.SLP || R1IOState.ALM
                                || R2IOState.SLN || R2IOState.SLP || R2IOState.ALM
                                || R3IOState.SLN || R3IOState.SLP || R3IOState.ALM)
                            {
                                break;
                            }
                        } while (!_r3AxisHomeIsOK);
                    }));
                    break;
                    #endregion
            }
            _homeWorkTh.IsBackground = true;
            _homeWorkTh.Start();
        }

        #endregion
        /// <summary>
        /// Single axis JOG
        /// </summary>
        /// <param name="axis">axis</param>
        /// <param name="pos">position</param>
        /// <returns></returns>
        public bool GotoAxis(ServoAixsName axis, double pos)
        {
            bool status = false;
            switch (axis)
            {
                case ServoAixsName.X:
                    status = Servo_X.Goto(pos);
                    break;
                case ServoAixsName.Y:
                    status = Servo_Y.Goto(pos);
                    break;
                case ServoAixsName.Z:
                    status = Servo_Z.Goto(pos);
                    break;
                case ServoAixsName.R1:
                    status = Servo_R1.Goto(pos);
                    break;
                case ServoAixsName.R2:
                    status = Servo_R2.Goto(pos);
                    break;
                case ServoAixsName.R3:
                    status = Servo_R3.Goto(pos);
                    break;
                case ServoAixsName.U:
                    status = Servo_U.Goto(pos);
                    break;
                default:
                    break;
            }
            return status;
        }

        /// <summary>
        /// Get X Y Z U position from table
        /// </summary>
        /// <param name="tarName">备注内容</param>
        /// <param name="table">表</param>
        /// <returns></returns>
        public double[] GetDestPosition(DataTable table, string tarName)
        {
            double[] destP = { 0, 0, 0 };
            DataTable dt = table;   //表名称在recipeDataSet中设置
            int tarIndex = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (tarName == dt.Rows[i]["Annotation"].ToString())
                {
                    tarIndex = i;
                    break;
                }
            }
            if (dt.Columns[0].ColumnName == "X") { destP[0] = double.Parse(dt.Rows[tarIndex][0].ToString()); }
            if (dt.Columns[1].ColumnName == "Y") { destP[1] = double.Parse(dt.Rows[tarIndex][1].ToString()); }
            if (dt.Columns[2].ColumnName == "Z") { destP[2] = double.Parse(dt.Rows[tarIndex][2].ToString()); }
            if (dt.Columns[3].ColumnName == "U") { destP[3] = double.Parse(dt.Rows[tarIndex][3].ToString()); }
            return destP;
        }

        /// <summary>
        /// Get current axis position
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public double GetCurrentPos(ServoAixsName axis)
        {
            double pos = 0;
            switch (axis)
            {
                case ServoAixsName.X:
                    pos = Servo_X.GetEncPos();
                    break;
                case ServoAixsName.Y:
                    pos = Servo_Y.GetEncPos();
                    break;
                case ServoAixsName.Z:
                    pos = Servo_Z.GetEncPos();
                    break;
                case ServoAixsName.R1:
                    pos = Servo_R1.GetEncPos();
                    break;
                case ServoAixsName.R2:
                    pos = Servo_R2.GetEncPos();
                    break;
                case ServoAixsName.R3:
                    pos = Servo_R3.GetEncPos();
                    break;
                case ServoAixsName.U:
                    pos = Servo_U.GetEncPos();
                    break;
                default:
                    break;
            }
            return pos;
        }

        public static bool DelayMs(int delayMilliseconds)
        {
            DateTime now = DateTime.Now;
            Double s;
            do
            {
                TimeSpan spand = DateTime.Now - now;
                s = spand.TotalMilliseconds + spand.Seconds * 1000;
                Application.DoEvents();
            }
            while (s < delayMilliseconds);
            return true;
        }

        internal bool GetAllAxisBusy()
        {
	        return false;
            //throw new NotImplementedException();
        }
    }

    public enum ServoAixsName
    {
        X = 0,
        Y = 1,
        Z = 2,
        R1 = 3,
        R2 = 4,
        R3 = 5,
        U = 6,
    }
}
