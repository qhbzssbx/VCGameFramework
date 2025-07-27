using Game.Core;
using Game.Infrastructure.Resource.Core;
using Game.Infrastructure.Resource.Providers.YooAssetProvider;
using VContainer;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Infrastructure.Resource.Module
{
    /// <summary>
    /// 资源模块 - 基础设施层级别的资源管理系统
    /// 
    /// 新设计说明：
    /// - IResourceService：专注于YooAsset初始化
    /// - ResourceLoader：组合式资源加载器，实际的资源加载通过它完成
    /// - AutoResourceRelease：GameObject级别的自动资源释放
    /// 
    /// 使用方式：
    /// 1. 在需要资源的类中创建 ResourceLoader 实例
    /// 2. 使用 LoadAssetAsync 或 LoadPrefabForInstantiate 加载资源
    /// 3. 可选择绑定到 GameObject 实现自动释放
    /// 4. 组件销毁时调用 ResourceLoader.Dispose()
    /// </summary>
    public class ResourceModule : IModuleWithOrder, IAsyncModule
    {
        /// <summary>
        /// 基础设施层高优先级，确保在业务模块前初始化
        /// </summary>
        public int Order => -500;
        
        public void Configure(IContainerBuilder builder)
        {
            // 注册简化的资源服务 - 仅负责YooAsset初始化
            builder.Register<IResourceService, YooAssetResourceService>(Lifetime.Singleton);
            
            Debug.Log("ResourceModule配置完成 - 简化版本，实际加载通过ResourceLoader完成");
        }

        public async UniTask InitializeAsync(IObjectResolver resolver)
        {
            Debug.Log("ResourceModule初始化开始 - 仅初始化YooAsset");
            
            var resourceService = resolver.Resolve<IResourceService>();
            await resourceService.InitializeAsync();
            
            Debug.Log("ResourceModule初始化完成 - YooAsset已就绪，可使用ResourceLoader进行资源加载");
        }
    }
}