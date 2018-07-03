using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zdd.Logger
{
    public struct AutoLoggerGuard : IDisposable
    {
        private string m_scope;

        private APILoggerWrapper m_logger;

        public AutoLoggerGuard(APILoggerWrapper argLogger, string argScope)
        {
            this.m_logger = argLogger;
            this.m_scope = argScope;
            this.m_logger.LogDebugFormat("Enter {0}", new object[]
			{
				this.m_scope
			});
        }

        void IDisposable.Dispose()
        {
            this.m_logger.LogDebugFormat("Leave {0}", new object[]
			{
				this.m_scope
			});
        }
    }
}
