using VContainer;
using VContainer.Unity;

namespace Game.UI.Inventory
{
    /// <summary>
    /// 背包窗口的作用域配置
    /// </summary>
    public class InventoryWindowLifetimeScope : LifetimeScope
    {
        /// <summary>
        /// 注册视图组件和 Presenter
        /// </summary>
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<InventoryView>();
            builder.RegisterEntryPoint<InventoryPresenter>(Lifetime.Scoped);
        }
    }
}
