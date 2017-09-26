using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace z.Diagnostics
{
    public sealed class Log
    {
        public delegate void LogHandler(string message, DateTime LogTime, LogType type);
        public static event LogHandler OnLog;

        public static void WriteLine(string Message, LogType type = LogType.Info)
        {
            DateTime dte = DateTime.Now;
            string msg = $"{ dte.ToString("HH:mm") } {type.ToString()} {Message}";
            Console.WriteLine(msg);
            OnLog?.Invoke(Message, dte, type);
        }

        public static void l(string Message) => WriteLine(Message);
        public static void e(string Message) => WriteLine(Message, LogType.Error);
        public static void v(string Message) => WriteLine(Message, LogType.Warning);

        public enum LogType : int
        {
            Info = 0,
            Warning = 1,
            Error = 2
        }
    }
}
