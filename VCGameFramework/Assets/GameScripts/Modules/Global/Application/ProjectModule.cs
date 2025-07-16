using Game.Core;
using Game.Modules.Global.Domain;
using Game.Modules.Global.Infrastructure;
using VContainer;

namespace Game.Modules.Global.Application
{
    public class ProjectModule : IModuleWithOrder
    {
        public int Order => -1000;

        public void Configure(IContainerBuilder builder)
        {
            builder.Register<INetworkService, NetworkService>(Lifetime.Singleton);
            builder.Register<IAccountService, AccountService>(Lifetime.Singleton);
            builder.Register<IMasterDataService, MasterDataService>(Lifetime.Singleton);
            builder.Register<IInventoryService, InventoryService>(Lifetime.Singleton);
            builder.Register<IGlobalEventBus, GlobalEventBus>(Lifetime.Singleton);
        }
    }
}
