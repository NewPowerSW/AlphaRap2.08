using AlphaRapLibrary;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlphaRap.MES
{
	public partial class Amphenol : ModuleBaseForm
	{
		public string Post_Message = string.Empty;

		#region 入站
		/// <summary>
		/// 入站Url
		/// </summary>
		string InStation_URL
		{
			get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_InURL");
			
		}
		/// <summary>
		/// 入站SfcNo
		/// </summary>
		string InStation_SfcNo
		{
			get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_InSfcNo");
		
		}
		/// <summary>
		/// 入站ProductSn
		/// </summary>
		string InStation_ProductSn
		{
			get;
			set;
			//get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_InProductSn");
			//set => MiddleLayer.MesF.SetSettingValue("Mset", "textBox_InProductSn", value);
		}
		/// <summary>
		/// 入站DeviceCode
		/// </summary>
		string InStation_DeviceCode
		{
			get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_InDeviceCode");
	
		}
		#endregion

		#region 出站
		/// <summary>
		/// 出站TestList键值队
		/// </summary>
		Dictionary<string, string> Dictionary_TestList = new Dictionary<string, string>();
		/// <summary>
		/// 出站Url
		/// </summary>
		string OutStation_URL
		{
			get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_OutURL");
			set => MiddleLayer.MesF.SetSettingValue("Mset", "textBox_OutURL", value);
		}
		/// <summary>
		/// 出站OutSfcNo
		/// </summary>
		string OutStation_SfcNo
		{
			get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_OutSfcNo");
			set => MiddleLayer.MesF.SetSettingValue("Mset", "textBox_OutSfcNo", value);
		}
		/// <summary>
		///出站OutUserID
		/// </summary>
		string OutStation_UserID
		{
			get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_OutUserID");
			set => MiddleLayer.MesF.SetSettingValue("Mset", "textBox_OutUserID", value);
		}
		/// <summary>
		/// 出站OutProductSn
		/// </summary>
		string OutStation_ProductSn
		{
			get;
			set;
			//get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_OutProductSn");
			//set => MiddleLayer.MesF.SetSettingValue("Mset", "textBox_OutProductSn", value);
		}
		/// <summary>
		/// 出站DeviceCode
		/// </summary>
		string OutStation_DeviceCode
		{
			get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_OutDeviceCode");
			set => MiddleLayer.MesF.SetSettingValue("Mset", "textBox_OutDeviceCode", value);
		}
		/// <summary>
		/// 出站OutEditionCode
		/// </summary>
		string OutStation_EditionCode
		{
			get;
			set;
			//get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_OutEditionCode");
			//set => MiddleLayer.MesF.SetSettingValue("Mset", "textBox_OutEditionCode", value);
		}
		/// <summary>
		/// 出站ErrorCode
		/// </summary>
		string OutStation_ErrorCode
		{
			get;
			set;
			//get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_OutErrorCode");
			//set => MiddleLayer.MesF.SetSettingValue("Mset", "textBox_OutErrorCode", value);
		}
		/// <summary>
		/// 出站ErrorSpot
		/// </summary>
		string OutStation_ErrorSpot
		{
			get;
			set;
			//get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_OutErrorSpot");
			//set => MiddleLayer.MesF.SetSettingValue("Mset", "textBox_OutErrorSpot", value);
		}
		#endregion

		#region 关联
		/// <summary>
		/// 关联Url
		/// </summary>
		public string Binding_Url
		{
			get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_BindingUrl");
			set => MiddleLayer.MesF.SetSettingValue("Mset", "textBox_BindingUrl", value);
		}
		/// <summary>
		/// 关联SfcNO
		/// </summary>
		public string Binding_SfcNO
		{
			get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_BindingSfcNO");
			set => MiddleLayer.MesF.SetSettingValue("Mset", "textBox_BindingSfcNO", value);
		}
		/// <summary>
		/// 关联ProductSn
		/// </summary>
		public string Binding_ProductSn
		{
			//get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_BindingProductSn");
			//set => MiddleLayer.MesF.SetSettingValue("Mset", "textBox_BindingProductSn", value);
			get;
			set;
		}

		/// <summary>
		/// 关联DeviceCode
		/// </summary>
		public string Binding_DeviceCode
		{
			get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_BindingDeviceCode");
			set => MiddleLayer.MesF.SetSettingValue("Mset", "textBox_BindingDeviceCode", value);
		}
		/// <summary>
		/// 关联UserID
		/// </summary>
		public string Binding_UserID
		{
			get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_BindingUserID");
			set => MiddleLayer.MesF.SetSettingValue("Mset", "textBox_BindingUserID", value);
		}
		/// <summary>
		/// 关联EditionCode
		/// </summary>
		public string BindingKeyPcbCode
		{
			//get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_BindingKeyPcbCode");
			//set => MiddleLayer.MesF.SetSettingValue("Mset", "textBox_BindingKeyPcbCode", value);
			get;
			set;
		}
		#endregion

		#region 镭雕
		/// <summary>
		/// 镭雕接口PCBList
		/// </summary>
		Dictionary<string, string> Dictionary_PCBList = new Dictionary<string, string>();
		/// <summary>
		/// 镭雕URL
		/// </summary>
		string LaserStation_URL
		{
			get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_LaserMarkUrl");
			set => MiddleLayer.MesF.SetSettingValue("Mset", "textBox_LaserMarkUrl", value);
		}
		/// <summary>
		/// 镭雕Product
		/// </summary>
		string LaserStation_Product
		{
			get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_LaserProduct");
			set => MiddleLayer.MesF.SetSettingValue("Mset", "textBox_LaserProduct", value);
		}
		/// <summary>
		/// 镭雕WorkStationName
		/// </summary>
		string LaserStation_WorkStationName
		{
			get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_LaserWorkStationName");
			set => MiddleLayer.MesF.SetSettingValue("Mset", "textBox_LaserWorkStationName", value);
		}
		/// <summary>
		/// 镭雕Version
		/// </summary>
		string LaserStation_Version
		{
			get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_LaserVersion");
			set => MiddleLayer.MesF.SetSettingValue("Mset", "textBox_LaserVersion", value);
		}
		/// <summary>
		/// 镭雕Side
		/// </summary>
		string LaserStation_Side
		{
			get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_LaserSide");
			set => MiddleLayer.MesF.SetSettingValue("Mset", "textBox_LaserSide", value);
		}
		/// <summary>
		/// 镭雕PaneCode
		/// </summary>
		string LaserStation_PaneCode
		{
			get => MiddleLayer.MesF.GetSettingValue("Mset", "textBox_LaserPaneCode");
			set => MiddleLayer.MesF.SetSettingValue("Mset", "textBox_LaserPaneCode", value);
		}

		#endregion

		public Amphenol()
		{
			InitializeComponent();
		}
		readonly Object OBJ = new object();
		/// <summary>
		/// Post上传
		/// </summary>
		/// <param name="Url">Url地址</param>
		/// <param name="jsonParas">上传的JSOn数据</param>
		/// <returns></returns>
		private string Post(string Url, string jsonParas,out string Post_Message)
		{
			lock (OBJ)
			{
				string strURL = Url;
				//创建一个HTTP请求  
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strURL);
				//Post请求方式  
				request.Method = "POST";
				//内容类型
				request.ContentType = "application/json";
				//设置响应时间
				request.Timeout = 30000;

				//设置参数，并进行URL编码           
				byte[] payload = Encoding.ASCII.GetBytes(jsonParas);
				//设置请求的ContentLength   
				request.ContentLength = payload.Length;
				//发送请求，获得请求流 
				Stream writer;
				try
				{
					writer = request.GetRequestStream();//获取用于写入请求数据的Stream对象
				}
				catch (Exception ex)
				{
					MiddleLayer.DataF.AddLogError(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + ex.ToString());
					writer = null;
					Console.Write("连接服务器失败!");
					//MessageBox.Show(ex.Message);
				}
				//将请求参数写入流

				writer.Write(payload, 0, payload.Length);
				writer.Close();//关闭请求流
							   // String strValue = "";//strValue为http响应所返回的字符流
				MiddleLayer.DataF.SaveMesLog("Send" + jsonParas);
				HttpWebResponse response;
				try
				{
					//获得响应流
					response = (HttpWebResponse)request.GetResponse();
				}
				catch (WebException ex)
				{
					response = ex.Response as HttpWebResponse;
					//MessageBox.Show(ex.Message);
				}
				Stream s = response.GetResponseStream();
				//  Stream postData = Request.InputStream;
				StreamReader sRead = new StreamReader(s);
				string postContent = sRead.ReadToEnd();
				sRead.Close();
				textBox24.Text = postContent;
				MiddleLayer.DataF.SaveMesLog("Receive" + postContent);
				Post_Message = postContent;
				return postContent;//返回Json数据
			}


		}

		public string parseJsonOfTerminal(string jsonText, string JsonNode)
		{
			JObject jObj = JObject.Parse(jsonText);
			return jObj[JsonNode].ToString();


		}
		/// <summary>
		/// 入站Api
		/// </summary>
		public bool InStation()
		{
			StringBuilder str = new StringBuilder();

			str.Append("{");
			str.Append("\"" + "SfcNo" + "\"" + ":" + "\"" + InStation_SfcNo + "\"" + ",");
			str.Append("\"" + "ProductSn" + "\"" + ":" + "\"" + InStation_ProductSn + "\"" + ",");
			str.Append("\"" + "DeviceCode" + "\"" + ":" + "\"" + InStation_DeviceCode + "\"");
			str.Append("}");
			MiddleLayer.DataF.SaveMesLog("InStation:" + str.ToString());
			return Convert.ToBoolean(parseJsonOfTerminal(Post(InStation_URL, str.ToString(),out Post_Message).Replace("NG", "false").Replace("OK", "true"), "Result"));

		}
		/// <summary>
		/// 出站Api
		/// </summary>
		public bool OutStation()
		{


			StringBuilder str = new StringBuilder();

			str.Append("{" + "\"" + "TestList" + "\"" + ":" + "[");
			int index = 0;
			foreach (KeyValuePair<string, string> item in Dictionary_TestList)
			{
				index++;

				if (index >= Dictionary_TestList.Count)
				{
					str.Append("{" + "\"" + "Key" + "\"" + ":" + "\"" + item.Key + "\"" + "," + "\"" + "Value" + "\"" + ":" + "\"" + item.Value + "\"" + "}" + "],");
				}
				else
				{
					str.Append("{" + "\"" + "Key" + "\"" + ":" + "\"" + item.Key + "\"" + "," + "\"" + "Value" + "\"" + ":" + "\"" + item.Value + "\"" + "}" + ",");
				}

			}
			str.Append("\"" + "SfcNo" + "\"" + ":" + "\"" + OutStation_SfcNo + "\"" + ",");
			str.Append("\"" + "UserID" + "\"" + ":" + "\"" + OutStation_UserID + "\"" + ",");
			str.Append("\"" + "ProductSn" + "\"" + ":" + "\"" + OutStation_ProductSn + "\"" + ",");
			str.Append("\"" + "DeviceCode" + "\"" + ":" + "\"" + OutStation_DeviceCode + "\"" + ",");
			str.Append("\"" + "EditionCode" + "\"" + ":" + "\"" + OutStation_EditionCode + "\"" + ",");
			str.Append("\"" + "ErrorCode" + "\"" + ":" + "\"" /*+ OutStation_ErrorCode*/ + "\"" + ",");
			str.Append("\"" + "ErrorSpot" + "\"" + ":" + "\"" /*+ OutStation_ErrorSpot*/ + "\"");
			str.Append("}");
			MiddleLayer.DataF.SaveMesLog("OutStation:" + str.ToString());
			Dictionary_TestList.Clear();
			return Convert.ToBoolean(parseJsonOfTerminal(Post(OutStation_URL, str.ToString(), out Post_Message).Replace("NG", "false").Replace("OK", "true"), "Result"));


		}
		public bool Binding()
		{
			StringBuilder str = new StringBuilder();

			str.Append("{");
			str.Append("\"" + "SfcNo" + "\"" + ":" + "\"" + Binding_SfcNO + "\"" + ",");
			str.Append("\"" + "ProductSn" + "\"" + ":" + "\"" + Binding_ProductSn + "\"" + ",");
			str.Append("\"" + "DeviceCode" + "\"" + ":" + "\"" + Binding_DeviceCode + "\"" + ",");
			str.Append("\"" + "UserID" + "\"" + ":" + "\"" + Binding_UserID + "\"" + ",");
			str.Append("\"" + "KeyPcbCode" + "\"" + ":" + "\"" + BindingKeyPcbCode + "\"");
			str.Append("}");
			MiddleLayer.DataF.SaveMesLog("Binding:" + str.ToString());
			return Convert.ToBoolean(parseJsonOfTerminal(Post(Binding_Url, str.ToString(), out Post_Message).Replace("NG", "false").Replace("OK", "true"), "Result"));

		}
		public bool LaserMark()
		{

			StringBuilder str = new StringBuilder();

			str.Append("{");
			str.Append("\"" + "Product" + "\"" + ":" + "\"" + LaserStation_Product + "\"" + ",");
			str.Append("\"" + "WorkStationName" + "\"" + ":" + "\"" + LaserStation_WorkStationName + "\"" + ",");
			str.Append("\"" + "Version" + "\"" + ":" + "\"" + LaserStation_Version + "\"" + ",");
			str.Append("\"" + "Side" + "\"" + ":" + "\"" + LaserStation_Side + "\"" + ",");
			str.Append("\"" + "PaneCode" + "\"" + ":" + "\"" + LaserStation_PaneCode + "\"" + ",");

			str.Append("\"" + "PCBList" + "\"" + ":" + ":{");
			int index = 0;
			foreach (KeyValuePair<string, string> item in Dictionary_PCBList)
			{
				index++;
				if (index >= Dictionary_PCBList.Count)
				{
					str.Append("\"" + item.Key + "\"" + ":" + "\"" + item.Value + "\"" + "}");
				}
				else
				{
					str.Append("\"" + item.Key + "\"" + ":" + "\"" + item.Value + "\"" + ",");
				}

			}
			str.Append("}");
			MiddleLayer.DataF.SaveMesLog("LaserMark" + str.ToString());
			Dictionary_PCBList.Clear();
			return Convert.ToBoolean(parseJsonOfTerminal(Post(LaserStation_URL, str.ToString(), out Post_Message), "Result"));


		}

		private void button1_Click(object sender, EventArgs e)
		{
			bool r = LaserMark();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			bool r = Binding();
		}

		private void button3_Click(object sender, EventArgs e)
		{
			bool r = InStation();
		}

		private void button4_Click(object sender, EventArgs e)
		{
			Dictionary_TestList.Add("测试结果", "NULL");
			bool r = OutStation();
		}
	}
}
