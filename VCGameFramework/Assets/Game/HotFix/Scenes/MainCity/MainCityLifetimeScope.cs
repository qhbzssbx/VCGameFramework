using VContainer;
using VContainer.Unity;

namespace Game.Scenes.MainCity
{
    /// <summary>
    /// 主城场景的作用域配置
    /// </summary>
    public class MainCityLifetimeScope : LifetimeScope
    {
        /// <summary>
        /// 在场景加载时注册相关入口点
        /// </summary>
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<ChatManager>(Lifetime.Scoped);
            builder.RegisterEntryPoint<AuctionHouseController>(Lifetime.Scoped);
        }
    }
}
