using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml;

namespace Zdd.Logger
{
	public class SolidXmlDocument : XmlDocument
	{
		private const string s_appFolder = "\\Config\\Backup4Config\\";

		protected string m_originFile;

		private object m_locker = new object();

		private string BackupFile
		{
			get
			{
				return string.Format("{0}bak", this.m_originFile);
			}
		}

		public override void Load(string filename)
		{
			lock (this.m_locker)
			{
				try
				{
					using (FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
					{
						base.Load(fileStream);
						fileStream.Flush();
						fileStream.Close();
					}
					this.m_originFile = filename;
                    string text = string.Format("{0}{1}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), s_appFolder);
					if (!Directory.Exists(text))
					{
						Directory.CreateDirectory(text);
					}
					string fileName = Path.GetFileName(filename);
					string destFileName = string.Format("{0}{1}", text, fileName);
					File.Copy(filename, destFileName, true);
					Thread.Sleep(1000);
				}
				catch (Exception ex)
				{
					Trace.TraceError(ex.Message);
					Trace.TraceInformation("Prepare for rollback the xml file[{0}]", new object[]
					{
						filename
					});
					string text2 = string.Format("{0}bak", filename);
					if (File.Exists(text2))
					{
						this.Rollback(filename, text2);
						try
						{
							using (FileStream fileStream2 = new FileStream(filename, FileMode.Open, FileAccess.Read))
							{
								base.Load(fileStream2);
								fileStream2.Flush();
								fileStream2.Close();
							}
							return;
						}
						catch (Exception ex2)
						{
							Trace.TraceError(ex2.Message);
						}
					}
					this.Resume(filename);
				}
			}
		}

		private void Resume(string argFileName)
		{
            string arg = string.Format("{0}{1}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), s_appFolder);
			string fileName = Path.GetFileName(argFileName);
			string text = string.Format("{0}{1}", arg, fileName);
			if (!File.Exists(text))
			{
				throw new Exception(string.Format("backup file{0} isn't exist", text));
			}
			if (File.Exists(argFileName))
			{
				FileInfo fileInfo = new FileInfo(argFileName);
				fileInfo.IsReadOnly = false;
			}
			File.Copy(text, argFileName, true);
			Thread.Sleep(1000);
			using (FileStream fileStream = new FileStream(argFileName, FileMode.Open, FileAccess.Read))
			{
				base.Load(fileStream);
				fileStream.Flush();
				fileStream.Close();
			}
		}

		public override void Save(string filename)
		{
			lock (this.m_locker)
			{
				if (string.IsNullOrEmpty(filename))
				{
					throw new ArgumentNullException("filename");
				}
				this.m_originFile = filename;
				string backupFile = this.BackupFile;
				if (!this.BeginSave(filename, backupFile))
				{
					throw new Exception(string.Format("Failed to save a xml file[{0}]", filename));
				}
				if (!this.SaveXml(filename))
				{
					this.Rollback(filename, backupFile);
					throw new Exception(string.Format("Failed to sava a xml file[{0}]", filename));
				}
				this.EndSave(backupFile);
			}
		}

		private bool BeginSave(string argXml, string argBackup)
		{
			try
			{
				if (File.Exists(argBackup))
				{
					FileInfo fileInfo = new FileInfo(argBackup);
					if (fileInfo.IsReadOnly)
					{
						fileInfo.IsReadOnly = false;
					}
				}
				FileInfo fileInfo2 = new FileInfo(argXml);
				if (fileInfo2.IsReadOnly)
				{
					fileInfo2.IsReadOnly = false;
				}
				File.Copy(argXml, argBackup, true);
				Thread.Sleep(1000);
			}
			catch (Exception ex)
			{
				Trace.TraceError(ex.Message);
				return false;
			}
			return true;
		}

		private bool SaveXml(string argFile)
		{
			try
			{
				using (FileStream fileStream = new FileStream(argFile, FileMode.Create, FileAccess.Write))
				{
					base.Save(fileStream);
					fileStream.Flush();
					fileStream.Close();
				}
				Thread.Sleep(1000);
			}
			catch (Exception ex)
			{
				Trace.TraceError(ex.Message);
				return false;
			}
			return true;
		}

		private void EndSave(string argBackupFile)
		{
			try
			{
				FileInfo fileInfo = new FileInfo(argBackupFile);
				if (fileInfo.IsReadOnly)
				{
					fileInfo.IsReadOnly = false;
				}
				fileInfo.Delete();
				Thread.Sleep(1000);
			}
			catch (Exception ex)
			{
				Trace.TraceError(ex.Message);
			}
		}

		private void Rollback(string argXmlFile, string argBackupFile)
		{
			try
			{
				if (!File.Exists(argBackupFile))
				{
					Trace.TraceError("{0} isn't exist", new object[]
					{
						argBackupFile
					});
				}
				else
				{
					File.Copy(argBackupFile, argXmlFile, true);
					Thread.Sleep(1000);
					File.Delete(argBackupFile);
					Thread.Sleep(1000);
				}
			}
			catch (Exception ex)
			{
				Trace.TraceError(ex.Message);
			}
		}
	}
}
