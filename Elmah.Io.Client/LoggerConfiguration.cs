using System;

namespace Elmah.Io.Client
{
    public class LoggerConfiguration
    {
        private Guid _logId;
        private LoggerOptions _options;

        public LoggerConfiguration UseLog(Guid logId)
        {
            _logId = logId;
            return this;
        }

        public LoggerConfiguration WithOptions(LoggerOptions options)
        {
            _options = options;
            return this;
        }

        public ILogger CreateLogger()
        {
            if (_options == null) _options = new LoggerOptions();
            return new Logger(_logId, _options);
        }
    }
}