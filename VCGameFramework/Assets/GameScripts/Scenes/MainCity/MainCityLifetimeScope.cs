using VContainer;
using VContainer.Unity;

namespace Game.Scenes.MainCity
{
    public class MainCityLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<ChatManager>(Lifetime.Scoped);
            builder.RegisterEntryPoint<AuctionHouseController>(Lifetime.Scoped);
        }
    }
}
