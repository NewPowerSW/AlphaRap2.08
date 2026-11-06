using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace NPSDK.Motion
{
	public enum MotorParaLanguage
	{
		English,
		Chinese
	}

	/// <summary>Axis parameter form. Supports save, CN/EN switch; prompts on close when dirty.</summary>
	public class MotorParaForm : Form
	{
		public static MotorParaLanguage CurrentLanguage = MotorParaLanguage.English;

		private readonly NP_Motor motor;
		private MotorParaData snapshot;
		private bool suppressDirtyPrompt;

		private ComboBox cmbLanguage;
		private Label lblWorkSpeed;
		private Label lblManualSpeed;
		private Label lblSpeedRatio;
		private Label lblTacc;
		private Label lblTdec;
		private Label lblMaxSpeed;
		private Label lblPulseEquivalent;
		private Label lblEncoderPulseEquivalent;
		private Label lblHomeMode;
		private Label lblHomeDir;
		private Label lblHomeHighSpeed;
		private Label lblHomeLowSpeed;
		private Label lblHomeOffset;
		private Label lblSavePath;
		private TextBox txtSavePath;
		private Label lblTitle;
		private Label lblMotorName;
		private Label lblSecMotion;
		private Label lblSecPulse;
		private Label lblSecHome;
		private Panel pnlFooter;

		private static readonly Color PageBg = Color.White;
		private static readonly Color FieldBg = Color.FromArgb(247, 248, 250);
		private static readonly Color Accent = Color.FromArgb(17, 24, 39);
		private static readonly Color TextMain = Color.FromArgb(17, 24, 39);
		private static readonly Color TextMuted = Color.FromArgb(107, 114, 128);
		private static readonly Color Line = Color.FromArgb(229, 231, 235);

		private const int FormWidth = 640;
		private const int Pad = 24;
		private const int ColWidth = 288;
		private const int ColGap = 16;
		private TextBox numWorkSpeed;
		private TextBox numManualSpeed;
		private TextBox numSpeedRatio;
		private TextBox numTacc;
		private TextBox numTdec;
		private TextBox numMaxSpeed;
		private TextBox numPulseEquivalent;
		private TextBox numEncoderPulseEquivalent;
		private TextBox numHomeMode;
		private CheckBox chkHomeDir;
		private TextBox numHomeHighSpeed;
		private TextBox numHomeLowSpeed;
		private TextBox numHomeOffset;
		private Button btnSave;
		private Button btnCancel;

		public MotorParaForm(NP_Motor workMotor)
		{
			if (workMotor == null)
			{
				throw new ArgumentNullException("workMotor");
			}
			motor = workMotor;
			BuildUi();
			ApplyLanguage();
			LoadFromMotor();
			snapshot = CaptureUi();
		}

		public static string GetParaDirectory()
		{
			return Path.Combine(Application.StartupPath, "MotorPara");
		}

		/// <summary>
		/// 按控件 Name 存文件（每个 Motor 独立一份）。
		/// 勿用 Port：多轴常同为 0，会互相覆盖。
		/// </summary>
		public static string GetParaFilePath(NP_Motor workMotor)
		{
			string key = GetMotorFileKey(workMotor);
			return Path.Combine(GetParaDirectory(), key + ".xml");
		}

		private static string GetMotorFileKey(NP_Motor workMotor)
		{
			if (workMotor == null)
			{
				return "Motor";
			}
			if (!string.IsNullOrWhiteSpace(workMotor.Name)
				&& !string.Equals(workMotor.Name, "ControlBaseInterface", StringComparison.OrdinalIgnoreCase)
				&& !string.Equals(workMotor.Name, "NP_Motor", StringComparison.OrdinalIgnoreCase))
			{
				return SanitizeFileName(workMotor.Name.Trim());
			}
			// Name 尚未就绪时用 Vendor+Port+哈希区分，避免多轴落到同一文件
			string port = string.IsNullOrWhiteSpace(workMotor.Port) ? "0" : workMotor.Port.Trim();
			return SanitizeFileName(workMotor.Vendor + "_" + port + "_" + workMotor.GetHashCode().ToString("X"));
		}

		private static string SanitizeFileName(string key)
		{
			foreach (char c in Path.GetInvalidFileNameChars())
			{
				key = key.Replace(c, '_');
			}
			return key;
		}

		/// <summary>仅读该控件自己的参数文件，不回退到按 Port 的共享文件。</summary>
		public static bool TryLoadFile(NP_Motor workMotor)
		{
			if (workMotor == null)
			{
				return false;
			}
			string path = GetParaFilePath(workMotor);
			if (!File.Exists(path))
			{
				return false;
			}
			try
			{
				MotorParaData data = Deserialize(path);
				ApplyDataToMotor(workMotor, data);
				return true;
			}
			catch
			{
				return false;
			}
		}

		private void BuildUi()
		{
			Font = new Font("Segoe UI", 9f);
			BackColor = PageBg;
			ForeColor = TextMain;
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;
			StartPosition = FormStartPosition.CenterParent;

			lblTitle = new Label
			{
				Location = new Point(Pad, 18),
				Size = new Size(360, 28),
				Font = new Font("Segoe UI", 16f, FontStyle.Bold),
				ForeColor = TextMain,
				TextAlign = ContentAlignment.MiddleLeft
			};
			lblMotorName = new Label
			{
				Location = new Point(Pad, 46),
				Size = new Size(360, 20),
				Font = new Font("Segoe UI", 9f),
				ForeColor = TextMuted,
				TextAlign = ContentAlignment.MiddleLeft
			};
			cmbLanguage = new ComboBox
			{
				Location = new Point(FormWidth - Pad - 120, 28),
				Size = new Size(120, 28),
				DropDownStyle = ComboBoxStyle.DropDownList,
				FlatStyle = FlatStyle.Flat,
				Font = Font
			};
			cmbLanguage.Items.Add("English");
			cmbLanguage.Items.Add("中文");
			cmbLanguage.SelectedIndex = CurrentLanguage == MotorParaLanguage.Chinese ? 1 : 0;
			cmbLanguage.SelectedIndexChanged += (s, e) =>
			{
				CurrentLanguage = cmbLanguage.SelectedIndex == 1
					? MotorParaLanguage.Chinese
					: MotorParaLanguage.English;
				ApplyLanguage();
			};
			Controls.Add(lblTitle);
			Controls.Add(lblMotorName);
			Controls.Add(cmbLanguage);

			int y = 84;
			lblSecMotion = AddSection(y);
			y += 28;
			lblWorkSpeed = AddCaption(Pad, y);
			numWorkSpeed = AddNumber(Pad, y + 20, 0, 100000, 3);
			lblManualSpeed = AddCaption(Pad + ColWidth + ColGap, y);
			numManualSpeed = AddNumber(Pad + ColWidth + ColGap, y + 20, 0, 100000, 3);
			y += 68;
			lblSpeedRatio = AddCaption(Pad, y);
			numSpeedRatio = AddNumber(Pad, y + 20, 0, 1000, 1);
			lblMaxSpeed = AddCaption(Pad + ColWidth + ColGap, y);
			numMaxSpeed = AddNumber(Pad + ColWidth + ColGap, y + 20, 0, 1000000, 3);
			y += 68;
			lblTacc = AddCaption(Pad, y);
			numTacc = AddNumber(Pad, y + 20, 0.001m, 100, 3);
			lblTdec = AddCaption(Pad + ColWidth + ColGap, y);
			numTdec = AddNumber(Pad + ColWidth + ColGap, y + 20, 0.001m, 100, 3);

			y += 78;
			lblSecPulse = AddSection(y);
			y += 28;
			lblPulseEquivalent = AddCaption(Pad, y);
			numPulseEquivalent = AddNumber(Pad, y + 20, 0.000001m, 1000000, 3);
			lblEncoderPulseEquivalent = AddCaption(Pad + ColWidth + ColGap, y);
			numEncoderPulseEquivalent = AddNumber(Pad + ColWidth + ColGap, y + 20, 0.000001m, 1000000, 3);

			y += 78;
			lblSecHome = AddSection(y);
			y += 28;
			lblHomeMode = AddCaption(Pad, y);
			numHomeMode = AddNumber(Pad, y + 20, 0, 1000, 0);
			lblHomeDir = AddCaption(Pad + ColWidth + ColGap, y);
			chkHomeDir = new CheckBox
			{
				Location = new Point(Pad + ColWidth + ColGap, y + 22),
				Size = new Size(ColWidth, 32),
				FlatStyle = FlatStyle.Standard,
				Font = new Font("Segoe UI", 10f),
				ForeColor = TextMain,
				TextAlign = ContentAlignment.MiddleLeft
			};
			Controls.Add(chkHomeDir);
			y += 68;
			lblHomeHighSpeed = AddCaption(Pad, y);
			numHomeHighSpeed = AddNumber(Pad, y + 20, 0, 100000, 3);
			lblHomeLowSpeed = AddCaption(Pad + ColWidth + ColGap, y);
			numHomeLowSpeed = AddNumber(Pad + ColWidth + ColGap, y + 20, 0, 100000, 3);
			y += 68;
			lblHomeOffset = AddCaption(Pad, y);
			numHomeOffset = AddNumber(Pad, y + 20, -1000000, 1000000, 3);

			y += 72;
			pnlFooter = new Panel
			{
				Location = new Point(0, y),
				Size = new Size(FormWidth, 72),
				BackColor = Color.FromArgb(249, 250, 251)
			};
			pnlFooter.Paint += (s, e) =>
			{
				using (var pen = new Pen(Line))
				{
					e.Graphics.DrawLine(pen, 0, 0, pnlFooter.Width, 0);
				}
			};
			lblSavePath = new Label
			{
				Location = new Point(Pad, 12),
				Size = new Size(280, 16),
				ForeColor = TextMuted,
				Font = new Font("Segoe UI", 8f)
			};
			txtSavePath = new TextBox
			{
				Location = new Point(Pad, 30),
				Size = new Size(300, 22),
				ReadOnly = true,
				BorderStyle = BorderStyle.None,
				BackColor = Color.FromArgb(249, 250, 251),
				ForeColor = TextMuted,
				Font = new Font("Segoe UI", 8f),
				Text = GetParaFilePath(motor)
			};
			btnCancel = MakeButton(false);
			btnSave = MakeButton(true);
			btnSave.Location = new Point(FormWidth - Pad - 88, 20);
			btnCancel.Location = new Point(btnSave.Left - 8 - 88, 20);
			btnSave.Click += (s, e) => Save(true);
			btnCancel.Click += (s, e) => Close();
			pnlFooter.Controls.Add(lblSavePath);
			pnlFooter.Controls.Add(txtSavePath);
			pnlFooter.Controls.Add(btnCancel);
			pnlFooter.Controls.Add(btnSave);
			Controls.Add(pnlFooter);

			AcceptButton = btnSave;
			CancelButton = btnCancel;
			ClientSize = new Size(FormWidth, pnlFooter.Bottom);
			FormClosing += OnFormClosing;
		}

		private Label AddSection(int y)
		{
			var label = new Label
			{
				Location = new Point(Pad, y),
				Size = new Size(FormWidth - Pad * 2, 20),
				Font = new Font("Segoe UI", 9f, FontStyle.Bold),
				ForeColor = TextMuted
			};
			Controls.Add(label);
			return label;
		}

		private Label AddCaption(int x, int y)
		{
			var label = new Label
			{
				Location = new Point(x, y),
				Size = new Size(ColWidth, 18),
				ForeColor = TextMain,
				Font = Font
			};
			Controls.Add(label);
			return label;
		}

		private TextBox AddNumber(int x, int y, decimal min, decimal max, int decimals)
		{
			var shell = new Panel
			{
				Location = new Point(x, y),
				Size = new Size(ColWidth, 34),
				BackColor = FieldBg
			};
			shell.Paint += (s, e) =>
			{
				using (var pen = new Pen(Line))
				{
					e.Graphics.DrawRectangle(pen, 0, 0, shell.Width - 1, shell.Height - 1);
				}
			};
			var box = new TextBox
			{
				BorderStyle = BorderStyle.None,
				BackColor = FieldBg,
				ForeColor = TextMain,
				Font = new Font("Segoe UI", 11f),
				Location = new Point(10, 7),
				Size = new Size(ColWidth - 20, 22),
				TextAlign = HorizontalAlignment.Left,
				Tag = new NumberSpec(min, max, decimals)
			};
			box.Leave += (s, e) => CommitNumber(box);
			box.KeyPress += Number_KeyPress;
			shell.Controls.Add(box);
			Controls.Add(shell);
			return box;
		}

		private static Button MakeButton(bool primary)
		{
			var button = new Button
			{
				Size = new Size(88, 32),
				FlatStyle = FlatStyle.Flat,
				Font = new Font("Segoe UI", 9f),
				Cursor = Cursors.Hand,
				UseVisualStyleBackColor = false
			};
			button.FlatAppearance.BorderSize = primary ? 0 : 1;
			if (primary)
			{
				button.BackColor = Accent;
				button.ForeColor = Color.White;
				button.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 65, 81);
				button.FlatAppearance.MouseDownBackColor = Color.Black;
			}
			else
			{
				button.BackColor = Color.White;
				button.ForeColor = TextMain;
				button.FlatAppearance.BorderColor = Line;
				button.FlatAppearance.MouseOverBackColor = FieldBg;
			}
			return button;
		}

		private void ApplyLanguage()
		{
			bool cn = CurrentLanguage == MotorParaLanguage.Chinese;
			string motorName = string.IsNullOrEmpty(motor.Text) ? motor.Name : motor.Text;
			Text = cn ? ("轴参数 - " + motorName) : ("Axis Parameters - " + motorName);
			lblTitle.Text = cn ? "轴参数" : "Axis Parameters";
			lblMotorName.Text = motorName;
			lblSecMotion.Text = cn ? "运动" : "Motion";
			lblSecPulse.Text = cn ? "脉冲当量" : "Pulse Equivalent";
			lblSecHome.Text = cn ? "回原" : "Homing";
			lblWorkSpeed.Text = cn ? "工作速度" : "Work Speed";
			lblManualSpeed.Text = cn ? "手动速度" : "Manual Speed";
			lblSpeedRatio.Text = cn ? "速度比例 %" : "Speed Ratio %";
			lblTacc.Text = cn ? "加速时间" : "Acceleration";
			lblTdec.Text = cn ? "减速时间" : "Deceleration";
			lblMaxSpeed.Text = cn ? "最大速度" : "Max Speed";
			lblPulseEquivalent.Text = cn ? "指令脉冲当量" : "Pulse Equivalent";
			lblEncoderPulseEquivalent.Text = cn ? "编码器脉冲当量" : "Encoder Equivalent";
			lblHomeMode.Text = cn ? "回原模式" : "Home Mode";
			lblHomeDir.Text = cn ? "回原方向" : "Home Direction";
			chkHomeDir.Text = cn ? "正方向" : "Positive";
			lblHomeHighSpeed.Text = cn ? "回原高速" : "Home High Speed";
			lblHomeLowSpeed.Text = cn ? "回原低速" : "Home Low Speed";
			lblHomeOffset.Text = cn ? "回原偏移" : "Home Offset";
			lblSavePath.Text = cn ? "保存路径" : "Save path";
			btnSave.Text = cn ? "保存" : "Save";
			btnCancel.Text = cn ? "取消" : "Cancel";
			if (cmbLanguage.SelectedIndex != (cn ? 1 : 0))
			{
				cmbLanguage.SelectedIndex = cn ? 1 : 0;
			}
		}

		private string T(string en, string cn)
		{
			return CurrentLanguage == MotorParaLanguage.Chinese ? cn : en;
		}

		private void LoadFromMotor()
		{
			SetNum(numWorkSpeed, motor.WorkSpeed);
			SetNum(numManualSpeed, motor.ManualSpeed);
			SetNum(numSpeedRatio, motor.SpeedRatio);
			SetNum(numTacc, motor.Tacc);
			SetNum(numTdec, motor.Tdec);
			SetNum(numMaxSpeed, motor.MaxSpeed);
			SetNum(numPulseEquivalent, motor.PulseEquivalent);
			SetNum(numEncoderPulseEquivalent, motor.EncoderPulseEquivalent);
			SetNum(numHomeMode, motor.HomeMode);
			chkHomeDir.Checked = motor.HomeDir;
			SetNum(numHomeHighSpeed, motor.HomeHighSpeed);
			SetNum(numHomeLowSpeed, motor.HomeLowSpeed);
			SetNum(numHomeOffset, motor.HomeOffset);
		}

		private static void SetNum(TextBox box, double value)
		{
			NumberSpec spec = (NumberSpec)box.Tag;
			decimal v = (decimal)value;
			if (v < spec.Min) v = spec.Min;
			if (v > spec.Max) v = spec.Max;
			box.Text = FormatNumber(v, spec.Decimals);
		}

		private static double ReadNum(TextBox box)
		{
			CommitNumber(box);
			decimal v;
			decimal.TryParse(box.Text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.CurrentCulture, out v);
			return (double)v;
		}

		private static void CommitNumber(TextBox box)
		{
			NumberSpec spec = (NumberSpec)box.Tag;
			decimal v;
			if (!decimal.TryParse(box.Text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.CurrentCulture, out v)
				&& !decimal.TryParse(box.Text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out v))
			{
				v = 0;
			}
			if (v < spec.Min) v = spec.Min;
			if (v > spec.Max) v = spec.Max;
			box.Text = FormatNumber(v, spec.Decimals);
		}

		private static string FormatNumber(decimal value, int decimals)
		{
			if (decimals <= 0)
			{
				return decimal.Truncate(value).ToString("0");
			}
			string text = value.ToString("0." + new string('#', decimals));
			return text;
		}

		private static void Number_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (char.IsControl(e.KeyChar))
			{
				return;
			}
			if (char.IsDigit(e.KeyChar) || e.KeyChar == '.' || e.KeyChar == '-')
			{
				return;
			}
			e.Handled = true;
		}

		private MotorParaData CaptureUi()
		{
			return new MotorParaData
			{
				WorkSpeed = ReadNum(numWorkSpeed),
				ManualSpeed = ReadNum(numManualSpeed),
				SpeedRatio = ReadNum(numSpeedRatio),
				Tacc = ReadNum(numTacc),
				Tdec = ReadNum(numTdec),
				MaxSpeed = ReadNum(numMaxSpeed),
				PulseEquivalent = ReadNum(numPulseEquivalent),
				EncoderPulseEquivalent = ReadNum(numEncoderPulseEquivalent),
				HomeMode = (int)ReadNum(numHomeMode),
				HomeDir = chkHomeDir.Checked,
				HomeHighSpeed = ReadNum(numHomeHighSpeed),
				HomeLowSpeed = ReadNum(numHomeLowSpeed),
				HomeOffset = ReadNum(numHomeOffset)
			};
		}

		private sealed class NumberSpec
		{
			public readonly decimal Min;
			public readonly decimal Max;
			public readonly int Decimals;

			public NumberSpec(decimal min, decimal max, int decimals)
			{
				Min = min;
				Max = max;
				Decimals = decimals;
			}
		}

		private bool IsDirty()
		{
			return !MotorParaData.Equals(snapshot, CaptureUi());
		}

		private bool Save(bool showMessage)
		{
			MotorParaData data = CaptureUi();
			ApplyDataToMotor(motor, data);
			try
			{
				string path = GetParaFilePath(motor);
				string dir = Path.GetDirectoryName(path);
				if (!Directory.Exists(dir))
				{
					Directory.CreateDirectory(dir);
				}
				Serialize(path, data);
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					this,
					T("Failed to save file: ", "保存文件失败：") + ex.Message,
					T("Axis Parameters", "轴参数"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return false;
			}
			snapshot = data;
			if (showMessage)
			{
				MessageBox.Show(
					this,
					T("Parameters saved.", "参数已保存。"),
					T("Axis Parameters", "轴参数"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
			return true;
		}

		private void OnFormClosing(object sender, FormClosingEventArgs e)
		{
			if (suppressDirtyPrompt || !IsDirty())
			{
				return;
			}
			DialogResult result = MessageBox.Show(
				this,
				T("Parameters have been changed. Save before exit?", "参数已修改，是否保存？"),
				T("Axis Parameters", "轴参数"),
				MessageBoxButtons.YesNoCancel,
				MessageBoxIcon.Question);
			if (result == DialogResult.Cancel)
			{
				e.Cancel = true;
				return;
			}
			if (result == DialogResult.Yes)
			{
				if (!Save(false))
				{
					e.Cancel = true;
					return;
				}
			}
			suppressDirtyPrompt = true;
		}

		private static void ApplyDataToMotor(NP_Motor target, MotorParaData data)
		{
			target.WorkSpeed = data.WorkSpeed;
			target.ManualSpeed = data.ManualSpeed;
			target.SpeedRatio = data.SpeedRatio;
			target.Tacc = data.Tacc;
			target.Tdec = data.Tdec;
			target.MaxSpeed = data.MaxSpeed;
			target.PulseEquivalent = data.PulseEquivalent;
			target.EncoderPulseEquivalent = data.EncoderPulseEquivalent;
			target.HomeMode = data.HomeMode;
			target.HomeDir = data.HomeDir;
			target.HomeHighSpeed = data.HomeHighSpeed;
			target.HomeLowSpeed = data.HomeLowSpeed;
			target.HomeOffset = data.HomeOffset;
		}

		private static void Serialize(string path, MotorParaData data)
		{
			using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
			{
				new XmlSerializer(typeof(MotorParaData)).Serialize(stream, data);
			}
		}

		private static MotorParaData Deserialize(string path)
		{
			using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				return (MotorParaData)new XmlSerializer(typeof(MotorParaData)).Deserialize(stream);
			}
		}
	}

	public class MotorParaData
	{
		public double WorkSpeed;
		public double ManualSpeed;
		public double SpeedRatio;
		public double Tacc;
		public double Tdec;
		public double MaxSpeed;
		public double PulseEquivalent = 1000;
		public double EncoderPulseEquivalent = 1000;
		public int HomeMode;
		public bool HomeDir;
		public double HomeHighSpeed;
		public double HomeLowSpeed;
		public double HomeOffset;

		public static bool Equals(MotorParaData a, MotorParaData b)
		{
			if (a == null || b == null)
			{
				return a == b;
			}
			return Nearly(a.WorkSpeed, b.WorkSpeed)
				&& Nearly(a.ManualSpeed, b.ManualSpeed)
				&& Nearly(a.SpeedRatio, b.SpeedRatio)
				&& Nearly(a.Tacc, b.Tacc)
				&& Nearly(a.Tdec, b.Tdec)
				&& Nearly(a.MaxSpeed, b.MaxSpeed)
				&& Nearly(a.PulseEquivalent, b.PulseEquivalent)
				&& Nearly(a.EncoderPulseEquivalent, b.EncoderPulseEquivalent)
				&& a.HomeMode == b.HomeMode
				&& a.HomeDir == b.HomeDir
				&& Nearly(a.HomeHighSpeed, b.HomeHighSpeed)
				&& Nearly(a.HomeLowSpeed, b.HomeLowSpeed)
				&& Nearly(a.HomeOffset, b.HomeOffset);
		}

		private static bool Nearly(double x, double y)
		{
			return Math.Abs(x - y) < 0.0000001;
		}
	}
}
