using System;
using System.Threading;
using Game.AOT.AssetSystem.Core;
using Game.Core;
using VContainer;
using VContainer.Unity;

namespace Game.AOT.AssetSystem.Module
{
    /// <summary>
    /// 资源系统模块：集成VContainer，提供AssetRegistry和AssetScope的依赖注入配置
    /// </summary>
    public sealed class AssetSystemModule : IModule, IModuleWithOrder
    {
        public int Order => -100; // 高优先级，确保在其他模块之前初始化

        public void Configure(IContainerBuilder builder)
        {
            // 注册单例AssetRegistry
            builder.Register<IAssetRegistry>(provider =>
            {
                var ttl = TimeSpan.FromSeconds(15); // 可从配置文件读取
                return new AssetRegistry(ttl);
            }, Lifetime.Singleton);

            // 注册Scoped级别的AssetScope
            builder.Register<IAssetScope>(provider =>
            {
                var registry = provider.Resolve<IAssetRegistry>();
                // 每个AssetScope使用自己的CancellationToken，默认为None
                var scopeToken = CancellationToken.None;
                return new AssetScope.AssetScope(registry, scopeToken);
            }, Lifetime.Scoped)
            .AsImplementedInterfaces()
            .As<IAsyncStartable>();

            UnityEngine.Debug.Log("[AssetSystemModule] Asset system configured with VContainer");
        }
    }

    /// <summary>
    /// AssetScope的VContainer扩展，提供便捷的Scope创建方法
    /// </summary>
    public static class AssetScopeExtensions
    {
        /// <summary>
        /// 创建一个专用于资源管理的子Scope
        /// </summary>
        /// <param name="parent">父Scope</param>
        /// <param name="name">Scope名称</param>
        /// <returns>新的资源管理Scope</returns>
        public static LifetimeScope CreateAssetScope(this LifetimeScope parent, string name = "AssetScope")
        {
            var childScope = parent.CreateChild(builder =>
            {
                // 创建专用于这个Scope的CancellationToken
                var cts = new CancellationTokenSource();
                builder.RegisterInstance(cts.Token);

                // 注册AssetScope
                builder.Register<IAssetScope>(provider =>
                {
                    var registry = provider.Resolve<IAssetRegistry>();
                    var token = provider.Resolve<CancellationToken>();
                    return new AssetScope.AssetScope(registry, token);
                }, Lifetime.Scoped)
                .AsImplementedInterfaces()
                .As<IAsyncStartable>();
            });

            childScope.name = name;
            return childScope;
        }

        /// <summary>
        /// 创建一个带有CancellationToken的子Scope
        /// </summary>
        /// <param name="parent">父Scope</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <param name="name">Scope名称</param>
        /// <returns>新的Scope</returns>
        public static LifetimeScope CreateChildWithToken(this LifetimeScope parent, CancellationToken cancellationToken, string name = "ChildScope")
        {
            var childScope = parent.CreateChild(builder =>
            {
                builder.RegisterInstance(cancellationToken);

                builder.Register<IAssetScope>(provider =>
                {
                    var registry = provider.Resolve<IAssetRegistry>();
                    var token = provider.Resolve<CancellationToken>();
                    return new AssetScope.AssetScope(registry, token);
                }, Lifetime.Scoped)
                .AsImplementedInterfaces()
                .As<IAsyncStartable>();
            });

            childScope.name = name;
            return childScope;
        }
    }
}