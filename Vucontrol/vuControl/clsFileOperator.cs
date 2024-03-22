using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using LogSv;
using Microsoft.VisualBasic.FileIO;

namespace VUControl
{
	internal class clsFileOperator
	{
		internal const string sectionName = "Parameters";

		internal const string subFileName = "CommParameter";

		public static bool ReadXmlToDataGridView(ref DataGridView myGrid, string xml_FilePath, string keyWord = "Parameters")
		{
			XmlDocument xmlDocument = new XmlDocument();
			if (File.Exists(xml_FilePath) && xml_FilePath.Contains(".xml"))
			{
				xmlDocument.Load(xml_FilePath);
				try
				{
					XmlNodeList childNodes = xmlDocument.SelectSingleNode(keyWord).ChildNodes;
					if (myGrid.Rows.Count > 1)
					{
						myGrid.Rows.Clear();
					}
					for (int i = 0; i < childNodes.Count; i++)
					{
						XmlElement xmlElement = (XmlElement)childNodes[i];
						myGrid.Rows.Add();
						for (int j = 0; j < myGrid.Columns.Count; j++)
						{
							myGrid.Rows[i].Cells[j].Value = xmlElement.ChildNodes.Item(j).InnerText;
						}
					}
					return true;
				}
				catch (Exception ex)
				{
					Log.log.Write("XML格式不对......" + ex.Message, Color.Red);
					return false;
				}
			}
			Log.log.Write("XML文件路径为空......", Color.Black);
			return false;
		}

		public static bool SaveXmlPara(DataGridView dgvParameters, string xmlPath, string keyWord = "Parameters")
		{
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				string empty = string.Empty;
				empty = (xmlPath.Contains(".xml") ? xmlPath : (xmlPath + ".xml"));
				XmlElement xmlElement = xmlDocument.CreateElement("Parameters");
				int count = dgvParameters.Rows.Count;
				if (count > 0)
				{
					int count2 = dgvParameters.Columns.Count;
					for (int i = 0; i < count; i++)
					{
						XmlElement xmlElement2 = xmlDocument.CreateElement("序号" + (i + 1));
						for (int j = 0; j < count2; j++)
						{
							XmlElement xmlElement3 = xmlDocument.CreateElement(dgvParameters.Columns[j].HeaderText.ToString());
							string empty2 = string.Empty;
							empty2 = ((dgvParameters.Rows[i].Cells[j].Value != null) ? dgvParameters.Rows[i].Cells[j].Value.ToString() : "");
							xmlElement3.InnerText = empty2;
							xmlElement2.AppendChild(xmlElement3);
							xmlElement.AppendChild(xmlElement2);
						}
					}
					xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", "utf-8", ""));
					xmlDocument.AppendChild(xmlElement);
					xmlDocument.Save(empty);
					Log.log.Write("XML格式文件保存成功!", Color.Black);
					return true;
				}
				Log.log.Write("表格中未包含任何行数据，不能保存XML文件", Color.Black);
				return false;
			}
			catch (Exception ex)
			{
				MessageBox.Show("参数保存失败....");
				Log.log.Write("保存XML文件Error:" + ex.Message, Color.Red);
				return false;
			}
		}

		public static void ReadCsv(ref DataGridView myDgv, string fileName, string delimiters, bool firstRowContainsFieldNames = true)
		{
			DataTable dataTable = new DataTable();
			using (TextFieldParser textFieldParser = new TextFieldParser(fileName, Encoding.GetEncoding(0)))
			{
				try
				{
					textFieldParser.SetDelimiters(delimiters);
					if (!textFieldParser.EndOfData)
					{
						string[] array = textFieldParser.ReadFields();
						for (int i = 0; i < array.Length; i++)
						{
							if (firstRowContainsFieldNames)
							{
								string[] array2 = array[i].Split(',');
								for (int j = 0; j < array2.Length; j++)
								{
									dataTable.Columns.Add(array2[j]);
								}
							}
							else
							{
								dataTable.Columns.Add("Col" + i);
							}
						}
						if (!firstRowContainsFieldNames)
						{
							DataRowCollection rows = dataTable.Rows;
							object[] values = array;
							rows.Add(values);
						}
					}
					while (!textFieldParser.EndOfData)
					{
						DataGridViewRow dataGridViewRow = new DataGridViewRow();
						string[] array3 = textFieldParser.ReadFields();
						for (int k = 0; k < array3.Length; k++)
						{
							string[] array4 = array3[k].Split(',');
							DataGridViewRow dataGridViewRow2 = new DataGridViewRow();
							for (int l = 0; l < myDgv.Columns.Count; l++)
							{
								DataGridViewColumn dataGridViewColumn = myDgv.Columns[l];
								dataGridViewRow2.Cells.Add(dataGridViewColumn.CellTemplate.Clone() as DataGridViewCell);
								dataGridViewRow2.Cells[l].Value = array4[l];
							}
							myDgv.Rows.Add(dataGridViewRow2);
						}
					}
				}
				catch (Exception ex)
				{
					Log.log.Write("Read CSV file Error:" + ex.Message, Color.Red);
				}
			}
		}

		public static bool DataGridViewExportToCSV(DataGridView dataGridView1)
		{
			bool result = false;
			if (dataGridView1.Rows.Count == 0)
			{
				result = false;
			}
			else
			{
				SaveFileDialog saveFileDialog = new SaveFileDialog();
				saveFileDialog.Filter = "CSV files (*.csv)|*.csv";
				saveFileDialog.FilterIndex = 0;
				saveFileDialog.RestoreDirectory = true;
				saveFileDialog.CreatePrompt = true;
				saveFileDialog.FileName = null;
				saveFileDialog.Title = "保存";
				if (saveFileDialog.ShowDialog() == DialogResult.OK)
				{
					Stream stream = saveFileDialog.OpenFile();
					StreamWriter streamWriter = new StreamWriter(stream, Encoding.GetEncoding(0));
					string text = "";
					try
					{
						for (int i = 0; i < dataGridView1.ColumnCount; i++)
						{
							if (i > 0)
							{
								text += ",";
							}
							text += dataGridView1.Columns[i].HeaderText;
						}
						text.Remove(text.Length - 1);
						streamWriter.WriteLine(text);
						text = "";
						for (int j = 0; j < dataGridView1.Rows.Count; j++)
						{
							text = "";
							int count = dataGridView1.Columns.Count;
							for (int k = 0; k < count; k++)
							{
								if (k > 0 && k < count)
								{
									text += ",";
								}
								if (dataGridView1.Rows[j].Cells[k].Value == null)
								{
									text = text ?? "";
									continue;
								}
								string text2 = dataGridView1.Rows[j].Cells[k].Value.ToString().Trim();
								text2 = text2.Replace("\"", "\"\"");
								text += text2;
							}
							streamWriter.WriteLine(text);
						}
						result = true;
					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.Message, "导出错误", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
						result = false;
					}
					finally
					{
						streamWriter.Close();
						stream.Close();
						MessageBox.Show("数据被导出到：" + saveFileDialog.FileName.ToString(), "导出完毕", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
					}
				}
			}
			return result;
		}
	}
}
