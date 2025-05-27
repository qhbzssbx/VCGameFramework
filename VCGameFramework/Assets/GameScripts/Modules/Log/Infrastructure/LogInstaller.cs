using VContainer;
using VContainer.Unity;
using Game.Modules.Log.Domain;
using Game.Modules.Log.Application;

namespace Game.Modules.Log.Infrastructure
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