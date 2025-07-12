using Game.Modules.Log.Application;
using Game.Modules.Log.Domain;
using Game.Modules.Log.Infrastructure;
using VContainer;
using VContainer.Unity;

namespace GameScripts.Modules.Log
{
    public class LogInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            string logPath = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, "game_log.txt");
            builder.Register<ILogService, FileLogService>(Lifetime.Singleton)
                   .WithParameter("path", logPath);
            builder.Register<LogAppService>(Lifetime.Singleton);
        }
    }
}