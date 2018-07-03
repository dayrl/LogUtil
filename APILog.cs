using System;

namespace Zdd.Logger
{
	public static class APILog
	{
		private static APILoggerWrapper kernelLogger;
        private static APILoggerWrapper dbcontextLogger;

        /// <summary>
        /// ������־
        /// </summary>
        public static APILoggerWrapper Kernel
        {
            get
            {
                if (APILog.kernelLogger == null)
                {
                    APILog.kernelLogger = APILoggerWrapper.Create("Kernel");
                }
                return APILog.kernelLogger;
            }
        }


        /// <summary>
        /// ������־
        /// </summary>
        public static APILoggerWrapper DbContext
        {
            get
            {
                if (APILog.dbcontextLogger == null)
                {
                    APILog.dbcontextLogger = APILoggerWrapper.Create("DbContext");
                }
                return APILog.dbcontextLogger;
            }
        }
	}
}
