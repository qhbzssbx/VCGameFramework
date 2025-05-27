using Game.Core;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        //builder.Register<ILogService, LogService>(Lifetime.Singleton);
        ModuleLoader.RegisterAllModules(builder);
    }
}
