using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.Xml;
using System.Diagnostics;
using System.IO;
using log4net.Config;

namespace Zdd.Logger
{
    public class Log4NetWrapper : ILogger
    {
        private static bool m_bIsOpen;
        private ILog m_iLog;
        private static bool s_showHex;
        private object m_synLocker = new object();
        private Dictionary<string, bool> m_dicFilter;

        public static bool Opened
        {
            get
            {
                return Log4NetWrapper.m_bIsOpen;
            }
        }

        public Dictionary<string, bool> Filters
        {
            get
            {
                if (this.m_dicFilter == null)
                {
                    this.m_dicFilter = new Dictionary<string, bool>();
                }
                return this.m_dicFilter;
            }
        }

        private Log4NetWrapper(ILog iLog, string argName)
        {
            this.m_iLog = iLog;
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(APILogManager.Instance.LogPath+ "config\\LogConfig.xml");
                XmlNode xmlNode = xmlDocument.DocumentElement.SelectSingleNode(string.Format("logger[@name='{0}']/appender-ref", argName));
                if (xmlNode != null)
                {
                    XmlAttribute xmlAttribute = xmlNode.Attributes["ref"];
                    if (xmlAttribute != null && !string.IsNullOrEmpty(xmlAttribute.Value))
                    {
                        XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes(string.Format("appender[@name='{0}']/Filter/Keyword", xmlAttribute.Value));
                        if (xmlNodeList.Count != 0)
                        {
                            XmlAttribute xmlAttribute2 = null;
                            int value = 0;
                            foreach (XmlNode xmlNode2 in xmlNodeList)
                            {
                                xmlAttribute2 = xmlNode2.Attributes["value"];
                                XmlAttribute xmlAttribute3 = xmlNode2.Attributes["enable"];
                                if (xmlAttribute2 != null && 
                                    !string.IsNullOrEmpty(xmlAttribute2.Value) && 
                                    xmlAttribute3 != null && 
                                    !string.IsNullOrEmpty(xmlAttribute3.Value) && 
                                    int.TryParse(xmlAttribute3.Value, out value))
                                {
                                    try
                                    {
                                        this.Filters.Add(xmlAttribute2.Value, value != 0);
                                    }
                                    catch (Exception)
                                    {
                                        Trace.TraceWarning("The filter [{0}] is duplicate", new object[]
										{
											xmlAttribute2.Value
										});
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        static Log4NetWrapper()
        {
            Log4NetWrapper.m_bIsOpen = false;
            Log4NetWrapper.s_showHex = true;
            try
            {
                string fileName = APILogManager.Instance.CfgPath;
                
                SolidXmlDocument solidXmlDocument = new SolidXmlDocument();
                solidXmlDocument.Load(fileName);
                XmlAttribute xmlAttribute = solidXmlDocument.DocumentElement.Attributes["showHex"];
                if (xmlAttribute != null && !string.IsNullOrEmpty(xmlAttribute.Value))
                {
                    int num = 0;
                    if (int.TryParse(xmlAttribute.Value, out num))
                    {
                        Log4NetWrapper.s_showHex = (num != 0);
                    }
                }
                XmlNode xmlNode = solidXmlDocument.DocumentElement.SelectSingleNode("EncryptLog");
                if (xmlNode != null)
                {
                    XmlAttribute xmlAttribute2 = xmlNode.Attributes["value"];
                    if (xmlAttribute2 != null && !string.IsNullOrEmpty(xmlAttribute2.Value))
                    {
                        LoggerKeyHelper.EncryptLoggerAppenders = xmlAttribute2.Value.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    }
                }
                FileInfo fileInfo = new FileInfo(fileName);
                XmlConfigurator.Configure(fileInfo);
                Log4NetWrapper.m_bIsOpen = true;
                solidXmlDocument.RemoveAll();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to startup the log4net service with error {0}", ex.Message);
            }
        }

        void ILogger.Log(string message, emLogLevel level)
        {
            lock (this.m_synLocker)
            {
                switch (level)
                {
                    case emLogLevel.Debug:
                        this.LogDebug(message, null);
                        break;
                    case emLogLevel.Info:
                        this.LogInfo(message, null);
                        break;
                    case emLogLevel.Warn:
                        this.LogWarn(message, null);
                        break;
                    case emLogLevel.Error:
                        this.LogError(message, null);
                        break;
                    case emLogLevel.Fatal:
                        this.LogFatal(message, null);
                        break;
                }
            }
        }

        void ILogger.LogHex(byte[] arrBuffer, int argSize)
        {
            lock (this.m_synLocker)
            {
                this.LogInfo(Log4NetWrapper.s_showHex ? ConvertHelper.HexToString(arrBuffer, argSize) : Encoding.ASCII.GetString(arrBuffer, 0, argSize), null);
            }
        }

        void ILogger.LogHexFormat(string argFormat, byte[] arrBuffer, int argSize)
        {
            lock (this.m_synLocker)
            {
                this.LogInfoFormat(argFormat, new object[]
				{
					Log4NetWrapper.s_showHex ? ConvertHelper.HexToString(arrBuffer, argSize) : Encoding.ASCII.GetString(arrBuffer, 0, argSize)
				});
            }
        }

        void ILogger.Log(string message, emLogLevel level, Exception exp)
        {
            lock (this.m_synLocker)
            {
                switch (level)
                {
                    case emLogLevel.Debug:
                        this.LogDebug(message, exp);
                        break;
                    case emLogLevel.Info:
                        this.LogInfo(message, exp);
                        break;
                    case emLogLevel.Warn:
                        this.LogWarn(message, exp);
                        break;
                    case emLogLevel.Error:
                        this.LogError(message, exp);
                        break;
                    case emLogLevel.Fatal:
                        this.LogFatal(message, exp);
                        break;
                }
            }
        }

        void ILogger.Log(string message, string key, emLogLevel level)
        {
            if (this.m_dicFilter != null)
            {
                bool flag = false;
                if (this.m_dicFilter.TryGetValue(key, out flag) && !flag)
                {
                    return;
                }
            }
            lock (this.m_synLocker)
            {
                switch (level)
                {
                    case emLogLevel.Debug:
                        this.LogDebug(message, null);
                        break;
                    case emLogLevel.Info:
                        this.LogInfo(message, null);
                        break;
                    case emLogLevel.Warn:
                        this.LogWarn(message, null);
                        break;
                    case emLogLevel.Error:
                        this.LogError(message, null);
                        break;
                    case emLogLevel.Fatal:
                        this.LogFatal(message, null);
                        break;
                }
            }
        }

        void ILogger.Log(string message, string key, emLogLevel level, Exception exp)
        {
            if (this.m_dicFilter != null)
            {
                bool flag = false;
                if (this.m_dicFilter.TryGetValue(key, out flag) && !flag)
                {
                    return;
                }
            }
            lock (this.m_synLocker)
            {
                switch (level)
                {
                    case emLogLevel.Debug:
                        this.LogDebug(message, exp);
                        break;
                    case emLogLevel.Info:
                        this.LogInfo(message, exp);
                        break;
                    case emLogLevel.Warn:
                        this.LogWarn(message, exp);
                        break;
                    case emLogLevel.Error:
                        this.LogError(message, exp);
                        break;
                    case emLogLevel.Fatal:
                        this.LogFatal(message, exp);
                        break;
                }
            }
        }

        void ILogger.LogFormat(string format, emLogLevel level, params object[] args)
        {
            lock (this.m_synLocker)
            {
                switch (level)
                {
                    case emLogLevel.Debug:
                        this.LogDebugFormat(format, args);
                        break;
                    case emLogLevel.Info:
                        this.LogInfoFormat(format, args);
                        break;
                    case emLogLevel.Warn:
                        this.LogWarnFormat(format, args);
                        break;
                    case emLogLevel.Error:
                        this.LogErrorFormat(format, args);
                        break;
                    case emLogLevel.Fatal:
                        this.LogFataFormat(format, args);
                        break;
                }
            }
        }

        void ILogger.LogFormat(string format, string key, emLogLevel level, params object[] args)
        {
            if (this.m_dicFilter != null)
            {
                bool flag = false;
                if (this.m_dicFilter.TryGetValue(key, out flag) && !flag)
                {
                    return;
                }
            }
            lock (this.m_synLocker)
            {
                switch (level)
                {
                    case emLogLevel.Debug:
                        this.LogDebugFormat(format, args);
                        break;
                    case emLogLevel.Info:
                        this.LogInfoFormat(format, args);
                        break;
                    case emLogLevel.Warn:
                        this.LogWarnFormat(format, args);
                        break;
                    case emLogLevel.Error:
                        this.LogErrorFormat(format, args);
                        break;
                    case emLogLevel.Fatal:
                        this.LogFataFormat(format, args);
                        break;
                }
            }
        }

        public static ILogger Create(string name)
        {
            return new Log4NetWrapper(LogManager.GetLogger(name), name);
        }

        public void LogDebug(string strMsg, Exception objExp = null)
        {
            if (this.m_iLog.IsDebugEnabled)
            {
                if (objExp == null)
                {
                    this.m_iLog.Debug(strMsg);
                    return;
                }
                this.m_iLog.Debug(strMsg, objExp);
            }
        }

        public void LogDebugFormat(string strFormat, params object[] arrObjs)
        {
            if (this.m_iLog.IsDebugEnabled)
            {
                this.m_iLog.DebugFormat(strFormat, arrObjs);
            }
        }

        public void LogInfo(string strMsg, Exception objExp = null)
        {
            if (this.m_iLog.IsInfoEnabled)
            {
                if (objExp == null)
                {
                    this.m_iLog.Info(strMsg);
                    return;
                }
                this.m_iLog.Info(strMsg, objExp);
            }
        }

        public void LogInfo(string strMsg, Exception objExp, bool argDate)
        {
            if (argDate)
            {
                DateTime now = DateTime.Now;
                string text = string.Format(" {0:dd/MM/yyyy HH:mm:ss} ", now);
                text = text.Replace("-", "/");
                strMsg = text + strMsg;
            }
            if (this.m_iLog.IsInfoEnabled)
            {
                if (objExp == null)
                {
                    this.m_iLog.Info(strMsg);
                    return;
                }
                this.m_iLog.Info(strMsg, objExp);
            }
        }

        public void LogInfoFormat(string strFormat, params object[] arrObjs)
        {
            if (this.m_iLog.IsInfoEnabled)
            {
                this.m_iLog.InfoFormat(strFormat, arrObjs);
            }
        }

        public void LogWarn(string strMsg, Exception objExp = null)
        {
            if (this.m_iLog.IsWarnEnabled)
            {
                if (objExp == null)
                {
                    this.m_iLog.Warn(strMsg);
                    return;
                }
                this.m_iLog.Warn(strMsg, objExp);
            }
        }

        public void LogWarnFormat(string strFormat, params object[] arrObjs)
        {
            if (this.m_iLog.IsWarnEnabled)
            {
                this.m_iLog.WarnFormat(strFormat, arrObjs);
            }
        }

        public void LogError(string strMsg, Exception objExp = null)
        {
            if (this.m_iLog.IsErrorEnabled)
            {
                if (objExp == null)
                {
                    this.m_iLog.Error(strMsg);
                    return;
                }
                this.m_iLog.Error(strMsg, objExp);
            }
        }

        public void LogErrorFormat(string strFormat, params object[] arrObjs)
        {
            if (this.m_iLog.IsErrorEnabled)
            {
                this.m_iLog.ErrorFormat(strFormat, arrObjs);
            }
        }

        public void LogFatal(string strMsg, Exception objExp = null)
        {
            if (this.m_iLog.IsFatalEnabled)
            {
                if (objExp == null)
                {
                    this.m_iLog.Fatal(strMsg);
                    return;
                }
                this.m_iLog.Fatal(strMsg, objExp);
            }
        }

        public void LogFataFormat(string strFormat, params object[] arrObjs)
        {
            if (this.m_iLog.IsFatalEnabled)
            {
                this.m_iLog.FatalFormat(strFormat, arrObjs);
            }
        }
    }
}