using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zdd.Logger
{
    public static class LoggerKeyHelper
    {
        private static string _key;

        public static IEnumerable<string> EncryptLoggerAppenders
        {
            get;
            set;
        }

        public static string Key
        {
            get
            {
                if (string.IsNullOrEmpty(LoggerKeyHelper._key))
                {
                    LoggerKeyHelper._key = "ST002312";
                }
                return LoggerKeyHelper._key;
            }
            set
            {
                if (string.IsNullOrEmpty(value) || value.Length < 8)
                {
                    throw new Exception("The Key min length is 8.");
                }
                LoggerKeyHelper._key = value;
            }
        }
    }
}
