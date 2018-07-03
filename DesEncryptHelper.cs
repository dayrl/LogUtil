using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Zdd.Logger
{
	public static class DesEncryptHelper
	{
		public static string EncryptString(string unEncryptString, string key)
		{
			if (string.IsNullOrEmpty(key) || key.Length < 8)
			{
				throw new ArgumentException("min length is 8.", "key");
			}
			DESCryptoServiceProvider dESCryptoServiceProvider = new DESCryptoServiceProvider();
			byte[] bytes = Encoding.UTF8.GetBytes(unEncryptString);
			dESCryptoServiceProvider.Key = Encoding.UTF8.GetBytes(key);
			dESCryptoServiceProvider.IV = Encoding.UTF8.GetBytes(key);
			MemoryStream memoryStream = new MemoryStream();
			CryptoStream cryptoStream = new CryptoStream(memoryStream, dESCryptoServiceProvider.CreateEncryptor(), CryptoStreamMode.Write);
			cryptoStream.Write(bytes, 0, bytes.Length);
			cryptoStream.FlushFinalBlock();
			StringBuilder stringBuilder = new StringBuilder();
			byte[] array = memoryStream.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				byte b = array[i];
				stringBuilder.AppendFormat("{0:X2}", b);
			}
			return stringBuilder.ToString();
		}

		public static string DecryptString(string encryptString, string key)
		{
			if (string.IsNullOrEmpty(key) || key.Length < 8)
			{
				throw new ArgumentException("min length is 8.", "key");
			}
			DESCryptoServiceProvider dESCryptoServiceProvider = new DESCryptoServiceProvider();
			byte[] array = new byte[encryptString.Length / 2];
			for (int i = 0; i < encryptString.Length / 2; i++)
			{
				int num = Convert.ToInt32(encryptString.Substring(i * 2, 2), 16);
				array[i] = (byte)num;
			}
			dESCryptoServiceProvider.Key = Encoding.UTF8.GetBytes(key);
			dESCryptoServiceProvider.IV = Encoding.UTF8.GetBytes(key);
			MemoryStream memoryStream = new MemoryStream();
			CryptoStream cryptoStream = new CryptoStream(memoryStream, dESCryptoServiceProvider.CreateDecryptor(), CryptoStreamMode.Write);
			cryptoStream.Write(array, 0, array.Length);
			cryptoStream.FlushFinalBlock();
			return Encoding.UTF8.GetString(memoryStream.ToArray());
		}
	}
}
