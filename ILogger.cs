using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zdd.Logger
{
    public interface ILogger
    {
        void Log(string message, emLogLevel level);

        void Log(string message, emLogLevel level, Exception exp);

        void Log(string message, string key, emLogLevel level);

        void Log(string message, string key, emLogLevel level, Exception exp);

        void LogHex(byte[] arrBuffer, int argSize);

        void LogHexFormat(string argFormat, byte[] arrBuffer, int argSize);

        void LogFormat(string format, emLogLevel level, params object[] args);

        void LogFormat(string format, string key, emLogLevel level, params object[] args);
    }
    [Flags]
    public enum emLogLevel
    {
        Debug = 0,
        Info = 1,
        Warn = 2,
        Error = 3,
        Fatal = 4
    }
    [Flags]
    public enum LogMethodTag
    {
        Begin = 0,
        End = 1
    }
}
