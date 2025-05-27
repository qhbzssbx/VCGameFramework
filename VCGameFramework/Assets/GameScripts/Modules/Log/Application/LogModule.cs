using Game.Core;
using VContainer;
using Game.Modules.Log.Domain;
using Game.Modules.Log.Infrastructure;

namespace Game.Modules.Log.Application
{
    public class LogModule : IModuleWithOrder
    {
        public int Order => -100; // 优先初始化

        public void Configure(IContainerBuilder builder)
        {
            string logPath = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, "game_log.txt");
            builder.Register<ILogService, FileLogService>(Lifetime.Singleton).WithParameter("path", logPath);
            builder.Register<LogAppService>(Lifetime.Singleton);
            builder.Register<ILogProvider, LogProvider>(Lifetime.Singleton);
        }
    }
}