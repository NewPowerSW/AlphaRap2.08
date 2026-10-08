using System;
using System.Drawing;
using System.Windows.Forms;
using AlphaRapLibrary;
using NPClient;
using System.Threading;
using System.IO.Ports;
using NP_PressureSensor;
using AlphaRap.Classes;
using System.Net.NetworkInformation;
using AlphaRap.PLC;

namespace AlphaRap
{
	public partial class Parameter : ModuleBaseForm
	{
		Keyence3DTcp keyence3DTcp = new Keyence3DTcp("keyence3DTcp");
		public INOVANCE iNOVANCE = new INOVANCE();
		PressureSensor Pressure = new PressureSensor();
		public TCPCLient OPTScann = new TCPCLient();
		public Parameter()
		{
			InitializeComponent();
		}

		private void trackBar1_ValueChanged(object sender, EventArgs e)
		{
			try
			{
				int a = RobotSpeed.Value;
				txtRobotSpeed.Text = a.ToString();
			}
			catch (Exception)
			{
				txtRobotSpeed.Text = RobotSpeed.Value.ToString();
			}
		}

		private void txtRobotSpeed_KeyPress(object sender, KeyPressEventArgs e)
		{
			if ((e.KeyChar <= 47 || e.KeyChar >= 58) && (e.KeyChar != 8) && (e.KeyChar != 46))
				e.Handled = true;
		}

		private void txtRobotSpeed_TextChanged(object sender, EventArgs e)
		{
			SysPara.items++;
		}

		private void DataChangeBar_Click(object sender, MouseEventArgs e)
		{
			SysPara.items++;
		}

		/// <summary>
		/// 页面离开时提示用户对操作的数据是否保存
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Parameter_Leave(object sender, EventArgs e)
		{
			if (SysPara.items > 1)
			{
				MiddleLayer.MainF.SaveData();
				SysPara.items = 1;
			}
		}
		private void DataChange_Click(object sender, EventArgs e)
		{
			SysPara.items++;
		}

#region 变量定义

#endregion
#region 恢复数据
#endregion

		private void txtRobotSpeed_Leave(object sender, EventArgs e)
		{
			if (txtRobotSpeed.Text != "")
			{
				int aa = Convert.ToInt32(txtRobotSpeed.Text);
				if (aa <= 100 && aa >= 1)
				{
					RobotSpeed.Value = Convert.ToInt32(txtRobotSpeed.Text);
				}
				else
				{
					txtRobotSpeed.Text = "100";
					RobotSpeed.Value = 100;
				}
			}
			else
			{
				txtRobotSpeed.Text = "50";
				RobotSpeed.Value = 50;
			}
		}

		private void Client_Click(object sender, EventArgs e)
		{
			string IP = MiddleLayer.ParF.GetSettingValue("MSet", "ScannIP");
			int Port = MiddleLayer.ParF.GetSettingValue("MSet", "ScannPort");
			OPTScann.Connect(IP, Port);
		}

		//public bool bConnectSP()
		//{
		//	bool r1 = serialPort1.IsOpen;
		//	if (r1)
		//	{
		//		serialPort1.Close();

		//	}
		//	serialPort1.Open();
		//	return false;
		//}

		//public string sPressureData(string sComment)   // :004RDGROSS=   压力传感器命令
		//{
		//	bool r1 = serialPort1.IsOpen;
		//	if (!r1)
		//	{
		//		serialPort1.Open();

		//	}

		//	string sData = "";
		//	string sBuffer = sComment + Environment.NewLine;
		//	serialPort1.Write(sBuffer);
		//	Thread.Sleep(50);
		//	sData = serialPort1.ReadExisting().Replace("\r\n", "");

		//	return sData.TrimStart().Replace("\r\n", "");
		//}

		//private void button3_Click(object sender, EventArgs e)
		//{
		//	bConnectSP();
		//}

		//private void button4_Click(object sender, EventArgs e)
		//{
		//	serialPort1.Close();
		//}

		private void button5_Click(object sender, EventArgs e)
		{
			keyence3DTcp.IP = MiddleLayer.ParF.GetSettingValue("PSet", "3D_IP");
			keyence3DTcp.Port = MiddleLayer.ParF.GetSettingValue("PSet", "3D_Port");
			keyence3DTcp.Open();
		}

		private void button11_Click(object sender, EventArgs e)
		{
			string COM = MiddleLayer.ParF.GetSettingValue("MSet", "Pressure_COM");
			if (Pressure.OpenPort(COM, 9600, Parity.None, 8, StopBits.One))
			{
				button110.BackColor = Color.Green;

				button9.Enabled = true;
			}
			else
			{
				button10.BackColor = Color.Red;
				button9.Enabled = false;
			}
		}
		private void button9_Click(object sender, EventArgs e)
		{
			Pressure.SendSerialPortData("01 03 00 50 00 02 C4 1A");
			Thread.Sleep(50);
			textBox6.Text = Pressure.DataReceiveFunction();
		}

		private void button10_Click(object sender, EventArgs e)
		{
			if (Pressure.ClosePort())
			{
				button110.BackColor = Color.Red;
				button10.BackColor = Color.Green;
				button9.Enabled = false;
			}
		}

		public FFUCom ffu = new FFUCom();
		public bool ConnectFFU(string com, string para)
		{
			if (ffu.Connect(com, para))
			{
				return true;
			}
			else { return false; }
		}

		public static void RefreshDifferentThreadUI(Control control, Action action)
		{
			if (control.InvokeRequired)
			{
				Action refreshUI = new Action(action);
				control.Invoke(refreshUI);
			}
			else
			{
				action.Invoke();
			}
		}
		private static readonly object PingTCPLock = new object();
		/// <summary>设备在线检测的 Ping 超时（毫秒）。</summary>
		private const int PingTimeoutMs = 1000;
		public bool PingTCP(string IP)
		{
			lock (PingTCPLock)
			{
				try
				{
					// 未配置 IP 时直接判为离线，不再抛异常刷错误日志
					if (string.IsNullOrWhiteSpace(IP))
						return false;

					// Ping 实现了 IDisposable，原来每次 new 不释放；
					// 超时从默认 5 秒缩短到 1 秒：局域网设备 1 秒不回就是离线，状态灯能更快反映断线。
					using (Ping ping = new Ping())
					{
						PingReply reply = ping.Send(IP.Trim(), PingTimeoutMs);
						return reply.Status == IPStatus.Success;
					}
				}
				catch(Exception ex)
				{
					MiddleLayer.DataF.AddLogError(ex.ToString());
					return false;
				}
			}
		}
		private void btServer_Click(object sender, EventArgs e)
		{
			if (!OPTScann.ConnectStatus())
				return;
			OPTScann.Sent("start");
			Thread.Sleep(300);
			tbMassage.Text = OPTScann.Receive();
		}

		private void button6_Click(object sender, EventArgs e)
		{
			keyence3DTcp.Triger3D(5);
		}

		#region INOVANCEPLC
		private void btnConnect_Click(object sender, EventArgs e)
		{
			string PLCIP = MiddleLayer.ParF.GetSettingValue("MSet", "PLCIP");
			int nIpPort = MiddleLayer.ParF.GetSettingValue("MSet", "PLCPort"); ;
			bool result = INOVANCE.Init_ETH_String(PLCIP, 0, nIpPort);

			if (!result )
			{
				MessageBox.Show(MiddleLayer.LangMsg("Parameter", "msg_PlcConnectFail", "连接失败", "Connection failed", "Error de conexión"));
			}
		}

		private void button12_Click(object sender, EventArgs e)
		{
			string outValue = string.Empty;
			string Error = string.Empty;
			textBox5.Text = iNOVANCE.ReadPlc(textBox3.Text, 2);
		}

		private void button11_Click_1(object sender, EventArgs e)
		{
			string Error = string.Empty;
			iNOVANCE.WritePlc(textBox3.Text, textBox5.Text);
		}

#endregion
    }
}
