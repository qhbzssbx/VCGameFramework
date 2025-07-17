using Game.Core;
using Game.Modules.Global.Domain;
using Game.Modules.Global.Infrastructure;
using VContainer;

namespace Game.Modules.Global.Application
{
    /// <summary>
    /// 全局模块，在 <see cref="ProjectContext"/> 中注册的服务都在此处声明
    /// </summary>
    public class ProjectModule : IModuleWithOrder
    {
        /// <summary>
        /// 该模块的初始化顺序，值越小越早执行
        /// </summary>
        public int Order => -1000;

        /// <summary>
        /// 注册所有全局单例服务
        /// </summary>
        public void Configure(IContainerBuilder builder)
        {
            // 网络服务
            builder.Register<INetworkService, NetworkService>(Lifetime.Singleton);
            // 账户服务
            builder.Register<IAccountService, AccountService>(Lifetime.Singleton);
            // 静态配置数据服务
            builder.Register<IMasterDataService, MasterDataService>(Lifetime.Singleton);
            // 背包数据服务
            builder.Register<IInventoryService, InventoryService>(Lifetime.Singleton);
            // 全局事件总线
            builder.Register<IGlobalEventBus, GlobalEventBus>(Lifetime.Singleton);
        }
    }
}
