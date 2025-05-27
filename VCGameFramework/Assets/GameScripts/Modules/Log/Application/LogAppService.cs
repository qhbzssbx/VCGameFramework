using Game.Modules.Log.Domain;

namespace Game.Modules.Log.Application
{
    public class LogAppService
    {
        private readonly ILogService _logService;

        public LogAppService(ILogService logService)
        {
            _logService = logService;
        }

        public void Log(string msg) => _logService.Log(LogLevel.Info, msg);
        public void Warn(string msg) => _logService.Log(LogLevel.Warning, msg);
        public void Error(string msg) => _logService.Log(LogLevel.Error, msg);
    }
}