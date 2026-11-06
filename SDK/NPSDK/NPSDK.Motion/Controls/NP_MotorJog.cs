using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NPSDK;

namespace NPSDK.Motion
{
	/// <summary>
	/// 点动按钮。Direction 切换方向图标；WorkMotor 绑定轴。
	/// </summary>
	public class NP_MotorJog : Button
	{
		public enum DirectionType
		{
			None,
			Forward,
			Reverse,
			Left,
			Right,
			Up,
			Down,
			Clockwise,
			Counterclockwise
		}

		public enum MoveModeType
		{
			Jog,
			Relative
		}

		public enum JogDirectionType
		{
			JogP,
			JogN
		}

		public enum RelativeDirectionType
		{
			Positive,
			Negative
		}

		private DirectionType direction = DirectionType.Up;

		[Browsable(true)]
		[Category("#NP")]
		[Description("按钮显示方向图标")]
		[DefaultValue(DirectionType.Up)]
		public DirectionType Direction
		{
			get { return direction; }
			set
			{
				direction = value;
				ApplyDirectionImage();
			}
		}

		[Browsable(true)]
		[Category("#NP")]
		public NP_Motor WorkMotor { get; set; }

		[Browsable(true)]
		[Category("#NP")]
		[Description("点动选项。设置后按 Continus / Distance 运动；未设置按钮自己的轴时使用选项上的轴")]
		public NP_JogOption JogOption { get; set; }

		[Browsable(true)]
		[Category("#NP")]
		[DefaultValue(JogDirectionType.JogP)]
		public JogDirectionType JogDirection { get; set; } = JogDirectionType.JogP;

		[Browsable(true)]
		[Category("#NP")]
		[DefaultValue(RelativeDirectionType.Positive)]
		public RelativeDirectionType RelativeDirection { get; set; } = RelativeDirectionType.Positive;

		[Browsable(true)]
		[Category("#NP")]
		[DefaultValue(MoveModeType.Jog)]
		public MoveModeType MoveMode { get; set; } = MoveModeType.Jog;

		[Browsable(true)]
		[Category("#NP")]
		public double RelativeDistance { get; set; }

		[Browsable(true)]
		[Category("#NP")]
		[DefaultValue(100.0)]
		public double SpeedRatio { get; set; } = 100;

		public NP_MotorJog()
		{
			SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
			UpdateStyles();

			Size = new Size(60, 60);
			FlatStyle = FlatStyle.Flat;
			FlatAppearance.BorderSize = 0;
			FlatAppearance.MouseOverBackColor = Color.Transparent;
			FlatAppearance.MouseDownBackColor = Color.Transparent;
			BackColor = Color.Transparent;
			BackgroundImageLayout = ImageLayout.Zoom;
			Cursor = Cursors.Hand;
			Text = string.Empty;
			TextImageRelation = TextImageRelation.Overlay;

			MouseDown += OnMouseDown;
			MouseUp += OnMouseUp;
			TextChanged += OnTextChangedClear;
			HandleCreated += (s, e) => ApplyDirectionImage();

			ApplyDirectionImage();
		}

		private void OnTextChangedClear(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(Text))
			{
				Text = string.Empty;
			}
		}

		private void ApplyDirectionImage()
		{
			Image image = GetDirectionImage(direction);
			if (!object.ReferenceEquals(BackgroundImage, image))
			{
				BackgroundImage = image;
			}
			if (!string.IsNullOrEmpty(Text))
			{
				Text = string.Empty;
			}
			Invalidate();
		}

		private static Image GetDirectionImage(DirectionType value)
		{
			switch (value)
			{
				case DirectionType.Forward:
					return Resource.Forward;
				case DirectionType.Reverse:
					return Resource.Reverse;
				case DirectionType.Left:
					return Resource.Left;
				case DirectionType.Right:
					return Resource.Right;
				case DirectionType.Up:
					return Resource.Up;
				case DirectionType.Down:
					return Resource.Down;
				case DirectionType.Clockwise:
					return Resource.Clockwise;
				case DirectionType.Counterclockwise:
					return Resource.CounterClockWise;
				default:
					return Resource.Up;
			}
		}

		private NP_Motor ResolveMotor()
		{
			if (WorkMotor != null)
			{
				return WorkMotor;
			}
			if (JogOption != null)
			{
				return JogOption.WorkMotor;
			}
			return null;
		}

		private void OnMouseDown(object sender, MouseEventArgs e)
		{
			NP_Motor motor = ResolveMotor();
			if (motor == null || motor.GetBusy())
			{
				return;
			}
			if (JogOption != null)
			{
				motor.WorkSpeed = motor.ManualSpeed;
				motor.SpeedRatio = SpeedRatio;
				JogOption.BeginMove(motor, JogDirection == JogDirectionType.JogP);
				return;
			}
			if (MoveMode != MoveModeType.Jog)
			{
				return;
			}
			motor.WorkSpeed = motor.ManualSpeed;
			motor.SpeedRatio = SpeedRatio;
			if (JogDirection == JogDirectionType.JogP)
			{
				motor.JogP();
			}
			else
			{
				motor.JogN();
			}
		}

		private void OnMouseUp(object sender, MouseEventArgs e)
		{
			NP_Motor motor = ResolveMotor();
			if (motor == null)
			{
				return;
			}
			if (JogOption != null)
			{
				JogOption.EndMove(motor);
				return;
			}
			if (MoveMode == MoveModeType.Jog)
			{
				motor.Stop();
				return;
			}
			if (motor.GetBusy())
			{
				return;
			}
			double position = motor.GetPos();
			position = RelativeDirection == RelativeDirectionType.Positive
				? position + RelativeDistance
				: position - RelativeDistance;
			motor.WorkSpeed = motor.ManualSpeed;
			motor.SpeedRatio = SpeedRatio;
			motor.Goto(position);
		}

		public void Stop()
		{
			if (WorkMotor != null)
			{
				WorkMotor.Stop();
			}
		}

		protected override void OnPaint(PaintEventArgs pevent)
		{
			if (BackgroundImage != null)
			{
				pevent.Graphics.Clear(Parent != null ? Parent.BackColor : BackColor);
				Rectangle rect = ClientRectangle;
				pevent.Graphics.DrawImage(BackgroundImage, rect);
				return;
			}
			base.OnPaint(pevent);
		}
	}
}
