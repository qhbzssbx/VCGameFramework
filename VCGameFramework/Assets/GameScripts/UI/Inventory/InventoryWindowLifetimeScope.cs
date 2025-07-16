using VContainer;
using VContainer.Unity;

namespace Game.UI.Inventory
{
    public class InventoryWindowLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<InventoryView>();
            builder.RegisterEntryPoint<InventoryPresenter>(Lifetime.Scoped);
        }
    }
}
