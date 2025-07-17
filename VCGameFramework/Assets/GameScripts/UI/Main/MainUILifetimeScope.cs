using VContainer;
using VContainer.Unity;

namespace Game.UI.Main
{
    /// <summary>
    /// 主界面 UI 的作用域配置
    /// </summary>
    public class MainUILifetimeScope : LifetimeScope
    {
        /// <summary>
        /// 注册 UI 组件和其对应的 Presenter
        /// </summary>
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
