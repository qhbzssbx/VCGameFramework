using Game.Core;
using GameScripts.Modules.Scene.Runtime;
using MessagePipe;
using VContainer;
using VContainer.Unity;

public class GlobalLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {


        var options = builder.RegisterMessagePipe();
        builder.RegisterBuildCallback(c => GlobalMessagePipe.SetProvider(c.AsServiceProvider()));
        builder.RegisterMessageBroker<int>(options);

        ModuleLoader.RegisterAllModules(builder);

        builder.Register<Test>(Lifetime.Singleton);
        builder.Register<CustomSceneManager>(Lifetime.Singleton);

        builder.RegisterEntryPoint<LaunchScript>();
    }
}
