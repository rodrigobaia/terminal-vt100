using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalVT100.StateFlow
{
    public class StateManage
    {
        private TedVT100Server TedVT100Server;
        private ConcurrentDictionary<string, ClientState> _clientStates;

        public static ILogger Logger;

        public StateManage()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var pathLog = Path.Combine(path, "logs");

            if (!Directory.Exists(pathLog))
            {
                Directory.CreateDirectory(pathLog);
            }

            var fileFull = Path.Combine(pathLog, "state-manage.log");

            Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(fileFull, rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
#if DEBUG
                .WriteTo.Console(Serilog.Events.LogEventLevel.Verbose)
#else
                .WriteTo.Console(Serilog.Events.LogEventLevel.Error)
#endif
                .CreateLogger();
        }



    }
}
