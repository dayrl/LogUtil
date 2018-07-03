using System;
using System.Collections.Generic;
using System.IO;

namespace Zdd.Logger
{
	internal class FileCreateTimeComparer : IComparer<string>
	{
		public int Compare(string argX, string argY)
		{
			FileInfo fileInfo = new FileInfo(argX);
			FileInfo fileInfo2 = new FileInfo(argY);
			return fileInfo.CreationTime.CompareTo(fileInfo2.CreationTime);
		}
	}
}
