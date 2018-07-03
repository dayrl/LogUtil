using log4net.Appender;
using System;
using System.IO;
using System.Linq;

namespace Zdd.Logger
{
	public class EncryptRollingFileAppender : RollingFileAppender
	{
		protected override void SetQWForFiles(TextWriter writer)
		{
			if (LoggerKeyHelper.EncryptLoggerAppenders == null || LoggerKeyHelper.EncryptLoggerAppenders.All((string t) => !string.Equals(t, base.Name, StringComparison.CurrentCultureIgnoreCase)))
			{
				base.SetQWForFiles(writer);
				return;
			}
			base.QuietWriter = (new EncryptCountingQuietTextWriter(LoggerKeyHelper.Key, writer, this.ErrorHandler));
		}
	}
}
