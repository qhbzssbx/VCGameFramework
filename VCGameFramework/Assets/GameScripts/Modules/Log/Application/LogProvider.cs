using VContainer;

namespace Game.Modules.Log.Application
{
    public class LogProvider : ILogProvider
    {
        [Inject] private LogAppService _log;

        public LogAppService Log => _log;
    }
}