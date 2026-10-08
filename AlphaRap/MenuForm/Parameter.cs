using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlphaRapLibrary;
using System.Net.Sockets;
using System.Net.Http;
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

		ScannerKeyenceTcp scannerKeyenceTcp = new ScannerKeyenceTcp("scannerKeyenceTcp");
		Keyence3DTcp keyence3DTcp = new Keyence3DTcp("keyence3DTcp");
		public INOVANCE iNOVANCE = new INOVANCE();
		PressureSensor Pressure = new PressureSensor();
		public TCPCLient OPTScann = new TCPCLient();
		public Parameter()
		{
			InitializeComponent();
		}
		private void txtRobotAcceleration_KeyPress(object sender, KeyPressEventArgs e)
		{
			if ((e.KeyChar <= 47 || e.KeyChar >= 58) && (e.KeyChar != 8) && (e.KeyChar != 46))
				e.Handled = true;
		}



		private void txtRobotDeceleration_KeyPress(object sender, KeyPressEventArgs e)
		{
			if ((e.KeyChar <= 47 || e.KeyChar >= 58) && (e.KeyChar != 8) && (e.KeyChar != 46))
				e.Handled = true;
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

		// RFID Manual 
		//引用  MiddleLayer.HomeF.RFIDOper 进行操作
		#region 变量定义
		private readonly string[] HeadIO = { RFIDHead1, RFIDHead2 };
		private const string RFIDHead1 = "01";
		private const string RFIDHead2 = "02";
		private int RFIDReadType = 1;  //RFID 读种类

		private List<RFIDRecord> rFIDRecordsWrite = new List<RFIDRecord>();
		private List<RFIDRecord> rFIDRecordsRead = new List<RFIDRecord>();
		private List<RFIDRecord> rFIDRecordsClearWrite = new List<RFIDRecord>();

		//private RFIDUIDRecords UidRecords = new RFIDUIDRecords();      //用于存初始数据

		//读显示
		int ishow = 0;
		private string strRFIDUID = "";
		private string strRFIDReadPallet = "";
		private string strRFIDReadFlwCnt = "";
		private string strRFIDReadLastFlg = "";
		private string strRFIDReadWkPlaceNo = "";
		private string strRFIDRead = "";
		private string strRFIDWriteStnStatus = "";
		#endregion


		#region 恢复数据
		private void RFIDClearReords()
		{
			RFIDRecord record = new RFIDRecord();
			string clearData = " ";
			rFIDRecordsClearWrite.Clear();

			//添加初始化数据Work Place No.
			record.dataName = MiddleLayer.HomeF.RFIDOper.RFIDDataName[4];
			record.data = clearData;
			rFIDRecordsClearWrite.Add(record);

			//添加初始化数据 Processs Type
			record.dataName = MiddleLayer.HomeF.RFIDOper.RFIDDataName[5];
			record.data = clearData;
			rFIDRecordsClearWrite.Add(record);

			//POne Stsatus
			record.dataName = MiddleLayer.HomeF.RFIDOper.RFIDDataName[6];
			record.data = clearData;
			rFIDRecordsClearWrite.Add(record);

			//PTwo Stsatus
			record.dataName = MiddleLayer.HomeF.RFIDOper.RFIDDataName[20];
			record.data = clearData;
			rFIDRecordsClearWrite.Add(record);

			//Work Place Mark
			record.dataName = MiddleLayer.HomeF.RFIDOper.RFIDDataName[7];
			record.data = clearData;
			rFIDRecordsClearWrite.Add(record);

			//Flow Count
			record.dataName = MiddleLayer.HomeF.RFIDOper.RFIDDataName[1];
			record.data = MiddleLayer.HomeF.RFIDOper.FlwCnt_Last;
			rFIDRecordsClearWrite.Add(record);

			//POne NG Flag
			record.dataName = MiddleLayer.HomeF.RFIDOper.RFIDDataName[3];
			record.data = clearData;
			rFIDRecordsClearWrite.Add(record);

			//PTwo NG Flag
			record.dataName = MiddleLayer.HomeF.RFIDOper.RFIDDataName[21];
			record.data = clearData;
			rFIDRecordsClearWrite.Add(record);

			//POne Cavity
			record.dataName = MiddleLayer.HomeF.RFIDOper.RFIDDataName[18];
			record.data = MiddleLayer.HomeF.RFIDOper.NGFlag_OK;
			rFIDRecordsClearWrite.Add(record);
			//PTwo Cavity
			record.dataName = MiddleLayer.HomeF.RFIDOper.RFIDDataName[19];
			record.data = MiddleLayer.HomeF.RFIDOper.NGFlag_OK;
			rFIDRecordsClearWrite.Add(record);
		}
		#endregion



		private void BypassRFID_CheckedChanged(object sender, EventArgs e)
		{
			SysPara.items++;
		}

		private void BypassFlwCtl_CheckedChanged(object sender, EventArgs e)
		{
			SysPara.items++;
		}

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




		private void button20_Click(object sender, EventArgs e)
		{
			ffu.OpenFFUMaxSpeed();

		}

		private void button4_Click_1(object sender, EventArgs e)
		{
			ffu.SetFFUSpeed(0);

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
		private void button13_Click(object sender, EventArgs e)
		{
			string ipstr = textBox12.Text;
			Ping ping = new Ping();
			string data = "ping test data";
			byte[] buf = Encoding.ASCII.GetBytes(data);
			PingReply reply = ping.Send(ipstr);
			if (reply.Status == IPStatus.Success)
			{
				textBox11.AppendText("答复主机地址:" + reply.Address.ToString());
				textBox11.AppendText("往返时间:" + reply.RoundtripTime);
				textBox11.AppendText("生存时间（Ttl）:" + reply.Options.Ttl);
				textBox11.AppendText("缓冲区大小:" + reply.Buffer.Length + "\r\n");
			}
			else
			{
				textBox11.AppendText(textBox12.Text + "通讯失败");
			}

		}
		private bool DectNetworkCable(string ip)
		{
			try
			{
				Ping ping = new Ping();
				PingReply pr = ping.Send(ip);
				if (pr.Status == IPStatus.Success)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			catch (Exception)
			{

				return false;
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



		private void button1_Click(object sender, EventArgs e)
		{
			string Error = string.Empty;
			if (textBox2.Text == "1")
			{
				iNOVANCE.WritePlc("M100", "50");
			}
			else
			{
				iNOVANCE.WritePlc("M100", "50");
			}
		}
        #endregion

        private void tbMassage_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
