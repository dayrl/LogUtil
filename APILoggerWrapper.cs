using System;

namespace Zdd.Logger
{
	public class APILoggerWrapper
	{
		private ILogger m_iLogger;

        private APILoggerWrapper(ILogger iLogger)
		{
			this.m_iLogger = iLogger;
		}

		public static APILoggerWrapper Create(string name)
		{
			return new APILoggerWrapper(Log4NetWrapper.Create(name));
		}

		public void LogDebug(string message)
		{
			this.m_iLogger.Log(message, emLogLevel.Debug);
		}

		public void LogDebug(string message, string key)
		{
			this.m_iLogger.Log(message, key, emLogLevel.Debug);
		}

		public void LogDebug(string message, Exception exp)
		{
			this.m_iLogger.Log(message, emLogLevel.Debug, exp);
		}

		public void LogDebug(string message, string key, Exception exp)
		{
			this.m_iLogger.Log(message, key, emLogLevel.Debug, exp);
		}

		public void LogDebugFormat(string format, params object[] args)
		{
			this.m_iLogger.LogFormat(format, emLogLevel.Debug, args);
		}

		public void LogInfo(string message)
		{
			this.m_iLogger.Log(message, emLogLevel.Info);
		}

		public void LogInfo(string message, string key)
		{
			this.m_iLogger.Log(message, key, emLogLevel.Info);
		}

		public void LogInfo(string message, Exception exp)
		{
			this.m_iLogger.Log(message, emLogLevel.Info, exp);
		}

		public void LogInfo(string message, string key, Exception exp)
		{
			this.m_iLogger.Log(message, key, emLogLevel.Info, exp);
		}

		public void LogInfoFormat(string format, params object[] args)
		{
			this.m_iLogger.LogFormat(format, emLogLevel.Info, args);
		}

		public void LogWarn(string message)
		{
			this.m_iLogger.Log(message, emLogLevel.Warn);
		}

		public void LogWarn(string message, string key)
		{
			this.m_iLogger.Log(message, key, emLogLevel.Warn);
		}

		public void LogWarn(string message, Exception exp)
		{
			this.m_iLogger.Log(message, emLogLevel.Warn, exp);
		}

		public void LogWarn(string message, string key, Exception exp)
		{
			this.m_iLogger.Log(message, key, emLogLevel.Warn, exp);
		}

		public void LogWarnFormat(string format, params object[] args)
		{
			this.m_iLogger.LogFormat(format, emLogLevel.Warn, args);
		}

		public void LogError(string message)
		{
			this.m_iLogger.Log(message, emLogLevel.Error);
		}

		public void LogError(string message, string key)
		{
			this.m_iLogger.Log(message, key, emLogLevel.Error);
		}

		public void LogError(string message, Exception exp)
		{
			this.m_iLogger.Log(message, emLogLevel.Error, exp);
		}

		public void LogError(string message, string key, Exception exp)
		{
			this.m_iLogger.Log(message, key, emLogLevel.Error, exp);
		}

		public void LogErrorFormat(string format, params object[] args)
		{
			this.m_iLogger.LogFormat(format, emLogLevel.Error, args);
		}

		public void LogFatal(string message)
		{
			this.m_iLogger.Log(message, emLogLevel.Fatal);
		}

		public void LogFatal(string message, string key)
		{
			this.m_iLogger.Log(message, key, emLogLevel.Fatal);
		}

		public void LogFatal(string message, Exception exp)
		{
			this.m_iLogger.Log(message, emLogLevel.Fatal, exp);
		}

		public void LogFatal(string message, string key, Exception exp)
		{
			this.m_iLogger.Log(message, key, emLogLevel.Fatal, exp);
		}

		public void LogFatalFormat(string format, params object[] args)
		{
			this.m_iLogger.LogFormat(format, emLogLevel.Fatal, args);
		}

		public void LogHex(byte[] arrBuffer, int argSize)
		{
			this.m_iLogger.LogHex(arrBuffer, argSize);
		}

		public void LogHexFormat(string argFormat, byte[] arrBuffer, int argSize)
		{
			this.m_iLogger.LogHexFormat(argFormat, arrBuffer, argSize);
		}
	}
}
