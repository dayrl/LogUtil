using log4net.Core;
using log4net.Util;
using System;
using System.IO;
using System.Linq;

namespace Zdd.Logger
{
	public class EncryptCountingQuietTextWriter : CountingQuietTextWriter
	{
		private readonly string _key;

		public EncryptCountingQuietTextWriter(string key, TextWriter writer, IErrorHandler errorHandler) : base(writer, errorHandler)
		{
			this._key = key;
		}

		public override void Write(char value)
		{
			try
			{
				string text = DesEncryptHelper.EncryptString(string.Join("", new object[]
				{
					value
				}), this._key);
				text = string.Format("{0}{1}{2}\n", "[", text,"]");
				base.Write(text);
				base.Count=(base.Count + (long)this.Encoding.GetByteCount(text));
			}
			catch (Exception ex)
			{
                base.ErrorHandler.Error("Failed to write [" + value + "].", ex, ErrorCode.WriteFailure);
			}
		}

		public override void Write(char[] buffer, int index, int count)
		{
			if (count > 0)
			{
				try
				{
					string text = DesEncryptHelper.EncryptString(string.Join<char>("", buffer.Skip(index).Take(count)), this._key);
					text = string.Format("{0}{1}{2}\n", "[", text,"]");
					base.Write(text);
					base.Count = (base.Count + (long)this.Encoding.GetByteCount(text));
				}
				catch (Exception ex)
				{
                    base.ErrorHandler.Error("Failed to write buffer.", ex, ErrorCode.WriteFailure);
				}
			}
		}

		public override void Write(string str)
		{
			if (string.IsNullOrEmpty(str))
			{
				return;
			}
			try
			{
				string text = DesEncryptHelper.EncryptString(string.Join("", new string[]
				{
					str
				}), this._key);
                text = string.Format("{0}{1}{2}\n", "[", text, "]");
				base.Write(text);
				base.Count=(base.Count + (long)this.Encoding.GetByteCount(text));
			}
			catch (Exception ex)
			{
                base.ErrorHandler.Error("Failed to write [" + str + "].", ex, ErrorCode.WriteFailure);
			}
		}
	}
}
