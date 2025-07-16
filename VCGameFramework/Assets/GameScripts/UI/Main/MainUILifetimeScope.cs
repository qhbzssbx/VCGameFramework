using VContainer;
using VContainer.Unity;

namespace Game.UI.Main
{
    public class MainUILifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<HUDView>();
            builder.RegisterComponentInHierarchy<MinimapView>();
            builder.RegisterComponentInHierarchy<QuestTrackerView>();

            builder.RegisterEntryPoint<HUDPresenter>(Lifetime.Scoped);
            builder.RegisterEntryPoint<MinimapPresenter>(Lifetime.Scoped);
            builder.RegisterEntryPoint<QuestTrackerPresenter>(Lifetime.Scoped);
        }
    }
}
