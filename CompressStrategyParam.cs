using System;
using System.Collections.Generic;

namespace Zdd.Logger
{
    /// <summary>
    /// WebAPI Log file compress strategy parameter
    /// </summary>
	internal class CompressStrategyParam
	{
		private string m_folder;

		private List<string> m_listFilePattern;
        /// <summary>
        /// file folder
        /// </summary>
		public string Folder
		{
			get
			{
				return this.m_folder;
			}
			set
			{
				this.m_folder = value;
			}
		}
        /// <summary>
        /// file pattern
        /// </summary>
		public List<string> FilePatternCollection
		{
			get
			{
				return this.m_listFilePattern;
			}
			set
			{
				this.m_listFilePattern = value;
			}
		}
	}
}
