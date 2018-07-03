/*************************************************************************
// Copyright (C) 2017  Qinchuan IoT Technology Co Ltd. 
// All Rights Reserved 
//
// FileName: D:\workspace\WebAPI\QCWebAPI\QCWebAPI\Logger\APILogManager.cs
// Function Description:
//			APILogManager
// 
// Creator:		zdd (Administrator)
// Create Time:	3:11:2017   15:35
//	
// Change History:
// Editor:
// Time:
// Comment:
//
// Version: V1.0.0
/*************************************************************************/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Web;

namespace Zdd.Logger
{
	public class APILogManager
	{
        private const string backupDay = "backupDay";
        private const string reservedSize = "reservedSize";
        private const string filePattern = "filePattern";
        private const string folder = "folder";
		private const string itemNode = "Item";
        private const string CompressStrategy = "CompressStrategy";
		public const string logCompressFile = "logCompressFiles.txt";
		public const string s_7zCompressFormat = "a -tzip -mx7 -ssw {0} @{1}";
        /// <summary>
        /// 1M字节
        /// </summary>
        private const int s_OneMBSize = 1024 * 1024;
        /// <summary>
        /// for single instance
        /// </summary>
		protected static APILogManager logManager;
        /// <summary>
        /// 日志保留天数据 90
        /// </summary>
		protected int m_backupDay = 90;
        /// <summary>
        /// 磁盘最小剩余空间保留大小,默认350M
        /// </summary>
        protected long m_reservedSize = 350 * 1024 * 1024;//350M
        protected string  m_logPath = string.Empty;
        /// <summary>
        /// 日志配置文件路径
        /// </summary>
        protected string m_cfgPath = string.Empty;
        /// <summary>
        /// 日志存放目录
        /// </summary>
        public  string LogPath
        {
            get { return m_logPath; }
        }
        /// <summary>
        /// 日志配置文件路径
        /// </summary>
        public string CfgPath
        {
            get { return m_cfgPath; }
        }
        /// <summary>
        /// 盘符字母
        /// </summary>
		protected string m_driverName;
        /// <summary>
        /// 需要清空的文件列表
        /// </summary>
		protected List<string> m_listNeedDelFiles = new List<string>();
        /// <summary>
        /// 需要压缩的策略
        /// </summary>
		private List<CompressStrategyParam> m_listCompressStrategy = new List<CompressStrategyParam>();

		protected Dictionary<DateTime, List<string>> m_dicNeedDelFiles = new Dictionary<DateTime, List<string>>();

		private Thread m_monitorThread;

		private ManualResetEvent[] m_arrEvts = new ManualResetEvent[]
		{
			new ManualResetEvent(false),
			new ManualResetEvent(false)
		};

		private Stopwatch m_timer = new Stopwatch();

		public static APILogManager Instance
		{
			get
			{
				return APILogManager.logManager;
			}
		}

		protected APILogManager()
		{
		}

		static APILogManager()
		{
			APILogManager.logManager = new APILogManager();
		}
        /// <summary>
        /// 初始化日志系统
        /// </summary>
        /// <param name="argCfgPath">配置文件路径</param>
        /// <param name="argLogPath">日志文件保存目录</param>
        /// <returns></returns>
		public bool Initialize(string argCfgPath, string argLogPath)
		{
            this.m_cfgPath = argCfgPath;
            this.m_logPath = argLogPath;
			this.LoadConfig(argCfgPath);
			this.m_driverName = Path.GetPathRoot(argLogPath);
			if (string.IsNullOrEmpty(this.m_driverName))
			{
				return false;
			}
			this.m_driverName = this.m_driverName.Substring(0, 1);
			this.DeleteLogFileByDateRule();
			this.DeleteLogFileBySize();
			this.CompressLogFiles();
			this.m_timer.Restart();
			this.m_monitorThread = new Thread(new ThreadStart(this.OnMonitorThread));
			this.m_monitorThread.Name = "APILogManagerThread";
            this.m_monitorThread.IsBackground = true;
			this.m_monitorThread.Start();
			return true;
		}

		public void Exit()
		{
			if (this.m_monitorThread != null)
			{
				this.m_arrEvts[0].Set();
				if (this.m_monitorThread.Join(3000))
				{
					this.m_monitorThread.Abort();
					this.m_monitorThread.Join(3000);
					this.m_monitorThread = null;
				}
			}
			ManualResetEvent[] arrEvts = this.m_arrEvts;
			for (int i = 0; i < arrEvts.Length; i++)
			{
				ManualResetEvent manualResetEvent = arrEvts[i];
				manualResetEvent.Dispose();
			}
			this.m_arrEvts[0] = null;
			this.m_arrEvts[1] = null;
		}
        /// <summary>
        /// 载入日志配置文件
        /// </summary>
        /// <param name="argCfgPath"></param>
		public void LoadConfig(string argCfgPath)
		{
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(argCfgPath);
                XmlAttribute xmlAttribute = xmlDocument.DocumentElement.Attributes[backupDay];
				if (xmlAttribute != null && !string.IsNullOrEmpty(xmlAttribute.Value))
				{
					int num = 0;
					if (int.TryParse(xmlAttribute.Value, out num) && num > 0)
					{
						this.m_backupDay = num;
					}
				}
                xmlAttribute = xmlDocument.DocumentElement.Attributes[reservedSize];
				if (xmlAttribute != null && !string.IsNullOrEmpty(xmlAttribute.Value))
				{
					int num2 = 0;
					if (int.TryParse(xmlAttribute.Value, out num2) && num2 > 0)
					{
                        this.m_reservedSize = (long)((num2 + 50) * 1024 * 1024);
					}
				}
                XmlNode xmlNode = xmlDocument.DocumentElement.SelectSingleNode(CompressStrategy);
				if (xmlNode != null)
				{
					XmlNodeList xmlNodeList = xmlNode.SelectNodes(itemNode);
					foreach (XmlNode xmlNode2 in xmlNodeList)
					{
						if (xmlNode2.NodeType == XmlNodeType.Element)
						{
                            xmlAttribute = xmlNode2.Attributes[folder];
							if (xmlAttribute != null /*&& !string.IsNullOrEmpty(xmlAttribute.Value)*/)
							{
                                string text = m_logPath + xmlAttribute.Value;
								if (Directory.Exists(text))
								{
                                    xmlAttribute = xmlNode2.Attributes[filePattern];
									if (xmlAttribute != null && !string.IsNullOrEmpty(xmlAttribute.Value))
									{
										List<string> list = new List<string>();
										string[] array = xmlAttribute.Value.Split(new char[]
										{
											'|'
										}, StringSplitOptions.RemoveEmptyEntries);
										string[] array2 = array;
										for (int i = 0; i < array2.Length; i++)
										{
											string item = array2[i];
											if (!list.Contains(item))
											{
												list.Add(item);
											}
										}
										if (list.Count != 0)
										{
											this.m_listCompressStrategy.Add(new CompressStrategyParam
											{
												Folder = text,
												FilePatternCollection = list
											});
										}
									}
								}
							}
						}
					}
				}
				xmlDocument.RemoveAll();
				xmlDocument = null;
			}
			catch (Exception exp)
			{
				APILog.Kernel.LogError("Failed to load config of log manager", exp);
			}
		}
        /// <summary>
        /// 删除90天以上的日志文件
        /// </summary>
		private void DeleteLogFileByDateRule()
		{
			DateTime now = DateTime.Now;
			TimeSpan value = new TimeSpan(this.m_backupDay, 0, 0, 0);
			try
			{
				this.m_listNeedDelFiles.Clear();
				foreach (string current in Directory.EnumerateFiles(this.m_logPath, "*.*", SearchOption.AllDirectories))
				{
					FileInfo fileInfo = new FileInfo(current);
					if (fileInfo.CreationTime.Add(value).CompareTo(now) < 0)
					{
						this.m_listNeedDelFiles.Add(current);
					}
				}
				if (this.m_listNeedDelFiles.Count != 0)
				{
					APILog.Kernel.LogDebug("Prepare for delete out-of-date files");
					foreach (string current2 in this.m_listNeedDelFiles)
					{
						try
						{
							FileInfo fileInfo2 = new FileInfo(current2);
							if (fileInfo2.IsReadOnly)
							{
								fileInfo2.IsReadOnly = false;
							}
							File.Delete(current2);
						}
						catch (Exception exp)
						{
							APILog.Kernel.LogWarn(string.Format("Failed to delete a file", current2), exp);
						}
					}
					this.m_listNeedDelFiles.Clear();
				}
			}
			catch (Exception exp2)
			{
                APILog.Kernel.LogDebug("Failed to delete log file", exp2);
			}
		}
        /// <summary>
        /// 根据磁盘剩余空间删除日志文件
        /// </summary>
		private void DeleteLogFileBySize()
		{
			try
			{
				string[] files = Directory.GetFiles(this.m_logPath, "*.*", SearchOption.AllDirectories);
				Array.Sort<string>(files, new FileCreateTimeComparer());
				DriveInfo driveInfo = new DriveInfo(this.m_driverName);
				if (driveInfo.TotalFreeSpace > this.m_reservedSize)
				{
					driveInfo = null;
                    return;
				}
				else
				{
					APILog.Kernel.LogDebug("Prepare for delete some files to get more space");
					DateTime now = DateTime.Now;
					DateTime t = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
					string[] array = files;
					for (int i = 0; i < array.Length; i++)
					{
						string text = array[i];
						try
						{
							FileInfo fileInfo = new FileInfo(text);
							if (fileInfo.CreationTime < t)
							{
								if (fileInfo.IsReadOnly)
								{
									fileInfo.IsReadOnly = false;
								}
								File.Delete(text);
								if (driveInfo.TotalFreeSpace > this.m_reservedSize)
								{
									break;
								}
							}
						}
						catch (Exception)
						{
						}
					}
					driveInfo = null;
				}
			}
			catch (Exception exp)
			{
                APILog.Kernel.LogDebug("Failed to delete log file", exp);
			}
		}
        /// <summary>
        /// 压缩日志文件
        /// </summary>
		private void CompressLogFiles()
		{
			DateTime now = DateTime.Now;
			try
			{
				if (this.m_listCompressStrategy.Count == 0)
				{
					APILog.Kernel.LogWarn("The compress file pattern is empty");
				}
				else
				{
                    string Excutor7z = (m_logPath+"7z.exe");
                    if (!File.Exists(Excutor7z))
					{
						APILog.Kernel.LogWarn("The 7z.exe isn't exist");
					}
					else
					{
						if (this.m_dicNeedDelFiles.Count > 0)
						{
							foreach (KeyValuePair<DateTime, List<string>> current in this.m_dicNeedDelFiles)
							{
								current.Value.Clear();
							}
							this.m_dicNeedDelFiles.Clear();
						}
						DateTime dateTime = new DateTime(now.Year, now.Month, now.Day, 23, 59, 0);
						DateTime t = dateTime.Subtract(new TimeSpan(5, 0, 0, 0, 0));
						foreach (CompressStrategyParam current2 in this.m_listCompressStrategy)
						{
							DirectoryInfo directoryInfo = new DirectoryInfo(current2.Folder);
							foreach (string current3 in current2.FilePatternCollection)
							{
								foreach (FileInfo current4 in directoryInfo.EnumerateFiles(current3, SearchOption.TopDirectoryOnly))
								{
									try
									{
										if (current4.CreationTime < t)
										{
											DateTime key = new DateTime(current4.CreationTime.Year, current4.CreationTime.Month, current4.CreationTime.Day);
											if (this.m_dicNeedDelFiles.ContainsKey(key))
											{
												List<string> list = this.m_dicNeedDelFiles[key];
												if (!list.Contains(current4.FullName))
												{
													list.Add(current4.FullName);
												}
											}
											else
											{
												List<string> list2 = new List<string>();
												list2.Add(current4.FullName);
												this.m_dicNeedDelFiles.Add(key, list2);
											}
											if (current4.IsReadOnly)
											{
												current4.IsReadOnly = false;
											}
										}
									}
									catch (Exception exp)
									{
										APILog.Kernel.LogWarn("Failed to get file information", exp);
									}
								}
							}
							directoryInfo = null;
						}
						if (this.m_dicNeedDelFiles.Count != 0)
						{
							APILog.Kernel.LogDebug("Prepare to compress log files");
                            string path = m_logPath + logCompressFile;
							List<string> list3 = null;
							foreach (KeyValuePair<DateTime, List<string>> current5 in this.m_dicNeedDelFiles)
							{
								list3 = current5.Value;
								using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
								{
									using (StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
									{
										foreach (string current6 in list3)
										{
											streamWriter.WriteLine(current6);
										}
									}
								}
								string text2 = string.Format("apilog{0:D4}{1:D2}{2:D2}.zip", current5.Key.Year, current5.Key.Month, current5.Key.Day);
                                string arguments = string.Format(s_7zCompressFormat, text2, logCompressFile);
								bool flag = false;
								try
								{
									using (Process process = new Process())
									{
                                        process.StartInfo.FileName = Excutor7z;
										process.StartInfo.Arguments = arguments;
										process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
										process.StartInfo.UseShellExecute = true;
										process.StartInfo.CreateNoWindow = true;
										process.StartInfo.ErrorDialog = false;
										process.Start();
										process.WaitForExit();
										flag = true;
									}
								}
								catch (Exception exp2)
								{
									APILog.Kernel.LogError("Failed to compress log files", exp2);
								}
								File.Delete(path);
                                string text3 = m_logPath + text2;
								if (flag && File.Exists(text3))
								{
									FileInfo fileInfo = new FileInfo(text3);
									fileInfo.CreationTime = current5.Key;
									fileInfo.LastAccessTime = current5.Key;
									fileInfo.LastWriteTime = current5.Key;
								}
								if (flag)
								{
									foreach (string current7 in list3)
									{
										try
										{
											File.Delete(current7);
										}
										catch (Exception)
										{
										}
									}
								}
							}
							foreach (KeyValuePair<DateTime, List<string>> current8 in this.m_dicNeedDelFiles)
							{
								current8.Value.Clear();
							}
							this.m_dicNeedDelFiles.Clear();
						}
					}
				}
			}
			catch (Exception exp3)
			{
				APILog.Kernel.LogError("Failed to compress log files", exp3);
			}
		}

		private void OnMonitorThread()
		{
			try
			{
				TimeSpan t = new TimeSpan(1, 0, 0, 0);
				while (true)
				{
					int num = WaitHandle.WaitAny(this.m_arrEvts, 1200000);
					if (num == 0)
					{
						break;
					}
					if (1 == num)
					{
						this.m_arrEvts[1].Reset();
					}
					else if (258 == num)
					{
						if (this.m_timer.Elapsed >= t)
						{
							this.m_timer.Restart();
							this.DeleteLogFileByDateRule();
						}
						if (!string.IsNullOrEmpty(this.m_driverName))
						{
							DriveInfo driveInfo = new DriveInfo(this.m_driverName);
							if (driveInfo.TotalFreeSpace < this.m_reservedSize)
							{
								this.DeleteLogFileBySize();
							}
						}
						DateTime now = DateTime.Now;
						if (now.Hour >= 3 && now.Hour <= 5)
						{
							this.CompressLogFiles();
						}
					}
				}
				this.m_arrEvts[0].Reset();
			}
			catch (ThreadAbortException exp)
			{
                APILog.Kernel.LogWarn("The monitor thread has been aborted", exp);
				Thread.ResetAbort();
			}
			catch (Exception exp2)
			{
                APILog.Kernel.LogError("Failed to start monitor thread", exp2);
			}
		}
	}
}
