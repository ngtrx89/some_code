using Common.Consts;
using System;

namespace Common.Logger
{
    public class LogMessage
    {
        public DateTime Time { get; } = DateTime.Now;
        public LoggerMessageTypes Type { get; set; }
        public string Message { get; set; }
    }
}
