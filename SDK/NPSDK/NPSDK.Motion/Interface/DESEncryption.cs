using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace NPSDK
{
	public class DESEncryption
	{
		private static string strKey = "NPSDK";

		private static string strIV = "07551234";

		public static string StringEncrypt(string strSource)
		{
			MemoryStream memoryStream = new MemoryStream();
			DESCryptoServiceProvider dESCryptoServiceProvider = new DESCryptoServiceProvider();
			CryptoStream cryptoStream = new CryptoStream(memoryStream, dESCryptoServiceProvider.CreateEncryptor(Encoding.Default.GetBytes(strKey), Encoding.Default.GetBytes(strIV)), CryptoStreamMode.Write);
			StreamWriter streamWriter = new StreamWriter(cryptoStream);
			streamWriter.WriteLine(strSource);
			streamWriter.Close();
			cryptoStream.Close();
			byte[] inArray = memoryStream.ToArray();
			memoryStream.Close();
			return Convert.ToBase64String(inArray);
		}

		public static string StringDecrypt(string strSource)
		{
			MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(strSource));
			DESCryptoServiceProvider dESCryptoServiceProvider = new DESCryptoServiceProvider();
			CryptoStream cryptoStream = new CryptoStream(memoryStream, dESCryptoServiceProvider.CreateDecryptor(Encoding.Default.GetBytes(strKey), Encoding.Default.GetBytes(strIV)), CryptoStreamMode.Read);
			StreamReader streamReader = new StreamReader(cryptoStream);
			string result = streamReader.ReadLine();
			streamReader.Close();
			cryptoStream.Close();
			memoryStream.Close();
			return result;
		}

		public static void XmlSaveEncrypt(XmlDocument xmlDoc, string filePath)
		{
			if (!Directory.Exists(Path.GetDirectoryName(filePath)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(filePath));
			}
			if (!File.Exists(filePath))
			{
				File.Create(filePath).Close();
			}
			FileStream fileStream = new FileStream(filePath, FileMode.Truncate);
			MemoryStream memoryStream = new MemoryStream();
			xmlDoc.Save(memoryStream);
			DESCryptoServiceProvider dESCryptoServiceProvider = new DESCryptoServiceProvider();
			CryptoStream cryptoStream = new CryptoStream(fileStream, dESCryptoServiceProvider.CreateEncryptor(Encoding.UTF8.GetBytes(strKey), Encoding.UTF8.GetBytes(strIV)), CryptoStreamMode.Write);
			cryptoStream.Write(memoryStream.ToArray(), 0, memoryStream.ToArray().Length);
			cryptoStream.FlushFinalBlock();
			memoryStream.Close();
			fileStream.Close();
		}

		public static XmlDocument XmlLoadDecrypt(string filePath)
		{
			XmlDocument xmlDocument = new XmlDocument();
			FileStream fileStream = new FileStream(filePath, FileMode.Open);
			byte[] array = new byte[fileStream.Length];
			fileStream.Read(array, 0, array.Length);
			fileStream.Close();
			MemoryStream memoryStream = new MemoryStream();
			DESCryptoServiceProvider dESCryptoServiceProvider = new DESCryptoServiceProvider();
			CryptoStream cryptoStream = new CryptoStream(memoryStream, dESCryptoServiceProvider.CreateDecryptor(Encoding.UTF8.GetBytes(strKey), Encoding.UTF8.GetBytes(strIV)), CryptoStreamMode.Write);
			cryptoStream.Write(array, 0, array.Length);
			cryptoStream.FlushFinalBlock();
			xmlDocument.Load(new MemoryStream(memoryStream.ToArray()));
			return xmlDocument;
		}
	}
}
