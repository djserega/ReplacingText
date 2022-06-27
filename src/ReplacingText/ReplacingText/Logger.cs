using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReplacingText
{
    internal class Logger
    {
        private static readonly Serilog.Core.Logger SeriLog;

        static Logger()
        {
            SeriLog = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File("log.txt")
                .CreateLogger();

            Inf("Logger starting");
        }

        public static void Inf(string text)
        {
            SeriLog.Information(text);
        }

        public static void Inf(string text, params object[] propertyValues)
        {
            SeriLog.Information(text, propertyValues);
        }
    }
}
