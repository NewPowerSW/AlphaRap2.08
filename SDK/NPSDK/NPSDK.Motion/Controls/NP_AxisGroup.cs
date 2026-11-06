using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NPSDK.Motion
{
    public partial class NP_AxisGroup : UserControl
    {
        private NP_Motor motor;
        private bool ownsMotor = true;
        private bool jogging, homing, moving, looping, forwardJog;
        private double target, runSpeed, loopStart, loopEnd;
        private int loopPhase;
        private readonly Stopwatch loopWait = new Stopwatch();
        private FlowResourceGate.Lease manualLease;
        private Func<string, string, string, string, string> translate;
        [Browsable(false)] public bool IsManualRunning { get { lock (motor) return jogging || homing || moving || looping || motor.GetBusy(); } }
        public event EventHandler ParametersRequested;
        [Browsable(false)] public Func<bool> CanOperate { get; set; }
        [Category("#NP"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public NP_Motor Motor { get { return motor; } }
        [Category("#NP"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public NP_PositionTable Points { get { return points; } }
        [Category("#NP"), DefaultValue(-10000.0)] public double LowLimit { get; set; } = -10000;
        [Category("#NP"), DefaultValue(10000.0)] public double HighLimit { get; set; } = 10000;
        [Category("#NP"), DefaultValue("Axis")] public string AxisCaption { get { return caption.Text; } set { caption.Text = value; } }
        private bool IsDesignHost { get { return LicenseManager.UsageMode == LicenseUsageMode.Designtime || DesignMode; } }

        /// <summary>创建集成轴调试模板，不连接驱动或执行运动。</summary>
        public NP_AxisGroup()
        {
            InitializeComponent();
            motor = new NP_Motor { Name = "Motor", Visible = false };
            motor.StopRequested += CancelManualMotion;
            Controls.Add(motor); points.WorkMotor = motor;
            caption.DoubleClick += (sender, args) => ShowParameters();
            DoubleClick += (sender, args) => ShowParameters();
            home.Click += BeginHome;
            stop.Click += (sender, args) => motor.Stop();
            servoOn.Click += (sender, args) => SetServo(true);
            servoOff.Click += (sender, args) => SetServo(false);
            loop.Click += BeginLoop;
            gotoFirst.Click += (sender, args) => MoveToManualPosition(loopFirst);
            gotoSecond.Click += (sender, args) => MoveToManualPosition(loopSecond);
            negative.MouseDown += (sender, args) => BeginJog(false, args);
            positive.MouseDown += (sender, args) => BeginJog(true, args);
            negative.MouseUp += EndJog; positive.MouseUp += EndJog;
            negative.MouseCaptureChanged += JogCaptureChanged; positive.MouseCaptureChanged += JogCaptureChanged;
            points.CanEdit = () => Allowed() && manualLease == null;
            points.Goto += MoveToSelectedPoint;
            refresh.Tick += RefreshAxis;
            VisibleChanged += VisibilityChanged;
            ApplyLanguage(null);
        }

        /// <summary>复用应用已有轴实例，保留外部对象所有权和自动运动参数。</summary>
        public void BindMotor(NP_Motor existing)
        {
            if (existing == null) throw new ArgumentNullException("existing");
            if (ReferenceEquals(existing, motor)) return;
            if (IsManualRunning) throw new InvalidOperationException(T("axisManualBusy", "请先停止当前轴。", "Stop the current axis first.", "Detenga primero el eje actual."));
            ReleaseManual();
            motor.StopRequested -= CancelManualMotion;
            if (ownsMotor) { Controls.Remove(motor); motor.Dispose(); }
            motor = existing; ownsMotor = false;
            motor.StopRequested += CancelManualMotion;
            points.WorkMotor = motor;
            UpdateSpeedLimit();
        }

        /// <summary>绑定当前配方点位，保留轴列键和外部 DataSet。</summary>
        public void BindPoints(DataSet data, DataTable table, string column)
        {
            if (data == null || table == null || !data.Tables.Contains(table.TableName) || !table.Columns.Contains(column)) throw new ArgumentException("Invalid axis point table.");
            points.DataSet = data; points.DataSet1 = table;
            points.ParameterSet.Clear(); points.ParameterSet.Add(new PositionColumn { Name = column, Axis = motor, Limit_Low = LowLimit, Limit_High = HighLimit });
            points.Position.DataSource = table;
        }

        /// <summary>由宿主统一提供轴配置编辑和主保存入口。</summary>
        public void ShowParameters()
        {
            if (IsDesignHost || !Allowed() || manualLease != null) return;
            if (ParametersRequested != null) ParametersRequested(this, EventArgs.Empty); else motor.ShowParaForm();
        }

        /// <summary>检查宿主权限及资源身份，当前手动会话允许访问自己的租约。</summary>
        private bool Allowed()
        {
            if (IsDesignHost || IsDisposed || (CanOperate != null && !CanOperate())) return false;
            return manualLease == null ? FlowResourceGate.CanAccess(motor) : manualLease.RunManual(() => FlowResourceGate.CanAccess(motor));
        }

        /// <summary>校验就绪、伺服、报警和急停，正负限位允许向脱离限位方向运动。</summary>
        private bool CanMove(double? destination)
        {
            if (!Allowed() || !motor.IsReady) return false;
            AxisIOState state = motor.GetAxisIOState();
            if (state.ALM || state.EMG || !state.SVON) return false;
            if (!destination.HasValue) return true;
            double value = destination.Value;
            if (double.IsNaN(value) || double.IsInfinity(value) || value < LowLimit || value > HighLimit) return false;
            double current = motor.GetEncPos();
            if (double.IsNaN(current) || double.IsInfinity(current)) return false;
            return !(value > current && (state.PEL || state.SLP)) && !(value < current && (state.MEL || state.SLN));
        }

        /// <summary>立即取得独占手动会话，抓取本次速度；失败时不排队发送指令。</summary>
        private bool AcquireManual(double? destination)
        {
            ReleaseIfIdle();
            UpdateSpeedLimit();
            if (manualLease != null || !CanMove(destination) || motor.GetBusy() || !manualSpeed.Enabled) return false;
            runSpeed = (double)manualSpeed.Value;
            if (!FlowResourceGate.TryAcquire(motor, out manualLease)) return false;
            if (CanMove(destination) && !motor.GetBusy()) { manualStatus.Text = ""; refresh.Start(); return true; }
            ReleaseManual();
            return false;
        }

        /// <summary>在独占会话中发出手动定位，命令期间不修改自动速度。</summary>
        private bool SendTarget()
        {
            return manualLease.RunManual(() => motor.ManualGoto(target, runSpeed));
        }

        /// <summary>建立受手动速度上限约束的回零会话。</summary>
        private void BeginHome(object sender, EventArgs args)
        {
            lock (motor)
            {
                if (!AcquireManual(null)) return;
                try { motor.HomeReset(); homing = true; }
                catch (Exception error) { FailManual(error); }
            }
        }

        /// <summary>任意停轴入口取消本页意图；减速中的资源仍保持占用。</summary>
        private void CancelManualMotion(object sender, EventArgs args)
        {
            homing = false; jogging = false; moving = false; looping = false;
            loopWait.Reset();
        }

        /// <summary>连续点动与定距移动使用同一个独立手动速度。</summary>
        private void BeginJog(bool forward, MouseEventArgs args)
        {
            if (args.Button != MouseButtons.Left) return;
            lock (motor)
            {
                double delta;
                if (!continuous.Checked && (!double.TryParse(distance.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out delta) || double.IsNaN(delta) || double.IsInfinity(delta) || delta <= 0)) return;
                delta = continuous.Checked ? 1 : double.Parse(distance.Text, CultureInfo.InvariantCulture);
                target = motor.GetEncPos() + (forward ? delta : -delta);
                if (!AcquireManual(target)) return;
                try
                {
                    if (!continuous.Checked) { moving = !SendTarget(); ReleaseIfIdle(); return; }
                    jogging = true; forwardJog = forward;
                    ((Control)(forward ? positive : negative)).Capture = true;
                    manualLease.RunManual(() => motor.ManualJog(forward, runSpeed));
                }
                catch (Exception error) { FailManual(error); }
            }
        }

        /// <summary>松开连续点动或失去鼠标捕获时停止；定距动作保持原有点击语义。</summary>
        private void EndJog(object sender, EventArgs args)
        {
            lock (motor) { if (jogging) motor.Stop(); }
        }

        /// <summary>获得捕获不改变动作，丢失捕获结束连续点动。</summary>
        private void JogCaptureChanged(object sender, EventArgs args)
        {
            if (((Control)sender).Capture) return;
            EndJog(sender, args);
        }

        /// <summary>按所选绑定行发出一次受限速保护的手动定位。</summary>
        private void MoveToSelectedPoint()
        {
            lock (motor)
            {
                if (points.Position.CurrentRow == null || points.ParameterSet.Count == 0) return;
                DataRowView row = points.Position.CurrentRow.DataBoundItem as DataRowView;
                if (row == null || row[points.ParameterSet[0].Name] == DBNull.Value) return;
                target = Convert.ToDouble(row[points.ParameterSet[0].Name], CultureInfo.InvariantCulture);
                if (!AcquireManual(target)) return;
                try { moving = !SendTarget(); ReleaseIfIdle(); }
                catch (Exception error) { FailManual(error); }
            }
        }

        /// <summary>伺服开关只在宿主允许且轴空闲时执行，不自动初始化硬件。</summary>
        private void SetServo(bool on)
        {
            lock (motor)
            {
                if (!Allowed() || !motor.IsReady || manualLease != null || motor.GetBusy()) return;
                AxisIOState state = motor.GetAxisIOState();
                if (on && (state.ALM || state.EMG)) return;
                FlowResourceGate.Lease lease;
                if (!FlowResourceGate.TryAcquire(motor, out lease)) return;
                try { lease.RunManual(() => { if (on) motor.ServoOn(); else motor.ServoOff(); return true; }); }
                catch (Exception error) { manualStatus.Text = error.Message; }
                finally { lease.Dispose(); }
            }
        }

        /// <summary>在输入的两个绝对位置间往返，位置和速度在开始时取得快照。</summary>
        private void BeginLoop(object sender, EventArgs args)
        {
            lock (motor)
            {
                if (looping) { motor.Stop(); return; }
                loopStart = (double)loopFirst.Value; loopEnd = (double)loopSecond.Value;
                if (!ValidateManualPosition(loopStart) || !ValidateManualPosition(loopEnd)) return;
                if (loopStart == loopEnd)
                {
                    manualStatus.Text = T("axisLoopSamePosition", "位置 A 与位置 B 不能相同。", "Positions A and B must differ.", "Las posiciones A y B deben ser distintas.");
                    return;
                }
                if (!CanMove(loopEnd) || !AcquireManual(loopStart)) return;
                looping = true; loopPhase = 0; target = loopStart;
                loopWait.Reset();
            }
        }

        /// <summary>校验手动输入的绝对坐标，超出软限位时不发送指令。</summary>
        private bool ValidateManualPosition(double value)
        {
            if (!double.IsNaN(value) && !double.IsInfinity(value) && value >= LowLimit && value <= HighLimit) return true;
            manualStatus.Text = T("axisManualOutOfRange", "目标位置超出轴的软限位。", "Target position is outside the axis soft limits.", "La posición excede los límites lógicos del eje.");
            return false;
        }

        /// <summary>按位置 A 或位置 B 单独定位，沿用手动速度和独占资源保护。</summary>
        private void MoveToManualPosition(NumericUpDown input)
        {
            lock (motor)
            {
                double value = (double)input.Value;
                if (!ValidateManualPosition(value)) return;
                if (!AcquireManual(value)) return;
                target = value;
                try { moving = !SendTarget(); ReleaseIfIdle(); }
                catch (Exception error) { FailManual(error); }
            }
        }

        /// <summary>往返按完成反馈和延时推进，禁止在报警、权限变化或限位后继续发指令。</summary>
        private void TickLoop()
        {
            if ((loopPhase & 1) == 0)
            {
                if (!CanMove(target)) { motor.Stop(); return; }
                if (!SendTarget()) return;
                loopWait.Restart(); loopPhase++;
                return;
            }
            if (loopWait.ElapsedMilliseconds < (double)loopDelay.Value) return;
            loopPhase = loopPhase == 1 ? 2 : 0;
            target = loopPhase == 2 ? loopEnd : loopStart;
        }

        /// <summary>动作结束且驱动实际停止后，允许自动流程重新取得资源。</summary>
        private void ReleaseIfIdle()
        {
            if (manualLease == null || homing || jogging || moving || looping || motor.GetBusy()) return;
            ReleaseManual();
        }

        /// <summary>释放本页手动租约且不影响已取消的其他资源。</summary>
        private void ReleaseManual()
        {
            FlowResourceGate.Lease lease = manualLease; manualLease = null;
            if (lease != null) lease.Dispose();
        }

        /// <summary>异常停止当前手动会话，保留原始错误供现场排查。</summary>
        private void FailManual(Exception error)
        {
            try { motor.Stop(); }
            catch (Exception stopError) { error = new AggregateException(error, stopError); }
            manualStatus.Text = error.Message;
        }

        /// <summary>离开轴页结束手动动作，隐藏页仍轮询减速反馈以安全释放租约。</summary>
        private void VisibilityChanged(object sender, EventArgs args)
        {
            if (IsDesignHost) return;
            if (!Visible && manualLease != null) motor.Stop();
            refresh.Enabled = Visible || manualLease != null;
        }

        /// <summary>轮询状态及当前手动动作，不在语言刷新或界面创建时发出运动。</summary>
        private void RefreshAxis(object sender, EventArgs args)
        {
            if (IsDesignHost || IsDisposed) return;
            lock (motor)
            {
                try { TickManual(); UpdateStatus(); UpdateSpeedLimit(); UpdateActions(); ReleaseIfIdle(); }
                catch (Exception error) { FailManual(error); }
            }
        }

        /// <summary>先复核联锁，再推进回零、定位及往返动作。</summary>
        private void TickManual()
        {
            if (manualLease == null) return;
            if (!CanMove(null) || runSpeed > Math.Min(motor.ManualSpeed, motor.MaxSpeed)) { motor.Stop(); return; }
            if (jogging && !CanMove(motor.GetEncPos() + (forwardJog ? 1 : -1))) { motor.Stop(); return; }
            if (homing && manualLease.RunManual(() => motor.ManualHome(runSpeed))) homing = false;
            if (moving && !CanMove(target)) { motor.Stop(); return; }
            if (moving && SendTarget()) moving = false;
            if (looping) TickLoop();
        }

        /// <summary>用灰色表示未就绪状态，红色表示保护信号，绿色表示伺服及运动信号。</summary>
        private void UpdateStatus()
        {
            bool ready = motor.IsReady;
            AxisIOState io = ready ? motor.GetAxisIOState() : default(AxisIOState);
            MotionState motion = ready ? motor.GetMotionState() : default(MotionState);
            SetIndicator(alm, io.ALM, true, ready); SetIndicator(emg, io.EMG, true, ready);
            SetIndicator(svon, io.SVON, false, ready); SetIndicator(smv, motion.SMV, false, ready); SetIndicator(hmv, motion.HMV, false, ready);
            SetIndicator(org, io.ORG, false, ready); SetIndicator(mel, io.MEL, true, ready); SetIndicator(pel, io.PEL, true, ready);
            SetIndicator(slp, io.SLP, true, ready); SetIndicator(sln, io.SLN, true, ready);
            command.Text = ready ? motor.GetPos().ToString("F3", CultureInfo.InvariantCulture) : "—";
            encoder.Text = ready ? motor.GetEncPos().ToString("F3", CultureInfo.InvariantCulture) : "—";
        }

        /// <summary>更新单个固定状态指示，不使用颜色伪装未连接设备为正常。</summary>
        private static void SetIndicator(Label label, bool on, bool warning, bool ready)
        {
            label.Text = (ready && on ? "●  " : "○  ") + (string)label.Tag;
            label.ForeColor = ready && on ? (warning ? System.Drawing.Color.FromArgb(185,28,28) : System.Drawing.Color.FromArgb(21,128,61)) : System.Drawing.Color.FromArgb(100,116,139);
        }

        /// <summary>手动速度限定为当前轴配置的 ManualSpeed 与 MaxSpeed 中较小值。</summary>
        private void UpdateSpeedLimit()
        {
            double limit = Math.Min(motor.ManualSpeed, motor.MaxSpeed);
            bool valid = !double.IsNaN(limit) && !double.IsInfinity(limit) && limit >= 0.001 && limit <= (double)decimal.MaxValue;
            decimal maximum = valid ? (decimal)limit : 0.001m;
            if (manualSpeed.Value > maximum) manualSpeed.Value = maximum;
            manualSpeed.Maximum = maximum;
            manualSpeed.Enabled = valid && manualLease == null;
            speedLimit.Text = string.Format(T("axisManualLimit", "上限 {0} mm/s · 自动速度独立", "Limit {0} mm/s · Auto speed independent", "Límite {0} mm/s · Automático independiente"), valid ? limit.ToString("0.###", CultureInfo.InvariantCulture) : "—");
        }

        /// <summary>按联锁和手动会话启用输入，往返坐标与配方点位行数无关。</summary>
        private void UpdateActions()
        {
            bool editable = Allowed() && manualLease == null && !motor.GetBusy();
            bool idle = editable && motor.IsReady;
            bool enabled = idle && CanMove(null);
            home.Enabled = enabled; negative.Enabled = enabled; positive.Enabled = enabled;
            servoOn.Enabled = idle; servoOff.Enabled = idle;
            points.Enabled = Allowed() && manualLease == null;
            loopFirst.Enabled = editable; loopSecond.Enabled = editable; loopDelay.Enabled = editable;
            gotoFirst.Enabled = enabled; gotoSecond.Enabled = enabled;
            loop.Enabled = looping || enabled;
            loop.BackColor = loop.Enabled ? System.Drawing.Color.FromArgb(37,99,180) : System.Drawing.Color.FromArgb(226,232,240);
            loop.ForeColor = loop.Enabled ? System.Drawing.Color.White : System.Drawing.Color.FromArgb(100,116,139);
            loop.Text = looping ? T("axisLoopStop", "停止往返", "Stop loop", "Parar ciclo") : T("axisLoopStart", "启动往返 / LOOP", "Start / LOOP", "Iniciar / LOOP");
            distance.Enabled = editable && !continuous.Checked; continuous.Enabled = editable;
        }

        /// <summary>释放页面时停止本页动作；等待实际停稳后才释放独占资源。</summary>
        private void DisposeManual()
        {
            if (motor == null) return;
            if (manualLease != null) motor.Stop();
            FlowResourceGate.Lease lease = manualLease; manualLease = null;
            NP_Motor current = motor;
            if (lease != null) Task.Run(async () =>
            {
                while (!current.IsDisposed && current.IsReady && current.GetBusy()) await Task.Delay(50).ConfigureAwait(false);
                lease.Dispose();
            });
            motor.StopRequested -= CancelManualMotion;
        }

        /// <summary>读取宿主三语底稿，独立设计预览默认使用中文。</summary>
        private string T(string key, string zh, string en, string es) { return translate == null ? zh : translate(key, zh, en, es); }

        /// <summary>仅刷新固定文字和提示，不重建配方行或改变正在编辑的数值。</summary>
        public void ApplyLanguage(Func<string, string, string, string, string> text)
        {
            translate = text;
            commandLabel.Text = T("axisCommand", "指令位置", "Command", "Posición de mando");
            encoderLabel.Text = T("axisEncoder", "编码器位置", "Encoder", "Codificador");
            home.Text = T("axisHome", "回零", "Home", "Origen"); stop.Text = T("axisStop", "停止", "Stop", "Detener");
            servoOn.Text = T("axisServoOn", "伺服开", "Servo on", "Activar"); servoOff.Text = T("axisServoOff", "伺服关", "Servo off", "Desactivar");
            continuous.Text = T("axisContinuous", "连续", "Continuous", "Continuo"); distanceLabel.Text = T("axisDistance", "距离", "Distance", "Distancia");
            speedLabel.Text = T("axisManualSpeed", "手动速度", "Manual speed", "Velocidad manual");
            loopFirstLabel.Text = T("axisManualPositionA", "位置 A", "Position A", "Posición A"); loopSecondLabel.Text = T("axisManualPositionB", "位置 B", "Position B", "Posición B");
            gotoFirst.Text = T("axisManualGoto", "前往", "Go to", "Ir a"); gotoSecond.Text = gotoFirst.Text;
            manualTitle.Text = T("axisManualTitle", "手动控制", "Manual control", "Control manual");
            loopTitle.Text = T("axisLoopTitle", "往返测试 / LOOP", "Reciprocation / LOOP", "Vaivén / LOOP");
            pointTitle.Text = T("axisPointsTitle", "配方点位", "Recipe points", "Puntos de receta");
            loopDelayLabel.Text = T("axisLoopDelay", "延时 ms", "Delay ms", "Pausa ms");
            loop.Text = looping ? T("axisLoopStop", "停止往返", "Stop loop", "Parar ciclo") : T("axisLoopStart", "启动往返 / LOOP", "Start / LOOP", "Iniciar / LOOP");
            SetStatusHints();
            if (motor != null) UpdateSpeedLimit();
            NP_GroupText.ApplyPoints(points, text ?? ((key, zh, en, es) => zh));
        }

        /// <summary>状态代码保持稳定，悬停提示按宿主语言解释信号含义。</summary>
        private void SetStatusHints()
        {
            statusTips.SetToolTip(alm, T("axisALM", "报警", "Alarm", "Alarma"));
            statusTips.SetToolTip(emg, T("axisEMG", "急停", "Emergency stop", "Parada de emergencia"));
            statusTips.SetToolTip(svon, T("axisSVON", "伺服已使能", "Servo enabled", "Servo activado"));
            statusTips.SetToolTip(smv, T("axisSMV", "正在运动", "Moving", "En movimiento"));
            statusTips.SetToolTip(hmv, T("axisHMV", "正在回零", "Homing", "Buscando origen"));
            statusTips.SetToolTip(org, T("axisORG", "原点信号", "Origin sensor", "Sensor de origen"));
            statusTips.SetToolTip(mel, T("axisMEL", "负硬限位", "Negative hardware limit", "Límite físico negativo"));
            statusTips.SetToolTip(pel, T("axisPEL", "正硬限位", "Positive hardware limit", "Límite físico positivo"));
            statusTips.SetToolTip(slp, T("axisSLP", "正软限位", "Positive software limit", "Límite lógico positivo"));
            statusTips.SetToolTip(sln, T("axisSLN", "负软限位", "Negative software limit", "Límite lógico negativo"));
        }
    }
}
