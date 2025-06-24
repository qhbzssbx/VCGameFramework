using Game.Tests.MessagePipe;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        

        builder.RegisterEntryPoint<MessagePipeDemo>(Lifetime.Singleton);

        builder.RegisterEntryPoint<Test2>();
    }
}
