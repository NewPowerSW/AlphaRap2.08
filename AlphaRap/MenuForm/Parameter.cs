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
		/// <summary>
		/// 构造函数：必须调用 InitializeComponent()，否则 Parameter.Designer.cs 里定义的控件
		/// （deviceControl1 等）一个都不会被创建 —— 现象是整页空白。
		/// </summary>
		public Parameter()
		{
			InitializeComponent();
		}
    }
}
