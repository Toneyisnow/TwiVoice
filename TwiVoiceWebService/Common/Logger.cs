using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwiVoiceWebService.Common
{
    public class Logger
    {
        private static Logger _instance = null;

        private Serilog.Core.Logger _log = null;

        public static Serilog.Core.Logger Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Logger();
                }

                return _instance._log;
            }
        }

        private Logger()
        {
            _log = new LoggerConfiguration()
                   .MinimumLevel.Debug()
                   .WriteTo.File(@"/tmp/twi/log_common.txt")
                   .CreateLogger();

        }

        public static void SetLogger(Serilog.Core.Logger log)
        {
            if (_instance == null)
            {
                _instance = new Logger();
            }

            _instance._log = log;
        }
    }
}
