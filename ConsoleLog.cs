using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Zdd.Logger
{
    public class ConsoleLog
    {
        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();
        public const string s_sendSymbol = "Send:";

        public const string s_recvSymbol = "Receive:";

        public const string s_XDCCommDateTimeFormat = "{0:D2}:{1:D2}:{2:D3}.{3}";

        public const string s_packageHeaderFormat = "{0} {1} Package Len={2},";

        public static StringBuilder s_logBuilder;

        private static object s_logLocker;

        static ConsoleLog()
        {
            AllocConsole();
            ConsoleLog.s_logBuilder = new StringBuilder(32);
            ConsoleLog.s_logLocker = new object();
        }

        public static void LogCommInfo(byte[] arrBuffer, int argSize, int argHeadSize, bool argIsSend)
        {
            lock (ConsoleLog.s_logLocker)
            {
                DateTime now = DateTime.Now;
                string text = string.Format("{0:D2}:{1:D2}:{2:D3}.{3}", new object[]
				{
					now.Hour,
					now.Minute,
					now.Second,
					now.Millisecond
				});
                ConsoleLog.s_logBuilder.AppendFormat("{0} {1} Package Len={2},", text, argIsSend ? "Send:" : "Receive:", argSize);
                if (argHeadSize > 0)
                {
                    ConsoleLog.s_logBuilder.Append("Head ");
                    for (int i = 0; i < argHeadSize; i++)
                    {
                        ConsoleLog.s_logBuilder.AppendFormat("[0x{0:x2}] ", arrBuffer[i]);
                    }
                    ConsoleLog.s_logBuilder.Append(":\r\n");
                }
                else
                {
                    ConsoleLog.s_logBuilder.Append("Head :\r\n");
                }
                int num = argSize - argHeadSize;
                if (num > 0)
                {
                    ConsoleLog.s_logBuilder.Append(' ', text.Length + 1);
                    for (int j = argHeadSize; j < argSize; j++)
                    {
                        char c = Convert.ToChar(arrBuffer[j]);
                        if (char.IsLetterOrDigit(c))
                        {
                            ConsoleLog.s_logBuilder.AppendFormat("{0}", c);
                        }
                        else
                        {
                            ConsoleLog.s_logBuilder.Append(Convert.ToChar(c));
                        }
                    }
                }
                ConsoleLog.LogMsg(ConsoleLog.s_logBuilder.ToString());
                ConsoleLog.s_logBuilder.Clear();
            }
        }

        public static void LogMsg(string argMsg)
        {
            Console.WriteLine(argMsg);
        }

        public static void LogMsgFormat(string argFormat, params object[] argParams)
        {
            ConsoleLog.LogMsg(string.Format(argFormat, argParams));
        }
    }
}
