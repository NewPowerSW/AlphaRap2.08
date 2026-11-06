using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Globalization;

namespace DataSave
{

	public partial class TypeCollection1 : Component
	{
		public TypeCollection1()
		{
			InitializeComponent();

			//   gName = this.components.Components.ToString();
		}

		private string gText;
		[Browsable(true)]
		[Description(""), Category("Design"), DefaultValue("")]
		public string Text
		{
			get { return gText; }
			set
			{

				gText = value;
			}
		}

		private string gSavePath;
		[Browsable(true)]
		[Description(""), Category("Design"), DefaultValue("")]
		public string SavePath
		{
			get { return gSavePath; }
			set
			{

				gSavePath = value;
			}
		}

		private List<DataFormat> gitems = new List<DataFormat>();
		[TypeConverter(typeof(System.ComponentModel.CollectionConverter))]//指定编辑器特性
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]//设定序列化特性
		[Category("DataFormat"), Description("图像文件集")]
		public List<DataFormat> Items
		{
			get
			{ return gitems; }
			set
			{
				
				gitems = value;

			}


		}


        /// <summary>按业务日期及条码保存记录，失败向业务调用方抛出。</summary>
        public void SaveDataToFile(DateTime DataDate, string barcode, string[] data)
        {
            AppendRecordAt(DataDate, new[] { barcode }.Concat(data).ToArray());
        }

        /// <summary>保存原有日志接口的完整字段，时间列由组件自动填写。</summary>
        public void SaveDataToFile(string[] data)
        {
            int width = Items.Count - 1;
            if (data != null && data.Length > width && data.Skip(width).All(string.IsNullOrEmpty)) data = data.Take(width).ToArray();
            AppendRecord(data);
        }

        /// <summary>读取指定日期范围内的所有日志版本，按实际文件表头合并列。</summary>
        public List<string[]> ReadDataFromToFile(DateTime starTime1, DateTime endTime1)
        {
            return ReadRecords(starTime1, endTime1);
        }

	}

	public partial class DataFormat
	{


		public DataFormat()
		{
			this.Name = "data";

		}


		private string gName;
		[Browsable(true)]
		[Description(""), Category("Design"), DefaultValue("")]
		public string Name
		{
			get { return gName; }
			set
			{

				gName = value;
			}
		}




	}

}
